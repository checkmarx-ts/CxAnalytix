using CxAnalytix.Out.AMQPOutput.Config.Impls;
using System;

namespace CxAnalytix.Out.AMQPOutput.Config.Contracts
{
	public interface IAmqpConnectionConfig
	{
		String UserName { get; set; }
		String Password { get; set; }
		AmqpEndpointCollection Endpoints { get; set; }
	}
}
