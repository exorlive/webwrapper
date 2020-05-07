using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using EO.WebBrowser;
using EO.Wpf;
using ExorLive;
using ExorLive.Client.WebWrapper;
using ExorLive.Properties;
using WebView = EO.WebBrowser.WebView;

/// <summary>
/// A interface for the Eo.WebBrowser object.
/// http://www.essentialobjects.com/Products/WebBrowser/
/// Documentation of the Eo browser API:
/// https://www.essentialobjects.com/doc/webbrowser/start/overview.aspx
/// </summary>
public class EoBrowser : IBrowser
{
	private static IBrowser _instance;
	public static IBrowser Instance => _instance ?? (_instance = new EoBrowser());

	private readonly WebControl _browser;
	private readonly EoBrowserObject browserObject;
	private readonly string externalPath = "app.api._host.externalInterface";
	private bool _isNavigating;

	/// <summary>
	/// Private constructor that sets up the WebControl, then attaches a javascript to it.
	/// THis javascript object gets added to every page the webview navigates to.
	/// </summary>
	private EoBrowser()
	{
		Log("Construct EoBrowser");

		var cultures = new List<CultureInfo>()
		{
			CultureInfo.CurrentUICulture,
			CultureInfo.InstalledUICulture
		};

		if (!string.IsNullOrWhiteSpace(Resources.EoBrowserLicenseFile))
		{
			Runtime.AddLicense(Resources.EoBrowserLicenseFile);
		}
		Runtime.DefaultEngineOptions.BypassUserGestureCheck = true;
		Runtime.DefaultEngineOptions.DisableSpellChecker = true;
		Runtime.DefaultEngineOptions.DisableGPU = true;
		Runtime.DefaultEngineOptions.DisableWMPointer = true;
		Runtime.DefaultEngineOptions.SkipWaitForLayerActivation = false;
		Runtime.DefaultEngineOptions.AllowProprietaryMediaFormats();
		Runtime.DefaultEngineOptions.UILanguage = CultureInfo.CurrentUICulture.Name;
		EO.Base.Runtime.EnableCrashReport = false;
		EO.Base.Runtime.EnableEOWP = true;
		EO.Base.Runtime.Exception += Runtime_Exception;
		if (App.Debug)
		{
			EO.Base.Runtime.LogFileName = Path.GetTempFileName();
			StartRemoteDebugging();
		}
		browserObject = new EoBrowserObject(this);
		_browser = new WebControl
		{
			WebView = new WebView()
			{
				ObjectForScripting = browserObject,
				AcceptLanguage = Util.ConvertToAcceptLanguageHeaderValue(cultures)
			}
		};
		_browser.WebView.Activate += WebView_Activate;
		_browser.WebView.AfterPrint += WebView_AfterPrint;
		_browser.WebView.AfterReceiveHeaders += WebView_AfterReceiveHeaders;
		_browser.WebView.BeforeDownload += WebView_BeforeDownload;
		_browser.WebView.BeforeNavigate += WebView_BeforeNavigate;
		_browser.WebView.BeforeRequestLoad += WebView_BeforeRequestLoad;
		_browser.WebView.BeforeSendHeaders += WebView_BeforeSendHeaders;
		_browser.WebView.CanGoBackChanged += WebView_CanGoBackChanged;
		_browser.WebView.CanGoForwardChanged += WebView_CanGoForwardChanged;
		_browser.WebView.CertificateError += WebView_CertificateError;
		_browser.WebView.Closed += WebView_Closed;
		_browser.WebView.Closing += WebView_Closing;
		_browser.WebView.Command += WebView_Command;
		_browser.WebView.ConsoleMessage += WebView_ConsoleMessage;
		_browser.WebView.ContextMenuDismissed += WebView_ContextMenuDismissed;
		_browser.WebView.Disposed += WebView_Disposed;
		_browser.WebView.IsLoadingChanged += WebView_IsLoadingChanged;
		_browser.WebView.JSExtInvoke += WebView_JSExtension;
		_browser.WebView.LoadFailed += WebView_LoadFailed;
		_browser.WebView.NewWindow += WebView_NewWindow;
		_browser.WebView.RenderUnresponsive += WebView_RenderUnresponsive;
		_browser.WebView.BeforeContextMenu += WebView_BeforeContextMenu;
		_browser.WebView.BeforePrint += WebView_BeforePrint;
		_browser.WebView.DownloadCanceled += WebView_DownloadCanceled;
		_browser.WebView.DownloadCompleted += WebView_DownloadCompleted;
		_browser.WebView.DownloadUpdated += WebView_DownloadUpdated;
		_browser.WebView.FaviconChanged += WebView_FaviconChanged;
		_browser.WebView.FileDialog += WebView_FileDialog;
		_browser.WebView.FullScreenModeChanged += WebView_FullScreenModeChanged;
		_browser.WebView.GiveFocus += WebView_GiveFocus;
		_browser.WebView.GotFocus += WebView_GotFocus;
		_browser.WebView.IsReadyChanged += WebView_IsReadyChanged;
		_browser.WebView.JSDialog += WebView_JSDialog;
		_browser.WebView.KeyDown += WebView_KeyDown;
		_browser.WebView.KeyUp += WebView_KeyUp;
		_browser.WebView.LaunchUrl += WebView_LaunchUrl;
		_browser.WebView.LoadCompleted += WebView_LoadCompleted;
		_browser.WebView.MouseClick += WebView_MouseClick;
		_browser.WebView.MouseDoubleClick += WebView_MouseDoubleClick;
		_browser.WebView.MouseDown += WebView_MouseDown;
		_browser.WebView.MouseEnter += WebView_MouseEnter;
		_browser.WebView.MouseLeave += WebView_MouseLeave;
		_browser.WebView.MouseMove += WebView_MouseMove;
		_browser.WebView.MouseUp += WebView_MouseUp;
		_browser.WebView.NeedClientCertificate += WebView_NeedClientCertificate;
		_browser.WebView.NeedCredentials += WebView_NeedCredentials;
		_browser.WebView.RequestPermissions += WebView_RequestPermissions;
		_browser.WebView.ScriptCallDone += WebView_ScriptCallDone;
		_browser.WebView.ShouldForceDownload += WebView_ShouldForceDownload;
		_browser.WebView.StatusMessageChanged += WebView_StatusMessageChanged;
		_browser.WebView.TitleChanged += WebView_TitleChanged;
		_browser.WebView.UrlChanged += WebView_UrlChanged;
		_browser.WebView.ZoomFactorChanged += WebView_ZoomFactorChanged;

		InjectJavascriptObject();
	}

