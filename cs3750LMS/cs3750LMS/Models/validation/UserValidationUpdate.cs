using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;

namespace cs3750LMS.Models
{
    public class UserValidationUpdate
    {
        public int UserId { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Maximum length of 100 characters")]
        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = "Only alphabetical characters are allowed")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Maximum length of 100 characters")]
        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = "Only alphabetical characters are allowed")]
        public string LastName { get; set; }

        [Required]
        [DateRange("01/01/1921", ErrorMessage = "Must be at least 16 years old")] //age range is between 100 years old and 16 years old.  The override for this is in the DateRangeAttribute.cs     
        [DataType(DataType.Date)]  //specifies only the Date, not the Time. 
        public DateTime Birthday { get; set; }

        [Required]
        [MaxLength(255, ErrorMessage = "Password must not exceed 255 characters long")] //this was based of the entity validation. 
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "The passwords do not match")]
        [Display(Name = "Password Confirmation")]
        public string ConfirmPassword { get; set; }

        [Required]
        public short AccountType { get; set; }

        public byte[] ProfileImage { get; set; }

        [StringLength(100, ErrorMessage = "Maximum length of 100 characters")]
        public string Address1 { get; set; }

        [StringLength(100, ErrorMessage = "Maximum length of 100 characters")]
        public string Address2 { get; set; }

        [StringLength(100, ErrorMessage = "Maximum length of 100 characters")]
        public string City { get; set; }
        public int State { get; set; }

        [StringLength(30, ErrorMessage = "Maximum length of 30 characters")]
        public string Zip { get; set; }

        [StringLength(30, ErrorMessage = "Maximum length of 30 characters")]
        public string Phone { get; set; }

        public List<Link> UserLinks { get; set; }
    }
}
