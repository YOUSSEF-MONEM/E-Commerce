using MimeKit;
using MailKit.Net.Smtp;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Interfaces;

namespace Users.Services
{
    public class RegistrationNotificationWithEmail : IRegistrationNotification
    {
        public void NotifyUserRegistration(string userEmail, string userName)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Fast Order", "youssefabdelmeni3m@gmail.com"));
            message.To.Add(new MailboxAddress($"Ser {userName}", userEmail));
            message.Subject = "You have been successfully registered.";

            message.Body = new TextPart("plain")
            {
                Text = "Welcome to our Fast order service! Get your product as quickly as possible. Have a wonderful day!\n" +
                "Elmesh Moparmeg"
            };

            using (var client = new SmtpClient())
            {
                
                client.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                client.Authenticate("youssefabdelmeni3m@gmail.com", "zaiimqmlejidqwos");
                client.Send(message);
                client.Disconnect(true);
            }
        }
    }
}
