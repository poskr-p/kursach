using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace material_design
{
    public static class AccessControl
    {
        // Права доступа по уровням
        public static bool CanManageEmployees(int accessLevel) => accessLevel >= 5;
        public static bool CanManageMenu(int accessLevel) => accessLevel >= 5;
        public static bool CanManageReservations(int accessLevel) => accessLevel >= 5;
        public static bool CanManageSchedule(int accessLevel) => accessLevel >= 5;
        public static bool CanViewReports(int accessLevel) => accessLevel >= 3;
        public static bool CanTakeOrders(int accessLevel) => accessLevel >= 2;
        public static bool CanViewTables(int accessLevel) => accessLevel >= 2;
        public static bool CanManageUsers(int accessLevel) => accessLevel >= 5;

        // Названия ролей
        public static string GetRoleName(int accessLevel)
        {
            switch (accessLevel)
            {
                case 5: return "Администратор";
                case 3: return "Бармен";
                case 2: return "Официант";
                default: return "Неизвестная роль";
            }
        }

        // Проверка доступа к конкретному модулю
        public static bool HasAccessToModule(string moduleName, int accessLevel)
        {
            switch (moduleName)
            {
                case "Управление персоналом": return CanManageEmployees(accessLevel);
                case "Просмотр таблиц": return CanViewTables(accessLevel);
                case "Фильтрация и поиск": return CanViewTables(accessLevel);
                case "Отчеты и аналитика": return CanViewReports(accessLevel);
                case "Прием заказов": return CanTakeOrders(accessLevel);
                case "Управление бронированием": return CanManageReservations(accessLevel);
                case "Управление графиком": return CanManageSchedule(accessLevel);
                case "Управление меню": return CanManageMenu(accessLevel);
                default: return false;
            }
        }
    }
}