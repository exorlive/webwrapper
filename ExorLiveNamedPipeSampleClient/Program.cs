using System;
using System.Collections.Generic;
using System.IO.Pipes;
using ExorLive.Client;
using System.Diagnostics;

namespace ExorLiveNamedPipeSampleClient
{
	internal class Program
	{
		private static string GetPipename(int processId)
		{
			return "exorlivepipe." + processId;
		}
		private static string WebwrapperProcessName
		{
			get { return "ExorLive.Client.WebWrapper"; }
		}

		private static NamedPipeClientStream GetPipeClient()
		{
			int processId = ProcessTool.FindProcessIdByName(WebwrapperProcessName);
			if(processId==0)
			{
				// For running in debugging tool.
				processId = ProcessTool.FindProcessIdByName(WebwrapperProcessName + ".vshost");
			}

			if(processId > 0)
			{
				var pipeClient = new NamedPipeClientStream(".", GetPipename(processId), PipeDirection.InOut, PipeOptions.None);
				pipeClient.Connect();
				return pipeClient;
			}
			else
			{
				// Webwrapper is not running.
				Console.WriteLine("***** WebWrapper is not running");
				return null;
			}
		}

		private static void Main()
		{
			Console.WriteLine("Starting Sample ExorLive client.");
			Console.WriteLine("v 4.0");
			Console.WriteLine("--------------------------------");
			Console.WriteLine("Starting Sample ExorLive client.");
			Console.WriteLine("Will call ExorLive to get some workout data over a Named Pipe");
			Console.WriteLine("Type 'help' for list of commands");
			while (true)
			{
				char[] splitter = { ' ' };
				Console.Write("> ");
				var line = Console.ReadLine()?.Trim();
				if (!string.IsNullOrWhiteSpace(line))
				{
					line = line.Trim();
					string lower = line.ToLower();
					if (lower == "exit" || lower == "quit" || lower == "q" || lower == "bye") break;
					if (lower == "help")
					{
						Console.WriteLine("--------------------------------");
						Console.WriteLine("> get <id> [from]");
						Console.WriteLine("> get 123 ");
						Console.WriteLine("> get x123 2016-06-01");
						Console.WriteLine("");
						Console.WriteLine("> workout <workout-Id> ");
						Console.WriteLine("> workout 123 ");
						Console.WriteLine("");
						Console.WriteLine("> selectperson id=<id> [<property>=<propertyvalue> <property>=<propertyvalue> ...]");
						Console.WriteLine("   Possible properties are:");
						Console.WriteLine("       firstname");
						Console.WriteLine("       lastname");
						Console.WriteLine("       email");
						Console.WriteLine("       address");
						Console.WriteLine("       phonehome");
						Console.WriteLine("       phonework");
						Console.WriteLine("       mobile");
						Console.WriteLine("       country");
						Console.WriteLine("       zipcode");
						Console.WriteLine("       location");
						Console.WriteLine("       dateOfBirth  (value in the format yyyy-MM-dd)");
						Console.WriteLine("");
						Console.WriteLine("> show");
						Console.WriteLine("> hide");
						Console.WriteLine("> close");
						Console.WriteLine("> version");
						Console.WriteLine("   Get the version number of the webwrapper");
						Console.WriteLine("> log");
						Console.WriteLine("   Enable logging in Webwrapper.");
						Console.WriteLine("");
						Console.WriteLine("> exit");
						Console.WriteLine("   Close this console application");
						Console.WriteLine("--------------------------------");
					}
					if (lower == "show")
					{
						Show();
					}
					if (lower == "hide")
					{
						Hide();
					}
					if (lower == "close")
					{
						Close();
					}
					if (lower == "version")
					{
						WebwrapperVersion();
					}
					if (lower == "log")
					{
						TurnOnLogging();
					}
					if (lower.StartsWith("get "))
					{
						string[] array = line.Split(splitter, StringSplitOptions.RemoveEmptyEntries);
						if (array.Length == 2)
						{
							GetWorkoutData(array[1], null);
						}
						else if (array.Length > 2)
						{
							GetWorkoutData(array[1], array[2]);
						}
					}
					if (lower.StartsWith("workout "))
					{
						string[] array = line.Split(splitter, StringSplitOptions.RemoveEmptyEntries);
						if (array.Length > 1)
						{
							OpenWorkout(array[1]);
						}
					}
					if (lower.StartsWith("selectperson "))
					{
						string[] array = line.Split(splitter, StringSplitOptions.RemoveEmptyEntries);
						if (array.Length > 1)
						{
							SelectPerson(array);
						}
					}
				}
			}
			Console.WriteLine("DONE");
		}

