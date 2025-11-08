using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace material_design
{
    public partial class Window2 : Window
    {
        private cafe_barEntities1 db;
        private string currentTableName;

        public Window2()
        {
            InitializeComponent();
            db = new cafe_barEntities1();
            LoadTableNames();
        }

        private void LoadTableNames()
        {
            var tableNames = new[]
            {
        "Post", "Employees", "Clients", "Regular_Clients", "Reservation",
        "CategoriesMenu", "Menu", "Orders", "Order_details", "Autorization"
    };

            // Создаем список с русскими названиями для отображения
            var displayNames = tableNames.Select(t => new
            {
                EnglishName = t,
                RussianName = RussianTranslator.GetTableName(t)
            }).ToList();

            cbTables.ItemsSource = displayNames;
            cbTables.DisplayMemberPath = "RussianName";
            cbTables.SelectedValuePath = "EnglishName";
        }

        private void cbTables_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbTables.SelectedItem == null) return;

            dynamic selectedItem = cbTables.SelectedItem;
            currentTableName = selectedItem.EnglishName;
            LoadTableData();
        }

        //private void cbTables_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        //{
        //    if (cbTables.SelectedItem == null) return;

        //    currentTableName = cbTables.SelectedItem.ToString();
        //    LoadTableData();
        //}

        private void LoadTableData()
        {
            try
            {
                switch (currentTableName)
                {
                    case "Post":
                        dataGrid.ItemsSource = db.Post.ToList();
                        ConfigureDataGridColumns(db.Post.FirstOrDefault());
                        break;

                    case "Employees":
                        var employees = from emp in db.Employees
                                        join post in db.Post on emp.post_emp_fk equals post.id_post
                                        select new
                                        {
                                            emp.id_employee,
                                            emp.name_employee,
                                            emp.ph_number_emp,
                                            emp.email,
                                            post.title_post
                                        };
                        dataGrid.ItemsSource = employees.ToList();
                        ConfigureDataGridColumns(employees.FirstOrDefault());
                        break;

                    case "Clients":
                        dataGrid.ItemsSource = db.Clients.ToList();
                        ConfigureDataGridColumns(db.Clients.FirstOrDefault());
                        break;

                    case "Regular_Clients":
                        var regularClients = from rc in db.Regular_Clients
                                             join c in db.Clients on rc.id_reg_client_fk equals c.id_client
                                             select new
                                             {
                                                 rc.id_reg_client_fk,
                                                 ClientName = c.name_client,
                                                 rc.discount_rate,
                                                 rc.total_spent
                                             };
                        dataGrid.ItemsSource = regularClients.ToList();
                        ConfigureDataGridColumns(regularClients.FirstOrDefault());
                        break;

                    case "Reservation":
                        var reservations = from r in db.Reservation
                                           join c in db.Clients on r.id_client_fk equals c.id_client
                                           join e in db.Employees on r.id_employee_fk equals e.id_employee
                                           select new
                                           {
                                               r.id_reservation,
                                               Client = c.name_client,
                                               Employee = e.name_employee,
                                               r.reservation_date,
                                               r.guests_count
                                           };
                        dataGrid.ItemsSource = reservations.ToList();
                        ConfigureDataGridColumns(reservations.FirstOrDefault());
                        break;

                    case "CategoriesMenu":
                        dataGrid.ItemsSource = db.CategoriesMenu.ToList();
                        ConfigureDataGridColumns(db.CategoriesMenu.FirstOrDefault());
                        break;

                    case "Menu":
                        var menu = from m in db.Menu
                                   join c in db.CategoriesMenu on m.id_category_fk equals c.id_category
                                   select new
                                   {
                                       m.id_menu_item,
                                       m.item_name,
                                       Category = c.title_category,
                                       m.cost_item
                                   };
                        dataGrid.ItemsSource = menu.ToList();
                        ConfigureDataGridColumns(menu.FirstOrDefault());
                        break;

                    case "Orders":
                        var orders = from o in db.Orders
                                     join c in db.Clients on o.id_cli_fk equals c.id_client
                                     join e in db.Employees on o.id_emp_fk equals e.id_employee
                                     select new
                                     {
                                         o.id_order,
                                         Client = c.name_client,
                                         Employee = e.name_employee,
                                         o.order_date,
                                         o.totalAmount
                                     };
                        dataGrid.ItemsSource = orders.ToList();
                        ConfigureDataGridColumns(orders.FirstOrDefault());
                        break;

                    case "Order_details":
                        var orderDetails = from od in db.Order_details
                                           join o in db.Orders on od.id_order_fk equals o.id_order
                                           join m in db.Menu on od.id_menu_item_fk equals m.id_menu_item
                                           select new
                                           {
                                               od.id_order_details,
                                               OrderId = o.id_order,
                                               MenuItem = m.item_name,
                                               od.quantity,
                                               od.unit_price,
                                               od.subtotal
                                           };
                        dataGrid.ItemsSource = orderDetails.ToList();
                        ConfigureDataGridColumns(orderDetails.FirstOrDefault());
                        break;

                    case "Autorization":
                        dataGrid.ItemsSource = db.Autorization.ToList();
                        ConfigureDataGridColumns(db.Autorization.FirstOrDefault());
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadTableNames();
            cbTables.SelectedIndex = 0; // Выбираем первую таблицу по умолчанию
        }

        private void ConfigureDataGridColumns(object sampleItem)
        {
            if (sampleItem == null) return;

            dataGrid.AutoGenerateColumns = false;
            dataGrid.Columns.Clear();

            var properties = sampleItem.GetType().GetProperties();

            foreach (var property in properties)
            {
                // Пропускаем ненужные или служебные свойства
                if (property.Name.Contains("Entity") || property.Name.Contains("Reference"))
                    continue;

                var column = new DataGridTextColumn
                {
                    Header = RussianTranslator.GetFieldName(property.Name),
                    Binding = new System.Windows.Data.Binding(property.Name)
                };
                dataGrid.Columns.Add(column);
            }
        }

        // Обработчик кнопки "Добавить"
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                switch (currentTableName)
                {
                    case "Post":
                        var postWindow = new EditPostWindow();
                        if (postWindow.ShowDialog() == true)
                            LoadTableData();
                        break;

                    case "Employees":
                        var empWindow = new EditEmployeeWindow();
                        if (empWindow.ShowDialog() == true)
                            LoadTableData();
                        break;

                    case "Clients":
                        var clientWindow = new EditClientWindow();
                        if (clientWindow.ShowDialog() == true)
                            LoadTableData();
                        break;

                    case "CategoriesMenu":
                        var categoryWindow = new EditCategoryWindow();
                        if (categoryWindow.ShowDialog() == true)
                            LoadTableData();
                        break;

                    case "Menu":
                        var menuWindow = new EditMenuWindow();
                        if (menuWindow.ShowDialog() == true)
                            LoadTableData();
                        break;

                    case "Orders":
                        var orderWindow = new EditOrderWindow();
                        if (orderWindow.ShowDialog() == true)
                            LoadTableData();
                        break;

                    case "Order_details":
                        var orderDetailWindow = new EditOrderDetailWindow();
                        if (orderDetailWindow.ShowDialog() == true)
                            LoadTableData();
                        break;

                    case "Reservation":
                        var reservationWindow = new EditReservationWindow();
                        if (reservationWindow.ShowDialog() == true)
                            LoadTableData();
                        break;

                    case "Regular_Clients":
                        var regularClientWindow = new EditRegularClientWindow();
                        if (regularClientWindow.ShowDialog() == true)
                            LoadTableData();
                        break;

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        // Обработчик двойного клика по DataGrid для редактирования
        private void dataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dataGrid.SelectedItem == null) return;

            try
            {
                switch (currentTableName)
                {
                    case "Post":
                        dynamic postItem = dataGrid.SelectedItem;
                        var post = db.Post.Find(postItem.id_post);
                        if (post != null)
                        {
                            var window = new EditPostWindow(post);
                            if (window.ShowDialog() == true)
                                LoadTableData();
                        }
                        break;

                    case "Employees":
                        dynamic empItem = dataGrid.SelectedItem;
                        var employee = db.Employees.Find(empItem.id_employee);
                        if (employee != null)
                        {
                            var window = new EditEmployeeWindow(employee);
                            if (window.ShowDialog() == true)
                                LoadTableData();
                        }
                        break;

                    case "Clients":
                        dynamic clientItem = dataGrid.SelectedItem;
                        var client = db.Clients.Find(clientItem.id_client);
                        if (client != null)
                        {
                            var window = new EditClientWindow(client);
                            if (window.ShowDialog() == true)
                                LoadTableData();
                        }
                        break;

                    case "CategoriesMenu":
                        dynamic categoryItem = dataGrid.SelectedItem;
                        var category = db.CategoriesMenu.Find(categoryItem.id_category);
                        if (category != null)
                        {
                            var window = new EditCategoryWindow(category);
                            if (window.ShowDialog() == true)
                                LoadTableData();
                        }
                        break;

                    case "Menu":
                        dynamic menuItem = dataGrid.SelectedItem;
                        var menu = db.Menu.Find(menuItem.id_menu_item);
                        if (menu != null)
                        {
                            var window = new EditMenuWindow(menu);
                            if (window.ShowDialog() == true)
                                LoadTableData();
                        }
                        break;

                    case "Orders":
                        dynamic orderItem = dataGrid.SelectedItem;
                        var order = db.Orders.Find(orderItem.id_order);
                        if (order != null)
                        {
                            var window = new EditOrderWindow(order);
                            if (window.ShowDialog() == true)
                                LoadTableData();
                        }
                        break;

                    case "Order_details":
                        dynamic detailItem = dataGrid.SelectedItem;
                        var orderDetail = db.Order_details.Find(detailItem.id_order_details);
                        if (orderDetail != null)
                        {
                            var window = new EditOrderDetailWindow(orderDetail);
                            if (window.ShowDialog() == true)
                                LoadTableData();
                        }
                        break;

                    case "Reservation":
                        dynamic reservationItem = dataGrid.SelectedItem;
                        var reservation = db.Reservation.Find(reservationItem.id_reservation);
                        if (reservation != null)
                        {
                            var window = new EditReservationWindow(reservation);
                            if (window.ShowDialog() == true)
                                LoadTableData();
                        }
                        break;

                    case "Regular_Clients":
                        MessageBox.Show("Для редактирования постоянных клиентов используйте форму редактирования клиентов");
                        break;

                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка редактирования: {ex.Message}");
            }
        }

        // Обработчик кнопки "Удалить"
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid.SelectedItem == null) return;

            try
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить эту запись?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    switch (currentTableName)
                    {
                        case "Post":
                            dynamic postItem = dataGrid.SelectedItem;
                            var post = db.Post.Find(postItem.id_post);
                            if (post != null)
                            {
                                db.Post.Remove(post);
                                db.SaveChanges();
                            }
                            break;

                        case "Employees":
                            dynamic empItem = dataGrid.SelectedItem;
                            var employee = db.Employees.Find(empItem.id_employee);
                            if (employee != null)
                            {
                                db.Employees.Remove(employee);
                                db.SaveChanges();
                            }
                            break;

                            // ... аналогично для других таблиц
                    }

                    LoadTableData();
                    MessageBox.Show("Запись успешно удалена");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}");
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                db.SaveChanges();
                MessageBox.Show("Изменения сохранены успешно!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadTableData();
        }

        
        private void MainMenuButton_Click(object sender, RoutedEventArgs e)
        {
            var mainDashboard = new MainDashboard();
            mainDashboard.Show();
            this.Close();
        }
    }
}