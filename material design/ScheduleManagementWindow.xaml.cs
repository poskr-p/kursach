using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;

namespace material_design
{
    public partial class ScheduleManagementWindow : Window
    {
        private cafe_barEntities1 db;
        private DateTime currentWeekStart;

        public class EmployeeSchedule
        {
            public string EmployeeName { get; set; }
            public string Monday { get; set; }
            public string Tuesday { get; set; }
            public string Wednesday { get; set; }
            public string Thursday { get; set; }
            public string Friday { get; set; }
            public string Saturday { get; set; }
            public string Sunday { get; set; }
        }

        public ScheduleManagementWindow()
        {
            InitializeComponent();
            db = new cafe_barEntities1();
            InitializeData();
            currentWeekStart = GetStartOfWeek(DateTime.Today);
            UpdateWeekDisplay();
            LoadSchedule();
        }

        private void InitializeData()
        {
            // Загрузка сотрудников (только официанты и бармены для графика)
            var employees = db.Employees
                .Where(e => e.Post.title_post == "Официант" || e.Post.title_post == "Бармен")
                .ToList();
            cbEmployee.ItemsSource = employees;
            cbEmployee.DisplayMemberPath = "name_employee";
            cbEmployee.SelectedValuePath = "id_employee";

            // Установка текущей даты
            dpWorkDate.SelectedDate = DateTime.Today;
            cbStartTime.SelectedIndex = 4; // 12:00 по умолчанию
            cbEndTime.SelectedIndex = 6;   // 20:00 по умолчанию
        }

        private DateTime GetStartOfWeek(DateTime date)
        {
            int diff = date.DayOfWeek - DayOfWeek.Monday;
            if (diff < 0) diff += 7;
            return date.AddDays(-diff).Date;
        }

        private void UpdateWeekDisplay()
        {
            DateTime weekEnd = currentWeekStart.AddDays(6);
            tbWeekRange.Text = $"Неделя: {currentWeekStart:dd.MM.yyyy} - {weekEnd:dd.MM.yyyy}";
        }

        private void LoadSchedule()
        {
            DateTime weekEnd = currentWeekStart.AddDays(7);

            var employees = db.Employees
                .Where(e => e.Post.title_post == "Официант" || e.Post.title_post == "Бармен")
                .ToList();

            var scheduleList = new List<EmployeeSchedule>();

            foreach (var employee in employees)
            {
                var schedule = new EmployeeSchedule
                {
                    EmployeeName = employee.name_employee
                };

                // Для каждого дня недели определяем смену
                for (int i = 0; i < 7; i++)
                {
                    DateTime day = currentWeekStart.AddDays(i);
                    // В реальной системе здесь была бы загрузка из таблицы WorkSchedule
                    // Пока используем заглушку
                    string shift = GetShiftForEmployee(employee.id_employee, day);

                    switch (i)
                    {
                        case 0: schedule.Monday = shift; break;
                        case 1: schedule.Tuesday = shift; break;
                        case 2: schedule.Wednesday = shift; break;
                        case 3: schedule.Thursday = shift; break;
                        case 4: schedule.Friday = shift; break;
                        case 5: schedule.Saturday = shift; break;
                        case 6: schedule.Sunday = shift; break;
                    }
                }

                scheduleList.Add(schedule);
            }

            dgSchedule.ItemsSource = scheduleList;
        }

        private string GetShiftForEmployee(int employeeId, DateTime date)
        {
            // В реальной системе здесь загрузка из базы данных
            // Пока возвращаем заглушку
            var random = new Random(employeeId + date.DayOfYear);
            if (random.Next(0, 100) > 70) // 30% вероятность смены
            {
                string[] shifts = { "08:00-16:00", "12:00-20:00", "16:00-00:00" };
                return shifts[random.Next(0, shifts.Length)];
            }
            return "Выходной";
        }

        private void AddShift_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cbEmployee.SelectedValue == null)
                {
                    MessageBox.Show("Выберите сотрудника!");
                    return;
                }

                if (dpWorkDate.SelectedDate == null)
                {
                    MessageBox.Show("Выберите дату!");
                    return;
                }

                string startTime = (cbStartTime.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content.ToString();
                string endTime = (cbEndTime.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content.ToString();

                if (string.IsNullOrEmpty(startTime) || string.IsNullOrEmpty(endTime))
                {
                    MessageBox.Show("Выберите время начала и конца смены!");
                    return;
                }

                // В реальной системе здесь сохранение в базу данных
                MessageBox.Show($"Смена добавлена для сотрудника {cbEmployee.Text}\n" +
                              $"Дата: {dpWorkDate.SelectedDate.Value:dd.MM.yyyy}\n" +
                              $"Время: {startTime}-{endTime}");

                // Обновляем отображение
                LoadSchedule();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления смены: {ex.Message}");
            }
        }

        private void PreviousWeek_Click(object sender, RoutedEventArgs e)
        {
            currentWeekStart = currentWeekStart.AddDays(-7);
            UpdateWeekDisplay();
            LoadSchedule();
        }

        private void NextWeek_Click(object sender, RoutedEventArgs e)
        {
            currentWeekStart = currentWeekStart.AddDays(7);
            UpdateWeekDisplay();
            LoadSchedule();
        }

        private void CurrentWeek_Click(object sender, RoutedEventArgs e)
        {
            currentWeekStart = GetStartOfWeek(DateTime.Today);
            UpdateWeekDisplay();
            LoadSchedule();
        }

        private void ClearForm_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            cbEmployee.SelectedIndex = -1;
            dpWorkDate.SelectedDate = DateTime.Today;
            cbStartTime.SelectedIndex = 4;
            cbEndTime.SelectedIndex = 6;
        }

        private void ExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBox.Show("Экспорт в Excel - функция в разработке");
                // Реализация экспорта в Excel
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}");
            }
        }

        private void MainMenuButton_Click(object sender, RoutedEventArgs e)
        {
            new MainDashboard().Show();
            this.Close();
        }
    }
}