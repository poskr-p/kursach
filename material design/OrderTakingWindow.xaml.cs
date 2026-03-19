using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace material_design
{
    public partial class OrderTakingWindow : Window
    {
        private cafe_barEntities db;
        private List<OrderItem> currentOrderItems;

        public class OrderItem
        {
            public int MenuItemId { get; set; }
            public string ItemName { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal Subtotal => Quantity * UnitPrice;
        }

        public OrderTakingWindow(int accessLevel)
        {
            InitializeComponent();

            if (!App.UserContext.IsAuthenticated)
            {
                MessageBox.Show("Требуется авторизация");
                var authWindow = new autorization();
                authWindow.Show();
                this.Close();
                return;
            }

            db = new cafe_barEntities();
            currentOrderItems = new List<OrderItem>();

            InitializeData();
            LoadMenuItems();
        }

        private void InitializeData()
        {
            var categories = db.CategoriesMenu.ToList();
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

            var clients = db.Clients.ToList();
            cbClients.ItemsSource = clients;
            cbClients.DisplayMemberPath = "name_client";
            cbClients.SelectedValuePath = "id_client";

            var employees = db.Employees
                .Where(e => e.Post.title_post == "Официант" || e.Post.title_post == "Бармен")
                .ToList();
            cbEmployees.ItemsSource = employees;
            cbEmployees.DisplayMemberPath = "name_employee";
            cbEmployees.SelectedValuePath = "id_employee";

            if (App.UserContext.IsAuthenticated)
            {
                tbCurrentUser.Text = $"{App.UserContext.UserName} ({App.UserContext.UserRole})";

                var currentUser = employees.FirstOrDefault(e =>
                    e.name_employee.Contains(App.UserContext.UserName) ||
                    e.email.Contains(App.UserContext.UserName));

                if (currentUser != null)
                {
                    cbEmployees.SelectedValue = currentUser.id_employee;
                }
                else if (employees.Count > 0)
                {
                    cbEmployees.SelectedIndex = 0;
                }
            }
        }

        private void LoadMenuItems()
        {
            var query = db.Menu.Include(m => m.CategoriesMenu).AsQueryable();

            if (cbCategories.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is int categoryId && categoryId > 0)
            {
                query = query.Where(m => m.id_category_fk == categoryId);
            }

            if (!string.IsNullOrEmpty(tbSearch.Text) && tbSearch.Text != "Поиск...")
            {
                query = query.Where(m => m.item_name.Contains(tbSearch.Text));
            }

            dgMenu.ItemsSource = query.OrderBy(m => m.item_name).ToList();
        }

        private void AddToOrder_Click(object sender, RoutedEventArgs e)
        {
            if (dgMenu.SelectedItem is Menu selectedItem)
            {
                var existingItem = currentOrderItems.FirstOrDefault(i => i.MenuItemId == selectedItem.id_menu_item);

                if (existingItem != null)
                {
                    existingItem.Quantity++;
                }
                else
                {
                    currentOrderItems.Add(new OrderItem
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
            if (sender is Button button && button.DataContext is OrderItem item)
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
            decimal total = currentOrderItems.Sum(item => item.Subtotal);
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
                MessageBox.Show("Выберите клиента!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                cbClients.Focus();
                return;
            }

            if (cbEmployees.SelectedValue == null)
            {
                MessageBox.Show("Выберите сотрудника!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                cbEmployees.Focus();
                return;
            }

            if (currentOrderItems.Count == 0)
            {
                MessageBox.Show("Добавьте позиции в заказ!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                int clientId = (int)cbClients.SelectedValue;
                int employeeId = (int)cbEmployees.SelectedValue;

                var clientExists = db.Clients.Any(c => c.id_client == clientId);
                var employeeExists = db.Employees.Any(emp => emp.id_employee == employeeId); 

                if (!clientExists)
                {
                    MessageBox.Show("Выбранный клиент не найден в базе данных!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!employeeExists)
                {
                    MessageBox.Show("Выбранный сотрудник не найден в базе данных!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var order = new Orders
                {
                    id_cli_fk = clientId,
                    id_emp_fk = employeeId,
                    order_date = DateTime.Now,
                    totalAmount = currentOrderItems.Sum(item => item.Subtotal)
                };

                db.Orders.Add(order);
                db.SaveChanges();

                foreach (var item in currentOrderItems)
                {
                    var orderDetail = new Order_details
                    {
                        id_order_fk = order.id_order,
                        id_menu_item_fk = item.MenuItemId,
                        quantity = (short)item.Quantity,
                        unit_price = item.UnitPrice
                    };
                    db.Order_details.Add(orderDetail);
                }

                db.SaveChanges();

                CheckAndAddRegularClient(clientId);

                MessageBox.Show($"Заказ №{order.id_order} успешно создан!\n" +
                              $"Клиент: {cbClients.Text}\n" +
                              $"Сотрудник: {cbEmployees.Text}\n" +
                              $"Сумма: {order.totalAmount:C}",
                              "Успех",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);

                ClearOrder_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания заказа: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CheckAndAddRegularClient(int clientId)
        {
            try
            {
                var isRegular = db.Regular_Clients.Any(rc => rc.id_reg_client_fk == clientId);

                if (!isRegular)
                {
                    decimal totalSpent = db.Orders
                        .Where(o => o.id_cli_fk == clientId)
                        .Sum(o => o.totalAmount);

                    int orderCount = db.Orders
                        .Where(o => o.id_cli_fk == clientId)
                        .Count();

                    if (orderCount >= 5 || totalSpent >= 10000)
                    {
                        decimal discountRate = 0;

                        if (totalSpent >= 30000)
                            discountRate = 15.00m;
                        else if (totalSpent >= 20000)
                            discountRate = 10.00m;
                        else if (totalSpent >= 10000)
                            discountRate = 5.00m;

                        var regularClient = new Regular_Clients
                        {
                            id_reg_client_fk = clientId,
                            discount_rate = discountRate,
                            total_spent = totalSpent
                        };

                        db.Regular_Clients.Add(regularClient);
                        db.SaveChanges();

                        MessageBox.Show($"Клиент добавлен в программу лояльности!\n" +
                                      $"Скидка: {discountRate}%",
                                      "Информация",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обработки лояльности: {ex.Message}");
            }
        }

        private void NewOrder_Click(object sender, RoutedEventArgs e)
        {
            ClearOrder_Click(sender, e);
        }

        private void ActiveOrders_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Просмотр активных заказов - функция в разработке",
                "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

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

        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadMenuItems();
        }

        private void cbCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadMenuItems();
        }

        private void MainMenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (App.UserContext.IsAuthenticated)
            {
                var mainDashboard = new MainDashboard(
                    App.UserContext.UserName,
                    App.UserContext.UserRole,
                    App.UserContext.AccessLevel);
                mainDashboard.Show();
            }
            else
            {
                var authWindow = new autorization();
                authWindow.Show();
            }

            this.Close();
        }
    }
}