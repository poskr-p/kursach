using System;
using System.Linq;
using System.Windows;

namespace material_design
{
    public partial class EditMenuWindow : Window
    {
        private cafe_barEntities1 _db;
        private Menu _menuItem;

        public EditMenuWindow(Menu menuItem = null)
        {
            InitializeComponent();
            _db = new cafe_barEntities1();
            _menuItem = menuItem ?? new Menu();

            // Загружаем категории
            cmbCategory.ItemsSource = _db.CategoriesMenu.ToList();

            if (_menuItem.id_menu_item != 0)
            {
                txtName.Text = _menuItem.item_name;
                cmbCategory.SelectedValue = _menuItem.id_category_fk;
                txtPrice.Text = _menuItem.cost_item.ToString("F2");
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Введите название позиции");
                    return;
                }

                if (cmbCategory.SelectedValue == null)
                {
                    MessageBox.Show("Выберите категорию");
                    return;
                }

                if (!decimal.TryParse(txtPrice.Text, out decimal price) || price <= 0)
                {
                    MessageBox.Show("Введите корректную цену");
                    return;
                }

                _menuItem.item_name = txtName.Text;
                _menuItem.id_category_fk = (int)cmbCategory.SelectedValue;
                _menuItem.cost_item = price;

                if (_menuItem.id_menu_item == 0)
                    _db.Menu.Add(_menuItem);

                _db.SaveChanges();
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}