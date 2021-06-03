using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace cs3750LMS.Models
{
    public partial class Link
    {
        [Key]
        public int LinkID  { get; set; }
        public int UserID { get; set; }
        public string Contents { get; set; }
    }
}
