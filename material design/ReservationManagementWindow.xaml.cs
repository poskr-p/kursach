using material_design.Repositories;
using material_design.Services;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace material_design
{
    public partial class ReservationManagementWindow : Window
    {
        private readonly cafe_barEntities _context;
        private readonly IReservationService _reservationService;
        private Reservation _currentReservation;

        public ReservationManagementWindow()
        {
            InitializeComponent();
            _context = new cafe_barEntities();
            var reservationRepo = new Repository<Reservation>(_context);
            var clientRepo = new Repository<Clients>(_context);
            var employeeRepo = new Repository<Employees>(_context);
            _reservationService = new ReservationService(reservationRepo, clientRepo, employeeRepo);
            InitializeData();
            LoadReservations();
        }

        private void InitializeData()
        {
            var clients = _reservationService.GetClients();
            cbClient.ItemsSource = clients;
            cbClient.DisplayMemberPath = "name_client";
            cbClient.SelectedValuePath = "id_client";

            var employees = _reservationService.GetEmployees()
                .Where(e => e.Post.title_post == "Администратор" || e.Post.title_post == "Официант")
                .ToList();
            cbEmployee.ItemsSource = employees;
            cbEmployee.DisplayMemberPath = "name_employee";
            cbEmployee.SelectedValuePath = "id_employee";

            dpReservationDate.SelectedDate = DateTime.Today;
            cbReservationTime.SelectedIndex = 2;
        }

        private void LoadReservations()
        {
            var reservations = _reservationService.GetAllReservations()
                .OrderByDescending(r => r.reservation_date)
                .ToList();
            dgReservations.ItemsSource = reservations;
        }

        private void SaveReservation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cbClient.SelectedValue == null) { MessageBox.Show("Выберите клиента!"); return; }
                if (cbEmployee.SelectedValue == null) { MessageBox.Show("Выберите сотрудника!"); return; }
                if (!int.TryParse(txtGuestsCount.Text, out int guests) || guests < 1 || guests > 20)
                { MessageBox.Show("Введите корректное количество гостей (1-20)!"); return; }
                if (dpReservationDate.SelectedDate == null) { MessageBox.Show("Выберите дату!"); return; }

                DateTime selectedDate = dpReservationDate.SelectedDate.Value;
                if (selectedDate < DateTime.Today) 
                { MessageBox.Show("Нельзя создавать бронирование на прошедшую дату!"); return; }

                string timeStr = (cbReservationTime.SelectedItem as ComboBoxItem)?.Content?.ToString();
                if (string.IsNullOrEmpty(timeStr)) { MessageBox.Show("Выберите время!"); return; }

                if (!DateTime.TryParse($"{selectedDate:yyyy-MM-dd} {timeStr}", out DateTime reservationDateTime))
                { MessageBox.Show("Ошибка формирования даты и времени!"); return; }

                if (_currentReservation == null)
                {
                    var newReservation = new Reservation
                    {
                        id_client_fk = (int)cbClient.SelectedValue,
                        id_employee_fk = (int)cbEmployee.SelectedValue,
                        reservation_date = reservationDateTime,
                        guests_count = (byte)guests
                    };
                    _reservationService.AddReservation(newReservation);
                    MessageBox.Show("Бронирование успешно добавлено!", "Успех");
                }
                else
                {
                    _currentReservation.id_client_fk = (int)cbClient.SelectedValue;
                    _currentReservation.id_employee_fk = (int)cbEmployee.SelectedValue;
                    _currentReservation.reservation_date = reservationDateTime;
                    _currentReservation.guests_count = (byte)guests;
                    _reservationService.UpdateReservation(_currentReservation);
                    MessageBox.Show("Бронирование успешно обновлено!", "Успех");
                }

                LoadReservations();
                ClearForm();
            }
            catch (Exception ex)
            {
                ExceptionHelper.ShowErrorMessage(ex, "сохранения бронирования");
            }
        }

        private void DeleteReservation_Click(object sender, RoutedEventArgs e)
        {
            if (_currentReservation == null) return;
            try
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить это бронирование?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    _reservationService.DeleteReservation(_currentReservation.id_reservation);
                    LoadReservations();
                    ClearForm();
                    MessageBox.Show("Бронирование успешно удалено!");
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
            _currentReservation = null;
            cbClient.SelectedIndex = -1;
            cbEmployee.SelectedIndex = -1;
            dpReservationDate.SelectedDate = DateTime.Today;
            cbReservationTime.SelectedIndex = 2;
            txtGuestsCount.Text = "";
        }

        private void dgReservations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgReservations.SelectedItem is Reservation reservation)
            {
                _currentReservation = reservation;
                cbClient.SelectedValue = reservation.id_client_fk;
                cbEmployee.SelectedValue = reservation.id_employee_fk;
                dpReservationDate.SelectedDate = reservation.reservation_date;
                txtGuestsCount.Text = reservation.guests_count.ToString();

                string timeStr = reservation.reservation_date.ToString("HH:mm");
                foreach (ComboBoxItem item in cbReservationTime.Items)
                {
                    if (item.Content.ToString() == timeStr)
                    {
                        cbReservationTime.SelectedItem = item;
                        break;
                    }
                }
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






