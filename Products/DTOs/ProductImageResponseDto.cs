using System;
using System.Collections.Generic;
using System.Text;

namespace Products.DTOs
{
    //  DTO for Product Image Response
    public class ProductImageResponseDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ImageURL { get; set; } = string.Empty;
    }
}
