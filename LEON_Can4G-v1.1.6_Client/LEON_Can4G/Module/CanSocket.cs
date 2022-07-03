

//==============================================================================================
// * Copyright (C) Real-Good Inc.
//
// Class: modSocket
// 
// TCP/IP Socket 
// 
//   
//==============================================================================================
using System;
using System.Configuration;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using HslCommunication.LogNet;
using System.Windows.Forms;


namespace LEON_Can4G.Module
{
    public class CanSocket
    {
        private Socket[] _SocketListener;
        private frmMain _frmMain;
        public Boolean CloseReceive = true; //結束Client收集
        //byte[] TestByte = new byte[1];  //檢查連線用
        Int32 SckCIndex = 0;    //目前正在使用的Socket數量
        Int32 RDataLen = 1024;  //緩衝區
        Dictionary<String, Socket> dicSockClinet = new Dictionary<String, Socket>();
        Dictionary<Socket, Boolean> dicCloseReceive = new Dictionary<Socket, Boolean>();
        public List<String> lsLog = new List<string>(); 
        public DeviceGroup objDeviceGroup = new DeviceGroup();
        public Boolean bOKUpload = false;
        public CanSocket(frmMain f)
        {
            _frmMain = f;
        }

        public void Listen(string LocalIP, string SPort)
        {
            // 用 Resize 的方式動態增加 Socket 的數目
            Array.Resize(ref _SocketListener, 1);
            //CloseReceive = true;
            _SocketListener[0] = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            dicCloseReceive.Add(_SocketListener[0], true);
            _SocketListener[0].Bind(new IPEndPoint(IPAddress.Parse(LocalIP), Int32.Parse(SPort)));
            // 其中 LocalIP 和 SPort 分別為 string 和 int 型態, 前者為 Server 端的IP, 後者為S erver 端的Port
            _SocketListener[0].Listen(10); // 進行聆聽; Listen( )為允許 Client 同時連線的最大數
            SckSWaitAccept();   // 另外寫一個函數用來分配 Client 端的 Socket
        }

        // 等待Client連線
        private void SckSWaitAccept()
        {
            // 判斷目前是否有空的 Socket 可以提供給Client端連線
            bool FlagFinded = false;
            for (Int32 i = 1; i < _SocketListener.Length; i++)
            {

                // SckSs[i] 若不為 null 表示已被實作過, 判斷是否有 Client 端連線
                if (_SocketListener[i] != null)
                {
                    if (i != SckCIndex)
                    {
                        ////如果新的Client與舊的有一樣，把舊的中斷
                        //if (_SocketListener[i].RemoteEndPoint.ToString().Split(':')[0] == _SocketListener[SckCIndex].RemoteEndPoint.ToString().Split(':')[0])
                        //{
                        //    _SocketListener[i].Disconnect(false);
                        //    _SocketListener[i] = null;
                        //    SckCIndex = i;
                        //    FlagFinded = true;
                        //    break;
                        //}
                        // 如果目前第 i 個 Socket 若沒有人連線, 便可提供給下一個 Client 進行連線
                        if (_SocketListener[i].Connected == false)
                        {
                            SckCIndex = i;
                            FlagFinded = true;
                            Debug.Write("第" + SckCIndex + "條Client不等於null，但沒有連線" + "\n");
                            lsLog.Add("系統:" + "第" + SckCIndex + "條Client不等於null，但沒有連線" + "\n");
                            break;
                        }
                    }
                }
                else
                {
                    SckCIndex = i;
                    FlagFinded = true;
                    Debug.Write("第" + SckCIndex + "條Client等於null" + "\n");
                    lsLog.Add("系統:" + "第" + SckCIndex + "條Client等於null" + "\n");
                    break;
                }
            }
            // 如果 FlagFinded 為 false 表示目前並沒有多餘的 Socket 可供 Client 連線
            if (FlagFinded == false)
            {
                // 增加 Socket 的數目以供下一個 Client 端進行連線
                SckCIndex = _SocketListener.Length;
                Array.Resize(ref _SocketListener, SckCIndex + 1);
                Debug.Write("連線數已滿，增加第" + SckCIndex + "條Client" + "\n");
                lsLog.Add("系統:" + "連線數已滿，增加第" + SckCIndex + "條Client" + "\n");
            }
            // 以下兩行為多執行緒的寫法, 因為接下來 Server 端的部份要使用 Accept() 讓 Cleint 進行連線;
            // 該執行緒有需要時再產生即可, 因此定義為區域性的 Thread. 命名為 SckSAcceptTd;
            // 在 new Thread( ) 裡為要多執行緒去執行的函數. 這裡命名為 SckSAcceptProc;
            Thread SckSAcceptTd = new Thread(SckSAcceptProc);
            SckSAcceptTd.Start();  // 開始執行 SckSAcceptTd 這個執行緒
            // 這裡要點出 SckSacceptTd 這個執行緒會在 Start( ) 之後開始執行 SckSAcceptProc 裡的程式碼, 同時主程式的執行緒也會繼續往下執行各做各的. 
            // 主程式不用等到 SckSAcceptProc 的程式碼執行完便會繼續往下執行.
        }

