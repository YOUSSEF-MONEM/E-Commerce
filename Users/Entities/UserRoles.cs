using System;
using Result_Pattern;
using Users.Constants;

namespace Users.Entities
{
    public class UserRoles
    {
        public int UserId { get; private set; }
        public Roles Role { get; private set; }

        // ✅ Navigation Property
        public  User User { get; private set; } 

        // ✅ Constructor خاص للـ EF Core
        private UserRoles() { }

        //  Internal Factory Method (للاستخدام من User فقط)
        internal static Result<UserRoles> CreateInternal(Roles role)
        {
            if (!Enum.IsDefined(typeof(Roles), role))
                return Result<UserRoles>.Failure("Invalid role");

            var userRole = new UserRoles
            {
                Role = role
            };

            return Result<UserRoles>.Success(userRole);
        }

        //  SetRole (internal للاستخدام من User فقط)
        internal Result SetRole(Roles role)
        {
            if (!Enum.IsDefined(typeof(Roles), role))
                return Result.Failure("Invalid role");

            Role = role;
            return Result.Success();
        }
    }
}