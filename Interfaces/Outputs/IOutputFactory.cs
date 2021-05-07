using System;
using System.Collections.Generic;
using System.Text;

namespace CxAnalytix.Interfaces.Outputs
{
    /// <summary>
    /// An interface that defines the factory implementation for IOutput
    /// instances.
    /// </summary>
    public interface IOutputFactory
    {
        /// <summary>
        /// Begins a transaction context to collect written records until such time
        /// as they are to be committed as a single write.
        /// </summary>
        /// <returns>An instance of a transaction context.</returns>
        IOutputTransaction StartTransaction();

        /// <summary>
        /// Registers a record with a specific name.
        /// </summary>
        /// <param name="recordName">The name of the record</param>
        /// <returns>A reference to the record that is used later to specify the record to which writes will occur.</returns>
        IRecordRef RegisterRecord(String recordName);

    }
}
