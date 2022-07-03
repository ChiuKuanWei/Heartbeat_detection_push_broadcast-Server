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
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.IO;

namespace LEON_Can4G
{
    public class modSocket
    {
        public Socket _socketclient;

        private IPAddress _ipaddress;

        private Int32 _port;

        // Size of receive buffer.  
        public const int BufferSize = 1024;

        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];

        //Client Received Status
        private Boolean IsEndReceived = false;

        //Client Connect Status
        private Boolean IsRunning = false;

        private Control _control;

        //Received Thread
        private Thread thclientReceived;

        private frmMain _frmMain;

        #region Func.: 建構子
        public modSocket(String ipaddress,Int32 iport, frmMain f,Control control)
        {
            _frmMain = f;
            _ipaddress = IPAddress.Parse(ipaddress);
            _port = iport;
            _socketclient = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
            _socketclient.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _socketclient.IOControl(IOControlCode.KeepAliveValues, KeepAlive(1, 1000, 100), null);
            _socketclient.ReceiveTimeout = 1000;
            _socketclient.SendTimeout = 1000;
            _control = control;
            IsEndReceived = false;
        }
        #endregion

        #region Func.: 建立連線
        public Boolean Connect()
        {
            try
            {
                _socketclient.Connect(_ipaddress, _port);
                thclientReceived = new Thread(Client_Received);
                thclientReceived.Start();
                //thReConnect = new Thread();
                IsRunning = true;
                return true;
            }
            catch (Exception ex)
            {
                UpdateUIControl.UpdateUI(_control, UpdateUIControl.Action_ID.Text, System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")+" Client 建立連線失敗，失敗原因: " + ex.ToString());
                //MessageBox.Show("Client 建立連線失敗，失敗原因: " + ex.ToString(), "連線失敗");
                IsRunning = false;
                return false;
            }
        }
        #endregion

        #region Func.: 發送訊息
        public void Send(byte[] bData)
        {
            if (_socketclient != null && IsRunning)
            {
                try
                {
                    _socketclient.Send(bData);
                }
                catch (Exception ex) { throw; }
            }
        }
        #endregion

        #region Func.: 連線中斷
        public void Client_Disconnect()
        {
            if (IsRunning)
            {
                IsRunning = false;
                IsEndReceived = true;
                _socketclient.Disconnect(false);
                //_socketclient = null;
                //thclientReceived.Abort();
            }
        }
        #endregion

        #region Func.: 接收資料
        public void Client_Received() //For Client Mode
        {
            try
            {
                IPEndPoint ipendpoint = (IPEndPoint)_socketclient.RemoteEndPoint;
                while (!IsEndReceived)
                {
                    int iRx = _socketclient.Receive(buffer);
                    if (iRx != 0)
                    {
                        if (iRx < buffer.Length)
                        {
                            byte[] tempData = new byte[iRx];
                            for (int i = 0; i < iRx; i++)
                                tempData[i] = buffer[i];
                            buffer = tempData;
                        }

                        char[] chars = new char[iRx];
                        int charLength = Encoding.GetEncoding("ISO-8859-1").GetDecoder().GetChars(buffer, 0, iRx, chars, 0);
                        string szData = new String(chars);
                        _frmMain.Socket_ReceiveData(ipendpoint, szData);

                    }
                }
            }
            catch (Exception ex)
            {
                UpdateUIControl.UpdateUI(_control, UpdateUIControl.Action_ID.Text,
                    System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " Client 訊息接收異常，失敗原因: " + ex.ToString());
                //new Thread(() => { MessageBox.Show(ex.Message, "網路連線中斷", MessageBoxButtons.OK, MessageBoxIcon.Error); }).Start();
            }
            finally
            {
                if (IsRunning)
                {
                    IsRunning = false;
                    IsEndReceived = true;
                    _socketclient.Close();
                    //_tcpClient = null;
                    //_Clients.Remove(acceptedClient);
                }
            }
        }
        #endregion

        #region Func.: 探測封包
        private byte[] KeepAlive(int onOff, int keepAliveTime, int keepAliveInterval)
        {
            byte[] buffer = new byte[12];
            BitConverter.GetBytes(onOff).CopyTo(buffer, 0);
            BitConverter.GetBytes(keepAliveTime).CopyTo(buffer, 4);
            BitConverter.GetBytes(keepAliveInterval).CopyTo(buffer, 8);
            return buffer;
        }
        #endregion

        #region Func.: 檢查連線狀態
        public Boolean Client_Connectd()
        {
            return _socketclient.Connected;
        }
        #endregion
    }
}
