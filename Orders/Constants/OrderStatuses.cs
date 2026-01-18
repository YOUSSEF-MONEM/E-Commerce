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

/*
 🧾 أشهر حالات الـ Order (Order Statuses)
1️⃣ Pending

🕒 قيد الانتظار

الأوردر اتعمل

لسه:

ما اتدفعش

أو لسه بيتأكد

📌 أول حالة دايمًا

2️⃣ Confirmed

✅ تم التأكيد

تم التأكيد من السيستم

أو الدفع نجح

3️⃣ Processing

⚙️ قيد التنفيذ

الأوردر بيتجهز

بيتحضّر للشحن

4️⃣ Shipped

🚚 تم الشحن

خرج من المخزن

في الطريق للعميل

5️⃣ Delivered

📦 تم التسليم

وصل للعميل

الأوردر انتهى بنجاح

6️⃣ Cancelled

❌ ملغي

اتلغى:

من العميل

أو من السيستم

غالبًا قبل الشحن

7️⃣ Returned

🔄 مرتجع

العميل رجّع الأوردر

بعد الاستلام

8️⃣ Failed

⚠️ فشل

مشكلة في:

الدفع

المخزون

السيستم

🧠 ترتيب منطقي للحالات (Flow)
Pending
   ↓
Confirmed
   ↓
Processing
   ↓
Shipped
   ↓
Delivered

حالات جانبية:
Pending → Cancelled
Confirmed → Cancelled
Delivered → Returned
Pending → Failed

🎯 تصميم احترافي (Enum)
public enum OrderStatus
{
    Pending = 1,
    Confirmed,
    Processing,
    Shipped,
    Delivered,
    Cancelled,
    Returned,
    Failed
}

⚠️ نقطة مهمة جدًا (Domain Rules)

❌ مينفعش:

Delivered → Processing

Cancelled → Shipped

📌 لازم تتحكم في الانتقالات جوه الـ Domain.

🧩 مثال Domain Logic
public Result Ship()
{
    if (Status != OrderStatus.Processing)
        return Result.Failure("Order cannot be shipped");

    Status = OrderStatus.Shipped;
    return Result.Success();
}

🟢 نصيحة زغلول المعمارية 🧠

خلي الـ Status في Domain

استخدم:

Enum

Methods تتحكم في الانتقال

متغيرش الـ Status مباشرة من الـ Controller ❌
 */