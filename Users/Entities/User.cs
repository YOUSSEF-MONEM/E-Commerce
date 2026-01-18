using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Users.Constants;
using Result_Pattern;
using Users.DTOs;

namespace Users.Entities
{
    public class User
    {
        public int Id { get; private set; }
        public string FirstName { get; private set; } = string.Empty;
        public string LastName { get; private set; } = string.Empty;
        public string Address { get; private set; } = string.Empty;
        public DateOnly? BirthDate { get; private set; }
        public int Age
        {
            get
            {
                if (!BirthDate.HasValue)
                    return 0;
                var today = DateOnly.FromDateTime(DateTime.Today);
                var age = today.Year - BirthDate.Value.Year;
                if (BirthDate.Value > today.AddYears(-age)) //هو عيد ميلاد الشخص عدى السنة دي ولا لسه؟
                    age--;
                return age;
            }
        }
        //عضو منذلعدد السنوات
        //طب لو عايز اعرفها بالسنوات والشهور اعمل ايه؟ هنشوفها دي
        public int MemberSince
        {
            get
            {
                return DateTime.UtcNow.Year - CreatedAt.Year;
            }

        }
        public string PhoneNumber { get; private set; } = string.Empty; //unique
        public string Email { get; private set; } = string.Empty;  //unique
        public string Password { get; private set; } = string.Empty; 
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; private set; }

        // ✅ Navigation Properties
        public ICollection<UserRoles> Roles { get; private set; } = new List<UserRoles>();

        public List<RefreshToken>? RefreshTokens { get; set; }

        private User() { }

        public static Result<User> Create(
            string firstName,
            string lastName,
            string address,
            DateOnly birthDate,
            string email,
            string password,
            string phoneNumber,
            Roles role = Constants.Roles.User)
        {
            var user = new User();

            // Apply all setters
            var results = new[]
            {
                user.SetName(firstName, lastName),
                user.SetAddress(address),
                user.SetBirthDate(birthDate),
                user.SetEmail(email),
                user.SetPassword(password),
                user.SetPhoneNumber(phoneNumber),
                user.AddRole(role)
            };

            foreach (var result in results)
            {
                if (!result.IsSuccess)
                    return Result<User>.Failure(result.Error);
            }

            return Result<User>.Success(user);
        }

        public static Result<User> Update(User user, UpdateUserDto userDto)
        {
            if(!string.IsNullOrWhiteSpace(userDto.FirstName))
            {
                var result = user.SetFirstName(userDto.FirstName);
                if (!result.IsSuccess)
                    return Result<User>.Failure(result.Error ?? string.Empty);
            }

            if(!string.IsNullOrWhiteSpace(userDto.LastName))
            {
                var result = user.SetLastName(userDto.LastName);
                if (!result.IsSuccess)
                    return Result<User>.Failure(result.Error ?? string.Empty);
            }

            if(!string.IsNullOrWhiteSpace(userDto.Address))
            {
                var result = user.SetAddress(userDto.Address);
                if (!result.IsSuccess)
                    return Result<User>.Failure(result.Error ?? string.Empty);
            }

            if(userDto.BirthDate.HasValue)
            {
                var result = user.SetBirthDate(userDto.BirthDate.Value);
                if (!result.IsSuccess)
                    return Result<User>.Failure(result.Error ?? string.Empty);
            }

            if(!string.IsNullOrWhiteSpace(userDto.Email))
            {
                var result = user.SetEmail(userDto.Email);
                if (!result.IsSuccess)
                    return Result<User>.Failure(result.Error ?? string.Empty);
            }

            //if (!string.IsNullOrWhiteSpace(userDto.Password))
            //{
            //    var result = user.SetPassword(userDto.Password);
            //    if (!result.IsSuccess)
            //        return Result<User>.Failure(result.Error ?? string.Empty);
            //}

            if (!string.IsNullOrWhiteSpace(userDto.PhoneNumber))
            {
                var result = user.SetPhoneNumber(userDto.PhoneNumber);
                if (!result.IsSuccess)
                    return Result<User>.Failure(result.Error ?? string.Empty);
            }

            return Result<User>.Success(user);
        }




        public Result SetPhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return Result.Failure("PhoneNumber is required");

            // Remove spaces and dashes
            phone = phone.Replace(" ", "").Replace("-", "");

            if (phone.Length != 11)
                return Result.Failure("PhoneNumber length must equal 11");

            if (!phone.All(char.IsDigit))
                return Result.Failure("PhoneNumber must contain only digits");

