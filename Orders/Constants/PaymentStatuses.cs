using System;
using System.Collections.Generic;
using System.Text;

namespace Orders.Constants
{
    public enum PaymentStatuses
    {
        Pending = 1,
        Authorized,
        Paid,
        Failed,
        Cancelled,
        Refunded,
        PartiallyRefunded,
        Expired
    }
}
