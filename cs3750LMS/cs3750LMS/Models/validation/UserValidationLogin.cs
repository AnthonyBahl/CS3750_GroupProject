using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using cs3750LMS.Models.validation;

#nullable disable

namespace cs3750LMS.Models
{
    public partial class UserValidationLogin
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }


        [Required]
        [MaxLength(255, ErrorMessage = "Cannot exceed 255 characters long")] 
        public string Password { get; set; }

    }

}
