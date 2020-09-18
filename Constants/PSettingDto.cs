using NetspherePatcher.security;
using System.IO;

namespace S4Launcher
{
	internal class AccountSettingDto
    {
		private static string _s_password = "YouWillNeverBreakThisCauseItIsS4PlusProtection";

		private string _username;
		private string _password;
       // private string _color;
        private string _language;
		public string Username
		{
			get
			{
				return _username;
			}
			set
			{
				_username = value;
			}
		}

		public string Password
		{
			get
			{
				if (_password == "")
				{
					return string.Empty;
				}
				return Account.Decrypt(_password, _s_password);
			}
			set
			{
				if (value == "")
				{
					_password = string.Empty;
				}
				_password = Account.Encrypt(value, _s_password);
			}
		}

      /*  public string Color
        {
            get
            {
                return _color;
            }
            set
            {
                _color = value;
            }
        }*/

        public string Language
        {
            get
            {
                return _language;
            }
            set
            {
                _language = value;
            }
        }

        public AccountSettingDto()
		{
			_username = "";
			_password = "";
           // _color = "";
            _language = "";
		}

		public AccountSettingDto(string username, string password, /*string color ,*/ string language)
		{
			_username = username;
			_password = password;
          //  _color = color;
            _language = language;

		}

		public static void Serialize(string path, AccountSettingDto dto)
		{
			using (FileStream output = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
			{
				using (BinaryWriter binaryWriter = new BinaryWriter(output))
				{
					binaryWriter.Write(dto._username);
					binaryWriter.Write(dto._password);
                 //   binaryWriter.Write(dto._color);
                    binaryWriter.Write(dto._language);
                }
			}
		}

		public static AccountSettingDto Deserialize(string path)
		{
			using (FileStream input = new FileStream(path, FileMode.Open, FileAccess.Read))
			{
				using (BinaryReader binaryReader = new BinaryReader(input))
				{
					return new AccountSettingDto
                    {
						_username = binaryReader.ReadString(),
						_password = binaryReader.ReadString(),
                       // _color = binaryReader.ReadString(),
                        _language = binaryReader.ReadString()

                    };
				}
			}
		}
	}
}
