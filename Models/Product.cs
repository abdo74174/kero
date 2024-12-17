using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class Product
    {

        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [Range(100, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        public double Price { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative.")]
        public int Stock { get; set; }
        public bool IsBestSeller { get; set; }

        [Range(0, 5)]
        public double Rating { get; set; }
        [ValidateNever]
        public string? ImageUrl { get; set; }
        public int CategoryId { get; set; }
        [ValidateNever]
        public Category Category { get; set; }
        public double DiscountPercentage { get; set; }

        public int SalesCount { get; set; }

    }
}