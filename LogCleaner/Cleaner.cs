using log4net;
using System;
using System.IO;
using System.Threading.Tasks;

namespace LogCleaner
{
    public class Cleaner
    {

        private static ILog _log = LogManager.GetLogger(typeof(Cleaner));

        private Cleaner()
        { }

        public static void CleanOldFiles (String rootFolderPath, String searchPattern, int olderThanDays)
        {
            if (String.IsNullOrEmpty (rootFolderPath) || 
                String.IsNullOrEmpty(searchPattern) || olderThanDays < 0)
                return;


            DateTime deleteBefore = DateTime.Now.AddDays(-olderThanDays);

            var files = Directory.GetFiles(rootFolderPath, searchPattern,
                SearchOption.AllDirectories);


            Parallel.ForEach<String>(files, (file) =>
           {
               DateTime lastWrite = File.GetLastWriteTime(file);

           if (lastWrite.CompareTo(deleteBefore) < 0)
           {
                   _log.Info($"Deleting file {file} that was last touched on {lastWrite}.");
                   File.Delete(file);
               }
           });
        }

    }
}
