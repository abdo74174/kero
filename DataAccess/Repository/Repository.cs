using DataAccess.Data;
using DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;



namespace DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            dbSet = _context.Set<T>();
        }
        public void Add(T entity)
        {
            dbSet.Add(entity);
            Commit();
        }
        public void Commit()
        {
            _context.SaveChanges();
        }

        public IEnumerable<T> GetAll(Expression<Func<T, object>>[]? includeProp = null, Expression<Func<T, bool>>? expression = null, bool tracked = true)
        {
            IQueryable<T> query = dbSet;

            if (includeProp != null)
            {
                foreach (var item in includeProp)
                {
                    query = query.Include(item);
                }
            }

            if (expression != null)
            {
                query = query.Where(expression);
            }

            if (!tracked)
            {
                query = query.AsNoTracking();
            }

            return query.ToList();
        }

        public T? GetOne(Expression<Func<T, object>>[]? includeProp = null, Expression<Func<T, bool>>? expression = null, bool tracked = true)
        {
            return GetAll(includeProp, expression, tracked).FirstOrDefault();
        }

        public void Update(T entity)
        {
            dbSet.Update(entity);
            Commit();
        }

        public void Delete(T entity)
        {
            dbSet.Remove(entity);
            Commit();
        }
    }
}
