using material_design.DTO;
using material_design.Repositories;
using material_design.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace material_design
{
    public partial class OrderTakingWindow : Window
    {
        private readonly cafe_barEntities _context;
        private readonly IOrderService _orderService;
        private List<OrderItemDto> currentOrderItems;
        private int currentEmployeeId;

        public OrderTakingWindow(int accessLevel, int employeeId = 0)
        {
            InitializeComponent();
            _context = new cafe_barEntities();

            var menuRepo = new Repository<Menu>(_context);
            var categoryRepo = new Repository<CategoriesMenu>(_context);
            var clientRepo = new Repository<Clients>(_context);
            var orderRepo = new Repository<Orders>(_context);
            var orderDetailsRepo = new Repository<Order_details>(_context);
            var employeeRepo = new Repository<Employees>(_context);
            var postRepo = new Repository<Post>(_context);

            _orderService = new OrderService(
                menuRepo,
                categoryRepo,
                clientRepo,
                orderRepo,
                orderDetailsRepo,
                employeeRepo,
                postRepo);

            currentEmployeeId = employeeId;
            currentOrderItems = new List<OrderItemDto>();

            // Сначала загружаем данные для ComboBox, потом загружаем меню
            InitializeData();

            // Небольшая задержка для гарантии, что UI обновился
            Dispatcher.BeginInvoke(new Action(() => LoadMenuItems()), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void InitializeData()
        {
            // Категории
            var categories = _orderService.GetCategories();
            cbCategories.Items.Clear();
            cbCategories.Items.Add(new ComboBoxItem { Content = "Все категории", Tag = 0 });
            foreach (var category in categories)
            {
                cbCategories.Items.Add(new ComboBoxItem
                {
                    Content = category.title_category,
                    Tag = category.id_category
                });
            }
            cbCategories.SelectedIndex = 0;

            // Клиенты
            var clients = _orderService.GetClients();
            cbClients.ItemsSource = clients;
            cbClients.DisplayMemberPath = "name_client";
            cbClients.SelectedValuePath = "id_client";

            // Сотрудники
            var employees = _orderService.GetEmployees();
            cbEmployees.ItemsSource = employees;
            cbEmployees.DisplayMemberPath = "name_employee";
            cbEmployees.SelectedValuePath = "id_employee";

            // Текущий пользователь (если передан employeeId)
            if (currentEmployeeId > 0)
            {
                var employee = employees.FirstOrDefault(e => e.id_employee == currentEmployeeId);
                if (employee != null)
                {
                    tbCurrentUser.Text = $"Официант: {employee.name_employee}";
                    cbEmployees.SelectedValue = currentEmployeeId; // автоматически выбираем
                }
            }
        }

        private void LoadMenuItems()
        {
            try
            {
                // Защита от вызова до инициализации
                if (_orderService == null)
                {
                    MessageBox.Show("Сервис не инициализирован");
                    return;
                }

                int? categoryId = null;
                if (cbCategories.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is int catId && catId > 0)
                    categoryId = catId;

                string search = tbSearch.Text == "Поиск..." ? null : tbSearch.Text;

                var items = _orderService.GetMenuItems(categoryId, search);
                dgMenu.ItemsSource = items;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка в LoadMenuItems: {ex.Message}\n\n{ex.StackTrace}");
            }
        }

        private void AddToOrder_Click(object sender, RoutedEventArgs e)
        {
            if (dgMenu.SelectedItem is Menu selectedItem)
            {
                var existing = currentOrderItems.FirstOrDefault(i => i.MenuItemId == selectedItem.id_menu_item);
                if (existing != null)
                    existing.Quantity++;
                else
                {
                    currentOrderItems.Add(new OrderItemDto
                    {
                        MenuItemId = selectedItem.id_menu_item,
                        ItemName = selectedItem.item_name,
                        Quantity = 1,
                        UnitPrice = selectedItem.cost_item
                    });
                }
                RefreshOrderItems();
                UpdateOrderTotal();
            }
        }

        private void RemoveFromOrder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is OrderItemDto item)
            {
                currentOrderItems.Remove(item);
                RefreshOrderItems();
                UpdateOrderTotal();
            }
        }

        private void RefreshOrderItems()
        {
            dgOrderItems.ItemsSource = null;
            dgOrderItems.ItemsSource = currentOrderItems;
        }

        private void UpdateOrderTotal()
        {
            decimal total = currentOrderItems.Sum(i => i.Subtotal);
            tbOrderTotal.Text = $"Итого: {total:C}";
        }

        private void ClearOrder_Click(object sender, RoutedEventArgs e)
        {
            currentOrderItems.Clear();
            RefreshOrderItems();
            UpdateOrderTotal();
            cbClients.SelectedIndex = -1;
        }

        private void CreateOrder_Click(object sender, RoutedEventArgs e)
        {
            if (cbClients.SelectedValue == null)
            {
                MessageBox.Show("Выберите клиента!");
                return;
            }
            if (cbEmployees.SelectedValue == null)
            {
                MessageBox.Show("Выберите сотрудника!");
                return;
            }
            if (currentOrderItems.Count == 0)
            {
                MessageBox.Show("Добавьте позиции в заказ!");
                return;
            }

            try
            {
                var order = new Orders
                {
                    id_cli_fk = (int)cbClients.SelectedValue,
                    id_emp_fk = (int)cbEmployees.SelectedValue,
                    order_date = DateTime.Now,
                    totalAmount = currentOrderItems.Sum(i => i.Subtotal)
                };

                _orderService.CreateOrder(order, currentOrderItems);

                MessageBox.Show($"Заказ №{order.id_order} успешно создан на сумму {order.totalAmount:C}");

                ClearOrder_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания заказа: {ex.Message}");
            }
        }
        private void NewOrder_Click(object sender, RoutedEventArgs e) => ClearOrder_Click(sender, e);
        private void ActiveOrders_Click(object sender, RoutedEventArgs e) => MessageBox.Show("Просмотр активных заказов - функция в разработке");

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && textBox.Text == "Поиск...")
            {
                textBox.Text = "";
                textBox.Foreground = Brushes.Black;
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = "Поиск...";
                textBox.Foreground = Brushes.Gray;
            }
        }

        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e) => LoadMenuItems();
        private void cbCategories_SelectionChanged(object sender, SelectionChangedEventArgs e) => LoadMenuItems();

        private void MainMenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (Appp.CurrentUser != null)
                new MainDashboard(Appp.CurrentUser.Login, AccessControl.GetRoleName(Appp.CurrentUser.accessLevel), Appp.CurrentUser.accessLevel).Show();
            else
                new MainDashboard().Show();
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _context?.Dispose();
        }
    }
}


