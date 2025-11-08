using System;
using System.Linq;
using System.Windows;

namespace material_design
{
    public partial class EditReservationWindow : Window
    {
        private cafe_barEntities1 _db;
        private Reservation _reservation;

        public EditReservationWindow(Reservation reservation = null)
        {
            InitializeComponent();
            _db = new cafe_barEntities1();
            _reservation = reservation ?? new Reservation();

            // Загружаем клиентов и сотрудников
            cmbClient.ItemsSource = _db.Clients.ToList();
            cmbEmployee.ItemsSource = _db.Employees.ToList();

            if (_reservation.id_reservation != 0)
            {
                cmbClient.SelectedValue = _reservation.id_client_fk;
                cmbEmployee.SelectedValue = _reservation.id_employee_fk;
                dpDate.SelectedDate = _reservation.reservation_date;
                txtTime.Text = _reservation.reservation_date.ToString("HH:mm");
                txtGuests.Text = _reservation.guests_count.ToString();
            }
            else
            {
                dpDate.SelectedDate = DateTime.Now;
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
                    MessageBox.Show("Выберите дату бронирования");
                    return;
                }

                if (!DateTime.TryParse($"{dpDate.SelectedDate.Value.ToShortDateString()} {txtTime.Text}",
                    out DateTime reservationDate))
                {
                    MessageBox.Show("Введите корректное время (формат: HH:mm)");
                    return;
                }

                if (!byte.TryParse(txtGuests.Text, out byte guests) || guests < 1)
                {
                    MessageBox.Show("Введите корректное количество гостей (от 1)");
                    return;
                }

                _reservation.id_client_fk = (int)cmbClient.SelectedValue;
                _reservation.id_employee_fk = (int)cmbEmployee.SelectedValue;
                _reservation.reservation_date = reservationDate;
                _reservation.guests_count = guests;

                if (_reservation.id_reservation == 0)
                    _db.Reservation.Add(_reservation);

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