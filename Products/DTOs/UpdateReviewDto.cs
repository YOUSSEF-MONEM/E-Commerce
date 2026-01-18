using System;
using System.Collections.Generic;
using System.Text;

namespace Products.DTOs
{
    public class UpdateReviewDto
    {

        public double? Rating { get;  set; } // ⬅️ Nullable (0-10)
        public string? Comment { get;  set; } // ⬅️ Nullable

    }
}
