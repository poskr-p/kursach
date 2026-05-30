using System;
using System.Collections.Generic;
using System.Linq;
using material_design.DTO;
using material_design.Repositories;

namespace material_design.Services
{
    public class ReportService : IReportService
    {
        private readonly IRepository<Orders> _orderRepo;
        private readonly IRepository<Order_details> _orderDetailsRepo;
        private readonly IRepository<Menu> _menuRepo;
        private readonly IRepository<CategoriesMenu> _categoryRepo;
        private readonly IRepository<Employees> _employeeRepo;
        private readonly IRepository<Clients> _clientRepo;
        private readonly IRepository<Regular_Clients> _regularClientRepo;
        private readonly IRepository<Post> _postRepo;

        public ReportService(
            IRepository<Orders> orderRepo,
            IRepository<Order_details> orderDetailsRepo,
            IRepository<Menu> menuRepo,
            IRepository<CategoriesMenu> categoryRepo,
            IRepository<Employees> employeeRepo,
            IRepository<Clients> clientRepo,
            IRepository<Regular_Clients> regularClientRepo,
            IRepository<Post> postRepo) // Добавили postRepo
        {
            _orderRepo = orderRepo;
            _orderDetailsRepo = orderDetailsRepo;
            _menuRepo = menuRepo;
            _categoryRepo = categoryRepo;
            _employeeRepo = employeeRepo;
            _clientRepo = clientRepo;
            _regularClientRepo = regularClientRepo;
            _postRepo = postRepo;
        }

        public List<SalesReportDto> GetSalesReport(DateTime start, DateTime end)
        {
            // Получаем все данные из БД
            var orders = _orderRepo.GetAll().Where(o => o.order_date >= start && o.order_date <= end).ToList();
            var orderDetails = _orderDetailsRepo.GetAll().ToList();
            var menu = _menuRepo.GetAll().ToList();
            var categories = _categoryRepo.GetAll().ToList();

            // Соединяем в памяти (LINQ to Objects)
            var query = from od in orderDetails
                        join o in orders on od.id_order_fk equals o.id_order
                        join m in menu on od.id_menu_item_fk equals m.id_menu_item
                        join c in categories on m.id_category_fk equals c.id_category
                        group new { od, m, c } by new { c.title_category, m.item_name } into g
                        select new SalesReportDto
                        {
                            Категория = g.Key.title_category,
                            Позиция = g.Key.item_name,
                            Количество = (int)g.Sum(x => x.od.quantity),
                            Выручка = (decimal)g.Sum(x => x.od.subtotal.GetValueOrDefault()),
                            Средняя_цена = g.Count() > 0 ? (decimal)g.Average(x => x.od.unit_price) : 0
                        };

            return query.ToList();
        }

        public List<EmployeeReportDto> GetEmployeeReport(DateTime start, DateTime end)
        {
            var orders = _orderRepo.GetAll().Where(o => o.order_date >= start && o.order_date <= end).ToList();
            var employees = _employeeRepo.GetAll().ToList();
            var posts = _postRepo.GetAll().ToDictionary(p => p.id_post);

            var query = from o in orders
                        join e in employees on o.id_emp_fk equals e.id_employee
                        group o by new { e.name_employee, PostId = e.post_emp_fk } into g
                        select new EmployeeReportDto
                        {
                            Сотрудник = g.Key.name_employee,
                            Должность = posts.ContainsKey(g.Key.PostId) ? posts[g.Key.PostId].title_post : "Неизвестно",
                            Заказов = g.Count(),
                            Выручка = (decimal)g.Sum(x => x.totalAmount),
                            Средний_чек = g.Count() > 0 ? (decimal)g.Sum(x => x.totalAmount) / g.Count() : 0
                        };

            return query.OrderByDescending(x => x.Выручка).ToList();
        }

        public List<ClientReportDto> GetClientReport()
        {
            var clients = _clientRepo.GetAll().ToList();
            var orders = _orderRepo.GetAll().ToList();
            var regularClients = _regularClientRepo.GetAll().ToDictionary(rc => rc.id_reg_client_fk);

            var query = from c in clients
                        join o in orders on c.id_client equals o.id_cli_fk into clientOrders
                        select new ClientReportDto
                        {
                            Клиент = c.name_client,
                            Всего_заказов = clientOrders.Count(),
                            Общая_сумма = clientOrders.Sum(o => o.totalAmount),
                            Средний_чек = clientOrders.Count() > 0 ? clientOrders.Sum(o => o.totalAmount) / clientOrders.Count() : 0,
                            Статус = regularClients.ContainsKey(c.id_client) ? "Постоянный" : "Обычный",
                            Скидка = regularClients.ContainsKey(c.id_client) ? regularClients[c.id_client].discount_rate ?? 0 : 0
                        };

            return query.OrderByDescending(x => x.Общая_сумма).ToList();
        }

        public List<MenuReportDto> GetMenuReport(DateTime start, DateTime end)
        {
            var orders = _orderRepo.GetAll().Where(o => o.order_date >= start && o.order_date <= end).ToList();
            var orderDetails = _orderDetailsRepo.GetAll().ToList();
            var menu = _menuRepo.GetAll().ToList();
            var categories = _categoryRepo.GetAll().ToList();

            // Сначала считаем общую выручку
            var orderIds = orders.Select(o => o.id_order).ToHashSet();
            var relevantDetails = orderDetails.Where(od => orderIds.Contains(od.id_order_fk)).ToList();
            decimal totalRevenue = relevantDetails.Sum(od => od.subtotal.GetValueOrDefault());

            var query = from od in relevantDetails
                        join m in menu on od.id_menu_item_fk equals m.id_menu_item
                        join c in categories on m.id_category_fk equals c.id_category
                        group new { od, m } by new { c.title_category, m.item_name } into g
                        select new MenuReportDto
                        {
                            Категория = g.Key.title_category,
                            Позиция = g.Key.item_name,
                            Продано = (int)g.Sum(x => x.od.quantity),
                            Выручка = (decimal)g.Sum(x => x.od.subtotal.GetValueOrDefault()),
                            Доля_в_выручке = totalRevenue > 0 ?
                                (decimal)g.Sum(x => x.od.subtotal.GetValueOrDefault()) / totalRevenue * 100 : 0
                        };

            return query.OrderByDescending(x => x.Выручка).ToList();
        }

        public List<FinancialReportDto> GetFinancialReport(DateTime start, DateTime end)
        {
            var orders = _orderRepo.GetAll()
                .Where(o => o.order_date >= start && o.order_date <= end)
                .ToList();

            var query = from o in orders
                        group o by o.order_date.Date into g
                        select new FinancialReportDto
                        {
                            Дата = g.Key,
                            Заказов = g.Count(),
                            Выручка = (decimal)g.Sum(x => x.totalAmount),
                            Средний_чек = g.Count() > 0 ? (decimal)g.Sum(x => x.totalAmount) / g.Count() : 0
                        };

            return query.OrderBy(x => x.Дата).ToList();
        }
    }
}