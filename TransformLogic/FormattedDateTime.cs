using System;
using System.Collections.Generic;
using System.Text;

namespace CxAnalytix.TransformLogic
{
    public class FormattedDateTime
    {
        private static readonly String DATE_FORMAT = "yyyy-MM-ddTHH:mm:ss.fffzzz";

        private DateTime _theDateTime;

        public FormattedDateTime (DateTime dt)
        {
            _theDateTime = dt;
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
    }
}
