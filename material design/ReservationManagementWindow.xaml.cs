using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;

namespace material_design
{
    public partial class ReservationManagementWindow : Window
    {
        private cafe_barEntities db;
        private Reservation currentReservation;

        public ReservationManagementWindow()
        {
            if (!App.UserContext.IsAuthenticated)
            {
                MessageBox.Show("Требуется авторизация");
                var authWindow = new autorization();
                authWindow.Show();
                this.Close();
                return;
            }

            db = new cafe_barEntities();
            InitializeData();
            LoadReservations();
        }

        private void InitializeData()
        {
            var clients = db.Clients.ToList();
            cbClient.ItemsSource = clients;
            cbClient.DisplayMemberPath = "name_client";
            cbClient.SelectedValuePath = "id_client";

            var employees = db.Employees
                .Where(e => e.Post.title_post == "Администратор" || e.Post.title_post == "Официант")
                .ToList();
            cbEmployee.ItemsSource = employees;
            cbEmployee.DisplayMemberPath = "name_employee";
            cbEmployee.SelectedValuePath = "id_employee";

            dpReservationDate.SelectedDate = DateTime.Today;
            cbReservationTime.SelectedIndex = 2; // 19:00 по умолчанию
        }

        private void LoadReservations()
        {
            var reservations = db.Reservation
                .Include(r => r.Clients)
                .Include(r => r.Employees)
                .OrderByDescending(r => r.reservation_date)
                .ToList();

            dgReservations.ItemsSource = reservations;
        }

        private void SaveReservation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cbClient.SelectedValue == null)
                {
                    MessageBox.Show("Выберите клиента!", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                    cbClient.Focus();
                    return;
                }

                if (cbEmployee.SelectedValue == null)
                {
                    MessageBox.Show("Выберите сотрудника!", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                    cbEmployee.Focus();
                    return;
                }

                if (!int.TryParse(txtGuestsCount.Text, out int guestsCount) || guestsCount < 1 || guestsCount > 20)
                {
                    MessageBox.Show("Введите корректное количество гостей (от 1 до 20)!",
                        "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtGuestsCount.Focus();
                    txtGuestsCount.SelectAll();
                    return;
                }

                if (dpReservationDate.SelectedDate == null)
                {
                    MessageBox.Show("Выберите дату!", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                    dpReservationDate.Focus();
                    return;
                }

                DateTime selectedDate = dpReservationDate.SelectedDate.Value;
                if (selectedDate < DateTime.Today)
                {
                    MessageBox.Show("Нельзя создавать бронирование на прошедшую дату!",
                        "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                    dpReservationDate.Focus();
                    return;
                }

                string timeStr = (cbReservationTime.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString();
                if (string.IsNullOrEmpty(timeStr))
                {
                    MessageBox.Show("Выберите время!", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                    cbReservationTime.Focus();
                    return;
                }

                if (!DateTime.TryParse($"{selectedDate:yyyy-MM-dd} {timeStr}", out DateTime reservationDateTime))
                {
                    MessageBox.Show("Ошибка формирования даты и времени!",
                        "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                TimeSpan time = reservationDateTime.TimeOfDay;
                if (time < TimeSpan.FromHours(10) || time > TimeSpan.FromHours(22))
                {
                    MessageBox.Show("Бронирование возможно только с 10:00 до 22:00!",
                        "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int clientId = (int)cbClient.SelectedValue;
                int employeeId = (int)cbEmployee.SelectedValue;

                var clientExists = db.Clients.Any(c => c.id_client == clientId);
                var employeeExists = db.Employees.Any(emp => emp.id_employee == employeeId);

                if (!clientExists)
                {
                    MessageBox.Show("Выбранный клиент не найден в базе!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!employeeExists)
                {
                    MessageBox.Show("Выбранный сотрудник не найден в базе!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (currentReservation == null)
                {
                    var newReservation = new Reservation
                    {
                        id_client_fk = clientId,
                        id_employee_fk = employeeId,
                        reservation_date = reservationDateTime,
                        guests_count = (byte)guestsCount
                    };

                    db.Reservation.Add(newReservation);

                    try
                    {
                        db.SaveChanges();
                        MessageBox.Show("Бронирование успешно добавлено!", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (System.Data.Entity.Infrastructure.DbUpdateException dbEx)
                    {
                        ExceptionHelper.ShowErrorMessage(dbEx, "добавления бронирования");
                        return;
                    }
                }
                else
                {
                    currentReservation.id_client_fk = clientId;
                    currentReservation.id_employee_fk = employeeId;
                    currentReservation.reservation_date = reservationDateTime;
                    currentReservation.guests_count = (byte)guestsCount;

                    try
                    {
                        db.SaveChanges();
                        MessageBox.Show("Бронирование успешно обновлено!", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (System.Data.Entity.Infrastructure.DbUpdateException dbEx)
                    {
                        ExceptionHelper.ShowErrorMessage(dbEx, "обновления бронирования");
                        return;
                    }
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
            if (currentReservation == null) return;

            try
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить это бронирование?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    db.Reservation.Remove(currentReservation);
                    db.SaveChanges();
                    MessageBox.Show("Бронирование успешно удалено!");
                    LoadReservations();
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
            currentReservation = null;
            cbClient.SelectedIndex = -1;
            cbEmployee.SelectedIndex = -1;
            dpReservationDate.SelectedDate = DateTime.Today;
            cbReservationTime.SelectedIndex = 2;
            txtGuestsCount.Text = "";
        }

        private void dgReservations_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (dgReservations.SelectedItem is Reservation reservation)
            {
                currentReservation = reservation;
                cbClient.SelectedValue = reservation.id_client_fk;
                cbEmployee.SelectedValue = reservation.id_employee_fk;
                dpReservationDate.SelectedDate = reservation.reservation_date;
                txtGuestsCount.Text = reservation.guests_count.ToString();

                string timeStr = reservation.reservation_date.ToString("HH:mm");
                foreach (System.Windows.Controls.ComboBoxItem item in cbReservationTime.Items)
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
            if (App.UserContext.IsAuthenticated)
            {
                var mainDashboard = new MainDashboard(
                    App.UserContext.UserName,
                    App.UserContext.UserRole,
                    App.UserContext.AccessLevel);
                mainDashboard.Show();
            }
            else
            {
                var authWindow = new autorization();
                authWindow.Show();
            }

            this.Close();
        }
    }
    
}