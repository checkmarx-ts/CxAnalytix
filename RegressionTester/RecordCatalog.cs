using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace RegressionTester
{
	public class RecordCatalog
	{

		private Dictionary<String, LinkedList<String>> _comparableDupes = new Dictionary<string, LinkedList<string>>();

		private Dictionary<String, RecordLocator> _records = new Dictionary<String, RecordLocator>();

		private Dictionary<String, LinkedList<String>> _internalDupes = new Dictionary<string, LinkedList<string>>();

		public HashSet<String> ValueKeys { get; private set; }

		public IReadOnlyCollection<LinkedList<String>> Duplicates { get => _internalDupes.Values; }

		public IReadOnlyDictionary<String, RecordLocator> Records { get => _records; }

		public long RecordCount { get => _records.Count;  }

		public RecordCatalog()
		{
			ValueKeys = new HashSet<string>();
		}


		private static String Hash(String value)
		{
			using (var sha = SHA256.Create())
				return Convert.ToBase64String(sha.ComputeHash(UTF8Encoding.UTF8.GetBytes(value)));
		}

		private static String Hash(Dictionary<String, Object> dict, String[] filteredKeys = null)
		{
			StringBuilder sb = new StringBuilder();

			foreach (var key in dict.Keys)
			{
				if (filteredKeys != null && filteredKeys.Contains(key))
					continue;

				sb.Append(key);
				sb.Append(dict[key]);
			}

			return Hash(sb.ToString());
		}


		public class RecordLocator
		{
			public RecordLocator (String uniqueId, Dictionary<String, Object> record, String location)
			{
				Record = record;
				Location = location;
				UniqueId = uniqueId;
			}

			public Dictionary<String, Object> Record { get; private set; }

			public String Location { get; private set; }

			public String UniqueId { get; private set; }


			public String HashByKeys(IEnumerable<String> keys)
			{
				var keysToFilter = Record.Keys.Except(Record.Keys.Intersect(keys)).ToArray();

				return RecordCatalog.Hash(Record, keysToFilter);
			}
		}


		public void AddRecord (String uid, String location, Dictionary<String, Object> record)
		{
			_records.Add(uid, new RecordLocator(uid, record, location));

			var dupeHash = Hash(record);

			if (_internalDupes.ContainsKey(dupeHash))
				_internalDupes[dupeHash].AddLast(location);
			else
				_internalDupes.Add(dupeHash, new LinkedList<String>(new String[] { location }));
			
			foreach (var valueKey in record.Keys)
				ValueKeys.Add(valueKey);
		}


	}
}
