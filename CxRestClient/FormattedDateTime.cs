using System;
using System.Collections.Generic;
using System.Text;

namespace CxRestClient
{
    public class FormattedDateTime : IComparable, IComparable<FormattedDateTime>
    {
        private static readonly String DATE_FORMAT = "yyyy-MM-ddTHH:mm:ss.fffzzz";

        private DateTime _theDateTime;

        public FormattedDateTime (DateTime dt)
        {
            _theDateTime = dt;
        }
        public FormattedDateTime(String dt)
        {
            _theDateTime = DateTime.Parse (dt);
        }
        
        public int CompareTo(FormattedDateTime other)
        {
            return ((IComparable)this).CompareTo(other);
        }

        public override bool Equals(object obj)
        {
            return _theDateTime.Equals(obj);
        }

        public override int GetHashCode()
        {
            return _theDateTime.GetHashCode();
        }

        public override string ToString()
        {
            return _theDateTime.ToString(DATE_FORMAT);
        }

        int IComparable.CompareTo(object obj)
        {
            return _theDateTime.CompareTo(obj);
        }
    }
}
