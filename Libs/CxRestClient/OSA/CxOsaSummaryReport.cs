﻿using CxRestClient.Utility;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace CxRestClient.OSA
{
	public class CxOsaSummaryReport
	{
		private static ILog _log = LogManager.GetLogger(typeof(CxOsaSummaryReport));

		private static String URL_SUFFIX = "cxrestapi/osa/reports";

		private CxOsaSummaryReport()
		{ }

		public class ScanSummary
		{
			[JsonProperty(PropertyName = "totalLibraries")]
			public int TotalLibraries { get; internal set; }
			[JsonProperty(PropertyName = "highVulnerabilityLibraries")]
			public int HighVulnerabilityLibraries { get; internal set; }
			[JsonProperty(PropertyName = "mediumVulnerabilityLibraries")]
			public int MediumVulnerabilityLibraries { get; internal set; }
			[JsonProperty(PropertyName = "lowVulnerabilityLibraries")]
			public int LowVulnerabilityLibraries { get; internal set; }
			[JsonProperty(PropertyName = "nonVulnerableLibraries")]
			public int NonVulnerableLibraries { get; internal set; }
			[JsonProperty(PropertyName = "vulnerableAndUpdated")]
			public int VulnerableAndUpdated { get; internal set; }
			[JsonProperty(PropertyName = "vulnerableAndOutdated")]
			public int VulnerableAndOutdated { get; internal set; }
			[JsonProperty(PropertyName = "vulnerabilityScore")]
			public String VulnerabilityScore { get; internal set; }
			[JsonProperty(PropertyName = "totalHighVulnerabilities")]
			public int TotalHighVulnerabilities { get; internal set; }
			[JsonProperty(PropertyName = "totalMediumVulnerabilities")]
			public int TotalMediumVulnerabilities { get; internal set; }
			[JsonProperty(PropertyName = "totalLowVulnerabilities")]
			public int TotalLowVulnerabilities { get; internal set; }
		}


		private static ScanSummary ParseScanSummary(JToken jt)
		{
			using (var reader = new JTokenReader(jt))
			{
				JsonSerializer js = new JsonSerializer();
				return js.Deserialize(reader, typeof(ScanSummary)) as ScanSummary;
			}
		}


		public static ScanSummary GetReport(CxSASTRestContext ctx, CancellationToken token,
			String scanId)
		{
			String url = UrlUtils.MakeUrl(ctx.Sast.ApiUrl, URL_SUFFIX, new Dictionary<String, String>()
				{
				{"scanId", Convert.ToString (scanId)  }
				});

			return WebOperation.ExecuteGet<ScanSummary>(
				ctx.Sast.Json.CreateClient
				, (response) =>
				{
					using (var sr = new StreamReader(response.Content.ReadAsStreamAsync().Result))
					using (var jtr = new JsonTextReader(sr))
					{
						JToken jt = JToken.Load(jtr);
						return ParseScanSummary(jt);
					}
				}
				, url
				, ctx.Sast
				, token);
		}
	}
}
