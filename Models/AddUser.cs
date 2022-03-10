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
        [StringLength(int.MaxValue, MinimumLength = 3, ErrorMessage = "Please enter a valid Usename!")]
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