using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace CxAnalytix.Out.AMQPOutput.Config.Impls
{
	public class AmqpEndpointCollection : ConfigurationElementCollection, IList<AmqpTcpEndpoint>
	{

		private List<AmqpTcpEndpoint> _endpoints = new List<AmqpTcpEndpoint>();

		public AmqpTcpEndpoint this[int index] { get => _endpoints[index]; set => _endpoints[index] = value; }

		bool ICollection<AmqpTcpEndpoint>.IsReadOnly => false;

		public void Add(AmqpTcpEndpoint item)
		{
			_endpoints.Add(item);
		}

		public void Clear()
		{
			_endpoints.Clear();
		}

		public bool Contains(AmqpTcpEndpoint item)
		{
			return _endpoints.Contains(item);
		}

		public void CopyTo(AmqpTcpEndpoint[] array, int arrayIndex)
		{
			_endpoints.CopyTo(array, arrayIndex);
		}

		public int IndexOf(AmqpTcpEndpoint item)
		{
			return _endpoints.IndexOf(item);
		}

		public void Insert(int index, AmqpTcpEndpoint item)
		{
			_endpoints.Insert(index, item);
		}

		public bool Remove(AmqpTcpEndpoint item)
		{
			return _endpoints.Remove(item);
		}

		public void RemoveAt(int index)
		{
			_endpoints.RemoveAt(index);
		}

		protected override ConfigurationElement CreateNewElement()
        {
            return new AmqpEndpointConfig();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
			return element;
        }

		protected override void PostDeserialize()
		{
			base.PostDeserialize();

			foreach (AmqpEndpointConfig e in this)
			{
				_endpoints.Add(new AmqpTcpEndpoint(new Uri(e.AmqpUri), e.SSL));
			}
		}


		IEnumerator<AmqpTcpEndpoint> IEnumerable<AmqpTcpEndpoint>.GetEnumerator()
		{
			return _endpoints.GetEnumerator();
		}
	}
}
