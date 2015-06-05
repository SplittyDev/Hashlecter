using System;
using System.Text;
using System.Text.RegularExpressions;

namespace hashlecter
{
	public static class HashCheck
	{
		public static bool IsMD5Hash (this string str) {
			const string pattern = "[0-9a-fA-F]{32}";
			return Regex.IsMatch (str, pattern);
		}

		public static bool IsSHA1Hash (this string str) {
			const string pattern = "[0-9a-fA-F]{40}";
			return Regex.IsMatch (str, pattern);
		}

		public static bool IsSHA1Base64 (this string str) {
			try {
				return Encoding.ASCII.GetString (Convert.FromBase64String (str)).IsSHA1Hash ();
			} catch {
				return false;
			}
		}
	}
}

