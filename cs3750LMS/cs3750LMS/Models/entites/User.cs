using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace cs3750LMS.Models
{
    public partial class User
    {
        [Key]
        public int UserId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime Birthday { get; set; }
        public string Password { get; set; }
        public short AccountType { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public int State { get; set; }
        public string Zip { get; set; }
        public string Phone { get; set; }
    }
}
