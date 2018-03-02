using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Helpers
{
	public static class XEXHelper
	{
		public static bool CheckXEXChecksum(byte[] xexchecksum)
		{
			byte[] numArray = File.ReadAllBytes(XEXHelper.ReadConfString("LatestXEXName"));
			SHA1 sHA1 = SHA1.Create();
			sHA1.Initialize();
			sHA1.TransformFinalBlock(numArray, 0, (int)numArray.Length);
			return sHA1.Hash.SequenceEqual<byte>(xexchecksum);
		}

		public static bool CheckXEXVersion(byte xexversion)
		{
			return xexversion >= (byte)XEXHelper.ReadConfInt("LatestXEXVersion");
		}

		public static string GetXEXName()
		{
			return XEXHelper.ReadConfString("LatestXEXName");
		}

		public static int GetXEXVersion()
		{
			return XEXHelper.ReadConfInt("LatestXEXVersion");
		}

		private static int ReadConfInt(string entry)
		{
			return int.Parse(XEXHelper.ReadConfString(entry));
		}

		private static string ReadConfString(string entry)
		{
			string str;
			string[] strArrays = File.ReadAllLines("Update Configuration.conf");
			int num = 0;
			while (true)
			{
				if (num >= (int)strArrays.Length)
				{
					str = "";
					break;
				}
				else if (!strArrays[num].StartsWith(entry))
				{
					num++;
				}
				else
				{
					str = strArrays[num].Replace(string.Concat(entry, " = "), "");
					break;
				}
			}
			return str;
		}

		public static void UpdateXEX(byte xexversion, string xexname)
		{
			XEXHelper.WriteConfInt("LatestXEXVersion", (int)xexversion);
			XEXHelper.WriteConfString("LatestXEXName", xexname);
		}

		private static void WriteConfInt(string entry, int value)
		{
			XEXHelper.WriteConfString(entry, value.ToString());
		}

		private static void WriteConfString(string entry, string value)
		{
			string[] strArrays = File.ReadAllLines("Update Configuration.conf");
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				if (strArrays[i].StartsWith(entry))
				{
					strArrays[i] = string.Concat(entry, " = ", value);
				}
			}
			File.WriteAllLines("Update Configuration.conf", strArrays);
		}

        public static byte[] GetXEXBytes(string xex) {
            return File.ReadAllBytes(xex);
        }
	}
}