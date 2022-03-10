using System;
using System.Web.Mvc;
using static Device_Tracking_System.Models.Notification;
using static Device_Tracking_System.BusinessLogic.Filters;
using Microsoft.Office.Interop.Word;
using System.Configuration;
using System.IO;
using System.Text.RegularExpressions;

/*
* Author: Jackson
* Date: 21/05/2021
* Version: 1.0.0.0
* Objective: Home Controller to handle download file and role checking
*/
namespace Device_Tracking_System.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        [CheckSessionTimeOut]
        public ActionResult Index()
        {
            if(Session["User"] != null && Session["Role"] != null)
            {
                Session.Remove("AuditLog");
                return View();
            }
            else
            {
                Alert("There was error occur during login! Please try re-login!",NotificationType.error, "Error");
                return RedirectToAction("Login", "Account");
            }
        }
        public ActionResult UpdateRoleCheck()
        {
            if(Session["Role"].ToString() == "Technician" || Session["Role"].ToString() == "Engineer" || Session["Role"].ToString() == "Admin")
            {
                return RedirectToAction("UpdateLotValidate", "Update");
            }
            else
            {
                Alert("You are not authorise to use this feature!", NotificationType.error, "Error");
                BusinessLogic.Log.WriteToErrorLogFile("You are not authorise to use this feature!");
                return RedirectToAction("Index","Home");
            }
        }
        public ActionResult AuditLogRoleCheck()
        {
            if(Session["Role"].ToString() == "Admin")
            {
                return RedirectToAction("Index","AuditLog");
            }
            else
            {
                Alert("You are not authorise to use this feature!", NotificationType.error, "Error");
                BusinessLogic.Log.WriteToErrorLogFile("You are not authorise to use this feature!");
                return RedirectToAction("Index", "Home");
            }
        }
        public ActionResult ViewDocument()
        {
            object documentFormat = 8;
            string randomName = DateTime.Now.Ticks.ToString();
            object htmlFilePath = Server.MapPath("~/Files/") + randomName + ".html";
            string directoryPath = Server.MapPath("~/Files/") + randomName + "_files";
            object filePath = Server.MapPath(ConfigurationManager.AppSettings["UserManualPath"]) +
                                ConfigurationManager.AppSettings["UserManualName"];

            if (!Directory.Exists(Server.MapPath("~/Files/")))
            {
                Directory.CreateDirectory(Server.MapPath("~/Files/"));
            }

            Application application = new Application();
            application.Documents.Open(ref filePath);
            Document document = application.ActiveDocument;
            document.SaveAs2(ref htmlFilePath, ref documentFormat);
            document.Close();
            string wordHTML = System.IO.File.ReadAllText(htmlFilePath.ToString());

            foreach(Match match in Regex.Matches(wordHTML, "<v:imagedata.+?src=[\"'](.+?)[\"'].*?>", RegexOptions.IgnoreCase))
            {
                wordHTML = Regex.Replace(wordHTML, match.Groups[1].Value, "Files/" + match.Groups[1].Value);
            }

            ViewBag.WordHTML = wordHTML;

            return View();
        }
        public ActionResult DownloadDocument()
        {
            string path = Server.MapPath(ConfigurationManager.AppSettings["UserManualPath"]) + ConfigurationManager.AppSettings["UserManualName"];
            byte[] bytes = System.IO.File.ReadAllBytes(path);

            return File(bytes, "application/octet-stream", ConfigurationManager.AppSettings["UserManualName"]);
        }
    }
}