using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Orders.DTOs
{
    // ✅ DTO for Initiating Payment
    public class InitiatePaymentDto
    {
        [Required]
        public int OrderId { get; set; }
    }
}
