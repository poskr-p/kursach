using Microsoft.Win32;
using System;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace material_design
{
    public partial class MainWindow : Window
    {
        private cafe_barEntities1 db;

        public MainWindow()
        {
            InitializeComponent();
            db = new cafe_barEntities1();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                var query = from emp in db.Employees
                            join post in db.Post on emp.post_emp_fk equals post.id_post
                            select new
                            {
                                id_employee = emp.id_employee,
                                name_employee = emp.name_employee,
                                ph_number_emp = emp.ph_number_emp,
                                post_emp_fk = emp.post_emp_fk,
                                email = emp.email,
                                title_post = post.title_post,
                                photo_data = emp.photo_data
                            };

                dgProduct.ItemsSource = query.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }



        private void OpenEditEmployeeWindow(Employees employee = null)
        {
            var editWindow = new EditEmployeeWindow(employee);
            if (editWindow.ShowDialog() == true)
            {
                LoadData(); // Перезагружаем данные после сохранения
            }
        }



        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    OpenEditEmployeeWindow();
        //}

        private void Button_Click(object sender, RoutedEventArgs e) // Добавление
        {
            try
            {
                //Получаем данные изображения, если оно было загружено
                byte[] photoData = null;
                if (imgEmployee.Source != null && imgEmployee.Source is BitmapImage bitmapImage)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        BitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                        encoder.Save(ms);
                        photoData = ms.ToArray();
                    }
                }

                db.Employees.Add(new Employees
                {
                    id_employee = Convert.ToInt32(tbId.Text),
                    name_employee = tbName.Text,
                    ph_number_emp = tbNumber.Text,
                    post_emp_fk = Convert.ToInt32(tbpost.Text),
                    email = tbEmail.Text,
                    photo_data = photoData
                });

                db.SaveChanges();
                LoadData();
                ClearFields();
                MessageBox.Show("Данные успешно добавлены", "Успех",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void Button_Click_1(object sender, RoutedEventArgs e)
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
                var employee = db.Employees.Find(id);

                if (employee != null)
                {
                    var result = MessageBox.Show("Вы уверены, что хотите удалить этого сотрудника?",
                        "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        db.Employees.Remove(employee);
                        db.SaveChanges();
                        LoadData();
                        ClearFields();
                        MessageBox.Show("Сотрудник успешно удален");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}");
            }
        }




        //private void Button_Click_1(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        var employee = db.Employees.Find(Convert.ToInt32(tbId.Text));
        //        if (employee != null)
        //        {
        //            var result = MessageBox.Show("Вы уверены, что хотите удалить этого сотрудника?", "Подтверждение удаления",
        //                                      MessageBoxButton.YesNo, MessageBoxImage.Question);
        //            if (result == MessageBoxResult.Yes)
        //            {
        //                db.Employees.Remove(employee);
        //                db.SaveChanges();
        //                LoadData();
        //                ClearFields();
        //                MessageBox.Show("Данные успешно удалены", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        //            }
        //        }
        //        else
        //        {
        //            MessageBox.Show("Сотрудник с указанным ID не найден", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}

        private void ImportFromFile()
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|Excel files (*.xls;*.xlsx)|*.xls;*.xlsx",
                    Title = "Выберите файл для импорта"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string filePath = openFileDialog.FileName;
                    string extension = Path.GetExtension(filePath).ToLower();

                    if (extension == ".csv")
                    {
                        ImportFromCsv(filePath);
                    }
                    else if (extension == ".xls" || extension == ".xlsx")
                    {
                        MessageBox.Show("Для импорта из Excel установите библиотеку EPPlus", "Информация",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Неподдерживаемый формат файла", "Ошибка",
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка импорта: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ImportFromCsv(string filePath)
        {
            try
            {
                var lines = File.ReadAllLines(filePath, Encoding.UTF8);

                
                foreach (string line in lines.Skip(1))
                {
                    var values = line.Split(',');

                    if (values.Length >= 5)
                    {
                        var employee = new Employees
                        {
                            name_employee = values[0],
                            ph_number_emp = values[1],
                            post_emp_fk = int.Parse(values[2]),
                            email = values[3]
                        };

                        db.Employees.Add(employee);
                    }
                }

                db.SaveChanges();
                LoadData();
                MessageBox.Show("Данные успешно импортированы из CSV", "Успех",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка импорта CSV: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportToCsv()
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
                    File.WriteAllText(saveFileDialog.FileName, csvData, Encoding.UTF8);
                    MessageBox.Show("Данные успешно экспортированы в CSV файл", "Экспорт завершен",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте данных: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void BackupDatabase()
        {
            try
            {
                string backupFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "CafeBarBackups"
                );
                Directory.CreateDirectory(backupFolder);

                string backupFile = Path.Combine(
                    backupFolder,
                    $"cafe_bar_backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak"
                );

                // SQL-запрос для создания резервной копии
                string backupQuery = $@"
            BACKUP DATABASE [cafe_bar] 
            TO DISK = '{backupFile}' 
            WITH FORMAT, COMPRESSION;
        ";

                // Выполнение команды
                db.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, backupQuery);

                MessageBox.Show($"Резервная копия создана:\n{backupFile}", "Успех");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании резервной копии: {ex.Message}", "Ошибка");
            }
        }

        private void RestoreDatabase()
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "Backup files (*.bak)|*.bak",
                    InitialDirectory = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                        "CafeBarBackups"
                    )
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string backupFile = openFileDialog.FileName;

                    // SQL-запрос для восстановления
                    string restoreQuery = $@"
                USE [master];
                ALTER DATABASE [cafe_bar] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                RESTORE DATABASE [cafe_bar] 
                FROM DISK = '{backupFile}' 
                WITH REPLACE;
                ALTER DATABASE [cafe_bar] SET MULTI_USER;
            ";

                    db.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, restoreQuery);
                    MessageBox.Show("База данных успешно восстановлена!", "Успех");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при восстановлении: {ex.Message}", "Ошибка");
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
        }

        private void dgProduct_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgProduct.SelectedItem != null)
            {
                try
                {
                    dynamic selectedItem = dgProduct.SelectedItem;
                    int id = selectedItem.id_employee;
                    var employee = db.Employees.Find(id);

                    if (employee != null)
                    {
                        OpenEditEmployeeWindow(employee);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}");
                }
            }
        }

      

        private void BackupButton_Click(object sender, RoutedEventArgs e)
        {
            BackupDatabase();
        }

        private void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите восстановить базу данных из резервной копии? Все текущие данные будут заменены.",
                                      "Подтверждение восстановления",
                                      MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                RestoreDatabase();
            }
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            ImportFromFile();
        }

        private void ExportCsvButton_Click(object sender, RoutedEventArgs e)
        {
            ExportToCsv();
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
                    // Чтение файла в массив байтов
                    byte[] imageData = File.ReadAllBytes(openFileDialog.FileName);

                    // Отображение изображения в интерфейсе
                    DisplayImage(imageData);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RemovePhoto_Click(object sender, RoutedEventArgs e)
        {
            imgEmployee.Source = null;
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
                MessageBox.Show($"Ошибка отображения изображения: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
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