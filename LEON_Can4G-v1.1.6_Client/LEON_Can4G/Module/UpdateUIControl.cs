using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LEON_Can4G
{
    class UpdateUIControl
    {
        public enum Action_ID
        {
            Text = 1,           // Text
            BackColor = 2,      //BackColor
            Enable = 3 ,        //Enable
            Item = 4,           //list Remove Add
            Msg = 5             //Msg
        }

        #region Delegate 委派:控制項更新
        delegate void UPdateCallBack(Control ctr, Action_ID index, String Data);
        public static void UpdateUI(Control ctr, Action_ID index, String Data)
        {
            if (ctr.InvokeRequired)
            {
                if (ctr == null || index == 0 || Data == null) return;
                UPdateCallBack updatecallback = new UPdateCallBack(UpdateUI);
                ctr.Invoke(updatecallback, ctr, index, Data);
            }
            else
            {

                switch (index)
                {
                    case Action_ID.Text:
                        ctr.Text = Data;
                        break;
                    case Action_ID.BackColor:
                        switch (Data)
                        {
                            case "Red":
                                ctr.BackColor = System.Drawing.Color.Red;
                                break;
                            case "Lime":
                                ctr.BackColor = System.Drawing.Color.Lime;
                                break;
                            case "Gray":
                                ctr.BackColor = System.Drawing.Color.Gray;
                                break;
                        }
                        break;
                    case Action_ID.Enable:
                        ctr.Enabled = Data == "True" ? true : false;
                        break;
                    case Action_ID.Item:
                        ListBox listbox = (ListBox)ctr;
                        string[] sArray = Data.Split('@');
                        if (sArray[0] == "Remove")
                            listbox.Items.RemoveAt(0);
                        else
                            listbox.Items.Add(sArray[1]);
                        break;
                    case Action_ID.Msg:
                        ctr.Text = Data+"\r\n" + ctr.Text;
                        break;
                }
                Application.DoEvents();
            }
        }
        #endregion

    }
}
