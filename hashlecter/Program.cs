#define MD5_COMPLETE

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Codeaddicts.libArgument;
using System.Text;

namespace hashlecter
{
	class MainClass
	{
		#region Constants

		/// <summary>
		/// The default SQLite database path.
		/// </summary>
		const string SQLITE_DB = "lecter.db";

		#endregion

		#region Statics

		/// <summary>
		/// The parsed command-line arguments.
		/// </summary>
		static Options options;

		/// <summary>
		/// A hashset containing the hashing methods.
		/// </summary>
		static HashSet<HashingMethod> methods;

		/// <summary>
		/// The current session identifier.
		/// </summary>
		static string session;

		/// <summary>
		/// The current SQLite database.
		/// </summary>
		static SQLite db;

		/// <summary>
		/// A flag that indicates whether all tasks should stop or not.
		/// </summary>
		static volatile bool done;

		#endregion

		/// <summary>
		/// The entry point of the program, where the program control starts and ends.
		/// </summary>
		/// <param name="args">The command-line arguments.</param>
		public static void Main (string[] args) {
			
			// Create a hashset for storing the hashing methods
			methods = new HashSet<HashingMethod> ();

			#if (MD5_COMPLETE)
			methods.Add (new hMD5 ());
			methods.Add (new hMD5_Double ());
			methods.Add (new hMD5_MyBB ());
			#endif

			// Connect to the SQLite database
			db = Database.Factory.CreateSQLite (SQLITE_DB);

			// Parse command-line arguments using libArgument
			options = ArgumentParser.Parse<Options> (args);

			// Use wizard if lecter was started directly
			options.wizard |= args.Length == 0;

			// Get or create a session identifier
			session = string.IsNullOrEmpty (options.session) ? GenerateSession () : options.session;

			if (options.help) {
				PreHelp ();
				ArgumentParser.Help ();
				Help ();
				PostHelp ();
				return;
			}

			if (options.wizard) {
				Wizard ();
			}

			if (options.show) {
				db.Show (string.IsNullOrEmpty (options.session) ? string.Empty : session);
				return;
			}

			// Get the method used for cracking the hashes
			var method = methods.FirstOrDefault (m => m.Name == options.method.ToLowerInvariant ());

			// Check if the cracking method is valid
			if (method == null) {
				Console.WriteLine ("Invalid hashing method!");
				Console.WriteLine ("Run lecter --help to get a list of all available hashing methods.");
				return;
			}

			string[] input_hashes = null;

			// Grab input from stdin
			if (options.input_stdin)
				input_hashes = FromStdin ();
			
			// Grab input from file
			else if (!string.IsNullOrEmpty (options.input_file))
				input_hashes = FromFile (options.input_file);

			Console.WriteLine ("Processing {0} hashes.", input_hashes.Count (s => !s.StartsWith ("#")));
			Process (input_hashes, method, options.input_dict);
		}

