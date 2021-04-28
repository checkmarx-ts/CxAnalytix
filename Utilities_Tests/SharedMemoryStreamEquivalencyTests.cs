using System;
using CxAnalytix.Utilities;
using System.IO;
using Xunit;

namespace Utilities_Tests
{
	public class SharedMemoryStreamEquivalencyTests
	{

		private static byte[] buffer1 = new byte[]
		{
			0, 1, 2, 3, 4
		};


		private long SeekEndIsFromWrittenLengthEquiv(Stream s)
		{
			s.Write(new byte[1024], 0, 1000);

			return s.Seek(0, SeekOrigin.End);
		}

		[Fact]
		public void SeekEndIsFromWrittenLength()
		{
			using (var ms = new MemoryStream(1024))
			using (var sms = new SharedMemoryStream(1024))
			{

				Assert.Equal(SeekEndIsFromWrittenLengthEquiv(ms), SeekEndIsFromWrittenLengthEquiv(sms) );
			}
		}


		private long LengthIsWrittenLengthEquiv (Stream s)
		{
			s.Write(new byte[1024], 0, 1000);

			return s.Length;
		}

		[Fact]
		public void LengthIsWrittenLength()
		{
			using (var ms = new MemoryStream(1024))
			using (var sms = new SharedMemoryStream(1024))
			{
				Assert.Equal(LengthIsWrittenLengthEquiv(ms), LengthIsWrittenLengthEquiv(sms));
			}
		}

		private long SeekPastWrittenEquiv(Stream s)
		{
			s.Write(new byte[1024], 0, 1000);
			return s.Seek(1019, SeekOrigin.Begin);
		}

		[Fact]
		public void SeekPastWritten()
		{
			using (var ms = new MemoryStream(1024))
			using (var sms = new SharedMemoryStream(1024))
			{
				Assert.Equal(SeekPastWrittenEquiv(ms), SeekPastWrittenEquiv(sms));
			}
		}


		private long SeekPastWrittenAndSizeRemainsSameEquiv(Stream s)
		{
			s.Write(new byte[1024], 0, 1000);
			return s.Seek(1019, SeekOrigin.Begin);
		}

		[Fact]
		public void SeekPastWrittenAndSizeRemainsSame()
		{
			using (var ms = new MemoryStream(1024))
			using (var sms = new SharedMemoryStream(1024))
			{
				Assert.Equal(SeekPastWrittenAndSizeRemainsSameEquiv(ms), SeekPastWrittenAndSizeRemainsSameEquiv(sms));
			}
		}

		private bool SizeAdjustsToLastWritePosEquiv(Stream s)
		{
			s.Write(new byte[1024], 0, 1000);
			s.Seek(1003, SeekOrigin.Begin);
			s.Write(buffer1, 0, buffer1.Length);
			return ((1003 + buffer1.Length) == s.Length);

		}

		[Fact]
		public void SizeAdjustsToLastWritePos()
		{
			using (var ms = new MemoryStream(1024))
			using (var sms = new SharedMemoryStream(1024))
			{
				Assert.True(SizeAdjustsToLastWritePosEquiv(ms) == SizeAdjustsToLastWritePosEquiv(sms));
			}
		}

		private bool TextReadAndWriteEquiv (Stream s)
		{
			String randomStr = "M37ERgzUh9bbJQvz56pYBX2EIF0IYJq6fEIBKogkZt8TMFXqWrHzgJyXzdyzvOfqOnB9MLIBpzKVwYmN42laMFADSCsa61yzda2Xw80pcSn2rt2KpbOsC4VgzWDQylMl4uGhFM7BXtj1u8DcBN5H7PP6alz7vNn4hgpAY4wYRDxOwg441kDdhXtjSXWuoLUXRzzZzZE9QgBoOO6lTmL1df93eP85EafwdXbpckWpYKmXySwHUU4zNcVRMyMCiwrdAnKKNJud2LxSa2fNfIkemALqFOUmHfcy3eYRDQgDcXH45GKf6dd6cl72mUenDiVzEALZjamabdqNzPsEDGh5tPDeMfXRhNSb4ZpXJOSuRgcT2JQ9KmXSmBqp33N7HMw9b6eMDvuRuJTc6K3dGzl6EEzACClIVYosMsrYlSicUBkEtgak4CAJrDskwDisHbItnrkhcVxuAf5fRp6QDz8UFSkSWr3S17Kc39RhXeys5JqFnyxswYSj8YFQ2PevMOWAAE0lPpgn6zQter2455UGITCPlq2dYMikYhgV6sM4uQoxDxw6EB1C8BeJZsNlay6ldjB42XWBEMPk5AvaZDKnoVMdtACS3Ho1Dk8LxNczcdvMUtu4PVTDmL18XYk0ktoaIZQG5YDhkiAc98aDyHxZ8FbWR5nVUg0gWbkQkuwC0KvwTx3ewxPAaGBUjU77kMtBwJAxxKx1odZ9g8XkZIdEFIeMmYaZbWLjzZoP17HmIdb5pYQT6rm2RulHy4Lpi1ims1RL425aOCUXzwSJ8q6FqD5twcLqpmvpqgMd0LeUyo1bPaXXy1LlE6RaMzGaXj1Tyu2GqAW10SZ3zaLOZ3H1umYYZKgpiOW8lvqD2L3Sb1SRWfgtxYO7O5hjB9k7AXzmm4y4JXbzmR2oZAjhOHaq390t3r6kXghjNqgoLT8L8QOCvFJPao8shQ4JPpSWdJyuGLSHpscbwLBz0qeCfukQndq9bt63BjFul6B5OtcvCZrE9cOIAxys7Tmvs20ZCPTI";

			using (TextWriter tw = new StreamWriter(s, leaveOpen: true))
				for (int x = 0; x < 10; x++)
					tw.WriteLine(randomStr);

			int readLines = 0;
			bool check = true;
			
			s.Seek(0, SeekOrigin.Begin);

			using (TextReader tr = new StreamReader(s))
			{

				while (true)
				{
					String line = tr.ReadLine();
					if (line == null)
						break;

					check &= line.CompareTo(randomStr) == 0;

					readLines++;
				}
			}



			return check && readLines == 10;
		}


		[Fact]
		public void TextReadAndWrite ()
		{
			using (var ms = new MemoryStream(64738))
			using (var sms = new SharedMemoryStream(64738))
			{
				Assert.True(TextReadAndWriteEquiv(ms) == TextReadAndWriteEquiv(sms));
			}
		}


		private long AutomaticCapacityExpansionEquiv (Stream s)
		{
			s.Write(buffer1, 0, buffer1.Length);
			return s.Seek(0, SeekOrigin.End);
		}

		[Fact]
		public void AutomaticCapacityExpansion ()
		{
			using (var ms = new MemoryStream(1))
			using (var sms = new SharedMemoryStream(1))
			{
				Assert.True(AutomaticCapacityExpansionEquiv(ms) == AutomaticCapacityExpansionEquiv(sms));
			}
		}

	}
}
