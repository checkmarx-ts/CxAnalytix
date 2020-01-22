using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace CxAnalytics.TransformLogic
{
    public class Transformer
    {
        // TODO: Pass in the configuration parameters collected from the caller (via app.config,
        // command line options, or other methods of collecting config data)
        // TODO: May need to pass a thread interupt object so that an in-progress transformation can be 
        // cancelled when the service is shut down
        public static void doTransform (int concurrentThreads, CancellationToken token)
        {
            // TODO: This method manages object lifecycle and threads to perform the extract and transform

            /* TODO:
             * Basic algorithm for scans (can be applied to SAST and OSA with adjustments as needed)
             * 
             * 1. Load the persistent state data to obtain the list of projects and the date/id of last scan
             * that was processed for the project.  Initially this will be empty.
             * 2. Load the list of projects from the REST service.  Remove projects from the persisted data that are no longer
             * found in the list of projects obtained from the REST service.  Add new projects to the persisted data.
             * 3. Using concurrent threads, load the list of "finished" scans for each project.
             * 4. Find the new scans for each project that have not been processed.  Filter out any private or scan subsets.
             * Add the remaining full or incremental scan to a list of scans to process.
             * 5. Using multiple threads, load results and transform them into appropriate record output formats.
             * 
             */
            
        }
    }
}
