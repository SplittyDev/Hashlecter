using System;
using System.Security.Cryptography;
using System.Text;

namespace hashlecter
{
	public abstract partial class HashingMethod {
		
		/// <summary>
		/// Calculates the SHA1-hash of the given string
		/// </summary>
		/// <returns>Hexadecimal representation of the SHA1-hash.</returns>
		/// <param name="str">Input string.</param>
		public static string SHA1 (string str) {

			var bytes = Encoding.ASCII.GetBytes (str);
			byte[] hash;

			// We're using the native SHA1 implementation here
			// since it's about 3 times faster than the managed implementation.
			using (var hasher = new SHA1CryptoServiceProvider ()) {
				hash = hasher.ComputeHash (bytes);
			}

			return hash.ToHex ().ToLowerInvariant ();
		}
	}

	public class hSHA1 : HashingMethod
	{
		#region implemented abstract members of HashingMethod

		public override HashDelegate Hash { get { return HashingMethod.SHA1; } }
		public override HashingAlgorithm Algorithm { get { return HashingAlgorithm.SHA1; } }
		public override string Name { get { return "sha1"; } }
		public override string FriendlyName { get { return "SHA-1"; } }
		public override string Format { get { return "sha1($p)"; } }

		public override bool CheckHash (string refhash, string input, out string output) {
			output = string.Empty;
			if (refhash == null || input == null)
				return false;
			var hash = SHA1 (input);
			if (MainClass.options.rounds > 0)
				for (var i = 1; i < MainClass.options.rounds; i++)
					hash = SHA1 (hash);
			var success = refhash == hash;
			if (success)
				output = input;
			return success;
		}

		#endregion
	}

	public class hSHA1_Double : HashingMethod
	{
		#region implemented abstract members of HashingMethod

		public override HashDelegate Hash { get { return HashingMethod.SHA1; } }
		public override HashingAlgorithm Algorithm { get { return HashingAlgorithm.SHA1; } }
		public override string Name { get { return "sha1_double"; } }
		public override string FriendlyName { get { return "Two-Round SHA-1"; } }
		public override string Format { get { return "sha1(sha1($p))"; } }

		public override bool CheckHash (string refhash, string input, out string output) {
			output = string.Empty;
			if (refhash == null || input == null)
				return false;
			var hash = SHA1 (SHA1 (input));
			var success = refhash == hash;
			if (success)
				output = input;
			return success;
		}

		#endregion
	}
}

