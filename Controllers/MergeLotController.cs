using Device_Tracking_System.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Web.Mvc;
using static Device_Tracking_System.BusinessLogic.Filters;

/*
* Author: Jackson
* Date: 21/05/2021
* Version: 1.0.0.0
* Objective: Merge Devices Controller to perform merge to MES and DTS, and lot validate
*/
namespace Device_Tracking_System.Controllers
{
    [Authorize]
    public class MergeLotController : BaseController
    {
        // GET: MergeLot
        [CheckSessionTimeOut]
        public ActionResult MergeLotValidate()
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
        public ActionResult MergeLotValidate(MergeLot mergeLot)
        {
            string motherLot = mergeLot.MotherLotNumber.ToUpper();
            mergeLot.MotherLotNumber = motherLot;
            string childLot = mergeLot.ChildLotNumber.ToUpper();
            mergeLot.ChildLotNumber = childLot;
            string childLot2 = string.Empty;
            string childLotId2 = string.Empty;
            var childLotInfos2 = new List<LotInfo>();
            var childDeviceList2 = new List<DeviceInfo>();
            string childLot3 = string.Empty;
            string childLotId3 = string.Empty;
            var childLotInfos3 = new List<LotInfo>();
            var childDeviceList3 = new List<DeviceInfo>();
            string childLot4 = string.Empty;
            string childLotId4 = string.Empty;
            var childLotInfos4 = new List<LotInfo>();
            var childDeviceList4 = new List<DeviceInfo>();
            string motherLotId = string.Empty;
            string childLotId = string.Empty;
            var motherLotInfos = new List<LotInfo>();
            var motherDeviceList = new List<DeviceInfo>();
            var childLotInfos = new List<LotInfo>();
            var childDeviceList = new List<DeviceInfo>();
            string errorMessage = string.Empty;
            bool mSuccess = false;
            string type = "MergeLot";

            if (string.IsNullOrEmpty(mergeLot.ChildLotNumber2) == false)
            {
                childLot2 = mergeLot.ChildLotNumber2.ToUpper();
                mergeLot.ChildLotNumber2 = childLot2;
            }

            if (string.IsNullOrEmpty(mergeLot.ChildLotNumber3) == false)
            {
                childLot3 = mergeLot.ChildLotNumber3.ToUpper();
                mergeLot.ChildLotNumber3 = childLot3;
            }

            if (string.IsNullOrEmpty(mergeLot.ChildLotNumber4) == false)
            {
                childLot4 = mergeLot.ChildLotNumber4.ToUpper();
                mergeLot.ChildLotNumber4 = childLot4;
            }

            if (ModelState.IsValid)
            {
                if (motherLot != childLot || motherLot != childLot2 || motherLot != childLot3 || motherLot != childLot4)
                {
                    try
                        {
                            //call lotInfoExt for mother lot
                            var mOutput = BusinessLogic.MESSQLDataAccess.GetLotInfo(motherLot);
                            //validate the data returned from getlotinfo
                            if (mOutput.Item1 == 0)
                            {
                                mergeLot.MotherLotOperation = Convert.ToInt32(mOutput.Item2);
                                //assign motherlotinfo after getting reply
                                motherLotInfos.Add(new LotInfo()
                                {
                                    LotNum = motherLot,
                                    Operation = mOutput.Item2,
                                    LotQty1 = mOutput.Item3,
                                    UserId = Session["User"].ToString()
                                });
                                // insert mother lot info to DB
                                motherLotId = BusinessLogic.DTSSQLDataAccess.InsertLotInfo(motherLotInfos);
                                //validate the mother lotinfoid returned from database
                                if (string.IsNullOrEmpty(motherLotId) == false)
                                {
                                    //convert and store the mother lotinfoid into mergelot model
                                    mergeLot.MotherLotId = Convert.ToInt32(motherLotId);
                                    //get mother lot devices
                                    var getMotherDeviceOutput = BusinessLogic.DISSQLDataAccess.GetDeviceInfo(motherLot, mOutput.Item2, ConfigurationManager.AppSettings["DTSEquipment"]);
                                                                       
                                    //validate the getdevicebylot result
                                    if (getMotherDeviceOutput.Item1 == 0)
                                    {
                                        //store the devices info into array
                                        var mDeviceArray = getMotherDeviceOutput.Item2.ToArray();
                                        //get the devices quantity
                                        var mDeviceQuantity = mDeviceArray.GetLength(0);

                                        //convert and store the individual device info into list
                                        for (int i = 0; i < mDeviceQuantity;)
                                        {
                                            motherDeviceList.Add(new DeviceInfo()
                                            {
                                                DeviceId = mDeviceArray[i].DeviceId,
                                                AliasId = mDeviceArray[i].AliasId,
                                                TargetSubstrateId = mDeviceArray[i].TargetSubstrateId,
                                                TargetPositionId = mDeviceArray[i].TargetPositionId,
                                                ToX = mDeviceArray[i].ToX,
                                                ToY = mDeviceArray[i].ToY,
                                                BinResult = mDeviceArray[i].Result,
                                                Pick = mDeviceArray[i].Pick,
                                                BinCode = mDeviceArray[i].BinCode,
                                                BinDesc = mDeviceArray[i].BinDesc,
                                            });
                                            i++;
                                        }
                                        //convert the devices info list into datatable
                                        var dTable = BusinessLogic.DTSSQLDataAccess.DTValidateDeviceInfo(motherDeviceList, Convert.ToInt32(motherLotId), motherLot);
                                        //insert deviceinfo to DB
                                        var dtResponse = BusinessLogic.DTSSQLDataAccess.InsertDeviceInfo(dTable);
                                        //validate the result returned from database
                                        if (string.IsNullOrEmpty(dtResponse) == false && dtResponse == "Success")
                                        {
                                            var motherLotDevices = BusinessLogic.DTSSQLDataAccess.GetDeviceInfo(motherLot,
                                                Convert.ToInt32(motherLotId), type);
                                            //store mother devices quantity in viewbag to show at view
                                            mergeLot.MotherQuantity = motherLotDevices.ToArray().GetLength(0);
                                            //get all the device info and store into session
                                            Session["MergeMotherDeviceInfo"] = motherLotDevices;
                                            //set mother lot transaction success
                                            mSuccess = true;
                                            #region Child Lot
                                            //validate the mother lot transaction
                                            if (mSuccess == true)
                                            {
                                                //call getlotinfo for child lot number
                                                var cOutput = BusinessLogic.MESSQLDataAccess.GetLotInfo(childLot);
                                                //validate the data returned from getlotinfo
                                                if (cOutput.Item1 == 0)
                                                {
                                                    mergeLot.ChildLot1Operation = Convert.ToInt32(cOutput.Item2);
                                                    //assign motherlotinfo after getting reply
                                                    childLotInfos.Add(new LotInfo()
                                                    {
                                                        LotNum = childLot,
                                                        Operation = cOutput.Item2,
                                                        LotQty1 = cOutput.Item3,
                                                        UserId = Session["User"].ToString()
                                                    });
                                                    //insert the child lot info into database
                                                    childLotId = BusinessLogic.DTSSQLDataAccess.InsertLotInfo(childLotInfos);
                                                    //validate the childlotid returned from database
                                                    if (string.IsNullOrEmpty(childLotId) == false)
                                                    {
                                                        //convert and store the childlotid into mergelot model
                                                        mergeLot.ChildLotId = Convert.ToInt32(childLotId);
                                                        //call getdevicebylot
                                                        var getChildDeviceOutput = BusinessLogic.DISSQLDataAccess.GetDeviceInfo(childLot, cOutput.Item2, ConfigurationManager.AppSettings["DTSEquipment"]);

                                                    //validate the getdevicebylot result for child lot
                                                        if (getChildDeviceOutput.Item1 == 0)
                                                    {
                                                        //convert and store the devices info into array
                                                        var cDeviceArray = getChildDeviceOutput.Item2.ToArray();
                                                        //get the devices quantity
                                                        var cDeviceQuantity = cDeviceArray.GetLength(0);
                                                        //store the individual device info into list
                                                        for (int i = 0; i < cDeviceQuantity;)
                                                        {
                                                            childDeviceList.Add(new DeviceInfo()
                                                            {
                                                                DeviceId = cDeviceArray[i].DeviceId,
                                                                AliasId = cDeviceArray[i].AliasId,
                                                                TargetSubstrateId = cDeviceArray[i].TargetSubstrateId,
                                                                TargetPositionId = cDeviceArray[i].TargetPositionId,
                                                                ToX = cDeviceArray[i].ToX,
                                                                ToY = cDeviceArray[i].ToY,
                                                                BinResult = cDeviceArray[i].Result,
                                                                Pick = cDeviceArray[i].Pick,
                                                                BinCode = cDeviceArray[i].BinCode,
                                                                BinDesc = cDeviceArray[i].BinDesc,
                                                            });
                                                            i++;
                                                        }
                                                        //convert the device info list into datatable
                                                        var cDTable = BusinessLogic.DTSSQLDataAccess.DTValidateDeviceInfo(childDeviceList,
                                                                      Convert.ToInt32(childLotId), childLot);
                                                        //insert deviceinfo to DB
                                                        var cDTResponse = BusinessLogic.DTSSQLDataAccess.InsertDeviceInfo(cDTable);
                                                        //validate result returned from database
                                                        if (string.IsNullOrEmpty(cDTResponse) == false && cDTResponse == "Success")
                                                        {
                                                            var childLot1Devices = BusinessLogic.DTSSQLDataAccess.GetDeviceInfo(childLot,
                                                                    Convert.ToInt32(childLotId), type);
                                                            //store child devices quantity in viewbag to show at view
                                                            mergeLot.ChildLot1Quantity = childLot1Devices.ToArray().GetLength(0);
                                                            //get all devices info and store into session
                                                            Session["MergeChildDeviceInfo"] = childLot1Devices;
                                                            #region Child lot 2
                                                            if (string.IsNullOrEmpty(childLot2) == false)
                                                            {
                                                                //call lotInfoExt for mother lot
                                                                var cOutput2 = BusinessLogic.MESSQLDataAccess.GetLotInfo(childLot2);
                                                                //validate the data returned from getlotinfo
                                                                if (cOutput2.Item1 == 0)
                                                                {
                                                                    mergeLot.ChildLot2Operation = cOutput2.Item2;
                                                                    //assign motherlotinfo after getting reply
                                                                    childLotInfos2.Add(new LotInfo()
                                                                    {
                                                                        LotNum = childLot2,
                                                                        Operation = cOutput2.Item2,
                                                                        LotQty1 = cOutput2.Item3,
                                                                        UserId = Session["User"].ToString()
                                                                    });
                                                                    // insert mother lot info to DB
                                                                    childLotId2 = BusinessLogic.DTSSQLDataAccess.InsertLotInfo(childLotInfos2);
                                                                    //validate the mother lotinfoid returned from database
                                                                    if (string.IsNullOrEmpty(childLotId2) == false)
                                                                    {
                                                                        //convert and store the mother lotinfoid into mergelot model
                                                                        mergeLot.ChildLotId2 = Convert.ToInt32(childLotId2);
                                                                        //get child lot 2 devices
                                                                        var getChildDeviceOutput2 = BusinessLogic.DISSQLDataAccess.GetDeviceInfo(childLot2, cOutput2.Item2, ConfigurationManager.AppSettings["DTSEquipment"]);

                                                                        //validate the getdevicebylot result
                                                                        if (getChildDeviceOutput2.Item1 == 0)
                                                                        {
                                                                            //store the devices info into array
                                                                            var cDeviceArray2 = getChildDeviceOutput2.Item2.ToArray();
                                                                            //get the devices quantity
                                                                            var cDeviceQuantity2 = cDeviceArray2.GetLength(0);
                                                                            //convert and store the individual device info into list
                                                                            for (int i = 0; i < cDeviceQuantity2;)
                                                                            {
                                                                                childDeviceList2.Add(new DeviceInfo()
                                                                                {
                                                                                    DeviceId = cDeviceArray2[i].DeviceId,
                                                                                    AliasId = cDeviceArray2[i].AliasId,
                                                                                    TargetSubstrateId = cDeviceArray2[i].TargetSubstrateId,
                                                                                    TargetPositionId = cDeviceArray2[i].TargetPositionId,
                                                                                    ToX = cDeviceArray2[i].ToX,
                                                                                    ToY = cDeviceArray2[i].ToY,
                                                                                    BinResult = cDeviceArray2[i].Result,
                                                                                    Pick = cDeviceArray2[i].Pick,
                                                                                    BinCode = cDeviceArray2[i].BinCode,
                                                                                    BinDesc = cDeviceArray2[i].BinDesc,
                                                                                });
                                                                                i++;
                                                                            }
                                                                            //convert the devices info list into datatable
                                                                            var dTable2 = BusinessLogic.DTSSQLDataAccess.DTValidateDeviceInfo(childDeviceList2,
                                                                                          Convert.ToInt32(childLotId2), childLot2);
                                                                            //insert deviceinfo to DB
                                                                            var dtResponse2 = BusinessLogic.DTSSQLDataAccess.InsertDeviceInfo(dTable2);
                                                                            //validate the result returned from database
                                                                            if (string.IsNullOrEmpty(dtResponse2) == false && dtResponse2 == "Success")
                                                                            {
                                                                                var childLot2Devices = BusinessLogic.DTSSQLDataAccess.GetDeviceInfo(childLot2,
                                                                                    Convert.ToInt32(childLotId2), type);
                                                                                //store mother devices quantity in viewbag to show at view
                                                                                mergeLot.ChildLot2Quantity = childLot2Devices.ToArray().GetLength(0);
                                                                                //get all the device info and store into session
                                                                                Session["MergeChildDeviceInfo2"] = childLot2Devices;

                                                                            }
                                                                            else
                                                                            {
                                                                                Alert("Error in saving child lot 2 devices info into database! Please try again later",
                                                                                    Notification.NotificationType.error, "Error");
                                                                                BusinessLogic.Log.WriteToErrorLogFile("Error in saving child lot 2 devices info into database!" +
                                                                                    " Please try again later");
                                                                                return View();
                                                                            }
                                                                        }
                                                                        else
                                                                        {
                                                                            Alert("Child Lot 2: " + getChildDeviceOutput2.Item3 + " Please contact Process Tech/Engineer!",
                                                                                Notification.NotificationType.error, "Error");
                                                                            BusinessLogic.Log.WriteToErrorLogFile("Child Lot 2: " + getChildDeviceOutput2.Item3 + 
                                                                                " Please contact Process Tech/Engineer!");
                                                                            return View();
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        Alert("Error in saving child lot 2 info to database! Please try again later!",
                                                                            Notification.NotificationType.error, "Error");
                                                                        BusinessLogic.Log.WriteToErrorLogFile("Error in saving child lot 2 info to database!" +
                                                                            " Please try again later!");
                                                                        return View();
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    Alert("Failed to get child lot 2 info from MES! Please try again later!",
                                                                        Notification.NotificationType.error, "Error");
                                                                    BusinessLogic.Log.WriteToErrorLogFile("Failed to get child lot 2 info from MES!" +
                                                                        " Please try again later!");
                                                                    return View();
                                                                }
                                                            }
                                                            #endregion
                                                            #region Child lot 3
                                                            if (string.IsNullOrEmpty(childLot3) == false)
                                                            {
                                                                //call lotInfoExt for mother lot
                                                                var cOutput3 = BusinessLogic.MESSQLDataAccess.GetLotInfo(childLot3);
                                                                //validate the data returned from getlotinfo
                                                                if (cOutput3.Item1 == 0)
                                                                {
                                                                    mergeLot.ChildLot3Operation = Convert.ToInt32(cOutput3.Item2);
                                                                    //assign motherlotinfo after getting reply
                                                                    childLotInfos3.Add(new LotInfo()
                                                                    {
                                                                        LotNum = childLot3,
                                                                        Operation = cOutput3.Item2,
                                                                        LotQty1 = cOutput3.Item3,
                                                                        UserId = Session["User"].ToString()
                                                                    });
                                                                    // insert mother lot info to DB
                                                                    childLotId3 = BusinessLogic.DTSSQLDataAccess.InsertLotInfo(childLotInfos3);
                                                                    //validate the mother lotinfoid returned from database
                                                                    if (string.IsNullOrEmpty(childLotId3) == false)
                                                                    {
                                                                        //convert and store the mother lotinfoid into mergelot model
                                                                        mergeLot.ChildLotId3 = Convert.ToInt32(childLotId3);
                                                                        //get mother lot devices
                                                                        var getChildDeviceOutput3 = BusinessLogic.DISSQLDataAccess.GetDeviceInfo(childLot3, cOutput3.Item2, ConfigurationManager.AppSettings["DTSEquipment"]);

                                                                        //validate the getdevicebylot result
                                                                        if (getChildDeviceOutput3.Item1 == 0)
                                                                        {
                                                                            //store the devices info into array
                                                                            var cDeviceArray3 = getChildDeviceOutput3.Item2.ToArray();
                                                                            //get the devices quantity
                                                                            var cDeviceQuantity3 = cDeviceArray3.GetLength(0);
                                                                            //convert and store the individual device info into list
                                                                            for (int i = 0; i < cDeviceQuantity3;)
                                                                            {
                                                                                childDeviceList3.Add(new DeviceInfo()
                                                                                {
                                                                                    DeviceId = cDeviceArray3[i].DeviceId,
                                                                                    AliasId = cDeviceArray3[i].AliasId,
                                                                                    TargetSubstrateId = cDeviceArray3[i].TargetSubstrateId,
                                                                                    TargetPositionId = cDeviceArray3[i].TargetPositionId,
                                                                                    ToX = cDeviceArray3[i].ToX,
                                                                                    ToY = cDeviceArray3[i].ToY,
                                                                                    BinResult = cDeviceArray3[i].Result,
                                                                                    Pick = cDeviceArray3[i].Pick,
                                                                                    BinCode = cDeviceArray3[i].BinCode,
                                                                                    BinDesc = cDeviceArray3[i].BinDesc,
                                                                                });
                                                                                i++;
                                                                            }
                                                                            //convert the devices info list into datatable
                                                                            var dTable3 = BusinessLogic.DTSSQLDataAccess.DTValidateDeviceInfo(childDeviceList3,
                                                                                          Convert.ToInt32(childLotId3), childLot3);
                                                                            //insert deviceinfo to DB
                                                                            var dtResponse3 = BusinessLogic.DTSSQLDataAccess.InsertDeviceInfo(dTable3);
                                                                            //validate the result returned from database
                                                                            if (string.IsNullOrEmpty(dtResponse3) == false && dtResponse3 == "Success")
                                                                            {
                                                                                var childLot3Devices = BusinessLogic.DTSSQLDataAccess.GetDeviceInfo(childLot3,
                                                                                    Convert.ToInt32(childLotId3), type);
                                                                                //store mother devices quantity in viewbag to show at view
                                                                                mergeLot.ChildLot3Quantity = childLot3Devices.ToArray().GetLength(0);
                                                                                //get all the device info and store into session
                                                                                Session["MergeChildDeviceInfo3"] = childLot3Devices;

                                                                            }
                                                                            else
                                                                            {
                                                                                Alert("Error in saving child lot 3 devices info into database! Please try again later",
                                                                                    Notification.NotificationType.error, "Error");
                                                                                BusinessLogic.Log.WriteToErrorLogFile("Error in saving child lot 3 devices info into database!" +
                                                                                    " Please try again later");
                                                                                return View();
                                                                            }
                                                                        }
                                                                        else
                                                                        {
                                                                            Alert("Child Lot 3: " + getChildDeviceOutput3.Item3 + " Please contact Process Tech/Engineer!",
                                                                                Notification.NotificationType.error, "Error");
                                                                            BusinessLogic.Log.WriteToErrorLogFile("Child Lot 3: " + getChildDeviceOutput3.Item3 +
                                                                                " Please contact Process Tech/Engineer!");
                                                                            return View();
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        Alert("Error in saving child lot 3 info to database! Please try again later!",
                                                                            Notification.NotificationType.error, "Error");
                                                                        BusinessLogic.Log.WriteToErrorLogFile("Error in saving child lot 3 info to database! Please try again later!");
                                                                        return View();
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    Alert("Failed to get child lot 3 info from MES! Please try again later!",
                                                                        Notification.NotificationType.error, "Error");
                                                                    BusinessLogic.Log.WriteToErrorLogFile("Failed to get child lot 3 info from MES! Please try again later!");
                                                                    return View();
                                                                }
                                                            }
                                                            #endregion
                                                            #region Child lot 4
                                                            if (string.IsNullOrEmpty(childLot4) == false)
                                                            {
                                                                //call lotInfoExt for mother lot
                                                                var cOutput4 = BusinessLogic.MESSQLDataAccess.GetLotInfo(childLot4);
                                                                //validate the data returned from getlotinfo
                                                                if (cOutput4.Item1 == 0)
                                                                {
                                                                    mergeLot.ChildLot4Operation = Convert.ToInt32(cOutput4.Item2);
                                                                    //assign motherlotinfo after getting reply
                                                                    childLotInfos4.Add(new LotInfo()
                                                                    {
                                                                        LotNum = childLot4,
                                                                        Operation = Convert.ToInt32(cOutput4.Item2),
                                                                        LotQty1 = cOutput4.Item3,
                                                                        UserId = Session["User"].ToString()
                                                                    });
                                                                    // insert mother lot info to DB
                                                                    childLotId4 = BusinessLogic.DTSSQLDataAccess.InsertLotInfo(childLotInfos4);
                                                                    //validate the mother lotinfoid returned from database
                                                                    if (string.IsNullOrEmpty(childLotId4) == false)
                                                                    {
                                                                        //convert and store the mother lotinfoid into mergelot model
                                                                        mergeLot.ChildLotId4 = Convert.ToInt32(childLotId4);
                                                                        //get mother lot devices
                                                                        var getChildDeviceOutput4 = BusinessLogic.DISSQLDataAccess.GetDeviceInfo(childLot4, cOutput4.Item2, ConfigurationManager.AppSettings["DTSEquipment"]);

                                                                        //validate the getdevicebylot result
                                                                        if (getChildDeviceOutput4.Item1 == 0)
                                                                        {
                                                                            //store the devices info into array
                                                                            var cDeviceArray4 = getChildDeviceOutput4.Item2.ToArray();
                                                                            //get the devices quantity
                                                                            var cDeviceQuantity4 = cDeviceArray4.GetLength(0);
                                                                            //convert and store the individual device info into list
                                                                            for (int i = 0; i < cDeviceQuantity4;)
                                                                            {
                                                                                childDeviceList4.Add(new DeviceInfo()
                                                                                {
                                                                                    DeviceId = cDeviceArray4[i].DeviceId,
                                                                                    AliasId = cDeviceArray4[i].AliasId,
                                                                                    TargetSubstrateId = cDeviceArray4[i].TargetSubstrateId,
                                                                                    TargetPositionId = cDeviceArray4[i].TargetPositionId,
                                                                                    ToX = cDeviceArray4[i].ToX,
                                                                                    ToY = cDeviceArray4[i].ToY,
                                                                                    BinResult = cDeviceArray4[i].Result,
                                                                                    Pick = cDeviceArray4[i].Pick,
                                                                                    BinCode = cDeviceArray4[i].BinCode,
                                                                                    BinDesc = cDeviceArray4[i].BinDesc,
                                                                                });
                                                                                i++;
                                                                            }
                                                                            //convert the devices info list into datatable
                                                                            var dTable4 = BusinessLogic.DTSSQLDataAccess.DTValidateDeviceInfo(childDeviceList4, Convert.ToInt32(childLotId4),
                                                                                          childLot4);
                                                                            //insert deviceinfo to DB
                                                                            var dtResponse4 = BusinessLogic.DTSSQLDataAccess.InsertDeviceInfo(dTable4);
                                                                            //validate the result returned from database
                                                                            if (string.IsNullOrEmpty(dtResponse4) == false && dtResponse4 == "Success")
                                                                            {
                                                                                var childLot4Devices = BusinessLogic.DTSSQLDataAccess.GetDeviceInfo(childLot4,
                                                                                    Convert.ToInt32(childLotId4), type);
                                                                                //store mother devices quantity in viewbag to show at view
                                                                                mergeLot.ChildLot4Quantity = childLot4Devices.ToArray().GetLength(0);
                                                                                //get all the device info and store into session
                                                                                Session["MergeChildDeviceInfo4"] = childLot4Devices;

                                                                            }
                                                                            else
                                                                            {
                                                                                Alert("Error in saving child lot 4 devices info into database! Please try again later",
                                                                                    Notification.NotificationType.error, "Error");
                                                                                BusinessLogic.Log.WriteToErrorLogFile("Error in saving child lot 4 devices info into database!" +
                                                                                    " Please try again later");
                                                                                return View();
                                                                            }
                                                                        }
                                                                        else
                                                                        {
                                                                           Alert("Child Lot 4: " + getChildDeviceOutput4.Item3 + " Please contact Process Tech/Engineer!",
                                                                               Notification.NotificationType.error, "Error");
                                                                           BusinessLogic.Log.WriteToErrorLogFile("Child Lot 4: " + getChildDeviceOutput4.Item3 +
                                                                               " Please contact Process Tech/Engineer!");
                                                                           return View();
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        Alert("Error in saving child lot 4 info to database! Please try again later!",
                                                                            Notification.NotificationType.error, "Error");
                                                                        BusinessLogic.Log.WriteToErrorLogFile("Error in saving child lot 4 info to database! Please try again later!");
                                                                        return View();
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    Alert("Failed to get child lot 4 info from MES! Please try again later!",
                                                                        Notification.NotificationType.error, "Error");
                                                                    BusinessLogic.Log.WriteToErrorLogFile("Failed to get child lot 4 info from MES! Please try again later!");
                                                                    return View();
                                                                }
                                                            }
                                                            #endregion
                                                            //store mother lot and child lot info into session
                                                            Session["TempLotInfos"] = mergeLot;

                                                            return RedirectToAction("LotGridView");
                                                        }
                                                        else
                                                        {
                                                            Alert("Error in saving child lot devices info into database! Please try again later",
                                                                Notification.NotificationType.error, "Error");
                                                            BusinessLogic.Log.WriteToErrorLogFile("Error in saving child lot devices info into database! Please try again later");
                                                            return View();
                                                        }
                                                    }
                                                        else
                                                        {
                                                            Alert("Child Lot: " + getChildDeviceOutput.Item3 + " Please contact Process Tech / Engineer!",
                                                                Notification.NotificationType.error, "Error");
                                                            BusinessLogic.Log.WriteToErrorLogFile("Mother Lot: " + getChildDeviceOutput.Item3 +
                                                                " Please contact Process Tech/Engineer!");
                                                            return View();
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Alert("Error in saving child lot info to database! Please try again later!", Notification.NotificationType.error, "Error");
                                                        BusinessLogic.Log.WriteToErrorLogFile("Error in saving child lot info to database! Please try again later!");
                                                        return View();
                                                    }
                                                }
                                                else
                                                {
                                                    Alert("Failed to get child lot info from MES! Please try again later!", Notification.NotificationType.error, "Error");
                                                    BusinessLogic.Log.WriteToErrorLogFile("Failed to get child lot info from MES! Please try again later!");
                                                    return View();
                                                }
                                                #endregion
                                            }
                                            else
                                            {
                                                Alert("Error in getting mother lot device info from database! Please try again later!", Notification.NotificationType.error, "Error");
                                                BusinessLogic.Log.WriteToErrorLogFile("Error in getting mother lot device info from database! Please try again later!");
                                                return View();
                                            }
                                        }
                                        else
                                        {
                                            Alert("Error in saving mother lot devices info into database! Please try again later", Notification.NotificationType.error, "Error");
                                            BusinessLogic.Log.WriteToErrorLogFile("Error in saving mother lot device info into database! Please try again later");
                                            return View();
                                        }
                                    }
                                    else
                                    {
                                        Alert("Mother Lot: " + getMotherDeviceOutput.Item3 + " Please contact Process Tech/Engineer!" ,
                                            Notification.NotificationType.error, "Error");
                                        BusinessLogic.Log.WriteToErrorLogFile("Mother Lot: " + getMotherDeviceOutput.Item3 + " Please contact Process Tech/Engineer!");
                                        return View();
                                    }
                                }
                                else
                                {
                                    Alert("Error in saving mother lot info to database! Please try again later!", Notification.NotificationType.error, "Error");
                                    BusinessLogic.Log.WriteToErrorLogFile("Error in saving mother lot info to database! Please try again later!");
                                    return View();
                                }
                            }
                            else
                            {
                                Alert("Failed to get mother lot info from MES! Please try again later!", Notification.NotificationType.error, "Error");
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
                    Alert("Mother Lot Number and Child Lot Number are same! Please enter correct Mother Lot Number or Child Lot Number!",
                        Notification.NotificationType.error, "Error");
                    BusinessLogic.Log.WriteToErrorLogFile("Mother Lot Number and Child Lot Number are same!" +
                        " Please enter correct Mother Lot Number or Child Lot Number!");
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
        public ActionResult LotGridView()
        {
            return View();
        }
        [CheckSessionTimeOut]
        [ValidateInput(false)]
        public ActionResult MotherLotGridViewPartial()
        {
            if (Session["MergeMotherDeviceInfo"] != null)
            {
                var tmpMotherDeviceInfos = (List<DisplayDeviceInfo>)Session["MergeMotherDeviceInfo"];
                return PartialView("_MotherLotGridViewPartial", tmpMotherDeviceInfos);
            }
            else
            {
                return PartialView("_MotherLotGridViewPartial");
            }
        }
        [CheckSessionTimeOut]
        [ValidateInput(false)]
        public ActionResult ChildLotGridViewPartial()
        {
            if (Session["MergeChildDeviceInfo"] != null)
            {
                var tmpChildDeviceInfos = (List<DisplayDeviceInfo>)Session["MergeChildDeviceInfo"];
                return PartialView("_ChildLotGridViewPartial", tmpChildDeviceInfos);
            }
            else
            {
                return PartialView("_ChildLotGridViewPartial");
            }
        }
        [CheckSessionTimeOut]
        [ValidateInput(false)]
        public ActionResult ChildLot2GridViewPartial()
        {
            //validate session data
            if (Session["MergeChildDeviceInfo2"] != null)
            {
                var tmpChildDeviceInfos2 = (List<DisplayDeviceInfo>)Session["MergeChildDeviceInfo2"];
                return PartialView("_ChildLot2GridViewPartial", tmpChildDeviceInfos2);
            }
            else
            {
                return PartialView("_ChildLot2GridViewPartial");
            }
        }
        [CheckSessionTimeOut]
        [ValidateInput(false)]
        public ActionResult ChildLot3GridViewPartial()
        {
            //validate session data
            if (Session["MergeChildDeviceInfo3"] != null)
            {
                var tmpChildDeviceInfos3 = (List<DisplayDeviceInfo>)Session["MergeChildDeviceInfo3"];
                return PartialView("_ChildLot3GridViewPartial", tmpChildDeviceInfos3);
            }
            else
            {
                return PartialView("_ChildLot3GridViewPartial");
            }
        }
        [CheckSessionTimeOut]
        [ValidateInput(false)]
        public ActionResult ChildLot4GridViewPartial()
        {
            //validate session data
            if (Session["MergeChildDeviceInfo4"] != null)
            {
                var tmpChildDeviceInfos4 = (List<DisplayDeviceInfo>)Session["MergeChildDeviceInfo4"];
                return PartialView("_ChildLot4GridViewPartial", tmpChildDeviceInfos4);
            }
            else
            {
                return PartialView("_ChildLot4GridViewPartial");
            }
        }
        [CheckSessionTimeOut]
        [HttpPost]
        public ActionResult MergeConfirm()
        {
            var lots = Session["TempLotInfos"] as MergeLot;
            string mLot = lots.MotherLotNumber.ToUpper();
            string cLot = lots.ChildLotNumber.ToUpper();
            string cLot2 = string.Empty;
            string cLot3 = string.Empty;
            string cLot4 = string.Empty;
            int mLotId = lots.MotherLotId;
            string errorMessage = string.Empty;

            if (string.IsNullOrEmpty(lots.ChildLotNumber2) == false)
            {
                cLot2 = lots.ChildLotNumber2.ToUpper();
            }

            if (string.IsNullOrEmpty(lots.ChildLotNumber3) == false)
            {
                cLot3 = lots.ChildLotNumber3.ToUpper();
            }

            if (string.IsNullOrEmpty(lots.ChildLotNumber4) == false)
            {
                cLot4 = lots.ChildLotNumber4.ToUpper();
            }

            try
            {
                //validate the mother lot number and child lot number
                if(string.IsNullOrEmpty(mLot) == false && string.IsNullOrEmpty(cLot) == false)
                {
                        //call merge MES web service
                        var mergeOutput = BusinessLogic.MESSQLDataAccess.MergeLot(mLot, cLot, cLot2, cLot3, cLot4);

                        //validate returned lot number
                        if (mergeOutput.Item1 == 0)
                        {
                                var mLotInfo = BusinessLogic.DTSSQLDataAccess.GetLotInfo(mLot, mLotId);

                                var mergeDevice = BusinessLogic.DISSQLDataAccess.MergeDevice(mLot, cLot, cLot2, cLot3, cLot4, lots.MotherLotOperation , ConfigurationManager.AppSettings["DTSEquipment"]);

                                if (mergeDevice.Item1 == 0)
                                {
                                    var dbMergeResponse = BusinessLogic.DTSSQLDataAccess.InsertMergeLot(mLot, cLot, cLot2, cLot3,
                                                          cLot4, lots.MotherLotOperation.ToString());

                                    if (string.IsNullOrEmpty(dbMergeResponse) == false)
                                    {
                                        //insert event into audit log
                                        //1 = Login
                                        //2 = Logout
                                        //3 = Split
                                        //4 = Merge
                                        //5 = Update
                                        //6 = BatchUpdate
                                        var dbAuditResponse = BusinessLogic.DTSSQLDataAccess.CreateAuditLog(Session["User"].ToString(), 4,
                                                              Convert.ToInt32(dbMergeResponse));

                                    if(string.IsNullOrEmpty(dbAuditResponse) == false)
                                    {
                                        //insert mother and child lot number into merge lot history
                                            Alert("Successful Merge Lot at MES and DTS!", Notification.NotificationType.success, "Success");
                                            Session.Remove("MergeMotherDeviceInfo");
                                            Session.Remove("MergeChildDeviceInfo");
                                            Session.Remove("MergeChildDeviceInfo2");
                                            Session.Remove("MergeChildDeviceInfo3");
                                            Session.Remove("MergeChildDeviceInfo4");
                                            Session.Remove("TempLotInfos");
                                            return RedirectToAction("MergeLotValidate");
                                        }
                                        else
                                        {
                                            Alert("Error saving audit log! Please verify with CIM Engineer!", Notification.NotificationType.error,
                                                "Error");
                                            BusinessLogic.Log.WriteToErrorLogFile("Error saving audit log! Please verify with CIM Engineer!");
                                            return RedirectToAction("MergeLotValidate");
                                        }
                                    }
                                    else
                                    {
                                        Alert("Error saving merge lot log! Please verify with CIM Engineer!", Notification.NotificationType.error,
                                            "Error");
                                        BusinessLogic.Log.WriteToErrorLogFile("Error saving merge lot log! Please verify with CIM Engineer!");
                                        return RedirectToAction("MergeLotValidate");
                                    }

                                }
                                else
                                {
                                        Alert("DTS Merge Devices: " + mergeDevice.Item2 + " Please contact Process Tech/Engineer!",
                                            Notification.NotificationType.error, "Error");
                                        BusinessLogic.Log.WriteToErrorLogFile("DTS Merge Devices: " + mergeDevice.Item2 +
                                            " Please contact Process Tech/Engineer!");
                                        return RedirectToAction("MergeLotValidate");
                                }
                            }
                        else
                        {
                            Alert("MES Merge transaction failed! Please try again later!", Notification.NotificationType.error, "Error");
                            BusinessLogic.Log.WriteToErrorLogFile("MES Merge transaction failed! Please try again later!");
                            return RedirectToAction("LotGridView");
                        }
                    }
                else
                {
                    Alert("No Mother Lot Number or Child Lot Number detected! Please re-validate the lot number!",
                        Notification.NotificationType.error, "Error");
                    BusinessLogic.Log.WriteToErrorLogFile("No Mother Lot Number or Child Lot Number detected!" +
                        " Please re-validate the lot number!");
                    return RedirectToAction("MergeLotValidate");
                }

            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Alert(errorMessage, Notification.NotificationType.error, "Error");
                return RedirectToAction("MergeLotValidate");
            }
        }
        [CheckSessionTimeOut]
        public ActionResult MergeCancel()
        {
            //clear all the session for this module
            Session.Remove("MergeMotherDeviceInfo");
            Session.Remove("MergeChildDeviceInfo");
            Session.Remove("MergeChildDeviceInfo2");
            Session.Remove("MergeChildDeviceInfo3");
            Session.Remove("MergeChildDeviceInfo4");
            Session.Remove("TempLotInfos");

            return RedirectToAction("MergeLotValidate");
        }
    }
}