using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Device_Tracking_System.Models
{
    public class DisplayDeviceInfo
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
        public int BinResultID { get; set; }
        [RegularExpression(@"^[a-zA-Z0-9_]*$", ErrorMessage = "Please remove the whitespace and leave the field blank if not required to update!")]
        public string BinCode { get; set; }
        [RegularExpression(@"^\S[a-zA-Z0-9_\s]*$", ErrorMessage = "Please remove the whitespace and leave the field blank if not required to update!")]
        public string BinDesc { get; set; }
        public bool Pick { get; set; }
    }
}