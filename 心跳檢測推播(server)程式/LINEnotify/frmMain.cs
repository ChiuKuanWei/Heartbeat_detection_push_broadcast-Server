using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Timers;
using System.Collections;
using System.Text.RegularExpressions;
using HslCommunication.LogNet;

namespace LINEnotify
{
    public partial class frmMain : Form
    {
        private ILogNet logNet;
        //Timer            
        System.Timers.Timer timer = new System.Timers.Timer();
        //Server變數
        private Socket socketListen = null;     //用於監聽的socket       
        private bool bListener = false;         //Server是否監聽
        private Socket socketConnect = null;            //兩端連線的socket橋梁
        //Client變數
        private bool bClientListener = false;
        Dictionary<string, Socket> dicClient = new Dictionary<string, Socket>();   //連線的客戶端集合

        #region Appsettings變數
        int iAppsettings = 0;         //Appsettings數量
        string[] arrayAllkey = null;  //取得keys集合
        bool[] arrayBoolean = null;   //keys布林值集合
        int[] iSendTime = null;       //無心跳時，發送的次數
        int iPoint = 0;               //再次檢查時，取得陣列索引位置
        #endregion

        public frmMain()
        {
            InitializeComponent();
            int iTimer = Convert.ToInt32(ConfigurationManager.ConnectionStrings["Timer"].ToString());
            timer.Interval = iTimer;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(Check_Heart);
        }

        #region  顯示接收內容再ListBox
        delegate void SetTextCallback(Control ctrl, string obj);
        void Control_UpdateCallBack(Control ctrl, string obj)
        {
            if (ctrl == null || obj == null) return;//判斷若 參數都是null return
            if (ctrl.InvokeRequired) //判斷是否是自己建立的Thrad若是，就進入if內
            {
                SetTextCallback d = new SetTextCallback(Control_UpdateCallBack);//委派把自己傳進來，所以會在執行這個方法第二次
                //所以第一次近來後，他會再跑一次，這時已經是第2次了，所以會直接掉入else去做Add的動作
                //每次回圈都Call這個方法，然後每次這個方法他會跑兩次，第二次才會去做Add的動作
                ctrl.Invoke(d, ctrl, obj);
            }
            else
            {               
                if(lbMessage.Items.Count == 100)
                {
                    lbMessage.Items.Clear();
                }
                lbMessage.Items.Add(obj);
                lbMessage.TopIndex = lbMessage.Items.Count - 1;
                //string sItem = lbMessage.Items[lbMessage.Items.Count - 1].ToString().Trim();                            
            }
        }
        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {
            logNet = new LogNetSingle(Application.StartupPath + "\\Logs\\log.txt");
            this.Text += FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion.ToString(); //取得版本
            #region 1.  Checked DKDMS.exe Execution
            //取得此process的名稱
            String name = Process.GetCurrentProcess().ProcessName;
            //取得所有與目前process名稱相同的process
            Process[] ps = Process.GetProcessesByName(name);
            //ps.Length > 1 表示此proces已重複執行
            if (ps.Length > 1)
            {
                System.Environment.Exit(System.Environment.ExitCode);
            }
            #endregion
            txtIP.Text = ConfigurationManager.ConnectionStrings["IP"].ToString();
            txtPort.Text = ConfigurationManager.ConnectionStrings["Port"].ToString();
            arrayAllkey = ConfigurationManager.AppSettings.AllKeys;
            iAppsettings = ConfigurationManager.AppSettings.Count;    //Appsettings數量
            if (iAppsettings == 0)
            {
                Control_UpdateCallBack(lbMessage, "AppSettings count is null.");
                btnConnect.Enabled = false;               
            }
            else
            {
                arrayBoolean = new bool[iAppsettings];
                iSendTime = new int[iAppsettings];                
            }
        }

