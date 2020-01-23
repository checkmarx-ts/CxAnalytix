using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CsvHelper;
using CxAPI_Core.dto;

namespace CxAPI_Core
{
    class csvHelper
    {
        public int writeCVSFile(List<csvScanOutput_1> projectList, resultClass token)
        {
            try
            {
                using (var writer = new StreamWriter(token.file_path + token.os_path + token.file_name))
                using (var csv = new CsvWriter(writer))
                {
                    csv.WriteRecords(projectList);
                }
            }
            catch(Exception ex)
            {
                Console.Error.Write(ex.ToString());
                return -1;
            }
            return 0;
        }
        public int writeCVSFile(List<csvScanOutput_2> projectList, resultClass token)
        {
            try
            {
                using (var writer = new StreamWriter(token.file_path + token.os_path + token.file_name))
                using (var csv = new CsvWriter(writer))
                {
                    csv.WriteRecords(projectList);
                }
            }
            catch (Exception ex)
            {
                Console.Error.Write(ex.ToString());
                return -1;
            }
            return 0;
        }
        public int writeCVSFile(List<ReportOutput> projectList, resultClass token)
        {
            try
            {
                using (var writer = new StreamWriter(token.file_path + token.os_path + token.file_name))
                using (var csv = new CsvWriter(writer))
                {
                    csv.WriteRecords(projectList);
                }
            }
            catch (Exception ex)
            {
                Console.Error.Write(ex.ToString());
                return -1;
            }
            return 0;
        }
        public int writeCVSFile(List<ReportOutputExtended> projectList, resultClass token)
        {
            try
            {
                using (var writer = new StreamWriter(token.file_path + token.os_path + token.file_name))
                using (var csv = new CsvWriter(writer))
                {
                    csv.WriteRecords(projectList);
                }
            }
            catch (Exception ex)
            {
                Console.Error.Write(ex.ToString());
                return -1;
            }
            return 0;
        }
        public int writeCVSFile(List<ReportResultExtended> projectList, resultClass token)
        {
            try
            {
                using (var writer = new StreamWriter(token.file_path + token.os_path + token.file_name))
                using (var csv = new CsvWriter(writer))
                {
                    csv.WriteRecords(projectList);
                }
            }
            catch (Exception ex)
            {
                Console.Error.Write(ex.ToString());
                return -1;
            }
            return 0;
        }
    }
}