		private static void WebwrapperVersion()
		{
			NamedPipeRequest request = new NamedPipeRequest();
			request.Method = "version";
			string json = Newtonsoft.Json.JsonConvert.SerializeObject(request);

			NamedPipeClientStream pipeClient = GetPipeClient();
			if (pipeClient != null)
			{
				StringStream ss = new StringStream(pipeClient);
				ss.WriteString(json);
				var response = ss.ReadString();
				Console.WriteLine(response);
				Console.WriteLine("------------------------------------------------------------");
			}
		}

		private static void TurnOnLogging()
		{
			NamedPipeRequest request = new NamedPipeRequest();
			request.Method = "log";
			string json = Newtonsoft.Json.JsonConvert.SerializeObject(request);

			NamedPipeClientStream pipeClient = GetPipeClient();
			if (pipeClient != null)
			{
				StringStream ss = new StringStream(pipeClient);
				ss.WriteString(json);
				Console.WriteLine("------------------------------------------------------------");
			}
		}

		static void SelectPerson(string[] array)
		{
			var request = new NamedPipeRequest
			{
				Method = "SelectPerson"
			};
			request.Args = new Dictionary<string, string>(array.Length -1);
			foreach (string item in array)
			{
				int index = item.IndexOf("=");
				if (index > 0 && index < item.Length - 1)
				{
					request.Args.Add(item.Substring(0, index), item.Substring(index + 1));
				}
			}
			var json = Newtonsoft.Json.JsonConvert.SerializeObject(request);

			NamedPipeClientStream pipeClient = GetPipeClient();
			if(pipeClient != null)
			{
				var ss = new StringStream(pipeClient);
				ss.WriteString(json);
				var response = ss.ReadString();
				Console.WriteLine("------------------------------------------------------------");
				Console.WriteLine(response);
				Console.WriteLine("------------------------------------------------------------");
			}
		}

		static void Show()
		{
			NamedPipeRequest request = new NamedPipeRequest();
			request.Method = "Show";
			string json = Newtonsoft.Json.JsonConvert.SerializeObject(request);

			NamedPipeClientStream pipeClient = GetPipeClient();
			if (pipeClient != null)
			{

				StringStream ss = new StringStream(pipeClient);
				ss.WriteString(json);
				Console.WriteLine("------------------------------------------------------------");
			}
		}
		static void Hide()
		{
			NamedPipeRequest request = new NamedPipeRequest();
			request.Method = "Hide";
			string json = Newtonsoft.Json.JsonConvert.SerializeObject(request);

			NamedPipeClientStream pipeClient = GetPipeClient();
			if (pipeClient != null)
			{
				StringStream ss = new StringStream(pipeClient);
				ss.WriteString(json);
				Console.WriteLine("------------------------------------------------------------");
			}
		}
		static void Close()
		{
			NamedPipeRequest request = new NamedPipeRequest();
			request.Method = "Close";
			string json = Newtonsoft.Json.JsonConvert.SerializeObject(request);

			NamedPipeClientStream pipeClient = GetPipeClient();
			if (pipeClient != null)
			{
				StringStream ss = new StringStream(pipeClient);
				ss.WriteString(json);
				Console.WriteLine("------------------------------------------------------------");
			}
		}

		static void OpenWorkout(string wid)
		{
			NamedPipeRequest request = new NamedPipeRequest();
			request.Method = "OpenWorkout";
			request.Args = new Dictionary<string, string>(1);
			request.Args.Add("id", wid);
			string json = Newtonsoft.Json.JsonConvert.SerializeObject(request);

			NamedPipeClientStream pipeClient = GetPipeClient();
			if (pipeClient != null)
			{
				StringStream ss = new StringStream(pipeClient);
				ss.WriteString(json);
				Console.WriteLine("------------------------------------------------------------");
			}
		}

		static void GetWorkoutData(string customId, string from)
		{
			var request = new NamedPipeRequest
			{
				Method = "GetWorkoutsForClient",
				Args = new Dictionary<string, string>(2) {{"customId", customId } }
			};
			if(from != null) request.Args.Add("from", from);
			var json = Newtonsoft.Json.JsonConvert.SerializeObject(request);

			NamedPipeClientStream pipeClient = GetPipeClient();
			if (pipeClient != null)
			{
				var ss = new StringStream(pipeClient);
				ss.WriteString(json);
				var response = ss.ReadString();
				Console.WriteLine("------------------------------------------------------------");
				Console.WriteLine(response);
				Console.WriteLine("------------------------------------------------------------");
				if (response.StartsWith("["))
				{
					var workoutlist = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ExorLive.Client.Entities.Workout>>(response);
					Console.WriteLine("Workouts: " + workoutlist.Count);
					foreach (var workout in workoutlist)
					{
						Console.WriteLine("  {0}: {1} {2}", workout.Id, workout.LastChangedAt.ToString("yyyy-MM-dd"), workout.Name);
					}
				}
			}
		}
	}

	public class NamedPipeRequest
	{
		public string Method { get; set; }
		public Dictionary<string, string> Args { get; set; }
	}

}
