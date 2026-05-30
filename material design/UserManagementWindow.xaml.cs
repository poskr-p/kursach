using material_design.Repositories;
using material_design.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace material_design
{
    public partial class UserManagementWindow : Window
    {
        private readonly cafe_barEntities _context;
        private readonly IUserManagementService _userService;
        private Autorization _currentUser;

        public UserManagementWindow()
        {
            InitializeComponent();
            _context = new cafe_barEntities();
            var userRepo = new Repository<Autorization>(_context);
            _userService = new UserManagementService(userRepo);
            LoadData();
            LoadAccessLevels();
        }

        private void LoadData()
        {
            var users = _userService.GetAllUsers();
            foreach (var u in users)
                u.RoleName = AccessControl.GetRoleName(u.accessLevel);
            dgUsers.ItemsSource = users;
            ClearForm();
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
                    MessageBox.Show("Введите логин!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_currentUser == null) // Добавление
                {
                    if (string.IsNullOrEmpty(password) || password.Length < 4)
                    {
                        MessageBox.Show("Пароль должен содержать минимум 4 символа!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    if (cbAccessLevel.SelectedValue == null)
                    {
                        MessageBox.Show("Выберите уровень доступа!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    byte accessLevel = (byte)cbAccessLevel.SelectedValue;
                    _userService.AddUser(login, password, accessLevel);
                    MessageBox.Show($"Пользователь '{login}' успешно добавлен!", "Успех");
                }
                else // Редактирование
                {
                    if (_userService.GetAllUsers().Any(u => u.Login == login && u.id != _currentUser.id))
                    {
                        MessageBox.Show("Пользователь с таким логином уже существует!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    _currentUser.Login = login;
                    string newPassword = string.IsNullOrEmpty(password) || password.Length < 4 ? null : password;
                    _userService.UpdateUser(_currentUser, newPassword);
                    MessageBox.Show($"Пользователь '{login}' успешно обновлен!", "Успех");
                }

                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null) return;
            try
            {
                if (_userService.IsLastAdmin(_currentUser.id))
                {
                    MessageBox.Show("Нельзя удалить последнего администратора!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var result = MessageBox.Show($"Вы уверены, что хотите удалить пользователя '{_currentUser.Login}'?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    _userService.DeleteUser(_currentUser.id);
                    LoadData();
                    MessageBox.Show("Пользователь успешно удален!", "Успех");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearForm_Click(object sender, RoutedEventArgs e) => ClearForm();

        private void ClearForm()
        {
            _currentUser = null;
            txtLogin.Text = "";
            txtPassword.Password = "";
            cbAccessLevel.SelectedIndex = 0;
            btnDelete.IsEnabled = false;
        }

        private void dgUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgUsers.SelectedItem is Autorization user)
            {
                _currentUser = user;
                txtLogin.Text = user.Login;
                cbAccessLevel.SelectedValue = user.accessLevel;
                txtPassword.Password = "";
                btnDelete.IsEnabled = true;
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

