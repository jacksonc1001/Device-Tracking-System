using System.Web;
using System.Web.Mvc;
using System.Web.Security;

/*
* Author: Jackson
* Date: 21/05/2021
* Version: 1.0.0.0
* Objective: Custom filter to check session time out
*/
namespace Device_Tracking_System.BusinessLogic
{
    public class Filters
    {
        public class CheckSessionTimeOutAttribute : ActionFilterAttribute
        {
            
            public override void OnActionExecuting(ActionExecutingContext filterContext)
            {
                if (HttpContext.Current.Session["User"] == null)
                {
                    FormsAuthentication.SignOut();
                    filterContext.Result = new RedirectResult("~/Account/TimeOutCheck");
                    return;
                }
                base.OnActionExecuting(filterContext);
            }
        }
    }
}