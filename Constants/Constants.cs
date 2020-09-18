using System;
using System.Collections.Generic;
using System.Net;
namespace S4Launcher
{

    public class Constants
    {
        public static string Ip = new WebClient().DownloadString("https://textuploader.com/1d07y/raw");
        public static string APort = new WebClient().DownloadString("https://textuploader.com/1d07b/raw");
        public static string GPort = new WebClient().DownloadString("https://textuploader.com/1d07w/raw");
        public static string UIp = new WebClient().DownloadString("https://textuploader.com/1dklp/raw");
        public static MainWindow LoginWindow;
        public static IPEndPoint ConnectEndPoint = new IPEndPoint(IPAddress.Parse(Ip), Convert.ToInt32(APort));
        public static List<string> ModifiedFiles = new List<string>();
        public static List<string> DownloadedFiles = new List<string>();
    }
 }
