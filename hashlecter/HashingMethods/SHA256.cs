using System;
using System.Security.Cryptography;
using System.Text;

namespace hashlecter
{
	public abstract partial class HashingMethod {

		/// <summary>
		/// Calculates the SHA256-hash of the given string
		/// </summary>
		/// <returns>Hexadecimal representation of the SHA256-hash.</returns>
		/// <param name="str">Input string.</param>
		public static string SHA256 (string str) {

			var bytes = Encoding.ASCII.GetBytes (str);
			byte[] hash;

			// We're using the native SHA256 implementation here.
			using (var hasher = new SHA256CryptoServiceProvider ()) {
				hash = hasher.ComputeHash (bytes);
			}

			return hash.ToHex ();
		}
	}

	public class hSHA256 : HashingMethod
	{
		#region implemented abstract members of HashingMethod

		public override HashDelegate Hash { get { return HashingMethod.SHA256; } }
		public override HashingAlgorithm Algorithm { get { return HashingAlgorithm.SHA256; } }
		public override string Name { get { return "sha256"; } }
		public override string FriendlyName { get { return "SHA-256"; } }
		public override string Format { get { return "sha256($p)"; } }

		public override bool CheckHash (string refhash, string input, out string output) {
			output = string.Empty;
			if (refhash == null || input == null)
				return false;
			var hash = SHA256 (input);
			if (MainClass.options.rounds > 0)
				for (var i = 1; i < MainClass.options.rounds; i++)
					hash = SHA256 (hash);
			var success = refhash == hash;
			if (success)
				output = input;
			return success;
		}

		#endregion
	}

	public class hSHA256_Double : HashingMethod
	{
		#region implemented abstract members of HashingMethod

		public override HashDelegate Hash { get { return HashingMethod.SHA256; } }
		public override HashingAlgorithm Algorithm { get { return HashingAlgorithm.SHA256; } }
		public override string Name { get { return "sha256_double"; } }
		public override string FriendlyName { get { return "Two-Round SHA-256"; } }
		public override string Format { get { return "sha256(sha256($p))"; } }

		public override bool CheckHash (string refhash, string input, out string output) {
			output = string.Empty;
			if (refhash == null || input == null)
				return false;
			var hash = SHA256 (SHA256 (input));
			var success = refhash == hash;
			if (success)
				output = input;
			return success;
		}

		#endregion
	}
}

