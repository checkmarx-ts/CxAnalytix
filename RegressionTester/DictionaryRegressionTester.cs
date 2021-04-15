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

		private LogDataSource<Dictionary<String, Object>> _old;
		private LogDataSource<Dictionary<String, Object>> _new;


		public DictionaryRegressionTester (String oldPath, String newPath)
		{
			_old = new LogDataSource<Dictionary<String, Object>>(oldPath, FileMask);
			_new = new LogDataSource<Dictionary<String, Object>>(newPath, FileMask);

			OutputWriter = new StreamWriter($"{TestName}-results.txt");

		}

		private bool CanFormKey (String[] keys, Dictionary<String, Object> values)
		{
			foreach (var element in keys)
				if (!values.ContainsKey(element))
					return false;

			return true;
		}

		private String Stringify (String[] values, String separator)
		{
			StringBuilder sb = new StringBuilder();
			foreach (var element in values)
			{
				if (sb.Length > 0)
					sb.Append(separator);
				sb.Append(element);
			}

			return sb.ToString();
		}

		private String BuildKey(String[] keys, Dictionary<String, Object> values)
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

		private async Task<RecordCatalog> Load(LogDataSource<Dictionary<String, Object>> src)
		{
			return await Task<RecordCatalog>.Run(() =>
			{

				var retVal = new RecordCatalog();

				foreach (var record in src)
				{
					if (!CanFormKey(UniqueIdentifierKeys, record))
					{ 
						OutputWriter.WriteLine($"Could not form unique key with elements {Stringify(UniqueIdentifierKeys, ",")} at {src.Location}");
						continue;
					}

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
			OutputWriter.WriteLine($"Unique key format: {Stringify(UniqueIdentifierKeys, "::")}");
			if (FilteredKeys.Length > 0)
				OutputWriter.WriteLine($"Elements not used in record equality evaluation: {Stringify(FilteredKeys, ",")}");


			OutputWriter.WriteLine("*----+ Checking if there are duplicate records in the old data set +----*");
			ReportDuplicateRecords(oldRecords.Duplicates);

			OutputWriter.WriteLine("*----+ Checking if there are duplicate records in the new data set +----*");
			ReportDuplicateRecords(newRecords.Duplicates);

			OutputWriter.WriteLine("*----+ Checking for any record element differences +----*");
			foreach (var elem in oldRecords.ValueKeys.Except(newRecords.ValueKeys))
				OutputWriter.WriteLine($"{elem} appears in the old data set, but is not found in the new data set");

			var newFieldFilter = newRecords.ValueKeys.Except(oldRecords.ValueKeys);
			foreach (var elem in newFieldFilter)
				OutputWriter.WriteLine($"{elem} appears to be a new data element");


			OutputWriter.WriteLine("*----+ Checking that the number of records in old and new data sets are equal +----*");

			OutputWriter.WriteLine($"Old records" +
				$"{oldRecords.RecordCount}, New records {newRecords.RecordCount}");

			if (oldRecords.RecordCount != newRecords.RecordCount)
			{

				var diff = new List<String>(oldRecords.Records.Keys.Except(newRecords.Records.Keys));

				if (diff.Count > 0)
				{
					OutputWriter.WriteLine($"----+ {diff.Count} records missing from the new record  +----*");

					foreach (var key in diff)
						OutputWriter.WriteLine($"{oldRecords.Records[key].UniqueId} : {oldRecords.Records[key].Location}");
				}

				diff = new List<String>(newRecords.Records.Keys.Except(oldRecords.Records.Keys));

				if (diff.Count > 0)
				{
					OutputWriter.WriteLine($"----+ {diff.Count} records missing from the old record  +----*");

					foreach (var key in diff)
						OutputWriter.WriteLine($"{newRecords.Records[key].UniqueId} : {newRecords.Records[key].Location}");
				}
			}

			OutputWriter.WriteLine("*----+ Verifying records match in data sets +----*");

			foreach (var recordId in oldRecords.Records.Keys)
			{
				if (!newRecords.Records.ContainsKey(recordId) )
				{
					OutputWriter.WriteLine($"Record {recordId} not found in the new data set");
					continue;
				}

				var comparisonElements = oldRecords.Records[recordId].Record.Keys.Except(FilteredKeys);
				var oldHash = oldRecords.Records[recordId].HashByKeys(comparisonElements);
				var newHash = newRecords.Records[recordId].HashByKeys(comparisonElements);

				if (oldHash.CompareTo(newHash) != 0)
					OutputWriter.WriteLine($"{recordId} did not match");
			}


			OutputWriter.WriteLine($"END TEST: {TestName}");
		}

		public async Task PerformTest ()
		{
			var oldLoad = Load(_old);
			var newLoad = Load(_new);

			await Task.Run(async () => { Reconcile(await oldLoad, await newLoad); });
		}

		public void Dispose()
		{
			if (OutputWriter != null)
			{
				OutputWriter.Flush();
				OutputWriter.Close();
			}
		}

		protected abstract String FileMask { get; }

		protected abstract String[] UniqueIdentifierKeys { get; }

		protected abstract String[] FilteredKeys { get; }

		protected abstract String TestName { get; }

		private TextWriter OutputWriter { get; set; }
	}
}
