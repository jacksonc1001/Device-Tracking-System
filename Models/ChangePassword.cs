using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Device_Tracking_System.Models
{
    public class ChangePassword
    {
        [Required(ErrorMessage = "Please Enter Your Old Password")]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }
        [Required(ErrorMessage = "Please Enter Your Password")]
        [DataType(DataType.Password)]
        [StringLength(18, ErrorMessage = "The password must be atleast 3 characters long", MinimumLength = 3)]
        public string NewPassword { get; set; }
    }
}