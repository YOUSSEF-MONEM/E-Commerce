using System;
using System.Collections.Generic;
using System.Text;

namespace Users.Interfaces
{
    public interface IRegistrationNotification
    {
        void NotifyUserRegistration(string userEmail, string userName);

    }
}
