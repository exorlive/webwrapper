using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using ExorLive.Desktop;
using ExorLive.Properties;
using Microsoft.Shell;
using ExorLive.Client.WebWrapper.NamedPipe;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace ExorLive.Client.WebWrapper
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : IHost, ISingleInstanceApp
	{
		private const string Unique = "20151117084412";
		private static IHosted _hostedComponent;
		private Dictionary<string, string> _applicationArguments;
		private string[] _cmd;
		private IExorLiveHost _webWrapperWindow;
		private NpServer _npServer;
		public bool ExorLiveIsRunning;
		public static Settings UserSettings { get; private set; }
		private string _automaticSignonExternalUser = null;
		/// <summary>
		/// Set flag to pick up signon details after the user has logged in manually.
		/// </summary>
		private bool _shallStoreAutomaticSignonUser = false;
		public static string ApplicationIdentifier => $"{Assembly.GetExecutingAssembly().FullName} ({_hostedComponent.GetName()})";

		public static bool Debug => Settings.Default.Debug;

		public void SelectPerson(PersonDTO person)
		{
			_webWrapperWindow.SelectPerson2(person);
		}
		/// <summary>
		/// SelectPerson3 is used. CustomId may not be unique.
		/// </summary>
		/// <param name="person"></param>
		public void SelectPerson3(PersonDTO person)
		{
			_webWrapperWindow.SelectPerson3(person);
		}

		public void DeletePerson(string externalId)
		{
			throw new NotImplementedException();
		}
		public void SelectTab(string tab)
		{
			_webWrapperWindow.SelectTab(tab);
		}

		private string GetSignonString(string signonuser)
		{
			if (Settings.Default.RememberLoggedInUser)
			{
				var dictstring = UserSettings.OsloSettings;
				if (!string.IsNullOrWhiteSpace(dictstring))
				{
					Dictionary<string, SignonDetails> dict;
					dict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, SignonDetails>>(dictstring);
					if (dict != null && dict.Count > 0)
					{
						if (dict.TryGetValue(signonuser, out SignonDetails details))
						{
							var time = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
							var crypt = GetHash(time);
							return string.Format("q={0}|{1}|{2}|{3}|{4}",
								details.UserId,
								details.Username,
								details.Token,
								crypt,
								time
							);
						}
					}
				}
			}
			return "";
		}

		private void StoreSignonDetails(string signonuser, int userId, string username, string token)
		{
			if (Settings.Default.RememberLoggedInUser)
			{
				Dictionary<string, SignonDetails> dict = null;
				var dictstring = UserSettings.OsloSettings;
				if (!string.IsNullOrWhiteSpace(dictstring))
				{
					dict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, SignonDetails>>(dictstring);
				}
				if (dict == null)
				{
					dict = new Dictionary<string, SignonDetails>(1);
				}
				dict[signonuser] = new SignonDetails()
				{
					UserId = userId,
					Username = username,
					Token = token
				};
				dictstring = Newtonsoft.Json.JsonConvert.SerializeObject(dict);
				UserSettings.OsloSettings = dictstring;
			}
		}

		private void RemoveSignonDetails(string signonuser)
		{
			var dictstring = UserSettings.OsloSettings;
			if (!string.IsNullOrWhiteSpace(dictstring))
			{
				Dictionary<string, SignonDetails> dict;
				dict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, SignonDetails>>(dictstring);
				if (dict != null && dict.Count > 0)
				{
					if (dict.TryGetValue(signonuser, out SignonDetails details))
					{
						dict.Remove(signonuser);
						dictstring = Newtonsoft.Json.JsonConvert.SerializeObject(dict);
						UserSettings.OsloSettings = dictstring;
					}
				}
			}
		}

		public void DisconnectedCurrentUser()
		{
			if (!string.IsNullOrWhiteSpace(_automaticSignonExternalUser))
			{
				RemoveSignonDetails(_automaticSignonExternalUser);
			}
		}

		private class SignonDetails
		{
			public int UserId { get; set; }
			public string Username { get; set; }
			public string Token { get; set; }
		}

		private static string GetHash(string txt)
		{
			var sha1 = new SHA1CryptoServiceProvider();
			return ByteToHex(sha1.ComputeHash(Encoding.UTF8.GetBytes(txt))).ToLower();
		}
		private static string ByteToHex(byte[] ba)
		{
			var hex = BitConverter.ToString(ba);
			return hex.Replace("-", "");
		}
		public void QueryWorkouts(string query)
		{
			_webWrapperWindow.QueryWorkouts(query);
		}
		public void QueryExercises(string query)
		{
			_webWrapperWindow.QueryExercises(query);
		}
		public void OpenWorkout(string workoutIdAsString)
		{
			if (int.TryParse(workoutIdAsString, out var id))
			{
				if (id > 0)
				{
					_webWrapperWindow.OpenWorkout(id);
				}
			}
		}
		private bool _logging = false;
		public bool Logging {
			get {
				return _logging;
			}
			set {
				if (_logging == true && value == false)
				{
					Log("Turning off logging");
				}
				_logging = value;
			}
		}
		public void Log(string format, params object[] args)
		{
			var path = Directory.GetCurrentDirectory();
			var filename = Path.Combine(path, "ExorLive.Client.Webwrapper.log");
			try
			{
				if (Logging)
				{
					if (args != null && args.Length > 0)
					{
						format = string.Format(format, args);
					}
					var line = string.Format("{0}: {1}{2}", DateTime.Now, format, Environment.NewLine);
					File.AppendAllText(filename, line, Encoding.UTF8);
				}
			}
			catch (Exception ex)
			{
				// Ignore any error.
				MessageBox.Show(ex.Message);
				// File.AppendAllText(filename, "ERROR DURING LOGGING:" + ex.Message, Encoding.UTF8);
			}
		}

#pragma warning disable 67 // The event is never used
		public event IHost.WindowMinifiedEventHandler WindowMinified;
		public event IHost.WindowClosingEventHandler WindowClosing;
#pragma warning restore 67 // The event is never used

		public void LogException(Exception ex, string message)
		{
			MessageBox.Show($"{message} {Environment.NewLine} {ex.Message}");
		}
		public bool SignalExternalCommandLineArgs(IList<string> args)
		{
			((MainWindow)_webWrapperWindow).Restore();
			if (_webWrapperWindow.Loaded)
			{
				HandleCommandLine(args.Skip(1).ToArray());
			}
			else
			{
				_applicationArguments = args.Skip(1).Select(argument => argument.Split('='))
					.Where(argumentKeyAndValue => argumentKeyAndValue.Length == 2)
					.ToDictionary(argumentKey => argumentKey[0], argumentValue => argumentValue[1]);
			}
			return false;
		}

		[STAThread]
		public static void Main()
		{
			if (!SingleInstance<App>.InitializeAsFirstInstance(Unique))
			{
				return;
			}
			UserSettings = new Settings();
			var application = new App();
			application.InitializeComponent();
			application.Run();

			// Allow single instance code to perform cleanup operations
			SingleInstance<App>.Cleanup();
		}

		/// <summary>
		/// Handles the Startup event of the Application control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.StartupEventArgs" /> instance containing the event data.</param>
		private void Application_Startup(object sender, StartupEventArgs e)
		{
			var url = Settings.Default.AppUrl;
			_cmd = e.Args;
			_applicationArguments = _cmd.Select(argument => argument.Split('='))
				.Where(argumentKeyAndValue => argumentKeyAndValue.Length == 2)
				.ToDictionary(argumentKey => argumentKey[0], argumentValue => argumentValue[1]);
			LoadProtocolProvider();
			Logging = UserSettings.Log;
			_webWrapperWindow = new MainWindow();
			_webWrapperWindow.IsLoaded += WebWrapperWindowExorLiveIsLoaded;
			_webWrapperWindow.IsUnloading += _webWrapperWindow_IsUnloading;
			_webWrapperWindow.SelectedUserChanged += WebWrapperWindowSelectedUserChanged;
			_webWrapperWindow.ExportUsersDataEvent += _webWrapperWindow_ExportUsersDataEvent;
			_webWrapperWindow.ExportUserListEvent += _webWrapperWindow_ExportUserListEvent;
			_webWrapperWindow.SelectPersonResultEvent += _webWrapperWindow_SelectPersonResultEvent;
			_webWrapperWindow.ExportSignonDetailsEvent += _webWrapperWindow_ExportSignonDetailsEvent;
			_webWrapperWindow.UserHasDisconnected += _webWrapperWindow_UserHasDisconnected;

			if (_applicationArguments.ContainsKey("culture"))
			{
				url = AppendUrlArg(url, $"culture={_applicationArguments["culture"]}");
			}
			var hasAutoSignonUser = false;
			if (UserSettings.SignonWithWindowsUser)
			{
				_automaticSignonExternalUser = Environment.UserName + "@" + Environment.UserDomainName;
				var signonstring = GetSignonString(_automaticSignonExternalUser);
				if (!string.IsNullOrWhiteSpace(signonstring))
				{
					url = AppendUrlArg(url, signonstring);
					hasAutoSignonUser = true;
				}
				// Set flag to pick up signon details after the user has logged in manually.
				_shallStoreAutomaticSignonUser = true;
			}
			if (!hasAutoSignonUser)
			{
				if (Settings.Default.RememberLoggedInUser)
				{
					var signonuser = _hostedComponent.GetSignonUser(e.Args);
					if (signonuser != null)
					{
						_automaticSignonExternalUser = signonuser;
						var signonstring = GetSignonString(signonuser);
						if (!string.IsNullOrWhiteSpace(signonstring))
						{
							url = AppendUrlArg(url, signonstring);
							hasAutoSignonUser = true;
						}
						// Set flag to pick up signon details after the user has logged in manually.
						_shallStoreAutomaticSignonUser = true;
					}
				}
			}
			if (hasAutoSignonUser)
			{
				// Navigate to the webwrapper.html instead of standard URL.
				if (url.Contains("/app/?"))
				{
					url = url.Replace("/app/?", "/auth/webwrapper.html?");
				}
				else if (url.Contains("/app?"))
				{
					url = url.Replace("/app?", "/auth/webwrapper.html?");
				}
			}
			if (!string.IsNullOrWhiteSpace(UserSettings.AdfsUrl))
			{
				// Adfs has its own URL and own way of remembering users.
				var signonuser = _hostedComponent.GetSignonUser(e.Args);
				url = UserSettings.AdfsUrl;
				if (!string.IsNullOrWhiteSpace(signonuser))
				{
					url = AppendUrlArg(url, "signon=" + signonuser);
				}
				_shallStoreAutomaticSignonUser = false;
				hasAutoSignonUser = false;
			}
			((MainWindow)_webWrapperWindow).Navigate(new Uri(url));
			StartNamedPipeServer();
		}

		private void _webWrapperWindow_UserHasDisconnected(object sender)
		{
			DisconnectedCurrentUser();
		}

		private string AppendUrlArg(string url, string toAppend)
		{
			if (!string.IsNullOrWhiteSpace(toAppend))
			{
				if (url.Contains('?'))
				{
					url += "&";
				}
				else
				{
					url += "?";
				}
				url += toAppend;
			}
			return url;
		}

		private static void _webWrapperWindow_IsUnloading(object sender)
		{
		}

		private void _webWrapperWindow_ExportUsersDataEvent(object sender, JsonEventArgs args)
		{
			_npServer?.PublishDataOnNamedPipe(args.JsonData);
		}
		private void _webWrapperWindow_ExportUserListEvent(object sender, JsonEventArgs args)
		{
			_npServer?.PublishDataOnNamedPipe(args.JsonData);
		}

		private void _webWrapperWindow_SelectPersonResultEvent(object sender, JsonEventArgs args)
		{
			_npServer?.PublishDataOnNamedPipe(args.JsonData);
		}

		private void _webWrapperWindow_ExportSignonDetailsEvent(object sender, JsonEventArgs args)
		{
			var jsonstring = args.JsonData;
			dynamic dyn = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(jsonstring);
			try
			{
				StoreSignonDetails(_automaticSignonExternalUser, (int)dyn.userid, (string)dyn.username, (string)dyn.token);
			}
			catch (Exception)
			{
				// Bad information in json string. Ignore.
			}
		}

		private static void WebWrapperWindowSelectedUserChanged(object sender, SelectedUserEventArgs args)
		{
		}
		private void WebWrapperWindowExorLiveIsLoaded(object sender)
		{
			ExorLiveIsRunning = true;
			HandleCommandLine(_cmd);
			if (_shallStoreAutomaticSignonUser)
			{
				if (Settings.Default.RememberLoggedInUser)
				{
					// Get signon-details for the logged-in user and store it in the UserSettings.
					_webWrapperWindow.GetSignonDetails();
				}
			}
		}
		private void LoadProtocolProvider()
		{
			var provider = string.Empty;
			if (_applicationArguments.ContainsKey("provider"))
			{
				provider = _applicationArguments["provider"];
			}
			else if (!string.IsNullOrEmpty(Settings.Default.ProtocolProvider))
			{
				provider = Settings.Default.ProtocolProvider;
			}
			if (string.IsNullOrEmpty(provider))
			{
				return;
			}
			var type = Type.GetType(provider, true);
			if (type == null)
			{
				return;
			}
			_hostedComponent = (IHosted)Activator.CreateInstance(type);
			_hostedComponent.Initialize(this, Environment.CurrentDirectory);
		}

		internal void GetWorkoutsForClient(int userId, string customId, DateTime from)
		{
			_webWrapperWindow.GetWorkoutsForClient(userId, customId, from);
		}

		internal void GetListOfUsers(string customId)
		{
			_webWrapperWindow.GetListOfUsers(customId);
		}

		internal void OpenWorkout(int id)
		{
			_webWrapperWindow.OpenWorkout(id);
		}

		private static void HandleCommandLine(string[] args)
		{
			_hostedComponent?.ReadCommandline(args);
		}

		/// <summary>
		/// Start a thread that listens on a Named Pipe channel. Remote clients may communicate with the Webwrapper and ExorLive on the Named Pipe channel.
		/// </summary>
		private void StartNamedPipeServer()
		{
			if (_npServer == null)
			{
				_npServer = new NpServer();
				_npServer.Initialize(this, (MainWindow)_webWrapperWindow);
				_npServer.StartNpServer();
			}
		}

	}
}