using System;

namespace CxAnalytix.Configuration.Contracts
{
	public interface ICxAnalytixService
	{
		bool EnablePseudoTransactions { get; }
		String InstanceIdentifier { get; }
		int ConcurrentThreads { get; }
		String StateDataStoragePath { get; }
		String OutputFactoryClassPath { get; }
		String OutputAssembly { get; }
		String OutputClass { get; }
		String SASTScanSummaryRecordName { get;  }
		String SASTScanDetailRecordName { get; }
		String SCAScanSummaryRecordName { get; }
		String SCAScanDetailRecordName { get; }
		String ProjectInfoRecordName { get; }
		String PolicyViolationsRecordName { get; }
		int ProcessPeriodMinutes { get; }
	}
}
