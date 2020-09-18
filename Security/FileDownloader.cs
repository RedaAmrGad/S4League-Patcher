using Ionic.Zip;
using S4Launcher.Update;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
namespace S4Launcher.Security
{
    class FileDownloader
    {
            private static Stopwatch stopWatch = new Stopwatch();
            static string file;
        public async void Download()
        {
            if (Constants.ModifiedFiles.Count > 0)
            {

                foreach (string filename in Constants.ModifiedFiles)
                {
                    file = filename;
                    WebClient webClient = new WebClient();
                    webClient.DownloadProgressChanged += webClient_DownloadProgressChanged;
                    webClient.DownloadFileCompleted += webClient_DownloadFileCompleted;
                    webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                    stopWatch.Start();
                    Constants.DownloadedFiles.Add(filename);
                    await webClient.DownloadFileTaskAsync(new Uri(Constants.UIp + "client/" + filename), filename);
                }
            }
            else
            {
                Constants.LoginWindow.State(1);
            }

           


        }

        private static void webClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Constants.LoginWindow.update_progress.Value = e.ProgressPercentage;
            Constants.LoginWindow.update_label.Content = string.Format("Downloading {3} [{0}MBs/{1}MBs ({2}%)] FilesLeft: {4}", (e.BytesReceived / 1024d / 1024d).ToString("0.00"), (e.TotalBytesToReceive / 1024d / 1024d).ToString("0.00"), e.ProgressPercentage.ToString() , file , Constants.ModifiedFiles.Count - Constants.DownloadedFiles.Count);

        }

        private static void webClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            stopWatch.Reset();
        }
        private static void Completed(object sender, AsyncCompletedEventArgs e)
        {
            var failure = e.Error;

            if (failure == null)
            {
                if (file.Contains(".zip"))
                {
                    using (ZipFile zip = ZipFile.Read(file))
                    {
                        for (int i = 0; i < zip.Count; i++)
                        {
                            Constants.LoginWindow.update_label.Content = "Extracting " + zip[i].FileName;
                            zip[i].Extract($"{Configuration.Client.PlusDirectory}", ExtractExistingFileAction.OverwriteSilently);
                        }
                    }

                    File.Delete(file);
                    if (file.Contains("S4Patcher"))
                    {
                        Process.Start(@"S4Patcher.exe");
                        Environment.Exit(0);
                    }
                }
                if (Constants.DownloadedFiles.Count >= Constants.ModifiedFiles.Count)
                {
                    Constants.LoginWindow.State(1);
                }
            }
            else
                Constants.LoginWindow.update_label.Content = string.Format("Update Failed: {0}", failure.Message);
        }

    }
}
