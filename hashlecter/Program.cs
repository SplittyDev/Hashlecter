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

			// Perform dictionary attack
			if (!string.IsNullOrEmpty (options.input_dict))
				DictionaryAttack.Run (input_hashes, method, options.input_dict);

			// Perform bruteforce attack
			// TODO: Add bruteforce attack
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
