using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using ExorLive.Client;

namespace ExorLiveNamedPipeSampleClient
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Starting Sample ExorLive client.");
			Console.WriteLine("Will call ExorLive to get some workout data over a Named Pipe");
			Console.WriteLine();
			Console.WriteLine("Enter userid and optionally a date.");
			Console.WriteLine("Example: > 1234 2016-01-01");
			Console.WriteLine();
			while (true)
			{
				Console.Write("> ");
				string line = Console.ReadLine().Trim();
				if (line.ToLower() == "exit" || line.ToLower() == "quit" || line.ToLower() == "q" ||  line.ToLower() == "bye") break;

				if (!string.IsNullOrEmpty(line))
				{
					int index = line.IndexOf(" ");
					if (index > 0)
					{
						Call(line.Substring(0, index), line.Substring(index+1));
					}
					else
					{
						Call(line, null);
					}
				}
			}
			Console.WriteLine("DONE");
		}

		static void Call(string userid, string from)
		{
			// TODO: Make sure that Exorlive Webwrapper is running.

			NamedPipeRequest request = new NamedPipeRequest();
			request.Method = "GetWorkoutsForClient";
			request.Args = new Dictionary<string, string>(2);
			request.Args.Add("userId", userid);
			if(from != null) request.Args.Add("from", from);
			string json = Newtonsoft.Json.JsonConvert.SerializeObject(request);

			NamedPipeClientStream pipeClient =
								new NamedPipeClientStream(".", "exorlivepipe", PipeDirection.InOut, PipeOptions.None);
			pipeClient.Connect();

			StringStream ss = new StringStream(pipeClient);
			ss.WriteString(json);
			string response = ss.ReadString();
			Console.WriteLine("------------------------------------------------------------");
			Console.WriteLine(response);
			Console.WriteLine("------------------------------------------------------------");
		}
	}

	public class NamedPipeRequest
	{
		public string Method { get; set; }
		public Dictionary<string, string> Args { get; set; }
	}

}
