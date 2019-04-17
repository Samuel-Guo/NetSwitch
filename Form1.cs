using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetSwitch
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        class Cmds
        {

            public  string exepath;
            public string args;
            public bool omitErr;
            public Cmds(string exepath, string args,bool omitErr=false)
            {
                this.exepath = exepath;
                this.args = args;
                this.omitErr = omitErr;
            }
        } 
        /// <summary>
        /// 确定当前主体是否属于具有指定 Administrator 的 Windows 用户组
        /// </summary>
        /// <returns>如果当前主体是指定的 Administrator 用户组的成员，则为 true；否则为 false。</returns>
        public  bool IsAdministrator()
        {
            bool result;
            try
            {
                System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent();
                System.Security.Principal.WindowsPrincipal principal = new System.Security.Principal.WindowsPrincipal(identity);
                result = principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);

                //http://www.cnblogs.com/Interkey/p/RunAsAdmin.html
                //AppDomain domain = Thread.GetDomain();
                //domain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);
                //WindowsPrincipal windowsPrincipal = (WindowsPrincipal)Thread.CurrentPrincipal;
                //result = windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                result = false;
            }
            return result;
        }

        public static bool Delay(double delaySecond)
        {
            DateTime now = DateTime.Now;
            long s;
            do
            {
                TimeSpan spand = DateTime.Now - now;
                s = spand.Milliseconds;
                Application.DoEvents();
            }
            while (s < delaySecond*1000);
            return true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Visible = false;
            this.ShowInTaskbar = false;
            AsyncGetNetStatus();
            //wifiManage wifi = new wifiManage();
            //var t =wifiManage.ScanAllSSID();
            //var n= t.Find(s => s.profileNames == "Samuel");
            //wifiManage.ConnectToSSID( n, "1234567890");
            ////wifiManage.GetCurrentConnection
        }


        private string RunSyncAndGetResults(string path,string args="",bool omitErr =false)

        {

            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo(path);

            psi.RedirectStandardOutput = true;

            psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Minimized;

            psi.UseShellExecute = false;

            psi.Arguments = args;

            psi.CreateNoWindow = true;
            System.Diagnostics.Process listFiles;

            listFiles = System.Diagnostics.Process.Start(psi);

            System.IO.StreamReader myOutput = listFiles.StandardOutput;

            listFiles.WaitForExit(10000);

            if (listFiles.HasExited)

            {

                string output = myOutput.ReadToEnd();

                return output;

            }
            else
            {
                if (!omitErr)
                    throw new Exception(path + " " + args + " 命令未执行成功!");
                else
                    return "-1";
            }
        }


        private void ToolStripMenuItem2_Click(object sender, EventArgs e)
        {

        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void NotifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            //GetNetStatus();
            //if (e.Button == MouseButtons.Right)
            //{
                contextMenuStrip1.Show();
            //}


            //if (e.Button == MouseButtons.Left)
            //{
            //    this.Visible = true;
            //    this.WindowState = FormWindowState.Normal;
            //    this.ShowInTaskbar = true;
            //}
        }


        private void 切换到内网ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            notifyIcon1.ShowBalloonTip(4000, "提示", "切换中...", ToolTipIcon.Info);

            List<Cmds> cmds = new List<Cmds>();
            cmds.Add(new Cmds("netsh",@"interface set interface ""VMware Network Adapter VMnet1"" enabled",true));
            cmds.Add(new Cmds("netsh", @"interface set interface ""VMware Network Adapter VMnet8"" enabled",true));
            cmds.Add(new Cmds("netsh", @"interface set interface ""eth"" enabled"));
            cmds.Add(new Cmds("netsh", @"interface set interface ""WLAN"" disable"));

            //netsh interface set interface "VMware Network Adapter VMnet8" enabled
            //netsh interface set interface "eth" enabled
            //netsh interface set interface "WLAN" disabled")
            //  var ree = RunSyncAndGetResults("netsh");
            try
            {
                foreach (var item in cmds)
                {
                    var re = RunSyncAndGetResults(item.exepath, item.args,item.omitErr);
                }

            }
            catch (Exception ex)
            {

                notifyIcon1.ShowBalloonTip(4000, "提示", ex.Message, ToolTipIcon.Error);
            }
            notifyIcon1.ShowBalloonTip(4000, "提示", "已成功切换到内网！", ToolTipIcon.Info );

        }

        private void 切换到外网ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            notifyIcon1.ShowBalloonTip(4000, "提示", "切换中...", ToolTipIcon.Info);

            List<Cmds> cmds = new List<Cmds>();
            cmds.Add(new Cmds("netsh", @"interface set interface ""WLAN"" enable"));
            cmds.Add(new Cmds("netsh", @"interface set interface ""VMware Network Adapter VMnet1"" disable"));
            cmds.Add(new Cmds("netsh", @"interface set interface ""VMware Network Adapter VMnet8"" disable"));
            cmds.Add(new Cmds("netsh", @"interface set interface ""eth"" disable"));

            //netsh interface set interface "VMware Network Adapter VMnet8" enabled
            //netsh interface set interface "eth" enabled
            //netsh interface set interface "WLAN" disabled")
            //  var ree = RunSyncAndGetResults("netsh");
            try
            {
                foreach (var item in cmds)
                {
                    var re = RunSyncAndGetResults(item.exepath, item.args);
                    Delay(0.5);
                }

            }
            catch (Exception ex)
            {

                notifyIcon1.ShowBalloonTip(4000, "提示", ex.Message, ToolTipIcon.Error);
            }
            notifyIcon1.ShowBalloonTip(4000, "提示", "已成功切换到外网！", ToolTipIcon.Info);

            Delay(1);
            if (wifiManage.GetCurrentConnection() == "NARI-5G" || wifiManage.GetCurrentConnection() == "NARI")
                return;

            var wifilist= wifiManage.ScanAllSSID();
            foreach (var item in wifilist)
            {
                if((item.profileNames== "NARI-5G" || item.profileNames == "NARI"))
                {
                    var xmllist = wifiManage.ListWifiXml();
                    foreach (var item2 in xmllist)
                    {
                        if (item2.Key == "NARI-5G" || item2.Key == "NARI")
                        {
                            wifiManage.connectViaXml(item,item2.Key,item2.Value);
                            notifyIcon1.ShowBalloonTip(5000, "提示", "连接"+item2.Key+"成功！", ToolTipIcon.Info);

                            break;
                        }
                    }

                }
            }
        }
        DateTime lastGetStatusTime;
        private void NotifyIcon1_MouseMove(object sender, MouseEventArgs e)
        {
            TimeSpan span = DateTime.Now - lastGetStatusTime;

            if (span.TotalSeconds > 3)
            {
                timetick = -1;

               // GetNetStatus();
                lastGetStatusTime = DateTime.Now;
            }
        }

        private void AsyncGetNetStatus()
        {
            Action handler = new Action(GetNetStatus);
            handler.BeginInvoke(null, null);
            //GetNetStatus();
        }

        private void GetNetStatus()
        {
            if (wifiManage.GetCurrentConnection() == "")
            {
                notifyIcon1.Text = "有线连接";
                MenuLAN.Text = "内网*";
                MenuWifi.Text = "切换到外网";
            }
            else
            {

                MenuLAN.Text = "切换到内网";
                MenuWifi.Text = "外网*";

                //MenuLAN.Enabled = true;
                //MenuWifi.Enabled = false;
                if (wifiManage.getNetStatus())

                    notifyIcon1.Text = "Wifi连接(联网)";
                else
                    notifyIcon1.Text = "Wifi连接(未联网)";
            }
        }

        private int timetick;
        private void Timer1_Tick(object sender, EventArgs e)
        {

            timetick++;
            if (timetick % 60==0)
            {
               // timer1.Enabled = false;
                //timetick = 0;
                lastGetStatusTime = DateTime.Now;
                AsyncGetNetStatus();
                //timetick++;
            }
        }
    }
}
