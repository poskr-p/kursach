using System;
using System.Linq;
using System.Windows;

namespace material_design
{
    public partial class EditRegularClientWindow : Window
    {
        private cafe_barEntities1 _db;
        private Regular_Clients _regularClient;

        public EditRegularClientWindow(Regular_Clients regularClient = null)
        {
            InitializeComponent();
            _db = new cafe_barEntities1();
            _regularClient = regularClient ?? new Regular_Clients();

            // Загружаем клиентов
            cmbClient.ItemsSource = _db.Clients.ToList();

            if (_regularClient.id_reg_client_fk != 0)
            {
                cmbClient.SelectedValue = _regularClient.id_reg_client_fk;
                txtDiscount.Text = _regularClient.discount_rate.ToString();
                txtTotalSpent.Text = _regularClient.total_spent.ToString();
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cmbClient.SelectedValue == null)
                {
                    MessageBox.Show("Выберите клиента");
                    return;
                }

                if (!decimal.TryParse(txtDiscount.Text, out decimal discount) || discount < 0 || discount > 100)
                {
                    MessageBox.Show("Введите корректный размер скидки (0-100%)");
                    return;
                }

                if (!decimal.TryParse(txtTotalSpent.Text, out decimal totalSpent) || totalSpent < 0)
                {
                    MessageBox.Show("Введите корректную сумму покупок");
                    return;
                }

                _regularClient.id_reg_client_fk = (int)cmbClient.SelectedValue;
                _regularClient.discount_rate = discount;
                _regularClient.total_spent = totalSpent;

                if (_db.Regular_Clients.Find(_regularClient.id_reg_client_fk) == null)
                    _db.Regular_Clients.Add(_regularClient);

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