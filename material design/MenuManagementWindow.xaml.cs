using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;

namespace material_design
{
    public partial class MenuManagementWindow : Window
    {
        private cafe_barEntities1 db;
        private Menu currentMenuItem;

        public MenuManagementWindow()
        {
            InitializeComponent();
            db = new cafe_barEntities1();
            InitializeData();
            LoadMenuItems();
        }

        private void InitializeData()
        {
            // Загрузка категорий
            var categories = db.CategoriesMenu.ToList();
            cbCategory.ItemsSource = categories;
            cbCategory.DisplayMemberPath = "title_category";
            cbCategory.SelectedValuePath = "id_category";
        }

        private void LoadMenuItems()
        {
            var menuItems = db.Menu
                .Include(m => m.CategoriesMenu)
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

                if (currentMenuItem == null)
                {
                    // Добавление новой позиции
                    var newMenuItem = new Menu
                    {
                        item_name = itemName,
                        id_category_fk = (int)cbCategory.SelectedValue,
                        cost_item = cost
                    };

                    db.Menu.Add(newMenuItem);
                    db.SaveChanges();
                    MessageBox.Show("Позиция успешно добавлена в меню!");
                }
                else
                {
                    // Редактирование существующей позиции
                    currentMenuItem.item_name = itemName;
                    currentMenuItem.id_category_fk = (int)cbCategory.SelectedValue;
                    currentMenuItem.cost_item = cost;

                    db.SaveChanges();
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
            if (currentMenuItem == null) return;

            try
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить эту позицию из меню?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    db.Menu.Remove(currentMenuItem);
                    db.SaveChanges();
                    MessageBox.Show("Позиция успешно удалена из меню!");
                    LoadMenuItems();
                    ClearForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}");
            }
        }

        private void ClearForm_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            currentMenuItem = null;
            txtItemName.Text = "";
            cbCategory.SelectedIndex = -1;
            txtCost.Text = "";
        }

        private void dgMenuItems_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (dgMenuItems.SelectedItem is Menu menuItem)
            {
                currentMenuItem = menuItem;
                txtItemName.Text = menuItem.item_name;
                cbCategory.SelectedValue = menuItem.id_category_fk;
                txtCost.Text = menuItem.cost_item.ToString("0.00");
            }
        }

        private void MainMenuButton_Click(object sender, RoutedEventArgs e)
        {
            new MainDashboard().Show();
            this.Close();
        }
    }
}