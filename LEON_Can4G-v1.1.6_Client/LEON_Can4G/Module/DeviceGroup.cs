using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LEON_Can4G.Module
{
    public class DeviceGroup
    {
        public Dictionary<String, Dictionary<String, String>> dicGroup = 
            new Dictionary<String, Dictionary<String, String>>();

        public Dictionary<String, String> dicDeviceData =
            new Dictionary<String, String>();

        public Dictionary<String, List<String>> dicUpload = new Dictionary<String, List<String>>();

        public Dictionary<String, String> InitialData()
        {
            Dictionary<String, String> dic =
            new Dictionary<String, String>();
            dic.Add("DEVICE_ID", "");
            dic.Add("DC_VOLT", "");           //88 16 80 28 F4 : 5 4
            dic.Add("AC_VOLT", "");           //88 16 90 30 28 : 7 6  
            dic.Add("AC_FREQ", "");           //88 16 90 30 28 : 9 8
            dic.Add("WORK_FREQ", "");         //88 16 90 30 28 : 11 10
            dic.Add("OUTPUT_VOLT", "");       //88 16 91 30 28 : 5 4
            dic.Add("OUTPUT_CURR", "");       //88 16 91 30 28 : 7 6
            dic.Add("LOAD_RATIO", "");        //88 16 91 30 28 : 9 8
            dic.Add("INNER_TEMP", "");        //88 16 91 30 28 : 11 10
            dic.Add("OUTER_TEMP", "");        //88 16 92 30 28 : 5 4
            dic.Add("OUTPUT_POWER", "");      //88 16 92 30 28 : 7 6
            dic.Add("RESERVE01", "");         //88 16 80 28 F4 : 9 8
            dic.Add("RESERVE02", "");         //市電電流:(((dVoltage * dCurrent * 1.35) / AC_VOLT) + OUTPUT_POWER / AC_VOLT * 1.03).ToString("0.00");
            dic.Add("RESERVE03", "");         //88 16 92 30 28 : 9 8
            dic.Add("RESERVE04", "0");
            dic.Add("RESERVE05", "0");
            dic.Add("RESERVE06", "0");
            dic.Add("RESERVE07", "0");
            dic.Add("RESERVE08", "0");
            dic.Add("RESERVE09", "0");
            dic.Add("RESERVE10", "0");
            dic.Add("RESERVE11", "0");
            dic.Add("CELL_VOLT01", "");       //88 18 00 28 F4 : 5 4
            dic.Add("CELL_VOLT02", "");       //88 18 00 28 F4 : 7 6
            dic.Add("CELL_VOLT03", "");       //88 18 00 28 F4 : 9 8
            dic.Add("CELL_VOLT04", "");       //88 18 00 28 F4 : 11 10
            dic.Add("CELL_VOLT05", "");       //88 18 01 28 F4 : 5 4
            dic.Add("CELL_VOLT06", "");       //88 18 01 28 F4 : 7 6
            dic.Add("CELL_VOLT07", "");       //88 18 01 28 F4 : 9 8
            dic.Add("CELL_VOLT08", "");       //88 18 01 28 F4 : 11 10
            dic.Add("CELL_VOLT09", "");       //86 18 02 28 F4 : 5 4
            dic.Add("CELL_VOLT10", "");       //86 18 02 28 F4 : 7 6
            dic.Add("CELL_VOLT11", "");       //86 18 02 28 F4 : 9 8
            dic.Add("CELL_VOLT12", " ");      //
            dic.Add("CELL_VOLT13", " ");      //
            dic.Add("CELL_VOLT14", " ");      //
            dic.Add("CELL_VOLT15", " ");      //
            dic.Add("CELL_VOLT16", " ");      //
            dic.Add("CELL_VOLT_SUM", "");     //88 16 80 28 F4 : 5 4
            dic.Add("CELL_CURR_SUM", "");     //88 16 80 28 F4 : 7 6
            dic.Add("CELL_TEMP01", "");       //88 18 A6 28 F4 : 4
            dic.Add("CELL_TEMP02", "");       //88 18 A6 28 F4 : 5
            dic.Add("CELL_TEMP03", "");       //88 18 A6 28 F4 : 6
            dic.Add("SYSTEM_CAPACITY", "");   //88 16 90 28 F4 : 7 6 5 4
            dic.Add("REMAIN_CAPACITY", "");   //88 16 90 28 F4 : 11 10 9 8
            dic.Add("REMAIN_RATIO", "");      //(SYSTEM_CAPACITY/REMAIN_CAPACITY)*100
            dic.Add("DISCHARGE_TIMES", "0");
            dic.Add("SYSTEM_STATE", "");      //88 16 92 30 28 : 9 8 000000000000000
            dic.Add("BATTERY_STATE", "0000000000000000");
            dic.Add("SYSTEM_PARAM", "0000000000000000");
            dic.Add("SYSTEM_STATE01", " ");    //88 16 92 30 28 : 9 8 用-切 0~15
            dic.Add("SYSTEM_STATE02", " ");
            dic.Add("SYSTEM_STATE03", " ");
            dic.Add("SYSTEM_STATE04", " ");
            dic.Add("SYSTEM_STATE05", " ");
            dic.Add("SYSTEM_STATE06", " ");
            dic.Add("SYSTEM_STATE07", " ");
            dic.Add("SYSTEM_STATE08", " ");
            dic.Add("SYSTEM_STATE09", " ");
            dic.Add("SYSTEM_STATE10", " ");
            dic.Add("SYSTEM_STATE11", " ");
            dic.Add("SYSTEM_STATE12", " ");
            dic.Add("SYSTEM_STATE13", " ");
            dic.Add("SYSTEM_STATE14", " ");
            dic.Add("SYSTEM_STATE15", " ");
            dic.Add("SYSTEM_STATE16", " ");
            dic.Add("AC_CURR", "");           //((CELL_VOLT_SUM * CELL_CURR_SUM * 1.35) + OUTPUT_POWER / AC_VOLT * 1.03).ToString("0.00");
            dic.Add("AC_POWER", "");           //((CELL_VOLT_SUM * CELL_CURR_SUM * 1.35) + OUTPUT_POWER / AC_VOLT * 1.03).ToString("0.00");
            dic.Add("DATA_CONTENT", "");      //將收到的原始數據紀錄下來
            dic.Add("ALERT_INFO01", "");      //SYSTEM_STATE01
            dic.Add("ALERT_INFO02", "");      //if[15]==1 SYSTEM_STATE02=0
            dic.Add("ALERT_INFO03", "");      //if[15]==1 SYSTEM_STATE03=0
            dic.Add("ALERT_INFO04", "");      //SYSTEM_STATE04
            dic.Add("ALERT_INFO05", "");      //if(CELL_VOLT_SUM)<21 SYSTEM_STATE05=1 else SYSTEM_STATE05=0
            dic.Add("ALERT_INFO06", "0");
            dic.Add("ALERT_INFO07", "0");
            dic.Add("ALERT_INFO08", "0");
            dic.Add("ALERT_INFO09", "0");
            dic.Add("ALERT_INFO10", "");      //SYSTEM_STATE10
            dic.Add("ALERT_INFO11", "");      //if(LOAD_RATIO<100) SYSTEM_STATE09
            dic.Add("ALERT_INFO12", "0");
            dic.Add("ALERT_INFO13", "");      //SYSTEM_STATE13
            dic.Add("ALERT_INFO14", "");      //SYSTEM_STATE14
            dic.Add("ALERT_INFO15", "");      //SYSTEM_STATE15
            dic.Add("ALERT_INFO16", "");      //if(SYSTEM_STATE16)==1 SYSTEM_STATE16=0 else SYSTEM_STATE16=1
            dic.Add("ALERT_RATIO", "?");
            dic.Add("UploadTime", "");
            return dic;
        }

        #region 預設數據內容
        public DeviceGroup()
        {
            dicDeviceData.Add("DEVICE_ID", "");
            dicDeviceData.Add("DC_VOLT", "");           //88 16 80 28 F4 : 5 4
            dicDeviceData.Add("AC_VOLT", "");           //88 16 90 30 28 : 7 6  
            dicDeviceData.Add("AC_FREQ", "");           //88 16 90 30 28 : 9 8
            dicDeviceData.Add("WORK_FREQ", "");         //88 16 90 30 28 : 11 10
            dicDeviceData.Add("OUTPUT_VOLT", "");       //88 16 91 30 28 : 5 4
            dicDeviceData.Add("OUTPUT_CURR", "");       //88 16 91 30 28 : 7 6
            dicDeviceData.Add("LOAD_RATIO", "");        //88 16 91 30 28 : 9 8
            dicDeviceData.Add("INNER_TEMP", "");        //88 16 91 30 28 : 11 10
            dicDeviceData.Add("OUTER_TEMP", "");        //88 16 92 30 28 : 5 4
            dicDeviceData.Add("OUTPUT_POWER", "");      //88 16 92 30 28 : 7 6
            dicDeviceData.Add("RESERVE01", "");         //88 16 80 28 F4 : 9 8
            dicDeviceData.Add("RESERVE02", "");         //市電電流:(((dVoltage * dCurrent * 1.35) / AC_VOLT) + OUTPUT_POWER / AC_VOLT * 1.03).ToString("0.00");
            dicDeviceData.Add("RESERVE03", "");         //88 16 92 30 28 : 9 8
            dicDeviceData.Add("RESERVE04", "0");
            dicDeviceData.Add("RESERVE05", "0");
            dicDeviceData.Add("RESERVE06", "0");
            dicDeviceData.Add("RESERVE07", "0");
            dicDeviceData.Add("RESERVE08", "0");
            dicDeviceData.Add("RESERVE09", "0");
            dicDeviceData.Add("RESERVE10", "0");
            dicDeviceData.Add("RESERVE11", "0");
            dicDeviceData.Add("CELL_VOLT01", "");       //88 18 00 28 F4 : 5 4
            dicDeviceData.Add("CELL_VOLT02", "");       //88 18 00 28 F4 : 7 6
            dicDeviceData.Add("CELL_VOLT03", "");       //88 18 00 28 F4 : 9 8
            dicDeviceData.Add("CELL_VOLT04", "");       //88 18 00 28 F4 : 11 10
            dicDeviceData.Add("CELL_VOLT05", "");       //88 18 01 28 F4 : 5 4
            dicDeviceData.Add("CELL_VOLT06", "");       //88 18 01 28 F4 : 7 6
            dicDeviceData.Add("CELL_VOLT07", "");       //88 18 01 28 F4 : 9 8
            dicDeviceData.Add("CELL_VOLT08", "");       //88 18 01 28 F4 : 11 10
            dicDeviceData.Add("CELL_VOLT09", "");       //86 18 02 28 F4 : 5 4
            dicDeviceData.Add("CELL_VOLT10", "");       //86 18 02 28 F4 : 7 6
            dicDeviceData.Add("CELL_VOLT11", "");       //86 18 02 28 F4 : 9 8
            dicDeviceData.Add("CELL_VOLT12", " ");      //
            dicDeviceData.Add("CELL_VOLT13", " ");      //
            dicDeviceData.Add("CELL_VOLT14", " ");      //
            dicDeviceData.Add("CELL_VOLT15", " ");      //
            dicDeviceData.Add("CELL_VOLT16", " ");      //
            dicDeviceData.Add("CELL_VOLT_SUM", "");     //88 16 80 28 F4 : 5 4
            dicDeviceData.Add("CELL_CURR_SUM", "");     //88 16 80 28 F4 : 7 6
            dicDeviceData.Add("CELL_TEMP01", "");       //88 18 A6 28 F4 : 4
            dicDeviceData.Add("CELL_TEMP02", "");       //88 18 A6 28 F4 : 5
            dicDeviceData.Add("CELL_TEMP03", "");       //88 18 A6 28 F4 : 6
            dicDeviceData.Add("SYSTEM_CAPACITY", "");   //88 16 90 28 F4 : 7 6 5 4
            dicDeviceData.Add("REMAIN_CAPACITY", "");   //88 16 90 28 F4 : 11 10 9 8
            dicDeviceData.Add("REMAIN_RATIO", "");      //(SYSTEM_CAPACITY/REMAIN_CAPACITY)*100
            dicDeviceData.Add("DISCHARGE_TIMES", "0");
            dicDeviceData.Add("SYSTEM_STATE", "");      //88 16 92 30 28 : 9 8 000000000000000
            dicDeviceData.Add("BATTERY_STATE", "0000000000000000");
            dicDeviceData.Add("SYSTEM_PARAM", "0000000000000000");
            dicDeviceData.Add("SYSTEM_STATE01", " ");    //88 16 92 30 28 : 9 8 用-切 0~15
            dicDeviceData.Add("SYSTEM_STATE02", " ");
            dicDeviceData.Add("SYSTEM_STATE03", " ");
            dicDeviceData.Add("SYSTEM_STATE04", " ");
            dicDeviceData.Add("SYSTEM_STATE05", " ");
            dicDeviceData.Add("SYSTEM_STATE06", " ");
            dicDeviceData.Add("SYSTEM_STATE07", " ");
            dicDeviceData.Add("SYSTEM_STATE08", " ");
            dicDeviceData.Add("SYSTEM_STATE09", " ");
            dicDeviceData.Add("SYSTEM_STATE10", " ");
            dicDeviceData.Add("SYSTEM_STATE11", " ");
            dicDeviceData.Add("SYSTEM_STATE12", " ");
            dicDeviceData.Add("SYSTEM_STATE13", " ");
            dicDeviceData.Add("SYSTEM_STATE14", " ");
            dicDeviceData.Add("SYSTEM_STATE15", " ");
            dicDeviceData.Add("SYSTEM_STATE16", " ");
            dicDeviceData.Add("AC_CURR", "");           //((CELL_VOLT_SUM * CELL_CURR_SUM * 1.35) + OUTPUT_POWER / AC_VOLT * 1.03).ToString("0.00");
            dicDeviceData.Add("AC_POWER", "");           //((CELL_VOLT_SUM * CELL_CURR_SUM * 1.35) + OUTPUT_POWER / AC_VOLT * 1.03).ToString("0.00");
            dicDeviceData.Add("DATA_CONTENT", "");      //將收到的原始數據紀錄下來
            dicDeviceData.Add("ALERT_INFO01", "");      //SYSTEM_STATE01
            dicDeviceData.Add("ALERT_INFO02", "");      //if[15]==1 SYSTEM_STATE02=0
            dicDeviceData.Add("ALERT_INFO03", "");      //if[15]==1 SYSTEM_STATE03=0
            dicDeviceData.Add("ALERT_INFO04", "");      //SYSTEM_STATE04
            dicDeviceData.Add("ALERT_INFO05", "");      //if(CELL_VOLT_SUM)<21 SYSTEM_STATE05=1 else SYSTEM_STATE05=0
            dicDeviceData.Add("ALERT_INFO06", "0");
            dicDeviceData.Add("ALERT_INFO07", "0");
            dicDeviceData.Add("ALERT_INFO08", "0");
            dicDeviceData.Add("ALERT_INFO09", "0");
            dicDeviceData.Add("ALERT_INFO10", "");      //SYSTEM_STATE10
            dicDeviceData.Add("ALERT_INFO11", "");      //if(LOAD_RATIO<100) SYSTEM_STATE09
            dicDeviceData.Add("ALERT_INFO12", "0");
            dicDeviceData.Add("ALERT_INFO13", "");      //SYSTEM_STATE13
            dicDeviceData.Add("ALERT_INFO14", "");      //SYSTEM_STATE14
            dicDeviceData.Add("ALERT_INFO15", "");      //SYSTEM_STATE15
            dicDeviceData.Add("ALERT_INFO16", "");      //if(SYSTEM_STATE16)==1 SYSTEM_STATE16=0 else SYSTEM_STATE16=1
            dicDeviceData.Add("ALERT_RATIO", "?");
            dicDeviceData.Add("UploadTime", "");
        }
        #endregion

        
    }
}
