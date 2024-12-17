using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Discount
    {

        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "Discount rate must be between 0 and 100.")]
        public double DiscountRate { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public int CouponCounter { get; set; }

        public DiscountStatus Status { get; set; }

    }
    public enum DiscountStatus
    {
        expires = 0,
        valid = 1,
        pending = 2
    }
}