            PhoneNumber = phone;
            MarkAsUpdated();
            return Result.Success();
        }

        // ✅ Method لإضافة Role
        public Result AddRole(Roles role)
        {
            // Check if role already exists
            if (Roles.Any(r => r.Role == role))
                return Result.Failure("User already has this role");

            var userRole = UserRoles.CreateInternal(role);
            if (!userRole.IsSuccess)
                return Result.Failure(userRole.Error ?? string.Empty);

            Roles.Add(userRole.Value!);
            MarkAsUpdated();
            return Result.Success();
        }

        // ✅ Method لحذف Role
        public Result RemoveRole(Roles role)
        {
            var userRole = Roles.FirstOrDefault(r => r.Role == role);

            if (userRole == null)
                return Result.Failure("User does not have this role");

            // ⚠️ Prevent removing last role
            if (Roles.Count == 1)
                return Result.Failure("User must have at least one role");

            Roles.Remove(userRole);
            MarkAsUpdated();
            return Result.Success();
        }

        // ✅ Check if user has role
        public bool HasRole(Roles role)
        {
            return Roles.Any(r => r.Role == role);
        }

        // ✅ Check if user has any of the roles
        public bool HasAnyRole(params Roles[] roles)
        {
            return Roles.Any(r => roles.Contains(r.Role));
        }

        // ✅ SetName بدون Exception
        public Result SetName(string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
                return Result.Failure("First Name and Last Name are required");

            FirstName = firstName.Trim();
            LastName = lastName.Trim();
            MarkAsUpdated();
            return Result.Success();
        }

        //عشان الابديت افرض عايز يعمل ابديت للاسم الاول فقط او الاسم الاخير فقط
        public Result SetFirstName(string firstName)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                return Result.Failure("FirstName is Required");
            FirstName = firstName.Trim();
            MarkAsUpdated();
            return Result.Success();
        }
        public Result SetLastName(string lastName)
        {
            if (string.IsNullOrWhiteSpace(lastName))
                return Result.Failure("LastName is Required");
            LastName = lastName.Trim();
            MarkAsUpdated();
            return Result.Success();
        }


        // ✅ SetAddress بدون Exception
        public Result SetAddress(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                return Result.Failure("Address is required");

            if (address.Length < 5)
                return Result.Failure("Address must be at least 5 characters");

            if (!address.Contains("-"))
                return Result.Failure("Address must be separated by -");

            Address = address.Trim();
            MarkAsUpdated();
            return Result.Success();
        }

        // ✅ SetBirthDate بدون Exception
        public Result SetBirthDate(DateOnly? birthDate)
        {
            const int minAge = 20;
            const int maxAge = 60;
            var today = DateOnly.FromDateTime(DateTime.Today);
            var minDate = today.AddYears(-maxAge);
            var maxDate = today.AddYears(-minAge);

            if (birthDate < minDate || birthDate > maxDate)
                return Result.Failure($"User must be between {minAge} and {maxAge} years old");

            BirthDate = birthDate;
            MarkAsUpdated();
            return Result.Success();
        }

        // ✅ SetEmail بدون Exception
        public Result SetEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Result.Failure("Email is required");

            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                return Result.Failure("Invalid email format");

            Email = email.Trim().ToLower();
            MarkAsUpdated();
            return Result.Success();
        }

        // ✅ SetPassword بدون Exception
        public Result SetPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return Result.Failure("Password is required");

            if (password.Length < 8)
                return Result.Failure("Password must be at least 8 characters");

            Password = BCrypt.Net.BCrypt.HashPassword(password);
            MarkAsUpdated();
            return Result.Success();
        }

        // ✅ VerifyPassword التحقق من كلمة المرور
        public bool VerifyPassword(string password)
        {
            return BCrypt.Net.BCrypt.Verify(password, Password);
        }

        // ✅ ChangePassword مع Result Pattern
        public Result ChangePassword(string currentPassword, string newPassword)
        {
            if (!VerifyPassword(currentPassword))
                return Result.Failure("Current password is incorrect");

            return SetPassword(newPassword);
        }

        private void MarkAsUpdated()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}


//// ✅ Factory Method بدون PhoneNumber (للحالات اللي مش محتاجة phone)
//public static Result<User> Create(
//    string firstName,
//    string lastName,
//    string address,
//    DateOnly birthDate,
//    string email,
//    string password)
//{
//    var user = new User();

//    var nameResult = user.SetName(firstName, lastName);
//    if (!nameResult.IsSuccess)
//        return Result<User>.Failure(nameResult.Error ?? string.Empty);

//    var addressResult = user.SetAddress(address);
//    if (!addressResult.IsSuccess)
//        return Result<User>.Failure(addressResult.Error ?? string.Empty);

//    var birthDateResult = user.SetBirthDate(birthDate);
//    if (!birthDateResult.IsSuccess)
//        return Result<User>.Failure(birthDateResult.Error ?? string.Empty);

//    var emailResult = user.SetEmail(email);
//    if (!emailResult.IsSuccess)
//        return Result<User>.Failure(emailResult.Error ?? string.Empty);

//    var passwordResult = user.SetPassword(password);
//    if (!passwordResult.IsSuccess)
//        return Result<User>.Failure(passwordResult.Error ?? string.Empty);

//    return Result<User>.Success(user);
//}

//// ✅ Factory Method مع Role (افتراضي = User)
//public static Result<User> Create(
//    string firstName,
//    string lastName,
//    string address,
//    DateOnly birthDate,
//    string email,
//    string password,
//    string phoneNumber,
//    Roles role = Constants.Roles.User) // ✅ Default role
//{
//    var userResult = Create(firstName, lastName, address, birthDate, email, password);

//    if (!userResult.IsSuccess)
//        return userResult;

//    var user = userResult.Value;

//    // Add phone


//    // Add role
//    var roleResult = user.AddRole(role);
//    if (!roleResult.IsSuccess)
//        return Result<User>.Failure(roleResult.Error ?? string.Empty);

//    return Result<User>.Success(user);
//}