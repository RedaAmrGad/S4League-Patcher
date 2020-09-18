using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using S4Launcher.Security;
namespace S4Launcher.Update
{
    class SUpdate
    {
        public static FileDownloader _downloader = new FileDownloader();
        public static void Update()
        {
            string Name = AppDomain.CurrentDomain.FriendlyName;
            if (Name == "S4Patcher_update.exe")
            {
                using (var client = new WebClient())
                {
                    Constants.ModifiedFiles.Add("S4Patcher.zip");
                    _downloader.Download();
                }
            }
            else
            {
                var v1 = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                var v2 = new WebClient().DownloadString(Constants.UIp + "s4patcher.txt");
                var Pcurrentver = new Version(v1);
                var Ponlinever = new Version(v2);

                var result = Pcurrentver.CompareTo(Ponlinever);
                if (result > 0)
                {
                    MessageBox.Show("Something Went Wrong With S4Patcher Assembly , Try Downloading The Patcher Again From Our Discord Server");
                    Environment.Exit(0);
                }
                else if (result < 0)
                {
                    if (File.Exists(@"S4Patcher_update.exe"))
                    {
                        File.Delete(@"S4Patcher_update.exe");
                    }
                    MessageBox.Show("S4Patcher Is Being Updated");
                    File.Copy(@"S4Patcher.exe", @"S4Patcher_update.exe");
                    Process.Start(@"S4Patcher_update.exe");
                    Environment.Exit(0);
                }
                else
                {
                    if (File.Exists(@"S4Patcher_update.exe"))
                    {
                        File.Delete(@"S4Patcher_update.exe");
                    }
                    Constants.LoginWindow.Update();                                                       //Checks For Updates
                }

            }
            
        }
    }
}
