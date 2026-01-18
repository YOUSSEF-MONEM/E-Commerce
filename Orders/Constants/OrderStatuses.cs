using System;
using System.Collections.Generic;
using System.Text;

namespace Orders.Constants
{
    public enum OrderStatuses
    {
        Pending = 1,
        Confirmed ,
        Processing,
        Shipped,
        Delivered,
        Cancelled,
        Returned,
        Failed
    }
}

