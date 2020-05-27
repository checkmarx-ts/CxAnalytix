using System;
using System.Collections.Generic;
using System.Text;

namespace CxRestClient
{
    public struct FormattedDateTime : IComparable, IComparable<FormattedDateTime>
    {
        private static readonly String DATE_FORMAT = "yyyy-MM-ddTHH:mm:ss.fffzzz";

        public DateTime Value { get; private set; }

        public FormattedDateTime (DateTime dt)
        {
            Value = dt;
        }
        public FormattedDateTime(String dt)
        {
            Value = DateTime.Parse (dt);
        }
        
        public int CompareTo(FormattedDateTime other)
        {
            return ((IComparable)this).CompareTo(other.Value);
        }

        public override bool Equals(object obj)
        {
            return Value.Equals(((FormattedDateTime)obj).Value);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString(DATE_FORMAT);
        }

        int IComparable.CompareTo(object obj)
        {
            var comp = (obj is FormattedDateTime) ? (((FormattedDateTime)obj).Value) : obj;

            return Value.CompareTo(obj);
        }
    }
}
