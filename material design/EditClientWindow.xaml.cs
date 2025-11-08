using System;
using System.Windows;

namespace material_design
{
    public partial class EditClientWindow : Window
    {
        private cafe_barEntities1 _db;
        private Clients _client;

        public EditClientWindow(Clients client = null)
        {
            InitializeComponent();
            _db = new cafe_barEntities1();
            _client = client ?? new Clients();

            if (_client.id_client != 0)
            {
                txtName.Text = _client.name_client;
                txtPhone.Text = _client.ph_numb_client;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Введите ФИО клиента");
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtPhone.Text) || txtPhone.Text.Length != 11)
                {
                    MessageBox.Show("Введите корректный номер телефона (11 цифр)");
                    return;
                }

                _client.name_client = txtName.Text;
                _client.ph_numb_client = txtPhone.Text;

                if (_client.id_client == 0)
                    _db.Clients.Add(_client);

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