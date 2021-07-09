using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace cs3750LMS.Models.validation
{
    public class NotificationValidation
    {
        
        [RegularExpression("^[0-9]*$")]
        public int NotificationID { get; set; }

        [Required]
        [RegularExpression("^[0-9]*$")]
        public int RecipientID { get; set; }

        [RegularExpression("^(?:[0-9]*|)$")]  //numbers on an empty string are accepted. 
        public int ReferenceID { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Maximum length of 50 characters")]
        public string NotificationType { get; set; }

        [Required]
        public string Message { get; set; }

        [Required]
        public DateTime DateCreated { get; set; }

        public DateTime DateViewed { get; set; }


    }
}
