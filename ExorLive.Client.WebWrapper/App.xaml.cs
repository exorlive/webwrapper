﻿using System;
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
		private NpServer _npServer;
		public bool ExorLiveIsRunning;
		public static Settings UserSettings { get; private set; }

		public static string ApplicationIdentifier
			=> $"{Assembly.GetExecutingAssembly().FullName} ({_hostedComponent.GetName()})";

		public static bool Debug => Convert.ToBoolean(Settings.Default.Debug);

		public void SelectPerson(PersonDTO person)
		{
			_webWrapperWindow.SelectPerson2(person);
		}

		public void SelectPerson3(PersonDTO person)
		{
			// SelectPerson3 is used from 2016-09-01. CustomId may not be unique.
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
			int id;
			if(int.TryParse(workoutIdAsString, out id))
			{
				if(id > 0)
				{
					_webWrapperWindow.OpenWorkout(id);
				}
			}
		}

		private bool _logging = false;
		public bool Logging
		{
			get { return _logging; }
			set { _logging = value; }
		}
		public void Log(string format, params object[] args)
		{
			try
			{
				if (Logging)
				{
					string path = Directory.GetCurrentDirectory();
					string filename = Path.Combine(path, "ExorLive.Client.Webwrapper.log");

					string line = string.Format("{0}: {1}{2}", DateTime.Now, args != null ? string.Format(format, args) : format, Environment.NewLine);
					File.AppendAllText(filename, line, Encoding.UTF8);
				}
			}
			catch(Exception)
			{
				// Ignore any error.
			}
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
			UserSettings = new Settings();
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
			_webWrapperWindow.IsLoaded += WebWrapperWindowExorLiveIsLoaded;
			_webWrapperWindow.IsUnloading += _webWrapperWindow_IsUnloading;
			_webWrapperWindow.SelectedUserChanged += WebWrapperWindowSelectedUserChanged;
			_webWrapperWindow.ExportUsersDataEvent += _webWrapperWindow_ExportUsersDataEvent;
			_webWrapperWindow.ExportUserListEvent += _webWrapperWindow_ExportUserListEvent;
			if (_applicationArguments.ContainsKey("culture"))
			{
				url += $"&culture={_applicationArguments["culture"]}";
			}
			((MainWindow)_webWrapperWindow).Navigate(new Uri(url));
			StartNamedPipeServer();
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

		private static void WebWrapperWindowSelectedUserChanged(object sender, SelectedUserEventArgs args)
		{
		}
		private void WebWrapperWindowExorLiveIsLoaded(object sender)
		{
			ExorLiveIsRunning = true;
			HandleCommandLine(_cmd);
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