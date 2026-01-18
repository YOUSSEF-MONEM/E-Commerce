using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Carts.DTOs
{
    public class UpdateQuantityDto
    {
        [Required]
        [Range(1, 1000)]
        public int Quantity { get; set; }
    }
}
