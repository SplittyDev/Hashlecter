using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

#if LIBNOTIFYNET
using libnotify.net;
#endif

namespace hashlecter
{
	public static class DictionaryAttack
	{
		#region Constants

		const int BUFFER_SZ = 102400;

		#endregion

		#region Static Fields

		static DictionaryAttackOptions options;
		static volatile int avg, avg_tmp, max;
		static volatile int processed, loaded, cracked;
		static string dict_current, current_hash;
		static bool done;

		#endregion

		#region Static Properties

		public static DictionaryAttackOptions Options {
			get {
				if (options == null)
					options = new DictionaryAttackOptions
					{
						mutate_simple = false,
						mutate_advanced = false,
					};
				return options;
			}
		}

		#endregion

		public static void Run (string[] hashes, HashingMethod method, string dictionary_path) {

			#if LIBNOTIFYNET
			Notification.Send ("Hashlecter", "Started dictionary attack.", 5000);
			#endif
			
			// Dictionary buffer
			string[] dict = new string[BUFFER_SZ];

			// Stopwatch for measuring time
			var watch = Stopwatch.StartNew ();

			if (!MainClass.options.silent) {

				// Update_Screen task
				var update = Task.Factory.StartNew (() => Update_Screen (watch));

				// Update_Stats task
				var update_avg = Task.Factory.StartNew (Update_Stats);
			}

			// Open dictionary file for reading
			using (var fdict = File.OpenRead (dictionary_path))
			using (var reader = new StreamReader (fdict)) {

				// Iterate over all hashes in the array
				for (var i = 0; i < hashes.Length; i++) {

					// Skip comments
					if (hashes[i].StartsWith ("#"))
						continue;

					current_hash = hashes[i];

					// Check if there is still something in the dictionary to read
					while (reader.BaseStream.Position < reader.BaseStream.Length) {

						// Iterate over all values in [0..BUFFER_SZ]
						for (var j = 0; j < BUFFER_SZ; j++) {

							// Add the next line from the dictionary to the buffer
							dict[j] = reader.ReadLine ();

							// Increment the loaded variable if the line is valid
							if (dict[j] != null) {
								dict[j] = dict[j].Replace ("\r", "");
								loaded++;
							}
						}

						var breakout = false;

						// Use parallel processing to iterate over all entries in the dictionary buffer
						Parallel.ForEach<string> (dict, (dict_entry, loopstate_inner) => {

							// Get the current hash
							var hash = hashes[i];

							// Create a variable for storing the output (if valid)
							string output;

							// Get the current dictionary entry
							// (for the Update_Screen task)
							dict_current = dict_entry;

							// Try to collide the hashes
							var success = method.CheckHash (hash, dict_entry, out output);

							// Increment the statistically relevant variables
							++processed;
							++avg_tmp;

							// Check if the collision succeeded
							if (success) {

								// Increment the amount of successfully collided hashes
								// (for the Update_Screen task)
								++cracked;

								// Add the collision to the database
								MainClass.db.Add (MainClass.session, hash, output, method);

								#if LIBNOTIFYNET
								var _libnotifynet_format = string.Format ("Successfully cracked {0} hash:\n{1}\nValue was: {2}", method.Name, hash, output);
								Notification.Send ("Hashlecter", _libnotifynet_format, 7500, 250, 100);
								#endif

								if (!MainClass.options.exp_single_cont) {
									
									// Break out of the loop
									breakout = true;
									loopstate_inner.Stop ();
								}
							}
						});

						if (breakout)
							break;
					}

					// Reset the dictionary position to 0
					// for processing the next hash
					reader.BaseStream.Position = 0;

					// Reset the 
					loaded = 0;
				}

				// We're done!
				// Tell all tasks to stop
				done = true;
				watch.Stop ();
			}

			// Display the correct stats after exiting
			if (!MainClass.options.silent) {
				
				Update_Screen (watch, force: true);
			}
		}

		//
		// This is an experimental method.
		// Enable using the --exp-lazy-eval switch
		//
		public static void RunLazy (string[] hashes, HashingMethod method, string dictionary_path) {
			
			// Dictionary buffer
			IEnumerable<string> dict;

			// Stopwatch for measuring time
			var watch = Stopwatch.StartNew ();

			if (!MainClass.options.silent) {
				
				// Update_Screen task
				var update = Task.Factory.StartNew (() => Update_Screen (watch));

				// Update_Stats task
				var update_avg = Task.Factory.StartNew (Update_Stats);
			}

			// Lazy approach
			var dictionary = File.ReadLines (dictionary_path);
			var dictionary_pos = 0;
			var dictionary_count = dictionary.Count ();

			// Iterate over all hashes in the array
			for (var i = 0; i < hashes.Length; i++) {

				// Skip comments
				if (hashes[i].StartsWith ("#"))
					continue;

				current_hash = hashes[i];

				//while (dictionary_pos <= dictionary_count) {
				while (dictionary_pos < dictionary_count) {
					
					dict = dictionary.Skip (dictionary_pos).Take (BUFFER_SZ);
					dictionary_pos += BUFFER_SZ;
					loaded = dictionary_pos;

					var breakout = false;

					// Use parallel processing to iterate over all entries in the dictionary buffer
					Parallel.ForEach<string> (dict, (dict_entry, loopstate_inner) => {

						// Get the current hash
						var hash = hashes[i];

						// Create a variable for storing the output (if valid)
						string output;

						// Get the current dictionary entry
						// (for the Update_Screen task)
						dict_current = dict_entry;

						// Try to collide the hashes
						var success = method.CheckHash (hash, dict_entry, out output);

						// Increment the statistically relevant variables
						++processed;
						++avg_tmp;

						// Check if the collision succeeded
						if (success) {

							// Increment the amount of successfully collided hashes
							// (for the Update_Screen task)
							++cracked;

							// Add the collision to the database
							MainClass.db.Add (MainClass.session, hash, output, method);

							// Break out of the loop
							breakout = true;
							loopstate_inner.Stop ();
						}
					});

					if (breakout)
						break;
				}

				dictionary_pos = 0;

			}

			done = true;
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
				Console.WriteLine ("Total  : {0} hashes", processed);
				Console.WriteLine ("Cracked: {0} cracked.", cracked);
				Console.WriteLine ("Hash   : {0}", current_hash);
				Console.WriteLine ();

				// Dictionary
				if (!string.IsNullOrEmpty (MainClass.options.input_dict)) {
					Console.WriteLine ("[Dictionary]");
					Console.WriteLine ("Loaded : {0}", loaded);
					Console.WriteLine ("Current: {0}", dict_current);
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

		public class DictionaryAttackOptions {
			public bool mutate_simple;
			public bool mutate_advanced;
		}
	}
}

