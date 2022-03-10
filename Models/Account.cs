using System.ComponentModel.DataAnnotations;

/*
* Author: Jackson
* Date: 08/03/2022
* Version: 1.0.0.0
* Objective: Account Models to get user account from view
*/
namespace Device_Tracking_System.Models
{
    public class Account
    {
        [Required(ErrorMessage = "Please enter Username")]
        [StringLength(int.MaxValue, MinimumLength = 3, ErrorMessage ="Please enter a valid Usename!")]
        public string Username { get; set; }
        [Required(ErrorMessage = "Please enter Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public string Roles { get; set; }
        public int RoleId { get; set; }
    }
}