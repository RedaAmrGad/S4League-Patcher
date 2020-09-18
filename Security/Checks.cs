using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace S4Launcher.Security
{
    public class Checks
    {
        public static void Networkcheck()
        {

            try
            {
                using(var client = new WebClient())
                using (client.OpenRead("http://clients3.google.com/generate_204"))
                { }
            }
            catch (WebException)
            {
                MessageBox.Show("You Have No Internet Connection !");
                Environment.Exit(0);
            }
        }

        public static bool Clientcheck()
        {
            if (!File.Exists("S4Client.exe")) { return false; }
            var versionInfo = FileVersionInfo.GetVersionInfo(@"S4Client.exe");
            string version = versionInfo.ProductVersion;
            if (version == "0, 8, 48, 101816")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void OnlinePlayers()
        {

            Constants.LoginWindow.playerscount.Content = new WebClient().DownloadString(Constants.UIp + "players.txt");

        }


        public static void IsS4Running()
        {
            try
            {

                Process[] S4 = Process.GetProcessesByName("S4Client");
                if (S4.Length != 0)
                {
                    S4[0].Kill();
                    Environment.Exit(-1);
                }
            }
            catch (Exception)
            {
                Environment.Exit(-1);
            }

        }
        public static void Serverstat()
        {
            using (TcpClient tcpClient = new TcpClient())
            {
                try
                {
                    tcpClient.Connect(Constants.Ip, Convert.ToInt32(Constants.APort));
                    Constants.LoginWindow.loginstat.Content = "Online";
                }
                catch (Exception)
                {
                    Constants.LoginWindow.loginstat.Content = "OFFLINE";
                    Constants.LoginWindow.loginstat.Foreground = new SolidColorBrush(Colors.Red);
                    MessageBox.Show("AuthServer is Offline. Please Try Again Later", "Maintenance");
                    Environment.Exit(0);

                }
            }

            using (TcpClient tcpClient = new TcpClient())
            {
                try
                {
                    tcpClient.Connect(Constants.Ip, Convert.ToInt32(Constants.GPort));
                    Constants.LoginWindow.serverstat.Content = "Online";
                }
                catch (Exception)
                {
                    Constants.LoginWindow.serverstat.Content = "OFFLINE";
                    Constants.LoginWindow.serverstat.Foreground = new SolidColorBrush(Colors.Red);
                    MessageBox.Show("GameServer is Offline . Please Try Again Later", "Maintenance");
                    Environment.Exit(0);
                }
            }

        }

        /*     public static void LoadAccount()
             {
               if (File.Exists("account.txt"))
               {
                     string[] lines = File.ReadAllLines("account.txt");
                         for (int i = 0; i < lines.Length; i++)
                     {
                         if (lines != null & lines[0]!=null & lines[1] != null & lines[2] != null & lines[3] != null & lines[3] != null)
                             {
                             if (lines[0].Contains("2"))
                             {
                                 Constants.LoginWindow.login_username.Text = lines[1];
                                 Constants.LoginWindow.login_passwd.Password = lines[2];
                                 if (lines[3] == "original") { Constants.LoginWindow.Coriginal.IsChecked = true; }
                                 else if (lines[3] == "original") { Constants.LoginWindow.Coriginal.IsChecked = true; }
                                 else if (lines[3] == "red") { Constants.LoginWindow.Coriginal.IsChecked = true; }
                                 else if (lines[3] == "gold") { Constants.LoginWindow.Coriginal.IsChecked = true; }
                                 else if (lines[3] == "blue") { Constants.LoginWindow.Coriginal.IsChecked = true; }
                                 else if (lines[3] == "violet") { Constants.LoginWindow.Coriginal.IsChecked = true; }
                                 if (lines[4] == "eng") { Constants.LoginWindow.eng.IsChecked = true; }
                                 else if (lines[4] == "fre") { Constants.LoginWindow.fre.IsChecked = true; }
                                 else if (lines[4] == "ger") { Constants.LoginWindow.ger.IsChecked = true; }
                                 else if (lines[4] == "ita") { Constants.LoginWindow.ita.IsChecked = true; }
                                 else if (lines[4] == "jap") { Constants.LoginWindow.jap.IsChecked = true; }
                                 else if (lines[4] == "rus") { Constants.LoginWindow.rus.IsChecked = true; }
                                 else if (lines[4] == "spa") { Constants.LoginWindow.spa.IsChecked = true; }
                                 else if (lines[4] == "tur") { Constants.LoginWindow.tur.IsChecked = true; }
                                 Constants.LoginWindow.savebox.IsChecked = true;
                             }
                            else  if (lines[0].Contains("1"))
                             {
                                 Constants.LoginWindow.login_username.Text = "";
                                 Constants.LoginWindow.login_passwd.Password = "";
                                 Constants.LoginWindow.savebox.IsChecked = false;
                             }
                         }
                     }
               }*/

    }


}
