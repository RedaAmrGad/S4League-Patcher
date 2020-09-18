using System;
using System.IO;

namespace S4Launcher.Update
{
  internal class Settings
  {
    static Settings instance;
    
    public SettingsDTO Values {get; set; }

    private Settings()
    {
      if(File.Exists(Configuration.Client.SettingsFile))
        Values = SettingsDTO.Deserialize(Configuration.Client.SettingsFile);
      else
      {
        Values = new SettingsDTO();
        SettingsDTO.Serialize(Configuration.Client.SettingsFile, Values);
      }
    }

    public static Settings CurrentInstance()
    {
      if(instance == null)
        instance = new Settings();

      return instance;
    }

    public void Save()
    {
      File.Delete(Configuration.Client.SettingsFile);
      SettingsDTO.Serialize(Configuration.Client.SettingsFile, this.Values);
    }
  }
}
