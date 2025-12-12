using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace material_design
{
    public partial class UserManagementWindow : Window
    {
        private cafe_barEntities db;
        private Autorization currentUser;

        public UserManagementWindow()
        {
            InitializeComponent();
            db = new cafe_barEntities();
            db.Autorization.Load(); // Загружаем данные
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                // Создаем список для DataGrid
                var users = db.Autorization.Local.ToList();

                // Добавляем RoleName для отображения
                foreach (var user in users)
                {
                    user.RoleName = AccessControl.GetRoleName(user.accessLevel);
                }

                dgUsers.ItemsSource = users;

                // Загружаем уровни доступа в ComboBox
                LoadAccessLevels();

                // Очищаем форму
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}\n\n{ex.InnerException?.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadAccessLevels()
        {
            var accessLevels = new Dictionary<byte, string>
            {
                { 5, "Администратор" },
                { 3, "Бармен" },
                { 2, "Официант" }
            };

            cbAccessLevel.ItemsSource = accessLevels;
            cbAccessLevel.DisplayMemberPath = "Value";
            cbAccessLevel.SelectedValuePath = "Key";
        }

        private void SaveUser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string login = txtLogin.Text.Trim();
                string password = txtPassword.Password;

                if (string.IsNullOrEmpty(login))
                {
                    MessageBox.Show("Введите логин!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (currentUser == null) // Добавление нового пользователя
                {
                    if (string.IsNullOrEmpty(password) || password.Length < 4)
                    {
                        MessageBox.Show("Пароль должен содержать минимум 4 символа!",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (cbAccessLevel.SelectedValue == null)
                    {
                        MessageBox.Show("Выберите уровень доступа!",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Проверяем, существует ли логин
                    if (db.Autorization.Any(u => u.Login == login))
                    {
                        MessageBox.Show("Пользователь с таким логином уже существует!",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    byte accessLevel = (byte)cbAccessLevel.SelectedValue;

                    // Генерируем хеш и соль
                    var (hash, salt) = PasswordHelper.GenerateHash(password);

                    var newUser = new Autorization
                    {
                        Login = login,
                        PasswordHash = hash,
                        Salt = salt,
                        accessLevel = accessLevel,
                        RoleName = AccessControl.GetRoleName(accessLevel)
                    };

                    db.Autorization.Add(newUser);
                    db.SaveChanges();

                    MessageBox.Show($"Пользователь '{login}' успешно добавлен!",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else // Редактирование существующего пользователя
                {
                    // Проверяем, не меняется ли логин на уже существующий
                    if (currentUser.Login != login &&
                        db.Autorization.Any(u => u.Login == login && u.id != currentUser.id))
                    {
                        MessageBox.Show("Пользователь с таким логином уже существует!",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    currentUser.Login = login;

                    if (!string.IsNullOrEmpty(password) && password.Length >= 4)
                    {
                        var (hash, salt) = PasswordHelper.GenerateHash(password);
                        currentUser.PasswordHash = hash;
                        currentUser.Salt = salt;
                    }

                    if (cbAccessLevel.SelectedValue != null)
                    {
                        byte accessLevel = (byte)cbAccessLevel.SelectedValue;
                        currentUser.accessLevel = accessLevel;
                        currentUser.RoleName = AccessControl.GetRoleName(accessLevel);
                    }

                    db.SaveChanges();

                    MessageBox.Show($"Пользователь '{login}' успешно обновлен!",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                LoadData();
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                string errorMessage = "Ошибка валидации:\n";
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        errorMessage += $"• {validationError.PropertyName}: {validationError.ErrorMessage}\n";
                    }
                }
                MessageBox.Show(errorMessage, "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
            {
                string errorMessage = "Ошибка обновления базы данных:\n";
                var innerException = ex.InnerException;
                while (innerException != null)
                {
                    errorMessage += $"{innerException.Message}\n";
                    innerException = innerException.InnerException;
                }
                MessageBox.Show(errorMessage, "Ошибка базы данных",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                string errorMessage = $"Ошибка сохранения: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\n\nВнутренняя ошибка: {ex.InnerException.Message}";
                    if (ex.InnerException.InnerException != null)
                    {
                        errorMessage += $"\nДетали: {ex.InnerException.InnerException.Message}";
                    }
                }
                MessageBox.Show(errorMessage, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser == null) return;

            try
            {
                // Проверяем, не удаляем ли мы последнего администратора
                if (currentUser.accessLevel == 5)
                {
                    int adminCount = db.Autorization.Count(u => u.accessLevel == 5);
                    if (adminCount <= 1)
                    {
                        MessageBox.Show("Нельзя удалить последнего администратора!",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                var result = MessageBox.Show($"Вы уверены, что хотите удалить пользователя '{currentUser.Login}'?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    db.Autorization.Remove(currentUser);
                    db.SaveChanges();

                    MessageBox.Show("Пользователь успешно удален!",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearForm_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            currentUser = null;
            txtLogin.Text = "";
            txtPassword.Password = "";
            cbAccessLevel.SelectedIndex = 0;
        }

        private void dgUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgUsers.SelectedItem is Autorization user)
            {
                currentUser = user;
                txtLogin.Text = user.Login;

                // Устанавливаем выбранный уровень доступа
                foreach (KeyValuePair<byte, string> item in cbAccessLevel.Items)
                {
                    if (item.Key == user.accessLevel)
                    {
                        cbAccessLevel.SelectedValue = item.Key;
                        break;
                    }
                }

                txtPassword.Password = ""; // Не показываем пароль
            }
        }

        private void MainMenuButton_Click(object sender, RoutedEventArgs e)
        {
            // Вернуться в главное меню
            var mainDashboard = new MainDashboard();
            mainDashboard.Show();
            this.Close();
        }
    }
}