using material_design;
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

        public filtering()
        {
            try
            {
                InitializeComponent();
                db = new cafe_barEntities1();

                // Проверка подключения
                if (!db.Database.Exists())
                {
                    MessageBox.Show("Нет подключения к базе данных");
                    Close();
                    return;
                }

                LoadData();

                // Инициализация подсказки
                tbN.Text = "Поиск по имени";
                tbN.Foreground = Brushes.Gray;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации: {ex.Message}");
                Close();
            }
        }

        private void LoadData()
        {
            try
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

                dgP.ItemsSource = result;
                cbP.ItemsSource = result;
                cbP.DisplayMemberPath = "name_employee";
                cbP.SelectedValuePath = "id_employee";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void tbN_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (db == null) return;

            try
            {
                if (string.IsNullOrWhiteSpace(tbN.Text) || tbN.Text == "Поиск по имени")
                {
                    LoadData();
                    return;
                }

                var filtered = from emp in db.Employees
                               join post in db.Post on emp.post_emp_fk equals post.id_post
                               where emp.name_employee.Contains(tbN.Text)
                               select new
                               {
                                   emp.id_employee,
                                   emp.name_employee,
                                   emp.ph_number_emp,
                                   title_post = post.title_post
                               };

                dgP.ItemsSource = filtered.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка фильтрации: {ex.Message}");
            }
        }

        private void cbP_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbP.SelectedItem == null)
            {
                LoadData();
                return;
            }

            try
            {
                dynamic selected = cbP.SelectedItem;
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

                dgP.ItemsSource = filtered.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка фильтрации: {ex.Message}");
            }
        }

        // Остальные методы без изменений
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintDialog pd = new PrintDialog();
                if (pd.ShowDialog() == true)
                {
                    pd.PrintVisual(dgP, "Employees Report");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка печати: {ex.Message}");
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Close();
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
    }
}