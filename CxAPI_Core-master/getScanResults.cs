using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using CxAPI_Core.dto;
using System.IO;
using System.Xml.Serialization;
using System.Xml.Linq;

namespace CxAPI_Core
{
    class getScanResults
    {
        public XElement GetResult(long report_id, resultClass token)
        {
            try
            {
                get httpGet = new get();

                secure token_secure = new secure(token);
                token_secure.findToken(token);
                string path = token_secure.get_rest_Uri(String.Format(CxConstant.CxReportFetch, report_id));
                httpGet.get_Http(token, path);
                if (token.status == 0)
                {
                    string result = token.op_result;
                    XElement xl = XElement.Parse(result);
                    return xl;
                }
            }
            catch (Exception ex)
            {
                token.status = -1;
                token.statusMessage = ex.Message;
            }
            return null;
        }
        public byte[] GetGenaricResult(long report_id, resultClass token)
        {
            try
            {
                get httpGet = new get();

                secure token_secure = new secure(token);
                token_secure.findToken(token);
                string path = token_secure.get_rest_Uri(String.Format(CxConstant.CxReportFetch, report_id));
                httpGet.get_Http(token, path);
                if (token.status == 0)
                {
                    return token.byte_result;
                }
            }
            catch (Exception ex)
            {
                token.status = -1;
                token.statusMessage = ex.Message;
            }
            return null;
        }

        public bool GetResultStatus(long report_id, resultClass token)
        {
            try
            {
                get httpGet = new get();
                secure token_secure = new secure(token);
                token_secure.findToken(token);
                string path = token_secure.get_rest_Uri(String.Format(CxConstant.CxReportStatus, report_id));
                httpGet.get_Http(token, path);
                if (token.status == 0)
                {
                    ReportReady ready = JsonConvert.DeserializeObject<ReportReady>(token.op_result);
                    if (ready.Status.Id == 2)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                token.status = -1;
                token.statusMessage = ex.Message;
            }
            return false;
        }
        public ReportResult SetResultRequest(long scan_id,string report_type, resultClass token)
        {
            try
            {
                ReportRequest request = new ReportRequest()
                {
                     reportType = report_type,
                     scanId = scan_id
                };

                post Post = new post();
                secure token_secure = new secure(token);
                token_secure.findToken(token);
                string path = token_secure.post_rest_Uri(CxConstant.CxReportRegister);
                Post.post_Http(token, path, request);
                if (token.status == 0)
                {
                    ReportResult report = JsonConvert.DeserializeObject<ReportResult>(token.op_result);
                    return report;
                }
            }
            catch (Exception ex)
            {
                token.status = -1;
                token.statusMessage = ex.Message;
            }
            return null;
        }
    }
}

