using System;
using System.Collections.Generic;
using System.Text;
using CxAPI_Core.dto;
using Newtonsoft.Json;

namespace CxAPI_Core
{
    class getScans
    {
        public List<ScanObject> getScan(resultClass token)
        {
            get httpGet = new get();
            List<ScanObject> sclass = new List<ScanObject>();
            secure token_secure = new secure(token);
            token_secure.findToken(token);
            string path = token_secure.get_rest_Uri(CxConstant.CxScans);
            httpGet.get_Http(token, path);
            if (token.status == 0)
            {
                sclass = JsonConvert.DeserializeObject<List<ScanObject>>(token.op_result);
            }
            else {

                throw new MissingFieldException("Failure to get scan results. Please check token validity and try again");
            }
            return sclass;
        }

        public ScanStatistics getScansStatistics(long scanId,resultClass token)
        {
            get httpGet = new get();
            ScanStatistics scanStatistics = new ScanStatistics();
            secure token_secure = new secure(token);
            token_secure.findToken(token);
            string path = token_secure.get_rest_Uri(String.Format(CxConstant.CxScanStatistics,scanId));
            httpGet.get_Http(token, path);
            if (token.status == 0)
            {
                scanStatistics = JsonConvert.DeserializeObject<ScanStatistics>(token.op_result);
            }
            return scanStatistics;
        }

    }
}
