using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

/*
* Author: Jackson
* Date: 21/05/2021
* Version: 1.0.0.0
* Objective: AuditLog Models to get and display audit log information
*/
namespace Device_Tracking_System.Models
{
    public class AuditLog
    {
        public string UserId { get; set; }
        public string Event { get; set; }
        public string EventTime { get; set; }
        [Required(ErrorMessage = "Please enter Start Date!")]
        [Display(Name = "Start Date: ")]
        public string StartDate { get; set; }
        [Required(ErrorMessage ="Please enter End Date!")]
        [Display(Name = "End Date: ")]
        public string EndDate { get; set; }
        public string MotherLot { get; set; }
        public string ChildLot { get; set; }
        public string Operation { get; set; }
    }
}