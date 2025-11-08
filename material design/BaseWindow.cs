using System.Windows;

namespace material_design
{
    public class BaseWindow : Window
    {
        protected void NavigateToMainDashboard()
        {
            var mainDashboard = new MainDashboard();
            mainDashboard.Show();
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