using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }
        public int OrderId { get; set; }
        [Required]
        public string PaymentMethod { get; set; }
        [Required]
        public double Amount { get; set; }

        public DateTime PaymentDate { get; set; }

        public PaymentStatus Status { get; set; }
        [ValidateNever]
        public Order Order { get; set; }    
 

    }

    public enum PaymentStatus { 
        success = 0,
        cancelled = 1,
        pending = 2,
    }
}
