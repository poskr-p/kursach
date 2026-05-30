using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace material_design
{
    public partial class DemandForecastWindow : Window
    {
        private readonly DemandForecastService _forecastService;
        private List<MenuItemInfo> _menuItems;

        public DemandForecastWindow()
        {
            InitializeComponent();
            _forecastService = new DemandForecastService();
            dpForecastDate.SelectedDate = DateTime.Today;
            LoadMenuItems();
        }

        private async void LoadMenuItems()
        {
            // Загружаем список позиций меню из базы данных (можно через сервис)
            try
            {
                using (var db = new cafe_barEntities())
                {
                    var items = db.Menu.Select(m => new MenuItemInfo
                    {
                        Id = m.id_menu_item,
                        ItemName = m.item_name
                    }).OrderBy(m => m.ItemName).ToList();

                    _menuItems = items;
                    lbMenuItems.ItemsSource = _menuItems;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки меню: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RbAllItems_Checked(object sender, RoutedEventArgs e)
        {
            lbMenuItems.IsEnabled = false;
        }

        private void RbSelectedItems_Checked(object sender, RoutedEventArgs e)
        {
            lbMenuItems.IsEnabled = true;
        }

        private async void BtnForecast_Click(object sender, RoutedEventArgs e)
        {
            if (dpForecastDate.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату прогноза.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime date = dpForecastDate.SelectedDate.Value;
            List<int> selectedIds;

            if (rbAllItems.IsChecked == true)
            {
                selectedIds = _menuItems.Select(item => item.Id).ToList();
            }
            else
            {
                selectedIds = lbMenuItems.SelectedItems.Cast<MenuItemInfo>().Select(item => item.Id).ToList();
                if (selectedIds.Count == 0)
                {
                    MessageBox.Show("Выберите хотя бы одну позицию.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            // Показать индикатор загрузки и заблокировать кнопку
            pbLoading.Visibility = Visibility.Visible;
            btnForecast.IsEnabled = false;
            dgResults.ItemsSource = null;

            try
            {
                var result = await _forecastService.GetForecastWithPollingAsync(date, selectedIds);
                if (result != null && result.predictions != null)
                {
                    dgResults.ItemsSource = result.predictions;
                    MessageBox.Show($"Прогноз получен для {result.predictions.Count} позиций.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Не удалось получить прогноз. Проверьте, запущен ли API-сервер.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                pbLoading.Visibility = Visibility.Collapsed;
                btnForecast.IsEnabled = true;
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public class MenuItemInfo
    {
        public int Id { get; set; }
        public string ItemName { get; set; }
    }
}