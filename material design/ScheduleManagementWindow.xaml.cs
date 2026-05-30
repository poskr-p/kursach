using material_design.DTO;
using material_design.Repositories;
using material_design.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace material_design
{
    public partial class ScheduleManagementWindow : Window
    {
        private readonly cafe_barEntities _context;
        private readonly IScheduleService _scheduleService;
        private DateTime currentWeekStart;

        public ScheduleManagementWindow()
        {
            InitializeComponent();
            _context = new cafe_barEntities();
            var employeeRepo = new Repository<Employees>(_context);
            var scheduleRepo = new Repository<WorkSchedule>(_context);
            var postRepo = new Repository<Post>(_context);
            _scheduleService = new ScheduleService(employeeRepo, scheduleRepo, postRepo);

            InitializeData();
            currentWeekStart = GetStartOfWeek(DateTime.Today);
            UpdateWeekDisplay();
            LoadSchedule();
        }

        private void InitializeData()
        {
            var employees = _scheduleService.GetEmployees();
            cbEmployee.ItemsSource = employees;
            cbEmployee.DisplayMemberPath = "name_employee";
            cbEmployee.SelectedValuePath = "id_employee";

            dpWorkDate.SelectedDate = DateTime.Today;
            cbStartTime.SelectedIndex = 4; // 12:00
            cbEndTime.SelectedIndex = 6;   // 20:00
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
            var scheduleList = _scheduleService.GetScheduleForWeek(currentWeekStart);
            dgSchedule.ItemsSource = scheduleList;
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

                string startTimeStr = (cbStartTime.SelectedItem as ComboBoxItem)?.Content.ToString();
                string endTimeStr = (cbEndTime.SelectedItem as ComboBoxItem)?.Content.ToString();

                if (string.IsNullOrEmpty(startTimeStr) || string.IsNullOrEmpty(endTimeStr))
                {
                    MessageBox.Show("Выберите время начала и конца смены!");
                    return;
                }

                if (!TimeSpan.TryParse(startTimeStr, out TimeSpan startTime) ||
                    !TimeSpan.TryParse(endTimeStr, out TimeSpan endTime))
                {
                    MessageBox.Show("Некорректный формат времени!");
                    return;
                }

                if (startTime >= endTime)
                {
                    MessageBox.Show("Время начала должно быть меньше времени окончания!");
                    return;
                }

                _scheduleService.AddShift((int)cbEmployee.SelectedValue, dpWorkDate.SelectedDate.Value, startTime, endTime);
                MessageBox.Show("Смена добавлена!");

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

        private void ClearForm_Click(object sender, RoutedEventArgs e) => ClearForm();

        private void ClearForm()
        {
            cbEmployee.SelectedIndex = -1;
            dpWorkDate.SelectedDate = DateTime.Today;
            cbStartTime.SelectedIndex = 4;
            cbEndTime.SelectedIndex = 6;
        }

        private void ExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Экспорт в Excel - функция в разработке");
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



