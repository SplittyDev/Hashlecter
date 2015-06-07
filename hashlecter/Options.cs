using System;
using Codeaddicts.libArgument.Attributes;

namespace hashlecter
{
	public class Options
	{
		// d: dict
		// i: input
		// m: method
		// s: session
		// r: round
		// v: verbosity

		#region Stable

		[Switch]
		[Docs ("Displays this help")]
		public bool help;

		[Switch ("create-sqlite-db")]
		[Docs ("Creates the database")]
		public bool create_db;

		[Switch ("stdin")]
		[Docs ("Accept input from stdin")]
		public bool input_stdin;

		[Argument ("i", "infile")]
		[Docs ("Input file; one hash per line")]
		public string input_file;

		[Argument ("d", "dict")]
		[Docs ("Input dictionary; one word per line")]
		public string input_dict;

		[Argument ("s", "session")]
		[Docs ("(Optional) Session name")]
		public string session;

		[Argument ("m", "method")]
		[Docs ("Hashing method")]
		public string method;

		[Argument ("r", "rounds")]
		[Docs ("Custom n-round hashing")]
		public int rounds;

		#endregion

		#region Testing

		[Switch]
		[Docs ("Painless configuration")]
		public bool wizard;

		[Switch ("show")]
		[Docs ("Show results")]
		public bool show;

		[Switch]
		[Docs ("Don't output anything to stdout")]
		public bool silent;

		#endregion

		#region Experimental / Unstable

		[Switch ("exp-lazy-eval")]
		[Docs ("EXPERIMENTAL: Lazy evaluation")]
		public bool exp_lazy_eval;

		[Switch ("exp-single-cont")]
		[Docs ("Continuous single-hash mode")]
		public bool exp_single_cont;

		#endregion

		/*
		[Argument ("v")]
		[Docs ("Output verbosity")]
		public string verbosity;
		*/

		/*
		public Verbosity Verbosity
		{
			get {
				if (silent)
					return hashlecter.Verbosity.silent;
				Verbosity verb;
				return Enum.TryParse<Verbosity> (verbosity.ToLowerInvariant (), out verb)
					? verb : hashlecter.Verbosity.low;
			}
		}
		*/
	}
}

