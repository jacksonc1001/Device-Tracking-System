using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Device_Tracking_System.Models;
using static Device_Tracking_System.BusinessLogic.Filters;

/*
* Author: Jackson
* Date: 21/05/2021
* Version: 1.0.0.0
* Objective: Audit Log Controller to get audit log
*/
namespace Device_Tracking_System.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AuditLogController : BaseController
    {
        // GET: AuditLog
        [CheckSessionTimeOut]
        public ActionResult Index()
        {
            if (Session["User"] != null && Session["Role"] != null)
            {
                return View();
            }
            else
            {
                Alert("There was error occur! Please try re-login!", Notification.NotificationType.error, "Error");
                return RedirectToAction("Login", "Account");
            }
        }
        [CheckSessionTimeOut]
        public ActionResult AuditLogGridViewPartial()
        {
            if( Session["AuditLog"] != null)
            {
                var aLogs = (List<AuditLog>)Session["AuditLog"];
                return PartialView("_auditLogGridViewPartial", aLogs);
            }
            else
            {
                return PartialView("_auditLogGridViewPartial");
            }
            
        }
        [CheckSessionTimeOut]
        [HttpPost]
        public ActionResult GetAuditLog(AuditLog audit)
        {
            if (ModelState.IsValid)
            {
                //var aLogs = BusinessLogic.DTSSQLDataAccess.GetAuditLog(audit.StartDate, audit.EndDate);
                var auditLogs = BusinessLogic.DTSSQLDataAccess.GetAuditLog(audit.StartDate, audit.EndDate);

                if (auditLogs?.Any() == true)
                {
                    Session["AuditLog"] = auditLogs;
                    return RedirectToAction("Index");
                }
                else
                {
                    Alert("No data in database! Please select others date!", Notification.NotificationType.error, "Error");
                    BusinessLogic.Log.WriteToErrorLogFile("No data in database! Please select others date!");
                    return RedirectToAction("Index");
                }
            }
            else
            {
                Alert("Invalid entry! Please enter correct data!", Notification.NotificationType.error, "Error");
                BusinessLogic.Log.WriteToErrorLogFile("Invalid entry! Please enter correct data!");
                return RedirectToAction("Index");
            }
        }
    }
}