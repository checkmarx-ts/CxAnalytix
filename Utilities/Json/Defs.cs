using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CxAnalytix.Utilities.Json
{
	public static class Defs
	{

        public static readonly String DATE_FORMAT = "yyyy-MM-ddTHH:mm:ss.fffzzz";

        public static JsonSerializerSettings serializerSettings = new JsonSerializerSettings()
        {
            DateFormatString = DATE_FORMAT,
            NullValueHandling = NullValueHandling.Ignore,
            StringEscapeHandling = StringEscapeHandling.EscapeHtml,
            Converters = new List<JsonConverter>()
            {
                new PrimitiveJsonConverter ()
            }

        };

    }
}
