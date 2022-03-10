using Device_Tracking_System.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

/*
* Author: Jackson
* Date: 08/03/2022
* Version: 1.0.0.0
* Objective: Maintain all the DTS database connection and transaction
*/
namespace Device_Tracking_System.BusinessLogic
{
    public class DTSSQLDataAccess
    {
        private static readonly string strDTOSConnString = ConfigurationManager.ConnectionStrings["dbDTSConn"].ConnectionString;

        #region Datable
        public static DataTable DTValidateDeviceInfo(List<DeviceInfo> devices, int lotInfoId, string lotNum)
        {
            int tmpLotInfoId = lotInfoId;
            string tmpLotNum = lotNum;
            var dt = new DataTable();
            dt.Columns.Add("LotInfoId");
            dt.Columns.Add("DeviceId");
            dt.Columns.Add("AliasId");
            dt.Columns.Add("SourceSubstrateId");
            dt.Columns.Add("TargetSubstrateId");
            dt.Columns.Add("SourcePositionId");
            dt.Columns.Add("TargetPositionId");
            dt.Columns.Add("FromX");
            dt.Columns.Add("FromY");
            dt.Columns.Add("ToX");
            dt.Columns.Add("ToY");
            dt.Columns.Add("BinResult");
            dt.Columns.Add("BinCode");
            dt.Columns.Add("Pick");
            dt.Columns.Add("BinDesc");
            dt.Columns.Add("LotNumber");

            var dataLength = devices.ToArray().GetLength(0);

            for (int i = 0; i < dataLength;)
            {
                //if (devices[i].Pick == true)
                //{
                //    dt.Rows.Add(tmpLotInfoId, devices[i].DeviceId, devices[i].AliasId, "", devices[i].TargetSubstrateId, "", devices[i].TargetPositionId, "", "",
                //    devices[i].ToX, devices[i].ToY, "PASS", devices[i].BinCode, devices[i].Pick, devices[i].BinDesc, tmpLotNum);
                //}
                //else if (devices[i].Pick == false)
                //{
                //    dt.Rows.Add(tmpLotInfoId, devices[i].DeviceId, devices[i].AliasId, "", devices[i].TargetSubstrateId, "", devices[i].TargetPositionId, "", "",
                //    devices[i].ToX, devices[i].ToY, "FAIL", devices[i].BinCode, devices[i].Pick, devices[i].BinDesc, tmpLotNum);
                //}
                //else
                //{
                    dt.Rows.Add(tmpLotInfoId, devices[i].DeviceId, devices[i].AliasId, "", devices[i].TargetSubstrateId, "", devices[i].TargetPositionId, "", "",
                    devices[i].ToX, devices[i].ToY, devices[i].BinResult.ToUpper(), devices[i].BinCode, devices[i].Pick, devices[i].BinDesc, tmpLotNum);
                //}
                i++;
            }

            return dt;
        }
        public static DataTable DTSetDeviceInfo(List<DisplayDeviceInfo> devices, int lotInfoId, string lotNum)
        {
            int tmpLotInfoId = lotInfoId;
            string tmpLotNum = lotNum;
            var dt = new DataTable();
            dt.Columns.Add("LotInfoId");
            dt.Columns.Add("DeviceId");
            dt.Columns.Add("AliasId");
            dt.Columns.Add("SourceSubstrateId");
            dt.Columns.Add("TargetSubstrateId");
            dt.Columns.Add("SourcePositionId");
            dt.Columns.Add("TargetPositionId");
            dt.Columns.Add("FromX");
            dt.Columns.Add("FromY");
            dt.Columns.Add("ToX");
            dt.Columns.Add("ToY");
            dt.Columns.Add("BinResult");
            dt.Columns.Add("BinCode");
            dt.Columns.Add("Pick");
            dt.Columns.Add("BinDesc");
            dt.Columns.Add("LotNumber");

            var dataLength = devices.ToArray().GetLength(0);

            for (int i = 0; i < dataLength;)
            {

                    dt.Rows.Add(tmpLotInfoId, devices[i].DeviceId, devices[i].AliasId, devices[i].SourceSubstrateId, devices[i].TargetSubstrateId, devices[i].SourcePositionId, devices[i].TargetPositionId, devices[i].FromX, devices[i].FromY,
                    devices[i].ToX, devices[i].ToY, devices[i].BinResultID.ToString(), devices[i].BinCode, devices[i].Pick, devices[i].BinDesc, tmpLotNum);
                i++;
            }

            return dt;
        }
        public static DataTable DTDeviceId(string[] devicesId)
        {
            var dt = new DataTable();

            dt.Columns.Add("DeviceId");

            var dataLength = devicesId.GetLength(0);

            for (int i = 0; i < dataLength;)
            {
                dt.Rows.Add(devicesId[i]);
                i++;
            }

            return dt;
        }
        public static DataTable DTScannedDeviceId(List<DeviceValidation> scannedDevices)
        {
            var scannedDevicesArray = scannedDevices.ToArray();
            var dt = new DataTable();

            dt.Columns.Add("DeviceId");

            var dataLength = scannedDevicesArray.GetLength(0);

            for (int i = 0; i < dataLength;)
            {
                dt.Rows.Add(scannedDevicesArray[i].DeviceId);
                i++;
            }

            return dt;
        }
        public static DataTable DTUserInfo(List<Account> users)
        {
            var dt = new DataTable();
            dt.Columns.Add("UserId");
            dt.Columns.Add("RoleId");
            
            var dataLength = users.ToArray().GetLength(0);

            for (int i = 0; i < dataLength;)
            {

                dt.Rows.Add(users[i].Username, users[i].RoleId);
                i++;
            }
            return dt;
        }
        public static DataTable DTUserId(string[] userIds)
        {
            var dt = new DataTable();
            dt.Columns.Add("UserId");

            var dataLength = userIds.GetLength(0);

            for (int i = 0; i < dataLength;)
            {
                dt.Rows.Add(userIds[i]);
                i++;
            }

            return dt;
        }

        #endregion

