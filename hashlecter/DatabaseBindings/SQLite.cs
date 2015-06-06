using System;
using System.Linq;
using System.Data.SQLite;
using System.Collections.Generic;

namespace hashlecter
{
	public class SQLite : IDisposable
	{
		SQLiteConnection con;

		#region Query Constants

		const string TABLE_LAYOUT =
			"CREATE TABLE IF NOT EXISTS collisions (" +
			"id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
			"type VARCHAR(64) NOT NULL," +
			"hash VARCHAR(128) NOT NULL," +
			"text VARCHAR(128) NOT NULL" +
			");" +
			"CREATE TABLE IF NOT EXISTS sessions (" +
			"id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
			"session VARCHAR(64) NOT NULL," +
			"collision VARCHAR(128) NOT NULL," +
			"FOREIGN KEY (collision) REFERENCES collisions (hash)" +
			");";

		const string SELECT_COLLISION_ID =
			"SELECT id FROM collisions WHERE hash = @hash LIMIT 0, 1";

		const string SELECT_SESSION_ID =
			"SELECT sessions.id FROM sessions " +
			"INNER JOIN collisions ON sessions.collision = collisions.hash " +
			"WHERE sessions.session = @session LIMIT 0, 1";

		const string SELECT_ALL_COLLISIONS =
			"SELECT * FROM collisions ORDER BY id ASC";

		const string SELECT_ALL_COLLISIONS_FROM_SESSION =
			"SELECT collisions.id, hash, text, type FROM collisions " +
			"INNER JOIN sessions ON collisions.hash = sessions.collision " +
			"WHERE sessions.session = @session " +
			"ORDER BY collisions.id ASC";

		const string INSERT_COLLISION =
			"INSERT INTO collisions (hash, text, type) " +
			"VALUES (@hash, @text, @type);" +
			"INSERT INTO sessions (session, collision) " +
			"VALUES (@session, @hash);";

		const string INSERT_COLLISION_FROM_SESSION =
			"INSERT INTO sessions (session, collision) " +
			"VALUES (@session, @hash);";

		#endregion

		public void Connect (string source) {
			con = new SQLiteConnection ();
			con.ConnectionString = string.Format ("Data Source={0}", source);
			con.Open ();
		}

		public void Prepare () {
			ExecNonQuery (TABLE_LAYOUT);
		}

		public void Add (string session, string hash, string text, HashingMethod method) {
			using (var reader = ExecReader (SELECT_COLLISION_ID, hash.ToSQLiteParam ("@hash"))) {
				if (!reader.HasRows) {
					ExecNonQuery (
						INSERT_COLLISION,
						hash.ToSQLiteParam ("@hash"),
						text.ToSQLiteParam ("@text"),
						session.ToSQLiteParam ("@session"),
						method.Algorithm.ToString ().ToSQLiteParam ("@type")
					);
				}
				else {
					using (var reader2 = ExecReader (SELECT_SESSION_ID, session.ToSQLiteParam ("@session"))) {
						if (!reader2.HasRows) {
							ExecNonQuery (
								INSERT_COLLISION_FROM_SESSION,
								session.ToSQLiteParam ("@session"),
								hash.ToSQLiteParam ("@hash")
							);
						}
					}
				}
			}
		}

		public void Show (string session = "") {
			SQLiteDataReader reader;
			if (string.IsNullOrEmpty (session))
				reader = ExecReader (SELECT_ALL_COLLISIONS);
			else
				reader = ExecReader (SELECT_ALL_COLLISIONS_FROM_SESSION, session.ToSQLiteParam ("@session"));
			using (reader) {
				if (!reader.HasRows) {
					Console.WriteLine ("No results.");
					return;
				}
				Console.WriteLine ("{0}{1}{2}{3}",
					"ID".PadRight (6),
					"Method".PadRight (12),
					"Hash".PadRight (32),
					"Text".PadRight (20)
				);
				while (reader.Read ()) {
					var id = reader["id"].ToString ().PadRight (6);
					var method = reader["type"].ToString ().PadRight (12);
					method = method.Length >= 12 ? new string (method.Take (11).ToArray ()).PadRight (12) : method.PadRight (12);
					var hash = reader["hash"].ToString ();
					hash = hash.Length >= 16 ? new string (hash.Take (31).ToArray ()).PadRight (32) : hash.PadRight (32);
					var text = reader["text"].ToString ();
					text = text.Length >= 20 ? new string (text.Take (19).ToArray ()).PadRight (20) : text.PadRight (20);
					Console.WriteLine ("{0}{1}{2}{3}", id, method, hash, text);
				}
			}
		}

		SQLiteCommand CreateCommand (string query, params KeyValuePair<string, object>[] args) {
			var com = con.CreateCommand ();
			com.CommandText = query;
			foreach (var kvp in args)
				com.Parameters.Add (new SQLiteParameter (kvp.Key, kvp.Value));
			com.Prepare ();
			return com;
		}

		void ExecNonQuery (string query, params KeyValuePair<string, object>[] args) {
			CreateCommand (query, args).ExecuteNonQuery ();
		}

		object ExecScalar (string query, params KeyValuePair<string, object>[] args) {
			return CreateCommand (query, args).ExecuteScalar ();
		}

		SQLiteDataReader ExecReader (string query, params KeyValuePair<string, object>[] args) {
			return CreateCommand (query, args).ExecuteReader ();
		}

		#region IDisposable implementation

		public void Dispose () {
			con.Close ();
			con.Shutdown ();
		}

		#endregion
	}
}

