using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Repository.IRepository;
using DataAccess.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DataAccess.Repository
{
    public class CartRepository : Repository<Cart>, ICartRepository
    {
        private readonly ApplicationDbContext dbContext;


        public CartRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            this.dbContext = dbContext;

        }
        
    }
}
