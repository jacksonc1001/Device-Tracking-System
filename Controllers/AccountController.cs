using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Security;
using DevExpress.Web.Mvc;
using Device_Tracking_System.Models;
using static Device_Tracking_System.BusinessLogic.Filters;

/*
* Author: Jackson
* Date: 08/03/2021
* Version: 1.0.0.0
* Objective: Account Controller to perform login, logout and user management
*/
namespace Device_Tracking_System.Controllers
{
    public class AccountController : BaseController
    {
        // GET: Account
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(Account userInfo)
        {
            string userID;
            if (ModelState.IsValid)
            {
                //trim os if user entered osXXXXX
                if (!Regex.Match(userInfo.Username, @"^[oO][sS][0-9]*$").Success)
                {
                    userID = userInfo.Username;
                }
                else
                {
                    userID = userInfo.Username.Substring(2);
                }

                var validUser = BusinessLogic.DTSSQLDataAccess.IsValidCredentials(userID, userInfo.Password);

                if (validUser.Item1 == 0)
                {
                        userInfo.Roles = validUser.Item2;

                        Session["User"] = userID;
                        Session["Role"] = userInfo.Roles;
                        FormsAuthentication.SetAuthCookie(userID, false);
                        //Event
                        //1 = Login
                        //2 = Logout
                        //3 = Split
                        //4 = Merge
                        //5 = Update
                        //6 = BatchUpdate
                        BusinessLogic.DTSSQLDataAccess.CreateAuditLog(Session["User"].ToString(), 1, 0);
                        return RedirectToAction("Index", "Home");
                }
                else
                {
                    Alert(validUser.Item3, Notification.NotificationType.error, "Error");
                    BusinessLogic.Log.WriteToErrorLogFile(validUser.Item3);
                    return View();
                }
            }
            else
            {
                Alert("Invalid entry! Please enter correct data!", Notification.NotificationType.error, "Error");
                BusinessLogic.Log.WriteToErrorLogFile("Invalid entry! Please enter correct data!");
                return View();
            }
    }
        public ActionResult TimeOutCheck()
        {
            Alert("No Session detected or Session been timed out! Please re-login!", Notification.NotificationType.error, "Error");
            BusinessLogic.Log.WriteToErrorLogFile("Session been timed out! Please re-login!");
            return RedirectToAction("Login","Account");
        }
        [CheckSessionTimeOut]
        public ActionResult Logout()
        {
            //Event
            //1 = Login
            //2 = Logout
            //3 = Split
            //4 = Merge
            //5 = Update
            //6 = BatchUpdate
            BusinessLogic.DTSSQLDataAccess.CreateAuditLog(Session["User"].ToString(), 2, 0);

            FormsAuthentication.SignOut();

            Session.RemoveAll();

            return RedirectToAction("Login", "Account");
        }
        [CheckSessionTimeOut]
        public ActionResult AddUser()
        {
            var roleList = BusinessLogic.DTSSQLDataAccess.GetRoleType();
            ViewBag.Role = roleList;
            return View();
        }
        [CheckSessionTimeOut]
        [HttpPost]
        public ActionResult AddUserAccount(Account user)
        {
            string errorMessage;

            if(ModelState.IsValid)
            {
                try
                {
                    var dbResponse = BusinessLogic.DTSSQLDataAccess.AddUser(user.Username, user.RoleId);

                    if (dbResponse.Item1 == 0)
                    {
                        List<Account> userInfo = new List<Account>();
                        userInfo.Add(new Account()
                        {
                            Username = user.Username,
                            RoleId = user.RoleId
                        });
                        var dtUserInfo = BusinessLogic.DTSSQLDataAccess.DTUserInfo(userInfo);
                        
                        var insertLog = BusinessLogic.DTSSQLDataAccess.InsertUserManagement(dtUserInfo, Session["User"].ToString());

                        if (insertLog.Item1 == 0)
                        {
                            //Insert the event into audit log
                            //1 = Login
                            //2 = Logout
                            //3 = Split
                            //4 = Merge
                            //5 = Update
                            //6 = BatchUpdate
                            //7 = SplitStandard
                            //8 = SplitNonStandard
                            //9 = SplitORT
                            //10 =ValidatePassDevice
                            //11 =ValidateFailDevice
                            //12 =ValidateSamplingDevice
                            //13 =AddUser
                            var dbAuditResponse = BusinessLogic.DTSSQLDataAccess.CreateAuditLog(Session["User"].ToString(), 13,
                                                        Convert.ToInt32(insertLog.Item2));

                            if (string.IsNullOrEmpty(dbAuditResponse) == false)
                            {
                                Alert("Successful Add User!", Notification.NotificationType.success, "Success");
                                return RedirectToAction("AddUser");
                            }
                            else
                            {
                                Alert("Error in saving audit log! Please verify with CIM Engineer", Notification.NotificationType.error,
                                    "Error");
                                BusinessLogic.Log.WriteToErrorLogFile("Error in saving audit log! Please verify with CIM Engineer");
                                return RedirectToAction("AddUser");
                            }
                        }
                        else
                        {
                            Alert("Error in saving user management log! Please verify with CIM Engineer", Notification.NotificationType.error,
                                "Error");
                            BusinessLogic.Log.WriteToErrorLogFile("Error in saving user management log! Please verify with CIM Engineer");
                            return RedirectToAction("AddUser");
                        }
                    }
                    else
                    {
                        Alert(dbResponse.Item2, Notification.NotificationType.error, "Error");
                        return RedirectToAction("AddUser");
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                    Alert(errorMessage, Notification.NotificationType.error, "Error");
                    BusinessLogic.Log.WriteToErrorLogFile(errorMessage);
                    return View("AddUser");
                }
            }
            else
            {
                Alert("Invalid data been entered! Please try again!", Notification.NotificationType.error, "Error");
                BusinessLogic.Log.WriteToErrorLogFile("Invalid data been entered! Please try again!");
                return RedirectToAction("AddUser");
            }
        }
        [CheckSessionTimeOut]
        public ActionResult ModifyUserInfo()
        {
            return View();
        }
        public PartialViewResult ModifyUserInfoGridView()
        {
            var userList = BusinessLogic.DTSSQLDataAccess.GetAllUser();
                
            if (userList != null)
            {
                return PartialView("ModifyUserInfoGridView", userList);
            }
            else
            {
                return PartialView("ModifyUserInfoGridView");
            }


        }
        [CheckSessionTimeOut]
        [HttpPost]
        public ActionResult ModifyUserInfo(MVCxGridViewBatchUpdateValues<Account, string> updateValues)
        {
            string errorMessage;
            
            if(ModelState.IsValid)
            {
                try
                {
                    var updatedUserInfo = updateValues.Update.ToList();

                    var dtUpdatedUserInfo = BusinessLogic.DTSSQLDataAccess.DTUserInfo(updatedUserInfo);

                    var dbResponse = BusinessLogic.DTSSQLDataAccess.ModifyUserInfo(dtUpdatedUserInfo);

                    if (dbResponse.Item1 == 0)
                    {
                        var insertLog = BusinessLogic.DTSSQLDataAccess.InsertUserManagement(dtUpdatedUserInfo, Session["User"].ToString());

                        if (insertLog.Item1 == 0)
                        {
                            //Insert the event into audit log
                            //1 = Login
                            //2 = Logout
                            //3 = Split
                            //4 = Merge
                            //5 = Update
                            //6 = BatchUpdate
                            //7 = SplitStandard
                            //8 = SplitNonStandard
                            //9 = SplitORT
                            //10 =ValidatePassDevice
                            //11 =ValidateFailDevice
                            //12 =ValidateSamplingDevice
                            //13 =AddUser
                            //14 =UpdateUserInfo
                            var dbAuditResponse = BusinessLogic.DTSSQLDataAccess.CreateAuditLog(Session["User"].ToString(), 14,
                                                        Convert.ToInt32(insertLog.Item2));

                            if (string.IsNullOrEmpty(dbAuditResponse) == false)
                            {
                                Alert("Successful Modify User Info!", Notification.NotificationType.success, "Success");
                                return RedirectToAction("ModifyUserInfoGridView");
                            }
                            else
                            {
                                Alert("Error in saving audit log! Please verify with CIM Engineer", Notification.NotificationType.error,
                                    "Error");
                                BusinessLogic.Log.WriteToErrorLogFile("Error in saving audit log! Please verify with CIM Engineer");
                                return RedirectToAction("ModifyUserInfoGridView");
                            }
                        }
                        else
                        {
                            Alert("Error in saving user management log! Please verify with CIM Engineer", Notification.NotificationType.error,
                                "Error");
                            BusinessLogic.Log.WriteToErrorLogFile("Error in saving user management log! Please verify with CIM Engineer");
                            return RedirectToAction("ModifyUserInfoGridView");
                        }
                    }
                    else
                    {
                        Alert(dbResponse.Item2, Notification.NotificationType.error, "Error");
                        return RedirectToAction("ModifyUserInfoGridView");
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                    Alert(errorMessage, Notification.NotificationType.error, "Error");
                    BusinessLogic.Log.WriteToErrorLogFile(errorMessage);
                    return View("ModifyUserInfoGridView");
                }
            }
            else
            {
                Alert("Invalid data been entered! Please try again!", Notification.NotificationType.error, "Error");
                BusinessLogic.Log.WriteToErrorLogFile("Invalid data been entered! Please try again!");
                return RedirectToAction("ModifyUserInfoGridView");
            }
        }
        [CheckSessionTimeOut]
        public ActionResult ChangePassword()
        {
            return View();
        }
        [CheckSessionTimeOut]
        [HttpPost]
        public ActionResult ChangePassword(ChangePassword password)
        {
            string errorMessage;
            var oldPass = password.OldPassword;
            var newPass = password.NewPassword;

            if(ModelState.IsValid)
            {
                try
                {
                    var dbResponse = BusinessLogic.DTSSQLDataAccess.ChangeUserPassword(Session["User"].ToString(), oldPass, newPass);

                    if (dbResponse.Item1 == 0)
                    {
                        Alert("Successful Change Password!", Notification.NotificationType.success, "Success");
                        return RedirectToAction("ChangePassword");
                    }
                    else
                    {
                        Alert(dbResponse.Item2, Notification.NotificationType.error, "Error");
                        return RedirectToAction("ChangePassword");
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                    Alert(errorMessage, Notification.NotificationType.error, "Error");
                    BusinessLogic.Log.WriteToErrorLogFile(errorMessage);
                    return View("ChangePassword");
                }
            }
            else
            {
                Alert("Invalid data been entered! Please try again!", Notification.NotificationType.error, "Error");
                BusinessLogic.Log.WriteToErrorLogFile("Invalid data been entered! Please try again!");
                return RedirectToAction("ChangePassword");
            }
        }
        [CheckSessionTimeOut]
        [HttpPost]
        public ActionResult DeleteUser(string selectedUserIDs)
        {
            string errorMessage;

            if (ModelState.IsValid)
            {
                try
                {
                    string[] arrSelectedUserIDs = selectedUserIDs.Split(',').ToArray();

                    var dtUserId = BusinessLogic.DTSSQLDataAccess.DTUserId(arrSelectedUserIDs);

                    var dbResponse = BusinessLogic.DTSSQLDataAccess.DeleteUser(dtUserId);

                    if (dbResponse.Item1 == 0)
                    {
                        List<Account> userInfo = new List<Account>();

                        var numUser = arrSelectedUserIDs.GetLength(0);

                        for (int i = 0; i < numUser;)
                        {
                            userInfo.Add(new Account()
                            {
                                Username = arrSelectedUserIDs[i]
                            });

                            i++;
                        }

                        var dtUserInfo = BusinessLogic.DTSSQLDataAccess.DTUserInfo(userInfo);

                        var insertLog = BusinessLogic.DTSSQLDataAccess.InsertUserManagement(dtUserInfo, Session["User"].ToString());

                        if (insertLog.Item1 == 0)
                        {
                            //Insert the event into audit log
                            //1 = Login
                            //2 = Logout
                            //3 = Split
                            //4 = Merge
                            //5 = Update
                            //6 = BatchUpdate
                            //7 = SplitStandard
                            //8 = SplitNonStandard
                            //9 = SplitORT
                            //10 =ValidatePassDevice
                            //11 =ValidateFailDevice
                            //12 =ValidateSamplingDevice
                            //13 =AddUser
                            //14 =UpdateUserInfo
                            //15 =DeleteUser
                            var dbAuditResponse = BusinessLogic.DTSSQLDataAccess.CreateAuditLog(Session["User"].ToString(), 15,
                                                        Convert.ToInt32(insertLog.Item2));

                            if (string.IsNullOrEmpty(dbAuditResponse) == false)
                            {
                                Alert("Successful Delete User!", Notification.NotificationType.success, "Success");
                                return RedirectToAction("ModifyUserInfoGridView");
                            }
                            else
                            {
                                Alert("Error in saving audit log! Please verify with CIM Engineer", Notification.NotificationType.error,
                                    "Error");
                                BusinessLogic.Log.WriteToErrorLogFile("Error in saving audit log! Please verify with CIM Engineer");
                                return RedirectToAction("ModifyUserInfoGridView");
                            }
                        }
                        else
                        {
                            Alert("Error in saving user management log! Please verify with CIM Engineer", Notification.NotificationType.error,
                                "Error");
                            BusinessLogic.Log.WriteToErrorLogFile("Error in saving user management log! Please verify with CIM Engineer");
                            return RedirectToAction("ModifyUserInfoGridView");
                        }
                    }
                    else
                    {
                        Alert(dbResponse.Item2, Notification.NotificationType.error, "Error");
                        return RedirectToAction("ModifyUserInfoGridView");
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                    Alert(errorMessage, Notification.NotificationType.error, "Error");
                    BusinessLogic.Log.WriteToErrorLogFile(errorMessage);
                    return View("ModifyUserInfoGridView");
                }
            }
            else
            {
                Alert("Invalid data been entered! Please try again!", Notification.NotificationType.error, "Error");
                BusinessLogic.Log.WriteToErrorLogFile("Invalid data been entered! Please try again!");
                return RedirectToAction("ModifyUserInfoGridView");
            }
        }
        public static IEnumerable GetRoleList()
        {
            var roleList = BusinessLogic.DTSSQLDataAccess.GetRoleType();
            return roleList;
        }
    }
}