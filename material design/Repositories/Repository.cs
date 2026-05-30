using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace material_design.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly cafe_barEntities _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(cafe_barEntities context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }
        public cafe_barEntities GetContext() => _context;
        public IEnumerable<T> GetAll() => _dbSet.ToList();

        public T GetById(int id) => _dbSet.Find(id);

        public void Add(T entity) => _dbSet.Add(entity);

        public void Update(T entity) => _context.Entry(entity).State = EntityState.Modified;

        public void Delete(T entity) => _dbSet.Remove(entity);

        public void Save() => _context.SaveChanges();
        public IQueryable<T> GetAllQuery()
        {
            return _dbSet;
        }
    }
}