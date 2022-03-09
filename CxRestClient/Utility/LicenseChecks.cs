using CxRestClient.OSA;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;

namespace CxRestClient.Utility
{
	public class LicenseChecks
	{

        private static readonly string LICENSE_SUMMARY_API_PATH = "/cxwebclient/api/license/getlicensesummarydata";
        private static readonly string OSA_EXTENSIONS_API_PATH = "/cxrestapi/osa/fileextensions";


        [JsonObject(MemberSerialization.OptIn)]
        private class LicenseSummary
        {
            [JsonProperty(PropertyName = "isOsaEnabled")]
            public bool OsaEnabled { get; internal set; }

            [JsonProperty(PropertyName = "isSuccesfull")]
            public bool RequestSuccess { get; internal set; }

            [JsonProperty(PropertyName = "errorMessage")]
            public String ErrorMessage { get; internal set; }
        }

        // The ability to get license summary data may not work due to permissions
        // or a version that doesn't support it.  If this results in an error, try
        // to call an OSA API - maybe /osa/fileextensions will produce a 403 if there is no license.

        private static ILog _log = LogManager.GetLogger(typeof(LicenseChecks));


        private static bool CheckIsOsaEnabledFromAPIResponseCode(CxRestContext ctx, CancellationToken token)
		{
            // 403 is returned when OSA is not licensed.
			return WebOperation.ExecuteGet<bool>(
			ctx.Xml.CreateSastClient
			, (response) =>
			{
                if (response.IsSuccessStatusCode)
                    return true;
                else
                    return false;

			}
			, CxRestContext.MakeUrl(ctx.Url, OSA_EXTENSIONS_API_PATH)
			, ctx, token, responseErrorLogic: (response) => 
            {
                return !(response.StatusCode == System.Net.HttpStatusCode.Forbidden);
            });
		}

        private static bool CheckIsOsaEnabledFromLicenseSummary (CxRestContext ctx, CancellationToken token)
		{
            var summary = WebOperation.ExecuteGet<LicenseSummary>(
            ctx.Json.CreateSastClient
            , (response) =>
            {
                using (var sr = new StreamReader(response.Content.ReadAsStreamAsync().Result))
                using (var jtr = new JsonTextReader(sr))
                {
                    return (LicenseSummary)new JsonSerializer().Deserialize(jtr, typeof(LicenseSummary));
                }
            }
            , CxRestContext.MakeUrl(ctx.Url, LICENSE_SUMMARY_API_PATH)
            , ctx, token);


            if (summary != null)
			{
                if (!summary.RequestSuccess)
                    throw new InvalidOperationException($"License retrieval failed: {summary.ErrorMessage}");
                else
                    return summary.OsaEnabled;
			}
            else
                throw new InvalidDataException("License summary not available.");
		}


		public static bool OsaIsNotLicensed(CxRestContext ctx, CancellationToken token)
		{
			_log.Info("Detecting OSA License...");

            bool osaEnabled = false;

            try
            {
                osaEnabled = CheckIsOsaEnabledFromLicenseSummary(ctx, token);
            }
            catch (Exception)
            {
                osaEnabled = CheckIsOsaEnabledFromAPIResponseCode(ctx, token);
            }

            _log.Info($"OSA is Licensed: {osaEnabled}");

			return !osaEnabled;
		}
	}
}
