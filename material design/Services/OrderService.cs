using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using material_design.DTO;
using material_design.Repositories;

namespace material_design.Services
{
    public class OrderService : IOrderService
    {
        private readonly IRepository<Menu> _menuRepo;
        private readonly IRepository<CategoriesMenu> _categoryRepo;
        private readonly IRepository<Clients> _clientRepo;
        private readonly IRepository<Orders> _orderRepo;
        private readonly IRepository<Order_details> _orderDetailsRepo;
        private readonly IRepository<Employees> _employeeRepo;
        private readonly IRepository<Post> _postRepo;

        public OrderService(
            IRepository<Menu> menuRepo,
            IRepository<CategoriesMenu> categoryRepo,
            IRepository<Clients> clientRepo,
            IRepository<Orders> orderRepo,
            IRepository<Order_details> orderDetailsRepo,
            IRepository<Employees> employeeRepo,
            IRepository<Post> postRepo)
        {
            _menuRepo = menuRepo;
            _categoryRepo = categoryRepo;
            _clientRepo = clientRepo;
            _orderRepo = orderRepo;
            _orderDetailsRepo = orderDetailsRepo;
            _employeeRepo = employeeRepo;
            _postRepo = postRepo;
        }

        public List<Menu> GetMenuItems(int? categoryId, string search)
        {
            try
            {
                if (_menuRepo == null)
                    throw new InvalidOperationException("_menuRepo не инициализирован");

                var query = _menuRepo.GetAllQuery();
                if (query == null)
                    throw new InvalidOperationException("GetAllQuery() вернул null");

                query = query.Include(m => m.CategoriesMenu);

                if (categoryId.HasValue && categoryId > 0)
                    query = query.Where(m => m.id_category_fk == categoryId.Value);

                if (!string.IsNullOrEmpty(search) && search != "Поиск...")
                    query = query.Where(m => m.item_name.Contains(search));

                var result = query.ToList();
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в GetMenuItems: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                throw new Exception($"Ошибка при загрузке меню: {ex.Message}", ex);
            }
        }

        public List<CategoriesMenu> GetCategories() => _categoryRepo.GetAll().ToList();

        public List<Clients> GetClients() => _clientRepo.GetAll().ToList();

        public List<Employees> GetEmployees()
        {
            var employees = _employeeRepo.GetAllQuery()
                .Include(e => e.Post)
                .ToList();
            return employees.Where(e => e.Post != null && (e.Post.title_post == "Официант" || e.Post.title_post == "Бармен")).ToList();
        }

        public void CreateOrder(Orders order, List<OrderItemDto> items)
        {
            _orderRepo.Add(order);
            _orderRepo.Save(); 

            foreach (var item in items)
            {
                var detail = new Order_details
                {
                    id_order_fk = order.id_order,
                    id_menu_item_fk = item.MenuItemId,
                    quantity = (short)item.Quantity,
                    unit_price = item.UnitPrice
                };
                _orderDetailsRepo.Add(detail);
            }
            _orderDetailsRepo.Save();
        }
    }
}
