using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace material_design
{
    public partial class filtering : Window
    {
        private cafe_barEntities1 db;
        private string currentDataType = "Сотрудники";

        public filtering()
        {
            try
            {
                InitializeComponent();
                db = new cafe_barEntities1();

                // Устанавливаем начальный выбор
                cbDataType.SelectedIndex = 0;

                // Инициализация подсказки
                tbSearch.Text = "Поиск по имени";
                tbSearch.Foreground = Brushes.Gray;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации: {ex.Message}");
                Close();
            }
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
                    LoadEmployeesData();
                }
                else if (currentDataType == "Клиенты")
                {
                    LoadClientsData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void LoadEmployeesData()
        {
            var query = from emp in db.Employees
                        join post in db.Post on emp.post_emp_fk equals post.id_post
                        select new
                        {
                            emp.id_employee,
                            emp.name_employee,
                            emp.ph_number_emp,
                            title_post = post.title_post
                        };

            var result = query.ToList();
            dgData.ItemsSource = result;

            // Настройка ComboBox для фильтрации
            cbFilter.ItemsSource = result;
            cbFilter.DisplayMemberPath = "name_employee";
            cbFilter.SelectedValuePath = "id_employee";

            // Настройка колонок DataGrid с русскими названиями
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

        private void LoadClientsData()
        {
            var query = from client in db.Clients
                        select new
                        {
                            client.id_client,
                            client.name_client,
                            client.ph_numb_client
                        };

            var result = query.ToList();
            dgData.ItemsSource = result;

            // Настройка ComboBox для фильтрации
            cbFilter.ItemsSource = result;
            cbFilter.DisplayMemberPath = "name_client";
            cbFilter.SelectedValuePath = "id_client";

            // Настройка колонок DataGrid с русскими названиями
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
            if (db == null) return;

            try
            {
                if (string.IsNullOrWhiteSpace(tbSearch.Text) || tbSearch.Text == "Поиск по имени")
                {
                    LoadData();
                    return;
                }

                if (currentDataType == "Сотрудники")
                {
                    var filtered = from emp in db.Employees
                                   join post in db.Post on emp.post_emp_fk equals post.id_post
                                   where emp.name_employee.Contains(tbSearch.Text)
                                   select new
                                   {
                                       emp.id_employee,
                                       emp.name_employee,
                                       emp.ph_number_emp,
                                       title_post = post.title_post
                                   };
                    dgData.ItemsSource = filtered.ToList();
                }
                else if (currentDataType == "Клиенты")
                {
                    var filtered = from client in db.Clients
                                   where client.name_client.Contains(tbSearch.Text)
                                   select new
                                   {
                                       client.id_client,
                                       client.name_client,
                                       client.ph_numb_client
                                   };
                    dgData.ItemsSource = filtered.ToList();
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

                    var filtered = from emp in db.Employees
                                   join post in db.Post on emp.post_emp_fk equals post.id_post
                                   where emp.id_employee == selectedId
                                   select new
                                   {
                                       emp.id_employee,
                                       emp.name_employee,
                                       emp.ph_number_emp,
                                       title_post = post.title_post
                                   };
                    dgData.ItemsSource = filtered.ToList();
                }
                else if (currentDataType == "Клиенты")
                {
                    dynamic selected = cbFilter.SelectedItem;
                    int selectedId = selected.id_client;

                    var filtered = from client in db.Clients
                                   where client.id_client == selectedId
                                   select new
                                   {
                                       client.id_client,
                                       client.name_client,
                                       client.ph_numb_client
                                   };
                    dgData.ItemsSource = filtered.ToList();
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
            var mainDashboard = new MainDashboard();
            mainDashboard.Show();
            this.Close();
        }
    }
}