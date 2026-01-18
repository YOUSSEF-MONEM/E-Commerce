using System;
using System.Collections.Generic;
using System.Text;

namespace Categories.DTOs
{
    public class RegisterCategoryDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryDescription { get; set; } = string.Empty;
        public int? ParentCategoryId { get; set; }
    }
}
