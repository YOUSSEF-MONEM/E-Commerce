using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Products.DTOs
{
    public class UpdateProductDto
    {

        [StringLength(200, MinimumLength = 2)]
        public string? ProductName { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal? Price { get; set; }

        [Range(0, 100)]
        public double? DiscountPercentage { get; set; }

        [Range(0, int.MaxValue)]
        public int? QuantityInStock { get; set; }

        [StringLength(1000)]
        public string? ProductDescription { get; set; }

        public int? CategoryId { get; set; }

        //public int Id { get; set; }
        //public string? ProductName { get; set; } = string.Empty;
        //public decimal? Price { get;  set; } 
        //public double? DiscountPercentage { get;  set; }
        //public int? QuantityInStock { get;  set; }
        //public string? ProductImageURL { get; set; } = string.Empty;
        //public string? ProductDescription { get; set; } = string.Empty;
        //public int? CategoryId { get;  set; }
    }
}
