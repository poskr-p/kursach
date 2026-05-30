using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace material_design
{
    public partial class Window2 : Window
    {
        private readonly cafe_barEntities _context;
        private Dictionary<string, Type> _tableTypes = new Dictionary<string, Type>();
        private string _currentTable;

        public Window2()
        {
            InitializeComponent();
            _context = new cafe_barEntities();
            InitializeTableTypes();
            LoadTables();
        }

        private void InitializeTableTypes()
        {
            _tableTypes.Add("Клиенты", typeof(Clients));
            _tableTypes.Add("Постоянные клиенты", typeof(Regular_Clients));
            _tableTypes.Add("Заказы", typeof(Orders));
            _tableTypes.Add("Детали заказов", typeof(Order_details));
        }

        private void LoadTables()
        {
            lvTables.Items.Clear();
            foreach (var table in _tableTypes.Keys)
            {
                lvTables.Items.Add(table);
            }
        }

        private void lvTables_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvTables.SelectedItem == null) return;

            _currentTable = lvTables.SelectedItem.ToString();
            tbTableTitle.Text = _currentTable;
            LoadTableData();
        }

        private void LoadTableData()
        {
            try
            {
                Type entityType = _tableTypes[_currentTable];
                var method = typeof(Window2).GetMethod("LoadTableDataGeneric", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .MakeGenericMethod(entityType);
                method.Invoke(this, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void LoadTableDataGeneric<T>() where T : class
        {
            if (typeof(T) == typeof(Clients))
            {
                var data = _context.Clients.ToList();
                dataGrid.ItemsSource = data.Select(c => new
                {
                    c.id_client,
                    ФИО = c.name_client,
                    Телефон = c.ph_numb_client
                }).ToList();
                dataGrid.AutoGenerateColumns = true;
            }
            else if (typeof(T) == typeof(Regular_Clients))
            {
                var data = _context.Regular_Clients.Include(rc => rc.Clients).ToList();
                dataGrid.ItemsSource = data.Select(rc => new
                {
                    ID_записи = rc.id_reg_client_fk,
                    Клиент = rc.Clients?.name_client,
                    Скидка = rc.discount_rate,
                    Всего_потрачено = rc.total_spent
                }).ToList();
                dataGrid.AutoGenerateColumns = true;
            }
            else if (typeof(T) == typeof(Orders))
            {
                var data = _context.Orders
                    .Include(o => o.Clients)
                    .Include(o => o.Employees)
                    .ToList();
                dataGrid.ItemsSource = data.Select(o => new
                {
                    o.id_order,
                    Клиент = o.Clients?.name_client,
                    Сотрудник = o.Employees?.name_employee,
                    Дата = o.order_date,
                    Сумма = o.totalAmount
                }).ToList();
                dataGrid.AutoGenerateColumns = true;
            }
            else if (typeof(T) == typeof(Order_details))
            {
                var data = _context.Order_details
                    .Include(od => od.Orders)
                    .Include(od => od.Menu)
                    .ToList();
                dataGrid.ItemsSource = data.Select(od => new
                {
                    od.id_order_details,
                    Заказ = od.Orders?.id_order,
                    Позиция = od.Menu?.item_name,
                    Количество = od.quantity,
                    Цена = od.unit_price,
                    Сумма = od.subtotal
                }).ToList();
                dataGrid.AutoGenerateColumns = true;
            }
            else
            {
                var dbSet = _context.Set<T>();
                dataGrid.ItemsSource = dbSet.ToList();
                dataGrid.AutoGenerateColumns = true;
            }

            if (dataGrid.ItemsSource != null)
            {
                var count = ((System.Collections.ICollection)dataGrid.ItemsSource).Count;
                tbStatus.Text = $"Загружено записей: {count}";
            }
            else
            {
                tbStatus.Text = "Нет данных";
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
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

