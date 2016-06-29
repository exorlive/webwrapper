using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Threading;

namespace ExorLive.Client.WebWrapper.NamedPipe
{
	public class NPServer
	{
		private Thread _namedPipeListener;
		private NamedPipeServerStream _pipeServer;
		private App _app;

		public void Initialize(App app)
		{
			_app = app;
		}

		public void StartNPServer()
		{
			// start a thread that is listening for Named Pipes calls.
			_namedPipeListener = new Thread(NamedPipeThreadStart);
			_namedPipeListener.IsBackground = true; // To make he thread abort when the application closes down.
			_namedPipeListener.Name = "NamedPipeListener";
			_namedPipeListener.Start();
		}

		private void NamedPipeThreadStart()
		{
			// NOTE:
			// This code is carefully craftet due to complicated threading issues.
			// Test thoroughly if you do any changes.
			// Invariant: there is one object _pipeServer. It is bound to "exorlivepipe" if it is not NULL.
			//            Whenever the _pipeServer is closed, it should be set to NULL.
			while (true)
			{
				try
				{
					var pipeServer = new NamedPipeServerStream("exorlivepipe", PipeDirection.InOut, 1,
						PipeTransmissionMode.Byte, PipeOptions.WriteThrough);
					_pipeServer = pipeServer;
					_pipeServer.WaitForConnection(); // This is a blocking call until a client connects.
				}
				catch (IOException)
				{
					// The "exorlivepipe" is in use by another process / thread.
					return;
				}
				try
				{
					var ss = new StringStream(_pipeServer);
					var request = ss.ReadString();
					string jsonresult;
					var isSuccessfullAsyncCall = HandlePipeRequest(request, out jsonresult);
					if (isSuccessfullAsyncCall)
					{
						// Stop this loop to wait for the async call to finish
						KeepPipeOpenforReply();
						return;
					}
					else
					{
						if (!string.IsNullOrWhiteSpace(jsonresult)) ss.WriteString(jsonresult);
						_pipeServer.Close();
						_pipeServer = null;
					}
				}
				// Catch the IOException that is raised if the pipe is broken
				// or disconnected.
				catch (IOException)
				{
					_pipeServer.Close();
					_pipeServer = null;
				}
			}
		}

		public void KeepPipeOpenforReply()
		{
			// Set at timer. Do not allow for more than 30 seconds until response.
			// Assume that call failed if it took more than 30 seconds.
			var ms = 30 * 1000;
			var timer = new System.Threading.Timer(TimeoutElapsed, null, ms, Timeout.Infinite);
		}

		public void TimeoutElapsed(object state)
		{
			if (_pipeServer != null && _pipeServer.IsConnected)
			{
				// The callback from ExorLive didn't return in a timely fashion. So we return an error.
				PublishDataOnNamedPipe(JsonFormatError("Timeout. No response."));
			}
		}

		public void PublishDataOnNamedPipe(string jsondata)
		{
			if(_pipeServer != null && _pipeServer.IsConnected)
			{
				try
				{
					var ss = new StringStream(_pipeServer);
					ss.WriteString(jsondata);
				}
				// Catch the IOException that is raised if the pipe is broken
				// or disconnected.
				catch (IOException)
				{
				}
				finally
				{
					_pipeServer.Close();
					_pipeServer = null;
				}
			}
			// Start over again - start listening for a connection again
			if(_pipeServer == null)
			{
				StartNPServer();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="requeststring"></param>
		/// <param name="directResult"></param>
		/// <returns>true if we are waiting for an asynch callback.</returns>
		private bool HandlePipeRequest(string requeststring, out string directResult)
		{
			directResult = "";
			var request = Newtonsoft.Json.JsonConvert.DeserializeObject<NamedPipeRequest>(requeststring);
			if (request != null)
			{
				if (!string.IsNullOrWhiteSpace(request.Method))
				{
					switch (request.Method.ToLower())
					{
						case "getworkoutsforclient":
							if (request.Args != null && request.Args.Count > 0)
							{
								var userId = 0;
								var from = DateTime.MinValue;
								foreach (var pair in request.Args)
								{
									if (pair.Key.ToLower() == "userid")
									{
										if (! int.TryParse(pair.Value, out userId))
										{
											directResult = JsonFormatError("Value '{0}' for userId is not an integer.", pair.Value);
											return false;
										}
									}
									if (pair.Key.ToLower() == "from")
									{
										if (! DateTime.TryParse(pair.Value, out from))
										{
											directResult = JsonFormatError("Value '{0}' could not be parsed to a valid datetime.", pair.Value);
											return false;
										}
									}
								}
								if (userId > 0)
								{
									CallGetWorkoutsForClient(userId, from);
									return true;
								}
								else
								{
									directResult = JsonFormatError("No valid userId specified");
								}
							}
							else
							{
								directResult = JsonFormatError("No arguments specified for method '{0}'.", request.Method);
							}
							break;
						case "openworkout":
							if (request.Args != null && request.Args.Count > 0)
							{
								bool foundId = false;
								foreach (var pair in request.Args)
								{
									if (pair.Key.ToLower() == "id")
									{
										foundId = true;
										int id;
										if (int.TryParse(pair.Value, out id))
										{
											OpenWorkout(id);
											return false;
										}
										else
										{
											directResult = JsonFormatError("Value '{0}' for id is not an integer.", pair.Value);
											return false;
										}
									}
								}
								if(!foundId)
								{
									directResult = JsonFormatError("Argument 'id' not specified for method '{0}'.", request.Method);
								}
							}
							else
							{
								directResult = JsonFormatError("No arguments specified for method '{0}'.", request.Method);
							}
							break;
						default:
							directResult = JsonFormatError("Method '{0}' not supported.", request.Method);
							break;
					}
				}
				else
				{
					directResult = JsonFormatError("No method specified");
				}
			}
			else
			{
				directResult = JsonFormatError("Failed to JSON-parse the request");
			}
			return false;
		}

		private void OpenWorkout(int id)
		{
			_app.OpenWorkout(id);
		}

		private void CallGetWorkoutsForClient(int userId, DateTime from)
		{
			_app.GetWorkoutsForClient(userId, from);
		}

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
