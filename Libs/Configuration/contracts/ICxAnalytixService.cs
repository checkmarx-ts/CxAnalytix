using System;
using System.Collections.Generic;
using System.Text;

namespace CxAnalytix.Configuration.Contracts
{
	public interface ICxAnalytixService
	{
		bool EnablePseudoTransactions { get; set; }
		String InstanceIdentifier { get; set; }
		int ConcurrentThreads { get; set; }
		String StateDataStoragePath { get; set; }
		String OutputFactoryClassPath { get; set; }
		String OutputAssembly { get; }
		String OutputClass { get; }
		String SASTScanSummaryRecordName { get; set; }
		String SASTScanDetailRecordName { get; set; }
		String SCAScanSummaryRecordName { get; set; }
		String SCAScanDetailRecordName { get; set; }
		String ProjectInfoRecordName { get; set; }
		String PolicyViolationsRecordName { get; set; }
		int ProcessPeriodMinutes { get; set; }
	}
}
