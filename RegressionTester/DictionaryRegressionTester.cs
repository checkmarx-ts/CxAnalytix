using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace RegressionTester
{
	public abstract class DictionaryRegressionTester : IDisposable
	{

		private LogDataSource<Dictionary<String, String>> _old;
		private LogDataSource<Dictionary<String, String>> _new;


		protected DictionaryRegressionTester (String oldPath, String newPath)
		{
			_old = new LogDataSource<Dictionary<String, String>>(oldPath, FileMask);
			_new = new LogDataSource<Dictionary<String, String>>(newPath, FileMask);
		}

		private bool CanFormKey (String[] keys, Dictionary<String, String> values)
		{
			foreach (var element in keys)
				if (!values.ContainsKey(element))
					return false;

			return true;
		}

		private String BuildKey(String[] keys, Dictionary<String, String> values)
		{
			StringBuilder sb = new StringBuilder();
			foreach (var element in keys)
			{
				if (sb.Length > 0)
					sb.Append("::");
				sb.Append(values[element]);
			}

			return sb.ToString();
		}

		private async Task<RecordCatalog> Load(LogDataSource<Dictionary<String, String>> src)
		{
			return await Task<RecordCatalog>.Run(() =>
			{

				var retVal = new RecordCatalog(FilteredKeys);

				foreach (var record in src)
				{
					if (!CanFormKey(UniqueIdentifierKeys, record))
						OutputWriter.WriteLine($"Could not form unique key with elements {UniqueIdentifierKeys} at {src.Location}");

					String uniqueId = BuildKey(UniqueIdentifierKeys, record);
					retVal.AddRecord(uniqueId, src.Location, record);
				}

				return retVal;
			});
		}

		private void ReportDuplicateRecords (IEnumerable<LinkedList<String>>  dupeList)
		{


			foreach (var list in dupeList)
				if (list.Count > 1)
				{
					StringBuilder sb = new StringBuilder();

					foreach (var line in list)
					{
						if (sb.Length > 0)
							sb.Append(",");

						sb.Append(line);
					}

					OutputWriter.WriteLine($"Duplicate: {{{sb.ToString () }}}");
				}
		}

		private void Reconcile(RecordCatalog oldRecords, RecordCatalog newRecords)
		{
			OutputWriter.WriteLine($"START TEST: {TestName}");

			OutputWriter.WriteLine("*----+ Checking if there are duplicate records in the old data set +----*");
			ReportDuplicateRecords(oldRecords.Duplicates);

			OutputWriter.WriteLine("*----+ Checking if there are duplicate records in the new data set +----*");
			ReportDuplicateRecords(newRecords.Duplicates);

			OutputWriter.WriteLine("*----+ Checking for any record element differences +----*");
			foreach (var elem in oldRecords.ValueKeys.Except(newRecords.ValueKeys))
				OutputWriter.WriteLine($"{elem} appears in the old data set, but is not found in the new data set");

			var newFieldFilter = newRecords.ValueKeys.Except(oldRecords.ValueKeys).ToArray ();
			foreach (var elem in newFieldFilter)
				OutputWriter.WriteLine($"{elem} appears to be a new data element");


			OutputWriter.WriteLine("*----+ Checking that the number of records in old and new data sets are equal +----*");

			OutputWriter.WriteLine($"Old record correlation keys " +
				$"{oldRecords.RecordCount} == New record correlation keys {newRecords.RecordCount}");

			if (oldRecords.RecordCount != newRecords.RecordCount)
			{
				var oldRecordList = oldRecords.GetRecordInfo(newFieldFilter);
				var newRecordList = newRecords.GetRecordInfo(newFieldFilter);

				var diff = new List<String>(oldRecordList.Keys.Except(newRecordList.Keys));

				if (diff.Count > 0)
				{
					OutputWriter.WriteLine($"----+ {diff.Count} records missing from the new record  +----*");

					foreach (var key in diff)
						OutputWriter.WriteLine($"{oldRecordList[key].UniqueId} : {oldRecordList[key].Location}");
				}

				diff = new List<String>(newRecordList.Keys.Except(oldRecordList.Keys));

				if (diff.Count > 0)
				{
					OutputWriter.WriteLine($"----+ {diff.Count} records missing from the old record  +----*");

					foreach (var key in diff)
						OutputWriter.WriteLine($"{newRecordList[key].UniqueId} : {newRecordList[key].Location}");
				}
			}

			OutputWriter.WriteLine($"END TEST: {TestName}");
		}

		public async Task PerformTest ()
		{
			var oldLoad = Load(_old);
			var newLoad = Load(_new);

			await Task.Run(async () => { Reconcile(await oldLoad, await newLoad); });
		}

		public abstract void Dispose();

		protected abstract String FileMask { get; }

		protected abstract String[] UniqueIdentifierKeys { get; }

		protected abstract String[] FilteredKeys { get; }

		protected abstract String TestName { get; }

		protected abstract TextWriter OutputWriter { get;  }
	}
}
