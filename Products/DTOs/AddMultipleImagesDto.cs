using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Products.DTOs
{
    // ✅ DTO for Adding Multiple Images
    public class AddMultipleImagesDto
    {
        [Required(ErrorMessage = "At least one image URL is required")]
        [MinLength(1, ErrorMessage = "At least one image URL is required")]
        public List<string> ImageURLs { get; set; } = new List<string>();
    }
}
