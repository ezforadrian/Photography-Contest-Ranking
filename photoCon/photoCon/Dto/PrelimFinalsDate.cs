using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace photoCon.Dto
{
    public class PrelimFinalsDate
    {
        [Required(ErrorMessage = "Start date is required.")]
        [DataType(DataType.Date, ErrorMessage = "Invalid date format.")]
        [CustomValidation(typeof(PrelimFinalsDate), nameof(ValidateDate))]
        public string prelimStartDate { get; set; }

        public static ValidationResult ValidateDate(string date, ValidationContext context)
        {
            DateTime parsedDate;
            if (!DateTime.TryParseExact(date, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out parsedDate))
            {
                return new ValidationResult("Invalid date format. The date must be in the format yyyy-MM-dd.");
            }

            if (parsedDate <= DateTime.UtcNow.Date)
            {
                return new ValidationResult("The date cannot be less than or equal the current date.");
            }

            return ValidationResult.Success;
        }
    }
}
