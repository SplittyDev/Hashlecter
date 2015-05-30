using System;

namespace hashlecter
{
	public static class Database
	{
		public static class Factory {

			public static SQLite CreateSQLite (string path) {
				var db = new SQLite ();
				db.Connect (path);
				db.Prepare ();
				return db;
			}

		}
	}
}

