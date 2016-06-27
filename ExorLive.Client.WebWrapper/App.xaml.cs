using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using ExorLive.Desktop;
using ExorLive.Properties;
using Microsoft.Shell;
using ExorLive.Client.WebWrapper.NamedPipe;

namespace ExorLive.Client.WebWrapper
{
	/// <summary>
	///     Interaction logic for App.xaml
	/// </summary>
	public partial class App : IHost, ISingleInstanceApp
	{
		private const string Unique = "20151117084412";
		private static IHosted _hostedComponent;
		private Dictionary<string, string> _applicationArguments;
		private string[] _cmd;
		private IExorLiveHost _webWrapperWindow;
		NPServer _npServer;

		public static string ApplicationIdentifier
			=> $"{Assembly.GetExecutingAssembly().FullName} ({_hostedComponent.GetName()})";

		public static bool Debug => Convert.ToBoolean(Settings.Default.Debug);

		public void SelectPerson(PersonDTO person)
		{
			_webWrapperWindow.SelectPerson2(person);
		}
		public void DeletePerson(string externalId)
		{
			throw new NotImplementedException();
		}
		public void SelectTab(string tab)
		{
			_webWrapperWindow.SelectTab(tab);
		}
		public void QueryWorkouts(string query)
		{
			_webWrapperWindow.QueryWorkouts(query);
		}
		public void QueryExercises(string query)
		{
			_webWrapperWindow.QueryExercises(query);
		}
		public event IHost.WindowMinifiedEventHandler WindowMinified;
		public event IHost.WindowClosingEventHandler WindowClosing;

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
			if (!SingleInstance<App>.InitializeAsFirstInstance(Unique)) return;
			var application = new App();

			application.InitializeComponent();
			application.Run();

			// Allow single instance code to perform cleanup operations
			SingleInstance<App>.Cleanup();
		}		

		/// <summary>
		///     Handles the Startup event of the Application control.
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
			_webWrapperWindow = new MainWindow();
			_webWrapperWindow.IsLoaded += WebWrapperWindowIsLoaded;
			_webWrapperWindow.IsUnloading += _webWrapperWindow_IsUnloading;
			_webWrapperWindow.SelectedUserChanged += WebWrapperWindowSelectedUserChanged;
			_webWrapperWindow.ExportUsersDataEvent += _webWrapperWindow_ExportUsersDataEvent;
			if (_applicationArguments.ContainsKey("culture"))
			{
				url += $"&culture={_applicationArguments["culture"]}";
			}
			((MainWindow)_webWrapperWindow).Navigate(new Uri(url));
		}

		private void _webWrapperWindow_IsUnloading(object sender)
		{
		}

		private void _webWrapperWindow_ExportUsersDataEvent(object sender, UsersDataEventArgs args)
		{
			if(_npServer != null)
			{
				_npServer.PublishDataOnNamedPipe(args.JsonData);
			}
		}

		private static void WebWrapperWindowSelectedUserChanged(object sender, SelectedUserEventArgs args)
		{
		}
		private void WebWrapperWindowIsLoaded(object sender)
		{
			HandleCommandLine(_cmd);
			StartNamedPipeServer();
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
			if (string.IsNullOrEmpty(provider)) return;
			var type = Type.GetType(provider, true);
			if (type == null) return;
			_hostedComponent = (IHosted)Activator.CreateInstance(type);
			_hostedComponent.Initialize(this, Environment.CurrentDirectory);
		}

		internal void GetWorkoutsForClient(int userId, DateTime from)
		{
			_webWrapperWindow.GetWorkoutsForClient(userId, from);
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
				_npServer = new NPServer();
				_npServer.Initialize(this);
				_npServer.StartNPServer();
			}
		}
	}
}