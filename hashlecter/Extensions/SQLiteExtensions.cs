using System;
using System.Collections.Generic;

namespace hashlecter
{
	public static class SQLiteExtensions
	{
		public static KeyValuePair<string, object> ToSQLiteParam (this object value, string key) {
			return new KeyValuePair<string, object> (key, value);
		}
	}
}

