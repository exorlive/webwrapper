using System;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using System.Deployment.Application;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Windows.Forms;

namespace ExorLive.Client.WebWrapper
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : IExorLiveHost
	{
		private BrowserEngines _defaultBrowserEngine;
		private IBrowser _browser;
		private bool _closeOnNavigate;
		private bool _doClose;
		private Uri _navigateToUri;

		// ReSharper disable UnusedMember.Global
		public bool Debug => App.Debug;
		public string ApplicationIdentifier => App.ApplicationIdentifier;
		// ReSharper restore UnusedMember.Global

		public MainWindow()
		{
			InitializeComponent();
			SetWindowSize();
			Restore();
			_doClose = !App.UserSettings.MinimizeOnExit;
			try
			{
				Title += $" {Assembly.GetExecutingAssembly().GetName().Version}";
			}
			catch (InvalidDeploymentException)
			{
				Title += " - No version";
			}
		}

		private void SetWindowSize()
		{
			// Make sure the rectangle is visible within screens.
			if (IsPointVisibleOnAScreen(new Point(App.UserSettings.Left, App.UserSettings.Top)) &&
				IsPointVisibleOnAScreen(new Point(App.UserSettings.Left + App.UserSettings.Width, App.UserSettings.Top + App.UserSettings.Height)))
			{				
				Top = App.UserSettings.Top;
				Left = App.UserSettings.Left;
				Height = App.UserSettings.Height;
				Width = App.UserSettings.Width;
				// NOTE: Do not set WindowState.Maximized here. It will break loading of browserwindow.
			}
		}

		private static bool IsPointVisibleOnAScreen(Point p)
		{
			return Screen.AllScreens.Any(s => p.X < s.Bounds.Right && p.X > s.Bounds.Left && p.Y > s.Bounds.Top && p.Y < s.Bounds.Bottom);
		}

		private void RememberWindowSize()
		{
			if (WindowState == WindowState.Maximized)
			{
				// Use the RestoreBounds as the current values will be 0, 0 and the size of the screen
				App.UserSettings.Top = RestoreBounds.Top;
				App.UserSettings.Left = RestoreBounds.Left;
				App.UserSettings.Height = RestoreBounds.Height;
				App.UserSettings.Width = RestoreBounds.Width;
				App.UserSettings.Maximized = true;
			}
			else
			{
				App.UserSettings.Top = Top;
				App.UserSettings.Left = Left;
				App.UserSettings.Height = Height;
				App.UserSettings.Width = Width;
				App.UserSettings.Maximized = false;
			}
			App.UserSettings.Save();
		}


		private void BrowserGrid_Loaded(object sender, RoutedEventArgs e)
		{
			_defaultBrowserEngine = App.UserSettings.BrowserEngine;
			_browser = (_defaultBrowserEngine == BrowserEngines.InternetExplorer) ?
				WindowsBrowser.Instance :
				EoBrowser.Instance;
			_browser.Navigated += browser_Navigated;
			_browser.SelectedUserChanged += _browser_SelectedUserChanged;
			_browser.IsLoaded += _browser_IsLoaded;
			_browser.IsUnloading += _browser_IsUnloading;
			_browser.BeforeNavigating += _browser_BeforeNavigating;
			_browser.ExportUsersDataEvent += _browser_ExportUsersDataEvent;
			_browser.ExportUserListEvent += _browser_ExportUserListEvent;
			BrowserGrid.Children.Add(_browser.GetUiElement());
			if (_navigateToUri != null)
			{
				_browser.Navigate(_navigateToUri);
			}
			if (App.UserSettings.CheckForUpdates)
			{
				CheckForUpdates();
			}
		}

		private void _browser_ExportUsersDataEvent(object sender, EventArgs e)
		{
			ExportUsersDataEvent?.Invoke(this, (JsonEventArgs)e);
		}

		private void _browser_ExportUserListEvent(object sender, EventArgs e)
		{
			ExportUserListEvent?.Invoke(this, (JsonEventArgs)e);
		}

		private void _browser_BeforeNavigating(object sender, Uri e)
		{
			if (e.AbsolutePath.Contains("signout"))
			{
				_closeOnNavigate = true;
			}
		}

		private void _browser_IsUnloading(object sender, EventArgs e)
		{
			NotifyIsUnloading();
		}
		private void _browser_IsLoaded(object sender, EventArgs e)
		{
			NotifyIsLoaded();
		}
		private void _browser_SelectedUserChanged(object sender, EventArgs e)
		{
			SelectedUserChanged?.Invoke(this, (SelectedUserEventArgs)e);
		}
		// ReSharper disable once UnusedMember.Global
		public void SetInterface(object comObject)
		{
			_browser.SetInterface(comObject);
		}

		private void NotifyIsLoaded()
		{
			Loaded = true;
			IsLoaded?.Invoke(this);
			UpdateNotification.IsExpanded = false;
		}
		// ReSharper disable once UnusedMember.Global
		public void NotifySelectingUser(int id, string externalId, string firstname, string lastname, string email, string dateofbirth)
		{
			var person = new PersonDTO()
			{
				Id = id,
				ExternalId = externalId,
				Firstname = firstname,
				Lastname = lastname,
				Email = email,
				DateOfBirth = dateofbirth
			};
			SelectedUserChanged?.Invoke(this, new SelectedUserEventArgs(person));
		}

		private void NotifyIsUnloading()
		{
			Loaded = false;
			IsUnloading?.Invoke(this);
		}

		public new event IsLoadedEventHandler IsLoaded;
		public event IsUnloadingEventHandler IsUnloading;
		public event SelectedUserChangedEventHandler SelectedUserChanged;
		public event ExportUsersDataEventHandler ExportUsersDataEvent;
		public event ExportUserListEventHandler ExportUserListEvent;

		public void SelectPerson(PersonDTO person)
		{
			_browser.SelectPerson(person.ExternalId, person.Firstname, person.Lastname, person.Email, person.DateOfBirth);
			Restore();
		}
		/// <summary>
		/// Handles the Navigated event of the browser control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
		private void browser_Navigated(object sender, EventArgs e)
		{
			if (!_closeOnNavigate) return;
			_doClose = true;
			Close();
		}
		public void SelectPerson2(PersonDTO person)
		{
			if(person.Id > 0)
			{
				SelectPersonById(person.Id);
			}
			else if(!string.IsNullOrWhiteSpace(person.ExternalId))
			{
				_browser.SelectPerson2(
					person.ExternalId,
					person.Firstname,
					person.Lastname,
					person.Email,
					person.DateOfBirth,
					person.Address,
					person.ZipCode,
					person.Location,
					person.Mobile,
					person.PhoneWork,
					person.Gender,
					person.HomePage,
					person.Employer,
					person.Comment,
					person.Country,
					person.PhoneHome
				);
			}
			Restore();
		}
		public void SelectPersonById(int id)
		{
			_browser.SelectPersonById(id);
			Restore();
		}
		public void SelectTab(string tab)
		{
			_browser.SelectTab(tab);
			Restore();
		}
		public void QueryWorkouts(string query)
		{
			_browser.QueryWorkouts(query);
			Restore();
		}
		public void QueryExercises(string query)
		{
			_browser.QueryExercises(query);
			Restore();
		}
		public void Restore()
		{
			if (!IsVisible)
			{
				Show();
			}
			if (WindowState == WindowState.Minimized)
			{
				WindowState = WindowState.Normal;
			}
			Activate();
			Topmost = true;  // important
			Topmost = false; // important
			Focus();         // important
		}

		public void ForceShowForeground()
		{
			// Must force webwrapper to appear in front on other (calling) application when that application calls "show" command.
			// http://stackoverflow.com/questions/257587/bring-a-window-to-the-front-in-wpf

			if (!IsVisible)
			{
				Show();
			}

			if (WindowState == WindowState.Minimized)
			{
				WindowState = WindowState.Normal;
			}

			Activate();
			Topmost = true;  // important
			Topmost = false; // important
			Focus();         // important
		}

		public new bool Loaded { get; private set; }
		public void Navigate(Uri uri)
		{
			_navigateToUri = uri;
			_browser?.Navigate(uri);
		}

		/// <summary>
		/// Handles the Closing event of the MainWIndow control.
		/// If we are logged in, minimize the app instead.
		/// If not, close it.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
		private void MainWIndow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			RememberWindowSize();
			if (_doClose) { return; }
			if (!Loaded) { return; }
			e.Cancel = true;
			WindowState = WindowState.Minimized;
			Hide();
			NotTray.ShowBalloonTip("Minimize", "Exor Live has been minimized to the tray. Use 'Sign Out' to close the application.", BalloonIcon.None);
		}


		private void MenuItem_Click(object sender, RoutedEventArgs e)
		{
			Restore();
		}

		public void SignoutAndQuitApplication()
		{
			_doClose = true;
			Navigate(new Uri(App.UserSettings.AppUrl.Replace("/app/", "/signout/").Replace("exorlive.com", "auth.exorlive.com")));
		}

		private void MenuItem_Click_1(object sender, RoutedEventArgs e)
		{
			SignoutAndQuitApplication();
		}

		private void NotTray_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
		{
			Restore();
		}

		private void CheckForUpdates()
		{
			var domain = new Uri("https://webwrapper.exorlive.com/");
			var versionFile = new Uri(domain + "msi/version.txt");
			var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
			var downloadLink = new Uri(domain + "msi/ExorLiveWebWrapper.x.x.x.x.msi");
			try
			{
				using (var client = new WebClient())
				{
					using (var stream = client.OpenRead(versionFile))
					{
						if (stream != null)
						{
							using (var reader = new StreamReader(stream))
							{
								var response = reader.ReadLine();
								if (response != null)
								{
									var s = response.Split('.');
									var newestVersion = new Version(
										int.Parse(s[0]),
										int.Parse(s[1]),
										int.Parse(s[2]),
										0
									);  // Ignore updates that is just a build or a revision.
									if (newestVersion > assemblyVersion)
									{
										DownloadLink.NavigateUri = new Uri(downloadLink.AbsoluteUri.Replace("x.x.x.x", newestVersion.ToString()));
										UpdateNotification.IsExpanded = true;
									}
								}
							}
						}
					}
				}
			}
			catch (Exception)
			{
				// No internet? Blocked? Lets do nothing.
			}
		}

		private void hideNotificationButton_Click(object sender, RoutedEventArgs e)
		{
			UpdateNotification.IsExpanded = false;
		}

		private void DownloadLink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
		{
			Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
			e.Handled = true;
		}

		public void GetWorkoutsForClient(int userId, string customId, DateTime from)
		{
			_browser.GetWorkoutsForClient(userId, customId, from);
		}
		public void GetListOfUsers(string customId)
		{
			_browser.GetListOfUsers(customId);
		}

		public void OpenWorkout(int id)
		{
			_browser.OpenWorkout(id);
		}
	}
}
