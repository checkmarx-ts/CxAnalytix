using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using CxRestClient.Utility;

namespace CxRestClient.OSA
{
	public class CxOsaScans
	{
		private static ILog _log = LogManager.GetLogger(typeof(CxOsaScans));

		private static String URL_SUFFIX = "cxrestapi/osa/scans";
		private static readonly String PAGE_SIZE = "1000";


		private CxOsaScans()
		{ }

		public struct Scan
		{
			public int ProjectId { get; internal set; }
			public String ScanId { get; internal set; }
			public DateTime FinishTime { get; internal set; }
			public DateTime StartTime { get; internal set; }

			public override string ToString()
			{
				return JsonConvert.SerializeObject(this, Formatting.None);
			}
		}


		private class ScansReader : IEnumerable<Scan>, IEnumerator<Scan>, IDisposable
		{

			private JToken _json;
			private JTokenReader _reader;
			private int _projectId;
			internal ScansReader(JToken json, int projectId)
			{
				_json = json;
				_reader = new JTokenReader(_json);
				_projectId = projectId;
			}
			internal ScansReader()
			{
				_json = null;
			}

			public Scan Current => _currentScan;

			object IEnumerator.Current => Current;

			public void Dispose()
			{
				if (_reader != null)
				{
					_reader.Close();
					_reader = null;
				}
			}

			public IEnumerator<Scan> GetEnumerator()
			{
				if (_json == null)
					return new ScansReader();

				return new ScansReader(_json, _projectId);
			}

			Scan _currentScan = new Scan();

			public bool MoveNext()
			{
				while (_reader != null && JsonUtils.MoveToNextProperty(_reader))
				{
					if (((JProperty)_reader.CurrentToken).Name.CompareTo("id") == 0)
					{
						_currentScan = new Scan()
						{
							ProjectId = _projectId,
							ScanId = ((JProperty)_reader.CurrentToken).Value.ToString()
						};

						if (!JsonUtils.MoveToNextProperty(_reader, "startAnalyzeTime"))
							return false;

						// OSA stores times in UTC.  SAST stores them in local time.
						_currentScan.StartTime = DateTime.Parse(((JProperty)_reader.CurrentToken).
							Value.ToString()).ToLocalTime();

						if (!JsonUtils.MoveToNextProperty(_reader, "endAnalyzeTime"))
							return false;

						_currentScan.FinishTime = DateTime.Parse(((JProperty)_reader.CurrentToken).
							Value.ToString()).ToLocalTime();

						if (!JsonUtils.MoveToNextProperty(_reader, "state"))
							return false;

						if (!JsonUtils.MoveToNextProperty(_reader, "name"))
							return false;

						return true;
					}
				}
				return false;
			}

			public void Reset()
			{
				throw new NotImplementedException();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return new ScansReader(_json, _projectId);
			}

		}


		public static IEnumerable<Scan> GetScans(CxSASTRestContext ctx, CancellationToken token,
			int projectId)
		{
			int curPage = 1;
			List<Scan> osaScans = new List<Scan>();

			Func<int, String> url = (pg) => CxSASTRestContext.MakeUrl(ctx.Url, URL_SUFFIX, new Dictionary<String, String>()
				{
					{"projectId", Convert.ToString (projectId)  },
					{ "page", Convert.ToString (pg) },
					{ "itemsPerPage", PAGE_SIZE}
				});


			while (true)
			{
				if (token.IsCancellationRequested)
					return null;

				var beforeCount = osaScans.Count;


				using (var scans = WebOperation.ExecuteGet<ScansReader>(ctx.Json.CreateSastClient
				, (response) =>
				{
					using (var sr = new StreamReader(response.Content.ReadAsStreamAsync().Result))
					using (var jtr = new JsonTextReader(sr))
					{
						JToken jt = JToken.Load(jtr);
						return new ScansReader(jt, projectId);
					}

				}
				, url(curPage++)
				, ctx
				, token
				, (response) =>
				{
					if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
					{
						_log.Debug("403 response indicates OSA is not licensed for this instance.");
						return false;
					}

					return true;
				}))
				{
					if (scans != null)
						osaScans.AddRange(scans);

					if (osaScans.Count == beforeCount)
						break;
				}

			}

			return osaScans;
		}
	}
}
