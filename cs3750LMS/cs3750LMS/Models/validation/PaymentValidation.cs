using System;
using System.ComponentModel.DataAnnotations;

namespace cs3750LMS.Models.validation
{
    public class PaymentValidation
    {
        [Required]
        public string cardHolderName { get; set; }

        [Required]
        [CreditCard(ErrorMessage = "Invalid credit card number.")]
        [MaxLength(19)]
        public string creditCardNumber { get; set; }

        [Required]
        [Range(1, 12, ErrorMessage = "Invalid Month")]
        public int expirationMonth { get; set; }

        int currentYear = (int)DateTime.Now.Year;
        [Required]
        [Range(1, 12, ErrorMessage = "Invalid Month")]
        public int expirationYear { get; set; }

        [Required]
        [MaxLength(4)]
        public int cvv { get; set; }

        [Required]
        [Range(.01, float.MaxValue, ErrorMessage = "Please enter a value bigger than {1}")]
        public int paymentAmount { get; set; }
    }
}