using CxAnalytix.Utilities;
using System;
using Xunit;

namespace Utilities_Tests
{
	public class SharedMemoryStreamTests
	{
		private static byte[] buffer1 = new byte[]
		{
			0, 1, 2, 3, 4
		};


		[Fact]
		public void ReadAfterWriteResultsInSameData()
		{
			using (var sms = new SharedMemoryStream(buffer1.Length))
			{
				sms.Write(buffer1, 0, buffer1.Length);
				sms.Seek(0, System.IO.SeekOrigin.Begin);

				byte[] readBack = new byte[buffer1.Length];

				int amount = sms.Read(readBack, 0, readBack.Length);

				Assert.Equal(buffer1.Length, amount);
				Assert.Equal(buffer1, readBack);
			}

		}

		[Fact]
		public void WriteMovesPositionSameAmountDefault()
		{
			using (var sms = new SharedMemoryStream(buffer1.Length))
			{
				var posBefore = sms.Position;

				sms.Write(buffer1, 0, buffer1.Length);

				Assert.Equal(buffer1.Length, sms.Position + 1 - posBefore);
			}
		}


		[Fact]
		public void WriteMovesPositionSameAmountAfterPositionChange()
		{
			using (var sms = new SharedMemoryStream(buffer1.Length * 2))
			{
				sms.Position++;

				var posBefore = sms.Position;

				sms.Write(buffer1, 0, buffer1.Length);

				Assert.Equal(buffer1.Length, sms.Position + 1 - posBefore);
			}

		}

		[Fact]
		public void MakeNewWithZeroSizeError()
		{

			try
			{
				new SharedMemoryStream(0).Dispose();
			}
			catch (ArgumentOutOfRangeException)
			{
				Assert.True(true);
				return;
			}

			Assert.True(false);
		}

		[Fact]
		public void LengthIsCapacity()
		{
			using (var sms = new SharedMemoryStream(1))
				Assert.Equal(1, sms.Length);
		}

		[Fact]
		public void CanSetPositionAtEndOfBuffer()
		{
			using (var sms = new SharedMemoryStream(1))
				try
				{
					sms.Position = 0;
				}
				catch (ArgumentOutOfRangeException)
				{
					Assert.True(false);
					return;
				}

			Assert.True(true);

		}


		[Fact]
		public void ErrorTryingToSetPosPastEndOfBuffer()
		{
			using (var sms = new SharedMemoryStream(1))
				try
				{
					sms.Position = 2;
				}
				catch (ArgumentOutOfRangeException)
				{
					Assert.True(true);
					return;
				}

			Assert.True(false);
		}

		[Fact]
		public void ErrorTryingToSetPosBeforeBuffer()
		{
			using (var sms = new SharedMemoryStream(1))
				try
				{
					sms.Position = -11;
				}
				catch (ArgumentOutOfRangeException)
				{
					Assert.True(true);
					return;
				}

			Assert.True(false);
		}

		[Fact]
		public void CapacityAndEndAreSame()
		{
			using (var sms = new SharedMemoryStream(10))
					sms.Position = 10;

			Assert.True(true);
		}

		[Fact]
		public void NoWriteBeyondCapacity()
		{
			using (var sms = new SharedMemoryStream(buffer1.Length - 1))
				try
				{
					sms.Write(buffer1, 0, buffer1.Length);
				}
				catch (ArgumentOutOfRangeException)
				{
					Assert.True(true);
					return;

				}

			Assert.True(false);
		}

		[Fact]
		public void NoReadBeyondCapacity()
		{
			using (var sms = new SharedMemoryStream(buffer1.Length - 1))
				try
				{
					sms.Read(buffer1, 0, buffer1.Length);
				}
				catch (ArgumentOutOfRangeException)
				{
					Assert.True(true);
					return;

				}

			Assert.True(false);
		}

		[Fact]
		public void NoReadNegative()
		{
			using (var sms = new SharedMemoryStream(buffer1.Length - 1))
				try
				{
					sms.Read(buffer1, 0, -1);
				}
				catch (ArgumentOutOfRangeException)
				{
					Assert.True(true);
					return;

				}

			Assert.True(false);
		}

		[Fact]
		public void NoWriteNegative()
		{
			using (var sms = new SharedMemoryStream(buffer1.Length - 1))
				try
				{
					sms.Write(buffer1, 0, -1);
				}
				catch (ArgumentOutOfRangeException)
				{
					Assert.True(true);
					return;

				}

			Assert.True(false);
		}


