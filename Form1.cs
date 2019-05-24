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

        readonly Cmds wifiE = new Cmds("netsh", @"interface set interface ""WLAN"" enable");

        bool IsOnline = false;
        bool IsWifi = false;
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
            double s;
            do
            {
                TimeSpan spand = DateTime.Now - now;
                s = spand.TotalMilliseconds;
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

            listFiles.WaitForExit(20000);

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
            if (e.Button == MouseButtons.Right)
            {

                contextMenuStrip1.Show();
            }


            //if (e.Button == MouseButtons.Left)
            //{
            //    this.Visible = true;
            //    this.WindowState = FormWindowState.Normal;
            //    this.ShowInTaskbar = true;
            //}
        }


        private void 切换到内网ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            notifyIcon1.ShowBalloonTip(2000, "提示", "切换中...", ToolTipIcon.Info);

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

                notifyIcon1.ShowBalloonTip(2000, "提示", ex.Message, ToolTipIcon.Error);
            }
            notifyIcon1.ShowBalloonTip(2000, "提示", "已成功切换到内网！", ToolTipIcon.Info );

        }

        private void 切换到外网ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            notifyIcon1.ShowBalloonTip(2000, "提示", "切换中...", ToolTipIcon.Info);

            List<Cmds> cmds = new List<Cmds>();
            cmds.Add(wifiE);
            cmds.Add(new Cmds("netsh", @"interface set interface ""VMware Network Adapter VMnet1"" disable"));
            cmds.Add(new Cmds("netsh", @"interface set interface ""VMware Network Adapter VMnet8"" disable"));
            cmds.Add(new Cmds("netsh", @"interface set interface ""eth"" disable"));

            //netsh interface set interface "VMware Network Adapter VMnet8" enabled
            //netsh interface set interface "eth" enabled
            //netsh interface set interface "WLAN" disabled")
            //  var ree = RunSyncAndGetResults("netsh");
            int retry = 0;
            while (true)
            {
                try
                {
                    foreach (var item in cmds)
                    {
                        var re = RunSyncAndGetResults(item.exepath, item.args);
                        Delay(0.5);
                    }
                    notifyIcon1.ShowBalloonTip(2000, "提示", "已成功切换到外网！", ToolTipIcon.Info);
                    break;
                }
                catch (Exception ex)
                {
                    retry++;
                    notifyIcon1.ShowBalloonTip(2000, "提示", ex.Message, ToolTipIcon.Error);

                    if (retry > 3)
                    {
                        notifyIcon1.ShowBalloonTip(2000, "提示", "重试" +retry+"次失败", ToolTipIcon.Error);

                        break;
                    }
                }
            }
            Delay(1);
            var CurrentWifi = wifiManage.GetCurrentConnection();
            if (CurrentWifi != "NARI-5G" && CurrentWifi != "NARI")
            {
                var wifilist = wifiManage.ScanAllSSID();
                foreach (var item in wifilist)
                {
                    if ((item.profileNames == "NARI-5G" || item.profileNames == "NARI"))
                    {
                        var xmllist = wifiManage.ListWifiXml();
                        foreach (var item2 in xmllist)
                        {
                            if (item2.Key == "NARI-5G" || item2.Key == "NARI")
                            {
                                if (wifiManage.connectViaXml(item, item2.Key, item2.Value))
                                {
                                    notifyIcon1.ShowBalloonTip(5000, "提示", "连接" + item2.Key + "成功！", ToolTipIcon.Info);

                                    break;
                                }
                            }
                        }

                    }
                }

            }



            if (!wifiManage.getNetStatus())
                System.Diagnostics.Process.Start(@"C:\Program Files (x86)\iNode\iNode Client\iNode Client.exe", " -p 5020 -c 5021");

        }
        DateTime lastGetStatusTime;
        private void NotifyIcon1_MouseMove(object sender, MouseEventArgs e)
        {
            TimeSpan span = DateTime.Now - lastGetStatusTime;
            this.SetTextAndIcon();

            if (span.TotalSeconds > 3)
            {
                timetick = 0;

               // GetNetStatus();
                lastGetStatusTime = DateTime.Now;
            }
        }

        private void AsyncGetNetStatus()
        {
            Action handler = new Action(GetNetStatus);
            handler.BeginInvoke(CallDone, null);
            //GetNetStatus();
        }

        private void CallDone(IAsyncResult asyncResult)
        {
            timetick = 0;
        }


        private void GetNetStatus()
        {

            IsWifi = (wifiManage.GetCurrentConnection() != "");
            IsOnline = wifiManage.getNetStatus();

        }

        private bool SetTextAndIcon()
        {
            if(IsWifi && IsOnline)
            {
                notifyIcon1.Text = "Wifi连接(联网)";
                notifyIcon1.Icon = Resource1.WifiOn;
                MenuLAN.Text = "切换到内网";
                MenuWifi.Text = "外网*";

                return true;
            }
            else if(IsWifi && !IsOnline)
            {
                notifyIcon1.Text = "Wifi连接";
                notifyIcon1.Icon = Resource1.WifiOff;
                MenuLAN.Text = "切换到内网";
                MenuWifi.Text = "外网*";

                return false;

            }
            else if (!IsWifi && IsOnline)
            {
                notifyIcon1.Text = "有线连接(联网)";
                MenuLAN.Text = "内网*";
                MenuWifi.Text = "切换到外网";
                notifyIcon1.Icon = Resource1.LANOn;

                return false;

            }
            else 
            {
                notifyIcon1.Text = "有线连接";
                MenuLAN.Text = "内网*";
                MenuWifi.Text = "切换到外网";
                notifyIcon1.Icon = Resource1.LANOff;

                return false;

            }
            //if (!IsWifi)
            //{
            //    notifyIcon1.Text = "有线连接";
            //    MenuLAN.Text = "内网*";
            //    MenuWifi.Text = "切换到外网";
            //    return false;
            //}
            //else
            //{

            //    MenuLAN.Text = "切换到内网";
            //    MenuWifi.Text = "外网*";

            //    //MenuLAN.Enabled = true;
            //    //MenuWifi.Enabled = false;
            //    if (IsOnline)
            //    {
            //        notifyIcon1.Text = "Wifi连接(联网)";
            //        return true;
            //    }
            //    else
            //    {
            //    }
            //}
        }

        private int timetick;
        private void Timer1_Tick(object sender, EventArgs e)
        {
            Console.WriteLine(timetick.ToString());

            timetick++;
            if(timetick % 60 ==1)
                SetTextAndIcon();

            if (timetick % 60==10)
            {
               // timer1.Enabled = false;
                //timetick = 0;
                lastGetStatusTime = DateTime.Now;
                AsyncGetNetStatus();
                if (!IsOnline && timetick % 300 == 299)
                {
                    TrySamuel();
                }
                //timetick++;
            }
        }

        private void TrySamuel()
        {
            try
            {
                var re = RunSyncAndGetResults(wifiE.exepath, wifiE.args);

                var CurrentWifi = wifiManage.GetCurrentConnection();
                if (CurrentWifi != "Samuel")
                {
                    var wifilist = wifiManage.ScanAllSSID();
                    foreach (var item in wifilist)
                    {
                        if (item.profileNames == "Samuel" )
                        {
                            var xmllist = wifiManage.ListWifiXml();
                            foreach (var item2 in xmllist)
                            {
                                if (item2.Key == "Samuel")
                                {
                                    if (wifiManage.connectViaXml(item, item2.Key, item2.Value))
                                    {
                                        notifyIcon1.ShowBalloonTip(5000, "提示", "连接" + item2.Key + "成功！", ToolTipIcon.Info);

                                        break;
                                    }
                                }
                            }

                        }
                    }

                }

            }
            catch (Exception)
            {

               // throw;
            }
            Delay(0.5);

        }

        private void Button1_Click(object sender, EventArgs e)
        {

        }

        private void NotifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if(IsWifi)
            {
                切换到内网ToolStripMenuItem_Click(sender, e);
            }
            else
            {
                切换到外网ToolStripMenuItem_Click(sender, e);
            }
        }
    }
}
