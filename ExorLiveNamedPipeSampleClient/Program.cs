using System;
using System.Collections.Generic;
using System.IO.Pipes;
using ExorLive.Client;

namespace ExorLiveNamedPipeSampleClient
{
	internal class Program
	{
		private static void Main()
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
				var line = Console.ReadLine()?.Trim();
				if (line != null && (line.ToLower() == "exit" || line.ToLower() == "quit" || line.ToLower() == "q" ||  line.ToLower() == "bye")) break;

				if (!string.IsNullOrEmpty(line))
				{
					var index = line.IndexOf(" ", StringComparison.Ordinal);
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

			var request = new NamedPipeRequest
			{
				Method = "GetWorkoutsForClient",
				Args = new Dictionary<string, string>(2) {{"userId", userid}}
			};
			if(from != null) request.Args.Add("from", from);
			var json = Newtonsoft.Json.JsonConvert.SerializeObject(request);

			var pipeClient =
								new NamedPipeClientStream(".", "exorlivepipe", PipeDirection.InOut, PipeOptions.None);
			pipeClient.Connect();

			var ss = new StringStream(pipeClient);
			ss.WriteString(json);
			var response = ss.ReadString();
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
