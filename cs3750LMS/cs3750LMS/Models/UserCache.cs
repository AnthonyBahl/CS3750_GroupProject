using System;
using System.Collections.Generic;

#nullable disable

namespace cs3750LMS.Models
{
    public partial class UserCache
    {
        [System.ComponentModel.DataAnnotations.Key]
        public string CacheId { get; set; }
        public string UserEmail { get; set; }
        public string CacheFirstName { get; set; }
        public string CacheLastName { get; set; }
        public DateTimeOffset ExpiresAtTime { get; set; }
        public long SlidingExpirationInSeconds { get; set;}
        public DateTimeOffset AbsoluteExpiration { get; set; }
 
    }
}
