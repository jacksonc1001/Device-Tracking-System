using Device_Tracking_System.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using static Device_Tracking_System.BusinessLogic.Filters;

/*
* Author: Jackson
* Date: 21/05/2021
* Version: 1.0.0.0
* Objective: Split Devices Controller to perform split to MES and DTS, and lot validate
*/
namespace Device_Tracking_System.Controllers
{
    [Authorize]
    public class SplitController : BaseController
    {
        // GET: Split
        [CheckSessionTimeOut]
        public ActionResult LotValidate()
        {
            if (Session["User"] != null && Session["Role"] != null)
            {
                Session.Remove("AuditLog");
                return View();
            }
            else
            {
                Alert("There was error occur during login! Please try re-login!", Notification.NotificationType.error, "Error");
                return RedirectToAction("Login", "Account");
            }
        }
        [CheckSessionTimeOut]
        [HttpPost]
        public ActionResult LotValidate(LotValidation lotValidation)
        {
            string errorMessage = string.Empty;
            string sLotId = string.Empty;
            string type = "SplitDevice";
            //convert the lot number to uppercase
            var lot = lotValidation.LotNumber.ToUpper();
            var sLotInfoList = new List<LotInfo>();
            var sDeviceList = new List<DeviceInfo>();

            if (ModelState.IsValid)
            {
                try
                {
                    //call lotInfoExt
                    var output = BusinessLogic.MESSQLDataAccess.GetLotInfo(lot);
                    //validate the data returned from getlotinfo
                    if (output.Item1 == 0)
                    {
                        //assign motherlotinfo after getting reply
                        sLotInfoList.Add(new LotInfo()
                        {
                            LotNum = lot,
                            Operation = output.Item2,
                            LotQty1 = output.Item3,
                            UserId = Session["User"].ToString()
                        });
                        //insert lotinfo to DB
                        sLotId = BusinessLogic.DTSSQLDataAccess.InsertLotInfo(sLotInfoList);
                        //validate the lotinfoid returned from database
                        if (string.IsNullOrEmpty(sLotId) == false)
                        {
                            //store the lotinfoid into lotvalidateion model
                            lotValidation.LotInfoId = sLotId;
                            lotValidation.Operation = output.Item2;
                            //call getdevicebylot
                            var getDeviceOutput = BusinessLogic.DISSQLDataAccess.GetDeviceInfo(lot, output.Item2, ConfigurationManager.AppSettings["DTSEquipment"]);

                            if (getDeviceOutput.Item1 == 0)
                            {
                                //store the output from getDevicesByLot into array
                                var deviceArray = getDeviceOutput.Item2.ToArray();
                                //get the device quantity
                                var deviceQuantity = deviceArray.GetLength(0);
                                //insert individual device info into list
                                for (int i = 0; i < deviceQuantity;)
                                {

                                    sDeviceList.Add(new DeviceInfo()
                                    {
                                        DeviceId = deviceArray[i].DeviceId,
                                        AliasId = deviceArray[i].AliasId,
                                        TargetSubstrateId = deviceArray[i].TargetSubstrateId,
                                        TargetPositionId = deviceArray[i].TargetPositionId,
                                        ToX = deviceArray[i].ToX,
                                        ToY = deviceArray[i].ToY,
                                        BinResult = deviceArray[i].Result,
                                        Pick = deviceArray[i].Pick,
                                        BinCode = deviceArray[i].BinCode,
                                        BinDesc = deviceArray[i].BinDesc,
                                    }
                                    );
                                    i++;
                                }
                                //convert the device info into datatable
                                var dTable = BusinessLogic.DTSSQLDataAccess.DTValidateDeviceInfo(sDeviceList, Convert.ToInt32(sLotId),
                                             lot);
                                //insert deviceinfo to DB
                                var dtResponse = BusinessLogic.DTSSQLDataAccess.InsertDeviceInfo(dTable);
                                //validate result from database
                                if (string.IsNullOrEmpty(dtResponse) == false && dtResponse == "Success")
                                {
                                    //get all devices info and store in session
                                    Session["SplitDeviceInfo"] = BusinessLogic.DTSSQLDataAccess.GetDeviceInfo(lot,
                                        Convert.ToInt32(sLotId), type);
                                    //store the lot info into tempdata
                                    Session["SplitLotInfo"] = lotValidation;

                                    return RedirectToAction("GridView");
                                }
                                else
                                {
                                    Alert("Error in saving devices info into database! Please try again later",
                                        Notification.NotificationType.error, "Error");
                                    BusinessLogic.Log.WriteToErrorLogFile("Error in saving devices info into database! Please try again later");
                                    return View();
                                }
                            }
                            else
                            {
                                    Alert(getDeviceOutput.Item3 + " Please verify with Process Tech/Engineer!",
                                        Notification.NotificationType.error, "Error");
                                    BusinessLogic.Log.WriteToErrorLogFile(getDeviceOutput.Item3 + 
                                        " Please verify with Process Tech/Engineer!");
                                    return View();
                            }

                        }
                        else
                        {
                            Alert("Error in saving lot info to database! Please try again later!", Notification.NotificationType.error, "Error");
                            BusinessLogic.Log.WriteToErrorLogFile("Error in saving lot info to database! Please try again later!");
                            return View();
                        }

                    }
                    else
                    {
                        Alert("Failed to get lot info from MES! Please try again later!", Notification.NotificationType.error, "Error");
                        BusinessLogic.Log.WriteToErrorLogFile("Failed to get lot info from MES! Please try again later!");
                        return View();
                    }

                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                    Alert(errorMessage, Notification.NotificationType.error, "Error");
                    BusinessLogic.Log.WriteToErrorLogFile(errorMessage);
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
        [CheckSessionTimeOut]
        public ActionResult GridView()
        {
            if (Session["User"] != null && Session["Role"] != null)
            {
                return View();
            }
            else
            {
                Alert("There was error occur during login! Please try re-login!", Notification.NotificationType.error, "Error");
                return RedirectToAction("Login", "Account");
            }
        }
        [CheckSessionTimeOut]
        public ActionResult GridViewPartial()
        {
            //validate session data
            if (Session["SplitDeviceInfo"] != null)
            {
                var tmpDeviceInfos = (List<DisplayDeviceInfo>)Session["SplitDeviceInfo"];
                return PartialView("_GridViewPartial", tmpDeviceInfos);
            }
            else
            {
                return PartialView("_GridViewPartial");
            }
        }
        [CheckSessionTimeOut]
        [HttpPost]
        public ActionResult SplitConfirm(string selectedDIDs, bool? splitFailDevices)
        {
            string errorMessage;
            try
            {
                if (string.IsNullOrEmpty(selectedDIDs) == false)
                {
                    //Get selected device ID from user and split into array form
                    string[] selectedIDs = selectedDIDs.Split(',').ToArray();
                    //count the quantity of the selected devices
                    //var totalDevices = selectedIDs.GetLength(0);
                    //Get lot number and lotinfoid from lot validation
                    var sLot = Session["SplitLotInfo"] as LotValidation;
                    string sLotNum = sLot.LotNumber.ToUpper();
                    int sLotInfoId = Convert.ToInt32(sLot.LotInfoId);
                    string splitTrans = string.Empty;

                    if(splitFailDevices == true)
                    {
                        splitTrans = "SelectPassFailDevices";
                    }
                    else
                    {
                        splitTrans = "SelectedPassDevices";
                    }

                    //declare a new variable for child lot number
                    string cLotNum = string.Empty;

                    //get lot info from DB using the lot number and lotinfoid from lot validation
                    var sLotInfo = BusinessLogic.DTSSQLDataAccess.GetLotInfo(sLotNum, sLotInfoId);

                    if (sLotInfo?.Any() == true)
                    {
                        //convert device ID from array to datatable
                        var dtSelectedDevicesId = BusinessLogic.DTSSQLDataAccess.DTDeviceId(selectedIDs);
                        //get device info for the selected device from DB
                        var selectedDevices = BusinessLogic.DTSSQLDataAccess.GetChildDeviceInfoDTS(sLotInfoId, dtSelectedDevicesId, splitTrans);

                        var totalPassDevices = selectedDevices.ToArray().GetLength(0);

                        if (totalPassDevices != 0)
                        {
                            //call split web service
                            var splitOutput = BusinessLogic.MESSQLDataAccess.SplitLot(sLotNum, totalPassDevices, sLot.Operation);

                            //Check the split result
                            if (splitOutput.Item1 == 0)
                            {
                                //store the child lot number into variable
                                cLotNum = splitOutput.Item2;
                                //store the info into DB if success
                                var dBOutput = BusinessLogic.DTSSQLDataAccess.InsertSplitLot(sLotNum, cLotNum, totalPassDevices, sLotInfo[0].Operation.ToString());

                                if (string.IsNullOrEmpty(dBOutput) == false)
                                {
                                    //convert device ID from array to datatable
                                    var dtDevicesId = BusinessLogic.DTSSQLDataAccess.DTDeviceId(selectedIDs);
                                    //get device info for the selected device from DB
                                    var cLotDevices = BusinessLogic.DTSSQLDataAccess.GetChildDeviceInfoDTS(sLotInfoId, dtDevicesId, "SplitDevice");
                                    //get device info for the parent device from DB
                                    var mLotDevices = BusinessLogic.DTSSQLDataAccess.GetMotherDeviceInfoDTS(sLotInfoId, dtDevicesId, "SplitDevice");
                                    //check content for cLotDevices and mLotDevices
                                    if (cLotDevices?.Any() == true && mLotDevices?.Any() == true)
                                    {
                                        var dtCLotDevices = BusinessLogic.DISSQLDataAccess.DTSetDISDeviceInfo(cLotDevices);

                                        var dtMLotDevices = BusinessLogic.DISSQLDataAccess.DTSetDISDeviceInfo(mLotDevices);
                                        //call split device web service
                                        var response = BusinessLogic.DISSQLDataAccess.SplitDevice(dtMLotDevices, dtCLotDevices, sLotNum, cLotNum, sLot.Operation, ConfigurationManager.AppSettings["DTSEquipment"]);

                                        if (response.Item1 == 0)
                                        {
                                            //insert event into audit log
                                            //1 = Login
                                            //2 = Logout
                                            //3 = Split
                                            //4 = Merge
                                            //5 = Update
                                            //6 = BatchUpdate
                                            var dbAuditResponse = BusinessLogic.DTSSQLDataAccess.CreateAuditLog(Session["User"].ToString(), 3,
                                                                   Convert.ToInt32(dBOutput));

                                            if (string.IsNullOrEmpty(dbAuditResponse) == false)
                                            {

                                                Alert("Successful Split Lot at MES and DIS! Mother Lot: " + sLotNum + " Child Lot: " +
                                                    cLotNum, Notification.NotificationType.success, "Success");
                                                //clear session data in this module
                                                Session.Remove("SplitDeviceInfo");
                                                Session.Remove("SplitLotInfo");

                                                return RedirectToAction("LotValidate");
                                            }
                                            else
                                            {
                                                Alert("Error in saving audit log! Please contact with CIM Engineer!",
                                                    Notification.NotificationType.error, "Error");
                                                BusinessLogic.Log.WriteToErrorLogFile("Error in saving audit log! Please contact with CIM Engineer!");
                                                return RedirectToAction("LotValidate");
                                            }
                                        }
                                        else
                                        {
                                            Alert(response.Item2 + " Please contact with CIM Engineer!",
                                                Notification.NotificationType.error, "Error");
                                            BusinessLogic.Log.WriteToErrorLogFile(response.Item2 +
                                                " Please contact with CIM Engineer!");
                                            return RedirectToAction("GridView");
                                        }
                                    }
                                    else
                                    {
                                        Alert("Error in retrieving Mother Lot or Child Lot Devices Info! Please contact with CIM Engineer!",
                                            Notification.NotificationType.error, "Error");
                                        BusinessLogic.Log.WriteToErrorLogFile("Error in retrieving Mother Lot or Child Lot Devices Info!" +
                                            " Please contact with CIM Engineer!");
                                        return RedirectToAction("GridView");
                                    }
                                }
                                else
                                {
                                    Alert("Error in saving Split lot info into Database! Please contact CIM Engineer!",
                                        Notification.NotificationType.error, "Error");
                                    BusinessLogic.Log.WriteToErrorLogFile("Error saving Split lot info into Database! Please contact CIM Engineer!");
                                    return RedirectToAction("GridView");
                                }
                            }
                            else
                            {
                                Alert(splitOutput.Item3 + " Please try again later!",
                                    Notification.NotificationType.error, "Error");
                                BusinessLogic.Log.WriteToErrorLogFile(splitOutput.Item3 + " Please try again later!");
                                return RedirectToAction("GridView");
                            }
                        }
                        else
                        {
                            Alert("Please check the Enable Split Fail Devices if want to split Fail Devices!", Notification.NotificationType.error, "Error");
                            BusinessLogic.Log.WriteToErrorLogFile("Please check the Enable Split Fail Devices if want to split Fail Devices!");
                            return RedirectToAction("GridView");
                        }
                    }
                    else
                    {
                        Alert("Error when getting lot info from database! Please try again later!",
                            Notification.NotificationType.error, "Error");
                        BusinessLogic.Log.WriteToErrorLogFile("Error when getting lot info from database! Please try again later!");
                        return RedirectToAction("GridView");
                    }
                }
                else
                {
                    Alert("No Devices been selected! Please select the checkbox in order to select the device for split!",
                        Notification.NotificationType.error, "Error");
                    BusinessLogic.Log.WriteToErrorLogFile("No Devices been selected! Please select the checkbox in order to select the device for split!");
                    return RedirectToAction("GridView");
                }
            }
            catch(Exception ex)
            {
                errorMessage = ex.Message;
                Alert(errorMessage, Notification.NotificationType.error, "Error");
                BusinessLogic.Log.WriteToErrorLogFile(errorMessage);
                return RedirectToAction("GridView");
            }
        }
        [CheckSessionTimeOut]
        [HttpPost]
        public ActionResult SplitCancel()
        {
            //clear all session data in this module
            Session.Remove("SplitDeviceInfo");
            Session.Remove("SplitLotInfo");

            return RedirectToAction("LotValidate");
        }
    }

}