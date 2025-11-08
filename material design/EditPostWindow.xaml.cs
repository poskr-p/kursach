using System;
using System.Windows;
using System.Windows.Controls;

namespace material_design
{
    public partial class EditPostWindow : Window
    {
        private cafe_barEntities1 _db;
        private Post _post;

        public EditPostWindow(Post post = null)
        {
            InitializeComponent();
            _db = new cafe_barEntities1();
            _post = post ?? new Post();

            if (_post.id_post != 0)
            {
                txtTitle.Text = _post.title_post;
                cmbAccessLevel.Text = _post.accessLevel.ToString();
            }
            else
            {
                cmbAccessLevel.SelectedIndex = 0;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtTitle.Text))
                {
                    MessageBox.Show("Введите название должности");
                    return;
                }

                if (!byte.TryParse(cmbAccessLevel.Text, out byte accessLevel) || accessLevel < 1 || accessLevel > 5)
                {
                    MessageBox.Show("Уровень доступа должен быть числом от 1 до 5");
                    return;
                }

                _post.title_post = txtTitle.Text;
                _post.accessLevel = accessLevel;

                if (_post.id_post == 0)
                    _db.Post.Add(_post);

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