	/// <summary>
	/// Occurs before navigating to other pages.
	/// </summary>
	public event BeforeNavigatingEventHandler BeforeNavigating;
	/// <summary>
	/// Occurs after having navigated to another page.
	/// </summary>
	public event EventHandler Navigated;
	/// <summary>
	/// Occurs when ExorLive is done loading.
	/// </summary>
	public event EventHandler IsLoaded;
	/// <summary>
	/// Occurs when ExorLive is unloading (user signs out).
	/// </summary>
	public event EventHandler IsUnloading;
	/// <summary>
	/// Occurs when the instructor changes his currently active contact.
	/// </summary>
	public event EventHandler SelectedUserChanged;
	/// <summary>
	/// Is the callback of 'getWorkoutsForUserId' and 'getWorkoutsForCustomId'
	/// </summary>
	public event EventHandler ExportUsersDataEvent;
	/// <summary>
	/// Is the callback of 'getListOfUsers'
	/// </summary>
	public event EventHandler ExportUserListEvent;
	/// <summary>
	/// Is the callback of 'selectPerson'
	/// </summary>
	public event EventHandler SelectPersonResultEvent;
	/// <summary>
	/// Is the callback of getOsloSignonDetails
	/// </summary>
	public event EventHandler ExportSignonDetailsEvent;
	/// <summary>
	/// Occurs when the user changes zoom level.
	/// </summary>
	public event EventHandler ZoomLevelChanged;

