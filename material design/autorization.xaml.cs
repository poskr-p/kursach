using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace material_design
{
    public partial class autorization : Window
    {
        private cafe_barEntities db;

        public autorization()
        {
            InitializeComponent();
            db = new cafe_barEntities();

            if (tBL != null)
            {
                tBL.Text = "Логин";
                tBL.Foreground = Brushes.Gray;
            }

            if (tBP != null)
            {
                tBP.Password = "Пароль";
                
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string login = tBL.Text.Trim();
                string password = tBP.Password;

                if (login == "Логин" || string.IsNullOrEmpty(login))
                {
                    MessageBox.Show("Введите логин!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (password == "Пароль" || string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Введите пароль!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var user = db.Autorization.FirstOrDefault(u => u.Login == login);

                if (user == null)
                {
                    MessageBox.Show("Пользователь не найден!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                bool isPasswordValid = PasswordHelper.VerifyPassword(password, user.PasswordHash, user.Salt);

                if (isPasswordValid)
                {
                    int accessLevel = user.accessLevel;
                    string roleName = AccessControl.GetRoleName(accessLevel);

                    App.UserContext.SetUser(login, roleName, accessLevel);

                    MainDashboard mainDashboard = new MainDashboard(login, roleName, accessLevel);
                    mainDashboard.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Неверный пароль!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка авторизации: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var registrationWindow = new RegistrationWindow();
            registrationWindow.Show();
            this.Close();
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    if (textBox.Name == "tBL")
                    {
                        textBox.Text = "Логин";
                        textBox.Foreground = Brushes.Gray;
                    }
                }
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (textBox.Text == "Логин")
                {
                    textBox.Text = "";
                    textBox.Foreground = Brushes.Black;
                }
            }
        }

        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                if (passwordBox.Password == "Пароль")
                {
                    passwordBox.Password = "";
                   
                }
            }
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                if (string.IsNullOrEmpty(passwordBox.Password))
                {
                    passwordBox.Password = "Пароль";
                }
            }
        }
    }
}