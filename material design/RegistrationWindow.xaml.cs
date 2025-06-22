using material_design;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace material_design
{
    /// <summary>
    /// Логика взаимодействия для RegistrationWindow.xaml
    /// </summary>
    public partial class RegistrationWindow : Window
    {
        private cafe_barEntities1 db;

        public RegistrationWindow()
        {
            InitializeComponent();
            db = new cafe_barEntities1();
            UsernameTextBox.Text = UsernameTextBox.Tag.ToString();
            UsernameTextBox.Foreground = Brushes.Gray;
            PasswordBox.Password = PasswordBox.Tag.ToString();
            PasswordBox.Foreground = Brushes.Gray;
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(UsernameTextBox.Text) || PasswordBox.Password.Length < 4)
                {
                    MessageBox.Show("Логин не может быть пустым, а пароль должен содержать минимум 4 символа");
                    return;
                }

                if (db.Autorization.Any(u => u.Login == UsernameTextBox.Text))
                {
                    MessageBox.Show("Пользователь с таким логином уже существует");
                    return;
                }

                var (hash, salt) = PasswordHelper.GenerateHash(PasswordBox.Password);

                var newUser = new Autorization
                {
                    Login = UsernameTextBox.Text,
                    PasswordHash = hash,
                    Salt = salt
                };

                db.Autorization.Add(newUser);
                db.SaveChanges();

                MessageBox.Show("Регистрация успешна!");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка регистрации: {ex.Message}");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && textBox.Text == textBox.Tag.ToString())
            {
                textBox.Text = "";
                textBox.Foreground = Brushes.Black;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = textBox.Tag.ToString();
                textBox.Foreground = Brushes.Gray;
            }
        }

        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            if (passwordBox != null && passwordBox.Password == passwordBox.Tag.ToString())
            {
                passwordBox.Password = "";
                passwordBox.Foreground = Brushes.Black;
            }
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            if (passwordBox != null && string.IsNullOrWhiteSpace(passwordBox.Password))
            {
                passwordBox.Password = passwordBox.Tag.ToString();
                passwordBox.Foreground = Brushes.Gray;
            }
        }

        // В конструкторе окна инициализируйте подсказки:
        
    }


}
