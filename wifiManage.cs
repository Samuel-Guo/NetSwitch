using NativeWifi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;




namespace NetSwitch
{
    class wifiManage
    {
        [DllImport("wininet")]
        private extern static bool InternetGetConnectedState(out int connectionDescription, int reservedValue);

        /// <summary>
        /// 检测本机是否联网
        /// </summary>
        /// <returns></returns>
        public static bool IsConnectedInternet()
        {
            int i = 0;
            if (InternetGetConnectedState(out i, 0))
            {
                //已联网
                return true;
            }
            else
            {
                //未联网
                return false;
            }
        }


        public static bool getNetStatus()
        {
            System.Net.NetworkInformation.Ping ping;
            System.Net.NetworkInformation.PingReply ret;
            ping = new System.Net.NetworkInformation.Ping();
            try
            {
                ret = ping.Send("www.baidu.com");
                if (ret.Status != System.Net.NetworkInformation.IPStatus.Success)
                {
                    //没网
                    return false;
                }
                else
                {
                    //有网
                    return true;
                }
            }
            catch (Exception err)
            {
            //    MessageBox.Show("获取网络状态异常：" + err.ToString());
                //MessageBox.Show("获取网络状态异常");
                return false;
            }
        }


        /// <summary>  
        /// 枚举所有无线设备接收到的SSID  
        /// </summary>  
        /// 
        /// <summary>
        /// Converts a 802.11 SSID to a string.
        /// </summary>
        static string GetStringForSSID(Wlan.Dot11Ssid ssid)
        {
            return Encoding.ASCII.GetString(ssid.SSID, 0, (int)ssid.SSIDLength);
        }

        public static List<WIFISSID> ScanAllSSID()
        {
            List<WIFISSID> wifiList = new List<WIFISSID>();
            string conectedNetworkName = string.Empty;
            foreach (WlanClient.WlanInterface wlanIface in client.Interfaces)
            {
                List<string> profileNames = new List<string>();
                // Lists all networks with WEP security  
                Wlan.WlanAvailableNetwork[] networks = wlanIface.GetAvailableNetworkList(0);
                foreach (Wlan.WlanAvailableNetwork network in networks)
                {
                    if (wlanIface.InterfaceState == Wlan.WlanInterfaceState.Connected && wlanIface.CurrentConnection.isState == Wlan.WlanInterfaceState.Connected)
                    {
                        conectedNetworkName = wlanIface.CurrentConnection.profileName;
                    }

                    WIFISSID targetSSID = new WIFISSID();
                    if (network.networkConnectable)
                    {
                        targetSSID.SSID = network.dot11Ssid;
                        if (string.IsNullOrEmpty(network.profileName))
                        {
                            targetSSID.profileNames = GetStringForSSID(network.dot11Ssid);
                        }
                        else
                        {
                            targetSSID.profileNames = network.profileName;
                        }


                        if (!profileNames.Contains(targetSSID.profileNames))
                        {
                            profileNames.Add(targetSSID.profileNames);
                            targetSSID.wlanInterface = wlanIface;
                            targetSSID.wlanSignalQuality = (int)network.wlanSignalQuality;
                            targetSSID.dot11DefaultAuthAlgorithm = network.dot11DefaultAuthAlgorithm;
                            targetSSID.dot11DefaultCipherAlgorithm = network.dot11DefaultCipherAlgorithm;
                            targetSSID.securityEnabled = network.securityEnabled;
                            wifiList.Add(targetSSID);
                            if (!string.IsNullOrEmpty(conectedNetworkName) && conectedNetworkName.Equals(network.profileName))
                            {
                                targetSSID.connected = true;
                            }
                            else
                            {
                                targetSSID.connected = false;
                            }
                        }
                    }
                }
            }

            return wifiList;
        }

        public static Dictionary<string,string> ListWifiXml()
        {
            Dictionary<string, string> wifiList = new Dictionary<string, string>();
            foreach (WlanClient.WlanInterface wlanIface in client.Interfaces)
            {
                // Lists all networks with WEP security
                //Wlan.WlanAvailableNetwork[] networks = wlanIface.GetAvailableNetworkList(0);
                //foreach (Wlan.WlanAvailableNetwork network in networks)
                //{
                //    //if ( network.dot11DefaultCipherAlgorithm == Wlan.Dot11CipherAlgorithm.WEP )
                //    //{
                //    Console.WriteLine("Found WEP network with SSID {0}.", GetStringForSSID(network.dot11Ssid));
                //    //}
                //}

                // Retrieves XML configurations of existing profiles.
                // This can assist you in constructing your own XML configuration
                // (that is, it will give you an example to follow).
                foreach (Wlan.WlanProfileInfo profileInfo in wlanIface.GetProfiles())
                {
                    string name = profileInfo.profileName; // this is typically the network's SSID
                    string xml = wlanIface.GetProfileXml(profileInfo.profileName);

                    //wlanIface.SetProfile(Wlan.WlanProfileFlags.AllUser, xml, true);
                    //wlanIface.Connect(Wlan.WlanConnectionMode.Profile, Wlan.Dot11BssType.Any, name);
                    wifiList.Add(name,xml);
                }

            }
            return wifiList;
        }

