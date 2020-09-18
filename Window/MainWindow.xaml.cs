using System;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using S4Launcher.LoginAPI;
using S4Launcher.Update;
using Ionic.Zip;
using System.IO;
using System.ComponentModel;
using System.Text.RegularExpressions;
using S4Launcher.Security;
using System.Windows.Controls;
using System.Threading;
using System.Reflection;

namespace S4Launcher
{
    public partial class MainWindow : Window
    {
        public static bool IsAttached { get; }
        private Settings S4Settings = Settings.CurrentInstance();
        private int currentOnlineS4Version;
        private int currentUpdateIndex;
        private int totalUpdateIndex;
        private int currentUpdate;
        private AccountSetting AccountSettings;
        public byte BState = 0;
        private string news1 = "";
        private string news2 = "";
        private string news3 = "";
        private string news4 = "";
        private string news5 = "";
        private string news6 = "";
        private string news7 = "";
        private string news8 = "";
        private string news9 = "";
        private string news10 = "";
        private WebClient client;
        private Stopwatch stopwatch;

        #region Events




        public event UpdaterDownloadProgressChangedEventHandler UpdaterDownloadProgressChangedHandler;
        internal virtual void OnUpdaterDownloadProgressChanged(UpdaterDownloadProgressChangedEventArgs e)
        {
            UpdaterDownloadProgressChangedHandler?.Invoke(this, e);
        }


        public event UpdaterExtractionProgressEventHandler UpdaterExtractionProgressChangedHandler;
        internal virtual void OnUpdaterExtractionProgress(UpdaterExtractionProgressEventArgs e)
        {
            UpdaterExtractionProgressChangedHandler?.Invoke(this, e);
        }


        #endregion

        #region EventArgs



        public class UpdaterDownloadProgressChangedEventArgs : EventArgs
        {
            public int DownloadPercentage { get; internal set; }
            public int CurrentUpdate { get; internal set; }
            public int TotalUpdate { get; internal set; }

            public UpdaterDownloadProgressChangedEventArgs() { }

            public UpdaterDownloadProgressChangedEventArgs(int downloadPercentage, int currentUpdate, int totalUpdate)
            {
                this.DownloadPercentage = downloadPercentage;
                this.CurrentUpdate = currentUpdate;
                this.TotalUpdate = totalUpdate;
            }
        }

        public class UpdaterExtractionProgressEventArgs : EventArgs
        {
            public string EntryName { get; internal set; }
            public int CurrentEntry { get; internal set; }
            public int TotalEntries { get; internal set; }

            public UpdaterExtractionProgressEventArgs() { }

            public UpdaterExtractionProgressEventArgs(int currentEntry, string entryName, int totalEntries)
            {
                this.EntryName = entryName;
                this.CurrentEntry = currentEntry;
                this.TotalEntries = totalEntries;
            }
        }


        #endregion

        #region Event Delegate

        public delegate void UpdaterDownloadProgressChangedEventHandler(object sender, UpdaterDownloadProgressChangedEventArgs e);
        public delegate void UpdaterExtractionProgressEventHandler(object sender, UpdaterExtractionProgressEventArgs e);
        #endregion

