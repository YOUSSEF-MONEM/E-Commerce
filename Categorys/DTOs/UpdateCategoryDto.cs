using System;
using System.Collections.Generic;
using System.Text;

namespace Categories.DTOs
{
    public class UpdateCategoryDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryDescription { get; set; } = string.Empty;
        public int? ParentCategoryId { get; set; }
    }
}
