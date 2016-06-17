using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ExorLive.Client.Entities;

namespace ExorLive.Client
{
	public class NPServer
	{
		private Thread _namedPipeListener;

		public void StartNPServer()
		{
			// start a thread that is listening for Named Pipes calls.
			_namedPipeListener = new Thread(NamedPipeThreadStart);
			_namedPipeListener.Start();
		}

		public void StopNPServer()
		{
			if (_namedPipeListener != null)
			{
				_namedPipeListener.Abort();
			}
		}

		private void NamedPipeThreadStart()
		{
			while (true)
			{
				NamedPipeServerStream pipeServer = new NamedPipeServerStream("exorlivepipe", PipeDirection.InOut, 1,
					PipeTransmissionMode.Byte, PipeOptions.WriteThrough);
				pipeServer.WaitForConnection();
				try
				{
					StringStream ss = new StringStream(pipeServer);
					string request = ss.ReadString();
					string response = HandlePipeRequest(request);
					ss.WriteString(response);
				}
				// Catch the IOException that is raised if the pipe is broken
				// or disconnected.
				catch (IOException)
				{
					// TODO: Log the error
				}
				finally
				{
					pipeServer.Close();
				}
			}
		}

		private string HandlePipeRequest(string requeststring)
		{
			NamedPipeRequest request = Newtonsoft.Json.JsonConvert.DeserializeObject<NamedPipeRequest>(requeststring);
			if (request != null)
			{
				if (!string.IsNullOrWhiteSpace(request.Method))
				{
					switch (request.Method.ToLower())
					{
						case "getworkoutsforclient":
							if (request.Args != null && request.Args.Count > 0)
							{
								int userId = 0;
								DateTime from = DateTime.MinValue;
								foreach (var pair in request.Args)
								{
									if (pair.Key.ToLower() == "userid")
									{
										if (! int.TryParse(pair.Value, out userId))
										{
											return JsonFormatError("Value '{0}' for userId is not an integer.", pair.Value);
										}
									}
									if (pair.Key.ToLower() == "from")
									{
										if (! DateTime.TryParse(pair.Value, out from))
										{
											return JsonFormatError("Value '{0}' could not be parsed to a valid datetime.", pair.Value);
										}
									}
								}
								if (userId > 0)
								{
									return GetWorkoutsForClient(userId, from);
								}
								else
								{
									return JsonFormatError("No valid userId specified");
								}
							}
							else
							{
								return JsonFormatError("No arguments specified for method '{0}'.", request.Method);
							}
							break;
						default:
							return JsonFormatError("Method '{0}' not supported.", request.Method);
					}
				}
				else
				{
					return JsonFormatError("No method specified");
				}
			}
			else
			{
				return JsonFormatError("Failed to JSON-parse the request");
			}

		}

		private string GetWorkoutsForClient(int userId, DateTime from)
		{
			var list = GetDummyWorkouts(userId, from);
			string json = Newtonsoft.Json.JsonConvert.SerializeObject(list);
			return json;
		}

		#region dummydata

		private List<Workout> GetDummyWorkouts(int userId, DateTime from)
		{
			// Create some dummy data
			List<Workout> workouts = new List<Workout>(12);
			DateTime at;
			DateTime start = new DateTime(2015,6,1);
			for (int i = 0; i <=11; i++)
			{
				at = start.AddMonths(i);
				if(at >= from)
					workouts.Add(GetDummyWorkout(userId, at, at.ToString("MMMM"), 3 + (i%5)));
			}
			return workouts;
		}

