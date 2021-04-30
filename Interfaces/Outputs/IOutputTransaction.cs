using System;
using System.Collections.Generic;
using System.Text;

namespace CxAnalytix.Interfaces.Outputs
{
/// <summary>
/// An instance of the selected output method that will allow a transactional write of data.
/// 
/// The implementation should avoid making written data visible until a call to Commit.  If the instance
/// is disposed before Commit is called, any write operations should be rolled back.
/// </summary>
	public interface IOutputTransaction : IDisposable
	{
		/// <summary>
		/// Writes data to a given record in the transaction context.
		/// </summary>
		/// <param name="which">The record where the data is written.</param>
		/// <param name="record">The record to write.</param>
		void write(IRecordRef which, IDictionary<String, Object> record);

		/// <summary>
		/// Finalizes the writes made to the transaction context, making them visible in the
		/// system where the records are to be written.
		/// </summary>
		/// <returns>True if the commit is a success, false otherwise.</returns>
		bool Commit();

		/// <summary>
		/// Produces an identifier for the transaction.
		/// </summary>
		String TransactionId { get; }

	}
}
