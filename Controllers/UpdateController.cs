using Device_Tracking_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using DevExpress.Web.Mvc;
using static Device_Tracking_System.BusinessLogic.Filters;
using System.Collections;
using System.Data;
using System.Configuration;

/*
* Author: Jackson
* Date: 21/05/2021
* Version: 1.0.0.0
* Objective: Update Devices Controller to perform update to MES and DTS, and lot validate
*/
namespace Device_Tracking_System.Controllers
{
    [Authorize(Roles = "Technician,Engineer,Admin")]
    public class UpdateController : BaseController
    {
        // GET: Update
        [CheckSessionTimeOut]
        public ActionResult UpdateLotValidate()
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
        public ActionResult UpdateLotValidate(LotValidation uLotValidation)
        {
            string errorMessage = string.Empty;
            string uLotId = string.Empty;
            //convert the lot number to uppercase
            var lot = uLotValidation.LotNumber.ToUpper();
            var uLot = new List<LotInfo>();
            var uDeviceList = new List<DeviceInfo>();
            string type = "UpdateDevice";

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
                        uLot.Add(new LotInfo()
                        {
                            LotNum = lot,
                            Operation = output.Item2,
                            LotQty1 = output.Item3,
                            UserId = Session["User"].ToString()
                        });
                        //insert lotinfo to DB
                        uLotId = BusinessLogic.DTSSQLDataAccess.InsertLotInfo(uLot);
                        //validate the lotinfoid returned from database
                        if (string.IsNullOrEmpty(uLotId) == false)
                        {
                            //store the lotinfoid into lotvalidation model
                            uLotValidation.LotInfoId = uLotId;
                            uLotValidation.Operation = output.Item2;
                            //call getdevicebylot
                            var getDeviceOutput = BusinessLogic.DISSQLDataAccess.GetDeviceInfo(lot, output.Item2, ConfigurationManager.AppSettings["DTSEquipment"]);

                            //validate the getdevicesbylot result
                            if (getDeviceOutput.Item1 == 0)
                            {
                                //store the devices info into array
                                var deviceList = getDeviceOutput.Item2.ToArray();
                                //get the devices quantity
                                var deviceQuantity = deviceList.GetLength(0);
                                //store the individual device info into list
                                for (int i = 0; i < deviceQuantity;)
                                {
                                    uDeviceList.Add(new DeviceInfo()
                                    {
                                        DeviceId = deviceList[i].DeviceId,
                                        AliasId = deviceList[i].AliasId,
                                        TargetSubstrateId = deviceList[i].TargetSubstrateId,
                                        TargetPositionId = deviceList[i].TargetPositionId,
                                        ToX = deviceList[i].ToX,
                                        ToY = deviceList[i].ToY,
                                        BinResult = deviceList[i].Result,
                                        Pick = deviceList[i].Pick,
                                        BinCode = deviceList[i].BinCode,
                                        BinDesc = deviceList[i].BinDesc,
                                    }
                                    );
                                    i++;
                                }
                                //convert the devices info list into datatable
                                var dTable = BusinessLogic.DTSSQLDataAccess.DTValidateDeviceInfo(uDeviceList, Convert.ToInt32(uLotId), lot);
                                //insert deviceinfo to DB
                                var dtResponse = BusinessLogic.DTSSQLDataAccess.InsertDeviceInfo(dTable);
                                //validate the result returned from database
                                if (string.IsNullOrEmpty(dtResponse) == false && dtResponse == "Success")
                                {
                                    //get all the devices info and store into session
                                    Session["UpdateDeviceInfo"] = BusinessLogic.DTSSQLDataAccess.GetDeviceInfo(lot, Convert.ToInt32(uLotId), type);
                                    //store the lot info into tempdata
                                    Session["UpdateLotInfo"] = uLotValidation;

                                    return RedirectToAction("UpdateGridView");
                                }
                                else
                                {
                                    Alert("Error in saving devices info into database! Please try again later!", Notification.NotificationType.error, "Error");
                                    BusinessLogic.Log.WriteToErrorLogFile("Error in saving devices info into database! Please try again later!");
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
        public ActionResult UpdateGridView()
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
            //}
        }
        [CheckSessionTimeOut]
        [ValidateInput(false)]
        public PartialViewResult UpdateGridViewPartial()
        {
            if (Session["UpdateDeviceInfo"] != null)
            {

                var tmpUpdateDeviceInfos = (List<DisplayDeviceInfo>)Session["UpdateDeviceInfo"];

                return PartialView("_UpdateGridViewPartial", tmpUpdateDeviceInfos);
            }
            else
            {
                return PartialView("_UpdateGridViewPartial");
            }
        }
        public PartialViewResult UpdateModal()
        {
            var binResultList = BusinessLogic.DTSSQLDataAccess.GetBinResultType();
            binResultList.RemoveAt(3);
            ViewBag.BinResult = binResultList;
            return PartialView("_ShowModalPartialView");
        }
        [CheckSessionTimeOut]
        [HttpPost, ValidateInput(false)]
        public ActionResult BatchEditingUpdate(MVCxGridViewBatchUpdateValues<DisplayDeviceInfo, string> updateValues)
        {
            string errorMessage;
            var updateLotInfo = Session["UpdateLotInfo"] as LotValidation;
            //convert the lot number to uppercase
            string uLotNum = updateLotInfo.LotNumber.ToUpper();
            //convert the lotinfoid to int
            int uLotInfoId = Convert.ToInt32(updateLotInfo.LotInfoId);

            if (ModelState.IsValid)
            {
                try
                {
                    //get and store the updated devices info from user into list
                    var updateDeviceInfo = updateValues.Update.ToList();
                    //get lot info from database
                    var uLotInfo = BusinessLogic.DTSSQLDataAccess.GetLotInfo(uLotNum, uLotInfoId);
                    //convert the devices info into datatable
                    var dtDeviceList = BusinessLogic.DTSSQLDataAccess.DTSetDeviceInfo(updateDeviceInfo, uLotInfoId, uLotNum);
                    //update the devices info into database
                    var dbResponse = BusinessLogic.DTSSQLDataAccess.UpdateDeviceInfo(dtDeviceList);
                    //validate the result returned from database
                    if (string.IsNullOrEmpty(dbResponse) == false && dbResponse == "Success")
                    {
                        //convert the lot number into array
                        string[] arrULotNum = new string[] { uLotNum };
                        //get all the devices info from database
                        var allDeviceInfo = BusinessLogic.DTSSQLDataAccess.GetDeviceInfoDTS(uLotNum, uLotInfoId, "Update");

                        if (allDeviceInfo?.Any() == true)
                        {
                            var dtUpdateDevice = BusinessLogic.DISSQLDataAccess.DTSetDISDeviceInfo(allDeviceInfo);
                            //call update devices info by lot
                            var dtsResponse = BusinessLogic.DISSQLDataAccess.UpdateDeviceInfo(uLotNum, dtUpdateDevice, updateLotInfo.Operation, ConfigurationManager.AppSettings["DTSEquipment"]);

                            //validate the result
                            if (dtsResponse.Item1 == 0)
                            {
                                //insert update lot log
                                var dbUpdateLotResponse = BusinessLogic.DTSSQLDataAccess.InsertUpdateLot(dtDeviceList, uLotInfo[0].Operation.ToString());
                                //validate update lot log id
                                if(string.IsNullOrEmpty(dbUpdateLotResponse) == false)
                                {
                                    //Insert the event into audit log
                                    //1 = Login
                                    //2 = Logout
                                    //3 = Split
                                    //4 = Merge
                                    //5 = Update
                                    //6 = BatchUpdate
                                    var dbAuditResponse = BusinessLogic.DTSSQLDataAccess.CreateAuditLog(Session["User"].ToString(), 5,
                                                          Convert.ToInt32(dbUpdateLotResponse));

                                    if (string.IsNullOrEmpty(dbAuditResponse) == false)
                                    {
                                        Alert("Successful Update Devices Info to DIS!", Notification.NotificationType.success, "Success");
                                        //clear the session data in this module
                                        Session.Remove("UpdateDeviceInfo");
                                        Session.Remove("UpdateLotInfo");
                                        return RedirectToAction("UpdateLotValidate");
                                    }
                                    else
                                    {
                                        Alert("Error in saving audit log! Please verify with CIM Engineer!", Notification.NotificationType.error, "Error");
                                        BusinessLogic.Log.WriteToErrorLogFile("Error in saving audit log! Please verify with CIM Engineer!");
                                        return RedirectToAction("UpdateLotValidate");
                                    }
                                }
                                else
                                {
                                    Alert("Error in saving update lot log! Please verify with CIM Engineer!", Notification.NotificationType.error, "Error");
                                    BusinessLogic.Log.WriteToErrorLogFile("Error in saving update lot log! Please verify with CIM Engineer!");
                                    return RedirectToAction("UpdateLotValidate");
                                }

                            }
                            else
                            {
                                Alert("Failed to update Device Info to DIS! Please try again later!", Notification.NotificationType.error, "Error");
                                BusinessLogic.Log.WriteToErrorLogFile("Failed to update Device Info to DIS! Please try again later!");
                                return RedirectToAction("UpdateGridView");
                            }
                        }
                        else
                        {
                            Alert("Error when getting Device Info from database! Please try again later!", Notification.NotificationType.error, "Error");
                            BusinessLogic.Log.WriteToErrorLogFile("Error when getting Device Info from database! Please try again later!");
                            return RedirectToAction("UpdateGridView");
                        }
                    }
                    else
                    {

                        Alert("Error when updating new Device Info to database! Please try again later!", Notification.NotificationType.error, "Error");
                        BusinessLogic.Log.WriteToErrorLogFile("Error when updating new Device Info to database! Please try again later!");
                        return RedirectToAction("UpdateGridView");
                    }

                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                    Alert(errorMessage, Notification.NotificationType.error, "Error");
                    BusinessLogic.Log.WriteToErrorLogFile(errorMessage);
                    return View("UpdateGridView");
                }
            }
            else
            {
                Alert("Invalid data been entered! Please try again!", Notification.NotificationType.error, "Error");
                BusinessLogic.Log.WriteToErrorLogFile("Invalid data been entered! Please try again!");
                return RedirectToAction("UpdateGridView");
            }
        }
        [CheckSessionTimeOut]
        [HttpPost]
        public ActionResult BatchUpdateConfirm(DeviceInfo uDeviceInfo, string selectedDeviceIDs)
        {
            var updateLotInfo = Session["UpdateLotInfo"] as LotValidation;
            //convert the lot number to uppercase
            var tLotNumber = updateLotInfo.LotNumber.ToUpper();
            var tLotInfoId = Convert.ToInt32(updateLotInfo.LotInfoId);
            
            string errorMessage;

            if (ModelState.IsValid)
            {
                try
                {
                    if (string.IsNullOrEmpty(selectedDeviceIDs) == false)
                    {
                        //split the device ids into array
                        string[] selectedDvIDs = selectedDeviceIDs.Split(',').ToArray();
                        //get lot info from database
                        var uLotInfo = BusinessLogic.DTSSQLDataAccess.GetLotInfo(tLotNumber, tLotInfoId);
                        //validate the lot info returned from database
                        if (uLotInfo?.Any() == true)
                        {
                            //convert and store the device ids into datatable
                            var dtDeviceIDs = BusinessLogic.DTSSQLDataAccess.DTDeviceId(selectedDvIDs);
                            //update the update device info for particular device into database
                            var dbResponse = BusinessLogic.DTSSQLDataAccess.BatchUpdateDeviceInfo(dtDeviceIDs, tLotNumber, tLotInfoId, uDeviceInfo);
                            //validate the result returned from database
                            if (string.IsNullOrEmpty(dbResponse) == false && dbResponse == "Success")
                            {
                                //get all the devices info from database
                                var allUpdatedDevices = BusinessLogic.DTSSQLDataAccess.GetDeviceInfoDTS(tLotNumber, tLotInfoId, "Update");
                                //validate the devices info returned from database
                                if (allUpdatedDevices?.Any() == true)
                                {
                                    var dtUpdateDevices = BusinessLogic.DISSQLDataAccess.DTSetDISDeviceInfo(allUpdatedDevices);
                                    //call update devices by lot 
                                    var setDTSDeviceInfo = BusinessLogic.DISSQLDataAccess.UpdateDeviceInfo(tLotNumber, dtUpdateDevices, updateLotInfo.Operation, ConfigurationManager.AppSettings["DTSEquipment"]);

                                    //validate the result returned from updatedevicesbylot
                                    if (setDTSDeviceInfo.Item1 == 0)
                                    {
                                       var dbBatchUpdateResponse = BusinessLogic.DTSSQLDataAccess.InsertBatchUpdateLot(dtDeviceIDs, uDeviceInfo, tLotNumber,
                                                                   tLotInfoId, uLotInfo[0].Operation.ToString());
                                        //validate update lot log id
                                        if (string.IsNullOrEmpty(dbBatchUpdateResponse) == false)
                                        {
                                            //Insert event into audit log
                                            //1 = Login
                                            //2 = Logout
                                            //3 = Split
                                            //4 = Merge
                                            //5 = Update
                                            //6 = BatchUpdate
                                            var dbAuditResponse = BusinessLogic.DTSSQLDataAccess.CreateAuditLog(Session["User"].ToString(), 6,
                                                                  Convert.ToInt32(dbBatchUpdateResponse));

                                            if (string.IsNullOrEmpty(dbAuditResponse) == false)
                                            {
                                                Alert("Successful Batch Update Devices Info to DIS!", Notification.NotificationType.success, "Success");
                                                //clear the session data in this module
                                                Session.Remove("UpdateDeviceInfo");
                                                Session.Remove("UpdateLotInfo");

                                                return RedirectToAction("UpdateLotValidate");
                                            }
                                            else
                                            {
                                                Alert("Error in saving audit log! Please verify with CIM Engineer", Notification.NotificationType.error,
                                                    "Error");
                                                BusinessLogic.Log.WriteToErrorLogFile("Error in saving audit log! Please verify with CIM Engineer");
                                                return RedirectToAction("UpdateLotValidate");
                                            }
                                        }
                                        else
                                        {
                                            Alert("Error in saving batch update log! Please verify with CIM Engineer", Notification.NotificationType.error,
                                                "Error");
                                            BusinessLogic.Log.WriteToErrorLogFile("Error in saving batch update log! Please verify with CIM Engineer");
                                            return RedirectToAction("UpdateLotValidate");
                                        }
                                    }
                                    else
                                    {
                                            Alert(setDTSDeviceInfo.Item2 + " Please try again later!", Notification.NotificationType.error,
                                                "Error");
                                            BusinessLogic.Log.WriteToErrorLogFile(setDTSDeviceInfo.Item2 + " Please try again later!");
                                            return RedirectToAction("UpdateGridView");
                                    }
                                }
                                else
                                {
                                    Alert("Error in retrieving Updated Devices Info from database! Please try again later!",
                                        Notification.NotificationType.error, "Error");
                                    BusinessLogic.Log.WriteToErrorLogFile("Error in retrieving Updated Devices Info from database! Please try again later!");
                                    return RedirectToAction("UpdateGridView");
                                }
                            }
                            else
                            {
                                Alert("Error in updating the new Devices Info into database! Please try again later!",
                                    Notification.NotificationType.error, "Error");
                                BusinessLogic.Log.WriteToErrorLogFile("Error in updating the new Devices Info into database! Please try again later!");
                                return RedirectToAction("UpdateGridView");
                            }
                        }
                        else
                        {
                            Alert("Error in getting the lot info from database! Please try again later!", Notification.NotificationType.error, "Error");
                            BusinessLogic.Log.WriteToErrorLogFile("Error in getting the lot info from database! Please try again later!");
                            return RedirectToAction("UpdateGridView");
                        }
                    }
                    else
                    {
                        Alert("No Devices been selected! Please select the checkbox in order to select the device for update!",
                            Notification.NotificationType.error, "Error");
                        BusinessLogic.Log.WriteToErrorLogFile("No Devices been selected! Please select the checkbox in order to select the device for update!");
                        return RedirectToAction("UpdateGridView");
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                    Alert(errorMessage, Notification.NotificationType.error, "Error");
                    BusinessLogic.Log.WriteToErrorLogFile(errorMessage);
                    return RedirectToAction("UpdateGridView");
                }
            }
            else
            {
                Alert("Invalid data been entered! Please try again!", Notification.NotificationType.error, "Error");
                BusinessLogic.Log.WriteToErrorLogFile("Invalid data been entered! Please try again!");
                return RedirectToAction("UpdateGridView");
            }

        }
        [CheckSessionTimeOut]
        [HttpPost]
        public ActionResult UpdateCancel()
        {
            //clear all the session data in this module
            Session.Remove("UpdateDeviceInfo");
            Session.Remove("UpdateLotInfo");

            return RedirectToAction("UpdateLotValidate");
        }
        public static IEnumerable GetBinResultList()
        {
            var binResultList = BusinessLogic.DTSSQLDataAccess.GetBinResultType();
            return binResultList;
        }
    }
}