        #region User Validation\Get User Role
        public static Tuple<int, string, string> IsValidCredentials(string user, string password)
        {
            string role = string.Empty;
            string error = string.Empty;
            int success = 1;
            try
            {
                using (SqlConnection con = new SqlConnection(strDTOSConnString))
                {
                    using (SqlCommand cmd = new SqlCommand("PR_DTS_ValidateLogin", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@I_UserName", user);
                        cmd.Parameters.AddWithValue("@I_Password", password);
                        cmd.Parameters.Add("@O_Success", SqlDbType.Int, 10);
                        cmd.Parameters["@O_Success"].Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("@O_ErrTxt", SqlDbType.VarChar, 250);
                        cmd.Parameters["@O_ErrTxt"].Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("@O_Role", SqlDbType.VarChar, 50);
                        cmd.Parameters["@O_Role"].Direction = ParameterDirection.Output;
                        if (con.State != System.Data.ConnectionState.Open)
                        {
                            con.Open();

                            //SqlDataReader rdr = cmd.ExecuteReader();

                            //rdr.Read();
                            //lotInfoId = rdr["LotInfoId"].ToString();

                            cmd.ExecuteNonQuery();

                            success = (int)cmd.Parameters["@O_Success"].Value;

                            error = (string)cmd.Parameters["@O_ErrTxt"].Value;

                            role = (string)cmd.Parameters["@O_Role"].Value.ToString();
                        }
                    }
                }

                return new Tuple<int, string, string>(success, role, error);
            }
            catch (SqlException sex)
            {
                error = sex.Message;
                Log.WriteToErrorLogFile(error);
                return new Tuple<int, string, string>(success, role, error);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                Log.WriteToErrorLogFile(error);
                return new Tuple<int, string, string>(success, role, error);
            }
        }
        public static Tuple<int, string> AddUser(string user, int role)
        {
            string error = string.Empty;
            int success = 1;
            try
            {
                using (SqlConnection con = new SqlConnection(strDTOSConnString))
                {
                    using (SqlCommand cmd = new SqlCommand("PR_DTS_AddUser", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@I_UserId", user);
                        cmd.Parameters.AddWithValue("@I_Role", role);
                        cmd.Parameters.Add("@O_Success", SqlDbType.Int, 10);
                        cmd.Parameters["@O_Success"].Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("@O_ErrTxt", SqlDbType.VarChar, 250);
                        cmd.Parameters["@O_ErrTxt"].Direction = ParameterDirection.Output;
                        if (con.State != System.Data.ConnectionState.Open)
                        {
                            con.Open();

                            cmd.ExecuteNonQuery();

                            success = (int)cmd.Parameters["@O_Success"].Value;

                            error = (string)cmd.Parameters["@O_ErrTxt"].Value;

                        }
                    }
                }

                return new Tuple<int, string>(success, error);
            }
            catch (SqlException sex)
            {
                error = sex.Message;
                Log.WriteToErrorLogFile(error);
                return new Tuple<int, string>(success, error);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                Log.WriteToErrorLogFile(error);
                return new Tuple<int, string>(success, error);
            }
        }
        public static Tuple<int, string> ChangeUserPassword(string user, string oPassword, string nPassword)
        {
            string error = string.Empty;
            int success = 1;
            try
            {
                using (SqlConnection con = new SqlConnection(strDTOSConnString))
                {
                    using (SqlCommand cmd = new SqlCommand("PR_DTS_ChangePassword", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@I_UserId", user);
                        cmd.Parameters.AddWithValue("@I_OldPassword", oPassword);
                        cmd.Parameters.AddWithValue("@I_NewPassword", nPassword);
                        cmd.Parameters.Add("@O_Success", SqlDbType.Int, 10);
                        cmd.Parameters["@O_Success"].Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("@O_ErrTxt", SqlDbType.VarChar, 250);
                        cmd.Parameters["@O_ErrTxt"].Direction = ParameterDirection.Output;
                        if (con.State != System.Data.ConnectionState.Open)
                        {
                            con.Open();

                            cmd.ExecuteNonQuery();

                            success = (int)cmd.Parameters["@O_Success"].Value;

                            error = (string)cmd.Parameters["@O_ErrTxt"].Value;

                        }
                    }
                }

                return new Tuple<int, string>(success, error);
            }
            catch (SqlException sex)
            {
                error = sex.Message;
                Log.WriteToErrorLogFile(error);
                return new Tuple<int, string>(success, error);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                Log.WriteToErrorLogFile(error);
                return new Tuple<int, string>(success, error);
            }
        }
        public static Tuple<int, string> ModifyUserInfo(DataTable updateUserList)
        {
            string error = string.Empty;
            int success = 1;
            try
            {
                using (SqlConnection con = new SqlConnection(strDTOSConnString))
                {
                    using (SqlCommand cmd = new SqlCommand("PR_DTS_ModifyUserInfo", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Tbl_UpdateUsers", updateUserList);
                        cmd.Parameters.Add("@O_Success", SqlDbType.Int, 10);
                        cmd.Parameters["@O_Success"].Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("@O_ErrTxt", SqlDbType.VarChar, 250);
                        cmd.Parameters["@O_ErrTxt"].Direction = ParameterDirection.Output;
                        if (con.State != System.Data.ConnectionState.Open)
                        {
                            con.Open();

                            cmd.ExecuteNonQuery();

                            success = (int)cmd.Parameters["@O_Success"].Value;

                            error = (string)cmd.Parameters["@O_ErrTxt"].Value;

                        }
                    }
                }

                return new Tuple<int, string>(success, error);
            }
            catch (SqlException sex)
            {
                error = sex.Message;
                Log.WriteToErrorLogFile(error);
                return new Tuple<int, string>(success, error);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                Log.WriteToErrorLogFile(error);
                return new Tuple<int, string>(success, error);
            }
        }
        public static Tuple<int, string> DeleteUser(DataTable deleteUserList)
        {
            string error = string.Empty;
            int success = 1;
            try
            {
                using (SqlConnection con = new SqlConnection(strDTOSConnString))
                {
                    using (SqlCommand cmd = new SqlCommand("PR_DTS_DeleteUser", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Tbl_UserList", deleteUserList);
                        cmd.Parameters.Add("@O_Success", SqlDbType.Int, 10);
                        cmd.Parameters["@O_Success"].Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("@O_ErrTxt", SqlDbType.VarChar, 250);
                        cmd.Parameters["@O_ErrTxt"].Direction = ParameterDirection.Output;
                        if (con.State != System.Data.ConnectionState.Open)
                        {
                            con.Open();

                            cmd.ExecuteNonQuery();

                            success = (int)cmd.Parameters["@O_Success"].Value;

                            error = (string)cmd.Parameters["@O_ErrTxt"].Value;

                        }
                    }
                }

                return new Tuple<int, string>(success, error);
            }
            catch (SqlException sex)
            {
                error = sex.Message;
                Log.WriteToErrorLogFile(error);
                return new Tuple<int, string>(success, error);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                Log.WriteToErrorLogFile(error);
                return new Tuple<int, string>(success, error);
            }
        }
        public static List<RoleInfo> GetRoleType()
        {
            string error;
            int success = 1;
            List<RoleInfo> roleList = new List<RoleInfo>();

            try
            {
                using (SqlConnection con = new SqlConnection(strDTOSConnString))
                {
                    using (SqlCommand cmd = new SqlCommand("PR_DTS_GetUserRole", con))
                    {

                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@O_Success", SqlDbType.Int, 10);
                        cmd.Parameters["@O_Success"].Direction = ParameterDirection.Output;
                        if (con.State != System.Data.ConnectionState.Open)
                        {
                            con.Open();

                            SqlDataReader sdr = cmd.ExecuteReader();

                            DataTable dtRole = new DataTable();

                            dtRole.Load(sdr);
                            foreach (DataRow row in dtRole.Rows)
                            {
                                roleList.Add(
                                    new RoleInfo
                                    {
                                        RoleId = Convert.ToInt32(row["RoleId"]),
                                        RoleName = row["RoleName"].ToString(),
                                    }
                                  );
                            }
                            success = (int)cmd.Parameters["@O_Success"].Value;
                        }
                    }
                }
                if (success == 0)
                {
                    return roleList;
                }
                else
                {
                    return null;
                }
            }
            catch (SqlException sex)
            {
                error = sex.Message;
                Log.WriteToErrorLogFile(error);
                return null;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                Log.WriteToErrorLogFile(error);
                return null;
            }
        }
        public static List<Account> GetAllUser()
        {
            string error;
            List<Account> userList = new List<Account>();

            try
            {
                using (SqlConnection con = new SqlConnection(strDTOSConnString))
                {
                    using (SqlCommand cmd = new SqlCommand("PR_DTS_GetAllUser", con))
                    {

                        cmd.CommandType = CommandType.StoredProcedure;
                        if (con.State != System.Data.ConnectionState.Open)
                        {
                            con.Open();

                            SqlDataReader sdr = cmd.ExecuteReader();

                            DataTable dtRole = new DataTable();

                            dtRole.Load(sdr);
                            foreach (DataRow row in dtRole.Rows)
                            {
                                userList.Add(
                                    new Account
                                    {
                                        Username = row["UserId"].ToString(),
                                        RoleId = Convert.ToInt32(row["RoleId"]),
                                    }
                                  );
                            }
                        }
                    }
                }
                return userList;
            }
            catch (SqlException sex)
            {
                error = sex.Message;
                Log.WriteToErrorLogFile(error);
                return null;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                Log.WriteToErrorLogFile(error);
                return null;
            }
        }
        public static Tuple<int, int, string> InsertUserManagement(DataTable userInfo, string user)
        {
            string error = string.Empty;
            int success = 1;
            int uManageLotId = 0;
            try
            {
                using (SqlConnection con = new SqlConnection(strDTOSConnString))
                {
                    using (SqlCommand cmd = new SqlCommand("PR_DTS_InsertUserManagement", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Tbl_UserTrans", userInfo);
                        cmd.Parameters.AddWithValue("@I_UserId", user);
                        cmd.Parameters.Add("@O_Success", SqlDbType.Int, 10);
                        cmd.Parameters["@O_Success"].Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("@O_ErrTxt", SqlDbType.VarChar, 250);
                        cmd.Parameters["@O_ErrTxt"].Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("@O_UserManageLogId", SqlDbType.BigInt, 999999);
                        cmd.Parameters["@O_UserManageLogId"].Direction = ParameterDirection.Output;
                        if (con.State != System.Data.ConnectionState.Open)
                        {
                            con.Open();

                            cmd.ExecuteNonQuery();

                            success = (int)cmd.Parameters["@O_Success"].Value;

                            error = (string)cmd.Parameters["@O_ErrTxt"].Value;

                            uManageLotId = (int)cmd.Parameters["@O_UserManageLogId"].Value;
                        }
                    }
                }

                return new Tuple<int, int, string>(success, uManageLotId, error);
            }
            catch (SqlException sex)
            {
                error = sex.Message;
                Log.WriteToErrorLogFile(error);
                return new Tuple<int, int, string>(success, uManageLotId, error);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                Log.WriteToErrorLogFile(error);
                return new Tuple<int, int, string>(success, uManageLotId, error);
            }
        }
        #endregion

        #region Get/Insert Lot Info
        public static string InsertLotInfo(List<LotInfo> info)
        {
            string lotInfoId = string.Empty;
            string error;
            int success = 1;
            try
            {
                using (SqlConnection con = new SqlConnection(strDTOSConnString))
                {
                    using (SqlCommand cmd = new SqlCommand("PR_DTS_CreateBatchInfo", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@I_LotNumber", info[0].LotNum);
                        cmd.Parameters.AddWithValue("@I_Operation", info[0].Operation);
                        cmd.Parameters.AddWithValue("@I_LotQty1", info[0].LotQty1);
                        cmd.Parameters.AddWithValue("@I_UserId", info[0].UserId);
                        cmd.Parameters.AddWithValue("@I_ValidateQuantity", SafeDbObject(info[0].ValidateQuantity));
                        cmd.Parameters.Add("@O_Success", SqlDbType.Int, 10);
                        cmd.Parameters["@O_Success"].Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("@O_LotInfoId", SqlDbType.BigInt, 999999);
                        cmd.Parameters["@O_LotInfoId"].Direction = ParameterDirection.Output;
                        if (con.State != System.Data.ConnectionState.Open)
                        {
                            con.Open();

                            cmd.ExecuteNonQuery();

                            success = (int)cmd.Parameters["@O_Success"].Value;

                            lotInfoId = (string)cmd.Parameters["@O_LotInfoId"].Value.ToString();
                        }
                    }
                }

                if (success == 0)
                {
                    return lotInfoId;
                }
                else
                {
                    return null;
                }
            }
            catch (SqlException sex)
            {
                error = sex.Message;
                Log.WriteToErrorLogFile(error);
                return null;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                Log.WriteToErrorLogFile(error);
                return null;
            }
        }
        public static List<LotInfo> GetLotInfo(string mLotNum, int mLotId)
        {
            string error;
            int success = 1;
            List<LotInfo> mLotInfo = new List<LotInfo>();

            try
            {
                using (SqlConnection con = new SqlConnection(strDTOSConnString))
                {
                    using (SqlCommand cmd = new SqlCommand("PR_DTS_GetLotInfo", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@I_LotInfoId", mLotId);
                        cmd.Parameters.AddWithValue("@I_LotNumber", mLotNum);
                        cmd.Parameters.Add("@O_Success", SqlDbType.Int, 10);
                        cmd.Parameters["@O_Success"].Direction = ParameterDirection.Output;
                        if (con.State != System.Data.ConnectionState.Open)
                        {
                            con.Open();

                            SqlDataReader sdr = cmd.ExecuteReader();

                            DataTable dtLotInfo = new DataTable();

                            dtLotInfo.Load(sdr);
                            foreach (DataRow row in dtLotInfo.Rows)
                            {
                                mLotInfo.Add(
                                    new LotInfo
                                    {
                                        LotNum = row["LotNumber"].ToString(),
                                        Operation = Convert.ToInt32(row["Operation"]),
                                        LotQty1 = Convert.ToInt32(row["LotQty1"]),
                                        ValidateQuantity = Convert.ToInt32(row["ValidateQuantity"])
                                    }
                                  );
                            }
                            success = (int)cmd.Parameters["@O_Success"].Value;
                        }
                    }
                }
                if (success == 0)
                {
                    return mLotInfo;
                }
                else
                {
                    return new List<LotInfo>();
                }
            }
            catch (SqlException sex)
            {
                error = sex.Message;
                Log.WriteToErrorLogFile(error);
                return new List<LotInfo>();
            }
            catch (Exception ex)
            {
                error = ex.Message;
                Log.WriteToErrorLogFile(error);
                return new List<LotInfo>();
            }
        }
        public static string InsertSplitLot(string motherLotNumber, string childLotNumber, int splitQuantity, string operation)
        {
            string output = string.Empty;
            string error;
            int success = 1;

            try
            {
                using (SqlConnection con = new SqlConnection(strDTOSConnString))
                {
                    using (SqlCommand cmd = new SqlCommand("PR_DTS_InsertSplitLot", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@I_MotherLotNumber", motherLotNumber);
                        cmd.Parameters.AddWithValue("@I_ChildLotNumber", childLotNumber);
                        cmd.Parameters.AddWithValue("@I_SplitQuantity", splitQuantity);
                        cmd.Parameters.AddWithValue("@I_Operation", operation);
                        cmd.Parameters.Add("@O_Success", SqlDbType.Int, 10);
                        cmd.Parameters["@O_Success"].Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("@O_SplitId", SqlDbType.BigInt, 999999);
                        cmd.Parameters["@O_SplitId"].Direction = ParameterDirection.Output;
                        if (con.State != System.Data.ConnectionState.Open)
                        {
                            con.Open();

                            cmd.ExecuteNonQuery();

                            success = (int)cmd.Parameters["@O_Success"].Value;

                            output = (string)cmd.Parameters["@O_SplitId"].Value.ToString();
                        }
                    }
                }
                if(success == 0)
                {
                    return output;
                }
                else
                {
                    return null;
                }
            }
            catch (SqlException sex)
            {
                error = sex.Message;
                Log.WriteToErrorLogFile(error);
                return null;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                Log.WriteToErrorLogFile(error);
                return null;
            }
        }
        public static string InsertMergeLot(string motherLotNumber, string childLotNumber, string childLotNumber2, string childLotNumber3, string childLotNumber4, string operation)
        {
            string output = string.Empty;
            string error;
            int success = 1;

            try
            {
                using (SqlConnection con = new SqlConnection(strDTOSConnString))
                {
                    using (SqlCommand cmd = new SqlCommand("PR_DTS_InsertMergeLot", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@I_MotherLotNumber", motherLotNumber);
                        cmd.Parameters.AddWithValue("@I_ChildLotNumber", childLotNumber);
                        cmd.Parameters.AddWithValue("@I_ChildLotNumber2", SafeDbObject(childLotNumber2));
                        cmd.Parameters.AddWithValue("@I_ChildLotNumber3", SafeDbObject(childLotNumber3));
                        cmd.Parameters.AddWithValue("@I_ChildLotNumber4", SafeDbObject(childLotNumber4));
                        cmd.Parameters.AddWithValue("@I_Operation", operation);
                        cmd.Parameters.Add("@O_Success", SqlDbType.Int, 10);
                        cmd.Parameters["@O_Success"].Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("@O_MergeId", SqlDbType.BigInt, 999999);
                        cmd.Parameters["@O_MergeId"].Direction = ParameterDirection.Output;
                        if (con.State != System.Data.ConnectionState.Open)
                        {
                            con.Open();

                            cmd.ExecuteNonQuery();

                            success = (int)cmd.Parameters["@O_Success"].Value;

                            output = (string)cmd.Parameters["@O_MergeId"].Value.ToString();
                        }
                    }
                }
                if (success == 0)
                {
                    return output;
                }
                else
                {
                    return null;
                }
            }
            catch (SqlException sex)
            {
                error = sex.Message;
                Log.WriteToErrorLogFile(error);
                return null;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                Log.WriteToErrorLogFile(error);
                return null;
            }
        }
        public static string InsertUpdateLot(DataTable updateDInfo, string operation)
        {
            string output = string.Empty;
            string error;
            int success = 1;
            try
            {
                using (SqlConnection con = new SqlConnection(strDTOSConnString))
                {
                    using (SqlCommand cmd = new SqlCommand("PR_DTS_InsertUpdateLot", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@I_TblUpdateDTSInfoValue", updateDInfo);
                        cmd.Parameters.AddWithValue("@I_Operation", operation);
                        cmd.Parameters.Add("@O_Success", SqlDbType.Int, 10);
                        cmd.Parameters["@O_Success"].Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("@O_UpdateEditLotLogId", SqlDbType.BigInt, 999999);
                        cmd.Parameters["@O_UpdateEditLotLogId"].Direction = ParameterDirection.Output;
                        if (con.State != System.Data.ConnectionState.Open)
                        {
                            con.Open();

                            cmd.ExecuteNonQuery();

                            success = (int)cmd.Parameters["@O_Success"].Value;

                            output = (string)cmd.Parameters["@O_UpdateEditLotLogId"].Value.ToString();
                        }
                    }
                }
                if (success == 0)
                {
                    return output;
                }
                else
                {
                    return null;
                }

            }
            catch (SqlException sex)
            {
                error = sex.Message;
                Log.WriteToErrorLogFile(error);
                return null;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                Log.WriteToErrorLogFile(error);
                return null;
            }
        }
        public static string InsertBatchUpdateLot(DataTable updateDInfo, DeviceInfo updateDeviceInfo, string uLotNum, int uLotInfoId, string operation)
        {
            string output = string.Empty;
            string error;
            int success = 1;
            try
            {
                using (SqlConnection con = new SqlConnection(strDTOSConnString))
                {
                    using (SqlCommand cmd = new SqlCommand("PR_DTS_InsertBatchUpdateLot", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@I_tblBUpdateDeviceInfo", updateDInfo);
                        cmd.Parameters.AddWithValue("@I_LotNumber", uLotNum);
                        cmd.Parameters.AddWithValue("@I_LotInfoId", uLotInfoId);
                        cmd.Parameters.AddWithValue("@I_Operation", operation);
                        cmd.Parameters.AddWithValue("@I_SourceSubstrateId", SafeDbObject(updateDeviceInfo.SourceSubstrateId));
                        cmd.Parameters.AddWithValue("@I_TargetSubstrateId", SafeDbObject(updateDeviceInfo.TargetSubstrateId));
                        cmd.Parameters.AddWithValue("@I_BinResult", SafeDbObject(updateDeviceInfo.BinResult));
                        cmd.Parameters.AddWithValue("@I_BinCode", SafeDbObject(updateDeviceInfo.BinCode));
                        cmd.Parameters.AddWithValue("@I_BinDesc", SafeDbObject(updateDeviceInfo.BinDesc));
                        cmd.Parameters.Add("@O_Success", SqlDbType.Int, 10);
                        cmd.Parameters["@O_Success"].Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("@O_UpdateLotLogId", SqlDbType.BigInt, 999999);
                        cmd.Parameters["@O_UpdateLotLogId"].Direction = ParameterDirection.Output;
                        if (con.State != System.Data.ConnectionState.Open)
                        {
                            con.Open();

                            cmd.ExecuteNonQuery();

                            success = (int)cmd.Parameters["@O_Success"].Value;

                            output = (string)cmd.Parameters["@O_UpdateLotLogId"].Value.ToString();
                        }
                    }
                }
                if (success == 0)
                {
                    return output;
                }
                else
                {
                    return null;
                }

            }
            catch (SqlException sex)
            {
                error = sex.Message;
                Log.WriteToErrorLogFile(error);
                return null;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                Log.WriteToErrorLogFile(error);
                return null;
            }
        }

        #endregion

        #region Get/Insert/Update Device Info
        //convert object to dbnull if null method
        public static Object SafeDbObject(Object input)
        {
            if (input == null)
            {
                return DBNull.Value;
            }
            else
            {
                return input;
            }
        }
        public static string InsertDeviceInfo(DataTable iDeviceInfo)
        {
            string output;
            string error;
            int success = 1;

            try
            {
                using (SqlConnection con = new SqlConnection(strDTOSConnString))
                {
                    using (SqlCommand cmd = new SqlCommand("PR_DTS_InsertDeviceInfo", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@I_tblDTSInfoValue", iDeviceInfo);
                        cmd.Parameters.Add("@O_Success", SqlDbType.Int, 10);
                        cmd.Parameters["@O_Success"].Direction = ParameterDirection.Output;
                        if (con.State != System.Data.ConnectionState.Open)
                        {
                            con.Open();

                            cmd.ExecuteNonQuery();

                            success = (int)cmd.Parameters["@O_Success"].Value;
                        }
                    }
                }
                if(success == 0)
                {
                    output = "Success";
                    return output;
                }
                else
                {
                    return null;
                }
            }
            catch (SqlException sex)
            {
                error = sex.Message;
                Log.WriteToErrorLogFile(error);
                return null;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                Log.WriteToErrorLogFile(error);
                return null;
            }
        }

        public static string UpdateDeviceInfo(DataTable uDInfo)
        {
            string output;
            string error;
            int success = 1;
            try
            {
                using (SqlConnection con = new SqlConnection(strDTOSConnString))
                {
                    using (SqlCommand cmd = new SqlCommand("PR_DTS_UpdateDeviceInfo", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@I_tblDTSInfoValue", uDInfo);
                        cmd.Parameters.Add("@O_Success", SqlDbType.Int, 10);
                        cmd.Parameters["@O_Success"].Direction = ParameterDirection.Output;
                        if (con.State != System.Data.ConnectionState.Open)
                        {
                            con.Open();

                            cmd.ExecuteNonQuery();

                            success = (int)cmd.Parameters["@O_Success"].Value;
                        }
                    }
                }
                if (success == 0)
                {
                    output = "Success";
                    return output;
                }
                else
                {
                    return null;
                }
            }
            catch (SqlException sex)
            {
                error = sex.Message;
                Log.WriteToErrorLogFile(error);
                return null;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                Log.WriteToErrorLogFile(error);
                return null;
            }
        }
        public static string BatchUpdateDeviceInfo(DataTable uDInfo, string uLotNum, int uLotInfoId, DeviceInfo deviceInfo)
        {
            string output;
            string error;
            int success = 1;
            try
            {
                using (SqlConnection con = new SqlConnection(strDTOSConnString))
                {
                    using (SqlCommand cmd = new SqlCommand("PR_DTS_BatchUpdateDeviceInfo", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@I_tblBUDeviceInfo", uDInfo);
                        cmd.Parameters.AddWithValue("@I_LotNumber", uLotNum);
                        cmd.Parameters.AddWithValue("@I_LotInfoId", uLotInfoId);
                        cmd.Parameters.AddWithValue("@I_SourceSubstrateId", SafeDbObject(deviceInfo.SourceSubstrateId));
                        cmd.Parameters.AddWithValue("@I_TargetSubstrateId", SafeDbObject(deviceInfo.TargetSubstrateId));
                        cmd.Parameters.AddWithValue("@I_BinResult", SafeDbObject(deviceInfo.BinResult));
                        cmd.Parameters.AddWithValue("@I_BinCode", SafeDbObject(deviceInfo.BinCode));
                        cmd.Parameters.AddWithValue("@I_BinDesc", SafeDbObject(deviceInfo.BinDesc));
                        cmd.Parameters.Add("@O_Success", SqlDbType.Int, 10);
                        cmd.Parameters["@O_Success"].Direction = ParameterDirection.Output;
                        if (con.State != System.Data.ConnectionState.Open)
                        {
                            con.Open();

                            cmd.ExecuteNonQuery();

                            success = (int)cmd.Parameters["@O_Success"].Value;
                        }
                    }
                }
                if(success == 0)
                {
                    output = "Success";
                    return output;
                }
                else
                {
                    return null;
                }
                
            }
            catch (SqlException sex)
            {
                error = sex.Message;
                Log.WriteToErrorLogFile(error);
                return null;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                Log.WriteToErrorLogFile(error);
                return null;
            }
        }

        public static List<DisplayDeviceInfo> GetDeviceInfo(string lot, int lotId, string type)
        {
            string error;
            int success = 1;
            List<DisplayDeviceInfo> deviceInfos = new List<DisplayDeviceInfo>();

            try
            {
                using (SqlConnection con = new SqlConnection(strDTOSConnString))
                {
                    using (SqlCommand cmd = new SqlCommand("PR_DTS_GetAllDevice", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@I_LotInfoId", lotId);
                        cmd.Parameters.AddWithValue("@I_LotNumber", lot);
                        cmd.Parameters.AddWithValue("@I_TransactionType", type);
                        cmd.Parameters.Add("@O_Success", SqlDbType.Int, 10);
                        cmd.Parameters["@O_Success"].Direction = ParameterDirection.Output;

                        //SqlParameter returnValue = new SqlParameter("@Return_Value", SqlDbType.Int);
                        //returnValue.Direction = ParameterDirection.ReturnValue;
                        //cmd.Parameters.Add(returnValue);

                        if (con.State != System.Data.ConnectionState.Open)
                        {
                            con.Open();

                            SqlDataReader sdr = cmd.ExecuteReader();

                            DataTable dtDeviceInfo = new DataTable();

                            dtDeviceInfo.Load(sdr);
                            foreach (DataRow row in dtDeviceInfo.Rows)
                            {
                                deviceInfos.Add(
                                    new DisplayDeviceInfo
                                    {
                                        DeviceId = row["DeviceId"].ToString(),
                                        AliasId = row["AliasId"].ToString(),
                                        SourceSubstrateId = row["SourceSubstrateId"].ToString(),
                                        TargetSubstrateId = row["TargetSubstrateId"].ToString(),
                                        SourcePositionId = row["SourcePositionId"].ToString(),
                                        TargetPositionId = row["TargetPositionId"].ToString(),
                                        FromX = row["FromX"].ToString(),
                                        FromY = row["FromY"].ToString(),
                                        ToX = row["ToX"].ToString(),
                                        ToY = row["ToY"].ToString(),
                                        BinResultID = Convert.ToInt32(row["BinResultId"]),
                                        BinCode = row["BinCode"].ToString(),
                                        BinDesc = row["BinDesc"].ToString()
                                    }
                                  );
                            }
                                success = (int)cmd.Parameters["@O_Success"].Value;
                        }
                    }
                }
                if (success == 0)
                {
                    return deviceInfos;
                }
                else
                {
                    return new List<DisplayDeviceInfo>();
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                Log.WriteToErrorLogFile(error);
                return new List<DisplayDeviceInfo>();
            }
        }
        public static List<DeviceInfo> GetDeviceInfoDTS(string lot, int lotId, string transactionType)
        {
            string error;
            int success = 1;
            List<DeviceInfo> deviceInfos = new List<DeviceInfo>();

            try
            {
                using (SqlConnection con = new SqlConnection(strDTOSConnString))
                {
                    using (SqlCommand cmd = new SqlCommand("PR_DTS_GetAllDeviceDTS", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@I_LotInfoId", lotId);
                        cmd.Parameters.AddWithValue("@I_LotNumber", lot);
                        cmd.Parameters.AddWithValue("@I_TransactionType", transactionType);
                        cmd.Parameters.Add("@O_Success", SqlDbType.Int, 10);
                        cmd.Parameters["@O_Success"].Direction = ParameterDirection.Output;

                        //SqlParameter returnValue = new SqlParameter("@Return_Value", SqlDbType.Int);
                        //returnValue.Direction = ParameterDirection.ReturnValue;
                        //cmd.Parameters.Add(returnValue);

                        if (con.State != System.Data.ConnectionState.Open)
                        {
                            con.Open();

                            SqlDataReader sdr = cmd.ExecuteReader();

                            DataTable dtDeviceInfo = new DataTable();

                            dtDeviceInfo.Load(sdr);
                            foreach (DataRow row in dtDeviceInfo.Rows)
                            {
                                deviceInfos.Add(
                                    new DeviceInfo
                                    {
                                        DeviceId = row["DeviceId"].ToString(),
                                        AliasId = row["AliasId"].ToString(),
                                        SourceSubstrateId = row["SourceSubstrateId"].ToString(),
                                        TargetSubstrateId = row["TargetSubstrateId"].ToString(),
                                        SourcePositionId = row["SourcePositionId"].ToString(),
                                        TargetPositionId = row["TargetPositionId"].ToString(),
                                        FromX = row["FromX"].ToString(),
                                        FromY = row["FromY"].ToString(),
                                        ToX = row["ToX"].ToString(),
                                        ToY = row["ToY"].ToString(),
                                        BinResult = row["BinResult"].ToString(),
                                        BinCode = row["BinCode"].ToString(),
                                        BinDesc = row["BinDesc"].ToString()
                                    }
                                  );
                            }
                            success = (int)cmd.Parameters["@O_Success"].Value;
                        }
                    }
                }
                if (success == 0)
                {
                    return deviceInfos;
                }
                else
                {
                    return new List<DeviceInfo>();
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                Log.WriteToErrorLogFile(error);
                return new List<DeviceInfo>();
            }
        }
        public static List<DeviceInfo> GetChildDeviceInfoDTS(int lotId, DataTable cDeviceId, string transactionType)
        {
            string error;
            int success = 1;
            List<DeviceInfo> cDeviceInfos = new List<DeviceInfo>();

            try
            {
                using (SqlConnection con = new SqlConnection(strDTOSConnString))
                {
                    using (SqlCommand cmd = new SqlCommand("PR_DTS_GetSelectedDeviceInfo", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@I_LotInfoId", lotId);
                        cmd.Parameters.AddWithValue("@I_tblDevicesId", cDeviceId);
                        cmd.Parameters.AddWithValue("@I_TransactionType", transactionType);
                        cmd.Parameters.Add("@O_Success", SqlDbType.Int, 10);
                        cmd.Parameters["@O_Success"].Direction = ParameterDirection.Output;
                        if (con.State != System.Data.ConnectionState.Open)
                        {
                            con.Open();

                            SqlDataReader sdr = cmd.ExecuteReader();

                            DataTable dtCDeviceInfo = new DataTable();

                            dtCDeviceInfo.Load(sdr);
                            foreach (DataRow row in dtCDeviceInfo.Rows)
                            {
                                cDeviceInfos.Add(
                                    new DeviceInfo
                                    {
                                        DeviceId = row["DeviceId"].ToString(),
                                        AliasId = row["AliasId"].ToString(),
                                        SourceSubstrateId = row["SourceSubstrateId"].ToString(),
                                        TargetSubstrateId = row["TargetSubstrateId"].ToString(),
                                        SourcePositionId = row["SourcePositionId"].ToString(),
                                        TargetPositionId = row["TargetPositionId"].ToString(),
                                        FromX = row["FromX"].ToString(),
                                        FromY = row["FromY"].ToString(),
                                        ToX = row["ToX"].ToString(),
                                        ToY = row["ToY"].ToString(),
                                        BinResult = row["BinResult"].ToString(),
                                        BinCode = row["BinCode"].ToString(),
                                        BinDesc = row["BinDesc"].ToString()
                                    }
                                  );
                            }
                            success = (int)cmd.Parameters["@O_Success"].Value;
                        }
                    }
                }
                if(success == 0)
                {
                    return cDeviceInfos;
                }
                else
                {
                    return new List<DeviceInfo>();
                }
            }
            catch (SqlException sex)
            {
                error = sex.Message;
                Log.WriteToErrorLogFile(error);
                return new List<DeviceInfo>();
            }
            catch(Exception ex)
            {
                error = ex.Message;
                Log.WriteToErrorLogFile(error);
                return new List<DeviceInfo>();
            }
        }
        public static List<DeviceInfo> GetMotherDeviceInfoDTS(int lotId, DataTable mDeviceId, string transactionType)
        {
            string error;
            int success = 1;
            List<DeviceInfo> mDeviceInfos = new List<DeviceInfo>();

            try
            {
                using (SqlConnection con = new SqlConnection(strDTOSConnString))
                {
                    using (SqlCommand cmd = new SqlCommand("PR_DTS_GetMotherLotDevices", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@I_LotInfoId", lotId);
                        cmd.Parameters.AddWithValue("@I_tblDevicesId", mDeviceId);
                        cmd.Parameters.AddWithValue("@I_TransactionType", transactionType);
                        cmd.Parameters.Add("@O_Success", SqlDbType.Int, 10);
                        cmd.Parameters["@O_Success"].Direction = ParameterDirection.Output;
                        if (con.State != System.Data.ConnectionState.Open)
                        {
                            con.Open();

                            SqlDataReader sdr = cmd.ExecuteReader();

                            DataTable dtMDeviceInfo = new DataTable();

                            dtMDeviceInfo.Load(sdr);
                            foreach (DataRow row in dtMDeviceInfo.Rows)
                            {
                                mDeviceInfos.Add(
                                    new DeviceInfo
                                    {
                                        DeviceId = row["DeviceId"].ToString(),
                                        AliasId = row["AliasId"].ToString(),
                                        SourceSubstrateId = row["SourceSubstrateId"].ToString(),
                                        TargetSubstrateId = row["TargetSubstrateId"].ToString(),
                                        SourcePositionId = row["SourcePositionId"].ToString(),
                                        TargetPositionId = row["TargetPositionId"].ToString(),
                                        FromX = row["FromX"].ToString(),
                                        FromY = row["FromY"].ToString(),
                                        ToX = row["ToX"].ToString(),
                                        ToY = row["ToY"].ToString(),
                                        BinResult = row["BinResult"].ToString(),
                                        BinCode = row["BinCode"].ToString(),
                                        BinDesc = row["BinDesc"].ToString()
                                    }
                                  );
                            }
                            success = (int)cmd.Parameters["@O_Success"].Value;
                        }
                    }
                }
                if (success == 0)
                {
                    return mDeviceInfos;
                }
                else
                {
                    return new List<DeviceInfo>();
                }
            }
            catch (SqlException sex)
            {
                error = sex.Message;
                Log.WriteToErrorLogFile(error);
                return new List<DeviceInfo>();
            }
            catch(Exception ex)
            {
                error = ex.Message;
                Log.WriteToErrorLogFile(error);
                return new List<DeviceInfo>();
            }
        }
        #endregion

        #region Create/Get Audit Log
        public static string CreateAuditLog(string user, int trans, int eventId)
        {
            string output;
            int success = 1;
            string error;
            try
            {
                using (SqlConnection con = new SqlConnection(strDTOSConnString))
                {
                    using (SqlCommand cmd = new SqlCommand("PR_DTS_InsertAuditLog", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@I_UserId", user);
                        cmd.Parameters.AddWithValue("@I_EventId", trans);
                        cmd.Parameters.AddWithValue("@I_TransactionId", eventId);
                        cmd.Parameters.Add("@O_Success", SqlDbType.Int, 10);
                        cmd.Parameters["@O_Success"].Direction = ParameterDirection.Output;
                        if (con.State != System.Data.ConnectionState.Open)
                        {
                            con.Open();

                            cmd.ExecuteNonQuery();

                            success = (int)cmd.Parameters["@O_Success"].Value;
                        }
                    }
                }
                if(success == 0)
                {
                    output = "Success";
                    return output;
                }
                else
                {
                    return null;
                }

            }
            catch (SqlException sex)
            {
                error = sex.Message;
                Log.WriteToErrorLogFile(error);
                return null;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                Log.WriteToErrorLogFile(error);
                return null;
            }
        }

        public static List<AuditLog> GetAuditLog(string startDateTime, string endDateTime)
        {
            string error;
            int success = 1;
            List<AuditLog> auditLogs = new List<AuditLog>();

            try
            {
                using (SqlConnection con = new SqlConnection(strDTOSConnString))
                {
                    using (SqlCommand cmd = new SqlCommand("PR_DTS_GetAuditLog", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@I_StartDateTime", startDateTime);
                        cmd.Parameters.AddWithValue("@I_EndDateTime", endDateTime);
                        cmd.Parameters.Add("@O_Success", SqlDbType.Int, 10);
                        cmd.Parameters["@O_Success"].Direction = ParameterDirection.Output;
                        if (con.State != System.Data.ConnectionState.Open)
                        {
                            con.Open();

                            SqlDataReader sdr = cmd.ExecuteReader();

                            DataTable dtAuditLogs = new DataTable();
                            dtAuditLogs.Load(sdr);
                            foreach (DataRow row in dtAuditLogs.Rows)
                            {
                                DateTime eventDate = Convert.ToDateTime(row["EventDateTime"]);

                                auditLogs.Add(
                                    new AuditLog
                                    {
                                        UserId = row["UserID"].ToString(),
                                        Event = row["EventName"].ToString(),
                                        EventTime = eventDate.ToString("MM/dd/yyyy HH:mm:ss.fff"),
                                        MotherLot = row["MotherLotNumber"].ToString(),
                                        ChildLot = row["ChildLotNumber"].ToString(),
                                        Operation = row["Operation"].ToString()
                                    });
                            }
                            success = (int)cmd.Parameters["@O_Success"].Value;
                        }
                    }
                }
                if(success == 0)
                {
                    return auditLogs;
                }
                else
                {
                    return new List<AuditLog>();
                }
            }
            catch (SqlException sex)
            {
                error = sex.Message;
                Log.WriteToErrorLogFile(error);
                return new List<AuditLog>();
            }
            catch (Exception ex)
            {
                error = ex.Message;
                Log.WriteToErrorLogFile(error);
                return new List<AuditLog>();
            }

        }
        #endregion

        #region Get BinResult Type
        public static List<BinResultInfo> GetBinResultType()
        {
            string error;
            int success = 1;
            List<BinResultInfo> binResultList = new List<BinResultInfo>();

            try
            {
                using (SqlConnection con = new SqlConnection(strDTOSConnString))
                {
                    using (SqlCommand cmd = new SqlCommand("PR_DTS_GetBinResultType", con))
                    {

                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@O_Success", SqlDbType.Int, 10);
                        cmd.Parameters["@O_Success"].Direction = ParameterDirection.Output;
                        if (con.State != System.Data.ConnectionState.Open)
                        {
                            con.Open();

                            SqlDataReader sdr = cmd.ExecuteReader();

                            DataTable dtBinResultType = new DataTable();

                            dtBinResultType.Load(sdr);
                            foreach (DataRow row in dtBinResultType.Rows)
                            {
                                binResultList.Add(
                                    new BinResultInfo
                                    {
                                        BinResultID = Convert.ToInt32(row["BinResultId"]),
                                        BinResultName = row["BinResultName"].ToString(),
                                    }
                                  );
                            }
                            success = (int)cmd.Parameters["@O_Success"].Value;
                        }
                    }
                }
                if (success == 0)
                {
                    return binResultList;
                }
                else
                {
                    return null;
                }
            }
            catch (SqlException sex)
            {
                error = sex.Message;
                Log.WriteToErrorLogFile(error);
                return null;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                Log.WriteToErrorLogFile(error);
                return null;
            }
        }
        #endregion

        #region ScanDevice
        public static string ValidateScanDevice(string deviceId, int lotInfoId, int operation, string transaction)
        {
            string result = string.Empty;
            string error;
            int success = 1;
            try
            {
                using (SqlConnection con = new SqlConnection(strDTOSConnString))
                {
                    using (SqlCommand cmd = new SqlCommand("PR_DTS_ValidateScannedDevice", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@I_DeviceId", deviceId);
                        cmd.Parameters.AddWithValue("@I_LotInfoId", lotInfoId);
                        cmd.Parameters.AddWithValue("@I_Operation", operation);
                        cmd.Parameters.AddWithValue("@I_TransactionType", transaction);
                        cmd.Parameters.Add("@O_Success", SqlDbType.Int, 10);
                        cmd.Parameters["@O_Success"].Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("@O_Result", SqlDbType.Int, 10);
                        cmd.Parameters["@O_Result"].Direction = ParameterDirection.Output;
                        if (con.State != System.Data.ConnectionState.Open)
                        {
                            con.Open();

                            //SqlDataReader rdr = cmd.ExecuteReader();

                            //rdr.Read();
                            //lotInfoId = rdr["LotInfoId"].ToString();

                            cmd.ExecuteNonQuery();

                            success = (int)cmd.Parameters["@O_Success"].Value;

                            result = (string)cmd.Parameters["@O_Result"].Value.ToString();
                        }
                    }
                }

                if (success == 0)
                {
                    return result;
                }
                else
                {
                    return null;
                }
            }
            catch (SqlException sex)
            {
                error = sex.Message;
                Log.WriteToErrorLogFile(error);
                return null;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                Log.WriteToErrorLogFile(error);
                return null;
            }
        }

        public static List<DeviceValidation> GetScannedDevice(int lotInfoId, int operation)
        {
            string error;
            int success = 1;
            List<DeviceValidation> scannedDeviceInfo = new List<DeviceValidation>();

            try
            {
                using (SqlConnection con = new SqlConnection(strDTOSConnString))
                {
                    using (SqlCommand cmd = new SqlCommand("PR_DTS_GetScannedDevice", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@I_LotInfoId", lotInfoId);
                        cmd.Parameters.AddWithValue("@I_Operation", operation);
                        cmd.Parameters.Add("@O_Success", SqlDbType.Int, 10);
                        cmd.Parameters["@O_Success"].Direction = ParameterDirection.Output;

                        //SqlParameter returnValue = new SqlParameter("@Return_Value", SqlDbType.Int);
                        //returnValue.Direction = ParameterDirection.ReturnValue;
                        //cmd.Parameters.Add(returnValue);

                        if (con.State != System.Data.ConnectionState.Open)
                        {
                            con.Open();

                            SqlDataReader sdr = cmd.ExecuteReader();

                            DataTable dtScannedDeviceInfo = new DataTable();

                            dtScannedDeviceInfo.Load(sdr);
                            foreach (DataRow row in dtScannedDeviceInfo.Rows)
                            {
                                scannedDeviceInfo.Add(
                                    new DeviceValidation
                                    {
                                        DeviceId = row["DeviceId"].ToString(),
                                        BinResult = row["BinResult"].ToString(),
                                        BinCode = row["BinCode"].ToString(),
                                        BinDesc = row["BinDesc"].ToString(),
                                        ValidateResult = row["ValidateResult"].ToString()
                                    }
                                  );
                            }
                            success = (int)cmd.Parameters["@O_Success"].Value;
                        }
                    }
                }
                if (success == 0)
                {
                    return scannedDeviceInfo;
                }
                else
                {
                    return new List<DeviceValidation>();
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                Log.WriteToErrorLogFile(error);
                return new List<DeviceValidation>();
            }
        }

        public static string ValidateScanDeviceResult(int lotInfoId, int operation, string validationType)
        {
            string result = string.Empty;
            string error;
            int success = 1;
            try
            {
                using (SqlConnection con = new SqlConnection(strDTOSConnString))
                {
                    using (SqlCommand cmd = new SqlCommand("PR_DTS_InsertValidationResult", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@I_LotInfoId", lotInfoId);
                        cmd.Parameters.AddWithValue("@I_Operation", operation);
                        cmd.Parameters.AddWithValue("@I_ValidationType", validationType);
                        cmd.Parameters.Add("@O_Success", SqlDbType.Int, 10);
                        cmd.Parameters["@O_Success"].Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("@O_ValidationResult", SqlDbType.VarChar, 50);
                        cmd.Parameters["@O_ValidationResult"].Direction = ParameterDirection.Output;
                        if (con.State != System.Data.ConnectionState.Open)
                        {
                            con.Open();

                            cmd.ExecuteNonQuery();

                            success = (int)cmd.Parameters["@O_Success"].Value;

                            result = (string)cmd.Parameters["@O_ValidationResult"].Value.ToString();
                        }
                    }
                }

                if (success == 0)
                {
                    return result;
                }
                else
                {
                    return null;
                }
            }
            catch (SqlException sex)
            {
                error = sex.Message;
                Log.WriteToErrorLogFile(error);
                return null;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                Log.WriteToErrorLogFile(error);
                return null;
            }
        }

        public static string DeleteScannedData(int lotInfoId, int operation)
        {
            string error;
            int success = 1;
            try
            {
                using (SqlConnection con = new SqlConnection(strDTOSConnString))
                {
                    using (SqlCommand cmd = new SqlCommand("PR_DTS_DeleteTempScanDevice", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@I_LotInfoId", lotInfoId);
                        cmd.Parameters.AddWithValue("@I_Operation", operation);
                        cmd.Parameters.Add("@O_Success", SqlDbType.Int, 10);
                        cmd.Parameters["@O_Success"].Direction = ParameterDirection.Output;
                        if (con.State != System.Data.ConnectionState.Open)
                        {
                            con.Open();

                            cmd.ExecuteNonQuery();

                            success = (int)cmd.Parameters["@O_Success"].Value;

                        }
                    }
                }

                if (success == 0)
                {
                    return success.ToString();
                }
                else
                {
                    return null;
                }
            }
            catch (SqlException sex)
            {
                error = sex.Message;
                Log.WriteToErrorLogFile(error);
                return null;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                Log.WriteToErrorLogFile(error);
                return null;
            }
        }

        public static string CopyScannedData(int lotInfoId)
        {
            string error;
            int success = 1;
            try
            {
                using (SqlConnection con = new SqlConnection(strDTOSConnString))
                {
                    using (SqlCommand cmd = new SqlCommand("PR_DTS_CopyScannedDeviceInfo", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@I_LotInfoId", lotInfoId);
                        cmd.Parameters.Add("@O_Success", SqlDbType.Int, 10);
                        cmd.Parameters["@O_Success"].Direction = ParameterDirection.Output;
                        if (con.State != System.Data.ConnectionState.Open)
                        {
                            con.Open();

                            cmd.ExecuteNonQuery();

                            success = (int)cmd.Parameters["@O_Success"].Value;

                        }
                    }
                }

                if (success == 0)
                {
                    return success.ToString();
                }
                else
                {
                    return null;
                }
            }
            catch (SqlException sex)
            {
                error = sex.Message;
                Log.WriteToErrorLogFile(error);
                return null;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                Log.WriteToErrorLogFile(error);
                return null;
            }
        }

        public static int GetValidationResultLogID(int lotInfoId, int operation)
        {
            int vLogId = 0;
            string error;
            int success = 1;
            try
            {
                using (SqlConnection con = new SqlConnection(strDTOSConnString))
                {
                    using (SqlCommand cmd = new SqlCommand("PR_DTS_GetValidationResultLogID", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@I_LotInfoId", lotInfoId);
                        cmd.Parameters.AddWithValue("@I_Operation", operation);
                        cmd.Parameters.Add("@O_Success", SqlDbType.Int, 10);
                        cmd.Parameters["@O_Success"].Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("@O_VLogID", SqlDbType.BigInt, 999999);
                        cmd.Parameters["@O_VLogID"].Direction = ParameterDirection.Output;
                        if (con.State != System.Data.ConnectionState.Open)
                        {
                            con.Open();

                            cmd.ExecuteNonQuery();

                            success = (int)cmd.Parameters["@O_Success"].Value;

                            vLogId = Convert.ToInt32((long)cmd.Parameters["@O_VLogID"].Value);
                        }
                    }
                }

                if (success == 0)
                {
                    return vLogId;
                }
                else
                {
                    return 0;
                }
            }
            catch (SqlException sex)
            {
                error = sex.Message;
                Log.WriteToErrorLogFile(error);
                return 0;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                Log.WriteToErrorLogFile(error);
                return 0;
            }
        }
        #endregion
        public static int GetnUpdateQuantityLotInfo(int lotInfoId, string transactionType)
        {
            int vQuantity = 0;
            string error;
            int success = 1;
            try
            {
                using (SqlConnection con = new SqlConnection(strDTOSConnString))
                {
                    using (SqlCommand cmd = new SqlCommand("PR_DTS_GetnUpdateQuantityLotInfo", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@I_LotInfoId", lotInfoId);
                        cmd.Parameters.AddWithValue("@I_TransactionType", transactionType);
                        cmd.Parameters.Add("@O_Success", SqlDbType.Int, 10);
                        cmd.Parameters["@O_Success"].Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("@O_DeviceQuantity", SqlDbType.Int, 999999);
                        cmd.Parameters["@O_DeviceQuantity"].Direction = ParameterDirection.Output;
                        if (con.State != System.Data.ConnectionState.Open)
                        {
                            con.Open();

                            cmd.ExecuteNonQuery();

                            success = (int)cmd.Parameters["@O_Success"].Value;

                            vQuantity = (int)cmd.Parameters["@O_DeviceQuantity"].Value;
                        }
                    }
                }

                if (success == 0 && vQuantity != 0)
                {
                    return vQuantity;
                }
                else if (success == 0 && vQuantity == 0)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }
            catch (SqlException sex)
            {
                error = sex.Message;
                Log.WriteToErrorLogFile(error);
                return 0;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                Log.WriteToErrorLogFile(error);
                return 0;
            }
        }
    }

}