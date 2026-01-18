using Orders.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Orders.DTOs
{
    // ✅ DTO for Confirming Payment
    public class ConfirmPaymentDto
    {
        [Required(ErrorMessage = "Transaction ID is required")]
        [StringLength(100, ErrorMessage = "Transaction ID cannot exceed 100 characters")]
        public string TransactionId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Payment method is required")]
        public PaymentMethods PaymentMethod { get; set; }
        //[Required]
        //[Range(1, int.MaxValue)]
        //public int PaymentId { get; set; }

        //[Required]
        //public PaymentMethods PaymentMethod { get; set; }
    }
}