		static void Process (string[] hashes, HashingMethod method, string dict_filename) {
			
			// Default dictionary buffer size
			const int BUFFER_SZ = 102400; // 100 KiB

			// Dictionary buffer
			string[] dict = new string[BUFFER_SZ];

			// Stopwatch for measuring time
			var watch = Stopwatch.StartNew ();

			// Update_Screen task
			var update = Task.Factory.StartNew (() => Update_Screen (watch));

			// Update_Stats task
			var update_avg = Task.Factory.StartNew (Update_Stats);

			// Open dictionary file for reading
			using (var fdict = File.OpenRead (dict_filename))
			using (var reader = new StreamReader (fdict)) {

				// Iterate over all hashes in the array
				for (var i = 0; i < hashes.Length; i++) {

					// Skip comments
					if (hashes[i].StartsWith ("#"))
						continue;

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
								cracked++;

								// Add the collision to the database
								db.Add (session, hash, output);

								// Break out of the loop
								loopstate_inner.Stop ();
							}
						});
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
			}
		}

		static int avg, avg_tmp, max;
		static int processed, loaded, cracked;
		static string dict_current;
		static void Update_Screen (Stopwatch watch) {
			while (!done) {
				Console.Clear ();

				// Basic
				Console.WriteLine ("[Basic]");
				Console.WriteLine ("Speed  : {0} hash/s", avg);
				Console.WriteLine ("Max    : {0} hash/s", max);
				Console.WriteLine ("Total  : {0} hashes", processed);
				Console.WriteLine ("Cracked: {0} cracked.", cracked);
				Console.WriteLine ();

				// Dictionary
				if (!string.IsNullOrEmpty (options.input_dict)) {
					Console.WriteLine ("[Dictionary]");
					Console.WriteLine ("Loaded : {0}", loaded);
					Console.WriteLine ("Current: {0}", dict_current);
					Console.WriteLine ();
				}

				// Database
				Console.WriteLine ("[Database]");
				Console.WriteLine ("Source : {0}", SQLITE_DB);
				Console.WriteLine ("Session: {0}", session);
				Console.WriteLine ();

				if (cracked > 0)
					Console.WriteLine ("Use lecter -s {0} --show to see the results.", session);

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

		/// <summary>
		/// Grab input from stdin
		/// </summary>
		/// <returns>An array of hashes</returns>
		static string[] FromStdin () {
			return Console.In.ReadToEnd ().Replace ("\r", string.Empty).Split ('\n');
		}

		/// <summary>
		/// Grabs input from file
		/// </summary>
		/// <returns>An array of hashes</returns>
		/// <param name="filename">Filename.</param>
		static string[] FromFile (string filename) {
			return File.ReadAllLines (filename);
		}

		/// <summary>
		/// Grabs input from sql dump
		/// </summary>
		/// <returns>An array of hashes</returns>
		/// <param name="filename">Filename.</param>
		static string[] FromDump (string filename) {

			// This needs to be implemented.
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Generates a session identifier.
		/// </summary>
		/// <returns>The session identifier.</returns>
		static string GenerateSession () {
			const string chars = "abcdefghijklmnopqrstuvwxyz";
			const int len = 6;
			var rand = new Random ();
			var accum = new StringBuilder ();
			for (var i = 0; i < len; i++) {
				accum.Append (chars[rand.Next (0, chars.Length)]);
			}
			return accum.ToString ();
		}

		#region Help

		/// <summary>
		/// Shows the text before the help text
		/// </summary>
		public static void PreHelp () {
			var version = Assembly.GetExecutingAssembly ().GetName ().Version.ToString (3);
			Console.WriteLine ("Hashlecter Version {0}\n", version);
		}

		/// <summary>
		/// Shows the help text
		/// </summary>
		public static void Help () {
			Console.WriteLine ("Verbosity:");
			Console.WriteLine ("silent              Print nothing");
			Console.WriteLine ("low                 Print only important messages");
			Console.WriteLine ("high                Print everything");
			Console.WriteLine ("\nHashing methods:");
			foreach (var method in methods) {
				var name = method.Name.PadRight (20, ' ');
				Console.WriteLine (string.Format ("{0}{1}", name, method.Format));
			}
			Console.WriteLine ();
		}

		/// <summary>
		/// Shows the text after the help text
		/// </summary>
		public static void PostHelp () {
			Console.WriteLine ("Examples:");
			Console.WriteLine ("MD5 bruteforce attack: lecter -m md5 -i hash.txt");
			Console.WriteLine ("MyBB dictionary attack: lecter -m md5_mybb -i hash.txt --dict dictionary.txt");
		}

		#endregion

		// This needs to be optimized.
		// A lot.
		public static void Wizard () {
			Console.Title = "Hashlecter Wizard";
			Console.WindowWidth = 60;
			Console.WindowHeight = 30;
			Console.Clear ();
			Console.WriteLine ("Hashlecter Wizard\n");
			Console.WriteLine ("Drag a file containing your hashes here.");
			options.input_file = ReadSilent ();
			var valid_choice = false;
			while (!valid_choice) {
				Console.Clear ();
				Console.WriteLine ("Hashlecter Wizard\n");
				Console.WriteLine ("Input: {0}\n", Path.GetFileName (options.input_file));
				Console.WriteLine ("Now select the hashing method.\n");
				for (var i = 0; i < methods.Count; i++) {
					Console.WriteLine ("[{0}] {1}", i, methods.ElementAt (i).FriendlyName);
				}
				Console.Write ("\nYour choice> ");
				var input = Console.ReadLine ();
				int choice;
				if (!int.TryParse (input, out choice) || choice >= methods.Count)
					continue;
				valid_choice = true;
				options.method = methods.ElementAt (choice).Name;
			}
			valid_choice = false;
			while (!valid_choice) {
				Console.Clear ();
				Console.WriteLine ("Hashlecter Wizard\n");
				Console.WriteLine ("Input: {0}", Path.GetFileName (options.input_file));
				Console.WriteLine ("Method: {0}", methods.First (x => x.Name == options.method).FriendlyName);
				Console.WriteLine ("\nNow select the attacking method.\n");
				Console.WriteLine ("[0] Dictionary attack");
				Console.Write ("\nYour choice> ");
				var input = Console.ReadLine ();
				int choice;
				if (!int.TryParse (input, out choice))
					continue;
				valid_choice = true;
				switch (choice) {
					case 0:
						Console.Clear ();
						Console.WriteLine ("Hashlecter Wizard\n");
						Console.WriteLine ("Input: {0}", Path.GetFileName (options.input_file));
						Console.WriteLine ("Method: {0}", methods.First (x => x.Name == options.method).FriendlyName);
						Console.WriteLine ("\nDrag the dictionary file here.");
						options.input_dict = ReadSilent ();
						break;
					default:
						valid_choice = false;
						continue;
				}
			}
			Console.Clear ();
			Console.WriteLine ("Hashlecter Wizard\n");
			Console.WriteLine ("Input: {0}", Path.GetFileName (options.input_file));
			Console.WriteLine ("Method: {0}", methods.First (x => x.Name == options.method).FriendlyName);
			Console.WriteLine ("Dictionary: {0}", Path.GetFileName (options.input_dict));
			Console.WriteLine ("\nPress any key to start.");
			Console.Read ();
		}

		public static string ReadSilent () {
			var key = Console.ReadKey (true);
			var accum = new StringBuilder ();
			while (key.Key != ConsoleKey.Enter) {
				accum.Append (key.KeyChar);
				if (File.Exists (accum.ToString ())) {
					var file = Path.GetFileName (accum.ToString ());
					return Path.GetFullPath (accum.ToString ());
				}
				key = Console.ReadKey (true);
			}
			return accum.ToString ();
		}
	}
}
