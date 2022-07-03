using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LEON_Can4G.Module
{
    class modProcess
    {
        
        #region Func.: 透過SQL Conn取得DataTable GetDataTableFromSQL_Adapter(string sCONN, string sQueryCMD, string sDataMember)
        public static DataTable GetDataTableFromSQL_Adapter(string sCONN, string sQueryCMD, string sDataMember)
        {
            DataSet dsData = new DataSet();
            DataTable dtReturn = new DataTable();

            using (SqlConnection connection = new SqlConnection(sCONN))
            {
                try //資料庫連線
                {
                    connection.Open();
                }
                catch (Exception e1)
                {
                    throw;
                    //MessageBox.Show("資料庫連線失敗:" + e1.Message);
                }
                SqlCommand command = new SqlCommand();
                command.Connection = connection;
                command.CommandText = sQueryCMD;

                SqlDataAdapter adapter = new SqlDataAdapter();
                adapter.SelectCommand = command;

                dsData.Tables.Clear();    //清除DataSet
                dsData.AcceptChanges();   //DataSet異動確認
                if (dsData.Tables[sDataMember] != null) //清除DataTable's DataMember
                {
                    DataTable _dt = dsData.Tables[sDataMember];
                    _dt.Rows.Clear();
                }
                try //資料庫綁定
                {
                    adapter.Fill(dsData, sDataMember);
                    dtReturn = dsData.Tables[sDataMember];
                }
                catch (Exception e2)
                {
                    throw;
                    //MessageBox.Show("資料庫載入失敗:" + e2.Message);
                }
                // The connection is automatically closed when the
                // code exits the using block.
            }
            return dtReturn;
        }
        #endregion

        #region Func.:_Save2ImachineSignals(String iUID, int iRes, float fCT, int iCount) 紀錄ImachineSignals資料(表頭)
        /// <summary>
        /// 紀錄上傳
        /// </summary>
        /// <param name="dic">Bartty CAN數據資料</param>
        public void _Save2BatteryData(Dictionary<String,String> dic)
        {
            using (SqlConnection sqlcnn = new SqlConnection(ConfigurationManager.ConnectionStrings["LEON_DB"].ToString()))
            {
                try //資料庫連線
                {
                    sqlcnn.Open();
                }
                catch (Exception e1)
                {
                    //MessageBox.Show("資料庫連線失敗:" + e1.Message);
                    throw ;
                }
                try
                {
                    //sqlcnn.Open();  //開啟資料庫連接
                    string sSQL = @"INSERT INTO [4GDTU_DB].[dbo].[RG_DEVICE_DATA] 
                                     VALUES (@DEVICE_ID,
                                             @DC_VOLT,@AC_VOLT,
                                             @AC_FREQ,@WORK_FREQ,
                                             @OUTPUT_VOLT,@OUTPUT_CURR,
                                             @LOAD_RATIO,@INNER_TEMP,
                                             @OUTER_TEMP,@OUTPUT_POWER,
                                             @RESERVE01,@RESERVE02,@RESERVE03,@RESERVE04,@RESERVE05,@RESERVE06,@RESERVE07,@RESERVE08,@RESERVE09,@RESERVE10,@RESERVE11,
                                             @CELL_VOLT01,@CELL_VOLT02,@CELL_VOLT03,@CELL_VOLT04,@CELL_VOLT05,@CELL_VOLT06,
                                             @CELL_VOLT07,@CELL_VOLT08,@CELL_VOLT09,@CELL_VOLT10,
                                             @CELL_VOLT11,@CELL_VOLT12,@CELL_VOLT13,@CELL_VOLT14,
                                             @CELL_VOLT15,@CELL_VOLT16,@CELL_VOLT_SUM,@CELL_CURR_SUM,
                                             @CELL_TEMP01,@CELL_TEMP02,@CELL_TEMP03,
                                             @SYSTEM_CAPACITY,@REMAIN_CAPACITY,@REMAIN_RATIO,@DISCHARGE_TIMES,
                                             @SYSTEM_STATE,@BATTERY_STATE,@SYSTEM_PARAM,getdate(),
                                             @SYSTEM_STATE01,@SYSTEM_STATE02,@SYSTEM_STATE03,@SYSTEM_STATE04,
                                             @SYSTEM_STATE05,@SYSTEM_STATE06,@SYSTEM_STATE07,@SYSTEM_STATE08,
                                             @SYSTEM_STATE09,@SYSTEM_STATE10,@SYSTEM_STATE11,@SYSTEM_STATE12,
                                             @SYSTEM_STATE13,@SYSTEM_STATE14,@SYSTEM_STATE15,@SYSTEM_STATE16,
                                             @AC_CURR,@AC_POWER,@DATA_CONTENT,
                                             @ALERT_INFO01,@ALERT_INFO02,@ALERT_INFO03,@ALERT_INFO04,
                                             @ALERT_INFO05,@ALERT_INFO06,@ALERT_INFO07,@ALERT_INFO08,
                                             @ALERT_INFO09,@ALERT_INFO10,@ALERT_INFO11,@ALERT_INFO12,
                                             @ALERT_INFO13,@ALERT_INFO14,@ALERT_INFO15,@ALERT_INFO16,@ALERT_RATIO) ";
                    using (SqlCommand cmd = sqlcnn.CreateCommand())
                    {
                        cmd.CommandText = sSQL;
                        cmd.Parameters.AddWithValue("@DEVICE_ID", dic["DEVICE_ID"]);
                        cmd.Parameters.AddWithValue("@DC_VOLT", dic["DC_VOLT"] + "V");
                        cmd.Parameters.AddWithValue("@AC_VOLT", dic["AC_VOLT"] + "V");
                        cmd.Parameters.AddWithValue("@AC_FREQ", dic["AC_FREQ"] + "Hz");
                        cmd.Parameters.AddWithValue("@WORK_FREQ", dic["WORK_FREQ"] + "Hz");
                        cmd.Parameters.AddWithValue("@OUTPUT_VOLT", dic["OUTPUT_VOLT"] + "V");
                        cmd.Parameters.AddWithValue("@OUTPUT_CURR", dic["OUTPUT_CURR"] + "A");
                        cmd.Parameters.AddWithValue("@LOAD_RATIO", dic["LOAD_RATIO"] + "%");
                        cmd.Parameters.AddWithValue("@INNER_TEMP", dic["INNER_TEMP"] + "℃");
                        cmd.Parameters.AddWithValue("@OUTER_TEMP", dic["OUTER_TEMP"] + "℃");
                        cmd.Parameters.AddWithValue("@OUTPUT_POWER", dic["OUTPUT_POWER"] + "W");
                        cmd.Parameters.AddWithValue("@RESERVE01", dic["RESERVE01"] + "%");
                        cmd.Parameters.AddWithValue("@RESERVE02", dic["RESERVE02"] + "A");
                        cmd.Parameters.AddWithValue("@RESERVE03", dic["RESERVE03"]);
                        cmd.Parameters.AddWithValue("@RESERVE04", dic["RESERVE04"]);
                        cmd.Parameters.AddWithValue("@RESERVE05", dic["RESERVE05"]);
                        cmd.Parameters.AddWithValue("@RESERVE06", dic["RESERVE06"]);
                        cmd.Parameters.AddWithValue("@RESERVE07", dic["RESERVE07"]);
                        cmd.Parameters.AddWithValue("@RESERVE08", dic["RESERVE08"]);
                        cmd.Parameters.AddWithValue("@RESERVE09", dic["RESERVE09"]);
                        cmd.Parameters.AddWithValue("@RESERVE10", dic["RESERVE10"]);
                        cmd.Parameters.AddWithValue("@RESERVE11", dic["RESERVE11"]);
                        cmd.Parameters.AddWithValue("@CELL_VOLT01", dic["CELL_VOLT01"]);
                        cmd.Parameters.AddWithValue("@CELL_VOLT02", dic["CELL_VOLT02"]);
                        cmd.Parameters.AddWithValue("@CELL_VOLT03", dic["CELL_VOLT03"]);
                        cmd.Parameters.AddWithValue("@CELL_VOLT04", dic["CELL_VOLT04"]);
                        cmd.Parameters.AddWithValue("@CELL_VOLT05", dic["CELL_VOLT05"]);
                        cmd.Parameters.AddWithValue("@CELL_VOLT06", dic["CELL_VOLT06"]);
                        cmd.Parameters.AddWithValue("@CELL_VOLT07", dic["CELL_VOLT07"]);
                        cmd.Parameters.AddWithValue("@CELL_VOLT08", dic["CELL_VOLT08"]);
                        cmd.Parameters.AddWithValue("@CELL_VOLT09", dic["CELL_VOLT09"]);
                        cmd.Parameters.AddWithValue("@CELL_VOLT10", dic["CELL_VOLT10"]);
                        cmd.Parameters.AddWithValue("@CELL_VOLT11", dic["CELL_VOLT11"]);
                        cmd.Parameters.AddWithValue("@CELL_VOLT12", dic["CELL_VOLT12"]);
                        cmd.Parameters.AddWithValue("@CELL_VOLT13", dic["CELL_VOLT13"]);
                        cmd.Parameters.AddWithValue("@CELL_VOLT14", dic["CELL_VOLT14"]);
                        cmd.Parameters.AddWithValue("@CELL_VOLT15", dic["CELL_VOLT15"]);
                        cmd.Parameters.AddWithValue("@CELL_VOLT16", dic["CELL_VOLT16"]);
                        cmd.Parameters.AddWithValue("@CELL_VOLT_SUM", dic["CELL_VOLT_SUM"] + "V");
                        cmd.Parameters.AddWithValue("@CELL_CURR_SUM", dic["CELL_CURR_SUM"] + "A");
                        cmd.Parameters.AddWithValue("@CELL_TEMP01", dic["CELL_TEMP01"] + "℃");
                        cmd.Parameters.AddWithValue("@CELL_TEMP02", dic["CELL_TEMP02"] + "℃");
                        cmd.Parameters.AddWithValue("@CELL_TEMP03", dic["CELL_TEMP03"] + "℃");
                        cmd.Parameters.AddWithValue("@SYSTEM_CAPACITY", dic["SYSTEM_CAPACITY"]);
                        cmd.Parameters.AddWithValue("@REMAIN_CAPACITY", dic["REMAIN_CAPACITY"]);
                        cmd.Parameters.AddWithValue("@REMAIN_RATIO", dic["REMAIN_RATIO"] + "%");
                        cmd.Parameters.AddWithValue("@DISCHARGE_TIMES", dic["DISCHARGE_TIMES"]);
                        cmd.Parameters.AddWithValue("@SYSTEM_STATE", dic["SYSTEM_STATE"]);
                        cmd.Parameters.AddWithValue("@BATTERY_STATE", dic["BATTERY_STATE"]);
                        cmd.Parameters.AddWithValue("@SYSTEM_PARAM", dic["SYSTEM_PARAM"]);
                        cmd.Parameters.AddWithValue("@SYSTEM_STATE01", dic["SYSTEM_STATE01"]);
                        cmd.Parameters.AddWithValue("@SYSTEM_STATE02", dic["SYSTEM_STATE02"]);
                        cmd.Parameters.AddWithValue("@SYSTEM_STATE03", dic["SYSTEM_STATE03"]);
                        cmd.Parameters.AddWithValue("@SYSTEM_STATE04", dic["SYSTEM_STATE04"]);
                        cmd.Parameters.AddWithValue("@SYSTEM_STATE05", dic["SYSTEM_STATE05"]);
                        cmd.Parameters.AddWithValue("@SYSTEM_STATE06", dic["SYSTEM_STATE06"]);
                        cmd.Parameters.AddWithValue("@SYSTEM_STATE07", dic["SYSTEM_STATE07"]);
                        cmd.Parameters.AddWithValue("@SYSTEM_STATE08", dic["SYSTEM_STATE08"]);
                        cmd.Parameters.AddWithValue("@SYSTEM_STATE09", dic["SYSTEM_STATE09"]);
                        cmd.Parameters.AddWithValue("@SYSTEM_STATE10", dic["SYSTEM_STATE10"]);
                        cmd.Parameters.AddWithValue("@SYSTEM_STATE11", dic["SYSTEM_STATE11"]);
                        cmd.Parameters.AddWithValue("@SYSTEM_STATE12", dic["SYSTEM_STATE12"]);
                        cmd.Parameters.AddWithValue("@SYSTEM_STATE13", dic["SYSTEM_STATE13"]);
                        cmd.Parameters.AddWithValue("@SYSTEM_STATE14", dic["SYSTEM_STATE14"]);
                        cmd.Parameters.AddWithValue("@SYSTEM_STATE15", dic["SYSTEM_STATE15"]);
                        cmd.Parameters.AddWithValue("@SYSTEM_STATE16", dic["SYSTEM_STATE16"]);
                        cmd.Parameters.AddWithValue("@AC_CURR", dic["AC_CURR"] + "A");
                        cmd.Parameters.AddWithValue("@AC_POWER", dic["AC_POWER"] + "W");
                        cmd.Parameters.AddWithValue("@DATA_CONTENT", dic["DATA_CONTENT"]);
                        cmd.Parameters.AddWithValue("@ALERT_INFO01", dic["ALERT_INFO01"]);
                        cmd.Parameters.AddWithValue("@ALERT_INFO02", dic["ALERT_INFO02"]);
                        cmd.Parameters.AddWithValue("@ALERT_INFO03", dic["ALERT_INFO03"]);
                        cmd.Parameters.AddWithValue("@ALERT_INFO04", dic["ALERT_INFO04"]);
                        cmd.Parameters.AddWithValue("@ALERT_INFO05", dic["ALERT_INFO05"]);
                        cmd.Parameters.AddWithValue("@ALERT_INFO06", dic["ALERT_INFO06"]);
                        cmd.Parameters.AddWithValue("@ALERT_INFO07", dic["ALERT_INFO07"]);
                        cmd.Parameters.AddWithValue("@ALERT_INFO08", dic["ALERT_INFO08"]);
                        cmd.Parameters.AddWithValue("@ALERT_INFO09", dic["ALERT_INFO09"]);
                        cmd.Parameters.AddWithValue("@ALERT_INFO10", dic["ALERT_INFO10"]);
                        cmd.Parameters.AddWithValue("@ALERT_INFO11", dic["ALERT_INFO11"]);
                        cmd.Parameters.AddWithValue("@ALERT_INFO12", dic["ALERT_INFO12"]);
                        cmd.Parameters.AddWithValue("@ALERT_INFO13", dic["ALERT_INFO13"]);
                        cmd.Parameters.AddWithValue("@ALERT_INFO14", dic["ALERT_INFO14"]);
                        cmd.Parameters.AddWithValue("@ALERT_INFO15", dic["ALERT_INFO15"]);
                        cmd.Parameters.AddWithValue("@ALERT_INFO16", dic["ALERT_INFO16"]);
                        cmd.Parameters.AddWithValue("@ALERT_RATIO", dic["ALERT_RATIO"]);
                        
                        var id = cmd.ExecuteScalar();
                        sqlcnn.Close();    //關閉資料庫連接
                    }
                    //MessageBox.Show("資料新增完成", modProject.sAppName, MessageBoxButtons.OK, MessageBoxIcon.Information);

                }
                catch (Exception ex)
                {
                    //MessageBox.Show("系統錯誤:\n" + ex.ToString(), frmVacPress_Demo.ActiveForm.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    if (sqlcnn.State == ConnectionState.Open) sqlcnn.Close();
                    throw;
                }
            }
        }
        #endregion

    }
}
