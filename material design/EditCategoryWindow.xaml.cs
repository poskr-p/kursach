using System;
using System.Windows;

namespace material_design
{
    public partial class EditCategoryWindow : Window
    {
        private cafe_barEntities1 _db;
        private CategoriesMenu _category;

        public EditCategoryWindow(CategoriesMenu category = null)
        {
            InitializeComponent();
            _db = new cafe_barEntities1();
            _category = category ?? new CategoriesMenu();

            if (_category.id_category != 0)
            {
                txtTitle.Text = _category.title_category;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtTitle.Text))
                {
                    MessageBox.Show("Введите название категории");
                    return;
                }

                _category.title_category = txtTitle.Text;

                if (_category.id_category == 0)
                    _db.CategoriesMenu.Add(_category);

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