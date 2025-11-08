using System;
using System.Collections.Generic;

namespace material_design
{
    public static class RussianTranslator
    {
        private static readonly Dictionary<string, string> TableNames = new Dictionary<string, string>
        {
            {"Post", "Должности"},
            {"Employees", "Сотрудники"},
            {"Clients", "Клиенты"},
            {"Regular_Clients", "Постоянные клиенты"},
            {"Reservation", "Бронирования"},
            {"CategoriesMenu", "Категории меню"},
            {"Menu", "Меню"},
            {"Orders", "Заказы"},
            {"Order_details", "Детали заказов"},
            {"Autorization", "Авторизация"}
        };

        private static readonly Dictionary<string, string> FieldNames = new Dictionary<string, string>
        {
            // Post
            {"id_post", "ID"},
            {"title_post", "Название должности"},
            {"accessLevel", "Уровень доступа"},
            
            // Employees
            {"id_employee", "ID"},
            {"name_employee", "ФИО сотрудника"},
            {"ph_number_emp", "Телефон"},
            {"post_emp_fk", "Должность"},
            {"email", "Email"},
            {"photo_data", "Фото"},
            
            // Clients
            {"id_client", "ID"},
            {"name_client", "ФИО клиента"},
            {"ph_numb_client", "Телефон"},
            
            // Regular_Clients
            {"id_reg_client_fk", "ID клиента"},
            {"discount_rate", "Скидка (%)"},
            {"total_spent", "Всего потрачено"},
            
            // Reservation
            {"id_reservation", "ID"},
            {"id_client_fk", "Клиент"},
            {"id_employee_fk", "Сотрудник"},
            {"reservation_date", "Дата бронирования"},
            {"guests_count", "Количество гостей"},
            
            // CategoriesMenu
            {"id_category", "ID"},
            {"title_category", "Название категории"},
            
            // Menu
            {"id_menu_item", "ID"},
            {"item_name", "Название позиции"},
            {"id_category_fk", "Категория"},
            {"cost_item", "Цена"},
            
            // Orders
            {"id_order", "ID"},
            {"id_cli_fk", "Клиент"},
            {"id_emp_fk", "Сотрудник"},
            {"order_date", "Дата заказа"},
            {"totalAmount", "Общая сумма"},
            
            // Order_details
            {"id_order_details", "ID"},
            {"id_order_fk", "Заказ"},
            {"id_menu_item_fk", "Позиция меню"},
            {"quantity", "Количество"},
            {"unit_price", "Цена за единицу"},
            {"subtotal", "Сумма"},
            
            // Autorization
            {"id", "ID"},
            {"Login", "Логин"},
            {"PasswordHash", "Хэш пароля"},
            {"Salt", "Соль"}
        };

        public static string GetTableName(string englishName)
        {
            return TableNames.ContainsKey(englishName) ? TableNames[englishName] : englishName;
        }

        public static string GetFieldName(string englishName)
        {
            return FieldNames.ContainsKey(englishName) ? FieldNames[englishName] : englishName;
        }

        public static string TranslateQueryResult(string englishText)
        {
            // Для переводов результатов запросов, если нужно
            var translations = new Dictionary<string, string>
            {
                {"Regular", "Постоянный"},
                {"Potential Regular", "Потенциальный постоянный"},
                {"New", "Новый"}
            };

            return translations.ContainsKey(englishText) ? translations[englishText] : englishText;
        }
    }
}