using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Order
    {
        public int Id { get; set; }
        [ValidateNever]
        public string UserId { get; set; }
        [ValidateNever]
        public ApplicationUser User { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required]
        public double TotalAmount { get; set; }

        [ValidateNever]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [StringLength(500)]
        public string? ShippingAddress { get; set; }
        [ValidateNever]
        public int? DiscountId { get; set; }
        [ValidateNever] 
        public Discount Discount { get; set; }
        [ValidateNever]
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        [ValidateNever]
        public Payment Payment { get; set; }
        
    }

    public enum OrderStatus
    {
        Pending,
        Approved,
        Delivered,
        Canceled
    }

}
