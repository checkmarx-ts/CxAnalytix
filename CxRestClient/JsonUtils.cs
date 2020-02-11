using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace CxRestClient
{
    internal class JsonUtils
    {

        public static bool MoveToNextProperty(JTokenReader reader)
        {
            while (reader.Read())
            {
                if (reader.CurrentToken.Type == JTokenType.Property)
                    return true;
            }

            return false;
        }

        public static bool MoveToNextProperty(JTokenReader reader, String named)
        {
            while (MoveToNextProperty(reader))
            {
                if (((JProperty)reader.CurrentToken).Name.CompareTo(named) == 0)
                    return true;
            }

            return false;
        }

    }
}
