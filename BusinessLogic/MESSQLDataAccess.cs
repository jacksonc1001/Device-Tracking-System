using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

/*
* Author: Jackson
* Date: 08/03/2022
* Version: 1.0.0.0
* Objective: Maintain all the MES database connection and transaction
*/

namespace Device_Tracking_System.BusinessLogic
{
    public class MESSQLDataAccess
    {
        private static readonly string strMESConnString = ConfigurationManager.ConnectionStrings["dbMESConn"].ConnectionString;

        #region Get Lot Info
        public static Tuple<int, int, int, string> GetLotInfo(string lotNum)
        {
            int operation = 0;
            int lotQty = 0;
            string error = string.Empty;
            int success = 1;
            try
            {
                using (SqlConnection con = new SqlConnection(strMESConnString))
                {
                    using (SqlCommand cmd = new SqlCommand("PR_MES_GetLotTransaction", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@I_LotNumber", lotNum);
                        cmd.Parameters.Add("@O_Success", SqlDbType.Int, 10);
                        cmd.Parameters["@O_Success"].Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("@O_ErrTxt", SqlDbType.VarChar, 250);
                        cmd.Parameters["@O_ErrTxt"].Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("@O_Operation", SqlDbType.Int, 99999);
                        cmd.Parameters["@O_Operation"].Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("@O_LotQty", SqlDbType.Int, 99999);
                        cmd.Parameters["@O_LotQty"].Direction = ParameterDirection.Output;
                        if (con.State != System.Data.ConnectionState.Open)
                        {
                            con.Open();

                            cmd.ExecuteNonQuery();

                            success = (int)cmd.Parameters["@O_Success"].Value;

                            error = (string)cmd.Parameters["@O_ErrTxt"].Value;

                            operation = (int)cmd.Parameters["@O_Operation"].Value;

                            lotQty = (int)cmd.Parameters["@O_LotQty"].Value;
                        }
                    }
                }

                return new Tuple<int, int, int, string>(success, operation, lotQty, error);
            }
            catch (SqlException sex)
            {
                error = sex.Message;
                Log.WriteToErrorLogFile(error);
                return new Tuple<int, int, int, string>(success, operation, lotQty, error);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                Log.WriteToErrorLogFile(error);
                return new Tuple<int, int, int, string>(success, operation, lotQty, error);
            }
        }
        #endregion

        #region Split Lot
        public static Tuple<int, string, string> SplitLot(string lotNum, int qty, int oper)
        {
            string nLotNum = string.Empty;
            string error = string.Empty;
            int success = 1;
            try
            {
                using (SqlConnection con = new SqlConnection(strMESConnString))
                {
                    using (SqlCommand cmd = new SqlCommand("PR_MES_SplitLot", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@I_LotNumber", lotNum);
                        cmd.Parameters.AddWithValue("@I_LotQty", qty);
                        cmd.Parameters.AddWithValue("@I_Operation", oper);
                        cmd.Parameters.Add("@O_Success", SqlDbType.Int, 10);
                        cmd.Parameters["@O_Success"].Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("@O_ErrTxt", SqlDbType.VarChar, 250);
                        cmd.Parameters["@O_ErrTxt"].Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("@O_NewLotNumber", SqlDbType.VarChar, 50);
                        cmd.Parameters["@O_NewLotNumber"].Direction = ParameterDirection.Output;
                        if (con.State != System.Data.ConnectionState.Open)
                        {
                            con.Open();

                            cmd.ExecuteNonQuery();

                            success = (int)cmd.Parameters["@O_Success"].Value;

                            error = (string)cmd.Parameters["@O_ErrTxt"].Value;

                            nLotNum = (string)cmd.Parameters["@O_NewLotNumber"].Value;

                        }
                    }
                }

                return new Tuple<int, string, string>(success, nLotNum, error);
            }
            catch (SqlException sex)
            {
                error = sex.Message;
                Log.WriteToErrorLogFile(error);
                return new Tuple<int, string, string>(success, nLotNum, error);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                Log.WriteToErrorLogFile(error);
                return new Tuple<int, string, string>(success, nLotNum, error);
            }
        }
        #endregion

        #region Merge Lot
        public static Tuple<int, string> MergeLot(string lotNum, string cLot, string cLot2, string cLot3, string cLot4)
        {
            string error = string.Empty;
            int success = 1;
            try
            {
                using (SqlConnection con = new SqlConnection(strMESConnString))
                {
                    using (SqlCommand cmd = new SqlCommand("PR_MES_MergeLot", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@I_MotherLot", lotNum);
                        cmd.Parameters.AddWithValue("@I_ChildLot", cLot);
                        cmd.Parameters.AddWithValue("@I_ChildLot2", cLot2);
                        cmd.Parameters.AddWithValue("@I_ChildLot3", cLot3);
                        cmd.Parameters.AddWithValue("@I_ChildLot4", cLot4);
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
        #endregion

        #region Update Validation Result
        public static Tuple<int, string> InsertValidationResult(string lotNum, string result)
        {
            string error = string.Empty;
            int success = 1;
            try
            {
                using (SqlConnection con = new SqlConnection(strMESConnString))
                {
                    using (SqlCommand cmd = new SqlCommand("PR_MES_InsertValidationResult", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@I_LotNumber", lotNum);
                        cmd.Parameters.AddWithValue("@I_Result", result);
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
        #endregion
    }
}