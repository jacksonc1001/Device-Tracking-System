using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/*
* Author: Jackson
* Date: 21/05/2021
* Version: 1.0.0.0
* Objective: Notification model that contain the type of notification
*/
namespace Device_Tracking_System.Models
{
    public class Notification
    {
        public enum NotificationType
        {
            error,
            success,
            warning,
            info
        }
    }
}