using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;
using System.Threading.Tasks;

namespace CxAnalytix.Utilities
{
	/// <summary>
	/// A memory stream implementation similar to MemoryStream but using shared memory as the backing memory.
	/// </summary>
	public class SharedMemoryStream : System.IO.Stream, IDisposable
	{

		private MemoryMappedFile _mmf;
		private MemoryMappedViewAccessor _mmfa;
		private long _pos;
		private long _capacity;
		private long _maxPosition;
		private bool _readOnly = false;

		private readonly float INCREASE_FACTOR = 0.2F;
		private readonly int COPY_BUFF_SIZE = 64738;


		public SharedMemoryStream(long initialCapacity) : base()
		{
			_capacity = initialCapacity;
			_pos = 0;
			_mmf = MemoryMappedFile.CreateNew(null, initialCapacity, MemoryMappedFileAccess.ReadWrite);
			_mmfa = _mmf.CreateViewAccessor(0, initialCapacity, MemoryMappedFileAccess.ReadWrite);
		}

		private SharedMemoryStream () : base()
		{

		}

		public Stream ReadOnlyView()
		{
			_mmfa.Flush();
			return _mmf.CreateViewStream(0, _capacity, MemoryMappedFileAccess.Read);
		}

		public override bool CanRead => true;

		public override bool CanSeek => true;

		public override bool CanWrite => !_readOnly;

		public override long Length {get => _maxPosition; }

		public long Capacity { get => _capacity;  }

		public override long Position
		{
			get => _pos; 
			
			set
			{
				if (value > _capacity)
					throw new ArgumentOutOfRangeException($"Buffer length is {Length}, {value} exceeds the buffer capaciity");

				if (value < 0)
					throw new ArgumentOutOfRangeException("Stream can't be set before start of the buffer");

				_pos = value;
			}
		}

		public override void Flush()
		{
			_mmfa.Flush();
		}
		public override void Close()
		{
			base.Close();
			if (_mmfa != null)
			{
				_mmfa.Flush();
				_mmfa.Dispose();
				_mmfa = null;
			}

			if (_mmf != null)
			{
				_mmf.Dispose();
				_mmf = null;
			}

		}


		public override int Read(byte[] buffer, int offset, int count)
		{

			if (buffer == null)
				throw new ArgumentException("It is not possible to read into a null buffer.");

			if (offset + count > buffer.Length)
				throw new ArgumentException($"The total of offset and count exceeds the buffer capacity");

			if (offset < 0)
				throw new ArgumentOutOfRangeException("offset", $"Value must be => 0");

			if (count < 0)
				throw new ArgumentOutOfRangeException("count", $"Value must be => 0");

			if (Position >= _capacity)
				return 0;

			int readAmount = _mmfa.ReadArray<byte>(Position, buffer, offset, count);

			if (Position + readAmount > _maxPosition)
				readAmount = Convert.ToInt32(_maxPosition - Position);

			Position += readAmount;

			return readAmount;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			switch (origin)
			{
				case SeekOrigin.Begin:
					if (offset < 0)
						throw new ArgumentOutOfRangeException("offset", $"Seek request to move {offset} from {origin} moves position before start of buffer");

					if (offset > _capacity)
						throw new ArgumentOutOfRangeException("offset", $"Seek request to move {offset} from {origin} moves position beyond end of buffer");
					
					Position = offset;
					break;

				case SeekOrigin.End:
					if (offset < 0)
						throw new ArgumentOutOfRangeException("offset", $"Seek request to move {offset} from {origin} moves position beyond end of buffer");

					if (Length - offset < 0)
						throw new ArgumentOutOfRangeException("offset", $"Seek request to move {offset} from {origin} moves position before start of buffer");

					Position = Length - offset;
					break;

				case SeekOrigin.Current:
					if (Position + offset < 0)
						throw new ArgumentOutOfRangeException("offset", $"Seek request to move {offset} from {origin} moves position before start of buffer");

					if (Position + offset > _capacity)
						throw new ArgumentOutOfRangeException("offset", $"Seek request to move {offset} from {origin} moves position beyond end of buffer");

					Position += offset;
					break;
			}

			return Position;
		}

		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}

		private void IncreaseCapacity (long newCapacity)
		{

			if (newCapacity <= _capacity)
				return;

			newCapacity += Convert.ToInt64 (newCapacity * INCREASE_FACTOR);

			byte[] buf = new byte[COPY_BUFF_SIZE];

			MemoryMappedFile new_mmf = MemoryMappedFile.CreateNew(null, newCapacity, MemoryMappedFileAccess.ReadWrite);
			MemoryMappedViewAccessor new_mmfa = new_mmf.CreateViewAccessor(0, newCapacity, MemoryMappedFileAccess.ReadWrite);
			_mmfa.Flush();

			long curPos = 0;

			while (curPos < _capacity)
			{
				int readAmount = _mmfa.ReadArray(curPos, buf, 0, buf.Length);
				if (readAmount <= 0)
					break;

				new_mmfa.WriteArray(curPos, buf, 0, readAmount);
				curPos += readAmount;
			}

			new_mmfa.Flush();
			_mmfa.Dispose();
			_mmf.Dispose();
			_mmf = new_mmf;
			_mmfa = new_mmfa;
			_capacity = newCapacity;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (_readOnly)
				throw new InvalidOperationException("This instance is read only.");

			if (buffer == null)
				throw new ArgumentException("It is not possible to write from a null buffer.");

			if (offset + count > buffer.Length)
				throw new ArgumentException($"The total of offset and count exceeds the buffer capacity");

			if (offset < 0)
				throw new ArgumentOutOfRangeException("offset", $"Value must be => 0");

			if (count < 0)
				throw new ArgumentOutOfRangeException("count", $"Value must be => 0");

			if (count + Position > _capacity)
				IncreaseCapacity(count + Position);


			_mmfa.WriteArray<byte>(Position, buffer, offset, count);

			Position += count;

			_maxPosition = Math.Max(Position, _maxPosition);
		}

		public new void Dispose ()
		{
			if (_mmfa != null)
			{
				_mmfa.Flush();
				_mmfa.Dispose();
				_mmfa = null;
			}

			if (_mmf != null)
			{
				_mmf.Dispose();
				_mmf = null;
			}

			base.Dispose();
		}

	}
}
