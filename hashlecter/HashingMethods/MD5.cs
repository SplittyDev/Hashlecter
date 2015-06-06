using System;
using System.Security.Cryptography;
using System.Text;

namespace hashlecter
{
	public abstract partial class HashingMethod {
		
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
			using (var hasher = new MD5CryptoServiceProvider ()) {
				hash = hasher.ComputeHash (bytes);
			}

			return hash.ToHex ().ToLowerInvariant ();
		}
	}

	public class hMD5 : HashingMethod
	{
		#region implemented abstract members of HashingMethod

		public override HashingAlgorithm Algorithm { get { return HashingAlgorithm.MD5; } }
		public override string Name { get { return "md5"; } }
		public override string FriendlyName { get { return "MD5"; } }
		public override string Format { get { return "md5($p)"; } }

		public override bool CheckHash (string refhash, string input, out string output) {
			output = string.Empty;
			if (refhash == null || input == null)
				return false;
			var hash = MD5 (input);
			if (MainClass.options.rounds > 0)
				for (var i = 1; i < MainClass.options.rounds; i++)
					hash = MD5 (hash);
			var success = refhash == hash;
			if (success)
				output = input;
			return success;
		}

		#endregion
	}

	public class hMD5_Double : HashingMethod
	{
		#region implemented abstract members of HashingMethod

		public override HashingAlgorithm Algorithm { get { return HashingAlgorithm.MD5; } }
		public override string Name { get { return "md5_double"; } }
		public override string FriendlyName { get { return "Two-Round MD5"; } }
		public override string Format { get { return "md5(md5($p))"; } }

		public override bool CheckHash (string refhash, string input, out string output) {
			output = string.Empty;
			if (refhash == null || input == null)
				return false;
			var hash = MD5 (MD5 (input));
			var success = refhash == hash;
			if (success)
				output = input;
			return success;
		}

		#endregion
	}

	public class hMD5_Salted : HashingMethod
	{
		#region implemented abstract members of HashingMethod

		public override HashingAlgorithm Algorithm { get { return HashingAlgorithm.MD5; } }
		public override string Name { get { return "md5_salted"; } }
		public override string FriendlyName { get { return "Simple Salted MD5"; } }
		public override string Format { get { return "md5($p.$s)"; } }

		public override bool CheckHash (string refhash, string input, out string output) {
			output = string.Empty;
			if (input == null || refhash == null)
				return false;
			var parts = refhash.Split (':');
			var realref = parts[0];
			var s = parts[1];
			var hash = MD5 (input + s);
			var success = realref == hash;
			if (success)
				output = input;
			return success;
		}

		#endregion
	}

	public class hMD5_MyBB : HashingMethod
	{
		#region implemented abstract members of HashingMethod

		public override HashingAlgorithm Algorithm { get { return HashingAlgorithm.MD5; } }
		public override string Name { get { return "md5_mybb"; } }
		public override string FriendlyName { get { return "MyBB-Style Salted MD5"; } }
		public override string Format { get { return "md5(md5($s).md5($p))"; } }

		public override bool CheckHash (string refhash, string input, out string output) {
			output = string.Empty;
			if (input == null || refhash == null)
				return false;
			var parts = refhash.Split (':');
			var realref = parts[0];
			var s = parts[1];
			var hash = MD5 (MD5 (s) + MD5 (input));
			var success = realref == hash;
			if (success)
				output = input;
			return success;
		}

		#endregion
	}
}

