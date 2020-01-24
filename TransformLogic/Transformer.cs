using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace CxAnalytics.TransformLogic
{
    /// <summary>
    /// A class that implements the data transformation.
    /// </summary>
    public class Transformer
    {
        // TODO: Pass in the configuration parameters collected from the caller (via app.config,
        // command line options, or other methods of collecting config data)
        /// <summary>
        /// The main logic for invoking a transformation.  It does not return until a sweep
        /// for new scans is performed across all projects.
        /// </summary>
        /// <param name="concurrentThreads">The number of concurrent scan transformation threads.</param>
        /// <param name="outFactory">The factory implementation for making IOutput instances
        /// used for outputting various record types.</param>
        /// <param name="token">A cancellation token that can be used to stop processing of data if
        /// the task needs to be interrupted.</param>
        public static void doTransform (int concurrentThreads, IOutputFactory outFactory, CancellationToken token)
        {
            Thread.Sleep(2500);

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
