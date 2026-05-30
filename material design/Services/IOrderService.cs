using System.Collections.Generic;
using material_design.DTO;

namespace material_design.Services
{
    public interface IOrderService
    {
        List<Menu> GetMenuItems(int? categoryId, string search);
        List<CategoriesMenu> GetCategories();
        List<Clients> GetClients();
        List<Employees> GetEmployees();
        void CreateOrder(Orders order, List<OrderItemDto> items);
    }
}