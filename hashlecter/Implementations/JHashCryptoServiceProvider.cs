using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace hashlecter
{
	public static class JHashCryptoServiceProvider
	{
		const int PAD_LEN = 8;

		static byte a, b, c, d, e, f, g, h;

		public static byte[] ComputeHash (byte[] input) {
			
			var buffer = new List<byte> (PAD_LEN);
			var padded = Pad (input);

			a = padded[0];
			b = padded[1];
			c = padded[2];
			d = padded[3];
			e = padded[4];
			f = padded[5];
			g = padded[6];
			h = padded[7];

			buffer.AddRange (new [] {
				(byte)TransA (),
				(byte)TransB (),
				(byte)TransC (),
				(byte)TransD (),
				(byte)TransE (),
				(byte)TransF (),
				(byte)TransG (),
				(byte)TransH (),
			});

			return buffer.ToArray ();
		}

		static int TransA () {
			return (a & b) | (~a & c);
		}

		static int TransB () {
			return (a & c) | (b & ~c);
		}

		static int TransC () {
			return a ^ b ^ c;
		}

		static int TransD () {
			return b ^ (a | ~c);
		}

		static int TransE () {
			return (d & e) | (~d & f);
		}

		static int TransF () {
			return (d & f) | (e & ~f);
		}

		static int TransG () {
			return (d ^ e ^ f);
		}

		static int TransH () {
			return e ^ (h | ~g);
		}

		static byte[] Pad (byte[] bytes) {
			
			if (bytes.Length >= PAD_LEN)
				return bytes;
			
			var buffer = new byte[PAD_LEN];
			Array.Copy (bytes, buffer, bytes.Length);

			for (var i = bytes.Length; i < PAD_LEN; i++)
				buffer[i] = 1;
			
			return buffer;
		}
	}
}

