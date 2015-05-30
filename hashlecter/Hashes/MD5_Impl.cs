using System;

namespace hashlecter
{

	//
	// DO NOT USE THIS CODE !!
	// It is completely broken and WILL NOT work.
	//

	public static class MD5_Impl
	{
		static ushort[] s = {
			7, 12, 17, 22,  7, 12, 17, 22,  7, 12, 17, 22,  7, 12, 17, 22,
			5,  9, 14, 20,  5,  9, 14, 20,  5,  9, 14, 20,  5,  9, 14, 20,
			4, 11, 16, 23,  4, 11, 16, 23,  4, 11, 16, 23,  4, 11, 16, 23,
			6, 10, 15, 21,  6, 10, 15, 21,  6, 10, 15, 21,  6, 10, 15, 21,
		};

		static uint[] K = {
			0xd76aa478, 0xe8c7b756, 0x242070db, 0xc1bdceee,
			0xf57c0faf, 0x4787c62a, 0xa8304613, 0xfd469501,
			0x698098d8, 0x8b44f7af, 0xffff5bb1, 0x895cd7be,
			0x6b901122, 0xfd987193, 0xa679438e, 0x49b40821,
			0xf61e2562, 0xc040b340, 0x265e5a51, 0xe9b6c7aa,
			0xd62f105d, 0x02441453, 0xd8a1e681, 0xe7d3fbc8,
			0x21e1cde6, 0xc33707d6, 0xf4d50d87, 0x455a14ed,
			0xa9e3e905, 0xfcefa3f8, 0x676f02d9, 0x8d2a4c8a,
			0xfffa3942, 0x8771f681, 0x6d9d6122, 0xfde5380c,
			0xa4beea44, 0x4bdecfa9, 0xf6bb4b60, 0xbebfbc70,
			0x289b7ec6, 0xeaa127fa, 0xd4ef3085, 0x04881d05,
			0xd9d4d039, 0xe6db99e5, 0x1fa27cf8, 0xc4ac5665,
			0xf4292244, 0x432aff97, 0xab9423a7, 0xfc93a039,
			0x655b59c3, 0x8f0ccc92, 0xffeff47d, 0x85845dd1,
			0x6fa87e4f, 0xfe2ce6e0, 0xa3014314, 0x4e0811a1,
			0xf7537e82, 0xbd3af235, 0x2ad7d2bb, 0xeb86d391,
		};

		static uint[] X;

