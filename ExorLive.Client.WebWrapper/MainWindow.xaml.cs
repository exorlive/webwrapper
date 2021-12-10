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
using ExorLive.WebWrapper.Interface;
using WebWrapper;

namespace ExorLive.Client.WebWrapper
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : IExorLiveHost
	{
		private IBrowser _browser;
		private bool _closeOnNavigate;
		private bool _doClose;
		private Uri _navigateToUri;
		private bool _handleDisconnectInNavigatedEvent = true;

		public bool Debug => App.Debug;
		public string ApplicationIdentifier => App.ApplicationIdentifier;

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

		/// <summary>
		/// Loads window size from user settings, and adjusts the size to fit within the desktop screen.
		/// </summary>
		private void SetWindowSize()
		{
			// Make sure the rectangle is visible within screens.
			if (WindowFitsScreen())
			{
				Top = App.UserSettings.Top;
				Left = App.UserSettings.Left;
				Height = App.UserSettings.Height;
				Width = App.UserSettings.Width;
			}
			if (App.UserSettings.Maximized)
			{
				WindowState = WindowState.Maximized;
			}
		}

		private static bool WindowFitsScreen()
		{
			var topLeft = new Point(
				App.UserSettings.Left,
				App.UserSettings.Top
			);
			var bottomRight = new Point(
				App.UserSettings.Left + App.UserSettings.Width,
				App.UserSettings.Top + App.UserSettings.Height
			);
			return IsPointVisibleOnAScreen(topLeft) && IsPointVisibleOnAScreen(bottomRight);
		}

		private static bool IsPointVisibleOnAScreen(Point p)
		{
			return Screen.AllScreens.Any(s => p.X < s.Bounds.Right && p.X > s.Bounds.Left && p.Y > s.Bounds.Top && p.Y < s.Bounds.Bottom);
		}

		/// <summary>
		/// Saves window size to user settings.
		/// </summary>
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

		/// <summary>
		/// Adds a Browser view into the browser grid.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BrowserGrid_Loaded(object sender, RoutedEventArgs e)
		{
			try
			{
				_browser = GetBrowser(App.UserSettings.BrowserEngine);
				_browser.Navigated += Browser_Navigated;
				_browser.SelectedUserChanged += _browser_SelectedUserChanged;
				_browser.IsLoaded += _browser_IsLoaded;
				_browser.IsUnloading += _browser_IsUnloading;
				_browser.BeforeNavigating += _browser_BeforeNavigating;
				_browser.ExportUsersDataEvent += _browser_ExportUsersDataEvent;
				_browser.ExportUserListEvent += _browser_ExportUserListEvent;
				_browser.SelectPersonResultEvent += _browser_SelectPersonResultEvent;
				_browser.ExportSignonDetailsEvent += _browser_ExportSignonDetailsEvent;
				_browser.ZoomFactorChanged += Browser_ZoomLevelChangedEvent;
				BrowserGrid.Children.Add(_browser.GetUiElement());
				BrowserSetZoom(App.UserSettings.ZoomFactor);

				if (_navigateToUri != null)
				{
					_browser.Navigate(_navigateToUri);
				}
				if (App.UserSettings.CheckForUpdates)
				{
					CheckForUpdates();
				}
			}
			catch (Exception ex)
			{
				System.Windows.MessageBox.Show(ex.Message, "Browser spawn error", MessageBoxButton.OK);
				throw;
			}
		}

		private IBrowser GetBrowser(BrowserEngines _defaultBrowserEngine)
		{
			if (_defaultBrowserEngine != BrowserEngines.InternetExplorer)
			{
				StatusBarInternetExplorer.Visibility = Visibility.Hidden;
			}
			if (_defaultBrowserEngine != BrowserEngines.EoWebBrowser)
			{
				StatusBarEoBrowser.Visibility = Visibility.Hidden;
			}

			switch (_defaultBrowserEngine)
			{
				case BrowserEngines.InternetExplorer:
					return WindowsBrowser.Instance;
				case BrowserEngines.EoWebBrowser:
					return EoBrowser.Instance;
				case BrowserEngines.WebViewBrowser:
					return WebViewBrowser.Instance;
				default:
					return EoBrowser.Instance;
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
		private void _browser_SelectPersonResultEvent(object sender, EventArgs e)
		{
			SelectPersonResultEvent?.Invoke(this, (JsonEventArgs)e);
		}
		private void _browser_ExportSignonDetailsEvent(object sender, EventArgs e)
		{
			ExportSignonDetailsEvent?.Invoke(this, (JsonEventArgs)e);
		}

		private bool _beforeNavigatedSignoutHandled = false;
		private void _browser_BeforeNavigating(object sender, Uri e)
		{
			if (e.AbsolutePath.Contains("signout") && (!_beforeNavigatedSignoutHandled))
			{
				_beforeNavigatedSignoutHandled = true;
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
		public void NotifySelectingUser(int id, string externalId, string firstname, string lastname, string email, string dateofbirth)
		{
			var person = new PersonDTO()
			{
				UserId = id,
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
		public event SelectPersonResultEventHandler SelectPersonResultEvent;
		public event ExportSignonDetailsEventHandler ExportSignonDetailsEvent;
		public event UserHasDisconnectedEventHandler UserHasDisconnected;

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
		private void Browser_Navigated(object sender, EventArgs e)
		{
			if (!_closeOnNavigate) { return; }
			var shallCloseNow = true;
			if (_handleDisconnectInNavigatedEvent)
			{
				// Remove any 'remembered signon' details when user selects "Log Out" in the ExorLive profile menu.
				_handleDisconnectInNavigatedEvent = false; // To avoid infinte loop
				UserHasDisconnected?.Invoke(this);
				shallCloseNow = !SignoutAdfs();
			}
			if (shallCloseNow)
			{
				_doClose = true;
				Close();
			}
		}

		/// <summary>
		/// Navigates to ADFS signout page.
		/// </summary>
		/// <returns>
		/// True if ADFS is enabled, false if ADFS isn't enabled.
		/// </returns>
		private bool SignoutAdfs()
		{
			if (!string.IsNullOrWhiteSpace(App.UserSettings.AdfsUrl))
			{
				var url = App.AppendUrlArg(App.UserSettings.AdfsUrl, "signout=1");
				if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out Uri uri))
				{
					Navigate(uri);
					return true;
				}
			}
			return false;
		}

		public void SelectPerson2(PersonDTO person)
		{
			if (person.UserId > 0)
			{
				SelectPersonById(person.UserId);
			}
			else if (!string.IsNullOrWhiteSpace(person.ExternalId))
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
		public void SelectPerson3(PersonDTO person)
		{
			// Logic implemented in ExorLive:
			//   If UserId is set and is a valid userid in the organization, open and update that user.
			//   If userId > 0 but not a user in the org, give NOTFOUND status back.
			//   If UserId == 0, ExternalId must be set. A new contact is created in ExorLive.
			//
			if (person.UserId > 0 || (!string.IsNullOrWhiteSpace(person.ExternalId)))
			{
				_browser.SelectPerson3(
					person.UserId,
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
					person.PhoneHome,
					person.ProfileData,
					person.Source
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
		/// <summary>
		/// Forces the window to the front.
		/// </summary>
		/// <see cref="https://stackoverflow.com/a/4831839/1395658"/>
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
		private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			RememberWindowSize();
			if (_doClose) { return; }
			if (!Loaded) { return; }
			e.Cancel = true;
			WindowState = WindowState.Minimized;
			Hide();
			NotTray.ShowBalloonTip("Minimize", "Exor Live has been minimized to the tray. Use 'Sign Out' to close the application.", BalloonIcon.None);
		}

		private void MenuItem_Click_Show(object sender, RoutedEventArgs e)
		{
			Restore();
		}

		/// <summary>
		/// Signs out from ADFS, then signs out from ExorLive.
		/// </summary>
		public void SignoutAndQuitApplication()
		{
			UserHasDisconnected?.Invoke(this);
			SignoutAdfs();
			QuitApplication();
		}

		/// <summary>
		/// Navigates to a signout page to sign out the user.
		/// </summary>
		public void QuitApplication()
		{
			_doClose = true;
			if (App.UserSettings.AppUrl.Contains("int.exorlive.com"))
			{
				Navigate(new Uri(App.UserSettings.AppUrl.Replace("/app/", "/signout/").Replace("int.exorlive.com", "auth-int.exorlive.com")));
			}
			else if (App.UserSettings.AppUrl.Contains("test.exorlive.com"))
			{
				Navigate(new Uri(App.UserSettings.AppUrl.Replace("/app/", "/signout/").Replace("test.exorlive.com", "auth.test.exorlive.com")));
			}
			else if (App.UserSettings.AppUrl.Contains("localhost:50000"))
			{
				Navigate(new Uri(App.UserSettings.AppUrl.Replace("/app/", "/signout/").Replace("localhost:50000", "localhost:50006")));
			}
			else
			{
				Navigate(new Uri(App.UserSettings.AppUrl.Replace("/app/", "/signout/").Replace("exorlive.com", "auth.exorlive.com")));
			}
		}

		private void MenuItem_Click_Close(object sender, RoutedEventArgs e)
		{
			_handleDisconnectInNavigatedEvent = false;
			QuitApplication();
		}
		private void MenuItem_Click_SignOut(object sender, RoutedEventArgs e)
		{
			_handleDisconnectInNavigatedEvent = false;
			SignoutAndQuitApplication();
		}


		private void NotTray_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
		{
			Restore();
		}

		/// <summary>
		/// Checks if there are available updates for the webwrapper.
		/// </summary>
		private void CheckForUpdates()
		{
			var domain = new Uri("https://webwrapper.exorlive.com/");
			var subfolder = App.UserSettings.UpdatePath;
			if (!string.IsNullOrWhiteSpace(subfolder))
			{
				domain = new Uri(domain + subfolder + "/");
			}
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
									var newestVersionComparison = new Version(
										int.Parse(s[0]),
										int.Parse(s[1]),
										int.Parse(s[2]),
										0
									);
									// Ignore build version changes.
									if (newestVersionComparison > assemblyVersion)
									{
										var newestVersion = new Version(
											int.Parse(s[0]),
											int.Parse(s[1]),
											int.Parse(s[2]),
											int.Parse(s[3])
										);
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

		private void HideNotificationButton_Click(object sender, RoutedEventArgs e)
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
		public void GetSignonDetails()
		{
			_browser.GetSignonDetails();
		}

		public void GetListOfUsers(string customId)
		{
			_browser.GetListOfUsers(customId);
		}

		public void OpenWorkout(int id)
		{
			_browser.OpenWorkout(id);
		}

		private void Browser_ZoomLevelChangedEvent(object sender, EventArgs e)
		{
			var zoomfactor = _browser.GetZoomFactor();
			var zoompercentage = (int)(zoomfactor * 100);
			ZoomLabel.Text = $"{zoompercentage}%";
		}

		private void BtnZoomIn_Click(object sender, RoutedEventArgs e)
		{
			if (_browser.SupportsZoom() == false) return;
			var newZoomFactor = _browser.GetZoomFactor() + 0.1M;
			BrowserSetZoom(newZoomFactor);
		}

		private void BtnZoomOut_Click(object sender, RoutedEventArgs e)
		{
			if (_browser.SupportsZoom() == false) return;
			var newZoomFactor = _browser.GetZoomFactor() - 0.1M;
			BrowserSetZoom(newZoomFactor);
		}

		private void BrowserSetZoom(decimal newZoomFactor)
		{
			if (_browser.SupportsZoom() == false) return;
			if (newZoomFactor > 2M) newZoomFactor = 2M;
			if (newZoomFactor < 0.1M) newZoomFactor = 0.1M;
			_browser.SetZoomFactor(newZoomFactor);
			if (App.UserSettings.ZoomFactor != newZoomFactor)
			{
				App.UserSettings.ZoomFactor = newZoomFactor;
			}
		}
	}
}
