using System.Collections.Generic;

namespace material_design.Services
{
    public interface IMenuService
    {
        List<Menu> GetAllMenuItems();
        List<CategoriesMenu> GetAllCategories();
        void AddMenuItem(Menu menu);
        void UpdateMenuItem(Menu menu);
        void DeleteMenuItem(int id);
    }
}