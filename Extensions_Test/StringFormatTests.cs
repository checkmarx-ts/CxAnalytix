using System;
using NUnit.Framework;
using System.Collections.Generic;
using CxAnalytix.Extensions;

namespace Extensions_Test
{
    [TestFixtureSource(typeof(DictionaryGenerator), "Dictionary")]
    public class StringFormatTests
    {

        IDictionary<String, Object> _dict = null;

        public StringFormatTests (IDictionary<String, Object> dict)
        {
            _dict = dict;
        }

        [Test]
        public void ValidateTestingSetup()
        {
            Assert.NotNull(_dict);
        }

        [Test]
        public void RegularStringIsARegularString()
        {
            String formatSpec = "SOMESTRING";

            Assert.AreEqual(_dict.ComposeString(formatSpec), "SOMESTRING");
        }

        [Test]
        public void StringOnlyHasFormatSpec()
        {
            String formatSpec = "{KEY1}";

            Assert.AreEqual(String.Format("{0}", DictionaryGenerator.VAL_KEY1), _dict.ComposeString(formatSpec));
        }

        [Test]
        public void FormatSpecUsesSameKeyMoreThanOnce()
        {
            String formatSpec = "{KEY1}SOMESTRING-{KEY1}";

            Assert.AreEqual(String.Format("{0}SOMESTRING-{0}", DictionaryGenerator.VAL_KEY1), _dict.ComposeString(formatSpec));
        }


        [Test]
        public void FormatSpecFirst()
        {
            String formatSpec = "{KEY3}-SOMESTRING";

            Assert.AreEqual (String.Format("{0}-SOMESTRING", DictionaryGenerator.VAL_KEY3), _dict.ComposeString(formatSpec) );
        }

        [Test]
        public void FormatSpecLast()
        {
            String formatSpec = "SOMESTRING-{KEY1}";

            Assert.AreEqual(String.Format("SOMESTRING-{0}", DictionaryGenerator.VAL_KEY1), _dict.ComposeString(formatSpec) );
        }

        [Test]
        public void FormatSpecSurrounded()
        {
            String formatSpec = "SOMESTRING-{KEY2}-SOMEOTHERSTRING";

            Assert.AreEqual(String.Format("SOMESTRING-{0}-SOMEOTHERSTRING", DictionaryGenerator.VAL_KEY2), _dict.ComposeString(formatSpec) );
        }

        [Test]
        public void FormatSpecWithMissingFieldFirst()
        {
            String formatSpec = "{NOPE}-SOMESTRING";

            Assert.AreEqual("-SOMESTRING", _dict.ComposeString(formatSpec) );
        }

        [Test]
        public void FormatSpecWithMissingFieldLast()
        {
            String formatSpec = "SOMESTRING-{NOPE}";

            Assert.AreEqual("SOMESTRING-", _dict.ComposeString(formatSpec) );

        }

        [Test]
        public void FormatSpecWithMissingFieldSurrounded()
        {
            String formatSpec = "{NOPE}-SOMESTRING-{NOPE}";

            Assert.AreEqual("-SOMESTRING-", _dict.ComposeString(formatSpec) );
        }

        [Test]
        public void EscapedFormatSpecFirst()
        {
            String formatSpec = "\\{SOMETHING IN CURLYS\\}{KEY3}-SOMESTRING";

            Assert.AreEqual(String.Format("{{SOMETHING IN CURLYS}}{0}-SOMESTRING", DictionaryGenerator.VAL_KEY3), _dict.ComposeString(formatSpec) );
        }

        [Test]
        public void EscapedFormatSpecLast()
        {
            String formatSpec = "SOMETHING IN CURLYS{KEY3}-SOMESTRING\\{";

            Assert.AreEqual(String.Format("SOMETHING IN CURLYS{0}-SOMESTRING{{", DictionaryGenerator.VAL_KEY3), _dict.ComposeString(formatSpec));
        }

        [Test]
        public void EscapedFormatSpecSurrounded()
        {
            String formatSpec = "TEST-\\{{KEY1}\\}-SOMESTRING";

            Assert.AreEqual(String.Format("TEST-{{{0}}}-SOMESTRING", DictionaryGenerator.VAL_KEY1), _dict.ComposeString(formatSpec));
        }

        [Test]
        public void EscapedAndUnescapedFormatSpec()
        {
            String formatSpec = "\\\\\\{KEY1\\}: {KEY1}";

            Assert.AreEqual(String.Format("\\{{KEY1}}: {0}", DictionaryGenerator.VAL_KEY1), _dict.ComposeString(formatSpec));
        }


        [Test]
        public void FormatSpecWithNoFormatting()
        {
            String formatSpec = "\\{KEY2\\}: {KEY2}";

            Assert.AreEqual(String.Format("{{KEY2}}: {0}", DictionaryGenerator.VAL_KEY2), _dict.ComposeString(formatSpec));
        }

        [Test]
        public void FormatSpecWithExtraFormatting()
        {
            String formatSpec = "\\{KEY2\\}: {KEY2:MM}";

            Assert.AreEqual(String.Format("{{KEY2}}: {0:MM}", DictionaryGenerator.VAL_KEY2), _dict.ComposeString(formatSpec));
        }

