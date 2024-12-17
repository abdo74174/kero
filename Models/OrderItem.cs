using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Models
{
    public class OrderItem
    {

        public int Id { get; set; }
        [ValidateNever]
        public int OrderId { get; set; }
        [ValidateNever]
        public int ProductId { get; set; }
        [Required]
        public int Quantity { get; set; }
        [Required]
        public double UnitPrice { get; set; }
        [ValidateNever]
        public Order Order { get; set; }
        [ValidateNever]
        public Product Product { get; set; }
    }
}
