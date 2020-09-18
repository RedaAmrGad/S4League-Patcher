using System;
using System.IO;
namespace S4Launcher.Update
{
  internal static class Configuration
  {
    internal struct Client
    {
      public static string PlusDirectory {get; } = Environment.CurrentDirectory;
      public static string DownloadDirectory  {get; } = Path.Combine(PlusDirectory, "updates\\" );
      public static string ShopDirectory      {get; } = Path.Combine(PlusDirectory, "shop\\"     );

      public static string S4Client     {get; } = Path.Combine(PlusDirectory, "S4Client.exe");
      public static string SettingsFile {get; } = Path.Combine(PlusDirectory, "settings.bin");
      public static string AccountSettingFile { get; } = Path.Combine(PlusDirectory, "account.bin");
        }

    internal struct FileInfo
    {
      public static string UpdateFileName       {get; }  = "S4Plus_";
      public static string UpdateFileExtension  {get; }  = ".zip";
    }

    internal struct Online
    {
      public static string OnlineDownloadFolder { get; } = Constants.UIp+"update/files/";
      public static string OnlineVersionFile    { get; } = Constants.UIp +"update/version.txt";
    }
  }
}
