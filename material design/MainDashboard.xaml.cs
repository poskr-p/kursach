using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace material_design
{
    public partial class MainDashboard : Window
    {
        public MainDashboard()
        {
            InitializeComponent();
        }

        private void Module_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border)
            {
                var stackPanel = border.Child as StackPanel;
                if (stackPanel != null && stackPanel.Children.Count > 1)
                {
                    var textBlock = stackPanel.Children[1] as TextBlock;
                    string moduleName = textBlock?.Text;

                    switch (moduleName)
                    {
                        case "Управление персоналом":
                            OpenPersonnelManagement();
                            break;
                        case "Просмотр таблиц":
                            OpenTableView();
                            break;
                        case "Фильтрация и поиск":
                            OpenFiltering();
                            break;
                        case "Отчеты и аналитика":
                            OpenReports();
                            break;
                    }
                }
            }
        }

        private void OpenPersonnelManagement()
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            this.Hide();
        }

        private void OpenTableView()
        {
            var window2 = new Window2();
            window2.Show();
            this.Hide();
        }

        private void OpenFiltering()
        {
            var filteringWindow = new filtering();
            filteringWindow.Show();
            this.Hide();
        }
        private void OpenReports()
        {
            var reportsWindow = new ReportsWindow();
            reportsWindow.Show();   
            this.Hide();
        }
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти из системы?",
                "Подтверждение выхода",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var authWindow = new autorization();
                authWindow.Show();
                this.Close();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }
    }
}