		[Fact]
		public void NoReadNullArray()
		{
			using (var sms = new SharedMemoryStream(buffer1.Length - 1))
				try
				{
					sms.Read(null, 0, 10);
				}
				catch (ArgumentException)
				{
					Assert.True(true);
					return;

				}

			Assert.True(false);
		}

		[Fact]
		public void NoWriteNullArray()
		{
			using (var sms = new SharedMemoryStream(buffer1.Length - 1))
				try
				{
					sms.Write(null, 0, 10);
				}
				catch (ArgumentException)
				{
					Assert.True(true);
					return;

				}

			Assert.True(false);
		}


		[Fact]
		public void ReadLengthZeroAtEnd()
		{
			using (var sms = new SharedMemoryStream(buffer1.Length))
			{
				sms.Seek(0, System.IO.SeekOrigin.End);
				Assert.Equal(0, sms.Read(buffer1, 0, 0));
			}
		}


		[Fact]
		public void SeekCurrentNoSeekPastBegin()
		{
			using (var sms = new SharedMemoryStream(buffer1.Length))
			{
				sms.Position = sms.Length - 1;

				try
				{
					sms.Seek(-sms.Length, System.IO.SeekOrigin.Current);
				}
				catch (ArgumentOutOfRangeException)
				{
					Assert.True(true);
					return;
				}
			}

			Assert.True(false);
		}

		[Fact]
		public void SeekCurrentNoSeekPastEnd()
		{
			using (var sms = new SharedMemoryStream(buffer1.Length))
				try
				{
					sms.Seek(sms.Length, System.IO.SeekOrigin.Current);
				}
				catch (ArgumentOutOfRangeException)
				{
					Assert.True(true);
					return;
				}

			Assert.True(false);
		}

		[Fact]
		public void SeekCurrentZeroOffsetDoesNotMovePosition()
		{
			using (var sms = new SharedMemoryStream(buffer1.Length))
			{
				sms.Position = buffer1.Length - 1;

				sms.Seek(0, System.IO.SeekOrigin.Current);
				Assert.Equal(buffer1.Length - 1, sms.Position);
			}
		}

		[Fact]
		public void SeekBeginNoSeekPastBegin()
		{
			using (var sms = new SharedMemoryStream(buffer1.Length))
				try
				{
					sms.Seek(-1, System.IO.SeekOrigin.Begin);
				}
				catch (ArgumentOutOfRangeException)
				{
					Assert.True(true);
					return;
				}

			Assert.True(false);
		}

		[Fact]
		public void SeekBeginNoSeekPastEnd()
		{
			using (var sms = new SharedMemoryStream(buffer1.Length))
				try
				{
					sms.Seek(sms.Length, System.IO.SeekOrigin.Begin);
				}
				catch (ArgumentOutOfRangeException)
				{
					Assert.True(true);
					return;
				}

			Assert.True(false);

		}

		[Fact]
		public void SeekBeginZeroOffsetMovesToBegin()
		{
			using (var sms = new SharedMemoryStream(buffer1.Length))
			{
				sms.Position = buffer1.Length - 1;

				sms.Seek(0, System.IO.SeekOrigin.Begin);
				Assert.Equal(0, sms.Position);
			}
		}


		[Fact]
		public void SeekEndNoSeekPastBegin()
		{
			using (var sms = new SharedMemoryStream(buffer1.Length))
			{
				sms.Position = buffer1.Length - 1;
				try
				{
					sms.Seek(buffer1.Length, System.IO.SeekOrigin.End);
				}
				catch (ArgumentOutOfRangeException)
				{
					Assert.True(true);
					return;
				}
			}

			Assert.True(false);
		}

		[Fact]
		public void SeekEndNoSeekPastEnd()
		{
			using (var sms = new SharedMemoryStream(buffer1.Length))
				try
				{
					sms.Seek(-1, System.IO.SeekOrigin.End);
				}
				catch (ArgumentOutOfRangeException)
				{
					Assert.True(true);
					return;
				}

			Assert.True(false);
		}

		[Fact]
		public void SeekEndZeroOffsetDoesMovesToEnd()
		{
			using (var sms = new SharedMemoryStream(buffer1.Length))
			{
				sms.Seek(0, System.IO.SeekOrigin.End);
				Assert.Equal(buffer1.Length - 1, sms.Position);
			}
		}

	}
}
