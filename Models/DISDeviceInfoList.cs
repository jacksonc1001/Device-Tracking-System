/*
* Author: Jackson
* Date: 08/03/2022
* Version: 1.0.0.0
* Objective: DIS Device Info Models to get the DIS data
*/
namespace Device_Tracking_System.Models
{
    public class DISDeviceInfoList
    {
        public string DeviceId { get; set; }
        public string AliasId { get; set; }
        public string TargetSubstrateId { get; set; }
        public string TargetPositionId { get; set; }
        public string ToX { get; set; }
        public string ToY { get; set; }
        public string Result { get; set; }
        public string BinCode { get; set; }
        public string BinDesc { get; set; }
        public bool Pick { get; set; }
    }
}