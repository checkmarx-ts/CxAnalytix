using System;
using System.Collections.Generic;
using System.Text;

namespace CxAnalytix.Out.AMQPOutput.Config.Contracts
{
	public interface IAmqpConfig
	{
		String Exchange { get; set; }
		AmqpRecordConfigCollection Records { get; set; }
	}
}
