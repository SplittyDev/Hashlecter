using System;
using System.Security.Cryptography;
using System.Text;

namespace hashlecter
{
	public static class JHashCryptoServiceProvider
	{
		const int HASHSIZE = 8;

		public static byte[] ComputeHash (byte[] input) {
			
			var buffer = new byte[HASHSIZE];
			var padded = Pad (input);

			for (var i = 0; i < buffer.Length; i++) {
				
				var mod = i % HASHSIZE;

				// buffer[mod] *= 2
				buffer[mod] <<= 1;

				// buffer[mod] ^= padded[(i * 2) % 8)]
				buffer[mod] ^= padded[(i << 1) % HASHSIZE];

				// buffer[mod] /= 2
				buffer[mod] >>= 1;

				// buffer[mod] ^= padded[sqrt(i) % 8]
				buffer[mod] ^= padded[Convert.ToInt32 (Math.Sqrt (i)) % HASHSIZE];

				// buffer[mod] *= 2
				buffer[mod] <<= 1;
			}

			return buffer;
		}

		static byte[] Pad (byte[] bytes) {
			
			if (bytes.Length >= HASHSIZE)
				return bytes;
			
			var buffer = new byte[HASHSIZE];
			Array.Copy (bytes, buffer, bytes.Length);

			for (var i = bytes.Length; i < HASHSIZE; i++)
				buffer[i] = 1;
			
			return buffer;
		}
	}
}

