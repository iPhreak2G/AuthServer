using System;
using System.IO;

namespace Helpers
{
	public static class ClientHelper
	{
		public static void AddClient(string Username, string CPUKey, string Expiration, string Tier, string PaymentInfo)
		{
			int length = (int)Directory.GetFiles("Database\\").Length;
			string str = string.Concat("Database\\", length, ".txt");
			string[] username = new string[] { "Username\t:", Username, Environment.NewLine, "CPU Key\t\t:", CPUKey, Environment.NewLine, "Expiration\t:", Expiration, Environment.NewLine, "Tier\t\t:", Tier, Environment.NewLine, "Payment Info\t:", PaymentInfo, Environment.NewLine, "Last IP\t\t:___.___.___.___" };
			File.WriteAllText(str, string.Concat(username));
		}

		public static void BanCPUKey(string CPUKey)
		{
			if (!File.ReadAllText("Banned.txt").Contains(CPUKey))
			{
				File.WriteAllText("Banned.txt", string.Concat(File.ReadAllText("Banned.txt"), Environment.NewLine, CPUKey));
			}
		}

		public static ClientHelper.KeyResponse CheckCPUKey(string CPUKey)
		{
			ClientHelper.KeyResponse keyResponse;
			if (!File.ReadAllText("Banned.txt").Contains(CPUKey))
			{
				string[] files = Directory.GetFiles("Database\\");
				int num = 0;
				while (num < (int)files.Length)
				{
					if (!File.ReadAllText(files[num]).Contains(CPUKey))
					{
						num++;
					}
					else
					{
						keyResponse = ClientHelper.KeyResponse.Registered;
						return keyResponse;
					}
				}
				keyResponse = ClientHelper.KeyResponse.Unknown;
			}
			else
			{
				keyResponse = ClientHelper.KeyResponse.Banned;
			}
			return keyResponse;
		}

		public static void EditCPUKey(int index, string CPUKey)
		{
			string[] strArrays = File.ReadAllLines(string.Concat("Database\\", index, ".txt"));
			strArrays[1] = string.Concat("CPU Key\t\t:", CPUKey);
			File.WriteAllLines(string.Concat("Database\\", index, ".txt"), strArrays);
		}

		public static void EditExpiration(int index, string Expiration)
		{
			string[] strArrays = File.ReadAllLines(string.Concat("Database\\", index, ".txt"));
			strArrays[2] = string.Concat("Expiration\t:", Expiration);
			File.WriteAllLines(string.Concat("Database\\", index, ".txt"), strArrays);
		}

		public static void EditIP(int index, string IP)
		{
			string[] strArrays = File.ReadAllLines(string.Concat("Database\\", index, ".txt"));
			strArrays[5] = string.Concat("Last IP\t\t:", IP);
			File.WriteAllLines(string.Concat("Database\\", index, ".txt"), strArrays);
		}

		public static void EditPaymentInfo(int index, string PaymentInfo)
		{
			string[] strArrays = File.ReadAllLines(string.Concat("Database\\", index, ".txt"));
			strArrays[4] = string.Concat("Payment Info\t:", PaymentInfo);
			File.WriteAllLines(string.Concat("Database\\", index, ".txt"), strArrays);
		}

		public static void EditTier(int index, string Tier)
		{
			string[] strArrays = File.ReadAllLines(string.Concat("Database\\", index, ".txt"));
			strArrays[3] = string.Concat("Tier\t\t:", Tier);
			File.WriteAllLines(string.Concat("Database\\", index, ".txt"), strArrays);
		}

		public static void EditUsername(int index, string Username)
		{
			string[] strArrays = File.ReadAllLines(string.Concat("Database\\", index, ".txt"));
			strArrays[0] = string.Concat("Username\t:", Username);
			File.WriteAllLines(string.Concat("Database\\", index, ".txt"), strArrays);
		}

		public static ClientHelper.Client GetClientInformation(int index)
		{
			ClientHelper.Client client = new ClientHelper.Client();
			string[] strArrays = File.ReadAllLines(string.Concat("Database\\", index, ".txt"));
			client.Username = strArrays[0].Replace("Username\t:", "");
			client.CPUKey = strArrays[1].Replace("CPU Key\t\t:", "");
			client.Expiration = strArrays[2].Replace("Expiration\t:", "");
			client.Tier = strArrays[3].Replace("Tier\t\t:", "");
			client.PaymentInfo = strArrays[4].Replace("Payment Info\t:", "");
			client.LastIP = strArrays[5].Replace("Last IP\t\t:", "");
			return client;
		}

		public static ClientHelper.Client[] GetClients()
		{
			int length = (int)Directory.GetFiles("Database\\").Length;
			ClientHelper.Client[] clientInformation = new ClientHelper.Client[length];
			for (int i = 0; i < length; i++)
			{
				clientInformation[i] = ClientHelper.GetClientInformation(i);
			}
			return clientInformation;
		}

		public static string GetCPUKey(int index)
		{
			string[] strArrays = File.ReadAllLines(string.Concat("Database\\", index, ".txt"));
			return strArrays[1].Replace("CPUKey\t\t:", "");
		}

		public static string GetExpiration(int index)
		{
			string[] strArrays = File.ReadAllLines(string.Concat("Database\\", index, ".txt"));
			return strArrays[2].Replace("Expiration\t:", "");
		}

		public static string GetIP(int index)
		{
			string[] strArrays = File.ReadAllLines(string.Concat("Database\\", index, ".txt"));
			return strArrays[5].Replace("Last IP\t\t:", "");
		}

		public static string GetPaymentInfo(int index)
		{
			string[] strArrays = File.ReadAllLines(string.Concat("Database\\", index, ".txt"));
			return strArrays[4].Replace("Payment Info\t:", "");
		}

		public static ClientHelper.TierType GetTier(int index)
		{
			ClientHelper.TierType tierType;
			string[] strArrays = File.ReadAllLines(string.Concat("Database\\", index, ".txt"));
            string[] conf = File.ReadAllLines("config.ini");
            if (!conf[0].Contains("free")) {
                if (!strArrays[3].Contains("Admin")) {
                    tierType = (!strArrays[3].Contains("VIP") ? ClientHelper.TierType.Client : ClientHelper.TierType.VIP);
                } else {
                    tierType = ClientHelper.TierType.Admin;
                }
            } else {
                tierType = ClientHelper.TierType.Free;
            }
			return tierType;
		}

		public static string GetUsername(int index)
		{
			string[] strArrays = File.ReadAllLines(string.Concat("Database\\", index, ".txt"));
			return strArrays[0].Replace("Username\t:", "");
		}

		public static void UnbanCPUKey(string CPUKey)
		{
			string str = "";
			string str1 = File.ReadAllText("Banned.txt");
			str = str1;
			if (str1.Contains(CPUKey))
			{
				File.WriteAllText("Banned.txt", str.Replace(CPUKey, ""));
			}
		}

		public struct Client
		{
			public string Username;

			public string CPUKey;

			public string Expiration;

			public string LastIP;

			public string Tier;

			public string PaymentInfo;
		}

		public enum KeyResponse
		{
			Banned,
			Registered,
			Unknown
		}

		public enum TierType : byte
		{
			Admin,
			Client,
			VIP,
            Free
        }
	}
}