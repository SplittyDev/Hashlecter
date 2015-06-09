#define MD5_COMPLETE
#define SHA_COMPLETE
#define EXP_COMPLETE

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Codeaddicts.libArgument;

namespace hashlecter
{
	class MainClass
	{
		#region Constants

		/// <summary>
		/// The default SQLite database path.
		/// </summary>
		public const string SQLITE_DB = "lecter.db";

		#endregion

		#region Statics

		/// <summary>
		/// The parsed command-line arguments.
		/// </summary>
		public static Options options;

		/// <summary>
		/// A hashset containing the hashing methods.
		/// </summary>
		static HashSet<HashingMethod> methods;

		/// <summary>
		/// The current session identifier.
		/// </summary>
		public static string session;

		/// <summary>
		/// The current SQLite database.
		/// </summary>
		public static SQLite db;

		#endregion

		/// <summary>
		/// The entry point of the program, where the program control starts and ends.
		/// </summary>
		/// <param name="args">The command-line arguments.</param>
		public static void Main (string[] args) {
			
			// Create a hashset for storing the hashing methods
			methods = new HashSet<HashingMethod> {
				
				#if MD5_COMPLETE
				HashingMethod.New<hMD5> (),
				HashingMethod.New<hMD5_Double> (),
				HashingMethod.New<hMD5_Salted> (),
				HashingMethod.New<hMD5_MyBB> (),
				#endif

				#if SHA_COMPLETE || SHA1_COMPLETE
				HashingMethod.New<hSHA1> (),
				HashingMethod.New<hSHA1_Double> (),
				#endif

				#if SHA_COMPLETE || SHA256_COMPLETE
				HashingMethod.New<hSHA256> (),
				HashingMethod.New<hSHA256_Double> (),
				#endif

				#if EXP_COMPLETE
				HashingMethod.New<hJHash> (),
				#endif
			};

			// Parse command-line arguments using libArgument
			options = ArgumentParser.Parse<Options> (args);

			// Use wizard if lecter was started directly
			options.wizard |= args.Length == 0;

			// Get or create a session identifier
			session = string.IsNullOrEmpty (options.session) ? GenerateSession () : options.session;

			// Connect to the SQLite database
			db = Database.Factory.CreateSQLite (SQLITE_DB);

			// Return if the user just wanted to make
			// sure that the db got created
			if (options.create_db) {
				Environment.Exit (0);
			}

			// Display help
			else if (options.help) {
				PreHelp ();
				ArgumentParser.Help ();
				Help ();
				Environment.Exit (0);
			}

			// Show all the cracked hashes
			else if (options.show) {
				db.Show (string.IsNullOrEmpty (options.session) ? string.Empty : session);
				Environment.Exit (0);
			}

			// Call the magician
			else if (options.wizard) {
				Wizard ();
			}

			// Null-check the method
			if (string.IsNullOrEmpty (options.method))

				// Fall back to MD5
				options.method = "md5";

			// Get the method used for cracking the hashes
			var method = methods.FirstOrDefault (m => m.Name == options.method.ToLowerInvariant ());

			// Check if the cracking method is valid
			if (method == null || method == default(HashingMethod)) {

				Console.Error.WriteLine ("Please specify a valid hashing method.");
				Console.Error.WriteLine ("Run lecter --help to get a list of valid hashing methods.");
				Environment.Exit (0);
			}

			// Check if we need to generate a hash
			if (options.gen) {

				// Read input string from -i/--input argument
				if (!string.IsNullOrEmpty (options.input_file))
					Console.WriteLine (method.Hash (options.input_file));
				
				// Hash an empty string if no string is given
				else
					Console.WriteLine (method.Hash (string.Empty));

				// Exit
				Environment.Exit (0);
			}

			string[] input_hashes = null;

			// Grab input from stdin
			if (options.input_stdin)
				input_hashes = FromStdin ();
			
			// Grab input from file
			else if (!string.IsNullOrEmpty (options.input_file))
				input_hashes = FromFile (options.input_file);

			// Perform dictionary attack
			if (!string.IsNullOrEmpty (options.input_dict)) {

				// Null-check the input file
				if (input_hashes == null) {
					Console.Error.WriteLine ("No input was specified.");
					Environment.Exit (0);
				}

				// Experimental lazy evaluation
				if (options.exp_lazy_eval)
					DictionaryAttack.RunLazy (input_hashes, method, options.input_dict);

				// Stable evaluation
				else
					DictionaryAttack.Run (input_hashes, method, options.input_dict);
			}

			// Perform bruteforce attack
			{
				// Null-check the input file
				if (input_hashes == null) {
					Console.Error.WriteLine ("No input was specified.");
					Environment.Exit (0);
				}

				string default_alphabet =
					BruteforceAttack.NUMERIC +
					BruteforceAttack.LOWER_ALPHANUMERIC;

				if (!string.IsNullOrEmpty (options.alphabet))
					BruteforceAttack.Run (input_hashes, method, options.alphabet, 5);
				else
					BruteforceAttack.Run (input_hashes, method, default_alphabet, 5);
			}

			if (!options.silent) {
				db.Show (session);
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
			Console.WriteLine ("\nHashing methods:");
			foreach (var method in methods) {
				var name = method.Name.PadRight (20, ' ');
				Console.WriteLine (string.Format ("{0}{1}", name, method.Format));
			}
			Console.WriteLine ();
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

		/// <summary>
		/// Reads a path.
		/// </summary>
		/// <returns>The path.</returns>
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
