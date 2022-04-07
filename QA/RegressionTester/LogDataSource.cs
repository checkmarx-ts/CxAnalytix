using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace RegressionTester
{
	public class LogDataSource<T> : IEnumerable<T>
	{
		private String[] _files;
		private String _currentLoc;
		private String _currentFile;

		public LogDataSource(String path, String fileMask)
		{
			var foundFiles = Directory.GetFiles(path, fileMask);

			List<String> filteredFiles = new List<string>();

			foreach (var filePath in foundFiles)
			{
				var fi = new FileInfo(filePath);

				if (fi.Length > 0)
					filteredFiles.Add(filePath);
			}

			_files = filteredFiles.ToArray();
		}

		private void ReportLocation (String fileName, long lineNumber)
		{
			_currentLoc = $"{fileName}:{lineNumber}";
			_currentFile = fileName;
		}

		public String Location { get => _currentLoc; }
		public String CurrentFile { get => _currentFile;  }

		private IEnumerator<String> RecordEnumerator { get => new JsonMultiRecordFileEnumerator(_files, ReportLocation); }

		public IEnumerator<T> GetEnumerator()
		{
			return new JsonObjectEnumerator<T>(RecordEnumerator);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private class JsonObjectEnumerator<JT> : IEnumerator<JT>
		{
			IEnumerator<String> _jsonRecords;
			JT _current;

			public JsonObjectEnumerator (IEnumerator<String> json)
			{
				_jsonRecords = json;
			}


			public JT Current => _current;

			object IEnumerator.Current => Current;

			public void Dispose()
			{
			}

			public bool MoveNext()
			{
				if (_jsonRecords.MoveNext())
				{
					_current = JsonConvert.DeserializeObject<JT>(_jsonRecords.Current);

					return true;
				}

				return false;
			}

			public void Reset()
			{
				_jsonRecords.Reset();
			}
		}

		private class JsonMultiRecordFileEnumerator : IEnumerator<String>
		{
			private String[] _files;
			private String _currentValue;
			private StreamReader _currentFile;
			private String _currentFileName;
			private long _currentFileLine;

			private Stack<String> _fileStack;

			private Action<String, long> _reportLoc;


			public JsonMultiRecordFileEnumerator (String[] files, Action<String, long> reportLoc)
			{
				_files = files;
				_reportLoc = reportLoc;
				Reset();
			}

			public string Current => _currentValue;

			object IEnumerator.Current => Current;

			public void Dispose()
			{
				if (_currentFile != null)
					_currentFile.Close();
			}

			public bool MoveNext()
			{
				if (_currentFile == null || _currentFile.EndOfStream)
					if (_fileStack.Count == 0)
						return false;
					else
					{
						if (_currentFile != null)
							_currentFile.Close();

						_currentFileName = _fileStack.Pop();
						_currentFileLine = 1;
						_currentFile = new StreamReader(_currentFileName);
					}

				_reportLoc(_currentFileName, _currentFileLine++);
				_currentValue = _currentFile.ReadLine();

				return true;
			}

			public void Reset()
			{
				_currentValue = null;

				if (_currentFile != null)
					_currentFile.Close();
				
				_currentFile = null;
				_currentFileLine = 1;
				_currentFileName = null;

				_fileStack = new Stack<string>(_files);
			}
		}
	}
}
