using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxAnalytix.Extensions
{
    public static class LoopExtensions
    {
        public static IEnumerable<T_OUT> AsGenerator<T_IN, T_OUT>(this IEnumerable<T_IN> elements, Func<T_IN, T_OUT> elementGenerator)
        {
            foreach(var elem in elements)
            {
                yield return elementGenerator(elem);
            }
        }

        public static void ActionForEach<T>(this IEnumerable<T> elements, Action<T> proc)
        {
            foreach (var elem in elements)
            {
                proc(elem);
            }
        }


    }
}
