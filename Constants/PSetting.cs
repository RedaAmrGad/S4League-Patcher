using System.IO;
using S4Launcher.Update;

namespace S4Launcher
{
	internal class AccountSetting
	{
		private static AccountSetting instance;

		public AccountSettingDto Values{ get; set; }

		private AccountSetting()
		{
			if (File.Exists(Configuration.Client.AccountSettingFile))
			{
				Values = AccountSettingDto.Deserialize(Configuration.Client.AccountSettingFile);
				return;
			}
			Values = new AccountSettingDto();
            AccountSettingDto.Serialize(Configuration.Client.AccountSettingFile, Values);
		}

		public static AccountSetting CurrentInstance()
		{
			if (instance == null)
			{
				instance = new AccountSetting();
			}
			return instance;
		}

		public void Save()
		{
			File.Delete(Configuration.Client.AccountSettingFile);
            AccountSettingDto.Serialize(Configuration.Client.AccountSettingFile, Values);
		}
	}
}
