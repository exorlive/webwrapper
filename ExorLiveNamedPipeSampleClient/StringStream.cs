using System;
using System.IO;
using System.Text;

namespace ExorLive.Client
{
	public class StringStream
	{
		private readonly Stream _ioStream;
		private readonly Encoding _streamEncoding;

		public StringStream(Stream ioStream)
		{
			_ioStream = ioStream;
			_streamEncoding = Encoding.UTF8;
		}

		public string ReadString()
		{
			// Use 4 bytes to describe the length
			var lengthbuffer = new byte[4];
			_ioStream.Read(lengthbuffer, 0, 4);
			var length = BitConverter.ToInt32(lengthbuffer, 0);
			var inBuffer = new byte[length];
			_ioStream.Read(inBuffer, 0, length);
			return _streamEncoding.GetString(inBuffer);
		}

		public int WriteString(string outString)
		{
			// Use 4 bytes to describe the length
			var outBuffer = _streamEncoding.GetBytes(outString);
			var length = outBuffer.Length;
			var lengthbuffer = BitConverter.GetBytes(length);
			_ioStream.Write(lengthbuffer, 0, 4);
			_ioStream.Write(outBuffer, 0, length);
			_ioStream.Flush();
			return outBuffer.Length + 4;
		}

	}
}
