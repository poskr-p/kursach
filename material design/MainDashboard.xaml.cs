using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace material_design
{
    public partial class MainDashboard : Window
    {
        private string currentUserName = "Пользователь";
        private string currentUserRole = "Гость";
        private int currentUserAccessLevel = 0;

        public class ModuleInfo
        {
            public string Name { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string Icon { get; set; }
        }

        // Конструктор без параметров (для совместимости)
        public MainDashboard()
        {
            InitializeComponent();
            InitializeUserInterface();
            LoadModules();
        }

        // Конструктор с параметрами (для авторизации)
        public MainDashboard(string userName, string userRole, int accessLevel)
        {
            InitializeComponent();
            currentUserName = userName;
            currentUserRole = userRole;
            currentUserAccessLevel = accessLevel;

            InitializeUserInterface();
            LoadModules();
        }

        private void InitializeUserInterface()
        {
            // Устанавливаем информацию о пользователе
            tbUserInfo.Text = $"{currentUserName} ({currentUserRole})";
            tbWelcome.Text = $"Добро пожаловать, {currentUserName}! Ваша роль: {currentUserRole}";
        }

        private void LoadModules()
        {
            var availableModules = new List<ModuleInfo>();

            // Базовые модули для всех ролей
            if (AccessControl.CanViewTables(currentUserAccessLevel))
            {
                availableModules.Add(new ModuleInfo
                {
                    Name = "TableView",
                    Title = "Просмотр таблиц",
                    Description = "Просмотр всех данных системы",
                    Icon = "📋"
                });
            }
            if (AccessControl.CanManageUsers(currentUserAccessLevel))
            {
                availableModules.Add(new ModuleInfo
                {
                    Name = "UserManagement",
                    Title = "Управление пользователями",
                    Description = "Создание и управление учетными записями",
                    Icon = "👤"
                });
            }
            if (AccessControl.CanTakeOrders(currentUserAccessLevel))
            {
                availableModules.Add(new ModuleInfo
                {
                    Name = "OrderTaking",
                    Title = "Прием заказов",
                    Description = "Создание и управление заказами",
                    Icon = "📝"
                });
            }

            // Модули для администратора
            if (AccessControl.CanManageEmployees(currentUserAccessLevel))
            {
                availableModules.Add(new ModuleInfo
                {
                    Name = "PersonnelManagement",
                    Title = "Управление персоналом",
                    Description = "Сотрудники, должности, графики",
                    Icon = "👥"
                });
            }

            if (AccessControl.CanManageReservations(currentUserAccessLevel))
            {
                availableModules.Add(new ModuleInfo
                {
                    Name = "ReservationManagement",
                    Title = "Управление бронированием",
                    Description = "Бронирование столиков, управление бронями",
                    Icon = "📅"
                });
            }

            if (AccessControl.CanManageMenu(currentUserAccessLevel))
            {
                availableModules.Add(new ModuleInfo
                {
                    Name = "MenuManagement",
                    Title = "Управление меню",
                    Description = "Категории, позиции меню, цены",
                    Icon = "🍽️"
                });
            }

            if (AccessControl.CanManageSchedule(currentUserAccessLevel))
            {
                availableModules.Add(new ModuleInfo
                {
                    Name = "ScheduleManagement",
                    Title = "Управление графиком",
                    Description = "График работы сотрудников",
                    Icon = "🕒"
                });
            }

            if (AccessControl.CanViewReports(currentUserAccessLevel))
            {
                availableModules.Add(new ModuleInfo
                {
                    Name = "Reports",
                    Title = "Отчеты и аналитика",
                    Description = "Анализ продаж и эффективности",
                    Icon = "📊"
                });
            }
            if (AccessControl.CanManageUsers(currentUserAccessLevel))
{
    availableModules.Add(new ModuleInfo 
    { 
        Name = "UserManagement", 
        Title = "Управление пользователями", 
        Description = "Создание и управление учетными записями",
        Icon = "👤" 
    });
}
            modulesContainer.ItemsSource = availableModules;
        }

        private void Module_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Controls.Border border && border.Tag is string moduleName)
            {
                OpenModule(moduleName);
            }
        }

        private void OpenModule(string moduleName)
        {
            try
            {
                switch (moduleName)
                {
                    case "PersonnelManagement":
                        if (AccessControl.CanManageEmployees(currentUserAccessLevel))
                        {
                            var mainWindow = new MainWindow();
                            mainWindow.Show();
                            this.Hide();
                        }
                        break;

                    case "TableView":
                        if (AccessControl.CanViewTables(currentUserAccessLevel))
                        {
                            var window2 = new Window2();
                            window2.Show();
                            this.Hide();
                        }
                        break;

                    case "OrderTaking":
                        if (AccessControl.CanTakeOrders(currentUserAccessLevel))
                        {
                            var orderWindow = new OrderTakingWindow(currentUserAccessLevel);
                            orderWindow.Show();
                            this.Hide();
                        }
                        break;

                    case "ReservationManagement":
                        if (AccessControl.CanManageReservations(currentUserAccessLevel))
                        {
                            var reservationWindow = new ReservationManagementWindow();
                            reservationWindow.Show();
                            this.Hide();
                        }
                        break;

                    case "MenuManagement":
                        if (AccessControl.CanManageMenu(currentUserAccessLevel))
                        {
                            var menuWindow = new MenuManagementWindow();
                            menuWindow.Show();
                            this.Hide();
                        }
                        break;

                    case "ScheduleManagement":
                        if (AccessControl.CanManageSchedule(currentUserAccessLevel))
                        {
                            var scheduleWindow = new ScheduleManagementWindow();
                            scheduleWindow.Show();
                            this.Hide();
                        }
                        break;

                    case "Reports":
                        if (AccessControl.CanViewReports(currentUserAccessLevel))
                        {
                            var reportsWindow = new ReportsWindow();
                            reportsWindow.Show();
                            this.Hide();
                        }
                        break;
                    case "UserManagement":
                        if (AccessControl.CanManageUsers(currentUserAccessLevel))
                        {
                            var userManagementWindow = new UserManagementWindow();
                            userManagementWindow.Show();
                            this.Hide();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия модуля: {ex.Message}");
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти из системы?",
                                        "Подтверждение выхода",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var authWindow = new autorization();
                authWindow.Show();
                this.Close();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }
    }
}