		private Workout GetDummyWorkout(int userId, DateTime createdAt, string name, int exercisecount)
		{
			int id = userId + (int) (createdAt - new DateTime(2000, 1, 1)).TotalSeconds;
			if (id < 0) id = -id;
			int startindex = id%100;
			List<Exercise> exercises = new List<Exercise>(exercisecount);
			for (int i = 1; i <= exercisecount; i++)
			{
				int index = (startindex + i)%100;
				exercises.Add(new Exercise()
				{
					Id = dummyexercises[index].Item1,
					Name = dummyexercises[index].Item2,
					Data = new List<ExerciseData>() {new ExerciseData(){Key = "Vekt", Unit = "Kg", Value = (10 + (index%10)).ToString()}}
				});
			}
			var w = new Workout()
			{
				Id = id,
				Name = name,
				Description = "Dette er en beskrivelse av dette treningsprogrammet.",
				LastChangedAt = createdAt,
				Owner = 1,
				Executor = userId,
				ExerciseCount = exercisecount,
				Exercises = exercises
			};
			return w;
		}

		private List<Tuple<int, string>> dummyexercises = new List<Tuple<int, string>>
		{
			new Tuple<int, string>(3, "Bekkenvipp"),
			new Tuple<int, string>(6, "Firfotstående sidehev m/rotasjon"),
			new Tuple<int, string>(7, "Firfotstående krum - svai 1"),
			new Tuple<int, string>(10, "Firfotstående 'sumo'"),
			new Tuple<int, string>(11, "Liggende seteløft"),
			new Tuple<int, string>(12, "Liggende ettbens seteløft"),
			new Tuple<int, string>(13, "Mageliggende rygghev 1"),
			new Tuple<int, string>(15, "Mageliggende benhev 1"),
			new Tuple<int, string>(16, "Sit up 1"),
			new Tuple<int, string>(17, "Sit up 2"),
			new Tuple<int, string>(18, "Sit up m/feste 1"),
			new Tuple<int, string>(19, "Bekkenhev 1"),
			new Tuple<int, string>(20, "Utside hofte 1"),
			new Tuple<int, string>(21, "Firfotstående diagonalhev"),
			new Tuple<int, string>(22, "Strakliggende seteløft"),
			new Tuple<int, string>(23, "Dips på en benk"),
			new Tuple<int, string>(26, "Sideligende bekkenløft 1"),
			new Tuple<int, string>(27, "Albuestøttende seteløft"),
			new Tuple<int, string>(28, "Push up på knærne 2"),
			new Tuple<int, string>(29, "Push up 2"),
			new Tuple<int, string>(30, "Push up smalt grep 1"),
			new Tuple<int, string>(31, "Sideligende bekkenløft 2"),
			new Tuple<int, string>(32, "Firfotstående benhev ut"),
			new Tuple<int, string>(52, "Benpress 1"),
			new Tuple<int, string>(53, "Knestrekk 1"),
			new Tuple<int, string>(54, "Knebøy 4"),
			new Tuple<int, string>(55, "Knebøy i Smith-stativ 1"),
			new Tuple<int, string>(56, "Lårcurl"),
			new Tuple<int, string>(57, "Sittende lårcurl"),
			new Tuple<int, string>(58, "Stående lårcurl"),
			new Tuple<int, string>(59, "Markløft m/strake ben 1"),
			new Tuple<int, string>(60, "'God morgen'"),
			new Tuple<int, string>(61, "Innoverføring ben 1"),
			new Tuple<int, string>(62, "Stående bentrekk inn 2"),
			new Tuple<int, string>(63, "Utfall til siden 2"),
			new Tuple<int, string>(64, "Sittende benføring ut 1"),
			new Tuple<int, string>(65, "Stående hoftestrekk 2"),
			new Tuple<int, string>(66, "Stående benhev ut 3"),
			new Tuple<int, string>(67, "Stående benhev bak"),
			new Tuple<int, string>(68, "Utfall fram 2"),
			new Tuple<int, string>(69, "Stående tåhev 1"),
			new Tuple<int, string>(70, "Sittende tåhev"),
			new Tuple<int, string>(71, "Ankelstrekk"),
			new Tuple<int, string>(74, "Brystpress 1"),
			new Tuple<int, string>(75, "Benkpress 1"),
			new Tuple<int, string>(76, "Benkpress i Smith-stativ"),
			new Tuple<int, string>(77, "Pec dec"),
			new Tuple<int, string>(78, "Pullover"),
			new Tuple<int, string>(79, "Liggende pullover 1"),
			new Tuple<int, string>(80, "Flies"),
			new Tuple<int, string>(81, "Stående kryssdrag 1"),
			new Tuple<int, string>(82, "Skrå benkpress 1"),
			new Tuple<int, string>(83, "Skrå brystpress"),
			new Tuple<int, string>(84, "Skrå benkpress i Smith-stativ"),
			new Tuple<int, string>(85, "Liggende brystpress"),
			new Tuple<int, string>(86, "Skrå flies 1"),
			new Tuple<int, string>(87, "Nedtrekk til nakke"),
			new Tuple<int, string>(88, "Nedtrekk til bryst"),
			new Tuple<int, string>(89, "Nedtrekk m/smalt grep"),
			new Tuple<int, string>(90, "Nedtrekk til skulder 1"),
			new Tuple<int, string>(91, "Roing m/støtte smalt grep"),
			new Tuple<int, string>(92, "Sittende roing 1"),
			new Tuple<int, string>(93, "Sittende enarms roing"),
			new Tuple<int, string>(94, "Ryggstrekk 1"),
			new Tuple<int, string>(95, "Rygghev 1"),
			new Tuple<int, string>(96, "Omvendt pec dec 1"),
			new Tuple<int, string>(97, "Kne- og håndstøttende roing"),
			new Tuple<int, string>(98, "Kroppsheving m/bredt grep"),
			new Tuple<int, string>(100, "Kroppsheving m/smalt grep"),
			new Tuple<int, string>(101, "Ryggliggende omvendt kryssdrag"),
			new Tuple<int, string>(102, "Skulderpress 1"),
			new Tuple<int, string>(103, "Sidehev"),
			new Tuple<int, string>(108, "Stående strak sidehev"),
			new Tuple<int, string>(110, "Stående drag til hake 1"),
			new Tuple<int, string>(112, "Stående fronthev 1"),
			new Tuple<int, string>(113, "Stående fronthev 2"),
			new Tuple<int, string>(114, "Skrå enarms fronthev 1"),
			new Tuple<int, string>(115, "Foroversittende sidehev"),
			new Tuple<int, string>(122, "Skråsittende benpress"),
			new Tuple<int, string>(123, "Skråsittende ettbens press"),
			new Tuple<int, string>(125, "Sittende benpress"),
			new Tuple<int, string>(126, "Skråsittende tåhev"),
			new Tuple<int, string>(127, "Liggende tåhev"),
			new Tuple<int, string>(128, "Knestrekk 2"),
			new Tuple<int, string>(129, "Ettbens knestrekk 1"),
			new Tuple<int, string>(130, "Ettbens knestrekk 2"),
			new Tuple<int, string>(131, "Ettbens knebøy i Smith-stativ"),
			new Tuple<int, string>(132, "Ettbens knebøy 2"),
			new Tuple<int, string>(133, "Knebøy 5"),
			new Tuple<int, string>(134, "Ettbens lårcurl"),
			new Tuple<int, string>(135, "Markløft"),
			new Tuple<int, string>(136, "Innoverføring ben 2"),
			new Tuple<int, string>(137, "Sittende benføring ut 2"),
			new Tuple<int, string>(138, "Stående hoftestrekk 4"),
			new Tuple<int, string>(139, "Brystpress 2"),
			new Tuple<int, string>(140, "Stående kryssdrag 3"),
			new Tuple<int, string>(141, "Nedtrekk m/spesialstang 1"),
			new Tuple<int, string>(142, "Nedtrekk m/spesialhåndtak"),
			new Tuple<int, string>(143, "Nedtrekk til skulder 2"),
			new Tuple<int, string>(144, "Roing m/støtte 1")
		};
		#endregion


		private string JsonFormatError(string error, params object[] args)
		{
			return string.Format("{{ \"error\": \"{0}\" }} ", string.Format(error, args).Replace('\"', '\''));
		}

	}

	public class NamedPipeRequest
	{
		public string Method { get; set; }
		public Dictionary<string, string> Args { get; set; }
	}

}
