using System;
using System.IO;
using System.Text;

namespace ExorLive.Client.WebWrapper.NamedPipe
{
	public class StringStream
	{
		private Stream ioStream;
		private Encoding streamEncoding;

		public StringStream(Stream ioStream)
		{
			this.ioStream = ioStream;
			streamEncoding = Encoding.UTF8;
		}

		public string ReadString()
		{
			// Use 4 bytes to describe the length
			var lengthbuffer = new byte[4];
			ioStream.Read(lengthbuffer, 0, 4);
			var length = BitConverter.ToInt32(lengthbuffer, 0);
			var inBuffer = new byte[length];
			ioStream.Read(inBuffer, 0, length);
			return streamEncoding.GetString(inBuffer);
		}

		public int WriteString(string outString)
		{
			// Use 4 bytes to describe the length
			var outBuffer = streamEncoding.GetBytes(outString);
			var length = outBuffer.Length;
			var lengthbuffer = BitConverter.GetBytes(length);
			ioStream.Write(lengthbuffer, 0, 4);
			ioStream.Write(outBuffer, 0, length);
			ioStream.Flush();
			return outBuffer.Length + 4;
		}
	}
}
