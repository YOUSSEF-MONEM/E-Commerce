using Users.Constants;

namespace E_Commerce2.Authorization
{
 
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class CheckPermissionAttribute : Attribute
    {
        public Roles Role { get; }

        public CheckPermissionAttribute(Roles role)
        {
            Role = role;
        }
    }
}