		public static string Hash (string msg) {
			X = new uint[16];

			uint a = 0x67452301;
			uint b = 0xefcdab89;
			uint c = 0x98badcfe;
			uint d = 0x10325476;

			byte[] newmsg;
			{
				var msgbytes = System.Text.Encoding.ASCII.GetBytes (msg);
				uint mbytelen = (uint)System.Text.Encoding.ASCII.GetByteCount (msg);
				uint msglen = mbytelen * 8;
				uint mod = 448 - (msglen % 512);
				uint padding = (mod + 512) % 512;

				if (padding == 0)
					padding = 512;

				uint bufsz = mbytelen + (padding / 8) + 8;
				newmsg = new byte[bufsz];

				for (var i = 0; i < mbytelen; i++)
					newmsg[i] = msgbytes[i];

				newmsg[msgbytes.Length] = 0x80;

				for (var i = 8; i > 0; i--)
					newmsg[bufsz - i] = (byte)(msglen >> ((8 - i) * 8) & 0x00000000000000FF);
			}

			var n = (uint)(newmsg.Length * 8) / 32;
			for (uint i = 0; i < n / 16; i++) {
				{
					var block = i << 6;
					for (uint j = 0; j < 61; j += 4)
						X[j >> 2] =
						(uint)newmsg[block + (j + 3)] << 24 |
						(uint)newmsg[block + (j + 2)] << 16 |
						(uint)newmsg[block + (j + 1)] << 8 |
						(uint)newmsg[block + j];
				}

				uint aa = a;
				uint bb = b;
				uint cc = c;
				uint dd = d;

				uint _i = 0;

				// Round 1
				Step (F, ref a, b, c, d, 0, s[_i], ++_i);
				Step (F, ref a, b, c, d, 1, s[_i], ++_i);
				Step (F, ref a, b, c, d, 2, s[_i], ++_i);
				Step (F, ref a, b, c, d, 3, s[_i], ++_i);
				Step (F, ref a, b, c, d, 4, s[_i], ++_i);
				Step (F, ref a, b, c, d, 5, s[_i], ++_i);
				Step (F, ref a, b, c, d, 6, s[_i], ++_i);
				Step (F, ref a, b, c, d, 7, s[_i], ++_i);
				Step (F, ref a, b, c, d, 8, s[_i], ++_i);
				Step (F, ref a, b, c, d, 9, s[_i], ++_i);
				Step (F, ref a, b, c, d, 10, s[_i], ++_i);
				Step (F, ref a, b, c, d, 11, s[_i], ++_i);
				Step (F, ref a, b, c, d, 12, s[_i], ++_i);
				Step (F, ref a, b, c, d, 13, s[_i], ++_i);
				Step (F, ref a, b, c, d, 14, s[_i], ++_i);
				Step (F, ref a, b, c, d, 15, s[_i], ++_i);

				// Round 2
				Step (G, ref a, b, c, d, 1, s[_i], ++_i);
				Step (G, ref a, b, c, d, 6, s[_i], ++_i);
				Step (G, ref a, b, c, d, 11, s[_i], ++_i);
				Step (G, ref a, b, c, d, 0, s[_i], ++_i);
				Step (G, ref a, b, c, d, 5, s[_i], ++_i);
				Step (G, ref a, b, c, d, 10, s[_i], ++_i);
				Step (G, ref a, b, c, d, 15, s[_i], ++_i);
				Step (G, ref a, b, c, d, 4, s[_i], ++_i);
				Step (G, ref a, b, c, d, 9, s[_i], ++_i);
				Step (G, ref a, b, c, d, 14, s[_i], ++_i);
				Step (G, ref a, b, c, d, 3, s[_i], ++_i);
				Step (G, ref a, b, c, d, 8, s[_i], ++_i);
				Step (G, ref a, b, c, d, 13, s[_i], ++_i);
				Step (G, ref a, b, c, d, 2, s[_i], ++_i);
				Step (G, ref a, b, c, d, 7, s[_i], ++_i);
				Step (G, ref a, b, c, d, 12, s[_i], ++_i);

				// Round 3
				Step (H, ref a, b, c, d, 5, s[_i], ++_i);
				Step (H, ref a, b, c, d, 8, s[_i], ++_i);
				Step (H, ref a, b, c, d, 11, s[_i], ++_i);
				Step (H, ref a, b, c, d, 14, s[_i], ++_i);
				Step (H, ref a, b, c, d, 1, s[_i], ++_i);
				Step (H, ref a, b, c, d, 4, s[_i], ++_i);
				Step (H, ref a, b, c, d, 7, s[_i], ++_i);
				Step (H, ref a, b, c, d, 10, s[_i], ++_i);
				Step (H, ref a, b, c, d, 13, s[_i], ++_i);
				Step (H, ref a, b, c, d, 0, s[_i], ++_i);
				Step (H, ref a, b, c, d, 3, s[_i], ++_i);
				Step (H, ref a, b, c, d, 6, s[_i], ++_i);
				Step (H, ref a, b, c, d, 9, s[_i], ++_i);
				Step (H, ref a, b, c, d, 12, s[_i], ++_i);
				Step (H, ref a, b, c, d, 15, s[_i], ++_i);
				Step (H, ref a, b, c, d, 2, s[_i], ++_i);

				// Round 4
				Step (I, ref a, b, c, d, 0, s[_i], ++_i);
				Step (I, ref a, b, c, d, 7, s[_i], ++_i);
				Step (I, ref a, b, c, d, 14, s[_i], ++_i);
				Step (I, ref a, b, c, d, 5, s[_i], ++_i);
				Step (I, ref a, b, c, d, 12, s[_i], ++_i);
				Step (I, ref a, b, c, d, 3, s[_i], ++_i);
				Step (I, ref a, b, c, d, 10, s[_i], ++_i);
				Step (I, ref a, b, c, d, 1, s[_i], ++_i);
				Step (I, ref a, b, c, d, 8, s[_i], ++_i);
				Step (I, ref a, b, c, d, 15, s[_i], ++_i);
				Step (I, ref a, b, c, d, 6, s[_i], ++_i);
				Step (I, ref a, b, c, d, 13, s[_i], ++_i);
				Step (I, ref a, b, c, d, 4, s[_i], ++_i);
				Step (I, ref a, b, c, d, 11, s[_i], ++_i);
				Step (I, ref a, b, c, d, 2, s[_i], ++_i);
				Step (I, ref a, b, c, d, 9, s[_i], ++_i);

				a += aa;
				b += bb;
				c += cc;
				d += dd;
			}

			var stra = ReverseByte (a).ToString ("X8");
			var strb = ReverseByte (b).ToString ("X8");
			var strc = ReverseByte (c).ToString ("X8");
			var strd = ReverseByte (d).ToString ("X8");
			return string.Format ("{0}{1}{2}{3}", stra, strb, strc, strd);
		}

		public static uint ReverseByte(uint uiNumber)
		{
			return
				(uiNumber & 0x000000ff) << 24 |
				(uiNumber >> 24) |
				(uiNumber & 0x00FF0000) >> 8 |
				(uiNumber & 0x0000FF00) << 8;
		}

		static void Step (Func<uint, uint, uint, uint> trans, ref uint a, uint b, uint c, uint d, uint k, ushort s, uint i) {
			uint transform = a + trans (b, c, d) + X[k] + K[i - 1];
			a = b + (transform >> 32 - s | transform << s);
		}

		#region MD5 Core Functions
		static uint F (uint x, uint y, uint z) {
			return z ^ (x & (y ^ z));
			// return x & y | ~x & z;
		}

		static uint G (uint x, uint y, uint z) {
			return y ^ (z & (x ^ y));
			// return F1 (z, x, y);
		}

		static uint H (uint x, uint y, uint z) {
			return x ^ y ^ z;
		}

		static uint I (uint x, uint y, uint z) {
			return y ^ (x | ~z);
		}
		#endregion

		/*
		static int xStep (Func<int, int, int, int> f, int w, int x, int y, int z, int data, int s) {
			// w += ((f (x, y, z) + data) << s | (f (x, y, z) + data) >> (32 - s)) + x;
			w += f (x, y, z) + data;
			// Rotate left
			w = w << s | w >> (32 - s);
			w += x;
			return w;
		}
		*/
	}
}

