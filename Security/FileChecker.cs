using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using S4Launcher;
namespace S4Launcher.Security
{
    class FilesCheck
    {
        //This Checks at Patcher Start and at Patcher Login
        //Checks for x3.xem , resource.s4hd , some resources

        public static FileDownloader _downloader = new FileDownloader();
        public static void GetText(Texts.Keys Key, params object[] Arguments)
        {
            foreach (KeyValuePair<Texts.Keys, string> item in Texts.Text)
            {
                if (item.Key == Key)
                {
                    MessageBox.Show(string.Format(item.Value, Arguments));

                }
            }
        }
        
        public static void LoadResCheck()
        {
            Constants.LoginWindow.State(0);
            String line;
            String Hash;
            const Int32 BufferSize = 128;
            if (!Directory.Exists("_resources"))
            {
                GetText(Texts.Keys.WRONGFOLDER, "");
                Environment.Exit(0);
            }
            if (!Directory.Exists("BattlEye"))
            {
                Directory.CreateDirectory("BattlEye");
            }
            WebRequest request = WebRequest.Create(Constants.UIp + "reshash.txt");
            WebResponse response = request.GetResponse();
            Stream hashes = response.GetResponseStream();
            
            using (var streamReader = new StreamReader(hashes, Encoding.UTF8, true, BufferSize))
            {

                while ((line = streamReader.ReadLine()) != null)
                {
                    if (line != "stop")
                    {
                        string filename = line.Substring(0, line.IndexOf(" "));
                       
                        Hash = line.Substring(line.IndexOf(" ") + 1);
                        if (File.Exists(filename))
                        {
                            if (!(Hash == BytesToString(GetFileHash(filename))))
                            {
                                Constants.ModifiedFiles.Add(filename);
                                File.Delete(filename);
                            }

                        }

                        else
                        {
                            Constants.ModifiedFiles.Add(filename);
                        }
                    }
                }
                _downloader.Download();
            }
        }
        
        public static void LoginResCheck()
        {
            Constants.LoginWindow.State(0);
            String line;
            String Hash;
            const Int32 BufferSize = 128;
            WebRequest request = WebRequest.Create(Constants.UIp + "reshash.txt");
            WebResponse response = request.GetResponse();
            Stream hashes = response.GetResponseStream();
            using (var streamReader = new StreamReader(hashes, Encoding.UTF8, true, BufferSize))
            {

                while ((line = streamReader.ReadLine()) != null)
                {
                    if (line != "stop")
                    {
                        string filename = line.Substring(0, line.IndexOf(" "));
                        Hash = line.Substring(line.IndexOf(" ") + 1);
                        if (File.Exists(filename))
                        {
                            if (!(Hash == BytesToString(GetFileHash(filename))))
                            {
                                GetText(Texts.Keys.MODIFIEDBINARY, filename);
                                Environment.Exit(0);
                            }
                        }

                        else
                        {
                            GetText(Texts.Keys.MISSINGBINARY, filename);
                            Environment.Exit(0);

                        }
                    }
                }

            }


        }
        public static string BytesToString(byte[] bytes)
        {
            string result = "";
            foreach (byte b in bytes) result += b.ToString("x2");
            return result;
        }
        public static byte[] GetFileHash(string fileName)
        {
            SHA256 Sha256 = SHA256.Create();
            using (var stream = new BufferedStream(File.OpenRead(fileName), 1200000))
            {
                return Sha256.ComputeHash(stream);

            }

        }
       
    }

}