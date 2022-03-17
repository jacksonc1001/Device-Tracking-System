using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Device_Tracking_System.Models
{
    public class AddUser
    {
        [Required(ErrorMessage = "Please enter Username")]
        [StringLength(int.MaxValue, MinimumLength = 4, ErrorMessage = "Please enter at least 4 characters!")]
        [RegularExpression(@"^[a-zA-Z0-9]*$", ErrorMessage = "Please enter only alphabet and numbers!")]
        public string UserId { get; set; }
        public int RoleId { get; set; }
        public string Role { get; set; }
    }
    public class RoleInfo
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
    }
}