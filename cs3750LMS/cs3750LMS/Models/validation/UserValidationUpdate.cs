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

        //TODO: Not sure if [DataType(DataType.text)] should be added to the model to match the database. 

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
        //[DateMinimumAge(16,ErrorMessage = "Must be at least {1} years of age")] 
        //TODO: Once I can figure out how to connect the DLL MinimumAgeAttribute this should work. 
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

        [StringLength(30, ErrorMessage = "Maximum length of 100 characters")]
        public string Zip { get; set; }

        [StringLength(30, ErrorMessage = "Maximum length of 100 characters")]
        public string Phone { get; set; }

        public List<Link> UserLinks { get; set; }
    }
}