        // 接收來自Client的連線與Client傳來的資料
        private void SckSAcceptProc()
        {
            // 這裡加入 try 是因為 SckSs[0] 若被 Close 的話, SckSs[0].Accept() 會產生錯誤
            Debug.Write("第" + SckCIndex + "條Client，等待連線" + "\n");
            lsLog.Add("系統:" + "第" + SckCIndex + "條Client，等待連線" + "\n");
            try
            {
                _SocketListener[SckCIndex] = _SocketListener[0].Accept();  // 等待Client 端連線
            }
            catch(SocketException sockex)
            {
                lsLog.Add(sockex.ToString());
                return;
            }
            if (!dicCloseReceive.ContainsKey(_SocketListener[SckCIndex]))
            {
                dicCloseReceive.Add(_SocketListener[SckCIndex], true);
            }
            else
            {
                dicCloseReceive[_SocketListener[SckCIndex]] = true;
            }
            // 為什麼 Accept 部份要用多執行緒, 因為 SckSs[0] 會停在這一行程式碼直到有 Client 端連上線, 並分配給 SckSs[SckCIndex] 給 Client 連線之後程式才會繼續往下, 若是將 Accept 寫在主執行緒裡, 在沒有Client連上來之前, 主程式將會被hand在這一行無法再做任何事了!!
            // 能來這表示有 Client 連上線. 記錄該 Client 對應的SckCIndex
            Int32 Scki = SckCIndex;
            Debug.Write("第" + Scki + "條Client，連線中，產生另一個執行緒等待下一個 Client 連線" + "\n");
            lsLog.Add("系統:" + "第" + Scki + "條Client，連線中，產生另一個執行緒等待下一個 Client 連線" + "\n");
            // 再產生另一個執行緒等待下一個 Client 連線
            SckSWaitAccept();
            byte[] clientData;  // 其中RDataLen為每次要接受來自 Client 傳來的資料長度
            String Station = String.Empty;  //DeivceID
            String sRegister = String.Empty;    //暫存字串
            Boolean bIDFlag = false; //false : 未登記DeivceID ， true : 已登記DeivceID
            Int32 index = 0;//查詢ID在字串的位置
            Boolean[] bSuccessful = new Boolean[10];
            Boolean bNext = false;
            modProcess objmodprocess = new modProcess();
            DateTime sTime = DateTime.Now;
            while (dicCloseReceive[_SocketListener[Scki]])
            {
                try
                {
                    //Debug.WriteIf(_SocketListener[Scki].Available > 0, "第" + Scki + "條Client，接收到:" + _SocketListener[Scki].Available + "\n");
                    //lsLog.Add("第" + Scki + "條Client，接收到:" + _SocketListener[Scki].Available + "\n");
                    if (_SocketListener[Scki].Available > 0)
                    {
                        if (bNext)
                        {
                            Debug.WriteIf(bNext, "第" + Scki + "條Client，上傳完畢，等待下批資料" + "\n");
                            lsLog.Add("系統:" + "第" + Scki + "條Client，上傳完畢，等待下批資料" + "\n");
                            if (DateTime.Now < sTime.AddSeconds(15))
                            {
                                Debug.WriteIf(DateTime.Now < sTime.AddSeconds(15), "第" + Scki + "條Client還在15秒內，清除網路暫存數量:" + _SocketListener[Scki].Available + "\n");
                                lsLog.Add("系統:" + "第" + Scki + "條Client還在15秒內，清除網路暫存數量:" + _SocketListener[Scki].Available + "\n");
                                clientData = new byte[_SocketListener[Scki].Available];
                                Int32 icount = _SocketListener[Scki].Receive(clientData);
                                sRegister = Encoding.ASCII.GetString(clientData, 0, icount);
                                sRegister = "";
                                continue;
                            }
                            else
                            {
                                bNext = false;
                                clientData = new byte[RDataLen];
                                Debug.Assert(DateTime.Now > sTime.AddSeconds(15), "第" + Scki + "條Client，恢復CANID判斷，網路暫存數量:" + _SocketListener[Scki].Available + "\n");
                                lsLog.Add("系統:" + "第" + Scki + "條Client，恢復CANID判斷，網路暫存數量:" + _SocketListener[Scki].Available + "\n");
                            }
                        }
                        else
                        {
                            clientData = new byte[RDataLen];
                            Debug.Write("第" + Scki + "條Client，讀取:" + RDataLen + " 網路暫存數量:" + _SocketListener[Scki].Available + "\n");
                            lsLog.Add("系統:" + "第" + Scki + "條Client，讀取:" + RDataLen + " 網路暫存數量:" + _SocketListener[Scki].Available + "\n");
                        }
                        // 程式會被 hand 在此, 等待接收來自 Client 端傳來的資料
                        Int32 iRx = _SocketListener[Scki].Receive(clientData);
                        if (iRx != 0)
                        {
                            #region 登記DeviceID
                            //clientData = Processing_Data_Incomplete(clientData, bytes, iRx);
                            if (!bIDFlag)
                            {
                                Debug.Write("第" + Scki + "條Client，登記DeviceID" + "\n");
                                lsLog.Add("系統:" + "第" + Scki + "條Client，登記DeviceID" + "\n");
                                sRegister += Encoding.ASCII.GetString(clientData, 0, iRx);
                                try
                                {
                                    if (sRegister.Length < 49)
                                    {
                                        Debug.Write(bIDFlag, "第" + Scki + "條Client，收到的字數小於49，跳出等待下次" + "\n");
                                        lsLog.Add("系統:" + "第" + Scki + "條Client，收到的字數小於49，跳出等待下次" + "\n");
                                        continue;
                                    }
                                    if (sRegister.Contains("QK-G400E"))
                                    {
                                        //在此Client執行緒登記DeivceID
                                        Station = sRegister.Substring(41, 10);
                                        Debug.Write(sRegister.Contains("QK-G400E"), "第" + Scki + "條Client，設備ID:" + Station + "\n");
                                        lsLog.Add("系統:" + "第" + Scki + "條Client，設備ID:" + Station + "\n");
                                        //如果存在此DeivceID，清空Group，不存在此DeivceID，重新建立Group
                                        if (objDeviceGroup.dicGroup.ContainsKey(Station))
                                        {
                                            objDeviceGroup.dicGroup[Station].Clear();
                                            objDeviceGroup.dicGroup[Station] = objDeviceGroup.InitialData();
                                            Debug.Write(objDeviceGroup.dicGroup.ContainsKey(Station), Station + " 匯入空白群組" + "\n");
                                            lsLog.Add("系統:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + " 匯入空白群組" + "\n");
                                        }
                                        else
                                        {
                                            Dictionary<String, String> dic = objDeviceGroup.InitialData();
                                            objDeviceGroup.dicGroup.Add(Station, dic);
                                            objDeviceGroup.dicGroup[Station]["DEVICE_ID"] = Station;
                                            Debug.Write(objDeviceGroup.dicGroup.ContainsKey(Station), "建立" + Station + " 到群組" + "\n");
                                            lsLog.Add("系統:" + "建立" + Station + " 到群組" + "\n");
                                        }
                                        //如果存在此DeivceID，更新Socket，不存在此DeivceID，登記目前Socket
                                        if (!dicSockClinet.ContainsKey(Station))
                                        {
                                            dicSockClinet.Add(Station, _SocketListener[Scki]);
                                            Debug.Write(objDeviceGroup.dicGroup.ContainsKey(Station), "建立到Socket字典裡" + Scki + "\n");
                                            lsLog.Add("系統:" + "建立到Socket字典裡" + Scki + "\n");
                                        }
                                        else
                                        {
                                            dicCloseReceive[dicSockClinet[Station]] = false;
                                            dicSockClinet[Station].Close();
                                            dicSockClinet[Station] = _SocketListener[Scki];
                                            Debug.Write(objDeviceGroup.dicGroup.ContainsKey(Station), "關閉舊的Socket，紀錄前Socket" + Scki + "\n");
                                            lsLog.Add("系統:" + "關閉舊的Socket，紀錄前Socket" + Scki + "\n");
                                        }
                                        bIDFlag = true;
                                    }
                                    else
                                    {
                                        //_SocketListener[Scki].Disconnect(false);
                                        dicCloseReceive[_SocketListener[Scki]] = false;
                                        _SocketListener[Scki].Close();
                                        Debug.Write(sRegister.Contains("QK-G400E"), "第" + Scki + "條Client中斷連線，不符合設定提供的ID格式:" + sRegister + "\n");
                                        lsLog.Add("系統:" + "第" + Scki + "條Client中斷連線，不符合設定提供的ID格式:" + sRegister + "\n");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _SocketListener[Scki].Close();
                                    //_SocketListener[Scki] = null;
                                    MessageBox.Show("設備ID:" + Station + "錯誤訊息:" + ex.ToString() + "\r\n" + sRegister);
                                }
                                sRegister = "";
                                //break;
                            }
                            #endregion

                            else
                            {
                                String sData = String.Empty;//暫存該CAN ID的原始數據
                                //88-16-80-28-F4-F8-00-86-7D-D5-02-08-03
                                sRegister += BitConverter.ToString(clientData, 0, iRx).Replace("-", " ") + " ";
                                //字數小於38是不完整的一筆CAN ID數據
                                if (sRegister.Length <= 38)
                                {
                                    //sRegister = "";
                                    continue;
                                }
                                try
                                {
                                    #region CAN ID : 88 16 80 28 F4
                                    if (sRegister.Contains("88 16 80 28 F4"))
                                    {
                                        Debug.Write(Scki + "原始數據存在88 16 80 28 F4" + "\n");
                                        lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "原始數據存在88 16 80 28 F4" + "\n");
                                        if (!bSuccessful[0])
                                        {
                                            Debug.Write(Scki + "開始分析:88 16 80 28 F4" + "\n");
                                            lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "開始分析:88 16 80 28 F4" + "\n");
                                            index = sRegister.IndexOf("88 16 80 28 F4");
                                            if (sRegister.Length - index - 38 >= 0)
                                            {
                                                sData = sRegister.Substring(index, 38);
                                                Debug.Write(Scki + "原始內容:" + sData + "\n");
                                                lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "原始內容:" + sData + "\n");
                                                objDeviceGroup.dicGroup[Station]["DATA_CONTENT"] += sData + " ";
                                                String[] sArray = sData.Split(' ');
                                                Double dvoltage = Convert.ToInt32((sArray[6] + sArray[5]), 16) / (double)10;
                                                objDeviceGroup.dicGroup[Station]["DC_VOLT"] = dvoltage.ToString("0.00");
                                                Debug.Write(Scki + "DC_VOLT:" + dvoltage.ToString("0.00") + "\n");
                                                lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "DC_VOLT:" + dvoltage.ToString("0.00") + "\n");
                                                objDeviceGroup.dicGroup[Station]["CELL_VOLT_SUM"] = dvoltage.ToString("0.00");
                                                Double dcurrent = (Convert.ToInt32((sArray[8] + sArray[7]), 16) - 32000) / (double)10;
                                                objDeviceGroup.dicGroup[Station]["CELL_CURR_SUM"] = dcurrent.ToString("0.000");
                                                Debug.Write(Scki + "CELL_CURR_SUM:" + dcurrent.ToString("0.000") + "\n");
                                                lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "CELL_CURR_SUM:" + dcurrent.ToString("0.000") + "\n");
                                                Double dsoc = Convert.ToInt32((sArray[10] + sArray[9]), 16) / (double)10;
                                                objDeviceGroup.dicGroup[Station]["RESERVE01"] = dsoc.ToString("0");
                                                Debug.Write(Scki + "RESERVE01:" + dsoc.ToString("0") + "\n");
                                                lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "RESERVE01:" + dsoc.ToString("0") + "\n");
                                                objDeviceGroup.dicGroup[Station]["REMAIN_RATIO"] = dsoc.ToString("0");
                                                sRegister = sRegister.Remove(index, 38);
                                                Debug.Write(Scki + "結束分析:88 16 80 28 F4" + "\n");
                                                lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "結束分析:88 16 80 28 F4" + "\n");
                                                bSuccessful[0] = true;
                                            }
                                        }
                                        else
                                        {
                                            index = sRegister.IndexOf("88 16 80 28 F4");
                                            sRegister.Remove(index, 14);
                                            Debug.Write(Scki + "已分析:88 16 80 28 F4" + "\n");
                                            lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "已分析:88 16 80 28 F4" + "\n");
                                        }
                                    }
                                    #endregion
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("設備ID:" + Station + "錯誤訊息:" + ex.Message + "\r\n" + sData);
                                }

