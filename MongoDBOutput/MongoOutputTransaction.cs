using CxAnalytix.Exceptions;
using CxAnalytix.Extensions;
using CxAnalytix.Interfaces.Outputs;
using CxAnalytix.Utilities;
using log4net;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace CxAnalytix.Out.MongoDBOutput
{
	internal class MongoOutputTransaction : MongoOutputBase
	{
		private static ILog _log = LogManager.GetLogger(typeof(MongoOutputTransaction));


		private bool _committed = false;
		private DateTime _trxStarted = DateTime.MinValue;
		private long _recordCount = 0;
		private Dictionary<String, LinkedList<IndexDescriptor>> _index = new Dictionary<string, LinkedList<IndexDescriptor>>();
		private SharedMemoryStream _cache = new SharedMemoryStream(2048000);


		private class IndexDescriptor
		{
			public long Position { get; set; }

			public long RecordLength { get; set; }
		}

		private class BsonCacheEnumerator : IEnumerator<BsonDocument>
		{
			private IEnumerator<IndexDescriptor> _docs;
			private BsonDocument _current;
			private Stream _cacheView;

			public BsonCacheEnumerator(IEnumerator<IndexDescriptor> docs, Stream view)
			{
				_docs = docs;
				_cacheView = view;
			}


			public BsonDocument Current
			{
				get
				{
					if (_current == null && _docs.Current != null)
					{
						_cacheView.Position = _docs.Current.Position;

						using (var br = new BsonBinaryReader(_cacheView))
							_current = BsonSerializer.Deserialize<BsonDocument>(br);
					}

					return _current;
				}
			}


			object IEnumerator.Current => this.Current;

			public void Dispose()
			{

			}

			public bool MoveNext()
			{
				_current = null;
				return _docs.MoveNext();
			}

			public void Reset()
			{
				_current = null;
				_docs.Reset();
			}
		}

		private class BsonCacheEnumerable : IEnumerable<BsonDocument>
		{
			private IEnumerable<IndexDescriptor> _docs;
			private Stream _cache;

			public BsonCacheEnumerable(IEnumerable<IndexDescriptor> docs, Stream cache)
			{
				_docs = docs;
				_cache = cache;
			}

			public IEnumerator<BsonDocument> GetEnumerator()
			{
				return new BsonCacheEnumerator(_docs.GetEnumerator (), _cache);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return this.GetEnumerator();
			}
		}



		internal MongoOutputTransaction(MongoDBOutFactory instance) : base(instance)
		{
			_trxStarted = DateTime.Now;
			_log.Debug($"Pseudo Transaction START at {_trxStarted}: {TransactionId}");
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public override bool Commit()
		{
			_committed = true;
			var end = DateTime.Now;
			_log.Debug($"Pseudo Transaction COMMIT at {end} elapsed time {end.Subtract(_trxStarted).TotalMilliseconds}ms for {TransactionId} [{_recordCount} records]");



			Parallel.ForEach(_index.Keys, (recordName) => {

				var outWriter = this[recordName];

				using (var session = MongoDBOutFactory.Client.StartSession())
				{
					var writeStart = DateTime.Now;
					using (var view = _cache.ReadOnlyView())
						outWriter.write(session, new BsonCacheEnumerable(_index[recordName], view));
					_log.Trace($"{recordName} records written in {DateTime.Now.Subtract(writeStart).TotalMilliseconds}ms ");
				}

			});

			return true;
		}

		public override void Dispose()
		{
			_log.Trace($"Disposing of pseudo transaction {TransactionId}: {_recordCount} records {((_committed) ? ("committed.") : ("discarded.")) } ");
		}


		[MethodImpl(MethodImplOptions.Synchronized)]
		public override void write(IRecordRef which, IDictionary<string, object> record)
		{
			var outInst = this[which.RecordName];

			if (outInst == null)
				throw new UnrecoverableOperationException($"Record reference for Mongo record {which.RecordName} is invalid.");

			var descriptor = new IndexDescriptor()
			{
				Position = _cache.Position
			};

			var doc = outInst.BsonSerialize(record);

			using (var bw = new BsonBinaryWriter(_cache))
				BsonSerializer.Serialize<BsonDocument>(bw, doc);

			descriptor.RecordLength = _cache.Position - descriptor.Position;

			if (!_index.ContainsKey(which.RecordName))
				_index.Add(which.RecordName, new LinkedList<IndexDescriptor>());

			_index[which.RecordName].AddLast(descriptor);
			_recordCount++;
		}
	}
}
