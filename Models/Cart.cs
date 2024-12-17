using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    [PrimaryKey("ProductId", "UserId")]
    public class Cart
    { 
       
        public int ProductId { get; set; }
        [ValidateNever]
        public Product Product { get; set; }
        [ForeignKey("ProductId")]
        [ValidateNever]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        [ValidateNever]
        public ApplicationUser applicationUser { get; set; }
        
        public int Count { get; set; }
        
    }

}
