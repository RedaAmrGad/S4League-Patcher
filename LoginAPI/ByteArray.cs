using System;
using System.Reflection;
using System.Text;

namespace S4Launcher.LoginAPI
{
  internal class ByteArray
  {
    private byte[] _buffer = new byte[0];
    private int _readOffset;
    private int _writeOffset;

    public byte[] GetBuffer() => (byte[])this._buffer;

    public int ReadOffset => _readOffset;
    public int WriteOffset => _writeOffset;

    public ByteArray() { }
    public ByteArray(ByteArray data)
    {
      _buffer = data._buffer;
    }
    public ByteArray(byte[] data)
    {
      _buffer = (byte[])data.Clone();
    }
    public ByteArray(byte[] data, int lenght)
    {
      _buffer = new byte[lenght];
      Array.Copy(data, _buffer, lenght);
    }

    internal void Write(byte[] obj)
    {
      if(_buffer.Length <= _writeOffset + obj.Length)
        Array.Resize(ref _buffer, _writeOffset + obj.Length);
      Array.Copy(obj, 0, _buffer, _writeOffset, obj.Length);
      _writeOffset = _writeOffset + obj.Length;
    }

    internal void Write(bool obj)
    {
      if(obj)
        Write((byte)1);
      else
        Write((byte)0);
    }

    internal void Write(byte obj)
    {
      var data = new byte[1];
      data[0] = obj;
      Write(data);
    }

    internal void Write(short obj)
    {
      Write(BitConverter.GetBytes(obj));
    }

    internal void Write(int obj)
    {
      Write(BitConverter.GetBytes(obj));
    }

    internal void Write(long obj)
    {
      Write(BitConverter.GetBytes(obj));
    }

    internal void Write(ByteArray obj)
    {
      WriteScalar(obj._writeOffset);
      Write(obj._buffer);
    }

    internal void WriteScalar(long obj)
    {
      if(obj <= sbyte.MaxValue)
      {
        Write((byte)1);
        Write((byte)obj);
      }
      else if(obj <= short.MaxValue)
      {
        Write((byte)2);
        Write((byte)obj);
      }
      else if(obj <= int.MaxValue)
      {
        Write((byte)4);
        Write((byte)obj);
      }
      else
      {
        Write((byte)8);
        Write((byte)obj);
      }
    }

    internal bool Read(ref byte[] obj, int lenght)
    {
      if(_buffer.Length >= _readOffset + lenght)
      {
        var data = new byte[lenght];
        Array.Copy(_buffer, _readOffset, data, 0, lenght);
        obj = data;
        _readOffset = _readOffset + lenght;
        return true;
      }
      
      return false;
    }

    internal bool Read(ref ByteArray obj)
    {
      long lenght = 0;
      if(ReadScalar(ref lenght))
      {
        var data = new byte[lenght];
        if(Read(ref data, data.Length))
        {
          obj = new ByteArray(data);
          return true;
        }
      }
      
      return false;
    }

    internal bool Read(ref bool obj)
    {
      byte a = 0;
      var retval = Read(ref a);
      obj = a == 1;

      return retval;
    }

    internal bool Read(ref byte obj)
    {
      if(_buffer.Length >= _readOffset)
      {
        obj = _buffer[_readOffset];
        _readOffset = _readOffset + 1;

        return true;
      }

      return false;
    }

    internal bool Read(ref short obj)
    {
      var data = new byte[2];
      if(Read(ref data[0])
        && Read(ref data[1]))
      {
        obj = BitConverter.ToInt16(data, 0);
        return true;
      }

      return false;
    }

    internal bool Read(ref int obj)
    {
      var data = new byte[4];
      if (Read(ref data[0])
          && Read(ref data[1])
          && Read(ref data[2])
          && Read(ref data[3]))
      {
        obj = BitConverter.ToInt32(data, 0);
        return true;
      }
      return false;
    }

    internal bool Read(ref long obj)
    {
      var data = new byte[8];
      if (Read(ref data[0])
          && Read(ref data[1])
          && Read(ref data[2])
          && Read(ref data[3])
          && Read(ref data[4])
          && Read(ref data[5])
          && Read(ref data[6])
          && Read(ref data[7]))
      {
        obj = BitConverter.ToInt64(data, 0);
        return true;
      }
      return false;
    }

    internal bool ReadScalar(ref long obj)
    {
      byte a = 0;
      short b = 0;
      var c = 0;
      long d = 0;

      byte pending = 0;
      if(!Read(ref pending))
        return false;
      switch(pending)
      {
        case 8:
          if(!Read(ref d))
            return false;
          obj = d;
          break;

        case 4:
          if (!Read(ref c))
            return false;
          obj = c;
          break;

        case 2:
          if (!Read(ref b))
            return false;
          obj = b;
          break;

        case 1:
          if (!Read(ref a))
            return false;
          obj = a;
          break;
          default:
          return false;
      }

      return true;
    }
  }
}
