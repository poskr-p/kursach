using material_design.Services;
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
        private readonly cafe_barEntities _context;
        private readonly IBackupService _backupService;
        public MainDashboard()
        {
            InitializeComponent();
            InitializeUserInterface();
            LoadModules();

        }

        public MainDashboard(string userName, string userRole, int accessLevel)
        {
            InitializeComponent();
            currentUserName = userName;
            currentUserRole = userRole;
            currentUserAccessLevel = accessLevel;
            _context = new cafe_barEntities();
            _backupService = new BackupService(_context);
            InitializeUserInterface();
            LoadModules();
        }

        private void InitializeUserInterface()
        {
            tbUserInfo.Text = $"{currentUserName} ({currentUserRole})";
            tbWelcome.Text = $"Добро пожаловать, {currentUserName}! Ваша роль: {currentUserRole}";
        }

        private void LoadModules()
        {
            var availableModules = new List<ModuleInfo>();

            if (AccessControl.CanViewTables(currentUserAccessLevel))
                availableModules.Add(new ModuleInfo { Name = "TableView", Title = "Просмотр таблиц", Description = "Просмотр всех данных системы", Icon = "📋" });

            if (AccessControl.CanManageUsers(currentUserAccessLevel))
                availableModules.Add(new ModuleInfo { Name = "UserManagement", Title = "Управление пользователями", Description = "Создание и управление учетными записями", Icon = "👤" });

            if (AccessControl.CanTakeOrders(currentUserAccessLevel))
                availableModules.Add(new ModuleInfo { Name = "OrderTaking", Title = "Прием заказов", Description = "Создание и управление заказами", Icon = "📝" });

            if (AccessControl.CanManageEmployees(currentUserAccessLevel))
                availableModules.Add(new ModuleInfo { Name = "PersonnelManagement", Title = "Управление персоналом", Description = "Сотрудники, должности, графики", Icon = "👥" });

            if (AccessControl.CanManageReservations(currentUserAccessLevel))
                availableModules.Add(new ModuleInfo { Name = "ReservationManagement", Title = "Управление бронированием", Description = "Бронирование столиков, управление бронями", Icon = "📅" });

            if (AccessControl.CanManageMenu(currentUserAccessLevel))
                availableModules.Add(new ModuleInfo { Name = "MenuManagement", Title = "Управление меню", Description = "Категории, позиции меню, цены", Icon = "🍽️" });

            if (AccessControl.CanManageSchedule(currentUserAccessLevel))
                availableModules.Add(new ModuleInfo { Name = "ScheduleManagement", Title = "Управление графиком", Description = "График работы сотрудников", Icon = "🕒" });

            if (AccessControl.CanViewReports(currentUserAccessLevel))
                availableModules.Add(new ModuleInfo { Name = "Reports", Title = "Отчеты и аналитика", Description = "Анализ продаж и эффективности", Icon = "📊" });
            if (AccessControl.CanViewReports(currentUserAccessLevel)) // или другое право, например, CanTakeOrders
            {
                availableModules.Add(new ModuleInfo
                {
                    Name = "DemandForecast",
                    Title = "Прогноз спроса",
                    Description = "Прогнозирование продаж на основе ML",
                    Icon = "📈"
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
                            new MainWindow().Show();
                            this.Hide();
                        }
                        break;
                    case "TableView":
                        if (AccessControl.CanViewTables(currentUserAccessLevel))
                        {
                            new Window2().Show();
                            this.Hide();
                        }
                        break;
                    case "OrderTaking":
                        if (AccessControl.CanTakeOrders(currentUserAccessLevel))
                        {
                            // В реальности нужно передать ID сотрудника, но пока заглушка
                            new OrderTakingWindow(currentUserAccessLevel).Show();
                            this.Hide();
                        }
                        break;
                    case "ReservationManagement":
                        if (AccessControl.CanManageReservations(currentUserAccessLevel))
                        {
                            new ReservationManagementWindow().Show();
                            this.Hide();
                        }
                        break;
                    case "MenuManagement":
                        if (AccessControl.CanManageMenu(currentUserAccessLevel))
                        {
                            new MenuManagementWindow().Show();
                            this.Hide();
                        }
                        break;
                    case "ScheduleManagement":
                        if (AccessControl.CanManageSchedule(currentUserAccessLevel))
                        {
                            new ScheduleManagementWindow().Show();
                            this.Hide();
                        }
                        break;
                    case "Reports":
                        if (AccessControl.CanViewReports(currentUserAccessLevel))
                        {
                            new ReportsWindow().Show();
                            this.Hide();
                        }
                        break;
                    case "UserManagement":
                        if (AccessControl.CanManageUsers(currentUserAccessLevel))
                        {
                            new UserManagementWindow().Show();
                            this.Hide();
                        }
                        break;
                    case "DemandForecast":
                        var forecastWindow = new DemandForecastWindow();
                        forecastWindow.Owner = this;
                        forecastWindow.ShowDialog();
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
                new autorization().Show();
                this.Close();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            popupMenu.IsOpen = !popupMenu.IsOpen;
        }

        private void BackupDatabase_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!AccessControl.CanManageUsers(currentUserAccessLevel))
                {
                    MessageBox.Show("Только администратор может создавать резервные копии.", "Доступ запрещён",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (MessageBox.Show("Создать резервную копию базы данных?\nКопия будет сохранена в папке 'CafeBarBackups' в документах.",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    _backupService.BackupDatabase();
                    MessageBox.Show("Резервная копия успешно создана!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании резервной копии: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                popupMenu.IsOpen = false;
            }
        }

        private void RestoreDatabase_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!AccessControl.CanManageUsers(currentUserAccessLevel))
                {
                    MessageBox.Show("Только администратор может восстанавливать базу данных.", "Доступ запрещён",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Backup files (*.bak)|*.bak",
                    Title = "Выберите файл резервной копии"
                };

                if (dialog.ShowDialog() == true)
                {
                    if (MessageBox.Show("Восстановление заменит все текущие данные. Продолжить?",
                        "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        _backupService.RestoreDatabase(dialog.FileName);
                        MessageBox.Show("База данных восстановлена. Перезапустите приложение.", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка восстановления: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                popupMenu.IsOpen = false;
            }
        }

        private void AboutProgram_Click(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow();
            aboutWindow.Owner = this;
            aboutWindow.ShowDialog();
            popupMenu.IsOpen = false;
        }
        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите выйти?", "Выход",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Appp.CurrentUser = null;
                new autorization().Show();
                this.Close();
            }
            popupMenu.IsOpen = false;
        }
    }
}