        public class WIFISSID
        {
            public string profileNames;
            public Wlan.Dot11Ssid SSID;
            public NativeWifi.Wlan.Dot11AuthAlgorithm dot11DefaultAuthAlgorithm;
            
            public NativeWifi.Wlan.Dot11CipherAlgorithm dot11DefaultCipherAlgorithm;
            public bool networkConnectable = true;
            public string wlanNotConnectableReason = "";
            public int wlanSignalQuality = 0;
            public WlanClient.WlanInterface wlanInterface = null;
            public bool securityEnabled;
            public bool connected = false;
        }
        private static WlanClient client=new WlanClient();

        public static string GetCurrentConnection()
        {
            try
            {
                foreach (WlanClient.WlanInterface wlanIface in client.Interfaces)
                {
                    Wlan.WlanAvailableNetwork[] networks = wlanIface.GetAvailableNetworkList(0);
                    foreach (Wlan.WlanAvailableNetwork network in networks)
                    {
                        if (wlanIface.InterfaceState == Wlan.WlanInterfaceState.Connected && wlanIface.CurrentConnection.isState == Wlan.WlanInterfaceState.Connected)
                        {
                            return wlanIface.CurrentConnection.profileName;
                        }
                    }
                }
                return string.Empty;


            }
            catch (Exception)
            {

                return string.Empty;

            }


        }
        /// <summary> 
        /// 连接到无线网络
        /// </summary> 
        /// <param name="ssid"></param> 
        public static bool ConnectToSSID(WIFISSID ssid, string key)
        {

            try
            {
                String auth = string.Empty;
                String cipher = string.Empty;
                bool isNoKey = false;
                String keytype = string.Empty;
                switch (ssid.dot11DefaultAuthAlgorithm)
                {
                    case Wlan.Dot11AuthAlgorithm.IEEE80211_Open:
                        auth = "open"; break;
                    //case Wlan.Dot11AuthAlgorithm.IEEE80211_SharedKey: 
                    // 'not implemented yet; 
                    //break; 
                    case Wlan.Dot11AuthAlgorithm.RSNA:
                        auth = "WPA2PSK"; break;
                    case Wlan.Dot11AuthAlgorithm.RSNA_PSK:
                        auth = "WPA2PSK"; break;
                    case Wlan.Dot11AuthAlgorithm.WPA:
                        auth = "WPAPSK"; break;
                    case Wlan.Dot11AuthAlgorithm.WPA_None:
                        auth = "WPAPSK"; break;
                    case Wlan.Dot11AuthAlgorithm.WPA_PSK:
                        auth = "WPAPSK"; break;
                }

                switch (ssid.dot11DefaultCipherAlgorithm)
                {
                    case Wlan.Dot11CipherAlgorithm.CCMP:
                        cipher = "AES";
                        keytype = "passPhrase";
                        break;
                    case Wlan.Dot11CipherAlgorithm.TKIP:
                        cipher = "TKIP";
                        keytype = "passPhrase";
                        break;
                    case Wlan.Dot11CipherAlgorithm.None:
                        cipher = "none"; keytype = "";
                        isNoKey = true;
                        break;
                    case Wlan.Dot11CipherAlgorithm.WEP:
                        cipher = "WEP";
                        keytype = "networkKey";
                        break;
                    case Wlan.Dot11CipherAlgorithm.WEP40:
                        cipher = "WEP";
                        keytype = "networkKey";
                        break;
                    case Wlan.Dot11CipherAlgorithm.WEP104:
                        cipher = "WEP";
                        keytype = "networkKey";
                        break;
                }

                if (isNoKey && !string.IsNullOrEmpty(key))
                {
                   // infoTB.Text = "无法连接网络！";
                    Loger.WriteLog("无法连接网络",
                        "SSID:" + ssid.SSID + "\r\n"
                        + "Dot11AuthAlgorithm:" + ssid.dot11DefaultAuthAlgorithm + "\r\n"
                        + "Dot11CipherAlgorithm:" + ssid.dot11DefaultAuthAlgorithm.ToString());
                    return false;
                }
                else if (!isNoKey && string.IsNullOrEmpty(key))
                {
                   // infoTB.Text = "无法连接网络！";
                    Loger.WriteLog("无法连接网络",
                        "SSID:" + ssid.SSID + "\r\n"
                        + "Dot11AuthAlgorithm:" + ssid.dot11DefaultAuthAlgorithm + "\r\n"
                        + "Dot11CipherAlgorithm:" + ssid.dot11DefaultAuthAlgorithm.ToString());
                    return false;
                }
                else
                {
                    string profileName = ssid.profileNames; // this is also the SSID 
                    string mac = StringToHex(profileName);
                    string profileXml = string.Empty;
                    if (!string.IsNullOrEmpty(key))
                    {
                        profileXml = string.Format("<?xml version=\"1.0\"?><WLANProfile xmlns=\"http://www.microsoft.com/networking/WLAN/profile/v1\"><name>{0}</name><SSIDConfig><SSID><hex>{1}</hex><name>{0}</name></SSID></SSIDConfig><connectionType>ESS</connectionType><connectionMode>auto</connectionMode><autoSwitch>false</autoSwitch><MSM><security><authEncryption><authentication>{2}</authentication><encryption>{3}</encryption><useOneX>false</useOneX></authEncryption><sharedKey><keyType>{4}</keyType><protected>false</protected><keyMaterial>{5}</keyMaterial></sharedKey><keyIndex>0</keyIndex></security></MSM></WLANProfile>",
                            profileName, mac, auth, cipher, keytype, key);
                    }
                    else
                    {
                        profileXml = string.Format("<?xml version=\"1.0\"?><WLANProfile xmlns=\"http://www.microsoft.com/networking/WLAN/profile/v1\"><name>{0}</name><SSIDConfig><SSID><hex>{1}</hex><name>{0}</name></SSID></SSIDConfig><connectionType>ESS</connectionType><connectionMode>auto</connectionMode><autoSwitch>false</autoSwitch><MSM><security><authEncryption><authentication>{2}</authentication><encryption>{3}</encryption><useOneX>false</useOneX></authEncryption></security></MSM></WLANProfile>",
                            profileName, mac, auth, cipher, keytype);
                    }

                    bool success = connectViaXml(ssid, profileName, profileXml);
                    if (!success)
                    {
                        //infoTB.Text = "连接网络失败！";
                        Loger.WriteLog("连接网络失败",
                            "SSID:" + ssid.SSID + "\r\n"
                            + "Dot11AuthAlgorithm:" + ssid.dot11DefaultAuthAlgorithm + "\r\n"
                            + "Dot11CipherAlgorithm:" + ssid.dot11DefaultAuthAlgorithm.ToString() + "\r\n");
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
               // infoTB.Text = "无法连接网络！" + e.Message;
                Loger.WriteLog("无法连接网络",
                    "SSID:" + ssid.SSID+ "\r\n"
                    + "Dot11AuthAlgorithm:" + ssid.dot11DefaultAuthAlgorithm + "\r\n"
                    + "Dot11CipherAlgorithm:" + ssid.dot11DefaultAuthAlgorithm.ToString() + "\r\n"
                    + e.Message);
                return false;
            }
            return true;
        }

        public static bool connectViaXml(WIFISSID ssid, string profileName, string profileXml)
        {
            ssid.wlanInterface.SetProfile(Wlan.WlanProfileFlags.AllUser, profileXml, true);
            //ssid.wlanInterface.Connect(Wlan.WlanConnectionMode.Profile, Wlan.Dot11BssType.Any, ssid.profileNames);
            bool success = ssid.wlanInterface.ConnectSynchronously(Wlan.WlanConnectionMode.Profile, Wlan.Dot11BssType.Any, profileName, 15000);
            return success;
        }

        /// <summary>
        /// 字符串转Hex
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static string StringToHex(string str)
        {
            StringBuilder sb = new StringBuilder();
            byte[] byStr = System.Text.Encoding.Default.GetBytes(str); //默认是System.Text.Encoding.Default.GetBytes(str)
            for (int i = 0; i < byStr.Length; i++)
            {
                sb.Append(Convert.ToString(byStr[i], 16));
            }
            return (sb.ToString().ToUpper());

        }
    }

    internal class Loger
    {
        internal static void WriteLog(string v, object p)
        {
            //throw new NotImplementedException();
        }
    }
}
