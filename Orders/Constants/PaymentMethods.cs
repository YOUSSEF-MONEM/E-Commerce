using System;
using System.Collections.Generic;
using System.Text;

namespace Orders.Constants
{
    public enum PaymentMethods
    {
        Cash = 1,
        CreditCard,
        DebitCard,
        Wallet,
        BankTransfer,
        MobilePayment,
        PayPal,
        Installments,
        Crypto
    }
}
