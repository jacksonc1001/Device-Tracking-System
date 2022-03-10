using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Device_Tracking_System.Models
{
    public class DeviceValidation
    {
        [Required(ErrorMessage = "Please scan Device ID!")]
        [Display(Name = "Device ID:")]
        public string DeviceId { get; set; }
        public string BinResult { get; set; }
        [RegularExpression(@"^[a-zA-Z0-9_]*$", ErrorMessage = "Please remove the whitespace and leave the field blank if not required to update!")]
        public string BinCode { get; set; }
        [RegularExpression(@"^[a-zA-Z0-9_]*$", ErrorMessage = "Please remove the whitespace and leave the field blank if not required to update!")]
        public string BinDesc { get; set; }
        public string ValidateResult { get; set; }
    }
}