        #region Mousedown
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
        private void discord_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://discord.gg/6HansTZ");
        }
        private void site_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start("http://s4plus.ga");
        }
        private void exit_MouseDown(object sender, MouseEventArgs e)
        {
            Environment.Exit(0);
        }
        #endregion

        #region AutoPatching

        public int GetVersion() => this.S4Settings.Values.S4Version;
        public void Update()
        {
            this.currentOnlineS4Version = Convert.ToInt32(Extensions.SendRequest(Configuration.Online.OnlineVersionFile));


            if (this.S4Settings.Values.S4Version > this.currentOnlineS4Version)
            {

                Error("Invalid version (Fix : Delete settings.bin file)");
                return;
            }

            if (this.S4Settings.Values.S4Version < this.currentOnlineS4Version)
            {
                State(2);

                if (Directory.Exists(Configuration.Client.ShopDirectory))
                    Directory.Delete(Configuration.Client.ShopDirectory, true);
                if (!Directory.Exists(Configuration.Client.DownloadDirectory))
                    Directory.CreateDirectory(Configuration.Client.DownloadDirectory);

                totalUpdateIndex = currentOnlineS4Version - S4Settings.Values.S4Version;

                this.currentUpdateIndex++;
                this.currentUpdate = this.S4Settings.Values.S4Version + 1;

                client.DownloadFileCompleted += Client_DownloadFileCompleted;
                using (client = new WebClient())
                {
                    stopwatch.Start();
                    client.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                    client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                    update_label.Content = "Fetching " + Configuration.FileInfo.UpdateFileName + this.currentUpdate;
                    try
                    {

                        client.DownloadFileAsync(new Uri($"{Configuration.Online.OnlineDownloadFolder}{Configuration.FileInfo.UpdateFileName}{this.currentUpdate}{Configuration.FileInfo.UpdateFileExtension}"),
                                                        $"{Configuration.Client.DownloadDirectory}{Configuration.FileInfo.UpdateFileName}{this.currentUpdate}{Configuration.FileInfo.UpdateFileExtension}");
                    }
                    catch (Exception e)
                    {
                        Error(e.Message);
                        return;
                    }
                }
            }
            else
            {
                if (Directory.Exists(Configuration.Client.DownloadDirectory))
                    Directory.Delete(Configuration.Client.DownloadDirectory);
                FilesCheck.LoadResCheck();

            }
        }
        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            update_progress.Value = e.ProgressPercentage;
            update_label.Content = string.Format("Downloading {0} [{1}MBs/{2}MBs ({3}%)] FilesLeft: {4} ", Configuration.FileInfo.UpdateFileName + currentUpdate, (e.BytesReceived / 1024d / 1024d).ToString("0.00"), (e.TotalBytesToReceive / 1024d / 1024d).ToString("0.00"), e.ProgressPercentage.ToString(), totalUpdateIndex);

        }

        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            var failure = e.Error;

            if (failure == null)
            {
                using (ZipFile zip = ZipFile.Read($"{Configuration.Client.DownloadDirectory}{Configuration.FileInfo.UpdateFileName}{this.currentUpdate}{Configuration.FileInfo.UpdateFileExtension}"))
                {
                    for (int i = 0; i < zip.Count; i++)
                    {
                        update_label.Content = "Extracting " + zip[i].FileName;
                        zip[i].Extract($"{Configuration.Client.PlusDirectory}", ExtractExistingFileAction.OverwriteSilently);
                        OnUpdaterExtractionProgress(new UpdaterExtractionProgressEventArgs(i, zip[i].FileName, zip.Count - 1));
                    }
                }

                File.Delete($"{Configuration.Client.DownloadDirectory}{Configuration.FileInfo.UpdateFileName}{currentUpdate}{Configuration.FileInfo.UpdateFileExtension}");

                SetVersion(S4Settings.Values.S4Version + 1);

                client.DownloadProgressChanged -= new DownloadProgressChangedEventHandler(ProgressChanged);
                client.DownloadFileCompleted -= Client_DownloadFileCompleted;

                Update();
                stopwatch.Reset();
            }
            else
                update_label.Content = string.Format("Update Failed: {0}", failure.Message);
        }
        public void SetVersion(int version)
        {
            this.S4Settings.Values.S4Version = version;
            this.S4Settings.Save();
        }





        private void Client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            using (ZipFile zip = ZipFile.Read($"{Configuration.Client.DownloadDirectory}{Configuration.FileInfo.UpdateFileName}{this.currentUpdate}{Configuration.FileInfo.UpdateFileExtension}"))
            {
                for (int i = 0; i < zip.Count; i++)
                {
                    zip[i].Extract($"{Configuration.Client.PlusDirectory}", ExtractExistingFileAction.OverwriteSilently);
                    OnUpdaterExtractionProgress(new UpdaterExtractionProgressEventArgs(i, zip[i].FileName, zip.Count - 1));
                }
            }

            File.Delete($"{Configuration.Client.DownloadDirectory}{Configuration.FileInfo.UpdateFileName}{currentUpdate}{Configuration.FileInfo.UpdateFileExtension}");

            SetVersion(S4Settings.Values.S4Version + 1);

            client.DownloadProgressChanged -= new DownloadProgressChangedEventHandler(ProgressChanged);
            client.DownloadFileCompleted -= Client_DownloadFileCompleted;

            Update();
        }


        #endregion

        #region ClientLanguage & ClientColor & NoticeBox & Save/LoadAccount
        public string Language()
        {
            if (eng.IsChecked == true) { return "eng"; }
            else if (fre.IsChecked == true) { return "fre"; }
            else if (spa.IsChecked == true) { return "spa"; }
            else if (ger.IsChecked == true) { return "ger"; }
            else if (ita.IsChecked == true) { return "ita"; }
            else if (rus.IsChecked == true) { return "rus"; }
            else if (jap.IsChecked == true) { return "jap"; }
            else if (rus.IsChecked == true) { return "tur"; }
            return "eng";
        }

        public void UpdateNoticeBox()
        {
            WebClient webClient = new WebClient();
            string text = news1 = webClient.DownloadString(Constants.UIp + "content1.txt");
            if (!(news1 == ""))
            {
                Label newItem1 = new Label
                {
                    Foreground = Brushes.White,
                    Content = news1,
                    IsTabStop = false
                };
                NoticeBox.Items.Add(newItem1);
            }
            string text2 = news2 = webClient.DownloadString(Constants.UIp + "content2.txt");
            if (!(news2 == ""))
            {
                Separator newItem2 = new Separator();
                NoticeBox.Items.Add(newItem2);
                Label newItem3 = new Label
                {
                    Foreground = Brushes.White,
                    Content = news2,
                    IsTabStop = false
                };
                NoticeBox.Items.Add(newItem3);
            }
            string text3 = news3 = webClient.DownloadString(Constants.UIp + "content3.txt");
            if (!(news3 == ""))
            {
                Separator newItem4 = new Separator();
                NoticeBox.Items.Add(newItem4);
                Label newItem5 = new Label
                {
                    Foreground = Brushes.White,
                    Content = news3,
                    IsTabStop = false
                };
                NoticeBox.Items.Add(newItem5);
            }
            string text4 = news4 = webClient.DownloadString(Constants.UIp + "content4.txt");
            if (!(news4 == ""))
            {
                Separator newItem6 = new Separator();
                NoticeBox.Items.Add(newItem6);
                Label newItem7 = new Label
                {
                    Foreground = Brushes.White,
                    Content = news4,
                    IsTabStop = false
                };
                NoticeBox.Items.Add(newItem7);
            }
            string text5 = news5 = webClient.DownloadString(Constants.UIp + "content5.txt");
            if (!(news5 == ""))
            {
                Separator newItem8 = new Separator();
                NoticeBox.Items.Add(newItem8);
                Label newItem9 = new Label
                {
                    Foreground = Brushes.White,
                    Content = news5,
                    IsTabStop = false
                };
                NoticeBox.Items.Add(newItem9);
            }
            string text6 = news6 = webClient.DownloadString(Constants.UIp + "content6.txt");
            if (!(news6 == ""))
            {
                Separator newItem10 = new Separator();
                NoticeBox.Items.Add(newItem10);
                Label newItem11 = new Label
                {
                    Foreground = Brushes.White,
                    Content = news6,
                    IsTabStop = false
                };
                NoticeBox.Items.Add(newItem11);
            }
            string text7 = news7 = webClient.DownloadString(Constants.UIp + "content7.txt");
            if (!(news7 == ""))
            {
                Separator newItem11 = new Separator();
                NoticeBox.Items.Add(newItem11);
                Label newItem12 = new Label
                {
                    Foreground = Brushes.White,
                    Content = news7,
                    IsTabStop = false
                };
                NoticeBox.Items.Add(newItem12);
            }
            string text8 = news8 = webClient.DownloadString(Constants.UIp + "content8.txt");
            if (!(news8 == ""))
            {
                Separator newItem12 = new Separator();
                NoticeBox.Items.Add(newItem12);
                Label newItem13 = new Label
                {
                    Foreground = Brushes.White,
                    Content = news8,
                    IsTabStop = false
                };
                NoticeBox.Items.Add(newItem13);
            }
            string text9 = news9 = webClient.DownloadString(Constants.UIp + "content9.txt");
            if (!(news9 == ""))
            {
                Separator newItem13 = new Separator();
                NoticeBox.Items.Add(newItem13);
                Label newItem14 = new Label
                {
                    Foreground = Brushes.White,
                    Content = news9,
                    IsTabStop = false
                };
                NoticeBox.Items.Add(newItem14);
            }
            string text10 = news10 = webClient.DownloadString(Constants.UIp + "content10.txt");
            if (!(news10 == ""))
            {
                Separator newItem14 = new Separator();
                NoticeBox.Items.Add(newItem14);
                Label newItem15 = new Label
                {
                    Foreground = Brushes.White,
                    Content = news10,
                    IsTabStop = false
                };
                NoticeBox.Items.Add(newItem15);
            }

        }

        private void GetSetting()
        {
            if (!(File.Exists(Configuration.Client.AccountSettingFile))) { MessageBox.Show("account.bin file is missing , restart the launcher"); Environment.Exit(0); }

            if (!string.IsNullOrEmpty(AccountSettings.Values.Username) && !string.IsNullOrEmpty(AccountSettings.Values.Password) && !string.IsNullOrEmpty(AccountSettings.Values.Language))
            {
                // Color Option Not Implemented Yet
                /*   if (AccountSettings.Values.Color == "original") { Constants.LoginWindow.Coriginal.IsChecked = true; }
                   else if (AccountSettings.Values.Color == "original") { Constants.LoginWindow.Coriginal.IsChecked = true; }
                   else if (AccountSettings.Values.Color == "red") { Constants.LoginWindow.Coriginal.IsChecked = true; }
                   else if (AccountSettings.Values.Color == "gold") { Constants.LoginWindow.Coriginal.IsChecked = true; }
                   else if (AccountSettings.Values.Color == "blue") { Constants.LoginWindow.Coriginal.IsChecked = true; }
                   else if (AccountSettings.Values.Color == "violet") { Constants.LoginWindow.Coriginal.IsChecked = true; }*/

                if (AccountSettings.Values.Language == "eng") { Constants.LoginWindow.eng.IsChecked = true; }
                else if (AccountSettings.Values.Language == "fre") { Constants.LoginWindow.fre.IsChecked = true; }
                else if (AccountSettings.Values.Language == "ger") { Constants.LoginWindow.ger.IsChecked = true; }
                else if (AccountSettings.Values.Language == "ita") { Constants.LoginWindow.ita.IsChecked = true; }
                else if (AccountSettings.Values.Language == "jap") { Constants.LoginWindow.jap.IsChecked = true; }
                else if (AccountSettings.Values.Language == "rus") { Constants.LoginWindow.rus.IsChecked = true; }
                else if (AccountSettings.Values.Language == "spa") { Constants.LoginWindow.spa.IsChecked = true; }
                else if (AccountSettings.Values.Language == "tur") { Constants.LoginWindow.tur.IsChecked = true; }
                login_username.Text = AccountSettings.Values.Username;
                login_passwd.Password = AccountSettings.Values.Password;
                Constants.LoginWindow.savebox.IsChecked = true;
            }
            else
            {
                Constants.LoginWindow.savebox.IsChecked = false;
            }
        }

        private void SaveSetting()
        {
            if (savebox.IsChecked == true)
            {
                // Color Option Not Implemented Yet
                /*if (Coriginal.IsChecked == true) { AccountSettings.Values.Color = "original"; }
                else if (Cred.IsChecked == true) { AccountSettings.Values.Color = "red"; }
                else if (Cgold.IsChecked == true) { AccountSettings.Values.Color = "gold"; }
                else if (Cblue.IsChecked == true) { AccountSettings.Values.Color = "blue"; }
                else if (Cviolet.IsChecked == true) { AccountSettings.Values.Color = "violet"; }*/

                if (eng.IsChecked == true) { AccountSettings.Values.Language = "eng"; }
                else if (tur.IsChecked == true) { AccountSettings.Values.Language = "tur"; }
                else if (spa.IsChecked == true) { AccountSettings.Values.Language = "spa"; }
                else if (ger.IsChecked == true) { AccountSettings.Values.Language = "ger"; }
                else if (ita.IsChecked == true) { AccountSettings.Values.Language = "ita"; }
                else if (fre.IsChecked == true) { AccountSettings.Values.Language = "fre"; }
                else if (jap.IsChecked == true) { AccountSettings.Values.Language = "jap"; }
                else if (rus.IsChecked == true) { AccountSettings.Values.Language = "rus"; }
                AccountSettings.Values.Username = login_username.Text;
                AccountSettings.Values.Password = login_passwd.Password;
                AccountSettings.Save();
            }
            else
            {
                File.Delete(Configuration.Client.AccountSettingFile);
            }

        }
        /* Client Color - Not Implemented Yet
       
      private void Clientcolor()
      {
          if (Cblue.IsChecked == true)
          {
              Clientcolor("blue");
          }
          else if (Cgold.IsChecked == true)
          {
              Clientcolor("gold");
          }
          else if (Cviolet.IsChecked == true)
          {
              Clientcolor("violet");
          }
          else if (Cred.IsChecked == true)
          {
              Clientcolor("red");
          }

      }

      private void Clientcolor(string color)
      {
          if (!File.Exists(@"_resources/resource_" + color + ".s4hd"))
          {
              MessageBox.Show("There is a missing resource file. Reinstall the game!");
              Environment.Exit(0);
          }
          else
          {
              File.Delete(@"resource.s4hd");
              File.Copy(@"_resources/resource_" + color + ".s4hd", @"resource.s4hd");
          }
      }*/
        // Old Save Account
        /*     public void SaveAccount()
             {
                 if (!File.Exists("account.txt"))
                 {
                     File.Create("account.txt").Dispose();
                     using (TextWriter tw = new StreamWriter("account.txt"))
                     {
                         if (Convert.ToBoolean(savebox.IsChecked))
                         {
                             tw.WriteLine("2");
                             tw.WriteLine(login_username.Text);
                             tw.WriteLine(login_passwd.Password);
                             if (Coriginal.IsChecked == true) { tw.WriteLine("original"); }
                             else if (Cred.IsChecked == true) { tw.WriteLine("red"); }
                             else if (Cgold.IsChecked == true) { tw.WriteLine("gold"); }
                             else if (Cblue.IsChecked == true) { tw.WriteLine("blue"); }
                             else if (Cviolet.IsChecked == true) { tw.WriteLine("violet"); }
                             if (eng.IsChecked == true) { tw.WriteLine("eng"); }
                             else if (tur.IsChecked == true) { tw.WriteLine("tur"); }
                             else if (spa.IsChecked == true) { tw.WriteLine("spa"); }
                             else if (ger.IsChecked == true) { tw.WriteLine("ger"); }
                             else if (ita.IsChecked == true) { tw.WriteLine("ita"); }
                             else if (fre.IsChecked == true) { tw.WriteLine("fre"); }
                             else if (jap.IsChecked == true) { tw.WriteLine("jap"); }
                             else if (rus.IsChecked == true) { tw.WriteLine("rus"); }

                         }
                         else
                         {
                             tw.Write("1");
                         }
                         tw.Close();
                     }

                 }
                 else
                 {
                     StreamWriter tw = new StreamWriter("account.txt", append: false);

                     if (Convert.ToBoolean(savebox.IsChecked))
                     {
                         tw.WriteLine("2");
                         tw.WriteLine(login_username.Text);
                         tw.WriteLine(login_passwd.Password);
                         if (Coriginal.IsChecked == true) { tw.WriteLine("original"); }
                         else if (Cred.IsChecked == true) { tw.WriteLine("red"); }
                         else if (Cgold.IsChecked == true) { tw.WriteLine("gold"); }
                         else if (Cblue.IsChecked == true) { tw.WriteLine("blue"); }
                         else if (Cviolet.IsChecked == true) { tw.WriteLine("violet"); }
                         if (eng.IsChecked == true) { tw.WriteLine("eng"); }
                         else if (tur.IsChecked == true) { tw.WriteLine("tur"); }
                         else if (spa.IsChecked == true) { tw.WriteLine("spa"); }
                         else if (ger.IsChecked == true) { tw.WriteLine("ger"); }
                         else if (ita.IsChecked == true) { tw.WriteLine("ita"); }
                         else if (fre.IsChecked == true) { tw.WriteLine("fre"); }
                         else if (jap.IsChecked == true) { tw.WriteLine("jap"); }
                         else if (rus.IsChecked == true) { tw.WriteLine("rus"); }
                     }
                     else
                     {
                         tw.Write("1");
                     }
                     tw.Close();
                 }

             }*/
        #endregion


        public MainWindow()
        {
            if (IsAttached)
            {
                Environment.Exit(0);
            }
            else
            {
                Constants.LoginWindow = this;
                Checks.Networkcheck();                                                 //Checks If Network is available By Pinging Google
                Checks.IsS4Running();                                                 //Checks Whether S4Client is running to stop MultiClient
                InitializeComponent();
                string resource1 = "S4Launcher.Ionic.Zip.dll";
                EmbeddedAssembly.Load(resource1, "Ionic.Zip.dll");
                AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
                stopwatch = new Stopwatch();
                client = new WebClient();
                AccountSettings = AccountSetting.CurrentInstance();
                this.currentOnlineS4Version = 0;
                this.currentUpdateIndex = 0;
                this.totalUpdateIndex = 0;
                this.currentUpdate = 0;
            }

        }
        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return EmbeddedAssembly.Get(args.Name);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            new Thread(delegate () { Dispatcher.Invoke(() => { MainCheck(); }); }).Start();
        }
        public void UpdateLabel(string message)
        {
            Dispatcher.Invoke(() => { result_label.Content = message; });

        }
        public string GetUsername()
        {
            return Dispatcher.Invoke(() => { return login_username.Text; });
        }
        public string GetPassword()
        {
            return Dispatcher.Invoke(() => { return login_passwd.Password; });
        }
        public void Error(string message)
        {
            MessageBox.Show(message);
            Environment.Exit(0);
        }
        public void LoginError(string error)
        {
            Dispatcher.Invoke(() =>
            {

                login_passwd.IsEnabled = false;
                login_username.IsEnabled = false;
                btn_Login.IsEnabled = false;
                btn_Login.Visibility = Visibility.Hidden;
                result_label.Content = error;
                start_label.Visibility = Visibility.Hidden;

            });
        }
        public void State(int Type)
        {
            if (Type == 0)
            {
                btn_Login.IsEnabled = false;
                btn_Login.Visibility = Visibility.Hidden;
                login_passwd.IsEnabled = false;
                login_username.IsEnabled = false;
                start_label.Visibility = Visibility.Visible;
                start_label.Content = "Checking";
            }

            else if (Type == 1)
            {
                btn_Login.IsEnabled = true;
                btn_Login.Visibility = Visibility.Visible;
                login_passwd.IsEnabled = true;
                login_username.IsEnabled = true;
                start_label.Visibility = Visibility.Hidden;
                result_label.Content = "You Can Login";
                update_label.Content = "Updates Sucessfully Completed";
                version_label.Content = S4Settings.Values.S4Version;
                update_progress.Value = 100;
                BState = 1;
            }
            else if (Type == 2)
            {
                btn_Login.IsEnabled = false;
                btn_Login.Visibility = Visibility.Hidden;
                login_passwd.IsEnabled = false;
                login_username.IsEnabled = false;
                start_label.Visibility = Visibility.Visible;
                start_label.Content = "Updating";
            }
        }

        public void MainCheck()
        {
            State(0);
            Checks.Serverstat();                                                //Checks If Server Is Online By Checking Ports
            Checks.OnlinePlayers();                                            //Gets Number Of Online Players From An Online Txt
            GetSetting();                                                     //This Loads Saved Account Details
            UpdateNoticeBox();                                               //Gets NoticeBox Items From The Update Server
            SUpdate.Update();                                               //Updates S4Patcher
            //Remember that Update Function is run through selfupdate and file check is done through update function (after update finishes)
        }
        private void Login_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (BState == 1)
            {
                if (login_passwd.Password.Length > 4 || login_username.Text.Length > 4)
                {
                    if (new Regex("^[a-zA-Z0-9.*_-]*$").IsMatch(login_username.Text))
                    {
                        State(0);
                        FilesCheck.LoginResCheck();
                        LoginClient.Connect(Constants.ConnectEndPoint);
                    }
                    else
                    {
                        result_label.Content = "Username Contains Invalid Characters";
                    }
                }
                else
                {
                    result_label.Content = "Id or Pw is less than 4 characters";
                }

            }
        }
        public void Start(string code)
        {
            Dispatcher.Invoke(() =>
            {
                if (Checks.Clientcheck())
                {
                    try
                    {
                        // Clientcolor(); Color Option - Not Implemented Yet
                        SaveSetting();
                        FilesCheck.LoginResCheck();
                        Process.Start("s4client_be.exe", string.Format("-rc:eu -lac:{2} -auth_server_ip:{0} -aeria_acc_code:{1}", (object)Constants.ConnectEndPoint.Address, (object)code, Language()));
                        Environment.Exit(0);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Failed to run S4Client.exe", "Error");
                        Environment.Exit(0);
                    }

                }
                else
                {
                    Error("Invalid S4Client");
                    Environment.Exit(0);
                }
            });
        }

    }

}