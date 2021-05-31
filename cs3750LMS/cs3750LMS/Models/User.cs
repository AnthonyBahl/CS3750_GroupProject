using System;
using System.Collections.Generic;

#nullable disable

namespace cs3750LMS.Models
{
    public partial class User
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime Birthday { get; set; }
        public string Password { get; set; }
        public short AccountType { get; set; }
    }
}
