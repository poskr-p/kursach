using material_design.DTO;
using material_design.Repositories;
using material_design.Services;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace material_design
{
    public partial class MainWindow : Window
    {
        private readonly cafe_barEntities _context;
        private readonly IEmployeeService _employeeService;
        private Employees _currentEmployee;
        private byte[] _currentPhotoData;

        public MainWindow()
        {
            InitializeComponent();
            _context = new cafe_barEntities();

            var employeeRepo = new Repository<Employees>(_context);
            var postRepo = new Repository<Post>(_context);

            _employeeService = new EmployeeService(employeeRepo, postRepo);

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                var employees = _employeeService.GetAllEmployeesWithPost();
                dgProduct.ItemsSource = employees;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e) // Добавление
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tbName.Text) ||
                    string.IsNullOrWhiteSpace(tbNumber.Text) ||
                    string.IsNullOrWhiteSpace(tbpost.Text) ||
                    string.IsNullOrWhiteSpace(tbEmail.Text))
                {
                    MessageBox.Show("Заполните все поля!");
                    return;
                }

                var employee = new Employees
                {
                    id_employee = string.IsNullOrEmpty(tbId.Text) ? 0 : Convert.ToInt32(tbId.Text),
                    name_employee = tbName.Text,
                    ph_number_emp = tbNumber.Text,
                    post_emp_fk = Convert.ToInt32(tbpost.Text),
                    email = tbEmail.Text,
                    photo_data = _currentPhotoData
                };

                _employeeService.AddEmployee(employee);
                LoadData();
                ClearFields();
                MessageBox.Show("Данные успешно добавлены", "Успех");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления: {ex.Message}");
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) // Удаление
        {
            try
            {
                if (dgProduct.SelectedItem == null)
                {
                    MessageBox.Show("Выберите сотрудника для удаления");
                    return;
                }

                dynamic selectedItem = dgProduct.SelectedItem;
                int id = selectedItem.id_employee;

                var result = MessageBox.Show("Вы уверены, что хотите удалить этого сотрудника?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _employeeService.DeleteEmployee(id);
                    LoadData();
                    ClearFields();
                    MessageBox.Show("Сотрудник успешно удален");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}");
            }
        }

        private void dgProduct_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgProduct.SelectedItem != null)
            {
                dynamic selected = dgProduct.SelectedItem;
                tbId.Text = selected.id_employee.ToString();
                tbName.Text = selected.name_employee;
                tbNumber.Text = selected.ph_number_emp;
                tbpost.Text = selected.post_emp_fk.ToString();
                tbEmail.Text = selected.email;

                if (selected.photo_data != null)
                {
                    _currentPhotoData = selected.photo_data;
                    DisplayImage(_currentPhotoData);
                }
            }
        }

        private void ClearFields()
        {
            tbId.Text = "";
            tbName.Text = "";
            tbNumber.Text = "";
            tbpost.Text = "";
            tbEmail.Text = "";
            imgEmployee.Source = null;
            _currentPhotoData = null;
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv",
                    Title = "Выберите файл для импорта"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    _employeeService.ImportFromCsv(openFileDialog.FileName);
                    LoadData();
                    MessageBox.Show("Данные импортированы", "Успех");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка импорта: {ex.Message}");
            }
        }

        private void ExportCsvButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dgProduct.SelectAllCells();
                dgProduct.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
                ApplicationCommands.Copy.Execute(null, dgProduct);
                dgProduct.UnselectAllCells();

                string csvData = (string)Clipboard.GetData(DataFormats.CommaSeparatedValue);

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv",
                    DefaultExt = "csv",
                    FileName = $"employees_export_{DateTime.Now:yyyyMMdd}.csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveFileDialog.FileName, csvData, System.Text.Encoding.UTF8);
                    MessageBox.Show("Данные успешно экспортированы в CSV файл");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте данных: {ex.Message}");
            }
        }

        

        

        private void SelectPhoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png",
                Title = "Выберите фото сотрудника"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    _currentPhotoData = File.ReadAllBytes(openFileDialog.FileName);
                    DisplayImage(_currentPhotoData);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}");
                }
            }
        }

        private void RemovePhoto_Click(object sender, RoutedEventArgs e)
        {
            imgEmployee.Source = null;
            _currentPhotoData = null;
        }

        private void DisplayImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return;

            try
            {
                using (MemoryStream ms = new MemoryStream(imageData))
                {
                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = ms;
                    image.EndInit();
                    imgEmployee.Source = image;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка отображения изображения: {ex.Message}");
            }
        }

        private void MainMenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (Appp.CurrentUser != null)
            {
                string userName = Appp.CurrentUser.Login;
                string roleName = AccessControl.GetRoleName(Appp.CurrentUser.accessLevel);
                int accessLevel = Appp.CurrentUser.accessLevel;

                new MainDashboard(userName, roleName, accessLevel).Show();
            }
            else
            {
                // Если по какой-то причине пользователь не найден
                new MainDashboard("Гость", "Неизвестно", 0).Show();
            }
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _context?.Dispose();
        }
    }
}

