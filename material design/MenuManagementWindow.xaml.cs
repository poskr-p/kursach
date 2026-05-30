using material_design.Repositories;
using material_design.Services;
using System;
using System.Linq;
using System.Windows;

namespace material_design
{
    public partial class MenuManagementWindow : Window
    {
        private readonly cafe_barEntities _context;
        private readonly IMenuService _menuService;
        private Menu _currentMenuItem;

        public MenuManagementWindow()
        {
            InitializeComponent();
            _context = new cafe_barEntities();
            var menuRepo = new Repository<Menu>(_context);
            var categoryRepo = new Repository<CategoriesMenu>(_context);
            _menuService = new MenuService(menuRepo, categoryRepo);
            InitializeData();
            LoadMenuItems();
        }

        private void InitializeData()
        {
            var categories = _menuService.GetAllCategories();
            cbCategory.ItemsSource = categories;
            cbCategory.DisplayMemberPath = "title_category";
            cbCategory.SelectedValuePath = "id_category";
        }

        private void LoadMenuItems()
        {
            var menuItems = _menuService.GetAllMenuItems()
                .OrderBy(m => m.id_category_fk)
                .ThenBy(m => m.item_name)
                .ToList();
            dgMenuItems.ItemsSource = menuItems;
        }

        private void SaveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string itemName = txtItemName.Text.Trim();
                if (string.IsNullOrEmpty(itemName))
                {
                    MessageBox.Show("Введите название позиции!");
                    return;
                }

                if (cbCategory.SelectedValue == null)
                {
                    MessageBox.Show("Выберите категорию!");
                    return;
                }

                if (!decimal.TryParse(txtCost.Text, out decimal cost) || cost <= 0)
                {
                    MessageBox.Show("Введите корректную цену!");
                    return;
                }

                if (_currentMenuItem == null)
                {
                    var newMenuItem = new Menu
                    {
                        item_name = itemName,
                        id_category_fk = (int)cbCategory.SelectedValue,
                        cost_item = cost
                    };
                    _menuService.AddMenuItem(newMenuItem);
                    MessageBox.Show("Позиция успешно добавлена в меню!");
                }
                else
                {
                    _currentMenuItem.item_name = itemName;
                    _currentMenuItem.id_category_fk = (int)cbCategory.SelectedValue;
                    _currentMenuItem.cost_item = cost;
                    _menuService.UpdateMenuItem(_currentMenuItem);
                    MessageBox.Show("Позиция успешно обновлена!");
                }

                LoadMenuItems();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }

        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_currentMenuItem == null) return;
            try
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить эту позицию из меню?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    _menuService.DeleteMenuItem(_currentMenuItem.id_menu_item);
                    LoadMenuItems();
                    ClearForm();
                    MessageBox.Show("Позиция успешно удалена из меню!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}");
            }
        }

        private void ClearForm_Click(object sender, RoutedEventArgs e) => ClearForm();

        private void ClearForm()
        {
            _currentMenuItem = null;
            txtItemName.Text = "";
            cbCategory.SelectedIndex = -1;
            txtCost.Text = "";
        }

        private void dgMenuItems_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (dgMenuItems.SelectedItem is Menu menuItem)
            {
                _currentMenuItem = menuItem;
                txtItemName.Text = menuItem.item_name;
                cbCategory.SelectedValue = menuItem.id_category_fk;
                txtCost.Text = menuItem.cost_item.ToString("0.00");
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



