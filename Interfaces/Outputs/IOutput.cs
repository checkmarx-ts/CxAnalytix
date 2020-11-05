using System;
using System.Collections.Generic;
using System.Text;

namespace CxAnalytix.Interfaces.Outputs
{
    /// <summary>
    /// An interface that is used to output records to some destination
    /// specific to the implementation of IOutput.
    /// </summary>
    public interface IOutput
    {
        /// <summary>
        /// The method called with a key/value dictionary for writing into the appropriate record
        /// format.
        /// </summary>
        /// <param name="record">The record data.</param>
        void write(IDictionary<String, Object> record);
    }
}
