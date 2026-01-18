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
/*
 حالات PaymentStatus مهمة جدًا لأن عليها:

تأكيد الأوردر

الشحن

الإلغاء

الـ Refund

خلّينا نمشيها بمنطق حقيقي مستخدم في الأنظمة 👇

💳 الحالات الأساسية لـ PaymentStatus
1️⃣ Pending

🕒 قيد الانتظار

تم إنشاء عملية الدفع

لسه:

العميل مادفعش

أو البنك لسه بيرد

📌 أول حالة دايمًا

2️⃣ Authorized

🔐 تم الحجز

الفلوس اتحجزت على كارت العميل

لسه ما اتخصمتش

📌 مهم جدًا في:

Online payments

Gateways زي Stripe

3️⃣ Paid / Completed

✅ تم الدفع

الفلوس اتخصمت فعليًا

الدفع نجح 100%

📌 بعدها:

Order → Confirmed

4️⃣ Failed

❌ فشل الدفع

رصيد مش كفاية

بيانات كارت غلط

Timeout

5️⃣ Cancelled

🚫 ملغي

العميل لغى الدفع

أو السيستم لغاه

6️⃣ Refunded

🔄 تم الاسترجاع

الفلوس رجعت للعميل

كلي أو جزئي

7️⃣ PartiallyRefunded

↩️ استرجاع جزئي

جزء من المبلغ رجع

(مثلاً منتج واحد من الأوردر)

8️⃣ Expired

⏳ انتهت الصلاحية

العميل مأكملش الدفع

Session انتهت

🧠 Flow منطقي للحالات
Pending
   ↓
Authorized
   ↓
Paid

مسارات تانية:
Pending → Failed
Pending → Cancelled
Pending → Expired

Paid → Refunded
Paid → PartiallyRefunded

🧾 Enum احترافي
public enum PaymentStatus
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

⚠️ Domain Rules مهمة جدًا

❌ مينفعش:

Refunded قبل Paid

Paid → Pending

Failed → Paid

📌 لازم Methods تتحكم في التغيير.

🧩 مثال Domain Method
public Result MarkAsPaid()
{
    if (Status != PaymentStatus.Authorized &&
        Status != PaymentStatus.Pending)
        return Result.Failure("Payment cannot be marked as paid");

    Status = PaymentStatus.Paid;
    return Result.Success();
}

🟢 نصيحة زغلول المعمارية 🔥

OrderStatus ≠ PaymentStatus

بس:

PaymentStatus = Paid
→ OrderStatus = Confirmed


خلي الربط بينهم في Domain Service مش Controller

🎯 الخلاصة

✔ حالات واقعية
✔ Flow واضح
✔ Enum أنضف حل
✔ Domain يتحكم
*/