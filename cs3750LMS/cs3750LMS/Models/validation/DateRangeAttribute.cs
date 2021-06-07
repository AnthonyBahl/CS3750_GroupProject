using System;
using System.ComponentModel.DataAnnotations;

namespace cs3750LMS.Models.validation
{
    /// <summary>
    /// This method overrides the Range Attribute Data annotation. It takes the minimum date and the current date - 16 years and compares them. 
    /// </summary>
    public class DateRangeAttribute : RangeAttribute
    {
        public DateRangeAttribute(string minimumValue) :base(typeof(DateTime), minimumValue, DateTime.Now.AddYears(-16).ToShortDateString())
        {

        }
    }
}
