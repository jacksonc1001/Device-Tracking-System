using Device_Tracking_System.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using static Device_Tracking_System.BusinessLogic.Filters;

namespace Device_Tracking_System.Controllers
{
    [Authorize]
    public class DMCValidationController : BaseController
    {
        // GET: DMCValidation
        #region Validate Passed Devices
        [CheckSessionTimeOut]
        public ActionResult PassedDeviceLotValidate()
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
        public ActionResult PassedDeviceLotValidate(LotValidation passedDeviceLotValidate)
        {
            string errorMessage;
            string pLotId;
            //convert the lot number to uppercase
            var lot = passedDeviceLotValidate.LotNumber.ToUpper();
            passedDeviceLotValidate.LotNumber = lot;
            var pLotInfoList = new List<LotInfo>();
            var pDeviceList = new List<DeviceInfo>();

            if (ModelState.IsValid)
            {
                try
                {
                    //call lotInfoExt
                    var output = BusinessLogic.MESSQLDataAccess.GetLotInfo(lot);
                    //validate the data returned from getlotinfo
                    if (output.Item1 == 0)
                    {
                        passedDeviceLotValidate.Operation = Convert.ToInt32(output.Item2);

                        //assign motherlotinfo after getting reply
                        pLotInfoList.Add(new LotInfo()
                        {
                            LotNum = lot,
                            Operation = output.Item2,
                            LotQty1 = output.Item3,
                            UserId = Session["User"].ToString()
                        });
                        //insert lotinfo to DB
                        pLotId = BusinessLogic.DTSSQLDataAccess.InsertLotInfo(pLotInfoList);
                        //validate the lotinfoid returned from database
                        if (string.IsNullOrEmpty(pLotId) == false)
                        {
                            //store the lotinfoid into lotvalidateion model
                            passedDeviceLotValidate.LotInfoId = pLotId;

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

                                    pDeviceList.Add(new DeviceInfo()
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
                                var dTable = BusinessLogic.DTSSQLDataAccess.DTValidateDeviceInfo(pDeviceList, Convert.ToInt32(pLotId), lot);
                                //insert deviceinfo to DB
                                var dtResponse = BusinessLogic.DTSSQLDataAccess.InsertDeviceInfo(dTable);
                                //validate result from database
                                if (string.IsNullOrEmpty(dtResponse) == false && dtResponse == "Success")
                                {
                                    var validateQuantity = BusinessLogic.DTSSQLDataAccess.GetnUpdateQuantityLotInfo(Convert.ToInt32(pLotId), "ValidatePass");

                                    if (validateQuantity != 0 && validateQuantity != -1)
                                    {
                                        passedDeviceLotValidate.Quantity = validateQuantity;

                                        //store the lot info into tempdata
                                        Session["ValidatePassLotInfo"] = passedDeviceLotValidate;

                                        return RedirectToAction("PassedDeviceGridView");
                                    }
                                    else if (validateQuantity == -1)
                                    {
                                        Alert("No pass device in the lot! Please verify with Process Tech/Engineer!", Notification.NotificationType.error,
                                            "Error");
                                        BusinessLogic.Log.WriteToErrorLogFile("No pass device in the lot! Please verify with Process Tech/Engineer!");
                                        return View();
                                    }
                                    else
                                    {
                                        Alert("Error to get the device quantity! Please try again later", Notification.NotificationType.error, "Error");
                                        BusinessLogic.Log.WriteToErrorLogFile("Error to get the device quantity! Please try again later");
                                        return View();
                                    }
                                }
                                else
                                {
                                    Alert("Error in saving devices info into database! Please try again later", Notification.NotificationType.error, "Error");
                                    BusinessLogic.Log.WriteToErrorLogFile("Error in saving devices info into database! Please try again later");
                                    return View();
                                }
                            }
                            else
                            {
                                Alert(getDeviceOutput.Item3 + " Please verify with Process Tech/Engineer!", Notification.NotificationType.error,
                                    "Error");
                                BusinessLogic.Log.WriteToErrorLogFile(getDeviceOutput.Item3 + " Please verify with Process Tech/Engineer!");
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
        public ActionResult PassedDeviceGridView()
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
        public ActionResult PassedDeviceGridViewPartial()
        {
            var lotInfoId = Convert.ToInt32((Session["ValidatePassLotInfo"] as LotValidation).LotInfoId);
            var operation = (Session["ValidatePassLotInfo"] as LotValidation).Operation;
            var scannedDeviceList = BusinessLogic.DTSSQLDataAccess.GetScannedDevice(lotInfoId, operation);
            (Session["ValidatePassLotInfo"] as LotValidation).ScannedQuantity = 0;

            if (scannedDeviceList != null)
            {
                var scannedDeviceArray = scannedDeviceList.ToArray();
                (Session["ValidatePassLotInfo"] as LotValidation).ScannedQuantity = scannedDeviceArray.GetLength(0);
                return PartialView("_PassedDeviceGridViewPartial", scannedDeviceList);
            }
            else
            {
                return PartialView("_PassedDeviceGridViewPartial");
            }
        }
        [CheckSessionTimeOut]
        public ActionResult GetDeviceIDPassedDevice(DeviceValidation passDevice)
        {
            string errorMessage;
            var scanDevice = passDevice.DeviceId;
            var lotInfoId = Convert.ToInt32((Session["ValidatePassLotInfo"] as LotValidation).LotInfoId);
            var operation = (Session["ValidatePassLotInfo"] as LotValidation).Operation;

            try
            {
                var result = BusinessLogic.DTSSQLDataAccess.ValidateScanDevice(scanDevice, lotInfoId, operation, "ValidatePass");

                if (result == "1")
                {
                    ModelState.Clear();
                    Alert("The scanned device is FAIL device!", Notification.NotificationType.warning, "Warning");
                    return View("PassedDeviceGridView");
                }
                else if (result == "2")
                {
                    ModelState.Clear();
                    Alert("The scanned device is not belong to the lot!", Notification.NotificationType.warning, "Warning");
                    return View("PassedDeviceGridView");
                }
                else if (result == "3")
                {
                    ModelState.Clear();
                    Alert("The scanned device is already been scanned previously! Please verify!", Notification.NotificationType.warning, "Warning");
                    return View("PassedDeviceGridView");
                }
                else if (result == "4")
                {
                    ModelState.Clear();
                    Alert("Scanned devices quantity is exceeded pass device quantity. Please verify!", Notification.NotificationType.warning, "Warning");
                    return View("PassedDeviceGridView");
                }
                else if (result == "5")
                {
                    ModelState.Clear();
                    Alert("The scanned device is different bin!", Notification.NotificationType.warning, "Warning");
                    return View("PassedDeviceGridView");
                }
                else
                {
                    ModelState.Clear();
                    return View("PassedDeviceGridView");
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Alert(errorMessage, Notification.NotificationType.error, "Error");
                BusinessLogic.Log.WriteToErrorLogFile(errorMessage);
                return View("PassedDeviceGridView");
            }
        }
        [CheckSessionTimeOut]
        public ActionResult PassedDeviceConfirm()
        {
            string errorMessage;
            try
            {
                var lotInfoId = Convert.ToInt32((Session["ValidatePassLotInfo"] as LotValidation).LotInfoId);
                var operation = (Session["ValidatePassLotInfo"] as LotValidation).Operation;
                var scannedQuantity = (Session["ValidatePassLotInfo"] as LotValidation).ScannedQuantity;
                var packageQuantity = (Session["ValidatePassLotInfo"] as LotValidation).Quantity;
                string lotNum = (Session["ValidatePassLotInfo"] as LotValidation).LotNumber.ToUpper();
                string transactionType = "ValidatePass";


                var validationResult = BusinessLogic.DTSSQLDataAccess.ValidateScanDeviceResult(lotInfoId, operation, transactionType);

                if (validationResult == null)
                {
                    Alert("Error in validating the devices! Please contact with CIM Engineer!", Notification.NotificationType.error, "Error");
                    BusinessLogic.Log.WriteToErrorLogFile("Error in validating the devices! Please contact with CIM Engineer!");
                    return RedirectToAction("PassedDeviceGridView");
                }

                var vLogId = BusinessLogic.DTSSQLDataAccess.GetValidationResultLogID(lotInfoId, operation);

                if (vLogId != 0)
                {
                    //insert event into audit log
                    //1 = Login
                    //2 = Logout
                    //3 = Split
                    //4 = Merge
                    //5 = Update
                    //6 = BatchUpdate
                    //7 = SplitStandard
                    //8 = SplitNonStandard
                    //9 = SplitORT
                    //10 = Validate Pass Device
                    //11 = Validate Fail Device
                    //12 = Validate Sampling Device
                    var dbAuditResponse = BusinessLogic.DTSSQLDataAccess.CreateAuditLog(Session["User"].ToString(), 10, vLogId);

                    if (string.IsNullOrEmpty(dbAuditResponse) == false)
                    {
                        if (string.IsNullOrEmpty(validationResult) == false && validationResult == "PASS")
                        {

                            var insertResult = BusinessLogic.MESSQLDataAccess.InsertValidationResult(lotNum, "PASS");

                            if (insertResult.Item1 == 0)
                            {
                                var cResult = BusinessLogic.DTSSQLDataAccess.CopyScannedData(lotInfoId);

                                if (string.IsNullOrEmpty(cResult) == false && cResult == "0")
                                {
                                    Alert("Lot Successful Validated! Please proceed to MVOU at MES!", Notification.NotificationType.success, "Success");
                                    //clear session data in this module
                                    Session.Remove("ValidatePassLotInfo");

                                    return RedirectToAction("PassedDeviceLotValidate");
                                }
                                else
                                {
                                    Alert("Fail to delete scanned data at database! Please try again later!", Notification.NotificationType.error, "Error");
                                    BusinessLogic.Log.WriteToErrorLogFile("Fail to delete scanned data at database! Please try again later!");
                                    return RedirectToAction("PassedDeviceGridView");
                                }
                            }
                            else
                            {
                                Alert("Insert validation result to MES failed! Please try again later!", Notification.NotificationType.error, "Error");
                                BusinessLogic.Log.WriteToErrorLogFile("Insert validation result to MES failed! Please try again later!");
                                return RedirectToAction("PassedDeviceGridView");
                            }
                        }
                        else if (validationResult == "FAIL F")
                        {

                            var insertResult = BusinessLogic.MESSQLDataAccess.InsertValidationResult(lotNum, "FAIL");

                            if (insertResult.Item1 == 0)
                            {
                                var dResult = BusinessLogic.DTSSQLDataAccess.CopyScannedData(lotInfoId);

                                if (string.IsNullOrEmpty(dResult) == false && dResult == "0")
                                {
                                    Alert("Validation FAILED due to some scanned device are fail device or alien device! Please verify and redo the transaction!",
                                        Notification.NotificationType.error, "Validation FAILED");
                                    BusinessLogic.Log.WriteToErrorLogFile("Validation FAILED due to some scanned device are fail device!" +
                                        " Please verify and redo the transaction!");
                                    Session.Remove("ValidatePassLotInfo");
                                    return RedirectToAction("PassedDeviceLotValidate");
                                }
                                else
                                {
                                    Alert("Fail to delete scanned data at database! Please try again later!", Notification.NotificationType.error, "Error");
                                    BusinessLogic.Log.WriteToErrorLogFile("Fail to delete scanned data at database! Please try again later!");
                                    return RedirectToAction("PassedDeviceGridView");
                                }
                            }
                            else
                            {
                                Alert("Insert validation result to MES failed! Please try again later!", Notification.NotificationType.error, "Error");
                                BusinessLogic.Log.WriteToErrorLogFile("Insert validation result to MES failed! Please try again later!");
                                return RedirectToAction("PassedDeviceGridView");
                            }
                        }
                        else if (validationResult == "FAIL B")
                        {
                            var insertResult = BusinessLogic.MESSQLDataAccess.InsertValidationResult(lotNum, "FAIL");

                            if (insertResult.Item1 == 0)
                            {
                                var dResult = BusinessLogic.DTSSQLDataAccess.CopyScannedData(lotInfoId);

                                if (string.IsNullOrEmpty(dResult) == false && dResult == "0")
                                {
                                    Alert("Validation FAILED due to some scanned device are different bin! Please verify and redo the transaction!",
                                        Notification.NotificationType.error, "Validation FAILED");
                                    BusinessLogic.Log.WriteToErrorLogFile("Validation FAILED due to some scanned device are different bin! " +
                                        "Please verify and redo the transaction!");
                                    Session.Remove("ValidatePassLotInfo");
                                    return RedirectToAction("PassedDeviceLotValidate");
                                }
                                else
                                {
                                    Alert("Fail to delete scanned data at database! Please try again later!", Notification.NotificationType.error, "Error");
                                    BusinessLogic.Log.WriteToErrorLogFile("Fail to delete scanned data at database! Please try again later!");
                                    return RedirectToAction("PassedDeviceGridView");
                                }
                            }
                            else
                            {
                                Alert("Insert validation result to MES failed! Please try again later!", Notification.NotificationType.error, "Error");
                                BusinessLogic.Log.WriteToErrorLogFile("Insert validation result to MES failed! Please try again later!");
                                return RedirectToAction("PassedDeviceGridView");
                            }
                        }
                        else if (validationResult == "FAIL Q")
                        {

                            var insertResult = BusinessLogic.MESSQLDataAccess.InsertValidationResult(lotNum, "FAIL");

                            if (insertResult.Item1 == 0)
                            {
                                var dResult = BusinessLogic.DTSSQLDataAccess.CopyScannedData(lotInfoId);

                                if (string.IsNullOrEmpty(dResult) == false && dResult == "0")
                                {
                                    Alert("Validation FAILED due to scanned device quantity (" + scannedQuantity + ") not match with pass device quantity (" +
                                        packageQuantity + ")! Please verify and redo the transaction!",
                                        Notification.NotificationType.error, "Validation FAILED");
                                    BusinessLogic.Log.WriteToErrorLogFile("Validation FAILED due to scanned device quantity (" + scannedQuantity + ")" +
                                        " not match with pass device quantity (" + packageQuantity + ")! Please verify and redo the transaction!");
                                    Session.Remove("ValidatePassLotInfo");
                                    return RedirectToAction("PassedDeviceLotValidate");
                                }
                                else
                                {
                                    Alert("Fail to delete scanned data at database! Please try again later!", Notification.NotificationType.error, "Error");
                                    BusinessLogic.Log.WriteToErrorLogFile("Fail to delete scanned data at database! Please try again later!");
                                    return RedirectToAction("PassedDeviceGridView");
                                }
                            }
                            else
                            {
                                Alert("Insert validation result to MES failed! Please try again later!", Notification.NotificationType.error, "Error");
                                BusinessLogic.Log.WriteToErrorLogFile("Insert validation result to MES failed! Please try again later!");
                                return RedirectToAction("PassedDeviceGridView");
                            }
                        }
                        else
                        {

                            var insertResult = BusinessLogic.MESSQLDataAccess.InsertValidationResult(lotNum, "FAIL");

                            if (insertResult.Item1 == 0)
                            {
                                var dResult = BusinessLogic.DTSSQLDataAccess.CopyScannedData(lotInfoId);

                                if (string.IsNullOrEmpty(dResult) == false && dResult == "0")
                                {
                                    Alert("Validation FAILED due to unknown error! Please call CIM Engineer!", Notification.NotificationType.error,
                                        "Validation FAILED");
                                    BusinessLogic.Log.WriteToErrorLogFile("Validation FAILED due to unknown error! Please call CIM Engineer!");
                                    Session.Remove("ValidatePassLotInfo");
                                    return RedirectToAction("PassedDeviceLotValidate");
                                }
                                else
                                {
                                    Alert("Fail to delete scanned data at database! Please try again later!", Notification.NotificationType.error, "Error");
                                    BusinessLogic.Log.WriteToErrorLogFile("Fail to delete scanned data at database! Please try again later!");
                                    return RedirectToAction("PassedDeviceGridView");
                                }
                            }
                            else
                            {
                                Alert("Insert validation result to MES failed! Please try again later!", Notification.NotificationType.error, "Error");
                                BusinessLogic.Log.WriteToErrorLogFile("Insert validation result to MES failed! Please try again later!");
                                return RedirectToAction("PassedDeviceGridView");
                            }
                        }
                    }
                    else
                    {
                        Alert("Error in saving audit log! Please contact with CIM Engineer!", Notification.NotificationType.error, "Error");
                        BusinessLogic.Log.WriteToErrorLogFile("Error in saving audit log! Please contact with CIM Engineer!");
                        return RedirectToAction("PassedDeviceGridView");
                    }
                }
                else
                {
                    Alert("Error getting validation log ID! Please contact with CIM Engineer!", Notification.NotificationType.error, "Error");
                    BusinessLogic.Log.WriteToErrorLogFile("Error getting validation log ID! Please contact with CIM Engineer!");
                    return RedirectToAction("PassedDeviceGridView");
                }
             }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Alert(errorMessage, Notification.NotificationType.error, "Error");
                BusinessLogic.Log.WriteToErrorLogFile(errorMessage);
                return RedirectToAction("PassedDeviceGridView");
            }
        }
        [CheckSessionTimeOut]
        public ActionResult PassedDeviceCancel()
        {
            var dResult = BusinessLogic.DTSSQLDataAccess.DeleteScannedData(Convert.ToInt32((Session["ValidatePassLotInfo"] as LotValidation).LotInfoId),
                (Session["ValidatePassLotInfo"] as LotValidation).Operation);

            if (string.IsNullOrEmpty(dResult) == false && dResult == "0")
            {
                Session.Remove("ValidatePassLotInfo");
                return RedirectToAction("PassedDeviceLotValidate");
            }
            else
            {
                Alert("Fail to delete scanned data at database! Please try again!", Notification.NotificationType.error, "Error");
                BusinessLogic.Log.WriteToErrorLogFile("Fail to delete scanned data at database! Please try again!");
                return RedirectToAction("PassedDeviceGridView");
            }
        }
        #endregion

        #region Validate Failed Devices
        [CheckSessionTimeOut]
        public ActionResult FailedDeviceLotValidate()
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
        public ActionResult FailedDeviceLotValidate(LotValidation failedDeviceLotValidation)
        {
            string errorMessage = string.Empty;
            string fLotId = string.Empty;
            //convert the lot number to uppercase
            var lot = failedDeviceLotValidation.LotNumber.ToUpper();
            failedDeviceLotValidation.LotNumber = lot;
            var fLotInfoList = new List<LotInfo>();
            var fDeviceList = new List<DeviceInfo>();

            if (ModelState.IsValid)
            {
                try
                {
                    //call lotInfoExt
                    var output = BusinessLogic.MESSQLDataAccess.GetLotInfo(lot);
                    //validate the data returned from getlotinfo
                    if (output.Item1 == 0)
                    {
                        failedDeviceLotValidation.Operation = Convert.ToInt32(output.Item2);

                        //assign motherlotinfo after getting reply
                        fLotInfoList.Add(new LotInfo()
                        {
                            LotNum = lot,
                            Operation = output.Item2,
                            LotQty1 = output.Item3,
                            UserId = Session["User"].ToString()
                        });
                        //insert lotinfo to DB
                        fLotId = BusinessLogic.DTSSQLDataAccess.InsertLotInfo(fLotInfoList);
                        //validate the lotinfoid returned from database
                        if (string.IsNullOrEmpty(fLotId) == false)
                        {
                            //store the lotinfoid into lotvalidateion model
                            failedDeviceLotValidation.LotInfoId = fLotId;

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

                                    fDeviceList.Add(new DeviceInfo()
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
                                var dTable = BusinessLogic.DTSSQLDataAccess.DTValidateDeviceInfo(fDeviceList, Convert.ToInt32(fLotId), lot);
                                //insert deviceinfo to DB
                                var dtResponse = BusinessLogic.DTSSQLDataAccess.InsertDeviceInfo(dTable);
                                //validate result from database
                                if (string.IsNullOrEmpty(dtResponse) == false && dtResponse == "Success")
                                {
                                    var validateQuantity = BusinessLogic.DTSSQLDataAccess.GetnUpdateQuantityLotInfo(Convert.ToInt32(fLotId), "ValidateFail");

                                    if (validateQuantity != 0 && validateQuantity != -1)
                                    {
                                        failedDeviceLotValidation.Quantity = validateQuantity;

                                        //get all devices info and store in session
                                        //Session["SplitDeviceInfo"] = BusinessLogic.DTSSQLDataAccess.GetDeviceInfo(deviceArray[0].LotName, Convert.ToInt32(sLotId));
                                        //store the lot info into tempdata
                                        Session["ValidateFailLotInfo"] = failedDeviceLotValidation;

                                        return RedirectToAction("FailedDeviceGridView");
                                    }
                                    else if (validateQuantity == -1)
                                    {
                                        Alert("No fail device in the lot! Please verify with Process Tech/Engineer!", Notification.NotificationType.error,
                                            "Error");
                                        BusinessLogic.Log.WriteToErrorLogFile("No fail device in the lot! Please verify with Process Tech/Engineer!");
                                        return View();
                                    }
                                    else
                                    {
                                        Alert("Error to get the device quantity! Please try again later", Notification.NotificationType.error, "Error");
                                        BusinessLogic.Log.WriteToErrorLogFile("Error to get the device quantity! Please try again later");
                                        return View();
                                    }
                                }
                                else
                                {
                                    Alert("Error in saving devices info into database! Please try again later", Notification.NotificationType.error, "Error");
                                    BusinessLogic.Log.WriteToErrorLogFile("Error in saving devices info into database! Please try again later");
                                    return View();
                                }
                            }
                            else
                            {
                                Alert(getDeviceOutput.Item3 + " Please verify with Process Tech/Engineer!", Notification.NotificationType.error,
                                    "Error");
                                BusinessLogic.Log.WriteToErrorLogFile(getDeviceOutput.Item3 + " Please verify with Process Tech/Engineer!");
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
        public ActionResult FailedDeviceGridView()
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
        public ActionResult FailedDeviceGridViewPartial()
        {
            var lotInfoId = Convert.ToInt32((Session["ValidateFailLotInfo"] as LotValidation).LotInfoId);
            var operation = (Session["ValidateFailLotInfo"] as LotValidation).Operation;
            var scannedDeviceList = BusinessLogic.DTSSQLDataAccess.GetScannedDevice(lotInfoId, operation);
            (Session["ValidateFailLotInfo"] as LotValidation).ScannedQuantity = 0;

            if (scannedDeviceList != null)
            {
                var scannedDeviceArray = scannedDeviceList.ToArray();
                (Session["ValidateFailLotInfo"] as LotValidation).ScannedQuantity = scannedDeviceArray.GetLength(0);
                return PartialView("_FailedDeviceGridViewPartial", scannedDeviceList);
            }
            else
            {
                return PartialView("_FailedDeviceGridViewPartial");
            }
        }
        [CheckSessionTimeOut]
        public ActionResult GetDeviceIDFailedDevice(DeviceValidation passDevice)
        {
            string errorMessage;
            var scanDevice = passDevice.DeviceId;
            var lotInfoId = Convert.ToInt32((Session["ValidateFailLotInfo"] as LotValidation).LotInfoId);
            var operation = (Session["ValidateFailLotInfo"] as LotValidation).Operation;

            try
            {
                var result = BusinessLogic.DTSSQLDataAccess.ValidateScanDevice(scanDevice, lotInfoId, operation, "ValidateFail");

                if (result == "1")
                {
                    ModelState.Clear();
                    Alert("The scanned device is PASS device!", Notification.NotificationType.warning, "Warning");
                    return View("FailedDeviceGridView");
                }
                else if (result == "2")
                {
                    ModelState.Clear();
                    Alert("The scanned device is not belong to the lot!", Notification.NotificationType.warning, "Warning");
                    return View("FailedDeviceGridView");
                }
                else if (result == "3")
                {
                    ModelState.Clear();
                    Alert("The scanned device is already been scanned previously! Please verify!", Notification.NotificationType.warning, "Warning");
                    return View("FailedDeviceGridView");
                }
                else if (result == "4")
                {
                    ModelState.Clear();
                    Alert("Scanned devices quantity is exceeded fail device quantity. Please verify!", Notification.NotificationType.warning, "Warning");
                    return View("FailedDeviceGridView");
                }
                else if (result == "5")
                {
                    ModelState.Clear();
                    Alert("The scanned device is different bin!", Notification.NotificationType.warning, "Warning");
                    return View("FailedDeviceGridView");
                }
                else
                {
                    ModelState.Clear();
                    return View("FailedDeviceGridView");
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Alert(errorMessage, Notification.NotificationType.error, "Error");
                BusinessLogic.Log.WriteToErrorLogFile(errorMessage);
                return View("FailedDeviceGridView");
            }
        }
        [CheckSessionTimeOut]
        public ActionResult FailedDeviceConfirm()
        {
            string errorMessage;
            try
            {
                var lotInfoId = Convert.ToInt32((Session["ValidateFailLotInfo"] as LotValidation).LotInfoId);
                var operation = (Session["ValidateFailLotInfo"] as LotValidation).Operation;
                var scannedQuantity = (Session["ValidateFailLotInfo"] as LotValidation).ScannedQuantity;
                var packageQuantity = (Session["ValidateFailLotInfo"] as LotValidation).Quantity;
                string lotNum = (Session["ValidateFailLotInfo"] as LotValidation).LotNumber.ToUpper();
                string transactionType = "ValidateFail";


                var validationResult = BusinessLogic.DTSSQLDataAccess.ValidateScanDeviceResult(lotInfoId, operation, transactionType);

                if (validationResult == null)
                {
                    Alert("Error in validating the devices! Please contact with CIM Engineer!", Notification.NotificationType.error, "Error");
                    BusinessLogic.Log.WriteToErrorLogFile("Error in validating the devices! Please contact with CIM Engineer!");
                    return RedirectToAction("FailedDeviceGridView");
                }

                var vLogId = BusinessLogic.DTSSQLDataAccess.GetValidationResultLogID(lotInfoId, operation);

                if (vLogId != 0)
                {
                    //insert event into audit log
                    //1 = Login
                    //2 = Logout
                    //3 = Split
                    //4 = Merge
                    //5 = Update
                    //6 = BatchUpdate
                    //7 = SplitStandard
                    //8 = SplitNonStandard
                    //9 = SplitORT
                    //10 = Validate Pass Device
                    //11 = Validate Fail Device
                    //12 = Validate Sampling Device
                    var dbAuditResponse = BusinessLogic.DTSSQLDataAccess.CreateAuditLog(Session["User"].ToString(), 11, vLogId);

                    if (string.IsNullOrEmpty(dbAuditResponse) == false)
                    {
                        if (string.IsNullOrEmpty(validationResult) == false && validationResult == "PASS")
                        {
                            var insertResult = BusinessLogic.MESSQLDataAccess.InsertValidationResult(lotNum, "PASS");

                            if (insertResult.Item1 == 0)
                            {
                                var cResult = BusinessLogic.DTSSQLDataAccess.CopyScannedData(lotInfoId);

                                if (string.IsNullOrEmpty(cResult) == false && cResult == "0")
                                {
                                    Alert("Lot Successful Validated! Please proceed to MVOU at MES!", Notification.NotificationType.success, "Success");
                                    //clear session data in this module
                                    Session.Remove("ValidateFailLotInfo");

                                    return RedirectToAction("FailedDeviceLotValidate");
                                }
                                else
                                {
                                    Alert("Fail to delete scanned data at database! Please try again later!", Notification.NotificationType.error, "Error");
                                    BusinessLogic.Log.WriteToErrorLogFile("Fail to delete scanned data at database! Please try again later!");
                                    return RedirectToAction("FailedDeviceGridView");
                                }
                            }
                            else
                            {
                                Alert("Insert validation result to MES failed! Please try again later!", Notification.NotificationType.error, "Error");
                                BusinessLogic.Log.WriteToErrorLogFile("Insert validation result to MES failed! Please try again later!");
                                return RedirectToAction("FailedDeviceGridView");
                            }
                        }
                        else if (validationResult == "FAIL F")
                        {
                            var insertResult = BusinessLogic.MESSQLDataAccess.InsertValidationResult(lotNum, "FAIL");

                            if (insertResult.Item1 == 0 )
                            {
                                var dResult = BusinessLogic.DTSSQLDataAccess.CopyScannedData(lotInfoId);

                                if (string.IsNullOrEmpty(dResult) == false && dResult == "0")
                                {
                                    Alert("Validation FAILED due to some scanned device are pass device or alien device! Please verify and redo the transaction!",
                                        Notification.NotificationType.error, "Validation FAILED");
                                    BusinessLogic.Log.WriteToErrorLogFile("Validation FAILED due to some scanned device are fail device! " +
                                        "Please verify and redo the transaction!");
                                    Session.Remove("ValidateFailLotInfo");
                                    return RedirectToAction("FailedDeviceLotValidate");
                                }
                                else
                                {
                                    Alert("Fail to delete scanned data at database! Please try again later!", Notification.NotificationType.error, "Error");
                                    BusinessLogic.Log.WriteToErrorLogFile("Fail to delete scanned data at database! Please try again later!");
                                    return RedirectToAction("FailedDeviceGridView");
                                }
                            }
                            else
                            {
                                Alert("Insert validation result to MES failed! Please try again later!", Notification.NotificationType.error, "Error");
                                BusinessLogic.Log.WriteToErrorLogFile("Insert validation result to MES failed! Please try again later!");
                                return RedirectToAction("FailedDeviceGridView");
                            }
                        }
                        else if (validationResult == "FAIL B")
                        {
                            var insertResult = BusinessLogic.MESSQLDataAccess.InsertValidationResult(lotNum, "FAIL");

                            if (insertResult.Item1 == 0)
                            {
                                var dResult = BusinessLogic.DTSSQLDataAccess.CopyScannedData(lotInfoId);

                                if (string.IsNullOrEmpty(dResult) == false && dResult == "0")
                                {
                                    Alert("Validation FAILED due to some scanned device are different bin! Please verify and redo the transaction!",
                                        Notification.NotificationType.error, "Validation FAILED");
                                    BusinessLogic.Log.WriteToErrorLogFile("Validation FAILED due to some scanned device are different bin! " +
                                        "Please verify and redo the transaction!");
                                    Session.Remove("ValidateFailLotInfo");
                                    return RedirectToAction("FailedDeviceLotValidate");
                                }
                                else
                                {
                                    Alert("Fail to delete scanned data at database! Please try again later!", Notification.NotificationType.error, "Error");
                                    BusinessLogic.Log.WriteToErrorLogFile("Fail to delete scanned data at database! Please try again later!");
                                    return RedirectToAction("FailedDeviceGridView");
                                }
                            }
                            else
                            {
                                Alert("Insert validation result to MES failed! Please try again later!", Notification.NotificationType.error, "Error");
                                BusinessLogic.Log.WriteToErrorLogFile("Insert validation result to MES failed! Please try again later!");
                                return RedirectToAction("FailedDeviceGridView");
                            }
                        }
                        else if (validationResult == "FAIL Q")
                        {
                                var insertResult = BusinessLogic.MESSQLDataAccess.InsertValidationResult(lotNum, "FAIL");

                            if (insertResult.Item1 == 0)
                            {
                                var dResult = BusinessLogic.DTSSQLDataAccess.CopyScannedData(lotInfoId);

                                if (string.IsNullOrEmpty(dResult) == false && dResult == "0")
                                {
                                    Alert("Validation FAILED due to scanned device quantity (" + scannedQuantity + ") not match with fail device quantity (" +
                                        packageQuantity + ")! Please verify and redo the transaction!", Notification.NotificationType.error, "Validation FAILED");
                                    BusinessLogic.Log.WriteToErrorLogFile("Validation FAILED due to scanned device quantity (" + scannedQuantity +
                                        ") not match with fail device quantity (" + packageQuantity + ")! Please verify and redo the transaction!");
                                    Session.Remove("ValidateFailLotInfo");
                                    return RedirectToAction("FailedDeviceLotValidate");
                                }
                                else
                                {
                                    Alert("Fail to delete scanned data at database! Please try again later!", Notification.NotificationType.error, "Error");
                                    BusinessLogic.Log.WriteToErrorLogFile("Fail to delete scanned data at database! Please try again later!");
                                    return RedirectToAction("FailedDeviceGridView");
                                }
                            }
                            else
                            {
                                Alert("Insert validation result to MES failed! Please try again later!", Notification.NotificationType.error, "Error");
                                BusinessLogic.Log.WriteToErrorLogFile("Insert validation result to MES failed! Please try again later!");
                                return RedirectToAction("FailedDeviceGridView");
                            }
                        }
                        else
                        {
                           var insertResult = BusinessLogic.MESSQLDataAccess.InsertValidationResult(lotNum, "FAIL");

                            if (insertResult.Item1 == 0)
                            {
                                var dResult = BusinessLogic.DTSSQLDataAccess.CopyScannedData(lotInfoId);

                                if (string.IsNullOrEmpty(dResult) == false && dResult == "0")
                                {
                                    Alert("Validation FAILED due to unknown error! Please call CIM Engineer!", Notification.NotificationType.error,
                                        "Validation FAILED");
                                    BusinessLogic.Log.WriteToErrorLogFile("Validation FAILED due to unknown error! Please call CIM Engineer!");
                                    Session.Remove("ValidateFailLotInfo");
                                    return RedirectToAction("FailedDeviceLotValidate");
                                }
                                else
                                {
                                    Alert("Fail to delete scanned data at database! Please try again later!", Notification.NotificationType.error, "Error");
                                    BusinessLogic.Log.WriteToErrorLogFile("Fail to delete scanned data at database! Please try again later!");
                                    return RedirectToAction("FailedDeviceGridView");
                                }
                            }
                            else
                            {
                                Alert("Insert validation result to MES failed! Please try again later!", Notification.NotificationType.error, "Error");
                                BusinessLogic.Log.WriteToErrorLogFile("Insert validation result to MES failed! Please try again later!");
                                return RedirectToAction("FailedDeviceGridView");
                            }
                        }
                    }
                    else
                    {
                        Alert("Error in saving audit log! Please contact with CIM Engineer!", Notification.NotificationType.error, "Error");
                        BusinessLogic.Log.WriteToErrorLogFile("Error in saving audit log! Please contact with CIM Engineer!");
                        return RedirectToAction("FailedDeviceGridView");
                    }
                }
                else
                {
                    Alert("Error getting validation log ID! Please contact with CIM Engineer!", Notification.NotificationType.error, "Error");
                    BusinessLogic.Log.WriteToErrorLogFile("Error getting validation log ID! Please contact with CIM Engineer!");
                    return RedirectToAction("FailedDeviceGridView");
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Alert(errorMessage, Notification.NotificationType.error, "Error");
                BusinessLogic.Log.WriteToErrorLogFile(errorMessage);
                return RedirectToAction("FailedDeviceGridView");
            }
        }
        [CheckSessionTimeOut]
        public ActionResult FailedDeviceCancel()
        {
            var dResult = BusinessLogic.DTSSQLDataAccess.DeleteScannedData(Convert.ToInt32((Session["ValidateFailLotInfo"] as LotValidation).LotInfoId),
                (Session["ValidateFailLotInfo"] as LotValidation).Operation);

            if (string.IsNullOrEmpty(dResult) == false && dResult == "0")
            {
                Session.Remove("ValidateFailLotInfo");
                return RedirectToAction("FailedDeviceLotValidate");
            }
            else
            {
                Alert("Fail to delete scanned data at database! Please try again!", Notification.NotificationType.error, "Error");
                BusinessLogic.Log.WriteToErrorLogFile("Fail to delete scanned data at database! Please try again!");
                return RedirectToAction("FailedDeviceGridView");
            }
        }
        #endregion
    }
}