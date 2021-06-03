using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace cs3750LMS.Models
{
    public partial class State
    {
        [Key]
        public int StateID { get; set; }
        public string StateCode { get; set; }
        public string StateName { get; set; }
    }
}
