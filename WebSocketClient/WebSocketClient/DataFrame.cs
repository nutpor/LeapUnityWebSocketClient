using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebSocketClient
{
	public class DataFrame
	{

		public const byte Start = 0;
		public const byte End = 255;

		private StringBuilder builder;
		public bool IsComplete { get; set; }

		public DataFrame()
		{
			IsComplete = false;
			builder = new StringBuilder();
		}

		public static byte[] Wrap(string data)
		{
			var bytes = Encoding.UTF8.GetBytes(data);
			// wrap the array with the wrapper bytes
			var wrappedBytes = new byte[bytes.Length + 2];
			wrappedBytes[0] = DataFrame.Start;
			wrappedBytes[wrappedBytes.Length - 1] = DataFrame.End;
			Array.Copy(bytes, 0, wrappedBytes, 1, bytes.Length);
			return wrappedBytes;
		}

		public void Append(byte[] data)
		{
			int start = 0, end = data.Length - 1;

			var bufferList = data.ToList();

			bool endIsInThisBuffer = data.Contains(DataFrame.End); // 255 = end
			if (endIsInThisBuffer)
			{
				end = bufferList.IndexOf(DataFrame.End);
				end--; // we dont want to include this byte
			}

			bool startIsInThisBuffer = data.Contains(DataFrame.Start); // 0 = start
			if (startIsInThisBuffer)
			{
				var zeroPos = bufferList.IndexOf(DataFrame.Start);
				if (zeroPos < end) // we might be looking at one of the bytes in the end of the array that hasn't been set
				{
					start = zeroPos;
					start++; // we dont want to include this byte
				}
			}

			//Logger.Log("***" + Encoding.UTF8.GetString(data, start, (end - start) + 1) + "***");
			builder.Append(Encoding.UTF8.GetString(data, start, (end - start) + 1));

			IsComplete = endIsInThisBuffer;
		}

		public override string ToString()
		{
			if (builder != null)
				return builder.ToString();
			else
				return "";
		}

	}
}
