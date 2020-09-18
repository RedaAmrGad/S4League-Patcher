using System;
using System.Management;
using System.Security.Cryptography;
using System.Security;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace S4Launcher.Security
{
  internal static class ThumbPrint
  {
    internal static string FingerPrint = string.Empty;
    private static string _vCpuId = "";
    private static string _vBiosId = "";
    private static string _vBaseId = "";

    internal static string Value()
    {
      if (!string.IsNullOrEmpty(FingerPrint))
        return FingerPrint;

      var actions = new List<Action>
            {
                () => _vCpuId = CpuId(),
                () => _vBiosId = BiosId(),
                () => _vBaseId = BaseId()
            };

      Parallel.Invoke(actions.ToArray());
      _classes.Clear();
      actions.Clear();

      FingerPrint = GetHash("CPU >> " + _vCpuId + "\nBIOS >> " + _vBiosId + "\nBASE >> " + _vBaseId);
      return FingerPrint;
    }

    private static string GetHash(string s)
    {
      var sec = new MD5CryptoServiceProvider();
      var enc = new ASCIIEncoding();
      var bt = enc.GetBytes(s);
      return GetHexString(sec.ComputeHash(bt));
    }

    private static string GetHexString(IReadOnlyList<byte> bt)
    {
      var s = string.Empty;
      for (var i = 0; i < bt.Count; i++)
      {
        var b = bt[i];
        int n = b;
        var n1 = n & 15;
        var n2 = (n >> 4) & 15;
        if (n2 > 9)
          s += ((char)(n2 - 10 + 'A')).ToString();
        else
          s += n2.ToString();
        if (n1 > 9)
          s += ((char)(n1 - 10 + 'A')).ToString();
        else
          s += n1.ToString();
        if (i + 1 != bt.Count && (i + 1) % 2 == 0) s += "-";
      }

      return s;
    }

    #region Original Device ID Getting Code

    private static ConcurrentDictionary<string, ManagementObjectCollection> _classes =
        new ConcurrentDictionary<string, ManagementObjectCollection>();

    //Return a hardware identifier
    private static string Identifier(string wmiClass, string wmiProperty)
    {
      if (!_classes.TryGetValue(wmiClass, out var moc))
      {
        var mc = new ManagementClass(wmiClass);
        moc = mc.GetInstances();
        _classes.TryAdd(wmiClass, moc);
      }

      foreach (var mo in moc)
      {
        var x = mo.GetPropertyValue(wmiProperty);
        if (x == null)
          continue;

        return mo[wmiProperty].ToString();
      }

      return "";
    }

    private static string CpuId()
    {
      var retVal = Identifier("Win32_Processor", "UniqueId");
      if (retVal != "")
        return retVal;

      retVal = Identifier("Win32_Processor", "ProcessorId");
      if (retVal != "")
        return retVal;

      retVal = Identifier("Win32_Processor", "Name");
      return retVal == "" ?
          "NULL" :
          Identifier("Win32_Processor", "Manufacturer");
    }

    //BIOS Identifier
    private static string BiosId()
    {
      return Identifier("Win32_BIOS", "Manufacturer")
             + Identifier("Win32_BIOS", "IdentificationCode")
             + Identifier("Win32_BIOS", "SerialNumber");
    }

    //Motherboard ID
    private static string BaseId()
    {
      return Identifier("Win32_BaseBoard", "Model")
             + Identifier("Win32_BaseBoard", "Manufacturer")
             + Identifier("Win32_BaseBoard", "Name")
             + Identifier("Win32_BaseBoard", "SerialNumber");
    }

    #endregion
  }
}
