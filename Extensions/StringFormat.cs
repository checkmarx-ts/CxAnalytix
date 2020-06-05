using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CxAnalytix.Extensions
{
    /// <summary>
    /// A class that provides an extension method to allow dictionary values to be
    /// used as if they were part of a string formatting mechanism.
    /// </summary>
    public static class StringFormat
    {
        private struct Pos
        {
            public bool resolve;
            public int begin;
            public int end;
        }


        public class InvalidFormatStringException : Exception
        {
            public InvalidFormatStringException(String formatSpec) : base(String.Format("Invalid format specification[{0}].", formatSpec))
            {
            }
        }


        private static List<Pos> ResolveSegmentPositions(String formatSpec)
        {
            var retVal = new List<Pos>();

            bool escape = false;
            bool started = false;
            int lastBegin = 0;
            int curFieldBegin = 0;

            for (int x = 0; x < formatSpec.Length; x++)
            {

                if (formatSpec[x] == '{' && !started && !escape)
                {
                    retVal.Add(new Pos { resolve = false, begin = lastBegin, end = x });
                    curFieldBegin = x;
                    started = true;
                    continue;
                }

                if (formatSpec[x] == '}' && started && !escape)
                {
                    lastBegin = x + 1;
                    retVal.Add(new Pos { resolve = true, begin = curFieldBegin, end = x });
                    started = false;
                    continue;
                }

                if (formatSpec[x] == '\\' && !escape)
                {
                    escape = true;
                    retVal.Add(new Pos { resolve = false, begin = lastBegin, end = x });
                    continue;
                }

                if (escape)
                {
                    lastBegin = x;
                    escape = false;
                }
            }

            if (started || escape)
                throw new InvalidFormatStringException(formatSpec);
            else
                retVal.Add(new Pos { resolve = false, begin = lastBegin, end = formatSpec.Length });


            return retVal;
        }


        /// <summary>
        /// Uses syntax similar to String.Format to compose a string made of values associated with dictionary keys.
        /// </summary>
        /// <remarks>
        /// The <see cref="formatSpec"/> can be used to specify key values in the dictionary to produce a formatted
        /// string containing the values associated with the specified key values.  The format values can be surrounded
        /// with static content.  The format values may also have format specifications that are applied in the same
        /// way they are applied when using String.Format.  The general format specification is:
        /// 
        /// <c>
        /// {[key][:][format value]}
        /// </c>
        /// 
        /// The format value follows normal format values supported in String.Format.
        /// 
        /// Curly braces can be prefixed with '\' to escape them from string formatting.
        /// 
        /// If specified keys are missing, an empty string will be substituted.
        /// 
        /// Keys for contained dictionaries can be separated by '.' to retrieve
        /// values from nested dictionaries. 
        /// 
        /// 
        /// Example:
        /// <c>
        ///using System;
        ///using System.Collections.Generic;
        ///using CxAnalytix.Extensions;
        ///namespace StringFormatExamples
        ///    {
        ///        class Program
        ///        {
        ///            static void Main(string[] args)
        ///            {
        ///            
        ///                Dictionary<String, object> sample = new Dictionary<string, object>()
        ///            {
        ///                { "DATE", DateTime.Now},
        ///                { "TEXT", "Hello, World!" },
        ///                { "BD", "my birthday is" },
        ///                { "FLOAT", Math.PI },
        ///                { "DICT", new Dictionary<String, Object> () { { "MyKey", "MyValue"}  } }
        ///            };
        ///
        ///                // Plain text
        ///                // Output example: Hello, World!
        ///                Console.WriteLine(sample.ComposeString("{TEXT}"));
        ///                
        ///                // Plain text surrounded with escaped curly braces
        ///                // Output example: {Hello, World!}
        ///                Console.WriteLine(sample.ComposeString("\\{{TEXT}\\}"));
        ///                
        ///                // Plain text in a sentence
        ///                // Output example: I just want to say "Hello, World!" That is all.
        ///                Console.WriteLine(sample.ComposeString("I just want to say \"{TEXT}\" That is all."));
        ///                
        ///                // Default DateTime string output.
        ///                // Output Example: 6/5/2020 2:52:30 PM
        ///                Console.WriteLine(sample.ComposeString("{DATE}"));
        ///                
        ///                // Date in year/month/day format 
        ///                // Output Example: This is a modified date format: 2020/06/05
        ///                Console.WriteLine(sample.ComposeString("This is a modified date format: {DATE:yyyy/MM/dd}"));
        ///                
        ///                // Dictionary key traveral when the value is a dictionary
        ///                // Output Example: MyKey = MyValue
        ///                Console.WriteLine(sample.ComposeString("MyKey = {DICT.MyKey}"));
        ///                
        ///                // Complex string
        ///                // Output Example: It is June and my birthday is on Friday. I will be 3 years old.
        ///                Console.WriteLine(sample.ComposeString("It is {DATE:MMMM} and {BD} on {DATE:dddd}. I will be {FLOAT:##} years old."));
        ///            }
        ///        }
        ///    }
        /// </c>
        /// 
        /// </remarks>
        /// <param name="elements">A dictionary containing elements to reference in the format specification.</param>
        /// <param name="formatSpec">The format specification string.</param>
        /// <returns>A string with a format derived from <seecref="formatSpec"/> </returns>
        public static String ComposeString(this IDictionary<String, Object> elements, String formatSpec)
        {
            var positions = ResolveSegmentPositions(formatSpec);

            StringBuilder theString = new StringBuilder();

            foreach (var pos in positions)
            {
                if (!pos.resolve)
                {
                    theString.Append(formatSpec.Substring(pos.begin, pos.end - pos.begin));
                }
                else
                {
                    String[] keyValueComponents = formatSpec.Substring(pos.begin + 1, (pos.end - pos.begin) - 1).Split(':');
                    String keyValue = keyValueComponents[0];

                    var dataValue = NormalizeDataValue(keyValue, elements);

                    String appendFormatSpec;

                    if (keyValueComponents.Length > 1)
                        appendFormatSpec = String.Format("{{0:{0}}}", keyValueComponents[1]);
                    else
                        appendFormatSpec = "{0}";

                    theString.AppendFormat(appendFormatSpec, dataValue);
                }
            }


            return theString.ToString();
        }

        private static Object NormalizeDataValue(String keyValue, IDictionary<string, object> elements)
        {
            String[] keyElements = keyValue.Split('.');
            if (keyElements.Length <= 1)
                return (elements.ContainsKey(keyElements[0])) ? (elements[keyElements[0]]) : (String.Empty);
            else if (elements.ContainsKey(keyElements[0]))
            {
                Type[] implInterfaces = elements[keyElements[0]].GetType().GetInterfaces();

                foreach (var t in implInterfaces)
                    if (t == typeof(IDictionary<String, Object>))
                        return NormalizeDataValue(keyValue.Substring(keyElements[0].Length + 1), elements[keyElements[0]] as IDictionary<String, Object>);
            }

            return String.Empty;
        }
    }
}
