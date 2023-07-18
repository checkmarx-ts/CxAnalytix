using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxAnalytix.Exceptions
{
    public class ScanCrawlException : Exception
    {
        private static String MakeMessage(String scanId, String projectName, String teamName) =>
            $"Exception caught processing scan [{scanId}] in [{projectName}]" +
                                        (!String.IsNullOrEmpty(teamName) ? $" assigned to team(s) [{teamName}]" : "");


        public ScanCrawlException (String scanId, String projectName, String teamName) 
            : base(ScanCrawlException.MakeMessage(scanId, projectName, teamName) )
        {

        }
        public ScanCrawlException(String scanId, String projectName, String teamName, Exception ex) 
            : base(ScanCrawlException.MakeMessage(scanId, projectName, teamName), ex)
        {

        }
    }
}
