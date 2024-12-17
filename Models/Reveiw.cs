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
    public class Review
    {
        public int Id { get; set; }
        public int ProductId { get; set; } 
        public string UserId { get; set; } 

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; } 

        [StringLength(1000)]
        public string Comment { get; set; } 

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [ValidateNever]
        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(UserId))]    
        public ApplicationUser User { get; set; }
    }

}
