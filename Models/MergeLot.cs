using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

/*
* Author: Jackson
* Date: 21/05/2021
* Version: 1.0.0.0
* Objective: MergeLot Model to get and store the merge lot information
*/
namespace Device_Tracking_System.Models
{
    public class MergeLot
    {
        [Required(ErrorMessage ="Please enter Mother Lot Number")]
        [Display(Name = "Mother Lot Number:")]
        [RegularExpression(@"^[a-zA-Z0-9._]*$", ErrorMessage = "Please remove the white space!")]
        public string MotherLotNumber { get; set; }
        [Required(ErrorMessage = "Please enter Child Lot Number")]
        [Display(Name = "Child Lot Number:")]
        [RegularExpression(@"^[a-zA-Z0-9._]*$", ErrorMessage = "Please remove the white space!")]
        public string ChildLotNumber { get; set; }
        [Display(Name = "Child Lot Number2:")]
        [RegularExpression(@"^[a-zA-Z0-9._]*$", ErrorMessage = "Please remove the white space!")]
        public string ChildLotNumber2 { get; set; }
        [Display(Name = "Child Lot Number3:")]
        [RegularExpression(@"^[a-zA-Z0-9._]*$", ErrorMessage = "Please remove the white space!")]
        public string ChildLotNumber3 { get; set; }
        [Display(Name = "Child Lot Number4:")]
        [RegularExpression(@"^[a-zA-Z0-9._]*$", ErrorMessage = "Please remove the white space!")]
        public string ChildLotNumber4 { get; set; }
        public int MotherLotId { get; set; }
        public int ChildLotId { get; set; }
        public int ChildLotId2 { get; set; }
        public int ChildLotId3 { get; set; }
        public int ChildLotId4 { get; set; }
        public int MotherLotOperation { get; set; }
        public int ChildLot1Operation { get; set; }
        public int ChildLot2Operation { get; set; }
        public int ChildLot3Operation { get; set; }
        public int ChildLot4Operation { get; set; }
        public int MotherQuantity { get; set; }
        public int ChildLot1Quantity { get; set; }
        public int ChildLot2Quantity { get; set; }
        public int ChildLot3Quantity { get; set; }
        public int ChildLot4Quantity { get; set; }
    }
}