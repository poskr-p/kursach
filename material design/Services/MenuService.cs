using System.Collections.Generic;
using System.Linq;
using material_design.Repositories;

namespace material_design.Services
{
    public class MenuService : IMenuService
    {
        private readonly IRepository<Menu> _menuRepo;
        private readonly IRepository<CategoriesMenu> _categoryRepo;

        public MenuService(IRepository<Menu> menuRepo, IRepository<CategoriesMenu> categoryRepo)
        {
            _menuRepo = menuRepo;
            _categoryRepo = categoryRepo;
        }

        public List<Menu> GetAllMenuItems()
        {
            // Подгружаем категории для отображения
            var menus = _menuRepo.GetAll().ToList();
            var categories = _categoryRepo.GetAll().ToDictionary(c => c.id_category);
            foreach (var m in menus)
                if (categories.ContainsKey(m.id_category_fk))
                    m.CategoriesMenu = categories[m.id_category_fk];
            return menus;
        }

        public List<CategoriesMenu> GetAllCategories() => _categoryRepo.GetAll().ToList();

        public void AddMenuItem(Menu menu)
        {
            _menuRepo.Add(menu);
            _menuRepo.Save();
        }

        public void UpdateMenuItem(Menu menu)
        {
            _menuRepo.Update(menu);
            _menuRepo.Save();
        }

        public void DeleteMenuItem(int id)
        {
            var item = _menuRepo.GetById(id);
            if (item != null)
            {
                _menuRepo.Delete(item);
                _menuRepo.Save();
            }
        }
    }
}