        #region 成員函數，判斷IP地址格式是否正確
        /// <summary>
        /// 判斷輸入的ip地址是否正確，返回TRUE or FALSE
        /// </summary>
        /// <param name="sIP">等待判斷的字符串</param>
        /// <returns>TRUE OR FALSE</returns>
        private bool CheckIPAddress(string sIP)
        {
            bool blnTest = false;
            bool _Result = true;

            Regex regex = new Regex("^[0-9]{1,3}.[0-9]{1,3}.[0-9]{1,3}.[0-9]{1,3}$");
            blnTest = regex.IsMatch(sIP);
            if (blnTest == true)
            {
                string[] strTemp = sIP.Split('.');
                int nDotCount = strTemp.Length - 1;  //字符串中.的數量，若.的數量小於3，則是非法的ip地址
                if (3 == nDotCount) //判斷字符串中.的數量
                {
                    for (int i = 0; i < strTemp.Length; i++)
                    {
                        if (Convert.ToInt32(strTemp[i]) > 255)
                        {
                            // >255则提示，不符合IP格式
                            MessageBox.Show("不符合IP格式");
                            _Result = false;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("不符合IP格式");
                    _Result = false;
                }
            }
            else
            {
                // 輸入非數字則提示，不符合IP格式
                MessageBox.Show("不符合IP格式");
                _Result = false;
            }
            return _Result;
        }
        #endregion

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (btnConnect.Text == "監聽")
            {
                if (txtIP.Text != "")
                {
                    if (!CheckIPAddress(txtIP.Text))
                        return;

                    try
                    {                       
                        StartSocket();  //開始監聽
                        btnConnect.Text = "中斷";
                        Control_UpdateCallBack(lbMessage, "監聽中...");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("開啟監聽失敗,錯誤訊息: " + ex.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    try
                    {                     
                        timer.Start();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Timer啟動失敗,錯誤訊息: " + ex.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("請設定IP", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                timer.Stop();
                timer.Close();
                try
                {
                    if(socketListen != null)
                    {
                        bListener = false;
                        bClientListener = false;
                        if(socketConnect != null)
                        {
                            socketConnect.Close();
                            socketConnect = null;
                        }                       
                        socketListen.Close();
                    }                   
                }
                catch{}
                
                if (timer_Checkagain.Enabled)
                {
                    timer_Checkagain.Enabled = false;
                }
                for (int i = 0; i < iAppsettings; i++)
                {
                    arrayBoolean[i] = false;
                    iSendTime[i] = 0;
                }
                btnConnect.Text = "監聽";
                
            }           
        }

        //開始監聽連線
        private void StartSocket()
        {
            //建立套接字,埠及IP
            IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(txtIP.Text), int.Parse(txtPort.Text));
            socketListen = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //監聽繫結的網路節點
            socketListen.Bind(ipe);
            //將套接字的監聽佇列長度限制為20
            socketListen.Listen(10);
            //負責監聽客戶端的執行緒:建立一個監聽執行緒           
            Thread threadwatch = new Thread(WatchConnecting);
            //將窗體執行緒設定為與後臺同步，隨著主執行緒結束而結束           
            threadwatch.IsBackground = true;
            bListener = true;
            threadwatch.Start();
        }

        //監聽客戶端發來的請求
        private void WatchConnecting()
        {            
            //持續不斷監聽客戶端發來的請求  
            while (bListener)
            {
                try
                {                   
                    socketConnect = socketListen.Accept();
                }
                catch (Exception ex)
                {
                    break;
                }
                //客戶端網路結點號
                string remoteEndPoint = socketConnect.RemoteEndPoint.ToString();              
                //建立一個通訊執行緒  
                Thread thread = new Thread(Receive);
                //設定為後臺執行緒，隨著主執行緒退出而退出
                thread.IsBackground = true;
                bClientListener = true;
                //啟動執行緒  
                thread.Start(socketConnect);
            }
        }

        /// <summary>
        /// Server接收訊息
        /// </summary>       
        private void Receive(object socketclientpara)
        {
            Socket socketServer = socketclientpara as Socket;

            while (bClientListener)
            {
                try
                {
                    byte[] bReceiveData = new byte[socketServer.ReceiveBufferSize];
                    int iRx = 0;
                    //開始非同步接收訊息
                    try
                    {
                        iRx = socketServer.Receive(bReceiveData, 0, bReceiveData.Length, SocketFlags.None);
                    }
                    catch (Exception ex)
                    {
                        //Control_UpdateCallBack(lbMessage, System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 接收異常:" + ex.Message);
                        break;
                    }
                    if (iRx != 0)
                    {
                        if (iRx < bReceiveData.Length)
                        {
                            byte[] tempData = new byte[iRx];
                            for (int i = 0; i < iRx; i++)
                            {
                                tempData[i] = bReceiveData[i];
                            }
                            bReceiveData = tempData;
                        }

                        string sRecive = Encoding.ASCII.GetString(bReceiveData, 0, iRx);
                        Control_UpdateCallBack(lbMessage, DateTime.Now.ToString() + " 接收內容 = " + sRecive);
                        //arrayAllkey = new string[] { "192.168.1.1", "192.168.1.2", "192.168.1.3" };
                        //arrayBoolean = new bool[] { false , false , false };
                        for (int i = 0; i < arrayAllkey.Length; i++)
                        {
                            
                            if (arrayAllkey[i] == sRecive)
                            {
                                arrayBoolean[i] = true;
                            }
                        }
                    }
                    else if (iRx == 0)
                    {
                        socketServer.Close();
                        socketServer = null;
                        //bListener = false;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Control_UpdateCallBack(lbMessage, System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 接收異常:" + ex.Message);
                    socketServer.Close();
                    break;
                }
            }
        }

        private void Check_Heart(object source, ElapsedEventArgs e)
        {
            //arrayAllkey = new string[] { "192.168.1.1", "192.168.1.2", "192.168.1.3" };
            //arrayBoolean = new bool[] { false , false , false };
            for(int i = 0; i < iAppsettings; i++)
            {
                if (arrayBoolean[i] == false && iSendTime[i] < 2)
                {
                    string sToken = ConfigurationManager.AppSettings[arrayAllkey[i]].ToString();
                    string sMessage = "主程式:" + arrayAllkey[i] + "無心跳!";

                    if (SendMessage(sMessage, sToken))
                    {
                        iPoint = i;
                        iSendTime[i] += 1;
                        timer_Checkagain.Enabled = true;
                    }                                        
                }
                else if (arrayBoolean[i] == true && iSendTime[i] != 0)
                {
                    iSendTime[i] = 0;
                    arrayBoolean[i] = false;
                    
                }
                else
                {                    
                    arrayBoolean[i] = false;
                }
            }
        }

        private void timer_Checkagain_Tick(object sender, EventArgs e)
        {           
            if (arrayBoolean[iPoint] == false && iSendTime[iPoint] < 2)
            {                
                string sToken = ConfigurationManager.AppSettings[arrayAllkey[iPoint]].ToString();
                string sMessage = "主程式:" + arrayAllkey[iPoint] + "無心跳!";

                if (SendMessage(sMessage, sToken))
                {
                    iSendTime[iPoint] += 1;                    
                }
            }
            else if (arrayBoolean[iPoint] == true && iSendTime[iPoint] != 0)
            {
                iSendTime[iPoint] = 0;
            }
            timer_Checkagain.Enabled = false;
        }
         

        //LINE推播
        public bool SendMessage(string sMessage, string sToken)
        {
            try
            {
                logNet.WriteNewLine();
                logNet.WriteInfo("一般信息", "進入推播了");
                string url = "https://notify-api.line.me/api/notify";
                //要傳送的文字內容
                string postData = "message=" + WebUtility.HtmlEncode("\r\n" + sMessage);
                //string postData = "imageFile=" + WebUtility.HtmlEncode("\r\n" + message);            
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                Uri target = new Uri(url);
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                //System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                WebRequest request = WebRequest.Create(target);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = byteArray.Length;
                request.Headers.Add("Authorization", "Bearer " + sToken);
                logNet.WriteInfo("一般信息", "推播取得驗證權杖");
                
                using (var dataStream = request.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                }
                logNet.WriteInfo("一般信息", "推播寫入串流");
                var response = (HttpWebResponse)request.GetResponse();
                //取得響應
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();//回傳JSON
                responseString = "[" + responseString + "]";
                logNet.WriteInfo("一般信息", "推播取得響應");
                //取得目前剩餘發送數量
                String str = string.Empty;
                for (int i = 0; i < response.Headers.Keys.Count; i++)
                {
                    str += response.Headers.Keys[i] + ":" + response.Headers.Get(i) + "\n";
                }
                logNet.WriteInfo("一般信息", "推播發送成功");
                return true;
            }
            catch (Exception ex)
            {
                Control_UpdateCallBack(lbMessage, System.DateTime.Now.ToString() + " 推播訊息異常，請檢查權杖碼設定是否正確或沒申請權杖碼給群組:" + ex.Message);
                logNet.WriteError("錯誤" , "推播訊息異常，請檢查權杖碼設定是否正確或沒申請權杖碼給群組:" + ex.Message);
                return false;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (socketListen != null)
                {
                    bListener = false;
                    bClientListener = false;
                    if (socketConnect != null)
                    {
                        socketConnect.Close();
                        socketConnect = null;
                    }
                    socketListen.Close();
                }
            }
            catch { }
            //立刻回收(釋放)程式占用記憶體
            GC.Collect();
        }

        
    }
}
