using System;
using System.Linq;
using System.Windows;

namespace material_design
{
    public partial class EditOrderWindow : Window
    {
        private cafe_barEntities1 _db;
        private Orders _order;

        public EditOrderWindow(Orders order = null)
        {
            InitializeComponent();
            _db = new cafe_barEntities1();
            _order = order ?? new Orders();

            // Загружаем клиентов и сотрудников
            cmbClient.ItemsSource = _db.Clients.ToList();
            cmbEmployee.ItemsSource = _db.Employees.ToList();

            if (_order.id_order != 0)
            {
                cmbClient.SelectedValue = _order.id_cli_fk;
                cmbEmployee.SelectedValue = _order.id_emp_fk;
                dpDate.SelectedDate = _order.order_date;
                txtTime.Text = _order.order_date.ToString("HH:mm");
                txtTotal.Text = _order.totalAmount.ToString("F2");
            }
            else
            {
                dpDate.SelectedDate = DateTime.Now;
                txtTime.Text = DateTime.Now.ToString("HH:mm");
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

                if (cmbEmployee.SelectedValue == null)
                {
                    MessageBox.Show("Выберите сотрудника");
                    return;
                }

                if (dpDate.SelectedDate == null)
                {
                    MessageBox.Show("Выберите дату заказа");
                    return;
                }

                if (!DateTime.TryParse($"{dpDate.SelectedDate.Value.ToShortDateString()} {txtTime.Text}",
                    out DateTime orderDate))
                {
                    MessageBox.Show("Введите корректное время (формат: HH:mm)");
                    return;
                }

                if (!decimal.TryParse(txtTotal.Text, out decimal total) || total < 0)
                {
                    MessageBox.Show("Введите корректную сумму");
                    return;
                }

                _order.id_cli_fk = (int)cmbClient.SelectedValue;
                _order.id_emp_fk = (int)cmbEmployee.SelectedValue;
                _order.order_date = orderDate;
                _order.totalAmount = total;

                if (_order.id_order == 0)
                    _db.Orders.Add(_order);

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