using material_design.DTO;
using material_design.Repositories;
using material_design.Services;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace material_design
{
    public partial class filtering : Window
    {
        private readonly cafe_barEntities _context;
        private readonly IFilterService _filterService;
        private string currentDataType = "Сотрудники";

        public filtering()
        {
            InitializeComponent();
            _context = new cafe_barEntities();
            var employeeRepo = new Repository<Employees>(_context);
            var postRepo = new Repository<Post>(_context);
            var clientRepo = new Repository<Clients>(_context);
            _filterService = new FilterService(employeeRepo, postRepo, clientRepo);

            cbDataType.SelectedIndex = 0;
            tbSearch.Text = "Поиск по имени";
            tbSearch.Foreground = Brushes.Gray;
        }

        private void cbDataType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbDataType.SelectedItem == null) return;
            currentDataType = (cbDataType.SelectedItem as ComboBoxItem)?.Content.ToString();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                if (currentDataType == "Сотрудники")
                {
                    var employees = _filterService.GetEmployees();
                    dgData.ItemsSource = employees;

                    cbFilter.ItemsSource = employees;
                    cbFilter.DisplayMemberPath = "name_employee";
                    cbFilter.SelectedValuePath = "id_employee";

                    ConfigureEmployeeColumns();
                }
                else if (currentDataType == "Клиенты")
                {
                    var clients = _filterService.GetClients();
                    dgData.ItemsSource = clients;

                    cbFilter.ItemsSource = clients;
                    cbFilter.DisplayMemberPath = "name_client";
                    cbFilter.SelectedValuePath = "id_client";

                    ConfigureClientColumns();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void ConfigureEmployeeColumns()
        {
            dgData.Columns.Clear();
            dgData.Columns.Add(new DataGridTextColumn
            {
                Header = RussianTranslator.GetFieldName("id_employee"),
                Binding = new System.Windows.Data.Binding("id_employee"),
                Width = 80
            });
            dgData.Columns.Add(new DataGridTextColumn
            {
                Header = RussianTranslator.GetFieldName("name_employee"),
                Binding = new System.Windows.Data.Binding("name_employee"),
                Width = 200
            });
            dgData.Columns.Add(new DataGridTextColumn
            {
                Header = RussianTranslator.GetFieldName("ph_number_emp"),
                Binding = new System.Windows.Data.Binding("ph_number_emp"),
                Width = 120
            });
            dgData.Columns.Add(new DataGridTextColumn
            {
                Header = RussianTranslator.GetFieldName("title_post"),
                Binding = new System.Windows.Data.Binding("title_post"),
                Width = 150
            });
        }

        private void ConfigureClientColumns()
        {
            dgData.Columns.Clear();
            dgData.Columns.Add(new DataGridTextColumn
            {
                Header = RussianTranslator.GetFieldName("id_client"),
                Binding = new System.Windows.Data.Binding("id_client"),
                Width = 80
            });
            dgData.Columns.Add(new DataGridTextColumn
            {
                Header = RussianTranslator.GetFieldName("name_client"),
                Binding = new System.Windows.Data.Binding("name_client"),
                Width = 250
            });
            dgData.Columns.Add(new DataGridTextColumn
            {
                Header = RussianTranslator.GetFieldName("ph_numb_client"),
                Binding = new System.Windows.Data.Binding("ph_numb_client"),
                Width = 120
            });
        }

        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_filterService == null) return;
            try
            {
                if (string.IsNullOrWhiteSpace(tbSearch.Text) || tbSearch.Text == "Поиск по имени")
                {
                    LoadData();
                    return;
                }

                if (currentDataType == "Сотрудники")
                {
                    var filtered = _filterService.GetEmployees(tbSearch.Text);
                    dgData.ItemsSource = filtered;
                }
                else if (currentDataType == "Клиенты")
                {
                    var filtered = _filterService.GetClients(tbSearch.Text);
                    dgData.ItemsSource = filtered;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка фильтрации: {ex.Message}");
            }
        }

        private void cbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbFilter.SelectedItem == null)
            {
                LoadData();
                return;
            }

            try
            {
                if (currentDataType == "Сотрудники")
                {
                    dynamic selected = cbFilter.SelectedItem;
                    int selectedId = selected.id_employee;
                    var filtered = _filterService.GetEmployees().Where(emp => emp.id_employee == selectedId).ToList();
                    dgData.ItemsSource = filtered;
                }
                else if (currentDataType == "Клиенты")
                {
                    dynamic selected = cbFilter.SelectedItem;
                    int selectedId = selected.id_client;
                    var filtered = _filterService.GetClients().Where(c => c.id_client == selectedId).ToList();
                    dgData.ItemsSource = filtered;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка фильтрации: {ex.Message}");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintDialog pd = new PrintDialog();
                if (pd.ShowDialog() == true)
                {
                    pd.PrintVisual(dgData, $"{currentDataType} Report");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка печати: {ex.Message}");
            }
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && textBox.Text == "Поиск по имени")
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
                textBox.Text = "Поиск по имени";
                textBox.Foreground = Brushes.Gray;
            }
        }

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


