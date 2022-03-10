using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

/*
* Author: Jackson
* Date: 21/05/2021
* Version: 1.0.0.0
* Objective: LotValidation Model to get lot information
*/
namespace Device_Tracking_System.Models
{
    public class LotValidation
    {
        [Required(ErrorMessage ="Please enter Lot Number")]
        [Display(Name = "Lot Number:")]
        [RegularExpression(@"^[a-zA-Z0-9._]*$", ErrorMessage = "Please remove the white space!")]
        public string LotNumber { get; set; }

        public string LotInfoId { get; set; }
        public int Quantity { get; set; }
        public int Operation { get; set; }
        public int ScannedQuantity { get; set; }
    }
}