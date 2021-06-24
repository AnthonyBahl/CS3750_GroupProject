using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace cs3750LMS.Models.entites
{
    public class Transaction
    {
        [Key]
        public int transactionID { get; set; }

        public DateTime Date { get; set; }

        public int userID { get; set; }

        public float amount { get; set; }

        public string status { get; set; }
    }
}
