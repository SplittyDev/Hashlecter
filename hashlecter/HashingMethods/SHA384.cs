using System;
using System.Security.Cryptography;
using System.Text;

namespace hashlecter
{
	public abstract partial class HashingMethod {

		/// <summary>
		/// Calculates the SHA384-hash of the given string
		/// </summary>
		/// <returns>Hexadecimal representation of the SHA384-hash.</returns>
		/// <param name="str">Input string.</param>
		public static string SHA384 (string str) {

			var bytes = Encoding.ASCII.GetBytes (str);
			byte[] hash;

			// We're using the native SHA384 implementation here.
			using (var hasher = new SHA384CryptoServiceProvider ()) {
				hash = hasher.ComputeHash (bytes);
			}

			return hash.ToHex ();
		}
	}

	public class hSHA384 : HashingMethod
	{
		#region implemented abstract members of HashingMethod

		public override HashDelegate Hash { get { return HashingMethod.SHA384; } }
		public override HashingAlgorithm Algorithm { get { return HashingAlgorithm.SHA384; } }
		public override string Name { get { return "sha384"; } }
		public override string FriendlyName { get { return "SHA-384"; } }
		public override string Format { get { return "sha384($p)"; } }

		public override bool CheckHash (string refhash, string input, out string output) {
			output = string.Empty;
			if (refhash == null || input == null)
				return false;
			var hash = SHA384 (input);
			if (MainClass.options.rounds > 0)
				for (var i = 1; i < MainClass.options.rounds; i++)
					hash = SHA384 (hash);
			var success = refhash == hash;
			if (success)
				output = input;
			return success;
		}

		#endregion
	}
}

