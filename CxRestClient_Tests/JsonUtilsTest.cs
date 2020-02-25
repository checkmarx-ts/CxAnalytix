using CxRestClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CxRestClient_Tests
{
    public class JsonUtilsTest
    {

        class Tester
        {
            public Tester()
            {
                AnArray = new LinkedList<string>();
                KeyValueArray = new Dictionary<string, string>();
            }

            public String RootString { get; set; }
            public Double RootDouble { get; set; }
            public LinkedList<String> AnArray { get; set; }
            public int SomeValue { get; set; }
            public Dictionary<String, String> KeyValueArray { get; set; }
            public int LastValue { get; set; }

        }

        private static readonly int MAX_ARRAY_VALUES = 30;

        private String _json;
        private JTokenReader _reader;

        [SetUp]
        public void MakeJsonDoc()
        {

            var listInit = new Func<LinkedList<String>>(() =>
            {

                var list = new LinkedList<String>();

                for (int x = 0; x < MAX_ARRAY_VALUES; x++)
                    list.AddLast($"String{x}");

                return list;
            });


            var dictInit = new Func<Dictionary<String, String>>(() =>
           {
               Dictionary<String, String> dict = new Dictionary<string, string>();

               for (int x = 0; x < MAX_ARRAY_VALUES; x++)
                   dict.Add($"Key{x}", $"Value{x}");

               return dict;
           });


            var test = new Tester()
            {
                RootString = "Hello",
                RootDouble = Math.PI,
                AnArray = listInit(),
                SomeValue = int.MaxValue,
                KeyValueArray = dictInit(),
                LastValue = int.MinValue
            };

            StringBuilder jsonStr = new StringBuilder();

            using (var sw = new StringWriter(jsonStr))
            {
                JsonSerializer js = new JsonSerializer();
                js.Serialize(sw, test);
            }

            _json = jsonStr.ToString();

            _reader = new JTokenReader(JToken.Load(new JsonTextReader(new StringReader(_json))));

        }

        [Test]
        public void ExpectInitializedReaderFirstTokenNull()
        {
            Assert.True(_reader.CurrentToken == null);
        }


        [Test]
        public void ExpectMoveToFirstPropertyAfterInitialize()
        {

            JsonUtils.MoveToNextProperty(_reader);

            Assert.True(_reader.CurrentToken.Type == JTokenType.Property &&
                ((JProperty)_reader.CurrentToken).Name.CompareTo("RootString") == 0);

        }


        [Test]
        public void Expect2ndPropertyAfterFirst()
        {
            JsonUtils.MoveToNextProperty(_reader);
            JsonUtils.MoveToNextProperty(_reader);

            Assert.True(_reader.CurrentToken.Type == JTokenType.Property &&
                ((JProperty)_reader.CurrentToken).Name.CompareTo("RootDouble") == 0);
        }

        [Test]
        public void GoesToPropertyNameInNestedElementWithoutName ()
        {
            JsonUtils.MoveToNextProperty(_reader); //RootString
            JsonUtils.MoveToNextProperty(_reader); //RootDouble
            JsonUtils.MoveToNextProperty(_reader); //AnArray
            JsonUtils.MoveToNextProperty(_reader); //SomeValue
            JsonUtils.MoveToNextProperty(_reader); //KeyValueArray
            JsonUtils.MoveToNextProperty(_reader); //KeyValueArray->element 1

            Assert.True(_reader.CurrentToken.Type == JTokenType.Property &&
                ((JProperty)_reader.CurrentToken).Name.CompareTo("Key0") == 0);

        }

        [Test]
        public void GoesToPropertyNameInNestedElementWithName()
        {
            JsonUtils.MoveToNextProperty(_reader, "Key1");

            Assert.True(_reader.CurrentToken.Type == JTokenType.Property &&
                ((JProperty)_reader.CurrentToken).Name.CompareTo("Key1") == 0);
        }

        [Test]
        public void WillNotFindNameIfPositionedOnRequestedName ()
        {
            bool firstRead = JsonUtils.MoveToNextProperty(_reader, "Key0");

            Assert.True(firstRead && !JsonUtils.MoveToNextProperty(_reader, "Key0"));
        }

        [Test]
        public void FindsSubsequentPropertyName()
        {
            bool firstRead = JsonUtils.MoveToNextProperty(_reader, "Key0");

            Assert.True(firstRead && JsonUtils.MoveToNextProperty(_reader, $"Key{MAX_ARRAY_VALUES - 1}"));
        }

        [Test]
        public void WillNotFindPassedValues ()
        {
            bool firstRead = JsonUtils.MoveToNextProperty(_reader, "RootDouble");

            Assert.True(firstRead && !JsonUtils.MoveToNextProperty(_reader, "RootString"));
        }

    }
}
