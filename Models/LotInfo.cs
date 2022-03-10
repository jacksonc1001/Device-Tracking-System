using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/*
* Author: Jackson
* Date: 21/05/2021
* Version: 1.0.0.0
* Objective: LotInfo Model to get and store the lot information
*/
namespace Device_Tracking_System.Models
{
    public class LotInfo
    {
        public string LotNum { get; set; }
        public int Operation { get; set; }
        public string Facility { get; set; }
        public string ProductionLevel { get; set; }
        public string Product { get; set; }
        public int LotQty1 { get; set; }
        public int LotQty2 { get; set; }
        public string RouteType { get; set; }
        public string UserId { get; set; }
        public int ValidateQuantity { get; set; }
    }
}