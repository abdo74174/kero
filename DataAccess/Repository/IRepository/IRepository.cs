using System.Linq.Expressions;

namespace DataAccess.Repository.IRepository
{

    public interface IRepository<T> where T : class
    {
      
        IEnumerable<T> GetAll(
            Expression<Func<T, object>>?[] includeProp = null, Expression<Func<T, bool>>? expression = null, bool tracked = true
        );

       
        T? GetOne(Expression<Func<T, object>>?[] includeProp = null, Expression<Func<T, bool>>? expression = null, bool tracked = true);

        
        void Add(T entity);

        
        void Update(T entity);

        
        void Delete(T entity);

       
        void Commit();
    }
}
