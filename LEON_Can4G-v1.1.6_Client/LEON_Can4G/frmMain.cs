using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using LEON_Can4G.Module;
using System.Net;

namespace LEON_Can4G
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }
        modSocket objSocket;
        CanSocket objCanServer;
        Dictionary<String, List<String>> dicUploadTime =
            new Dictionary<String, List<String>>();
        Thread thUpload;
        Boolean bEndUpload = false;
        Thread thAlive;
        Boolean bEndAlive = false;
        
        private void BarttytoSQL()
        {
            while (bEndUpload)
            {
                if (objCanServer != null)
                {
                    if (objCanServer.lsLog.Count > 0)
                    {
                        if(ConfigurationManager.AppSettings["Log"]=="1")
                            WriteErrorLog(objCanServer.lsLog[0]);
                        objCanServer.lsLog.RemoveAt(0);
                    }
                    else
                    {
                        Thread.Sleep(10);
                    }
                }
            }
        }
        
        private void frmMain_Load(object sender, EventArgs e)
        {
            txtIPAddress.Text = ConfigurationManager.AppSettings["ServerIPAddress"].ToString();
            txtPort.Text = ConfigurationManager.AppSettings["ServerPort"].ToString();
        }

        #region Delegate 委派:控制項數值更新
        delegate void UPdateCallBack(Control ctr, Int32 index, String Data);
        public void UpdateUI(Control ctr, Int32 index, String Data)
        {
            if (ctr.InvokeRequired)
            {
                if (ctr == null || Data == null) return;
                UPdateCallBack updatecallback = new UPdateCallBack(UpdateUI);
                ctr.Invoke(updatecallback, ctr, index, Data);
            }
            else
            {
                ctr.Text = Data;
            }
        }

        #endregion

        private void btnListen_Click(object sender, EventArgs e)
        {
            if (btnListen.Text == "監聽")
            {
                Debug.WriteLineIf(objCanServer == null, "建立新的Socket Class");
                Debug.Assert(objCanServer == null, "原本的Socket Class已存在");
                if (objCanServer == null)
                {
                    objCanServer = new CanSocket(this);
                }
                else
                {
                    objCanServer = null;
                    objCanServer = new CanSocket(this);
                }
                objCanServer.Listen(txtIPAddress.Text, txtPort.Text);
                Debug.WriteLine("開始Listen:"+ txtIPAddress.Text+":"+txtPort.Text);
                bEndUpload = true;
                thUpload = new Thread(BarttytoSQL);
                thUpload.Start();
                tmrShow.Enabled = true;
                //tmrUpdate.Enabled = true;
                Debug.WriteLine("開始每15秒繪製畫面一次");
                btnListen.Text = "關閉";
            }
            else
            {
                Debug.WriteLineIf(objCanServer != null, "中斷Server對所有的連線");
                Debug.Assert(objCanServer != null, "存在Socket Class，但無監聽");
                if (objCanServer != null)
                {
                    objCanServer.Client_Disconnect();
                    objCanServer = null;
                }
                bEndUpload = false;
                thUpload.Abort();
                tmrShow.Enabled = false;
                btnListen.Text = "監聽";
            }
        }

        private void tmrShow_Tick(object sender, EventArgs e)
        {
            Int32 i = 1;
            if (objCanServer != null)
            {
                if (objCanServer.objDeviceGroup.dicUpload.Count > 0)
                {
                    String str = "Count                Device ID                UpLOADTime1                UpLOADTime2                UpLOADTime3                " + "\r\n";
                    for(int x = 0;x< objCanServer.objDeviceGroup.dicUpload.Count;x++)
                    {
                        KeyValuePair<string, List<string>> kv = objCanServer.objDeviceGroup.dicUpload.ElementAt(x);
                        if (i >= 10)
                        {
                            str += i + "                  " + kv.Key.ToString() + "            ";
                        }
                        else
                        {
                            str += i + "                    " + kv.Key.ToString() + "            ";
                        }
                        for(int y = 0; y<kv.Value.Count;y++)
                        {
                            str += kv.Value[y].ToString() + "             ";
                        }
                        str += "\r\n";
                        i++;
                    }
                    //foreach (KeyValuePair<String, List<String>> item in objCanServer.objDeviceGroup.dicUpload)
                    //{
                    //    if (i >= 10)
                    //    {
                    //        str += i + "                  " + item.Key.ToString() + "            ";
                    //    }
                    //    else
                    //    {
                    //        str += i + "                    " + item.Key.ToString() + "            ";
                    //    }
                    //    foreach (String stime in item.Value)
                    //    {
                    //        str += stime.ToString() + "             ";
                    //    }
                    //    str += "\r\n";
                    //    i++;
                    //}
                    UpdateUI(richTextBox1, 1, str);
                }
            }
        }

        internal void Socket_ReceiveData(IPEndPoint ipendpoint, string szData)
        {
            throw new NotImplementedException();
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            bEndUpload = false;
            tmrShow.Enabled = false;
            bEndAlive = true;
            if (objCanServer!=null)objCanServer.Client_Disconnect();
            try
            {
                thUpload.Abort();
            }
            catch { }
            try
            {
                thAlive.Abort();
            }
            catch { }
        }

        private void tmrUpdate_Tick(object sender, EventArgs e)
        {
            //try
            //{
            //    while (objCanServer.lsLog.Count > 0)
            //    {
            //        WriteErrorLog(objCanServer.lsLog[0]);
            //        objCanServer.lsLog.RemoveAt(0);
            //    }
            //}
            //catch { }
        }

        #region ERRORLog
        private void WriteErrorLog(string logDesc)
        {
            try
            {
                StreamWriter writer;
                string path = AppDomain.CurrentDomain.BaseDirectory + "SystemLog" + "\\\\" + DateTime.Now.ToString("yyyyMMdd");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                if (logDesc.Split(':')[0] == "執行緒")
                {
                    int IDindex = logDesc.IndexOf("設備ID:");
                    string logStationName = path + @"\" + logDesc.Substring(IDindex+5, 10) + ".txt";
                    if (!File.Exists(logStationName))
                    {
                        writer = File.CreateText(logStationName);
                    }
                    else
                    {
                        writer = File.AppendText(logStationName);
                    }
                    writer.WriteLine(logDesc);
                    writer.Close();
                }
                else if (logDesc.Split(':')[0] == "系統")
                {
                    string logFileName = path + @"\" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                    if (!File.Exists(logFileName))
                    {
                        writer = File.CreateText(logFileName);
                    }
                    else
                    {
                        writer = File.AppendText(logFileName);
                    }
                    writer.WriteLine(logDesc);
                    writer.Close();
                }
            }
            catch { }
        }
        #endregion

        private void btnAlive_Click(object sender, EventArgs e)
        {
            string IPAddress = ConfigurationManager.AppSettings["ClientIP"].ToString();
            Int32 Port = int.Parse(ConfigurationManager.AppSettings["ClientPort"].ToString());
            if (btnAlive.Text == "心跳啟動")
            {
                bEndAlive = false;
                thAlive = new Thread(Client_Alive);
                thAlive.Start();
                btnAlive.Text = "心跳關閉";
            }
            else if (btnAlive.Text == "心跳關閉")
            {
                bEndAlive = true;
                try
                {
                    thAlive.Abort();
                }
                catch { }
                btnAlive.Text = "心跳啟動";
            }
        }

        private void Client_Alive()
        {
            Int32 iTime = int.Parse(ConfigurationManager.AppSettings["Time"].ToString());
            string Msg = ConfigurationManager.AppSettings["Message"].ToString();
            string IPAddress = ConfigurationManager.AppSettings["ClientIP"].ToString();
            Int32 Port = int.Parse(ConfigurationManager.AppSettings["ClientPort"].ToString());
            try
            {
                while (!bEndAlive)
                {
                    if (objSocket != null)
                        objSocket.Client_Disconnect();
                    objSocket = new modSocket(IPAddress, Port, this, rtbMsg);
                    if (objSocket.Connect())
                    {
                        if (objSocket.Client_Connectd())
                        {
                            try
                            {
                                objSocket.Send(Encoding.Default.GetBytes(Msg));
                            }
                            catch (Exception ex)
                            {
                                UpdateUIControl.UpdateUI(rtbMsg, UpdateUIControl.Action_ID.Msg,
                                    System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " 心跳發送失敗，異常原因:" + ex.ToString());
                            }
                        }
                    }
                    objSocket.Client_Disconnect();
                    Thread.Sleep(iTime);
                }
            }
            catch (Exception e1)
            {
                UpdateUIControl.UpdateUI(rtbMsg, UpdateUIControl.Action_ID.Msg,
                    System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " 心跳發送失敗，異常原因:" + e1.ToString());
            }
        }

    }
}
