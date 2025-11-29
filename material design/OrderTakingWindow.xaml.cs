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
        private cafe_barEntities1 db;
        private int currentUserAccessLevel;
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
            db = new cafe_barEntities1();
            currentUserAccessLevel = accessLevel;
            currentOrderItems = new List<OrderItem>();

            InitializeData();
        }

        private void InitializeData()
        {
            // Загрузка категорий
            var categories = db.CategoriesMenu.ToList();
            cbCategories.ItemsSource = categories;
            cbCategories.DisplayMemberPath = "title_category";
            cbCategories.SelectedValuePath = "id_category";

            // Загрузка клиентов
            var clients = db.Clients.ToList();
            cbClients.ItemsSource = clients;
            cbClients.DisplayMemberPath = "name_client";
            cbClients.SelectedValuePath = "id_client";

            // Загрузка меню
            LoadMenuItems();

            // Обновление итогов
            UpdateOrderTotal();
        }

        private void LoadMenuItems()
        {
            var menuItems = db.Menu
                .Include("CategoriesMenu")
                .ToList();
            dgMenu.ItemsSource = menuItems;
        }

        private void cbCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterMenuItems();
        }

        private void FilterMenuItems()
        {
            var allItems = db.Menu.Include("CategoriesMenu").AsQueryable();

            if (cbCategories.SelectedValue != null && (int)cbCategories.SelectedValue > 0)
            {
                allItems = allItems.Where(m => m.id_category_fk == (int)cbCategories.SelectedValue);
            }

            if (!string.IsNullOrWhiteSpace(tbSearch.Text) && tbSearch.Text != "Поиск...")
            {
                allItems = allItems.Where(m => m.item_name.Contains(tbSearch.Text));
            }

            dgMenu.ItemsSource = allItems.ToList();
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

            try
            {
                // Создание заказа
                var order = new Orders
                {
                    id_cli_fk = (int)cbClients.SelectedValue,
                    id_emp_fk = GetCurrentEmployeeId(), // Нужно реализовать получение ID текущего сотрудника
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

        private int GetCurrentEmployeeId()
        {
            // В реальном приложении здесь должна быть логика получения ID текущего сотрудника
            // Пока возвращаем первого сотрудника с доступом официанта/бармена
            return db.Employees
                .Where(e => e.Post.accessLevel == currentUserAccessLevel)
                .Select(e => e.id_employee)
                .FirstOrDefault();
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
            FilterMenuItems();
        }

        private void MainMenuButton_Click(object sender, RoutedEventArgs e)
        {
            new MainDashboard().Show();
            this.Close();
        }

        private void dgMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ///
        }
    }
}