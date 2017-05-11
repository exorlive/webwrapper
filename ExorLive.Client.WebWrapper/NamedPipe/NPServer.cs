using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading;

namespace ExorLive.Client.WebWrapper.NamedPipe
{
	public class NpServer
	{
		private Thread _namedPipeListener;
		private NamedPipeServerStream _pipeServer;
		private App _app;
		private MainWindow _window;

		public void Initialize(App app, MainWindow window)
		{
			_app = app;
			_window = window;
			_app.Log("Initialize WebWrapper");
		}

		private string Pipename => "exorlivepipe." + Process.GetCurrentProcess().Id;

		public void StartNpServer()
		{
			// start a thread that is listening for Named Pipes calls.
			_namedPipeListener = new Thread(NamedPipeThreadStart)
			{
				IsBackground = true,
				Name = "NamedPipeListener"
			};
			// To make he thread abort when the application closes down.
			_app.Log("StartNpServer");
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
                    var rule = new PipeAccessRule(new SecurityIdentifier("S-1-1-0"), PipeAccessRights.ReadWrite, System.Security.AccessControl.AccessControlType.Allow);
                    var pipeSecurity = new PipeSecurity();
                    pipeSecurity.AddAccessRule(rule);

                    var pipeServer = new NamedPipeServerStream(Pipename, PipeDirection.InOut, 1,
						PipeTransmissionMode.Byte, PipeOptions.WriteThrough, 1024, 1024, pipeSecurity, HandleInheritability.None);
					_pipeServer = pipeServer;
					_pipeServer.WaitForConnection(); // This is a blocking call until a client connects.
					if (!_app.ExorLiveIsRunning)
					{
						_window.Dispatcher.BeginInvoke(new Action(() =>
						{
							_window.Restore();
						}));
						SpinWait.SpinUntil(() => _app.ExorLiveIsRunning, -1);   //Blocks the thread until the user has logged in.
					}
				}
				catch (IOException ex)
				{
					// The "exorlivepipe" is in use by another process / thread.
					_app.Log("NamedPipeThreadStart(1) EXCEPTION: {0}", ex.Message);
					return;
				}
				try
				{
					var ss = new StringStream(_pipeServer);
					var request = ss.ReadString();
					var isSuccessfullAsyncCall = HandlePipeRequest(request, out var jsonresult);
					if (isSuccessfullAsyncCall)
					{
						// Stop this loop to wait for the async call to finish
						KeepPipeOpenforReply();
						return;
					}
					else
					{
						if (!string.IsNullOrWhiteSpace(jsonresult)) {
							ss.WriteString(jsonresult);
						}

						_pipeServer.Close();
						_pipeServer = null;
					}
				}
				// Catch the IOException that is raised if the pipe is broken
				// or disconnected.
				catch (IOException ex)
				{
					_app.Log("NamedPipeThreadStart(2) EXCEPTION: {0}", ex.Message);
					_pipeServer?.Close();
					_pipeServer = null;
				}
			}
		}

		private void KeepPipeOpenforReply()
		{
			// Set at timer. Do not allow for more than 30 seconds until response.
			// Assume that call failed if it took more than 30 seconds.
			const int ms = 30 * 1000;
			// ReSharper disable once UnusedVariable
			var timer = new Timer(TimeoutElapsed, null, ms, Timeout.Infinite);
		}

		private void TimeoutElapsed(object state)
		{
			if (_pipeServer != null && _pipeServer.IsConnected)
			{
				// The callback from ExorLive didn't return in a timely fashion. So we return an error.
				PublishDataOnNamedPipe(JsonFormatError("Timeout. No response."));
			}
		}

		public void PublishDataOnNamedPipe(string jsondata)
		{
			if (_pipeServer != null && _pipeServer.IsConnected)
			{
				try
				{
					var ss = new StringStream(_pipeServer);
					ss.WriteString(jsondata);
				}
				// Catch the IOException that is raised if the pipe is broken
				// or disconnected.
				catch (IOException ex)
				{
					_app.Log("PublishDataOnNamedPipe EXCEPTION: {0}", ex.Message);
					_app.Log(jsondata);
				}
				finally
				{
					_pipeServer?.Close();
					_pipeServer = null;
				}
			}
			// Start over again - start listening for a connection again
			if (_pipeServer == null)
			{
				StartNpServer();
			}
		}

		private bool HandleSelectPerson(NamedPipeRequest2 request, out string directResult)
		{
			directResult = "";
			if (request.Args != null && request.Args.Count > 0)
			{
				var dto = new PersonDTO();
				foreach (var pair in request.Args)
				{
					var key = pair.Key.ToLower();
					var value = pair.Value.ToString();
					switch (key)
					{
						case "id": dto.ExternalId = value; break;
						case "userid":
							{
								if(int.TryParse(value, out var idAsInt)) {
									dto.UserId = idAsInt;
								}
							}
							break;
						case "customid": dto.ExternalId = value; break;
						case "firstname": dto.Firstname = value; break;
						case "lastname": dto.Lastname = value; break;
						case "email": dto.Email = value; break;
						case "address": dto.Address = value; break;
						case "phonehome": dto.PhoneHome = value; break;
						case "phonework": dto.PhoneWork = value; break;
						case "mobile": dto.Mobile = value; break;
						case "country": dto.Country = value; break;
						case "zipcode": dto.ZipCode = value; break;
						case "location": dto.Location = value; break;
						case "source": dto.Source = value; break;
						case "dateofbirth":
							if (!string.IsNullOrWhiteSpace(value))
							{
								if(DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt)) {
									dto.DateOfBirth = dt.ToString("yyyy-MM-dd");
								} else {
									// We would prefer people to use ISO 8601, but let the OS try to parse it instead:
									if(DateTime.TryParse(value, out dt)) {
										// And THEN i transform it back to a ISO 8601 valid date string:
										dto.DateOfBirth = dt.ToString("yyyy-MM-dd");
									}
								}
							}
							break;
						case "gender":
							if (!string.IsNullOrWhiteSpace(value))
							{
								if (value.StartsWith("M") || value.StartsWith("m"))
								{
									dto.Gender = 1;
								}
								if (value.StartsWith("F") || value.StartsWith("f") || value.StartsWith("K") || value.StartsWith("k"))
								{
									dto.Gender = 2;
								}
							}
							break;
						case "profiledata":
							try
							{
								// Format the profile as a json string, to be able to send it as a string to ExorLive.
								var json = "";
								if(pair.Value is JArray data) {
									json = Newtonsoft.Json.JsonConvert.SerializeObject(pair.Value);
									StringBuilder sblog = new StringBuilder("SelectPerson.ProfileData").AppendLine();
									foreach(var entry in data) {
										if(entry is IList<JToken> list) {
											var firstProp = true;
											foreach(JProperty prop in list) {
												if(firstProp) {
													firstProp = false;
													sblog.Append("   ");
												} else {
													sblog.Append(" | ");
												}
												sblog.Append(prop.Name).Append("=").Append(prop.Value);
											}
											sblog.AppendLine();
										}
									}
									_app.Log(sblog.ToString());
								}
								dto.ProfileData = json;
							} catch(Exception ex)
							{
								directResult = JsonFormatError("Failed to interpret 'profiledata' in the SelectPerson call. {0}" + ex.Message);
							}
							break;
					}
				}
				if (dto.UserId > 0 || (!string.IsNullOrWhiteSpace(dto.ExternalId)))
				{
					if(string.IsNullOrWhiteSpace(dto.Firstname) || string.IsNullOrWhiteSpace(dto.Lastname))
					{
						directResult = JsonFormatError("Firstname and lastname are mandatory.");
					}
					else if (string.IsNullOrWhiteSpace(dto.Source))
					{
						directResult = JsonFormatError("Source (the name of your application) is mandatory.");
					}
					else
					{
						dto.Source = "WebWrapper " + dto.Source;
						_window.Dispatcher.BeginInvoke(new Action(() =>
						{
							_window.Restore();
							_app.SelectPerson3(dto);
							_app.Log("    SelectPerson Id:'{0}' ExternalId:'{1}' Firstname:'{2}', Lastname:'{3}', Email:'{4}', DoB: '{5}'", dto.UserId, dto.ExternalId, dto.Firstname, dto.Lastname, dto.Email, dto.DateOfBirth);
						}));
						return true;
					}
				}
				else
				{
					directResult = JsonFormatError("No id specified for '{0}'.", request.Method);
				}
			}
			else
			{
				directResult = JsonFormatError("No arguments specified for method '{0}'.", request.Method);
			}
			return false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="requeststring"></param>
		/// <param name="directResult"></param>
		/// <returns>true if we are waiting for an asynch callback.</returns>
		private bool HandlePipeRequest(string requeststring, out string directResult)
		{
			_app.Log(">> Request: {0}", requeststring);
			try
			{
				directResult = "";
				var indexArgs = requeststring.IndexOf("\"Args\"", StringComparison.OrdinalIgnoreCase);
				var indexMethodTest = requeststring.IndexOf("\"SelectPerson\"", StringComparison.OrdinalIgnoreCase);
				if (indexMethodTest > 0 && indexMethodTest < indexArgs)
				{
					var request = Newtonsoft.Json.JsonConvert.DeserializeObject<NamedPipeRequest2>(requeststring);
					if (request != null)
					{
						return HandleSelectPerson(request, out directResult);
					} else
					{
						directResult = JsonFormatError("Failed to parse JSON for 'SelectPerson'.");
					}
					return false;
				}
				else
				{
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
										var customId = "";
										var from = DateTime.MinValue;
										foreach (var pair in request.Args)
										{
											if (pair.Key.ToLower() == "customid")
											{
												customId = pair.Value;
											}
											if (pair.Key.ToLower() == "userid")
											{
												if(!int.TryParse(pair.Value, out userId))
												{
													directResult = JsonFormatError("Invalid userId specified");
													return false;
												}
											}
											if (pair.Key.ToLower() == "from")
											{
												if (!string.IsNullOrWhiteSpace(pair.Value))
												{
													if (!DateTime.TryParse(pair.Value, out from))
													{
														directResult = JsonFormatError("Value '{0}' could not be parsed to a valid datetime.", pair.Value);
														_app.Log("<< Result: {0}", directResult);
														return false;
													}
												}
											}
										}
										if (userId > 0 || (!string.IsNullOrWhiteSpace(customId)))
										{
											CallGetWorkoutsForClient(userId, customId, from);
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
										var foundId = false;
										foreach (var pair in request.Args)
										{
											if (pair.Key.ToLower() == "id")
											{
												// ReSharper disable once RedundantAssignment
												foundId = true;
												if(int.TryParse(pair.Value, out var id)) {
													OpenWorkout(id);
													return false;
												} else {
													directResult = JsonFormatError("Value '{0}' for id is not an integer.", pair.Value);
													_app.Log("<< Result: {0}", directResult);
													return false;
												}
											}
										}
										// ReSharper disable once ConditionIsAlwaysTrueOrFalse
										if (foundId == false)
										{
											directResult = JsonFormatError("Argument 'id' not specified for method '{0}'.", request.Method);
										}
									}
									else
									{
										directResult = JsonFormatError("No arguments specified for method '{0}'.", request.Method);
									}
									break;
								case "close":
									_window.Dispatcher.BeginInvoke(new Action(() =>
									{
										_window.SignoutAndQuitApplication();
									}));
									break;
								case "log":
									_app.Logging = true;
									_app.Log("Starting logging");
									break;
								case "show":
									_window.Dispatcher.BeginInvoke(new Action(() =>
									{
										_window.ForceShowForeground();
									}));
									break;
								case "hide":
									_window.Dispatcher.BeginInvoke(new Action(() =>
									{
										_window.WindowState = System.Windows.WindowState.Minimized;
										_window.Hide();
									}));
									break;
								case "version":
									directResult = Assembly.GetEntryAssembly().GetName().Version.ToString();
									break;
								case "getlistofusers":
									string customid = null;
									if (request.Args != null && request.Args.Count > 0)
									{
										foreach (var pair in request.Args)
										{
											var key = pair.Key.ToLower();
											switch (key)
											{
												case "customid": customid = pair.Value; break;
											}
										}
									}
									CallGetListOfUsers(customid);
									return true;
								//case "selectperson":
								//	if (request.Args != null && request.Args.Count > 0)
								//	{
								//		var dto = new PersonDTO();
								//		foreach (var pair in request.Args)
								//		{
								//			var key = pair.Key.ToLower();
								//			switch (key)
								//			{
								//				case "id": dto.ExternalId = pair.Value; break;
								//				case "firstname": dto.Firstname = pair.Value; break;
								//				case "lastname": dto.Lastname = pair.Value; break;
								//				case "email": dto.Email = pair.Value; break;
								//				case "address": dto.Address = pair.Value; break;
								//				case "phonehome": dto.PhoneHome = pair.Value; break;
								//				case "phonework": dto.PhoneWork = pair.Value; break;
								//				case "mobile": dto.Mobile = pair.Value; break;
								//				case "country": dto.Country = pair.Value; break;
								//				case "zipcode": dto.ZipCode = pair.Value; break;
								//				case "location": dto.Location = pair.Value; break;
								//				case "dateofbirth":
								//					if (!string.IsNullOrWhiteSpace(pair.Value))
								//					{
								//						DateTime dt;
								//						if (DateTime.TryParseExact(pair.Value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
								//						{
								//							dto.DateOfBirth = dt.ToString("yyyy-MM-dd");
								//						}
								//						else
								//						{
								//							// We would prefer people to use ISO 8601, but let the OS try to parse it instead:
								//							if (DateTime.TryParse(pair.Value, out dt))
								//							{
								//								// And THEN i transform it back to a ISO 8601 valid date string:
								//								dto.DateOfBirth = dt.ToString("yyyy-MM-dd");
								//							}
								//						}
								//					}
								//					break;
								//			}
								//		}
								//		if (!string.IsNullOrWhiteSpace(dto.ExternalId))
								//		{
								//			_window.Dispatcher.BeginInvoke(new Action(() =>
								//			{
								//				_app.SelectPerson(dto);
								//				_app.Log("    SelectPerson ExternalId:'{0}' Firstname:'{1}', Lastname:'{2}', Email:'{3}', DoB: '{4}'", dto.ExternalId, dto.Firstname, dto.Lastname, dto.Email, dto.DateOfBirth);
								//			}));
								//		}
								//		else
								//		{
								//			directResult = JsonFormatError("No id specified for '{0}'.", request.Method);
								//		}
								//	}
								//	else
								//	{
								//		directResult = JsonFormatError("No arguments specified for method '{0}'.", request.Method);
								//	}
								//	break;
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
					if (!string.IsNullOrWhiteSpace(directResult))
					{
						_app.Log("<< Result: {0}", directResult);
					}
					return false;
				}
			}
			catch (Exception ex)
			{
				directResult = JsonFormatError("Exception: {0}", ex.Message);
				_app.Log("<< Result: {0}", directResult);
				return false;
			}
		}

		private void OpenWorkout(int id)
		{
			_window.Dispatcher.BeginInvoke(new Action(() =>
			{
				_window.Restore();
			}));
			_app.OpenWorkout(id);
		}

		private void CallGetWorkoutsForClient(int userId, string customId, DateTime from)
		{
			_app.GetWorkoutsForClient(userId, customId, from);
		}
		private void CallGetListOfUsers(string customId)
		{
			_app.GetListOfUsers(customId);
		}

		private static string JsonFormatError(string error, params object[] args)
		{
			return $"{{ \"error\": \"{string.Format(error, args).Replace('\"', '\'')}\" }} ";
		}
	}

	public class NamedPipeRequest
	{
		// ReSharper disable once UnusedAutoPropertyAccessor.Global
		public string Method { get; set; }
		// ReSharper disable once UnusedAutoPropertyAccessor.Global
		public Dictionary<string, string> Args { get; set; }
	}

	public class NamedPipeRequest2
	{
		// ReSharper disable once UnusedAutoPropertyAccessor.Global
		public string Method { get; set; }
		// ReSharper disable once UnusedAutoPropertyAccessor.Global
		public Dictionary<string, object> Args { get; set; }
	}


}