	/// <summary>
	/// Navigates to the specified URL.
	/// </summary>
	/// <param name="url">The URL.</param>
	/// <returns>Returns true.</returns>
	public bool Navigate(Uri url)
	{
		Log("Navigate");
		_browser.WebView.Url = url.AbsoluteUri;
		return true;
	}

	/// <summary>
	/// Retrieve the browsers current zoom level.
	/// </summary>
	/// <returns>The current zoom level, or a negative number if the browser doesnt support zoom.</returns>
	public decimal GetZoomLevel() => (decimal)_browser.WebView.ZoomFactor;

	/// <summary>
	/// Sets the current zoom level in the browser.
	/// </summary>
	/// <param name="v">A value where 1 equals 100%.</param>
	/// <remarks>Remember to check if the browser supports zoom before setting this.</remarks>
	/// <exception cref="NotImplementedException">Should throw a NotImplementedException if the browser doesnt support zoom.</exception>
	public void SetZoomLevel(decimal v) => _browser.WebView.ZoomFactor = (double)v;

	/// <summary>
	/// Sends an object from the browser which the webwrapper can use to call javascript methods.
	/// </summary>
	/// <remarks>
	/// The obj passed from the browser have begun disappearing sometimes when we use EOBrowser.
	/// So we no longer rely on this object, and instead we retrieve the object directly from DOM every time we want to invoke a method.
	/// See the externalPath string above.
	/// </remarks>
	/// <param name="obj"></param>
	public void SetInterface(object obj) => Log("SetInterface");

	public void NotifyIsLoaded() => IsLoaded?.Invoke(this, new EventArgs());
	public void SelectPerson(string externalId, string firstname, string lastname, string email, string dateOfBirth) => Obj.InvokeFunction("selectPerson", externalId, firstname, lastname, email, dateOfBirth);
	public void SelectPersonById(int id) => Obj.InvokeFunction("selectPersonById", id.ToString());
	public void SelectTab(string tab) => Obj.InvokeFunction("selectTab", tab);
	public void QueryWorkouts(string query) => Obj.InvokeFunction("queryWorkouts", query);
	public void QueryExercises(string query) => Obj.InvokeFunction("queryExercises", query);
	public void GetSignonDetails() => Obj.InvokeFunction("getOsloSignonDetails");
	public void GetListOfUsers(string customId) => Obj.InvokeFunction("getListOfUsers", customId);
	public void OpenWorkout(int id) => Obj.InvokeFunction("openWorkout", id);
	public static void Log(string format, params object[] args) => System.Diagnostics.Debug.WriteLine(format, args);
	public void NotifyIsUnloading() => IsUnloading?.Invoke(this, new EventArgs());
	public bool Debug => App.Debug;
	public string ApplicationIdentifier => App.ApplicationIdentifier;
	public UIElement GetUiElement() => _browser;

