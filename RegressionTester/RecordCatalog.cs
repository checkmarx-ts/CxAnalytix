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

		private List<RecordLocator> _records = new List<RecordLocator>();

		private Dictionary<String, LinkedList<String>> _internalDupes = new Dictionary<string, LinkedList<string>>();

		public HashSet<String> ValueKeys { get; private set; }
		private String[] FilteredKeys { get; set; }

		public IReadOnlyCollection<LinkedList<String>> Duplicates { get => _internalDupes.Values; }


		public long RecordCount { get => _records.Count;  }

		public RecordCatalog(String[] filterKeys)
		{
			ValueKeys = new HashSet<string>();
			FilteredKeys = filterKeys;
		}

		public IReadOnlyDictionary<String, RecordLocator> GetRecordInfo(String[] filteredDataElements)
		{
			var retVal = new Dictionary<String, RecordLocator>();

			foreach (var recordLocator in _records)
			{
				var hash = Hash(recordLocator.Record, filteredDataElements);

				if (!retVal.ContainsKey(hash) )
					retVal.Add(hash, recordLocator);
			}

			return retVal;
		}

		private String Hash(String value)
		{
			using (var sha = SHA256.Create())
				return Convert.ToBase64String(sha.ComputeHash(UTF8Encoding.UTF8.GetBytes(value)));
		}

		private String Hash(Dictionary<String, String> dict, String[] additionalFilteredKeys = null)
		{
			StringBuilder sb = new StringBuilder();

			foreach (var key in dict.Keys)
			{
				if (FilteredKeys != null && FilteredKeys.Contains(key))
					continue;

				if (additionalFilteredKeys != null && additionalFilteredKeys.Contains(key))
					continue;

				sb.Append(key);
				sb.Append(dict[key]);
			}

			return Hash(sb.ToString());
		}


		public class RecordLocator
		{
			public RecordLocator (String uniqueId, Dictionary<String, String> record, String location)
			{
				Record = record;
				Location = location;
				UniqueId = uniqueId;
			}

			public Dictionary<String, String> Record { get; private set; }

			public String Location { get; private set; }

			public String UniqueId { get; private set; }
		}


		public void AddRecord (String uid, String location, Dictionary<String, String> record)
		{
			_records.Add(new RecordLocator(uid, record, location));

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
