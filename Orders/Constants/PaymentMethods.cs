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
/*
 💳 أشهر أنواع PaymentMethod
1️⃣ CreditCard

💳 كارت بنكي

Visa

MasterCard

Meeza

📌 Online Payments

2️⃣ DebitCard

🏧 كارت خصم مباشر

نفس فكرة الكريدت

الفلوس بتتخصم فورًا

3️⃣ Cash

💵 كاش

عند الاستلام (COD)

في المحل

4️⃣ BankTransfer

🏦 تحويل بنكي

حساب → حساب

محتاج تأكيد يدوي

5️⃣ Wallet

📱 محفظة إلكترونية

Vodafone Cash

Orange Money

Etisalat Cash

6️⃣ MobilePayment

📲 دفع بالموبايل

Apple Pay

Google Pay

7️⃣ PayPal

🌍 PayPal

عالمي

Online

8️⃣ Crypto

🪙 عملات رقمية

Bitcoin

Ethereum

(نادراً في الأنظمة العادية)

9️⃣ Installments

📆 تقسيط

Valu

Sympl

Shahry

🧠 Enum احترافي
public enum PaymentMethod
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

🎯 استخدام واقعي
مثال:
{
  "paymentMethod": "Wallet"
}


أو (أفضل في DB):

{
  "paymentMethod": 4
}

⚠️ ملاحظة مهمة معماريًا

مش كل PaymentMethod:

يحتاج Payment Gateway

أو Refund

مثال:

الطريقة	Refund؟
Cash	❌
Wallet	✔
Card	✔
BankTransfer	يدوي

📌 خلي ده في الـ Business Rules.

🟢 نصيحة زغلول 🔥

استخدم Enum

خزّنه في DB كـ int

اعمل Mapping في الـ API (DTO → Enum)

🧩 لو حابب نطوّرها أكتر

نعمل Payment Aggregate

أو نربط PaymentMethod بـ PaymentStatus

أو نضيف قيود حسب الطريقة
 */