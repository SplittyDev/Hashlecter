using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace hashlecter
{
	public static class BruteforceAttack
	{
		#region Constants

		const string NUMERIC			= "0123456789";
		const string LOWER_ALPHANUMERIC	= "abcdefghijklmnopqrstuvxzyz";
		const string UPPER_ALPHANUMERIC	= "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		const string SPECIAL_BASIC		= "!?-$+";
		const string SPECIAL_ADVANCED	= ".,:;_#&~*'/\\\"%(){}^<>|";

		#endregion

		#region Static Fields

		static volatile int avg, avg_tmp, max;
		static volatile int processed, generated, cracked;
		static string current_str, current_hash;
		static bool done;

		#endregion

		public static void Run (string[] hashes, HashingMethod method) {

			// Stopwatch for measuring time
			var watch = Stopwatch.StartNew ();

			if (!MainClass.options.silent) {

				// Update_Screen task
				var update = Task.Factory.StartNew (() => Update_Screen (watch));

				// Update_Stats task
				var update_avg = Task.Factory.StartNew (Update_Stats);
			}

			// Iterate over all hashes
			for (var i = 0; i < hashes.Length; i++) {

				// Skip comments
				if (hashes[i].StartsWith ("#"))
					continue;

				current_hash = hashes[i];

				// TODO: Implement bruteforce
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
				Console.WriteLine ("Total  : {0} hashes", processed);
				Console.WriteLine ("Cracked: {0} cracked.", cracked);
				Console.WriteLine ("Hash   : {0}", current_hash);
				Console.WriteLine ();

				// Dictionary
				if (!string.IsNullOrEmpty (MainClass.options.input_dict)) {
					Console.WriteLine ("[Bruteforce]");
					Console.WriteLine ("Generated: {0}", generated);
					Console.WriteLine ("Current  : {0}", current_str);
					Console.WriteLine ();
				}

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

