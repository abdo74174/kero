using DataAccess.Data;
using DataAccess.Repository.IRepository;
using Microsoft.IdentityModel.Tokens;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IProductRepository repository;

        public ProductRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            this.dbContext = dbContext;

        }

        //    List<Product> Search (string query)
        //    {
        //        if (!query.IsNullOrEmpty())
        //        {
        //            var product = repository.GetAll([e => e.Category],
        //           (m => m.Name.Contains(query) || m.Description.Contains(query) ||
        //               m.Category.Name.Contains(query)));
        //            return (List<Product>)product;
        //        }

        //        return new List<Product>();
        //   }
    }
}
