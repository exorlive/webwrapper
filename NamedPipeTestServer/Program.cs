using ExorLive.Client;

namespace NamedPipeTestServer
{
	internal class Program
	{
		private static void Main()
		{
			var server = new NpServer();
			server.StartNpServer();
		}
	}
}
