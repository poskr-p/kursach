using material_design;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace material_design
{
    public partial class autorization : Window
    {
        private cafe_barEntities1 db;

        public autorization()
        {
            InitializeComponent();
            db = new cafe_barEntities1();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string login = tBL.Text.Trim();
                string password = tBP.Password;

                if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Введите логин и пароль!");
                    return;
                }

                // Ищем пользователя в таблице авторизации
                var user = db.Autorization.FirstOrDefault(u => u.Login == login);

                if (user == null)
                {
                    MessageBox.Show("Пользователь не найден!");
                    return;
                }

                // Проверяем пароль с использованием соли
                bool isPasswordValid = PasswordHelper.VerifyPassword(password, user.PasswordHash, user.Salt);

                if (isPasswordValid)
                {
                    // Получаем уровень доступа из таблицы Autorization
                    int accessLevel = user.accessLevel;
                    string roleName = AccessControl.GetRoleName(accessLevel);

                    // Успешный вход
                    new MainDashboard(login, roleName, accessLevel).Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Неверный пароль!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка авторизации: {ex.Message}");
            }
        }

        // ... остальные методы для работы с UI (без изменений)
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && textBox.Text == textBox.Tag?.ToString())
            {
                textBox.Text = "";
                textBox.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = textBox.Tag?.ToString();
                textBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            if (passwordBox != null && passwordBox.Password == passwordBox.Tag?.ToString())
            {
                passwordBox.Password = "";
                passwordBox.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            if (passwordBox != null && string.IsNullOrWhiteSpace(passwordBox.Password))
            {
                passwordBox.Password = passwordBox.Tag?.ToString();
                passwordBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }
    }
}