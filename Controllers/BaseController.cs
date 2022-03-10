using System.Web.Mvc;
using static Device_Tracking_System.Models.Notification;

/*
* Author: Jackson
* Date: 21/05/2021
* Version: 1.0.0.0
* Objective: Base Controller that maintain sweetalert notification method
*/
namespace Device_Tracking_System.Controllers
{
    public abstract class BaseController : Controller
    {
        public void Alert(string message, NotificationType notificationType, string notificationTitle)
        {
                var msg = "<script language='javascript'>swal('" + notificationTitle.ToUpper() + "', '" + message + "','" + notificationType + "')" + "</script>";
                TempData["Notification"] = msg;

        }
    }
}