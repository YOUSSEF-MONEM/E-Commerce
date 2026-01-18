using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Orders.DTOs
{
    public class CreateOrderDto
    {


        [Required]
        [StringLength(500, MinimumLength = 10)]
        public string ShippingAddress { get; set; } = string.Empty;



        [Required]
        [MinLength(1, ErrorMessage = "At least one item is required")]
        public List<AddOrderItemDto> OrderItems { get; set; } = new();
    }
}
