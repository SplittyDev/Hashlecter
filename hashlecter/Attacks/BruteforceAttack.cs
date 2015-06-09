using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace hashlecter
{
	public static class BruteforceAttack
	{
		#region Constants

		public const string NUMERIC				= "0123456789";
		public const string LOWER_ALPHANUMERIC	= "abcdefghijklmnopqrstuvxzyz";
		public const string UPPER_ALPHANUMERIC	= "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		public const string SPECIAL_BASIC		= "!?-$+";
		public const string SPECIAL_ADVANCED	= ".,:;_#&~*'/\\\"%(){}[]^<>|";

		#endregion

		#region Static Fields

		static volatile int avg, avg_tmp, max;
		static volatile int generated, cracked;
		static double bruteforce_max;
		static string current_str, current_hash;
		static bool done;

		#endregion

		public static void Run (string[] hashes, HashingMethod method, string alphabet, int length = 6) {

			// Stopwatch for measuring time
			var watch = Stopwatch.StartNew ();

			if (!MainClass.options.silent) {

				// Update_Screen task
				var update = Task.Factory.StartNew (() => Update_Screen (watch));

				// Update_Stats task
				var update_avg = Task.Factory.StartNew (Update_Stats);
			}

			string output;
			var alphabet_first = alphabet.First ();
			var alphabet_last = alphabet.Last ();

			for (var ihash = 0; ihash < hashes.Length; ihash++) {

				// Skip comments
				if (hashes [ihash].StartsWith ("#"))
					continue;

				current_hash = hashes [ihash];

				// Incremental bruteforce
				if (MainClass.options.incremental) {

					// Calculate maximum
					for (var i = 1; i <= length; i++)
						bruteforce_max += Math.Pow (i, alphabet.Length);

					for (var curlen = 1; curlen <= length; ++curlen) {

						// Initialize buffer
						var accum = new StringBuilder (new String (alphabet_first, curlen));

						while (true) {
						
							++generated;
							++avg_tmp;
							var value = accum.ToString ();
							current_str = value;

							var success = method.CheckHash (current_hash, value, out output);
							if (success) {
								++cracked;
								MainClass.db.Add (MainClass.session, current_hash, value, method);
								done = true;
								goto end;
							}

							if (value.All (val => val == alphabet_last))
								break;

							// TODO: Parallelize this
							for (var i = curlen - 1; i >= 0; --i) {
								if (accum [i] != alphabet_last) {
									accum [i] = alphabet [alphabet.IndexOf (accum [i]) + 1];
									break;
								} else
									accum [i] = alphabet_first;
							}
						}
					}
				}

			// Fixed-length bruteforce
			else {

					// Calculated maximum
					bruteforce_max = Math.Pow (length, alphabet.Length);

					// Initialize buffer
					var accum = new StringBuilder (new String (alphabet_first, length));

					while (true) {

						++generated;
						++avg_tmp;
						var value = accum.ToString ();
						current_str = value;

						var success = method.CheckHash (current_hash, value, out output);
						if (success) {
							++cracked;
							MainClass.db.Add (MainClass.session, current_hash, value, method);
							done = true;
							goto end;
						}

						if (value.All (val => val == alphabet_last))
							break;

						// TODO: Parallelize this
						for (var i = length - 1; i >= 0; --i) {
							if (accum [i] != alphabet_last) {
								accum [i] = alphabet [alphabet.IndexOf (accum [i]) + 1];
								break;
							} else
								accum [i] = alphabet_first;
						}
					}
				}

				end: {}
			}


			watch.Stop ();

			// Display the correct stats after exiting
			if (!MainClass.options.silent) {

				Update_Screen (watch, force: true);
			}
		}

		static void Update_Screen (Stopwatch watch, bool force = false) {
			while (!done || force) {

				force = false;
				Console.Clear ();

				// Basic
				Console.WriteLine ("[Basic]");
				Console.WriteLine ("Speed  : {0} hash/s", avg == 0 ? "N/A" : avg.ToString ());
				Console.WriteLine ("Max    : {0} hash/s", max == 0 ? "N/A" : max.ToString ());
				Console.WriteLine ("Total  : {0} hashes", generated);
				Console.WriteLine ("Cracked: {0} cracked.", cracked);
				Console.WriteLine ("Hash   : {0}", current_hash);
				Console.WriteLine ();

				// Bruteforce
				Console.WriteLine ("[Bruteforce]");
				Console.WriteLine ("Max      : {0}", bruteforce_max);
				Console.WriteLine ("Generated: {0}", generated);
				Console.WriteLine ("Current  : {0}", current_str);
				Console.WriteLine ();

				// Database
				Console.WriteLine ("[Database]");
				Console.WriteLine ("Source : {0}", MainClass.SQLITE_DB);
				Console.WriteLine ("Session: {0}", MainClass.session);
				Console.WriteLine ();

				// N/A?
				if (avg == 0 || max == 0)
					Console.WriteLine ("Info: N/A means that Hashlecter couldn't collect enough information\n" +
						"      to display the average/max speed. Don't worry about it.\n");

				if (cracked > 0)
					Console.WriteLine ("Use lecter -s {0} --show to see the results.", MainClass.session);

				Thread.Sleep (250);
			}
		}

		static void Update_Stats () {
			while (!done) {
				Thread.Sleep (1000);
				avg = (avg + avg_tmp) / 2;
				max = Math.Max (max, avg_tmp);
				avg_tmp = 0;
			}
		}
	}
}