        [Test]
        public void FormatSpecWithMissingFormatting()
        {
            String formatSpec = "\\{KEY2\\}: {KEY2:}";

            Assert.AreEqual(String.Format("{{KEY2}}: {0}", DictionaryGenerator.VAL_KEY2), _dict.ComposeString(formatSpec));

        }

        [Test]
        public void FormatSpecWithMultipleExtraFormatting()
        {
            String formatSpec = "\\{KEY2\\}: {KEY2:dd-MM-yyy} {KEY4:####.00}";

            Assert.AreEqual(String.Format("{{KEY2}}: {0:dd-MM-yyyy} {1:####.00}", DictionaryGenerator.VAL_KEY2, DictionaryGenerator.VAL_KEY4), _dict.ComposeString(formatSpec));
        }

        [Test]
        public void FormatSpecMixedWithFormattingAndNonFormatting()
        {
            String formatSpec = "\\{KEY2\\}: {KEY2:dd-MM-yyy} {KEY4:####.00}{KEY1}";

            Assert.AreEqual(String.Format("{{KEY2}}: {0:dd-MM-yyyy} {1:####.00}{2}", DictionaryGenerator.VAL_KEY2, DictionaryGenerator.VAL_KEY4, 
                DictionaryGenerator.VAL_KEY1), _dict.ComposeString(formatSpec));
        }


        [Test]
        public void UnclosedFormatStringThrowsException()
        {
            try
            {
                String formatSpec = "{KEY2";
                _dict.ComposeString(formatSpec);

            }
            catch (StringFormat.InvalidFormatStringException ex)
            {
                Assert.Pass();
                return;
            }
            Assert.Fail();
        }

        [Test]
        public void EscapeAtEndOfStringThrowsException()
        {
            try
            {
                String formatSpec = "\\";
                _dict.ComposeString(formatSpec);

            }
            catch (StringFormat.InvalidFormatStringException ex)
            {
                Assert.Pass();
                return;
            }
            Assert.Fail();
        }

        [Test]
        public void MultiLevelKeyNameInFormat()
        {
            String formatSpec = "{KEY5.KEY1}";

            Assert.AreEqual(String.Format("{0}", DictionaryGenerator.VAL_KEY1), _dict.ComposeString(formatSpec));
        }

        [Test]
        public void MultiLevelKeyNameInFormatButMissingDictionaryValue()
        {
            String formatSpec = "{KEY1.VAL_KEY1}";
            Assert.AreEqual(String.Empty, _dict.ComposeString(formatSpec));
        }

        [Test]
        public void MultiLevelKeyNameInFormatButNotDictionaryValue()
        {
            String formatSpec = "{KEY5.KEY1.KEY1}";
            Assert.AreEqual(String.Empty, _dict.ComposeString(formatSpec));
        }

        [Test]
        public void MultiLevelKeyNameInFormatWith2LevelDepth()
        {
            String formatSpec = "{KEY5.KEY7.KEY1}";
            Assert.AreEqual(String.Format("{0}", DictionaryGenerator.VAL_KEY1), _dict.ComposeString(formatSpec));
        }

        [Test]
        public void DictionaryValueUsedInFormat()
        {
            String formatSpec = "{KEY5}";
            Assert.AreEqual(String.Format("{0}", DictionaryGenerator.VAL_KEY5), _dict.ComposeString(formatSpec));
        }

        [Test]
        public void ListValueUsedInFormat()
        {
            String formatSpec = "{KEY6}";
            Assert.AreEqual(String.Format("{0}", DictionaryGenerator.VAL_KEY6), _dict.ComposeString(formatSpec));
        }

        [Test]
        public void TestEscapeEscapeChar()
        {
            String formatSpec = "PATH\\\\{KEY3}-SOMESTRING";

            Assert.AreEqual(String.Format("PATH\\{0}-SOMESTRING", DictionaryGenerator.VAL_KEY3), _dict.ComposeString(formatSpec));
        }

        [Test]
        public void TestSingleEscapedOpenBeginChar()
        {
            String formatSpec = "\\{SOMETHING IN CURLYS{KEY3}-SOMESTRING";

            Assert.AreEqual(String.Format("{{SOMETHING IN CURLYS{0}-SOMESTRING", DictionaryGenerator.VAL_KEY3), _dict.ComposeString(formatSpec) );
        }

        [Test]
        public void TestSingleEscapedOpenNonBeginChar()
        {
            String formatSpec = "SOMETHING \\{IN CURLYS{KEY3}-SOMESTRING";

            Assert.AreEqual(String.Format("SOMETHING {{IN CURLYS{0}-SOMESTRING", DictionaryGenerator.VAL_KEY3), _dict.ComposeString(formatSpec) );
        }

        [Test]
        public void TestSingleEscapedCloseBeginChar()
        {
            String formatSpec = "\\}SOMETHING IN CURLYS{KEY3}-SOMESTRING";

            Assert.AreEqual(String.Format("}}SOMETHING IN CURLYS{0}-SOMESTRING", DictionaryGenerator.VAL_KEY3), _dict.ComposeString(formatSpec) );
        }

        [Test]
        public void TestSingleEscapedCloseNonBeginChar()
        {
            String formatSpec = "SOMETHING \\}IN CURLYS{KEY3}-SOMESTRING";

            Assert.AreEqual(String.Format("SOMETHING }}IN CURLYS{0}-SOMESTRING", DictionaryGenerator.VAL_KEY3), _dict.ComposeString(formatSpec) );
        }
    }
}