using System;
using System.Security.Cryptography;
using System.Text;

namespace hashlecter
{
	public abstract partial class HashingMethod {

		/// <summary>
		/// Calculates the RIPEMD160-hash of the given string
		/// </summary>
		/// <returns>Hexadecimal representation of the RIPEMD160-hash.</returns>
		/// <param name="str">Input string.</param>
		public static string RIPEMD160 (string str) {

			var bytes = Encoding.ASCII.GetBytes (str);
			byte[] hash;

			// We're using the managed RIPEMD160 implementation here.
			using (var hasher = new RIPEMD160Managed ()) {
				hash = hasher.ComputeHash (bytes);
			}

			return hash.ToHex ();
		}
	}

	public class hRIPEMD160 : HashingMethod
	{
		#region implemented abstract members of HashingMethod

		public override HashDelegate Hash { get { return HashingMethod.RIPEMD160; } }
		public override HashingAlgorithm Algorithm { get { return HashingAlgorithm.RIPEMD160; } }
		public override string Name { get { return "ripemd160"; } }
		public override string FriendlyName { get { return "RIPEMD-160"; } }
		public override string Format { get { return "ripemd160($p)"; } }

		public override bool CheckHash (string refhash, string input, out string output) {
			output = string.Empty;
			if (refhash == null || input == null)
				return false;
			var hash = RIPEMD160 (input);
			if (MainClass.options.rounds > 0)
				for (var i = 1; i < MainClass.options.rounds; i++)
					hash = RIPEMD160 (hash);
			var success = refhash == hash;
			if (success)
				output = input;
			return success;
		}

		#endregion
	}
}

