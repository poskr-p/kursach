using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace material_design
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static class UserContext
        {
            public static string UserName { get; private set; }
            public static string UserRole { get; private set; }
            public static int AccessLevel { get; private set; }
            public static bool IsAuthenticated => !string.IsNullOrEmpty(UserName);

            public static void SetUser(string userName, string userRole, int accessLevel)
            {
                UserName = userName;
                UserRole = userRole;
                AccessLevel = accessLevel;
            }

            public static void Clear()
            {
                UserName = null;
                UserRole = null;
                AccessLevel = 0;
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var authWindow = new autorization();
            authWindow.Show();

            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                MessageBox.Show($"Критическая ошибка: {args.ExceptionObject}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            };

            DispatcherUnhandledException += (sender, args) =>
            {
                MessageBox.Show($"Ошибка в интерфейсе: {args.Exception.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true;
            };
        }
    }
}
