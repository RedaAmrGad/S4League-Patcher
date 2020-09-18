using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace NetspherePatcher.security
{
	public static class Account
    {
		private static byte[] _salt = new byte[8]
		{
			175,
			49,
			80,
			127,
			22,
			46,
			40,
			113
		};

		public static string Encrypt(string plainText, string password)
		{
			if (plainText == null)
			{
				return null;
			}
			if (password == null)
			{
				password = string.Empty;
			}
			byte[] bytes = Encoding.UTF8.GetBytes(plainText);
			byte[] bytes2 = Encoding.UTF8.GetBytes(password);
			bytes2 = SHA256.Create().ComputeHash(bytes2);
			byte[] inArray = Encrypt(bytes, bytes2);
			return Convert.ToBase64String(inArray);
		}

		public static string Decrypt(string encryptedText, string password)
		{
			if (encryptedText == null)
			{
				return null;
			}
			if (password == null)
			{
				password = string.Empty;
			}
			byte[] bytesToBeDecrypted = Convert.FromBase64String(encryptedText);
			byte[] bytes = Encoding.UTF8.GetBytes(password);
			bytes = SHA256.Create().ComputeHash(bytes);
			byte[] bytes2 = Decrypt(bytesToBeDecrypted, bytes);
			return Encoding.UTF8.GetString(bytes2);
		}

		private static byte[] Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
		{
			byte[] result = null;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				using (RijndaelManaged rijndaelManaged = new RijndaelManaged())
				{
					Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(passwordBytes, _salt, 1000);
					rijndaelManaged.KeySize = 256;
					rijndaelManaged.BlockSize = 128;
					rijndaelManaged.Key = rfc2898DeriveBytes.GetBytes(rijndaelManaged.KeySize / 8);
					rijndaelManaged.IV = rfc2898DeriveBytes.GetBytes(rijndaelManaged.BlockSize / 8);
					rijndaelManaged.Mode = CipherMode.CBC;
					using (CryptoStream cryptoStream = new CryptoStream(memoryStream, rijndaelManaged.CreateEncryptor(), CryptoStreamMode.Write))
					{
						cryptoStream.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
						cryptoStream.Close();
					}
					result = memoryStream.ToArray();
				}
			}
			return result;
		}

		private static byte[] Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
		{
			byte[] result = null;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				using (RijndaelManaged rijndaelManaged = new RijndaelManaged())
				{
					Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(passwordBytes, _salt, 1000);
					rijndaelManaged.KeySize = 256;
					rijndaelManaged.BlockSize = 128;
					rijndaelManaged.Key = rfc2898DeriveBytes.GetBytes(rijndaelManaged.KeySize / 8);
					rijndaelManaged.IV = rfc2898DeriveBytes.GetBytes(rijndaelManaged.BlockSize / 8);
					rijndaelManaged.Mode = CipherMode.CBC;
					using (CryptoStream cryptoStream = new CryptoStream(memoryStream, rijndaelManaged.CreateDecryptor(), CryptoStreamMode.Write))
					{
						cryptoStream.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
						cryptoStream.Close();
					}
					result = memoryStream.ToArray();
				}
			}
			return result;
		}
	}
}
