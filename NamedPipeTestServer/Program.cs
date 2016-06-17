using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExorLive.Client;

namespace NamedPipeTestServer
{
	class Program
	{
		static void Main(string[] args)
		{
			NPServer server = new NPServer();
			server.StartNPServer();
		}
	}
}
