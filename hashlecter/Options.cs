using System;
using Codeaddicts.libArgument.Attributes;

namespace hashlecter
{
	public class Options
	{
		[Switch]
		[Docs ("Displays this help")]
		public bool help;

		[Switch ("create-sqlite-db")]
		[Docs ("Creates the database")]
		public bool create_db;

		[Switch]
		[Docs ("Painless configuration")]
		public bool wizard;

		[Argument ("stdin")]
		[Docs ("Accept input from stdin")]
		public bool input_stdin;

		[Argument ("i", "infile")]
		[Docs ("Input file; one hash per line")]
		public string input_file;

		[Argument ("dict")]
		[Docs ("Input dictionary; one word per line")]
		public string input_dict;

		[Argument ("m", "method")]
		[Docs ("Hashing method")]
		public string method;

		[Argument ("s", "session")]
		[Docs ("(Optional) Session name")]
		public string session;

		[Switch ("show")]
		[Docs ("Show results")]
		public bool show;

		[Switch]
		[Docs ("Don't output anything to stdout")]
		public bool silent;

		[Argument ("v")]
		[Docs ("Output verbosity")]
		public string verbosity;

		public Verbosity Verbosity
		{
			get {
				Verbosity verb;
				return Enum.TryParse<Verbosity> (verbosity.ToLowerInvariant (), out verb)
					? verb : hashlecter.Verbosity.low;
			}
		}
	}
}

