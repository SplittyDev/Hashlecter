using System;
using System.Security.Cryptography;
using System.Text;

namespace hashlecter
{
	public abstract partial class HashingMethod {

		/// <summary>
		/// Calculates the Whirlpool-hash of the given string
		/// </summary>
		/// <returns>Hexadecimal representation of the Whirlpool-hash.</returns>
		/// <param name="str">Input string.</param>
		public static string Whirlpool (string str) {

			var bytes = Encoding.ASCII.GetBytes (str);
			byte[] hash;

			// Let's use our own Whirlpool implementation
			using (var hasher = new WhirlpoolCryptoServiceProvider ()) {
				hash = hasher.ComputeHash (bytes);
			}

			return hash.ToHex ();
		}
	}

	public class hWhirlpool : HashingMethod
	{
		#region implemented abstract members of HashingMethod

		public override HashDelegate Hash { get { return HashingMethod.Whirlpool; } }
		public override HashingAlgorithm Algorithm { get { return HashingAlgorithm.Whirlpool; } }
		public override string Name { get { return "whirlpool"; } }
		public override string FriendlyName { get { return "Whirlpool"; } }
		public override string Format { get { return "whirlpool($p)"; } }

		public override bool CheckHash (string refhash, string input, out string output) {
			output = string.Empty;
			if (refhash == null || input == null)
				return false;
			var hash = Whirlpool (input);
			if (MainClass.options.rounds > 0)
				for (var i = 1; i < MainClass.options.rounds; i++)
					hash = Whirlpool (hash);
			var success = refhash == hash;
			if (success)
				output = input;
			return success;
		}

		#endregion
	}
}

