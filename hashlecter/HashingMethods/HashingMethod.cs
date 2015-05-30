using System;
using System.Text;
using MD5x = System.Security.Cryptography.MD5;

namespace hashlecter
{
	public abstract class HashingMethod {

		/// <summary>
		/// Name of the hashing method
		/// </summary>
		/// <value></value>
		public abstract string Name { get; }

		/// <summary>
		/// Friendly name of the hashing method
		/// </summary>
		/// <value></value>
		public abstract string FriendlyName { get; }

		/// <summary>
		/// Format of the hashing method
		/// </summary>
		/// <value></value>
		public abstract string Format { get; }

		/// <summary>
		/// Checks the hash.
		/// </summary>
		/// <returns><c>true</c>, if the hashes collided, <c>false</c> otherwise.</returns>
		/// <param name="refhash">Original hash.</param>
		/// <param name="input">Input.</param>
		/// <param name="output">Output.</param>
		public abstract bool CheckHash (string refhash, string input, out string output);

		/// <summary>
		/// Calculates the MD5-hash of the given string
		/// </summary>
		/// <returns>Hexadecimal representation of the MD5-hash.</returns>
		/// <param name="str">Input string.</param>
		public static string MD5 (string str) {
			var bytes = Encoding.ASCII.GetBytes (str);
			byte[] hash;

			// This looks like it'd be really slow
			// and it probably is. But the .Net MD5 implementation
			// sucks and is not thread safe, so we need to instantiate
			// a new one every time we want to hash something.
			// gg Microsoft!
			using (var hasher = MD5x.Create ()) {
				hash = hasher.ComputeHash (bytes);
			}
			return hash.ToHex ();
		}
	}
}

