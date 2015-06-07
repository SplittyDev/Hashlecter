using System;
using System.Text;

namespace hashlecter
{
	public abstract partial class HashingMethod {

		/// <summary>
		/// Calculates the JHash-hash of the given string
		/// </summary>
		/// <returns>Hexadecimal representation of the JHash-hash.</returns>
		/// <param name="str">Input string.</param>
		public static string JHash (string str) {

			var bytes = Encoding.ASCII.GetBytes (str);
			byte[] hash;

			hash = JHashCryptoServiceProvider.ComputeHash (bytes);

			return hash.ToHex ().ToLowerInvariant ();
		}
	}

	public class hJHash : HashingMethod
	{
		#region implemented abstract members of HashingMethod

		public override HashingAlgorithm Algorithm { get { return HashingAlgorithm.SHA256; } }
		public override string Name { get { return "jhash"; } }
		public override string FriendlyName { get { return "JHash"; } }
		public override string Format { get { return "jhash($p)"; } }

		public override bool CheckHash (string refhash, string input, out string output) {
			output = string.Empty;
			if (refhash == null || input == null)
				return false;
			var hash = JHash (input);
			if (MainClass.options.rounds > 0)
				for (var i = 1; i < MainClass.options.rounds; i++)
					hash = JHash (hash);
			var success = refhash == hash;
			if (success)
				output = input;
			return success;
		}

		#endregion
	}
}