	public void SelectPerson2(
		string externalId,
		string firstname,
		string lastname,
		string email,
		string dateOfBirth,
		string address,
		string zipCode,
		string location,
		string mobile,
		string phoneWork,
		int gender,
		string homepage,
		string employer,
		string comment,
		string country,
		string phonehome
	)
	{
		Log("InvokeFunction SelectPerson2");
		Obj.InvokeFunction("selectPerson2",
			externalId,
			firstname,
			lastname,
			email,
			dateOfBirth,
			address,
			zipCode,
			location,
			mobile,
			phoneWork,
			gender.ToString(),
			homepage,
			employer,
			comment
		);
	}
	public void SelectPerson3(
		int userId,
		string externalId,
		string firstname,
		string lastname,
		string email,
		string dateOfBirth,
		string address,
		string zipCode,
		string location,
		string mobile,
		string phoneWork,
		int gender,
		string homepage,
		string employer,
		string comment,
		string country,
		string phoneHome,
		string profiledata,
		string source
	)
	{
		Log("InvokeFunction SelectPerson3");
		Obj.InvokeFunction("selectPerson3",
			userId,
			externalId,
			firstname,
			lastname,
			email,
			dateOfBirth,
			phoneHome,
			phoneWork,
			mobile,
			address,
			zipCode,
			location,
			country,
			gender,
			homepage,
			employer,
			comment,
			profiledata,
			source
		);
	}

	public void GetWorkoutsForClient(int userId, string customId, DateTime from)
	{
		if (userId <= 0)
		{
			Log("InvokeFunction ");
			// Call a Javascript method in ExorLive
			Obj.InvokeFunction("getWorkoutsForCustomId", customId, from);
		}
		else
		{
			Log("InvokeFunction ");
			// Call a Javascript method in ExorLive
			Obj.InvokeFunction("getWorkoutsForUserId", userId, from);
		}
	}


	public void NotifySelectingUser(int id, string externalId, string firstname, string lastname, string email, string dateofbirth)
	{
		var person = new PersonDTO
		{
			UserId = id,
			ExternalId = externalId,
			Firstname = firstname,
			Lastname = lastname,
			Email = email,
			DateOfBirth = dateofbirth
		};
		Log("NotifySelectingUser");
		SelectedUserChanged?.Invoke(this, new SelectedUserEventArgs(person));
	}

	public void ExportUsersData(string jsondata)
	{
		if (!string.IsNullOrWhiteSpace(jsondata))
		{
			ExportUsersDataEvent?.Invoke(this, new JsonEventArgs(jsondata));
		}
	}
	public void ExportUserList(string jsondata)
	{
		if (!string.IsNullOrWhiteSpace(jsondata))
		{
			ExportUserListEvent?.Invoke(this, new JsonEventArgs(jsondata));
		}
	}

	public void ExportSignonDetails(string jsondata)
	{
		if (!string.IsNullOrWhiteSpace(jsondata))
		{
			ExportSignonDetailsEvent?.Invoke(this, new JsonEventArgs(jsondata));
		}
	}

	public void SelectPersonResult(string jsondata)
	{
		if (!string.IsNullOrWhiteSpace(jsondata))
		{
			SelectPersonResultEvent?.Invoke(this, new JsonEventArgs(jsondata));
		}
	}

