using System;

namespace material_design.DTO
{
    public class SalesReportDto
    {
        public string Категория { get; set; }
        public string Позиция { get; set; }
        public int Количество { get; set; }
        public decimal Выручка { get; set; }
        public decimal Средняя_цена { get; set; }
    }

    public class EmployeeReportDto
    {
        public string Сотрудник { get; set; }
        public string Должность { get; set; }
        public int Заказов { get; set; }
        public decimal Выручка { get; set; }
        public decimal Средний_чек { get; set; }
    }

    public class ClientReportDto
    {
        public string Клиент { get; set; }
        public int Всего_заказов { get; set; }
        public decimal Общая_сумма { get; set; }
        public decimal Средний_чек { get; set; }
        public string Статус { get; set; }
        public decimal Скидка { get; set; }
    }

    public class MenuReportDto
    {
        public string Категория { get; set; }
        public string Позиция { get; set; }
        public int Продано { get; set; }
        public decimal Выручка { get; set; }
        public decimal Доля_в_выручке { get; set; }
    }

    public class FinancialReportDto
    {
        public DateTime Дата { get; set; }
        public int Заказов { get; set; }
        public decimal Выручка { get; set; }
        public decimal Средний_чек { get; set; }
    }
}