                                try
                                {
                                    #region CAN ID : 88 16 90 28 F4
                                    if (sRegister.Contains("88 16 90 28 F4"))
                                    {
                                        Debug.Write(Scki + "原始數據存在88 16 90 28 F4" + "\n");
                                        lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "原始數據存在88 16 90 28 F4" + "\n");
                                        if (!bSuccessful[1])
                                        {
                                            Debug.Write(Scki + "開始分析:88 16 90 28 F4" + "\n");
                                            lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "開始分析:88 16 90 28 F4" + "\n");
                                            index = sRegister.IndexOf("88 16 90 28 F4");
                                            if (sRegister.Length - index - 38 >= 0)
                                            {
                                                try
                                                {
                                                    sData = sRegister.Substring(index, 38);
                                                    Debug.Write(Scki + "原始內容:" + sData + "\n");
                                                    lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "原始內容:" + sData + "\n");
                                                    objDeviceGroup.dicGroup[Station]["DATA_CONTENT"] += sData + " ";
                                                    String[] sArray = sData.Split(' ');
                                                    Int32 dsystem_capacity = Convert.ToInt32((sArray[8] + sArray[7] + sArray[6] + sArray[5]), 16);
                                                    objDeviceGroup.dicGroup[Station]["SYSTEM_CAPACITY"] = dsystem_capacity.ToString();
                                                    Debug.Write(Scki + "SYSTEM_CAPACITY:" + dsystem_capacity.ToString() + "\n");
                                                    lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "SYSTEM_CAPACITY:" + dsystem_capacity.ToString() + "\n");
                                                    Int32 dremain_capacity = Convert.ToInt32((sArray[12] + sArray[11] + sArray[10] + sArray[9]), 16);
                                                    objDeviceGroup.dicGroup[Station]["REMAIN_CAPACITY"] = dremain_capacity.ToString();
                                                    Debug.Write(Scki + "REMAIN_CAPACITY:" + dremain_capacity.ToString() + "\n");
                                                    lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "REMAIN_CAPACITY:" + dremain_capacity.ToString() + "\n");
                                                    //Double dremain_ratio = (dsystem_capacity / dremain_capacity) * 100;
                                                    sRegister = sRegister.Remove(index, 38);
                                                    Debug.Write(Scki + "結束分析:88 16 90 28 F4" + "\n");
                                                    lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "結束分析:88 16 90 28 F4" + "\n");
                                                    bSuccessful[1] = true;
                                                }
                                                catch (OverflowException ex)
                                                {
                                                    sRegister.Remove(index, 14);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            index = sRegister.IndexOf("88 16 90 28 F4");
                                            sRegister.Remove(index, 14);
                                            Debug.Write(Scki + "已分析:88 16 90 28 F4" + "\n");
                                            lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "已分析:88 16 90 28 F4" + "\n");
                                        }
                                    }
                                    #endregion
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("設備ID:" + Station + "錯誤訊息:" + ex.Message + "\r\n" + sData);

                                }

                                try
                                {
                                    #region CAN ID : 88 16 90 30 28
                                    if (sRegister.Contains("88 16 90 30 28"))
                                    {
                                        Debug.Write(Scki + "原始數據存在88 16 90 30 28" + "\n");
                                        lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "原始數據存在88 16 90 30 28" + "\n");
                                        if (!bSuccessful[2])
                                        {
                                            Debug.Write(Scki + "開始分析:88 16 90 30 28" + "\n");
                                            lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "開始分析:88 16 90 30 28" + "\n");
                                            index = sRegister.IndexOf("88 16 90 30 28");
                                            if (sRegister.Length - index - 38 >= 0)
                                            {
                                                sData = sRegister.Substring(index, 38);
                                                Debug.Write(Scki + "原始內容:" + sData + "\n");
                                                lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "原始內容:" + sData + "\n");
                                                objDeviceGroup.dicGroup[Station]["DATA_CONTENT"] += sData + " ";
                                                String[] sArray = sData.Split(' ');
                                                Double dmainsvoltage = Convert.ToInt32((sArray[8] + sArray[7]), 16) / (double)10;
                                                objDeviceGroup.dicGroup[Station]["AC_VOLT"] = dmainsvoltage.ToString("0.00");
                                                Debug.Write(Scki + "AC_VOLT:" + dmainsvoltage.ToString("0.00") + "\n");
                                                lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "AC_VOLT:" + dmainsvoltage.ToString("0.00") + "\n");
                                                Double dmainsfrequency = Convert.ToInt32((sArray[10] + sArray[9]), 16) / (double)10;
                                                objDeviceGroup.dicGroup[Station]["AC_FREQ"] = dmainsfrequency.ToString("0.00");
                                                Debug.Write(Scki + "AC_FREQ:" + dmainsfrequency.ToString("0.00") + "\n");
                                                lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "AC_FREQ:" + dmainsfrequency.ToString("0.00") + "\n");
                                                Double dinverterfrequency = Convert.ToInt32((sArray[12] + sArray[11]), 16) / (double)10;
                                                objDeviceGroup.dicGroup[Station]["WORK_FREQ"] = dinverterfrequency.ToString("0.00");
                                                Debug.Write(Scki + "WORK_FREQ:" + dinverterfrequency.ToString("0.00") + "\n");
                                                lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "WORK_FREQ:" + dinverterfrequency.ToString("0.00") + "\n");
                                                sRegister = sRegister.Remove(index, 38);
                                                Debug.Write(Scki + "結束分析:88 16 90 30 28" + "\n");
                                                lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "結束分析:88 16 90 30 28" + "\n");
                                                bSuccessful[2] = true;
                                            }
                                        }
                                        else
                                        {
                                            index = sRegister.IndexOf("88 16 90 30 28");
                                            sRegister.Remove(index, 14);
                                            Debug.Write(Scki + "已分析:88 16 90 30 28" + "\n");
                                            lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "已分析:88 16 90 30 28" + "\n");
                                        }
                                    }
                                    #endregion
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("設備ID:" + Station + "錯誤訊息:" + ex.Message + "\r\n" + sData);
                                }
                                try
                                {
                                    #region CAN ID : 88 16 91 30 28
                                    if (sRegister.Contains("88 16 91 30 28"))
                                    {
                                        Debug.Write(Scki + "原始數據存在88 16 91 30 28" + "\n");
                                        lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "原始數據存在88 16 91 30 28" + "\n");
                                        if (!bSuccessful[3])
                                        {
                                            Debug.Write(Scki + "開始分析:88 16 91 30 28" + "\n");
                                            lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "開始分析:88 16 91 30 28" + "\n");
                                            index = sRegister.IndexOf("88 16 91 30 28");
                                            if (sRegister.Length - index - 38 >= 0)
                                            {
                                                sData = sRegister.Substring(index, 38);
                                                Debug.Write(Scki + "原始內容:" + sData + "\n");
                                                lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "原始內容:" + sData + "\n");
                                                objDeviceGroup.dicGroup[Station]["DATA_CONTENT"] += sData + " ";
                                                String[] sArray = sData.Split(' ');
                                                Double dACvoltageout = Convert.ToInt32((sArray[6] + sArray[5]), 16) / (double)10;
                                                objDeviceGroup.dicGroup[Station]["OUTPUT_VOLT"] = dACvoltageout.ToString("0.00");
                                                Debug.Write(Scki + "OUTPUT_VOLT:" + dACvoltageout.ToString("0.00") + "\n");
                                                lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "OUTPUT_VOLT:" + dACvoltageout.ToString("0.00") + "\n");
                                                Double dalternatcurrentout = Convert.ToInt32((sArray[8] + sArray[7]), 16) / (double)10;
                                                objDeviceGroup.dicGroup[Station]["OUTPUT_CURR"] = dalternatcurrentout.ToString("0.00");
                                                Debug.Write(Scki + "OUTPUT_CURR:" + dalternatcurrentout.ToString("0.00") + "\n");
                                                lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "OUTPUT_CURR:" + dalternatcurrentout.ToString("0.00") + "\n");
                                                Double dloadpercent = Convert.ToInt32((sArray[10] + sArray[9]), 16) / (double)10;
                                                objDeviceGroup.dicGroup[Station]["LOAD_RATIO"] = dloadpercent.ToString("0.00");
                                                Debug.Write(Scki + "LOAD_RATIO:" + dloadpercent.ToString("0.00") + "\n");
                                                lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "LOAD_RATIO:" + dloadpercent.ToString("0.00") + "\n");
                                                if (dloadpercent < 100)
                                                    objDeviceGroup.dicGroup[Station]["ALERT_INFO11"] = "0";
                                                else
                                                    objDeviceGroup.dicGroup[Station]["ALERT_INFO11"] = "1";

                                                Double dinternaltemp = Convert.ToInt32((sArray[12] + sArray[11]), 16) / (double)10;
                                                objDeviceGroup.dicGroup[Station]["INNER_TEMP"] = dinternaltemp.ToString("0.00");
                                                Debug.Write(Scki + "INNER_TEMP:" + dinternaltemp.ToString("0.00") + "\n");
                                                lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "INNER_TEMP:" + dinternaltemp.ToString("0.00") + "\n");
                                                sRegister = sRegister.Remove(index, 38);
                                                Debug.Write(Scki + "結束分析:88 16 91 30 28" + "\n");
                                                lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "結束分析:88 16 91 30 28" + "\n");
                                                bSuccessful[3] = true;
                                            }
                                        }
                                        else
                                        {
                                            index = sRegister.IndexOf("88 16 91 30 28");
                                            sRegister.Remove(index, 14);
                                            Debug.Write(Scki + "已分析:88 16 91 30 28" + "\n");
                                            lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "已分析:88 16 91 30 28" + "\n");
                                        }
                                    }
                                    #endregion
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("設備ID:" + Station + "錯誤訊息:" + ex.Message + "\r\n" + sData);
                                }
                                try
                                {
                                    #region CAN ID : 88 16 92 30 28
                                    if (sRegister.Contains("88 16 92 30 28"))
                                    {
                                        Debug.Write(Scki + "原始數據存在88 16 92 30 28" + "\n");
                                        lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "原始數據存在88 16 92 30 28" + "\n");
                                        if (!bSuccessful[4])
                                        {
                                            Debug.Write(Scki + "開始分析:88 16 92 30 28" + "\n");
                                            lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "開始分析:88 16 92 30 28" + "\n");
                                            index = sRegister.IndexOf("88 16 92 30 28");
                                            if (sRegister.Length - index - 38 >= 0)
                                            {
                                                //String sACDC = String.Empty;
                                                //String sONOFF = String.Empty;
                                                sData = sRegister.Substring(index, 38);
                                                Debug.Write(Scki + "原始內容:" + sData + "\n");
                                                lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "原始內容:" + sData + "\n");
                                                objDeviceGroup.dicGroup[Station]["DATA_CONTENT"] += sData + " ";
                                                String[] sArray = sData.Split(' ');
                                                Double dambienttemp = Convert.ToInt32((sArray[6] + sArray[5]), 16) / (double)10;
                                                objDeviceGroup.dicGroup[Station]["OUTER_TEMP"] = dambienttemp.ToString("0.00");
                                                Debug.Write(Scki + "OUTER_TEMP:" + dambienttemp.ToString("0.00") + "\n");
                                                lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "OUTER_TEMP:" + dambienttemp.ToString("0.00") + "\n");
                                                Double doutputpower = Convert.ToInt32((sArray[8] + sArray[7]), 16);
                                                objDeviceGroup.dicGroup[Station]["OUTPUT_POWER"] = doutputpower.ToString("0.00");
                                                Debug.Write(Scki + "OUTPUT_POWER:" + doutputpower.ToString("0.00") + "\n");
                                                lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "OUTPUT_POWER:" + doutputpower.ToString("0.00") + "\n");
                                                int dwarninginformation = Convert.ToInt32((sArray[10] + sArray[9]), 16);
                                                objDeviceGroup.dicGroup[Station]["RESERVE03"] = dwarninginformation.ToString();
                                                Debug.Write(Scki + "RESERVE03:" + dwarninginformation.ToString() + "\n");
                                                lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "RESERVE03:" + dwarninginformation.ToString() + "\n");
                                                String swarnmsg = Convert.ToString(dwarninginformation, 2).PadLeft(16, '0');
                                                objDeviceGroup.dicGroup[Station]["SYSTEM_STATE"] = swarnmsg;
                                                Debug.Write(Scki + "SYSTEM_STATE:" + swarnmsg + "\n");
                                                lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "SYSTEM_STATE:" + swarnmsg + "\n");
                                                objDeviceGroup.dicGroup[Station]["SYSTEM_STATE01"] = swarnmsg[0].ToString();
                                                objDeviceGroup.dicGroup[Station]["ALERT_INFO01"] = swarnmsg[0].ToString();
                                                objDeviceGroup.dicGroup[Station]["SYSTEM_STATE02"] = swarnmsg[1].ToString();
                                                objDeviceGroup.dicGroup[Station]["SYSTEM_STATE03"] = swarnmsg[2].ToString();
                                                if (swarnmsg[15].ToString() == "0")
                                                {
                                                    objDeviceGroup.dicGroup[Station]["ALERT_INFO02"] = "0";
                                                    objDeviceGroup.dicGroup[Station]["ALERT_INFO03"] = "1";
                                                }
                                                else
                                                {
                                                    objDeviceGroup.dicGroup[Station]["ALERT_INFO02"] = "0";
                                                    objDeviceGroup.dicGroup[Station]["ALERT_INFO03"] = "0";
                                                }
                                                objDeviceGroup.dicGroup[Station]["SYSTEM_STATE04"] = swarnmsg[3].ToString();
                                                objDeviceGroup.dicGroup[Station]["ALERT_INFO04"] = swarnmsg[3].ToString();
                                                objDeviceGroup.dicGroup[Station]["SYSTEM_STATE05"] = swarnmsg[4].ToString();
                                                objDeviceGroup.dicGroup[Station]["SYSTEM_STATE06"] = swarnmsg[5].ToString();
                                                objDeviceGroup.dicGroup[Station]["SYSTEM_STATE07"] = swarnmsg[6].ToString();
                                                objDeviceGroup.dicGroup[Station]["SYSTEM_STATE08"] = swarnmsg[7].ToString();
                                                objDeviceGroup.dicGroup[Station]["SYSTEM_STATE09"] = swarnmsg[8].ToString();
                                                objDeviceGroup.dicGroup[Station]["SYSTEM_STATE10"] = swarnmsg[9].ToString();
                                                objDeviceGroup.dicGroup[Station]["ALERT_INFO10"] = swarnmsg[9].ToString();
                                                objDeviceGroup.dicGroup[Station]["SYSTEM_STATE11"] = swarnmsg[10].ToString();
                                                objDeviceGroup.dicGroup[Station]["SYSTEM_STATE12"] = swarnmsg[11].ToString();
                                                objDeviceGroup.dicGroup[Station]["SYSTEM_STATE13"] = swarnmsg[12].ToString();
                                                objDeviceGroup.dicGroup[Station]["ALERT_INFO13"] = swarnmsg[12].ToString();
                                                objDeviceGroup.dicGroup[Station]["SYSTEM_STATE14"] = swarnmsg[13].ToString();
                                                objDeviceGroup.dicGroup[Station]["ALERT_INFO14"] = swarnmsg[13].ToString();
                                                objDeviceGroup.dicGroup[Station]["SYSTEM_STATE15"] = swarnmsg[14].ToString();
                                                objDeviceGroup.dicGroup[Station]["ALERT_INFO15"] = swarnmsg[14].ToString();
                                                objDeviceGroup.dicGroup[Station]["SYSTEM_STATE16"] = swarnmsg[15].ToString();
                                                if (swarnmsg[15].ToString() == "1")
                                                    objDeviceGroup.dicGroup[Station]["ALERT_INFO16"] = "0";
                                                else
                                                    objDeviceGroup.dicGroup[Station]["ALERT_INFO16"] = "1";

                                                //if (sArray[11].ToString() == "AC")
                                                //    sACDC = "AC";
                                                //if (sArray[12].ToString() == "00")
                                                //    sONOFF = "ON";
                                                //else
                                                //    sONOFF = "OFF";
                                                sRegister = sRegister.Remove(index, 38);
                                                Debug.Write(Scki + "結束分析:88 16 92 30 28" + "\n");
                                                lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "結束分析:88 16 92 30 28" + "\n");
                                                bSuccessful[4] = true;
                                            }
                                        }
                                        else
                                        {
                                            index = sRegister.IndexOf("88 16 92 30 28");
                                            sRegister.Remove(index, 14);
                                            Debug.Write(Scki + "已分析:88 16 92 30 28" + "\n");
                                            lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "已分析:88 16 92 30 28" + "\n");
                                        }
                                    }
                                    #endregion
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("設備ID:" + Station + "錯誤訊息:" + ex.Message + "\r\n" + sData);
                                }
                                try
                                {
                                    #region CAN ID : 88 18 00 28 F4
                                    if (sRegister.Contains("88 18 00 28 F4"))
                                    {
                                        Debug.Write(Scki + "原始數據存在88 18 00 28 F4" + "\n");
                                        lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "原始數據存在88 18 00 28 F4" + "\n");
                                        if (!bSuccessful[5])
                                        {
                                            Debug.Write(Scki + "開始分析:88 18 00 28 F4" + "\n");
                                            lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "開始分析:88 18 00 28 F4" + "\n");
                                            index = sRegister.IndexOf("88 18 00 28 F4");
                                            if (sRegister.Length - index - 38 >= 0)
                                            {
                                                sData = sRegister.Substring(index, 38);
                                                Debug.Write(Scki + "原始內容:" + sData + "\n");
                                                lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "原始內容:" + sData + "\n");
                                                objDeviceGroup.dicGroup[Station]["DATA_CONTENT"] += sData + " ";
                                                String[] sArray = sData.Split(' ');
                                                Double dCell1voltage = Convert.ToInt32((sArray[6] + sArray[5]), 16) / (double)1000;
                                                objDeviceGroup.dicGroup[Station]["CELL_VOLT01"] = dCell1voltage.ToString("0.000");
                                                Debug.Write(Scki + "CELL_VOLT01:" + dCell1voltage.ToString("0.000") + "\n");
                                                lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "CELL_VOLT01:" + dCell1voltage.ToString("0.000") + "\n");
                                                Double dCell2voltage = Convert.ToInt32((sArray[8] + sArray[7]), 16) / (double)1000;
                                                objDeviceGroup.dicGroup[Station]["CELL_VOLT02"] = dCell2voltage.ToString("0.000");
                                                Debug.Write(Scki + "CELL_VOLT02:" + dCell2voltage.ToString("0.000") + "\n");
                                                lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "CELL_VOLT02:" + dCell2voltage.ToString("0.000") + "\n");
                                                Double dCell3voltage = Convert.ToInt32((sArray[10] + sArray[9]), 16) / (double)1000;
                                                objDeviceGroup.dicGroup[Station]["CELL_VOLT03"] = dCell3voltage.ToString("0.000");
                                                Debug.Write(Scki + "CELL_VOLT03:" + dCell3voltage.ToString("0.000") + "\n");
                                                lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "CELL_VOLT03:" + dCell3voltage.ToString("0.000") + "\n");
                                                Double dCell4voltage = Convert.ToInt32((sArray[12] + sArray[11]), 16) / (double)1000;
                                                objDeviceGroup.dicGroup[Station]["CELL_VOLT04"] = dCell4voltage.ToString("0.000");
                                                Debug.Write(Scki + "CELL_VOLT04:" + dCell4voltage.ToString("0.000") + "\n");
                                                lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "CELL_VOLT04:" + dCell4voltage.ToString("0.000") + "\n");
                                                sRegister = sRegister.Remove(index, 38);
                                                Debug.Write(Scki + "結束分析:88 18 00 28 F4" + "\n");
                                                lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "結束分析:88 18 00 28 F4" + "\n");
                                                bSuccessful[5] = true;
                                            }
                                        }
                                        else
                                        {
                                            index = sRegister.IndexOf("88 18 00 28 F4");
                                            sRegister.Remove(index, 14);
                                            Debug.Write(Scki + "已分析:88 18 00 28 F4" + "\n");
                                            lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "已分析:88 18 00 28 F4" + "\n");
                                        }
                                    }
                                    #endregion
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("設備ID:" + Station + "錯誤訊息:" + ex.Message + "\r\n" + sData);
                                }
                                try
                                {
                                    #region CAN ID : 88 18 01 28 F4
                                    if (sRegister.Contains("88 18 01 28 F4"))
                                    {
                                        Debug.Write(Scki + "原始數據存在88 18 01 28 F4" + "\n");
                                        lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "原始數據存在88 18 01 28 F4" + "\n");
                                        if (!bSuccessful[6])
                                        {
                                            Debug.Write(Scki + "開始分析:88 18 01 28 F4" + "\n");
                                            lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "開始分析:88 18 01 28 F4" + "\n");
                                            index = sRegister.IndexOf("88 18 01 28 F4");
                                            if (sRegister.Length - index - 38 >= 0)
                                            {
                                                sData = sRegister.Substring(index, 38);
                                                Debug.Write(Scki + "原始內容:" + sData + "\n");
                                                lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "原始內容:" + sData + "\n");
                                                objDeviceGroup.dicGroup[Station]["DATA_CONTENT"] += sData + " ";
                                                String[] sArray = sData.Split(' ');
                                                Double dCell5voltage = Convert.ToInt32((sArray[6] + sArray[5]), 16) / (double)1000;
                                                objDeviceGroup.dicGroup[Station]["CELL_VOLT05"] = dCell5voltage.ToString("0.000");
                                                Debug.Write(Scki + "CELL_VOLT05:" + dCell5voltage.ToString("0.000") + "\n");
                                                lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "CELL_VOLT05:" + dCell5voltage.ToString("0.000") + "\n");
                                                Double dCell6voltage = Convert.ToInt32((sArray[8] + sArray[7]), 16) / (double)1000;
                                                objDeviceGroup.dicGroup[Station]["CELL_VOLT06"] = dCell6voltage.ToString("0.000");
                                                Debug.Write(Scki + "CELL_VOLT06:" + dCell6voltage.ToString("0.000") + "\n");
                                                lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "CELL_VOLT06:" + dCell6voltage.ToString("0.000") + "\n");
                                                Double dCell7voltage = Convert.ToInt32((sArray[10] + sArray[9]), 16) / (double)1000;
                                                objDeviceGroup.dicGroup[Station]["CELL_VOLT07"] = dCell7voltage.ToString("0.000");
                                                Debug.Write(Scki + "CELL_VOLT07:" + dCell7voltage.ToString("0.000") + "\n");
                                                lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "CELL_VOLT07:" + dCell7voltage.ToString("0.000") + "\n");
                                                Double dCell8voltage = Convert.ToInt32((sArray[12] + sArray[11]), 16) / (double)1000;
                                                objDeviceGroup.dicGroup[Station]["CELL_VOLT08"] = dCell8voltage.ToString("0.000");
                                                Debug.Write(Scki + "CELL_VOLT08:" + dCell8voltage.ToString("0.000") + "\n");
                                                lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "CELL_VOLT08:" + dCell8voltage.ToString("0.000") + "\n");
                                                sRegister = sRegister.Remove(index, 38);
                                                Debug.Write(Scki + "結束分析:88 18 01 28 F4" + "\n");
                                                lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "結束分析:88 18 01 28 F4" + "\n");
                                                bSuccessful[6] = true;
                                            }
                                        }
                                        else
                                        {
                                            index = sRegister.IndexOf("88 18 01 28 F4");
                                            sRegister.Remove(index, 14);
                                            Debug.Write(Scki + "已分析:88 18 01 28 F4" + "\n");
                                            lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "已分析:88 18 01 28 F4" + "\n");
                                        }
                                    }
                                    #endregion
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("設備ID:" + Station + "錯誤訊息:" + ex.Message + "\r\n" + sData);
                                }
                                try
                                {
                                    #region CAN ID : 86 18 02 28 F4
                                    if (sRegister.Contains("86 18 02 28 F4"))
                                    {
                                        Debug.Write(Scki + "原始數據存在86 18 02 28 F4" + "\n");
                                        lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "原始數據存在86 18 02 28 F4" + "\n");
                                        if (!bSuccessful[7])
                                        {
                                            Debug.Write(Scki + "開始分析:86 18 02 28 F4" + "\n");
                                            lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "開始分析:86 18 02 28 F4" + "\n");
                                            index = sRegister.IndexOf("86 18 02 28 F4");
                                            if (sRegister.Length - index - 32 >= 0)
                                            {
                                                sData = sRegister.Substring(index, 32);
                                                Debug.Write(Scki + "原始內容:" + sData + "\n");
                                                lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "原始內容:" + sData + "\n");
                                                objDeviceGroup.dicGroup[Station]["DATA_CONTENT"] += sData + " ";
                                                String[] sArray = sData.Split(' ');
                                                Double dCell9voltage = Convert.ToInt32((sArray[6] + sArray[5]), 16) / (double)1000;
                                                objDeviceGroup.dicGroup[Station]["CELL_VOLT09"] = dCell9voltage.ToString("0.000");
                                                Debug.Write(Scki + "CELL_VOLT09:" + dCell9voltage.ToString("0.000") + "\n");
                                                lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "CELL_VOLT09:" + dCell9voltage.ToString("0.000") + "\n");
                                                Double dCell10voltage = Convert.ToInt32((sArray[8] + sArray[7]), 16) / (double)1000;
                                                objDeviceGroup.dicGroup[Station]["CELL_VOLT10"] = dCell10voltage.ToString("0.000");
                                                Debug.Write(Scki + "CELL_VOLT10:" + dCell10voltage.ToString("0.000") + "\n");
                                                lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "CELL_VOLT10:" + dCell10voltage.ToString("0.000") + "\n");
                                                Double dCell11voltage = Convert.ToInt32((sArray[10] + sArray[9]), 16) / (double)1000;
                                                objDeviceGroup.dicGroup[Station]["CELL_VOLT11"] = dCell11voltage.ToString("0.000");
                                                Debug.Write(Scki + "CELL_VOLT11:" + dCell11voltage.ToString("0.000") + "\n");
                                                lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "CELL_VOLT11:" + dCell11voltage.ToString("0.000") + "\n");
                                                sRegister = sRegister.Remove(index, 32);
                                                Debug.Write(Scki + "結束分析:86 18 02 28 F4" + "\n");
                                                lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "結束分析:86 18 02 28 F4" + "\n");
                                                bSuccessful[7] = true;
                                            }
                                        }
                                        else
                                        {
                                            index = sRegister.IndexOf("86 18 02 28 F4");
                                            sRegister.Remove(index, 14);
                                            Debug.Write(Scki + "已分析:86 18 02 28 F4" + "\n");
                                            lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "已分析:86 18 02 28 F4" + "\n");
                                        }
                                    }
                                    #endregion
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("設備ID:" + Station + "錯誤訊息:" + ex.Message + "\r\n" + sData);
                                }
                                try
                                {
                                    #region CAN ID : 88 18 A6 28 F4
                                    if (sRegister.Contains("88 18 A6 28 F4"))
                                    {
                                        Debug.Write(Scki + "原始數據存在88 18 A6 28 F4" + "\n");
                                        lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "原始數據存在88 18 A6 28 F4" + "\n");
                                        if (!bSuccessful[8])
                                        {
                                            Debug.Write(Scki + "開始分析:88 18 A6 28 F4" + "\n");
                                            lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "開始分析:88 18 A6 28 F4" + "\n");
                                            index = sRegister.IndexOf("88 18 A6 28 F4");
                                            if (sRegister.Length - index - 38 >= 0)
                                            {
                                                sData = sRegister.Substring(index, 38);
                                                Debug.Write(Scki + "原始內容:" + sData + "\n");
                                                lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "原始內容:" + sData + "\n");
                                                objDeviceGroup.dicGroup[Station]["DATA_CONTENT"] += sData + " ";
                                                String[] sArray = sData.Split(' ');
                                                Double dcell01temp = Convert.ToInt32((sArray[5]), 16) - 40;
                                                objDeviceGroup.dicGroup[Station]["CELL_TEMP01"] = dcell01temp.ToString("0.000");
                                                Debug.Write(Scki + "CELL_TEMP01:" + dcell01temp.ToString("0.000") + "\n");
                                                lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "CELL_TEMP01:" + dcell01temp.ToString("0.000") + "\n");
                                                Double dcell02temp = Convert.ToInt32((sArray[6]), 16) - 40;
                                                objDeviceGroup.dicGroup[Station]["CELL_TEMP02"] = dcell02temp.ToString("0.000");
                                                Debug.Write(Scki + "CELL_TEMP02:" + dcell02temp.ToString("0.000") + "\n");
                                                lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "CELL_TEMP02:" + dcell02temp.ToString("0.000") + "\n");
                                                Double dcell03temp = Convert.ToInt32((sArray[7]), 16) - 40;
                                                objDeviceGroup.dicGroup[Station]["CELL_TEMP03"] = dcell03temp.ToString("0.000");
                                                Debug.Write(Scki + "CELL_TEMP03:" + dcell03temp.ToString("0.000") + "\n");
                                                lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "CELL_TEMP03:" + dcell03temp.ToString("0.000") + "\n");
                                                sRegister = sRegister.Remove(index, 38);
                                                Debug.Write(Scki + "結束分析:88 18 A6 28 F4" + "\n");
                                                lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "結束分析:88 18 A6 28 F4" + "\n");
                                                bSuccessful[8] = true;
                                            }
                                        }
                                        else
                                        {
                                            index = sRegister.IndexOf("88 18 A6 28 F4");
                                            sRegister.Remove(index, 14);
                                            Debug.Write(Scki + "已分析:88 18 A6 28 F4" + "\n");
                                            lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "已分析:88 18 A6 28 F4" + "\n");
                                        }
                                    }
                                    #endregion
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("設備ID:" + Station + " 錯誤訊息:" + ex.Message + "\r\n" + sData);
                                }

                                #region 需要有數據後才能計算
                                if (!bSuccessful[9])
                                {
                                    Debug.Write(Scki + "開始計算" + "\n");
                                    lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "開始計算" + "\n");
                                    if (bSuccessful[0] && bSuccessful[2] && bSuccessful[4])
                                    {
                                        try
                                        {
                                            if (Double.Parse(objDeviceGroup.dicGroup[Station]["CELL_CURR_SUM"]) < 0 ||
                                                Double.Parse(objDeviceGroup.dicGroup[Station]["AC_VOLT"]) == 0)
                                            {
                                                objDeviceGroup.dicGroup[Station]["RESERVE02"] = "0.00";
                                                Debug.Write(Scki + "RESERVE02:" + "0.00" + "\n");
                                                lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "RESERVE02:" + "0.00" + "\n");
                                            }
                                            else
                                            {
                                                Double dRecharge = Double.Parse(objDeviceGroup.dicGroup[Station]["DC_VOLT"]) *
                                                 Double.Parse(objDeviceGroup.dicGroup[Station]["CELL_CURR_SUM"]) *
                                                 1.35;
                                                dRecharge = dRecharge / Double.Parse(objDeviceGroup.dicGroup[Station]["AC_VOLT"]);
                                                objDeviceGroup.dicGroup[Station]["RESERVE02"] =
                                                      (dRecharge +
                                                      Double.Parse(objDeviceGroup.dicGroup[Station]["OUTPUT_POWER"]) /
                                                      Double.Parse(objDeviceGroup.dicGroup[Station]["AC_VOLT"]) *
                                                      1.03).ToString("0.00");
                                                Debug.Write(Scki + "RESERVE02:" + (dRecharge +
                                                      Double.Parse(objDeviceGroup.dicGroup[Station]["OUTPUT_POWER"]) /
                                                      Double.Parse(objDeviceGroup.dicGroup[Station]["AC_VOLT"]) *
                                                      1.03).ToString("0.00") + "\n");
                                                lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "RESERVE02:" + (dRecharge +
                                                      Double.Parse(objDeviceGroup.dicGroup[Station]["OUTPUT_POWER"]) /
                                                      Double.Parse(objDeviceGroup.dicGroup[Station]["AC_VOLT"]) *
                                                      1.03).ToString("0.00") + "\n");
                                            }
                                        }
                                        catch (Exception ex1)
                                        {
                                            MessageBox.Show("RESERVE02運算異常:" +
                                                " CELL_CURR_SUM:" + objDeviceGroup.dicGroup[Station]["CELL_CURR_SUM"] +
                                                " AC_VOLT:" + objDeviceGroup.dicGroup[Station]["AC_VOLT"] +
                                                " DC_VOLT:" + objDeviceGroup.dicGroup[Station]["DC_VOLT"] +
                                                " OUTPUT_POWER:" + objDeviceGroup.dicGroup[Station]["OUTPUT_POWER"]);
                                        }
                                        try
                                        {
                                            objDeviceGroup.dicGroup[Station]["AC_POWER"] =
                                                (Double.Parse(objDeviceGroup.dicGroup[Station]["RESERVE02"]) *
                                                Double.Parse(objDeviceGroup.dicGroup[Station]["AC_VOLT"])).ToString("0.0");
                                            Debug.Write(Scki + "AC_POWER:" + (Double.Parse(objDeviceGroup.dicGroup[Station]["RESERVE02"]) *
                                                Double.Parse(objDeviceGroup.dicGroup[Station]["AC_VOLT"])).ToString("0.0") + "\n");
                                            lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "AC_POWER:" + (Double.Parse(objDeviceGroup.dicGroup[Station]["RESERVE02"]) *
                                                Double.Parse(objDeviceGroup.dicGroup[Station]["AC_VOLT"])).ToString("0.0") + "\n");

                                            if (Double.Parse(objDeviceGroup.dicGroup[Station]["AC_POWER"]) < 0)
                                            {
                                                objDeviceGroup.dicGroup[Station]["AC_POWER"] = "0";
                                                Debug.Write(Scki + "AC_POWER:" + "0" + "\n");
                                                lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "AC_POWER:" + "0" + "\n");
                                            }
                                        }catch(Exception ex2)
                                        {
                                            MessageBox.Show("AC_POWER運算異常:" +
                                                " RESERVE02:" + objDeviceGroup.dicGroup[Station]["RESERVE02"] +
                                                " AC_VOLT:" + objDeviceGroup.dicGroup[Station]["AC_VOLT"] +
                                                " AC_POWER:" + objDeviceGroup.dicGroup[Station]["AC_POWER"]);
                                        }
                                        objDeviceGroup.dicGroup[Station]["AC_CURR"] = objDeviceGroup.dicGroup[Station]["RESERVE02"];
                                        Debug.Write(Scki + "AC_CURR:" + objDeviceGroup.dicGroup[Station]["RESERVE02"] + "\n");
                                        lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "AC_CURR:" + objDeviceGroup.dicGroup[Station]["RESERVE02"] + "\n");

                                        if (Double.Parse(objDeviceGroup.dicGroup[Station]["DC_VOLT"]) < 21)
                                        {
                                            objDeviceGroup.dicGroup[Station]["ALERT_INFO05"] = "1";
                                            Debug.Write(Scki + "ALERT_INFO05:" + "1" + "\n");
                                            lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "ALERT_INFO05:" + "1" + "\n");
                                        }
                                        else
                                        {
                                            objDeviceGroup.dicGroup[Station]["ALERT_INFO05"] = "0";
                                            Debug.Write(Scki + "ALERT_INFO05:" + "0" + "\n");
                                            lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "ALERT_INFO05:" + "0" + "\n");
                                        }
                                        Debug.Write(Scki + "結束計算" + "\n");
                                        lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "結束計算" + "\n");
                                        bSuccessful[9] = true;
                                    }
                                }
                                #endregion

                                Int32 iCheckAllTrue = Array.IndexOf(bSuccessful, false);
                                Debug.Write(Scki + "分析進度還差:" + iCheckAllTrue + "\n");
                                lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + " 分析進度還差:" + iCheckAllTrue + "\n");
                                if (iCheckAllTrue == -1)
                                {
                                    Debug.Write(Scki + "所有進度完成" + "\n");
                                    lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + " 所有進度完成" + "\n");
                                    objDeviceGroup.dicGroup[Station]["DEVICE_ID"] = Station;
                                    objmodprocess._Save2BatteryData(objDeviceGroup.dicGroup[Station]);
                                    lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + " 字典內容: " + objDeviceGroup.dicGroup[Station]["DEVICE_ID"] + " " + objDeviceGroup.dicGroup[Station]["SYSTEM_STATE"] + "\n");
                                    lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + " 字典數量:" + objDeviceGroup.dicGroup.Count.ToString() + "\n");
                                    sTime = DateTime.Now;
                                    if (objDeviceGroup.dicUpload.ContainsKey(Station))
                                    {
                                        objDeviceGroup.dicUpload[Station].Insert(0, sTime.ToString("yyMMddHH:mm:ss"));
                                        if (objDeviceGroup.dicUpload[Station].Count > 3)
                                            objDeviceGroup.dicUpload[Station].RemoveAt(3);
                                        Debug.Write(Scki + "畫面更新:" + Station + "\n");
                                        lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + " 畫面更新:" + Station + "\n");
                                    }
                                    else
                                    {
                                        List<String> ls = new List<String>();
                                        ls.Insert(0, sTime.ToString("yyMMddHH:mm:ss"));
                                        objDeviceGroup.dicUpload.Add(Station, ls);
                                    }
                                    objDeviceGroup.dicGroup[Station]["DATA_CONTENT"] = "";
                                    objDeviceGroup.dicGroup[Station].Clear();
                                    objDeviceGroup.dicGroup[Station] = objDeviceGroup.InitialData();
                                    Debug.Write(Scki + "清空暫存:" + Station + "\n");
                                    //lsLog.Add(System.DateTime.Now.ToString("yyMMdd HH:mm:ss")+":" +Station + "清空暫存:" + Station + "\n");
                                    lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + " 確認是否清空:" + objDeviceGroup.dicGroup[Station]["DEVICE_ID"] + "\n");
                                    Array.Clear(bSuccessful, 0, bSuccessful.Length);
                                    sRegister = "";
                                    bNext = true;
                                }
                            }
                        }
                    }
                    Thread.Sleep(30);
                }
                catch (Exception ex)
                {
                    lsLog.Add("執行緒:" + System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + " 設備ID:" + Station + "錯誤訊息:" + ex.ToString() + "\n");
                    //MessageBox.Show(System.DateTime.Now.ToString("yyMMdd HH:mm:ss") + ":" + Station + "錯誤訊息:" + ex.ToString() + "\n");
                    // 這裡若出錯主要是來自 SckSs[Scki] 出問題, 可能是自己 Close, 也可能是 Client 斷線, 自己加判斷吧~
                }
            }
        }

        #region 資料:. 整併資料收集的數據
        /// <summary>
        /// 整併資料收集的數據
        /// </summary>
        /// <param name="SourceData">前一次處理後剩下的數據</param>
        /// <param name="ReceiveData">當下接收的數據</param>
        /// <param name="iLen">當下接受的數據數量</param>
        /// <returns>整併後的資料</returns>
        private Byte[] Processing_Data_Incomplete(Byte[] SourceData, Byte[] ReceiveData, Int32 iLen)
        {
            byte[] bRegister;
            if (SourceData != null)
            {
                //宣告一個暫存byte陣列
                bRegister = new byte[SourceData.Length + iLen];
                //複製原本的內容到暫存
                Array.Copy(SourceData, 0, bRegister, 0, SourceData.Length);
                //複製收集的內容到暫存
                Array.Copy(ReceiveData, 0, bRegister, SourceData.Length, iLen);
            }
            else
            {
                bRegister = new byte[iLen];
                //複製到暫存陣列
                Array.Copy(bRegister, ReceiveData, iLen);
            }
            return bRegister;
        }
        #endregion

        #region 中斷所有連線
        public void Client_Disconnect()
        {
            //CloseReceive = false;
            if (_SocketListener != null)
            {
                foreach (Socket socket in _SocketListener)
                {
                    if (socket != null)
                    {
                        dicCloseReceive[socket] = false;
                        //if (socket.Connected == true)
                        //{
                            socket.Close();
                        //}
                        //else
                        //{
                        //    socket.Dispose();
                        //}
                        
                    }
                }
            }
            //objDeviceGroup.dicDeviceData.Clear();
            //objDeviceGroup.dicGroup.Clear();
            //objDeviceGroup.dicUpload.Clear();
            //objDeviceGroup = null;
            //dicCloseReceive.Clear();
            //dicSockClinet.Clear();
        }
        #endregion

        #region Hex換算溫度
        public String HextoTemp(String sData)
        {
            Double Temp = Double.Parse(sData);
            if (Temp > 4)
            {
                Temp = (Temp - 4) * 100 * 0.125;
                return Math.Round(Temp, 1).ToString();
            }
            else
                return "0";
        }
        #endregion

    }
}
