using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;
using System.Threading.Tasks;

namespace CxAnalytix.Utilities
{
	public class SharedMemoryStream : System.IO.Stream, IDisposable
	{

		private MemoryMappedFile _mmf;
		private MemoryMappedViewAccessor _mmfa;
		private long _pos;
		private long _capacity;


		public SharedMemoryStream(long capacity) : base()
		{
			_capacity = capacity;
			_pos = 0;
			_mmf = MemoryMappedFile.CreateNew(null, capacity, MemoryMappedFileAccess.CopyOnWrite);
			_mmfa = _mmf.CreateViewAccessor(0, capacity, MemoryMappedFileAccess.CopyOnWrite);
		}


		public override bool CanRead => true;

		public override bool CanSeek => true;

		public override bool CanWrite => true;

		public override long Length => _capacity;

		public override long Position
		{
			get => _pos; 
			
			set
			{
				if (value > Length)
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

			if (count + Position > Length)
				throw new ArgumentOutOfRangeException("count", $"Attempt to read beyond end of stream");

			if (Position >= Length)
				return 0;

			int readAmount = _mmfa.ReadArray<byte>(Position, buffer, offset, count);

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

					if (offset > Length)
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

					if (Position + offset > Length)
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

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (buffer == null)
				throw new ArgumentException("It is not possible to write from a null buffer.");

			if (offset + count > buffer.Length)
				throw new ArgumentException($"The total of offset and count exceeds the buffer capacity");

			if (offset < 0)
				throw new ArgumentOutOfRangeException("offset", $"Value must be => 0");

			if (count < 0)
				throw new ArgumentOutOfRangeException("count", $"Value must be => 0");

			if (count + Position > Length)
				throw new ArgumentOutOfRangeException("count", $"Attempt to write beyond end of stream");


			_mmfa.WriteArray<byte>(Position, buffer, offset, count);

			Position += count;
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
