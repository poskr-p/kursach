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
        private int currentUserAccessLevel;
        private List<OrderItem> currentOrderItems;
        private int currentEmployeeId;

        public class OrderItem
        {
            public int MenuItemId { get; set; }
            public string ItemName { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal Subtotal => Quantity * UnitPrice;
        }

        public OrderTakingWindow(int accessLevel, int employeeId = 0)
        {
            InitializeComponent();
            db = new cafe_barEntities();
            currentUserAccessLevel = accessLevel;
            currentEmployeeId = employeeId;
            currentOrderItems = new List<OrderItem>();

            InitializeData();
            LoadMenuItems();
        }

        private void InitializeData()
        {
            // Загрузка категорий
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

            // Загрузка клиентов
            var clients = db.Clients.ToList();
            cbClients.ItemsSource = clients;
            cbClients.DisplayMemberPath = "name_client";
            cbClients.SelectedValuePath = "id_client";

            // Информация о текущем пользователе
            if (currentEmployeeId > 0)
            {
                var employee = db.Employees.Find(currentEmployeeId);
                if (employee != null)
                {
                    tbCurrentUser.Text = $"Официант: {employee.name_employee}";
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
                MessageBox.Show("Выберите клиента!");
                return;
            }

            if (currentOrderItems.Count == 0)
            {
                MessageBox.Show("Добавьте позиции в заказ!");
                return;
            }

            if (currentEmployeeId == 0)
            {
                MessageBox.Show("Ошибка: сотрудник не определен!");
                return;
            }

            try
            {
                // Создание заказа
                var order = new Orders
                {
                    id_cli_fk = (int)cbClients.SelectedValue,
                    id_emp_fk = currentEmployeeId,
                    order_date = DateTime.Now,
                    totalAmount = currentOrderItems.Sum(item => item.Subtotal)
                };

                db.Orders.Add(order);
                db.SaveChanges();

                // Создание деталей заказа
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

                MessageBox.Show($"Заказ №{order.id_order} успешно создан на сумму {order.totalAmount:C}");

                // Очистка формы
                ClearOrder_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания заказа: {ex.Message}");
            }
        }

        private void NewOrder_Click(object sender, RoutedEventArgs e)
        {
            ClearOrder_Click(sender, e);
        }

        private void ActiveOrders_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Просмотр активных заказов - функция в разработке");
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
            new MainDashboard().Show();
            this.Close();
        }
    }
}