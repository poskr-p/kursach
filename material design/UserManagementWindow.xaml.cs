using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace material_design
{
    public partial class UserManagementWindow : Window
    {
        private cafe_barEntities1 db;
        private Autorization currentUser;

        public UserManagementWindow()
        {
            InitializeComponent();
            db = new cafe_barEntities1();
            LoadUsers();
            LoadAccessLevels();
        }

        private void LoadUsers()
        {
            var users = db.Autorization.ToList();
            foreach (var user in users)
            {
                user.RoleName = AccessControl.GetRoleName(user.accessLevel);
            }
            dgUsers.ItemsSource = users;
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
            cbAccessLevel.SelectedIndex = 0;
        }

        private void SaveUser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string login = txtLogin.Text.Trim();
                string password = txtPassword.Password;

                if (string.IsNullOrEmpty(login))
                {
                    MessageBox.Show("Введите логин!");
                    return;
                }

                if (currentUser == null)
                {
                    // Добавление нового пользователя
                    if (string.IsNullOrEmpty(password) || password.Length < 4)
                    {
                        MessageBox.Show("Пароль должен содержать минимум 4 символа!");
                        return;
                    }

                    if (db.Autorization.Any(u => u.Login == login))
                    {
                        MessageBox.Show("Пользователь с таким логином уже существует!");
                        return;
                    }

                    var (hash, salt) = PasswordHelper.GenerateHash(password);
                    var newUser = new Autorization
                    {
                        Login = login,
                        PasswordHash = hash,
                        Salt = salt,
                        accessLevel = (byte)cbAccessLevel.SelectedValue
                    };

                    db.Autorization.Add(newUser);
                    db.SaveChanges();
                    MessageBox.Show("Пользователь успешно добавлен!");
                }
                else
                {
                    // Редактирование существующего пользователя
                    currentUser.Login = login;
                    currentUser.accessLevel = (byte)cbAccessLevel.SelectedValue;

                    if (!string.IsNullOrEmpty(password) && password.Length >= 4)
                    {
                        var (hash, salt) = PasswordHelper.GenerateHash(password);
                        currentUser.PasswordHash = hash;
                        currentUser.Salt = salt;
                    }

                    db.SaveChanges();
                    MessageBox.Show("Пользователь успешно обновлен!");
                }

                LoadUsers();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }

        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser == null) return;

            try
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить этого пользователя?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    db.Autorization.Remove(currentUser);
                    db.SaveChanges();
                    MessageBox.Show("Пользователь успешно удален!");
                    LoadUsers();
                    ClearForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}");
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
                cbAccessLevel.SelectedValue = user.accessLevel;
                txtPassword.Password = ""; // не показываем
            }
        }

        private void MainMenuButton_Click(object sender, RoutedEventArgs e)
        {
            new MainDashboard().Show();
            this.Close();
        }
    }
}