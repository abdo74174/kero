using DataAccess.Data;
using DataAccess.Repository.IRepository;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class DiscountRepository : Repository<Discount>, IDiscountRepository
    {
        private readonly ApplicationDbContext context;

        public DiscountRepository(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }
    }
}
