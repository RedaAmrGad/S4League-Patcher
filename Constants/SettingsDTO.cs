using System;
using System.IO;
using System.Net;

namespace S4Launcher.Update
{
  [Serializable]
  internal class SettingsDTO
  {
    public int S4Version {get; set; }

    public SettingsDTO()
    {
      this.S4Version  = 0;
   
    }

    public SettingsDTO(int s4Version)
    {
      this.S4Version  = s4Version;
    }

    public static void Serialize(string path, SettingsDTO dto)
    {
      using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
      using (BinaryWriter bw = new BinaryWriter(fs))
      {
        bw.Write(dto.S4Version);
      }
    }

    public static SettingsDTO Deserialize(string path)
    {
      using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
      using (BinaryReader br = new BinaryReader(fs))
      {
        SettingsDTO dto = new SettingsDTO()
        {
          S4Version = br.ReadInt32(),
        };

        return dto;
      }
    }
  }
}
