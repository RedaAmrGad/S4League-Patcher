using System;
using System.IO;
using System.Net;

namespace S4Launcher.Update
{
  internal static class Extensions
  {
    public static T ReadEnum<T>(object value) where T : new()
    {
      string val = Convert.ToString(value);
      if (val != null && val != "")
        return (T)Enum.Parse(typeof(T), Convert.ToString(value));
      else
        return new T();
    }

    internal static string SendRequest(string url)
    {
      WebRequest request = (WebRequest)WebRequest.Create(url);
      request.Proxy = null;

      WebResponse response = request.GetResponse();
      Stream stream = response.GetResponseStream();
      using (StreamReader reader = new StreamReader(stream))
        return reader.ReadToEnd();
    }
  }
}
