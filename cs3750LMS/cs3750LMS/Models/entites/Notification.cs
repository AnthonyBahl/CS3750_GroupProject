using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace cs3750LMS.Models.entites
{
    public class Notification
    {
        [Key]
        public int NotificationID { get; set; }

        public int RecipientID { get; set; }

        public int ReferenceID { get; set; }

        public string NotificationType { get; set; }

        public string Message { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime DateViewed { get; set; }



    }
}
