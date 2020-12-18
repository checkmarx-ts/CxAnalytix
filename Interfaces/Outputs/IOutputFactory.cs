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
        /// A factory method for making a new instance of <see cref="IOutput"/>
        /// that can handle the particular record type.
        /// </summary>
        /// <param name="recordType">A string indicating the record type.</param>
        /// <returns></returns>
        IOutput newInstance(String recordType);
    }
}
