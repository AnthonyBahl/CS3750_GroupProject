using System;
using System.ComponentModel.DataAnnotations;

namespace cs3750LMS.Models.validation
{
    public class DateRangeAttribute : RangeAttribute
    {
        public DateRangeAttribute(string minimumValue)
            :base(typeof(DateTime), minimumValue, DateTime.Now.AddYears(-16).ToShortDateString())
        {

        }
    }
}
