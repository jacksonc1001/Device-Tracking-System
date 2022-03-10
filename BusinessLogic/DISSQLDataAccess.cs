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
* Objective: Maintain all the DIS database connection and transaction
*/

namespace Device_Tracking_System.BusinessLogic
{
    public class DISSQLDataAccess
    {
        private static readonly string strDISConnString = ConfigurationManager.ConnectionStrings["dbDISConn"].ConnectionString;

        #region Datable
        public static DataTable DTSetDISDeviceInfo(List<DeviceInfo> devicesList)
        {
            var dt = new DataTable();
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
            dt.Columns.Add("Result");
            dt.Columns.Add("BinCode");
            dt.Columns.Add("BinDesc");

            var dataLength = devicesList.ToArray().GetLength(0);

            for (int i = 0; i < dataLength;)
            {

                dt.Rows.Add(devicesList[i].DeviceId, devicesList[i].AliasId, devicesList[i].SourceSubstrateId, devicesList[i].TargetSubstrateId, devicesList[i].SourcePositionId, devicesList[i].TargetPositionId, devicesList[i].FromX, devicesList[i].FromY,
                devicesList[i].ToX, devicesList[i].ToY, devicesList[i].BinResult, devicesList[i].BinCode, devicesList[i].BinDesc);
                i++;
            }

            return dt;
        }
        #endregion

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

        #region Get Device Info
        public static Tuple<int, List<DISDeviceInfoList>, string> GetDeviceInfo(string lot, int oper, string eqp)
        {
            string error = string.Empty;
            int success = 1;
            List<DISDeviceInfoList> deviceInfos = new List<DISDeviceInfoList>();

            try
            {
                using (SqlConnection con = new SqlConnection(strDISConnString))
                {
                    using (SqlCommand cmd = new SqlCommand("PR_DIS_GetDeviceInfoByLot", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@I_LotNumber", lot);
                        cmd.Parameters.AddWithValue("@I_Operation", oper);
                        cmd.Parameters.AddWithValue("@I_Equipment", eqp);
                        cmd.Parameters.Add("@O_Success", SqlDbType.Int, 10);
                        cmd.Parameters["@O_Success"].Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("@O_ErrTxt", SqlDbType.VarChar, 250);
                        cmd.Parameters["@O_ErrTxt"].Direction = ParameterDirection.Output;

                        if (con.State != System.Data.ConnectionState.Open)
                        {
                            con.Open();

                            SqlDataReader sdr = cmd.ExecuteReader();

                            DataTable dtDeviceInfo = new DataTable();

                            dtDeviceInfo.Load(sdr);
                            foreach (DataRow row in dtDeviceInfo.Rows)
                            {
                                deviceInfos.Add(
                                    new DISDeviceInfoList
                                    {
                                        DeviceId = row["DeviceId"].ToString(),
                                        AliasId = row["AliasId"].ToString(),
                                        TargetSubstrateId = row["TargetSubstrateId"].ToString(),
                                        TargetPositionId = row["TargetPositionId"].ToString(),
                                        ToX = row["ToX"].ToString(),
                                        ToY = row["ToY"].ToString(),
                                        Result = row["Result"].ToString(),
                                        BinCode = row["BinCode"].ToString(),
                                        Pick = Convert.ToBoolean(row["Pick"]),
                                        BinDesc = row["BinDesc"].ToString()
                                    }
                                  );
                            }

                            success = (int)cmd.Parameters["@O_Success"].Value;

                            error = (string)cmd.Parameters["@O_ErrTxt"].Value;
                        }
                    }
                }
                return new Tuple<int, List<DISDeviceInfoList>, string>(success, deviceInfos, error);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                Log.WriteToErrorLogFile(error);
                return new Tuple<int, List<DISDeviceInfoList>, string>(success, deviceInfos, error);
            }
        }
        #endregion

        #region Update Device Info
        public static Tuple<int, string> UpdateDeviceInfo(string lot, DataTable deviceLists, int oper, string eqp)
        {
            string error = string.Empty;
            int success = 1;

            try
            {
                using (SqlConnection con = new SqlConnection(strDISConnString))
                {
                    using (SqlCommand cmd = new SqlCommand("PR_DIS_UpdateDeviceInfo", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Tbl_DeviceInformationList", deviceLists);
                        cmd.Parameters.AddWithValue("@I_LotNumber", lot);
                        cmd.Parameters.AddWithValue("@I_Operation", oper);
                        cmd.Parameters.AddWithValue("@I_Equipment", eqp);
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
            catch (Exception ex)
            {
                error = ex.Message;
                Log.WriteToErrorLogFile(error);
                return new Tuple<int, string>(success, error);
            }
        }
        #endregion

        #region Split Device
        public static Tuple<int, string> SplitDevice(DataTable mDeviceList, DataTable cDeviceList, string mLot, string cLot, int oper, string eqp)
        {
            string error = string.Empty;
            int success = 1;

            try
            {
                using (SqlConnection con = new SqlConnection(strDISConnString))
                {
                    using (SqlCommand cmd = new SqlCommand("PR_DIS_UpdateSplitDevice", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Tbl_DeviceInformationList", mDeviceList);
                        cmd.Parameters.AddWithValue("@Tbl_ChildDeviceInformationList", cDeviceList);
                        cmd.Parameters.AddWithValue("@I_MotherLot", mLot);
                        cmd.Parameters.AddWithValue("@I_ChildLot", cLot);
                        cmd.Parameters.AddWithValue("@I_Operation", oper);
                        cmd.Parameters.AddWithValue("@I_Equipment", eqp);
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
            catch (Exception ex)
            {
                error = ex.Message;
                Log.WriteToErrorLogFile(error);
                return new Tuple<int, string>(success, error);
            }
        }
        #endregion

        #region Merge Device
        public static Tuple<int, string> MergeDevice(string mLot, string cLot, string cLot2, string cLot3, string cLot4,
                                                    int oper, string eqp)
        {
            string error = string.Empty;
            int success = 1;

            try
            {
                using (SqlConnection con = new SqlConnection(strDISConnString))
                {
                    using (SqlCommand cmd = new SqlCommand("PR_DIS_UpdateMergeDevice", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@I_MotherLot", mLot);
                        cmd.Parameters.AddWithValue("@I_ChildLot", cLot);
                        cmd.Parameters.AddWithValue("@I_ChildLot2", cLot2);
                        cmd.Parameters.AddWithValue("@I_ChildLot3", cLot3);
                        cmd.Parameters.AddWithValue("@I_ChildLot4", cLot4);
                        cmd.Parameters.AddWithValue("@I_Operation", oper);
                        cmd.Parameters.AddWithValue("@I_Equipment", eqp);
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
            catch (Exception ex)
            {
                error = ex.Message;
                Log.WriteToErrorLogFile(error);
                return new Tuple<int, string>(success, error);
            }
        }
        #endregion
    }
}