using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

/*
* Author: Jackson
* Date: 21/05/2021
* Version: 1.0.0.0
* Objective: Custom role provider to validate role, authentication and authorisation
*/
namespace Device_Tracking_System.BusinessLogic
{
    public class CustomRoleProvider : RoleProvider
    {
        public override bool IsUserInRole(string username, string roleName)
        {
            //var user = HttpContext.Current.Session["User"];
            //    if (user == null)
            //        return false;
            //    return HttpContext.Current.Session["Role"] != null && roleName == HttpContext.Current.Session["Role"].ToString();
            throw new NotImplementedException();
        }
        public override string[] GetRolesForUser(string username)
        {
            if(HttpContext.Current.Session["User"] != null && HttpContext.Current.Session["Role"] != null)
            {
                string[] userRole = new string[]{ HttpContext.Current.Session["Role"].ToString()};
 

                return userRole;
            }
            else
            {
                return null;
            }

        }

        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            throw new NotImplementedException();
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override string[] GetUsersInRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override string[] GetAllRoles()
        {
            throw new NotImplementedException();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }

        public override string ApplicationName { get; set; }
    }
}