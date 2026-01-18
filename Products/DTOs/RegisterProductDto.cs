using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Products.DTOs
{
    // ✅ Updated RegisterProductDto to include optional image
    public class RegisterProductDto
    {
        [Required(ErrorMessage = "Product name is required")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Product name must be between 2 and 200 characters")]
        public string ProductName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Range(0, 100, ErrorMessage = "Discount percentage must be between 0 and 100")]
        public double DiscountPercentage { get; set; }

        [Required(ErrorMessage = "Quantity in stock is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative")]
        public int QuantityInStock { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string ProductDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category ID is required")]
        public int CategoryId { get; set; }



        // ✅ Optional image URL during product creation
        [Url(ErrorMessage = "Invalid URL format")]
        [StringLength(500, ErrorMessage = "Image URL cannot exceed 500 characters")]
        public string? ProductImageURL { get; set; }
    }
    //public class RegisterProductDto
    //{
    //    public string ProductName { get; set; } = string.Empty;
    //    public decimal Price { get;  set; }
    //    public double DiscountPercentage { get;  set; }
    //    public int QuantityInStock { get; set; }
    //    public string ProductImageURL { get; set; } = string.Empty;
    //    public string ProductDescription { get; set; } = string.Empty;
    //    public int CategoryId { get; set; }

    //}
}
