using System;
using System.Collections;
using System.Collections.Generic;

namespace Extensions_Test
{
    internal class DictionaryGenerator
    {
        public static readonly String VAL_KEY1 = "StringValue1";
        public static readonly IDictionary<String, Object> VAL_KEY7 = new Dictionary<String, Object>() { { "KEY1", VAL_KEY1 } };
        public static readonly DateTime VAL_KEY2 = DateTime.Now;
        public static readonly Guid VAL_KEY3 = Guid.NewGuid();
        public static readonly Double VAL_KEY4 = Double.MaxValue;
        public static readonly IDictionary<String, Object> VAL_KEY5 = new Dictionary<String, Object>() { { "KEY1", VAL_KEY1 }, { "KEY7", VAL_KEY7} };
        public static readonly List<String> VAL_KEY6 = new List<string> { "A", "B", "C"};

        protected static IDictionary<String, Object> CreateDictionary<T>() where T : IDictionary<String, Object>, new()
        {

            var retVal = new T();

            retVal.Add("KEY1", VAL_KEY1);
            retVal.Add("KEY2", VAL_KEY2);
            retVal.Add("KEY3", VAL_KEY3);
            retVal.Add("KEY4", VAL_KEY4);
            retVal.Add("KEY5", VAL_KEY5);
            retVal.Add("KEY6", VAL_KEY6);
            retVal.Add("KEY7", VAL_KEY7);

            return retVal;
        }

        public static IEnumerable Dictionary
        {
            get
            {
                yield return CreateDictionary<Dictionary<String, Object>>();
                yield return CreateDictionary<SortedDictionary<String, Object>>();
            }
        }
    }
}
