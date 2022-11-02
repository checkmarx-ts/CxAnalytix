using CxAnalytix.Extensions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extensions_Test
{
    public class BatchingEnumerableTests
    {

        SortedSet<String> _strings;
        SortedSet<int> _numbers;

        private static readonly int MAX_ELEMS = 107;

        public BatchingEnumerableTests()
        {
            _strings = new();
            _numbers = new();
            for(int x = 0; x < MAX_ELEMS; x++)
            {
                _numbers.Add(x);
                _strings.Add(Guid.NewGuid().ToString());
            }
        }


        [Test]
        public void CanaryTest()
        {
            Assert.True(true);
        }

        private static int IterationCountDefaultBatch<T>(ICollection<T> values)
        {
            int count = 0;

            foreach (var val in values.AsBatches<T>())
                count++;

            return count;
        }
        private static int IterationCountBatch<T>(ICollection<T> values, int batchSize)
        {
            int count = 0;

            foreach (var val in values.AsBatches<T>(batchSize))
                count++;

            return count;
        }
        private static Stack<int> IterationCountHistoryBatch<T>(ICollection<T> values, int batchSize)
        {
            var result = new Stack<int>();

            foreach (var val in values.AsBatches<T>(batchSize))
                result.Push(val.Count);

            return result;
        }

        [Test]
        public void NumbersDefaultBatchSizeProduceMoreThanOneBatch()
        {
            Assert.Greater(IterationCountDefaultBatch(_numbers), 1);
        }

        [Test]
        public void StringsDefaultBatchSizeProduceMoreThanOneBatch()
        {
            Assert.Greater(IterationCountDefaultBatch(_strings), 1);
        }

        [Test]
        public void NumbersMaxBatchSizeProduceOneBatch()
        {
            Assert.True(IterationCountBatch(_numbers, MAX_ELEMS) == 1);
        }

        [Test]
        public void StringsMaxBatchSizeProduceOneBatch()
        {
            Assert.True(IterationCountBatch(_strings, MAX_ELEMS) == 1);
        }


        [Test]
        public void MaxElementsForTestingIsOdd()
        {
            Assert.True(MAX_ELEMS % 2 != 0);
        }


        [Test]
        public void NumbersEvenBatchSizeLastBatchIsOddForOddSet()
        {
            int batchSize = MAX_ELEMS / 2;

            Assert.True(IterationCountHistoryBatch(_numbers, batchSize).Pop() % 2 != 0);
        }

        [Test]
        public void StringsEvenBatchSizeLastBatchIsOddForOddSet()
        {
            int batchSize = MAX_ELEMS / 2;

            Assert.True(IterationCountHistoryBatch(_strings, batchSize).Pop() % 2 != 0);
        }

        [Test]
        public void StringCsvProducesMoreThanOneResultForDefaultBatch()
        {
            var csvEnum = _strings.AsCsvStringBatches().GetEnumerator();

            Assert.True(csvEnum.MoveNext() && csvEnum.MoveNext() );
        }

        [Test]
        public void StringCsvMatchForEntireBatch()
        {
            var batchResult = _strings.AsCsvStringBatches(MAX_ELEMS).First();
            var manualCsv = String.Join(",", _strings);

            Assert.True(manualCsv.CompareTo(batchResult) == 0);
        }

    }
}
