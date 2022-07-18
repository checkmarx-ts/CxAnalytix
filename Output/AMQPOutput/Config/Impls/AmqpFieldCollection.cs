using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace CxAnalytix.Out.AMQPOutput.Config.Impls
{
	public class AmqpFieldCollection : ConfigurationElementCollection, ISet<AmqpFieldCollection.AmqpField>
	{
		public class AmqpField : ConfigurationElement
		{
			[ConfigurationProperty("Name", IsRequired = true)]
			public String Name
			{
				get => (String)base["Name"];
				internal set => base["Name"] = value;
			}

		}

		private HashSet<AmqpField> _set = new HashSet<AmqpField>();

		bool ICollection<AmqpField>.IsReadOnly => false;

		public bool Add(AmqpField item) => _set.Add(item);

		public void Clear() => _set.Clear();

		public bool Contains(AmqpField item) => _set.Contains(item);

		public void CopyTo(AmqpField[] array, int arrayIndex) => _set.CopyTo(array, arrayIndex);

		public void ExceptWith(IEnumerable<AmqpField> other) => _set.ExceptWith(other);

		public void IntersectWith(IEnumerable<AmqpField> other) => _set.IntersectWith(other);

		public bool IsProperSubsetOf(IEnumerable<AmqpField> other) => _set.IsProperSubsetOf(other);

		public bool IsProperSupersetOf(IEnumerable<AmqpField> other) => _set.IsProperSupersetOf(other);

		public bool IsSubsetOf(IEnumerable<AmqpField> other) => _set.IsSubsetOf(other);

		public bool IsSupersetOf(IEnumerable<AmqpField> other) => _set.IsSupersetOf(other);

		public bool Overlaps(IEnumerable<AmqpField> other) => _set.Overlaps(other);

		public bool Remove(AmqpField item) => _set.Remove(item);

		public bool SetEquals(IEnumerable<AmqpField> other) => _set.SetEquals(other);

		public void SymmetricExceptWith(IEnumerable<AmqpField> other) => _set.SymmetricExceptWith(other);

		public void UnionWith(IEnumerable<AmqpField> other) => _set.UnionWith(other);

		protected override ConfigurationElement CreateNewElement()
		{
			return new AmqpField();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return element;
		}

		void ICollection<AmqpField>.Add(AmqpField item) => _set.Add(item);

		IEnumerator<AmqpField> IEnumerable<AmqpField>.GetEnumerator() => _set.GetEnumerator();
	}
}
