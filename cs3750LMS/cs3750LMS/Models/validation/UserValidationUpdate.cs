using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;
using cs3750LMS.Models.validation;
using Microsoft.AspNetCore.Http;

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

        public IFormFile ProfileImage { get; set; }

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
        [RegularExpression(@"^[\+]?[(]?[0-9]{3}[)]?[-\s\.]?[0-9]{3}[-\s\.]?[0-9]{4,6}$", ErrorMessage = "Invalid Phone Number")]
        public string Phone { get; set; }

        public List<Link> UserLinks { get; set; }

        //[RegularExpression(@"^[http]", ErrorMessage = "Invalid url")]
        public string gitHubLink { get; set; }

        //[RegularExpression(@"^(http\:\/\/|https\:\/\/)?([a - z0 - 9][a - z0 - 9\-]*\.)+[a-z0-9] [a-z0-9\-]*$@i", ErrorMessage = "Invalid url")]
        public string TwitterLink { get; set; }

        //[RegularExpression(@"^(http\:\/\/|https\:\/\/)?([a - z0 - 9][a - z0 - 9\-]*\.)+[a-z0-9] [a-z0-9\-]*$@i", ErrorMessage = "Invalid url")]
        public string InstagramLink { get; set; }

        //[RegularExpression(@"^(http\:\/\/|https\:\/\/)?([a - z0 - 9][a - z0 - 9\-]*\.)+[a-z0-9] [a-z0-9\-]*$@i", ErrorMessage = "Invalid url")]
        public string FacebookLink { get; set; }

        //[RegularExpression(@"^(http\:\/\/|https\:\/\/)?([a - z0 - 9][a - z0 - 9\-]*\.)+[a-z0-9] [a-z0-9\-]*$@i", ErrorMessage = "Invalid url")]
        public string LinkedInLink { get; set; }

        public string Bio { get; set; }
    }
}
