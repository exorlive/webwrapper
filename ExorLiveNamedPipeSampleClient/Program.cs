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
		private static string WebwrapperProcessName => "ExorLive.Client.WebWrapper";
		private static NamedPipeClientStream GetPipeClient()
		{
			var processId = ProcessTool.FindProcessIdByName(WebwrapperProcessName);
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
					var lower = line.ToLower();
					if (lower == "exit" || lower == "quit" || lower == "q" || lower == "bye") {
						break;
					}

					if (lower == "help")
					{
						Console.WriteLine("--------------------------------");
						Console.WriteLine("> listw <userid> [customid] [from]");
						Console.WriteLine("> listw 0 x123 2016-06-01 ");
						Console.WriteLine("> listw 123 0 2016-06-01");
						Console.WriteLine("> listw 123");
						Console.WriteLine("> listw 0 23");
						Console.WriteLine("      get a list of workouts for the given user. Specify either userid or customid. Use 0 for unspecified id.");
						Console.WriteLine("");
						Console.WriteLine("> workout <workout-Id> ");
						Console.WriteLine("> workout 123 ");
						Console.WriteLine("       open/show the specified workout in the ExorLive GUI");
						Console.WriteLine("");
						Console.WriteLine("> selectperson [<property>=<propertyvalue> <property>=<propertyvalue> ...]");
						Console.WriteLine("   Possible properties are:");
						Console.WriteLine("       id (obsolete, but is interpreted as customid)");
						Console.WriteLine("       customid (the id of this person in your system)");
						Console.WriteLine("       userid (the exorlive id of this user), use 0 for new contact/person.");
						Console.WriteLine("       firstname");
						Console.WriteLine("       lastname");
						Console.WriteLine("       email");
						Console.WriteLine("       address");
						Console.WriteLine("       phonehome");
						Console.WriteLine("       phonework");
						Console.WriteLine("       mobile");
						Console.WriteLine("       gender (M or F)");
						Console.WriteLine("       country");
						Console.WriteLine("       zipcode");
						Console.WriteLine("       location");
						Console.WriteLine("       dateOfBirth  (value in the format yyyy-MM-dd)");
						Console.WriteLine("");
						Console.WriteLine("> users <id>");
						Console.WriteLine("       list all your users in exorlive or filter by the id (the 'id' is your id  (customid) and may not be unique in ExorLive)");
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
					if (lower == "users" || lower.StartsWith("users "))
					{
						string customid = null;
						string[] array = line.Split(splitter, StringSplitOptions.RemoveEmptyEntries);
						if (array.Length > 1) {
							customid = array[1];
						}

						GetListOfUsers(customid);
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
					if (lower.StartsWith("listw "))
					{
						string[] array = line.Split(splitter, StringSplitOptions.RemoveEmptyEntries);
						if (array.Length > 1)
						{
							var userId = array[1];
							var customId = "";
							var from = "";
							if (array.Length > 2) {
								customId = array[2];
							}

							if (array.Length > 3) {
								from = array[3];
							}

							GetWorkoutData(userId, customId, from);
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
			var request = new NamedPipeRequest() {
				Method = "version"
			};
			var json = Newtonsoft.Json.JsonConvert.SerializeObject(request);

			NamedPipeClientStream pipeClient = GetPipeClient();
			if (pipeClient != null)
			{
				var ss = new StringStream(pipeClient);
				ss.WriteString(json);
				var response = ss.ReadString();
				Console.WriteLine(response);
				Console.WriteLine("------------------------------------------------------------");
			}
		}

		private static void TurnOnLogging()
		{
			var request = new NamedPipeRequest() {
				Method = "log"
			};
			var json = Newtonsoft.Json.JsonConvert.SerializeObject(request);

			NamedPipeClientStream pipeClient = GetPipeClient();
			if (pipeClient != null)
			{
				var ss = new StringStream(pipeClient);
				ss.WriteString(json);
				Console.WriteLine("------------------------------------------------------------");
			}
		}

		static void SelectPerson(string[] array)
		{
			var request = new NamedPipeRequest2
			{
				Method = "SelectPerson"
			};
			request.Args = new Dictionary<string, object>(array.Length);
			foreach (var item in array)
			{
				var index = item.IndexOf("=");
				if (index > 0 && index < item.Length - 1)
				{
					request.Args.Add(item.Substring(0, index), item.Substring(index + 1));
				}
			}
			// Add some ProfileData
			var data = new List<Dictionary<string, string>>(3) {
				new Dictionary<string, string> { { "key", "diagnosis" }, { "name", "Diagnose" }, { "value", "Lumbago" }, { "date", "2016-08-24 14:04" }, { "source", "MyJournal" } },
				new Dictionary<string, string> { { "key", "diagnosiscode" }, { "name", "Diagnosekode" }, { "value", "L2" }, { "unit", "ICPC-2" }, { "date", "2016-08-24 14:04" }, { "source", "MyJournal" } },
				new Dictionary<string, string> { { "key", "weight" }, { "name", "Vekt" }, { "value", "80" }, { "unit", "kg" }, { "date", "2016-08-23" }, { "source", "ExorLive GO" }, { "description", "På badevekta om morgenen" } }
			};
			request.Args.Add("profiledata", data);

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
			var request = new NamedPipeRequest() {
				Method = "Show"
			};
			var json = Newtonsoft.Json.JsonConvert.SerializeObject(request);

			NamedPipeClientStream pipeClient = GetPipeClient();
			if (pipeClient != null)
			{

				var ss = new StringStream(pipeClient);
				ss.WriteString(json);
				Console.WriteLine("------------------------------------------------------------");
			}
		}
		static void Hide()
		{
			var request = new NamedPipeRequest() {
				Method = "Hide"
			};
			var json = Newtonsoft.Json.JsonConvert.SerializeObject(request);

			NamedPipeClientStream pipeClient = GetPipeClient();
			if (pipeClient != null)
			{
				var ss = new StringStream(pipeClient);
				ss.WriteString(json);
				Console.WriteLine("------------------------------------------------------------");
			}
		}
		static void Close()
		{
			var request = new NamedPipeRequest() {
				Method = "Close"
			};
			var json = Newtonsoft.Json.JsonConvert.SerializeObject(request);

			NamedPipeClientStream pipeClient = GetPipeClient();
			if (pipeClient != null)
			{
				var ss = new StringStream(pipeClient);
				ss.WriteString(json);
				Console.WriteLine("------------------------------------------------------------");
			}
		}

		static void OpenWorkout(string wid)
		{
			var request = new NamedPipeRequest() {
				Method = "OpenWorkout",
				Args = new Dictionary<string, string>(1)
			};
			request.Args.Add("id", wid);
			var json = Newtonsoft.Json.JsonConvert.SerializeObject(request);

			NamedPipeClientStream pipeClient = GetPipeClient();
			if (pipeClient != null)
			{
				var ss = new StringStream(pipeClient);
				ss.WriteString(json);
				Console.WriteLine("------------------------------------------------------------");
			}
		}

		static void GetWorkoutData(string userId, string customId, string from)
		{
			var request = new NamedPipeRequest
			{
				Method = "GetWorkoutsForClient",
				Args = new Dictionary<string, string>(3)
			};
			if (!string.IsNullOrWhiteSpace(userId) && userId != "0") {
				request.Args.Add("userid", userId);
			}

			if (!string.IsNullOrWhiteSpace(customId) && customId != "0") {
				request.Args.Add("customid", customId);
			}

			if (!string.IsNullOrWhiteSpace(from) && from != "0") {
				request.Args.Add("from", from);
			}

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
						Console.WriteLine("  {0}: {1} Owner: {2} Executor: {3} Name: {4} ", workout.Id, workout.LastChangedAt.ToString("yyyy-MM-dd"), workout.Owner, workout.Executor, workout.Name);
					}
				}
			}
		}

		static void GetListOfUsers(string customId)
		{
			var request = new NamedPipeRequest
			{
				Method = "GetListOfUsers"
			};
			if(!string.IsNullOrEmpty(customId))
			{
				request.Args = new Dictionary<string, string>(1) { { "customId", customId } };
			}
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
					dynamic userslist = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(response);
					Console.WriteLine("Users: " + userslist.Count);
					foreach (var user in userslist)
					{
						string date = user.DateOfBirth;
						if (date.StartsWith("000")) {
							date = "";
						}

						Console.WriteLine("  {0,8}: CustomId: {1,8} Date-of-birth: {2,11}  - {3} {4} - {5}", user.Id, user.CustomId, date, user.Firstname, user.Lastname, user.Email);
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
	public class NamedPipeRequest2
	{
		public string Method { get; set; }
		public Dictionary<string, object> Args { get; set; }
	}

}
