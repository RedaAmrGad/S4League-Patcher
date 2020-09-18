using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S4Launcher.LoginAPI
{
  internal class EMessage : ByteArray
  {
    public byte[] Buffer => GetBuffer();
    public int Length => Buffer.Length;

    public EMessage() { }
    public EMessage(ByteArray packet) : base(packet) { }
    public EMessage(byte[] data, int lenght) : base(data, lenght) { }

    internal void Write(MessageType obj)
    {
      Write((byte)obj);
    }

    internal void Write(EMessage obj)
    {
      Write(obj.Buffer);
    }

    internal void Write(string obj)
    {
      Write((byte)1);
      WriteScalar(obj.Length);
      Write(Encoding.ASCII.GetBytes(Encoding.ASCII.GetString(Encoding.UTF8.GetBytes(obj))));
    }

    internal bool Read(ref string obj)
    {
      long lenght = 0;
      byte type = 0;
      if (Read(ref type)
        && ReadScalar(ref lenght))
      {
        var binaryText = new byte[lenght];
        if (Read(ref binaryText, (int)lenght))
          switch (type)
          {
            case 1:
              obj = Encoding.ASCII.GetString(binaryText);
              return true;
            case 2:
              obj = Encoding.Unicode.GetString(binaryText);
              return true;
            default:
              return false;
          }
      }

      return false;
    }

    internal bool Read(ref MessageType obj)
    {
      byte a = 0;
      if (!Read(ref a))
        return false;
      obj = (MessageType)a;
      return true;
    }

    internal enum MessageType
    {
      Ignore,
      Rmi,
      Encrypted,
      Notify
    }
  }
}
