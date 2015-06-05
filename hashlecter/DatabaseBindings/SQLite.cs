using System;
using System.Linq;
using System.Data.SQLite;
using System.Collections.Generic;

namespace hashlecter
{
	public class SQLite : IDisposable
	{
		SQLiteConnection con;

		public void Connect (string source) {
			con = new SQLiteConnection ();
			con.ConnectionString = string.Format ("Data Source={0}", source);
			con.Open ();
		}

		public void Prepare () {
			ExecNonQuery (
				"CREATE TABLE IF NOT EXISTS collisions (" +
					"id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
					"hash VARCHAR(128) NOT NULL," +
					"text VARCHAR(128) NOT NULL" +
				");" +
				"CREATE TABLE IF NOT EXISTS sessions (" +
					"id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
					"session VARCHAR(64) NOT NULL," +
					"collision VARCHAR(128) NOT NULL," +
					"FOREIGN KEY (collision) REFERENCES collisions (hash)" +
				");"
			);
		}

		public void Add (string session, string hash, string text) {
			using (var reader = ExecReader ("SELECT id FROM collisions WHERE hash = @hash LIMIT 0, 1", hash.ToSQLiteParam ("@hash"))) {
				if (!reader.HasRows) {
					ExecNonQuery (
						"INSERT INTO collisions (hash, text) " +
						"VALUES (@hash, @text);" +
						"INSERT INTO sessions (session, collision) " +
						"VALUES (@session, @hash);",
						hash.ToSQLiteParam ("@hash"),
						text.ToSQLiteParam ("@text"),
						session.ToSQLiteParam ("@session")
					);
				}
				else {
					using (var reader2 = ExecReader (
						"SELECT sessions.id FROM sessions " +
						"INNER JOIN collisions ON sessions.collision = collisions.hash " +
						"WHERE sessions.session = @session LIMIT 0, 1",
						session.ToSQLiteParam ("@session")
					)) {
						if (!reader2.HasRows) {
							ExecNonQuery (
								"INSERT INTO sessions (session, collision) " +
								"VALUES (@session, @hash);",
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
				reader = ExecReader (
					"SELECT id, hash, text FROM collisions ORDER BY id ASC"
				);
			else
				reader = ExecReader (
					"SELECT collisions.id, hash, text FROM collisions " +
						"INNER JOIN sessions ON collisions.hash = sessions.collision " +
						"WHERE sessions.session = @session " +
						"ORDER BY collisions.id ASC",
					session.ToSQLiteParam ("@session"));
			using (reader) {
				if (!reader.HasRows) {
					Console.WriteLine ("No results.");
					return;
				}
				Console.WriteLine ("{0}{1}{2}",
					"ID".PadRight (6),
					"Hash".PadRight (16),
					"Text".PadRight (20)
				);
				while (reader.Read ()) {
					var id = reader["id"].ToString ().PadRight (6);
					var hash = reader["hash"].ToString ();
					hash = hash.Length >= 16 ? new string (hash.Take (15).ToArray ()).PadRight (16) : hash.PadRight (16);
					var text = reader["text"].ToString ();
					text = text.Length >= 20 ? new string (text.Take (19).ToArray ()).PadRight (20) : text.PadRight (20);
					Console.WriteLine ("{0}{1}{2}", id, hash, text);
				}
			}
		}

		void ExecNonQuery (string query, params KeyValuePair<string, object>[] args) {
			var com = con.CreateCommand ();
			com.CommandText = query;
			foreach (var kvp in args)
				com.Parameters.Add (new SQLiteParameter (kvp.Key, kvp.Value));
			com.Prepare ();
			com.ExecuteNonQuery ();
		}

		object ExecScalar (string query, params KeyValuePair<string, object>[] args) {
			var com = con.CreateCommand ();
			com.CommandText = query;
			foreach (var kvp in args)
				com.Parameters.Add (new SQLiteParameter (kvp.Key, kvp.Value));
			com.Prepare ();
			return com.ExecuteScalar ();
		}

		SQLiteDataReader ExecReader (string query, params KeyValuePair<string, object>[] args) {
			var com = con.CreateCommand ();
			com.CommandText = query;
			foreach (var kvp in args)
				com.Parameters.Add (new SQLiteParameter (kvp.Key, kvp.Value));
			com.Prepare ();
			return com.ExecuteReader ();
		}

		#region IDisposable implementation

		public void Dispose () {
			con.Close ();
			con.Shutdown ();
		}

		#endregion
	}
}

