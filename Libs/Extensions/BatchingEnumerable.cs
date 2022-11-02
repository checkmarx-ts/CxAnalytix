using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxAnalytix.Extensions
{
    public static class BatchingEnumerable
    {
        public static IEnumerable<ICollection<T>> AsBatches<T>(this ICollection<T> elements, int maxElements = 20)
        {
            int count = 0;
            List<T> batch = new();

            var enumerator = elements.GetEnumerator();
            if (enumerator.MoveNext())
            {
                do
                {
                    batch.Add(enumerator.Current);

                    if (++count == maxElements)
                    {
                        yield return batch;
                        batch.Clear();
                        count = 0;
                    }

                    if (enumerator.MoveNext())
                        continue;

                    if (batch.Count > 0)
                        yield return batch;

                    break;
                } while (true);

            }
        }

        public static IEnumerable<String> AsCsvStringBatches(this ICollection<String> elements, int maxElements = 20)
        {
            foreach (var batch in elements.AsBatches<String>(maxElements))
                yield return String.Join(",", batch);
        }

    }
}
