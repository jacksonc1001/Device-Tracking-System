using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

/*
* Author: Jackson
* Date: 21/05/2021
* Version: 1.0.0.0
* Objective: DeviceInfo Model to get/display device information and get bin result type from database
*/
namespace Device_Tracking_System.Models
{
    public class DeviceInfo
    {
        public string DeviceId { get; set; }
        public string AliasId { get; set; }
        [RegularExpression(@"^[a-zA-Z0-9_]*$", ErrorMessage = "Please remove the whitespace and leave the field blank if not required to update!")]
        public string SourceSubstrateId { get; set; }
        [RegularExpression(@"^[a-zA-Z0-9_]*$", ErrorMessage = "Please remove the whitespace and leave the field blank if not required to update!")]
        public string TargetSubstrateId { get; set; }
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "Please enter number only or leave it empty!")]
        public string SourcePositionId { get; set; }
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "Please enter number only or leave it empty!")]
        public string TargetPositionId { get; set; }
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "Please enter number only or leave it empty!")]
        public string FromX { get; set; }
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "Please enter number only or leave it empty!")]
        public string FromY { get; set; }
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "Please enter number only or leave it empty!")]
        public string ToX { get; set; }
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "Please enter number only or leave it empty!")]
        public string ToY { get; set; }
        //[RegularExpression(@"^[a-zA-Z0-9_]*$", ErrorMessage = "Please remove the whitespace and leave the field blank if not required to update!")]
        public string BinResult { get; set; }
        [RegularExpression(@"^[a-zA-Z0-9_]*$", ErrorMessage = "Please remove the whitespace and leave the field blank if not required to update!")]
        public string BinCode { get; set; }
        [RegularExpression(@"^\S[a-zA-Z0-9_\s]*$", ErrorMessage = "Please remove the whitespace and leave the field blank if not required to update!")]
        public string BinDesc { get; set; }
        public bool Pick { get; set; }
        public string ValidateResult { get; set; }
    }
    public class BinResultInfo
    {
        public int BinResultID { get; set; }
        public string BinResultName { get; set; }
    }
}