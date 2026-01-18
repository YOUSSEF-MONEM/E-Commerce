using System;
using System.Collections.Generic;
using System.Text;

namespace Users.DTOs
{
    public class UpdateUserDto
    {
        public int Id { get; set; }
        public string? FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; } = string.Empty;
        public string? Address { get; set; } = string.Empty;
        public DateOnly? BirthDate { get; set; }
        public string? Email { get; set; } = string.Empty;
        //public string? Password { get; set; } = string.Empty;////
        public string? PhoneNumber { get; set; } = string.Empty;


    }
}
