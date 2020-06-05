using System;
using System.Collections.Generic;
using CxAnalytix.Extensions;

namespace StringFormatExamples
{
    class Program
    {
        static void Main(string[] args)
        {

            Dictionary<String, object> sample = new Dictionary<string, object> ()
            {
                { "DATE", DateTime.Now},
                { "TEXT", "Hello, World!" },
                { "BD", "my birthday is" },
                { "FLOAT", Math.PI },
                { "DICT", new Dictionary<String, Object> () { { "MyKey", "MyValue"}  } }
            };



            // Plain text
            // Output example: Hello, World!
            Console.WriteLine(sample.ComposeString("{TEXT}"));

            // Plain text surrounded with escaped curly braces
            // Output example: {Hello, World!}
            Console.WriteLine(sample.ComposeString("\\{{TEXT}\\}"));

            // Plain text in a sentence
            // Output example: I just want to say "Hello, World!" That is all.
            Console.WriteLine(sample.ComposeString("I just want to say \"{TEXT}\" That is all."));

            // Default DateTime string output.
            // Output Example: 6/5/2020 2:52:30 PM
            Console.WriteLine(sample.ComposeString("{DATE}") );

            // Date in year/month/day format 
            // Output Example: This is a modified date format: 2020/06/05
            Console.WriteLine(sample.ComposeString("This is a modified date format: {DATE:yyyy/MM/dd}"));

            // Dictionary key traveral when the value is a dictionary
            // Output Example: MyKey = MyValue
            Console.WriteLine(sample.ComposeString("MyKey = {DICT.MyKey}"));

            // Complex string
            // Output Example: It is June and my birthday is on Friday. I will be 3 years old.
            Console.WriteLine(sample.ComposeString("It is {DATE:MMMM} and {BD} on {DATE:dddd}. I will be {FLOAT:##} years old."));
            



        }
    }
}