	private void Runtime_Exception(object sender, EO.Base.ExceptionEventArgs e) { }
	private void WebView_ZoomFactorChanged(object sender, EventArgs e) => ZoomLevelChanged?.Invoke(sender, e);
	private void WebView_UrlChanged(object sender, EventArgs e) { }
	private void WebView_TitleChanged(object sender, EventArgs e) { }
	private void WebView_StatusMessageChanged(object sender, EventArgs e) { }
	private void WebView_ScriptCallDone(object sender, ScriptCallDoneEventArgs e) => Log($"WebView_ScriptCallDone: {e.Call.Script}");
	private void WebView_RequestPermissions(object sender, RequestPermissionEventArgs e) => Log("WebView_RequestPermissions");
	private void WebView_NeedCredentials(object sender, NeedCredentialsEventArgs e) => Log("WebView_NeedCredentials");
	private void WebView_NeedClientCertificate(object sender, NeedClientCertificateEventArgs e) => Log("WebView_NeedClientCertificate");
	private void WebView_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e) { }
	private void WebView_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e) { }
	private void WebView_MouseLeave(object sender, EventArgs e) { }
	private void WebView_MouseEnter(object sender, EventArgs e) { }
	private void WebView_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e) { }
	private void WebView_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e) { }
	private void WebView_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e) { }
	private void WebView_LoadCompleted(object sender, LoadCompletedEventArgs e) => Log("WebView_LoadCompleted");
	private void WebView_LaunchUrl(object sender, LaunchUrlEventArgs e) => Log("WebView_LaunchUrl");
	private void WebView_KeyUp(object sender, WndMsgEventArgs e) { }
	private void WebView_KeyDown(object sender, WndMsgEventArgs e) { }
	private void WebView_JSDialog(object sender, JSDialogEventArgs e) => Log("WebView_JSDialog");
	private void WebView_IsReadyChanged(object sender, EventArgs e) => Log("WebView_IsReadyChanged");
	private void WebView_GotFocus(object sender, EventArgs e) => Log("WebView_GotFocus");
	private void WebView_GiveFocus(object sender, GiveFocusEventArgs e) => Log("WebView_GiveFocus");
	private void WebView_FullScreenModeChanged(object sender, FullscreenModeChangedArgs e) => Log("WebView_FullScreenModeChanged");
	private void WebView_FileDialog(object sender, FileDialogEventArgs e) => Log("WebView_FileDialog");
	private void WebView_FaviconChanged(object sender, EventArgs e) { }
	private void WebView_DownloadUpdated(object sender, DownloadEventArgs e) => Log("WebView_DownloadUpdated");
	private void WebView_DownloadCompleted(object sender, DownloadEventArgs e) => Log("WebView_DownloadCompleted");
	private void WebView_DownloadCanceled(object sender, DownloadEventArgs e) => Log("WebView_DownloadCanceled");
	private void WebView_BeforePrint(object sender, BeforePrintEventArgs e) => Log("WebView_BeforePrint");
	private void WebView_BeforeContextMenu(object sender, BeforeContextMenuEventArgs e) { }
	private void WebView_Disposed(object sender, EventArgs e) => Log("WebView_Disposed");
	private void WebView_ContextMenuDismissed(object sender, FrameEventArgs e) { }
	private void WebView_ConsoleMessage(object sender, ConsoleMessageEventArgs e) { }
	private void WebView_Command(object sender, CommandEventArgs e) => Log("WebView_Command");
	private void WebView_Closing(object sender, CancelEventArgs e) => Log("WebView_Closing");
	private void WebView_Closed(object sender, WebViewClosedEventArgs e) => Log("WebView_Closed");
	private void WebView_CanGoForwardChanged(object sender, EventArgs e) { }
	private void WebView_CanGoBackChanged(object sender, EventArgs e) { }
	private void WebView_BeforeSendHeaders(object sender, RequestEventArgs e) { }
	private void WebView_BeforeRequestLoad(object sender, BeforeRequestLoadEventArgs e) { }
	private void WebView_BeforeDownload(object sender, BeforeDownloadEventArgs e) => Log("WebView_BeforeDownload");
	private void WebView_AfterReceiveHeaders(object sender, ResponseEventArgs e) { }
	private void WebView_AfterPrint(object sender, AfterPrintEventArgs e) => Log("WebView_AfterPrint");
	private void WebView_Activate(object sender, EventArgs e) => Log("WebView_Activate");
	private void WebView_RenderUnresponsive(object sender, RenderUnresponsiveEventArgs e) => Log("WebView_RenderUnresponsive");
	private void StartRemoteDebugging() => Runtime.DefaultEngineOptions.RemoteDebugPort = 9223;

	/// <summary>
	/// This javascript file is added to every page you navigate to.
	/// </summary>
	private void InjectJavascriptObject()
	{
		var appid = Util.EncodeJsString(ApplicationIdentifier);
		var debug = Util.EncodeJsString(Debug.ToString().ToLower());
		var distro = Util.EncodeJsString(Settings.Default.DistributorName);
		var checforupdates = Util.EncodeJsString(App.UserSettings.CheckForUpdates.ToString());
		_browser.WebView.JSInitCode = $@"
			window.external.ApplicationIdentifier = '{appid}';
			window.external.Debug = {debug};
			window.external.DistributorName = '{distro}';
			window.external.CheckForUpdates = '{checforupdates}';
		";
	}

	private void WebView_ShouldForceDownload(object sender, ShouldForceDownloadEventArgs e)
	{
		Log($"WebView_ShouldForceDownload = {e.ForceDownload}.");
		Log($"Url: {e.Url}");
	}

	private void WebView_CertificateError(object sender, CertificateErrorEventArgs e)
	{
		if (App.Debug)
		{
			MessageBox.Show($"Can't load the url \"{e.Url}\", the SSL certificate is invalid.");
		}
	}

	private void WebView_LoadFailed(object sender, LoadFailedEventArgs e)
	{
		switch (e.ErrorCode)
		{
			case ErrorCode.ConnectionRefused:
				e.ErrorMessage = $"Can't connect to \"{e.Url}\".";
				break;
			default:
				e.UseDefaultMessage();
				break;
		}
		Log($"{e.ErrorCode} - {e.ErrorMessage}");
	}

	/// <summary>
	/// For when the browser window navigates to an external link,
	/// open the link in the default browser
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private static void WebView_NewWindow(object sender, NewWindowEventArgs e)
	{
		Log("WebView_NewWindow");
		if (string.IsNullOrWhiteSpace(e.TargetUrl)) { return; }
		var targeturi = new Uri(e.TargetUrl);
		try
		{
			Process.Start(targeturi.AbsoluteUri);
		}
		catch (Win32Exception)
		{
			Process.Start("IExplore.exe", targeturi.AbsoluteUri);
		}
	}

	/// <summary>
	/// Workaround to trigger the Navigated event, because EO's webview only had a BeforeNavigating event.
	/// </summary>
	/// <param name="sender">The source of the event.</param>
	/// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
	private void WebView_IsLoadingChanged(object sender, EventArgs e)
	{
		Log("WebView_IsLoadingChanged");
		if (_isNavigating && ((WebView)sender).IsLoading == false)
		{
			Navigated?.Invoke(sender, e);
			_isNavigating = false;
		}
	}

	/// <summary>
	/// Workaround to trigger the Navigated event, because EO's webview only had a BeforeNavigating event.
	/// </summary>
	/// <param name="sender">The source of the event.</param>
	/// <param name="e">The <see cref="EO.WebBrowser.BeforeNavigateEventArgs" /> instance containing the event data.</param>
	private void WebView_BeforeNavigate(object sender, BeforeNavigateEventArgs e)
	{
		Log("WebView_BeforeNavigate");
		_isNavigating = true;
		BeforeNavigating?.Invoke(sender, new Uri(e.NewUrl));
	}
	/// <summary>
	/// When the Eo.WebBrowser.WebView invokes the js method "eoWebBrowser.extInvoke('MethodName', arguments)",
	/// this method handles its JSExtInvoke event.
	/// </summary>
	/// <param name="sender">The sender.</param>
	/// <param name="arg">The argument.</param>
	private void WebView_JSExtension(object sender, JSExtInvokeArgs arg) => Log($"Incoming call from ExorLive application. Method \"{arg.FunctionName}\"");

#pragma warning disable IDE0051 // Remove unused private members
	private void NotifySelectingUser(JSExtInvokeArgs arg)
	{
		// To cast safely and return null if failure, use C#'s "as" keyword.
		NotifySelectingUser(
			(int)arg.Arguments[0],
			arg.Arguments[1] as string,
			arg.Arguments[2] as string,
			arg.Arguments[3] as string,
			arg.Arguments[4] as string,
			arg.Arguments[5] as string
		);
	}
#pragma warning restore IDE0051 // Remove unused private members

	private JSObject Obj => (JSObject)_browser.WebView.EvalScript(externalPath);
}
