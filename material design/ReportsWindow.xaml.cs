using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace material_design
{
    public partial class ReportsWindow : Window
    {
        private cafe_barEntities1 db;

        public ReportsWindow()
        {
            InitializeComponent();
            db = new cafe_barEntities1();

            // Устанавливаем даты по умолчанию
            dpStartDate.SelectedDate = DateTime.Today.AddDays(-30);
            dpEndDate.SelectedDate = DateTime.Today;
        }

        private void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            if (cbReportType.SelectedItem == null)
            {
                MessageBox.Show("Выберите тип отчета");
                return;
            }

            if (dpStartDate.SelectedDate == null || dpEndDate.SelectedDate == null)
            {
                MessageBox.Show("Выберите период");
                return;
            }

            DateTime startDate = dpStartDate.SelectedDate.Value;
            DateTime endDate = dpEndDate.SelectedDate.Value;

            string reportType = ((ComboBoxItem)cbReportType.SelectedItem).Content.ToString();

            try
            {
                switch (reportType)
                {
                    case "Отчет по продажам за период":
                        GenerateSalesReport(startDate, endDate);
                        break;
                    case "Эффективность сотрудников":
                        GenerateEmployeeReport(startDate, endDate);
                        break;
                    case "Анализ клиентской базы":
                        GenerateClientReport();
                        break;
                    case "Популярность меню":
                        GenerateMenuReport(startDate, endDate);
                        break;
                    case "Финансовый отчет":
                        GenerateFinancialReport(startDate, endDate);
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка формирования отчета: {ex.Message}");
            }
        }

        private void GenerateSalesReport(DateTime startDate, DateTime endDate)
        {
            var salesData = (from od in db.Order_details
                             join o in db.Orders on od.id_order_fk equals o.id_order
                             join m in db.Menu on od.id_menu_item_fk equals m.id_menu_item
                             join c in db.CategoriesMenu on m.id_category_fk equals c.id_category
                             where o.order_date >= startDate && o.order_date <= endDate
                             group new { od, m, c } by new { c.title_category, m.item_name } into g
                             select new
                             {
                                 Категория = g.Key.title_category,
                                 Позиция = g.Key.item_name,
                                 Количество = (int)g.Sum(x => x.od.quantity),
                                 Выручка = (decimal)g.Sum(x => x.od.subtotal),
                                 Средняя_цена = (decimal)g.Average(x => x.od.unit_price)
                             }).ToList();

            dgReport.ItemsSource = salesData;

            // Добавляем итоги
            decimal totalRevenue = salesData.Sum(x => x.Выручка);
            int totalItems = salesData.Sum(x => x.Количество);

            MessageBox.Show($"Итоги за период:\nВыручка: {totalRevenue:C2}\nПозиций продано: {totalItems}");
        }

        private void GenerateEmployeeReport(DateTime startDate, DateTime endDate)
        {
            var employeeData = (from o in db.Orders
                                join e in db.Employees on o.id_emp_fk equals e.id_employee
                                join p in db.Post on e.post_emp_fk equals p.id_post
                                where o.order_date >= startDate && o.order_date <= endDate
                                group o by new { e.name_employee, p.title_post } into g
                                select new
                                {
                                    Сотрудник = g.Key.name_employee,
                                    Должность = g.Key.title_post,
                                    Заказов = g.Count(),
                                    Выручка = (decimal)g.Sum(x => x.totalAmount),
                                    Средний_чек = (decimal)g.Average(x => x.totalAmount)
                                }).OrderByDescending(x => x.Выручка).ToList();

            dgReport.ItemsSource = employeeData;
        }

        private void GenerateClientReport()
        {
            var clientData = (from c in db.Clients
                              join o in db.Orders on c.id_client equals o.id_cli_fk into orders
                              from o in orders.DefaultIfEmpty()
                              join rc in db.Regular_Clients on c.id_client equals rc.id_reg_client_fk into regular
                              from r in regular.DefaultIfEmpty()
                              group new { o, r } by new { c.id_client, c.name_client } into g
                              select new
                              {
                                  Клиент = g.Key.name_client,
                                  Всего_заказов = g.Count(x => x.o != null),
                                  Общая_сумма = (decimal)g.Sum(x => x.o != null ? x.o.totalAmount : 0),
                                  Средний_чек = (decimal)g.Average(x => x.o != null ? x.o.totalAmount : 0),
                                  Статус = g.Any(x => x.r != null) ? "Постоянный" : "Обычный",
                                  Скидка = (decimal)g.Max(x => x.r != null ? x.r.discount_rate : 0)
                              }).OrderByDescending(x => x.Общая_сумма).ToList();

            dgReport.ItemsSource = clientData;
        }

        private void GenerateMenuReport(DateTime startDate, DateTime endDate)
        {
            // Сначала получаем общую выручку за период
            decimal totalRevenue = (decimal)(from od in db.Order_details
                                             join o in db.Orders on od.id_order_fk equals o.id_order
                                             where o.order_date >= startDate && o.order_date <= endDate
                                             select od.subtotal).Sum();

            var menuData = (from od in db.Order_details
                            join o in db.Orders on od.id_order_fk equals o.id_order
                            join m in db.Menu on od.id_menu_item_fk equals m.id_menu_item
                            join c in db.CategoriesMenu on m.id_category_fk equals c.id_category
                            where o.order_date >= startDate && o.order_date <= endDate
                            group new { od, m } by new { c.title_category, m.item_name } into g
                            select new
                            {
                                Категория = g.Key.title_category,
                                Позиция = g.Key.item_name,
                                Продано = (int)g.Sum(x => x.od.quantity),
                                Выручка = (decimal)g.Sum(x => x.od.subtotal),
                                Доля_в_выручке = totalRevenue > 0 ?
                                    (decimal)g.Sum(x => x.od.subtotal) / totalRevenue * 100 : 0
                            }).OrderByDescending(x => x.Выручка).ToList();

            dgReport.ItemsSource = menuData;
        }

        private void GenerateFinancialReport(DateTime startDate, DateTime endDate)
        {
            var financialData = (from o in db.Orders
                                 where o.order_date >= startDate && o.order_date <= endDate
                                 group o by o.order_date.Date into g
                                 select new
                                 {
                                     Дата = g.Key,
                                     Заказов = g.Count(),
                                     Выручка = (decimal)g.Sum(x => x.totalAmount),
                                     Средний_чек = (decimal)g.Average(x => x.totalAmount)
                                 }).OrderBy(x => x.Дата).ToList();

            dgReport.ItemsSource = financialData;

            decimal totalRevenue = financialData.Sum(x => x.Выручка);
            int totalOrders = financialData.Sum(x => x.Заказов);
            decimal avgOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

            MessageBox.Show($"Финансовые итоги:\nОбщая выручка: {totalRevenue:C2}\n" +
                           $"Всего заказов: {totalOrders}\nСредний чек: {avgOrderValue:C2}");
        }

        private void ExportCsv_Click(object sender, RoutedEventArgs e)
        {
            if (dgReport.Items.Count == 0)
            {
                MessageBox.Show("Нет данных для экспорта");
                return;
            }

            try
            {
                Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv",
                    FileName = $"Отчет_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    using (var writer = new StreamWriter(saveFileDialog.FileName, false, System.Text.Encoding.UTF8))
                    {
                        // Заголовки
                        var headers = dgReport.Columns.Select(c => c.Header.ToString());
                        writer.WriteLine(string.Join(";", headers));

                        // Данные
                        foreach (var item in dgReport.Items)
                        {
                            var values = new List<string>();
                            foreach (var column in dgReport.Columns)
                            {
                                var content = column.GetCellContent(item);
                                if (content is TextBlock textBlock)
                                {
                                    values.Add($"\"{textBlock.Text.Replace("\"", "\"\"")}\"");
                                }
                                else if (content != null)
                                {
                                    values.Add($"\"{content.ToString().Replace("\"", "\"\"")}\"");
                                }
                                else
                                {
                                    values.Add("\"\"");
                                }
                            }
                            writer.WriteLine(string.Join(";", values));
                        }
                    }

                    MessageBox.Show("Данные экспортированы в CSV файл");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта в CSV: {ex.Message}");
            }
        }

        private void PrintReport_Click(object sender, RoutedEventArgs e)
        {
            if (dgReport.Items.Count == 0)
            {
                MessageBox.Show("Нет данных для печати");
                return;
            }

            try
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    printDialog.PrintVisual(dgReport, "Отчет");
                    MessageBox.Show("Отчет отправлен на печать");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка печати: {ex.Message}");
            }
        }

        private void MainMenu_Click(object sender, RoutedEventArgs e)
        {
            var mainDashboard = new MainDashboard();
            mainDashboard.Show();
            this.Close();
        }
        private void ExportExcel_Click(object sender, RoutedEventArgs e)
        {
            if (dgReport.Items.Count == 0)
            {
                MessageBox.Show("Нет данных для экспорта");
                return;
            }

            try
            {
                Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Excel files (*.xlsx)|*.xlsx",
                    FileName = $"Отчет_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Отчет");

                        // Заголовки
                        for (int i = 0; i < dgReport.Columns.Count; i++)
                        {
                            worksheet.Cell(1, i + 1).Value = dgReport.Columns[i].Header.ToString();
                            worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                            worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                        }

                        // Данные
                        for (int i = 0; i < dgReport.Items.Count; i++)
                        {
                            dynamic item = dgReport.Items[i];
                            for (int j = 0; j < dgReport.Columns.Count; j++)
                            {
                                var column = dgReport.Columns[j];
                                var cellValue = GetCellValue(item, column);
                                worksheet.Cell(i + 2, j + 1).Value = cellValue;

                                // Форматирование числовых полей
                                if (IsNumeric(cellValue))
                                {
                                    worksheet.Cell(i + 2, j + 1).Style.NumberFormat.Format = "#,##0.00";
                                }
                            }
                        }

                        // Автоподбор ширины колонок
                        worksheet.Columns().AdjustToContents();

                        workbook.SaveAs(saveFileDialog.FileName);
                    }

                    MessageBox.Show("Данные экспортированы в Excel файл");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта в Excel: {ex.Message}");
            }
        }

        private object GetCellValue(object item, DataGridColumn column)
        {
            var content = column.GetCellContent(item);

            if (content is TextBlock textBlock)
            {
                return textBlock.Text;
            }
            else if (content is CheckBox checkBox)
            {
                return checkBox.IsChecked;
            }
            else if (content is ComboBox comboBox)
            {
                return comboBox.SelectedValue;
            }
            else if (content != null)
            {
                return content.ToString();
            }

            return null;
        }

        private bool IsNumeric(object value)
        {
            if (value == null) return false;

            return value is int || value is decimal || value is double || value is float ||
                   value is long || value is short || value is byte ||
                   (value is string str && decimal.TryParse(str, out _));
        }
    }
}