using System.Windows;

namespace material_design
{
    public class BaseWindow : Window
    {
        protected void NavigateToMainDashboard()
        {
            if (App.UserContext.IsAuthenticated)
            {
                var mainDashboard = new MainDashboard(
                    App.UserContext.UserName,
                    App.UserContext.UserRole,
                    App.UserContext.AccessLevel);
                mainDashboard.Show();
            }
            else
            {
                var authWindow = new autorization();
                authWindow.Show();
            }

            this.Close();
        }

        protected void ShowError(string message)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        protected void ShowInfo(string message)
        {
            MessageBox.Show(message, "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}