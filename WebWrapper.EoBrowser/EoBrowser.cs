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
using ExorLive.WebWrapper.Interface;
using WebWrapper.Properties;
using WebView = EO.WebBrowser.WebView;

/// <summary>
/// A interface for the Eo.WebBrowser object.
/// http://www.essentialobjects.com/Products/WebBrowser/
/// Documentation of the Eo browser API:
/// https://www.essentialobjects.com/doc/webbrowser/start/overview.aspx
/// </summary>
public class EoBrowser : IBrowser
{
	private bool Debug;
	private string ApplicationIdentifier;

	// TODO: Fix:
	private string DistributorName = string.Empty;
	//private string DistributorName = Settings.Default.DistributorName;
	private bool CheckForUpdates = false;
	//private bool CheckForUpdates = App.UserSettings.CheckForUpdates;

	private readonly WebControl _browser;
	private readonly EoBrowserObject browserObject;
	private readonly string externalPath = "app.api._host.externalInterface";
	private bool _isNavigating;

	/// <summary>
	/// Constructor that sets up the WebControl, then attaches a javascript to it.
	/// THis javascript object gets added to every page the webview navigates to.
	/// </summary>
	public EoBrowser(string applicationIdentifier, bool debug, string distributorName, bool checkForUpdates)
	{
		Debug = debug;
		ApplicationIdentifier = applicationIdentifier;
		DistributorName = distributorName;
		CheckForUpdates = checkForUpdates;

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
		Runtime.DefaultEngineOptions.AllowProprietaryMediaFormats();
		Runtime.DefaultEngineOptions.UILanguage = CultureInfo.CurrentUICulture.Name;
		EO.Base.Runtime.EnableCrashReport = false;
		EO.Base.Runtime.EnableEOWP = true;
		if (Debug)
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
		_browser.WebView.BeforeNavigate += WebView_BeforeNavigate;
		_browser.WebView.CertificateError += WebView_CertificateError;
		_browser.WebView.IsLoadingChanged += WebView_IsLoadingChanged;
		_browser.WebView.JSExtInvoke += WebView_JSExtension;
		_browser.WebView.LoadFailed += WebView_LoadFailed;
		_browser.WebView.NewWindow += WebView_NewWindow;
		_browser.WebView.LaunchUrl += WebView_LaunchUrl;
		_browser.WebView.NeedClientCertificate += WebView_NeedClientCertificate;
		_browser.WebView.ShouldForceDownload += WebView_ShouldForceDownload;
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
	public event EventHandler ZoomFactorChanged;

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
	/// <returns>The current zoom level.</returns>
	public decimal GetZoomFactor() => (decimal)_browser.WebView.ZoomFactor;

	/// <summary>
	/// Sets the current zoom level in the browser.
	/// </summary>
	/// <param name="v">A value where 1 equals 100%.</param>
	/// <remarks>Remember to check if the browser supports zoom before setting this.</remarks>
	/// <exception cref="NotImplementedException">Should throw a NotImplementedException if the browser doesnt support zoom.</exception>
	public void SetZoomFactor(decimal v) => _browser.WebView.ZoomFactor = (double)v;

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
	public void NotifyIsUnloading() => IsUnloading?.Invoke(this, new EventArgs());


	public void SelectPerson(string externalId, string firstname, string lastname, string email, string dateOfBirth) => Call("selectPerson", externalId, firstname, lastname, email, dateOfBirth);

	public void SelectPersonById(int id) => Call("selectPersonById", id);

	public void SelectTab(string tab) => Call("selectTab", tab);

	public void QueryWorkouts(string query) => Call("queryWorkouts", query);

	public void QueryExercises(string query) => Call("queryExercises", query);

	public void GetSignonDetails() => Call("getOsloSignonDetails");

	public void GetListOfUsers(string customId) => Call("getListOfUsers", customId);

	public void OpenWorkout(int id) => Call("openWorkout", id);

	private static void Log(string format, params object[] args) => System.Diagnostics.Debug.WriteLine(format, args);

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
		Call(
			"selectPerson2",
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
			gender,
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
		Call("selectPerson3",
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
			Call("getWorkoutsForCustomId", customId, from);
		}
		else
		{
			Call("getWorkoutsForUserId", userId, from);
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

	private void WebView_ZoomFactorChanged(object sender, EventArgs e) => ZoomFactorChanged?.Invoke(sender, e);
	private void WebView_NeedClientCertificate(object sender, NeedClientCertificateEventArgs e) { }

	private void WebView_LaunchUrl(object sender, LaunchUrlEventArgs e)
	{
		Log("WebView_LaunchUrl");
		// Support OS handlers for some protocols.
		if (
			e.Url.StartsWith("mailto://") ||
			e.Url.StartsWith("netid://")
		)
		{
			e.UseOSHandler = true;
		}
	}

	private void StartRemoteDebugging() => Runtime.DefaultEngineOptions.RemoteDebugPort = 9223;

	/// <summary>
	/// This javascript file is added to every page you navigate to.
	/// </summary>
	private void InjectJavascriptObject()
	{
		var appid = Util.EncodeJsString(ApplicationIdentifier);
		var debug = Util.EncodeJsString(Debug.ToString().ToLower());
		var distro = Util.EncodeJsString(DistributorName);
		var checforupdates = Util.EncodeJsString(CheckForUpdates.ToString());
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
		if (Debug)
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

	public bool SupportsZoom() => true;

	private void Call(string method, params object[] args)
	{
		var argslist = new List<string>();
		foreach (var item in args)
		{
			if (item is string itstring)
			{
				argslist.Add($"'{itstring}'");
			}
			else if (item is null)
			{
				argslist.Add($"null");
			}
			else if (item is DateTime it)
			{
				var ite = it.ToString("s");
				argslist.Add($"Date('{ite}')");
			}
			else if (item is bool isIt)
			{
				if (isIt)
				{
					argslist.Add($"true");
				}
				else
				{
					argslist.Add($"false");
				}
			}
			else
			{
				argslist.Add($"{item}");
			}
		}
		var arguments = string.Join(",", argslist);
		var call = $"{externalPath}.{method}({arguments})";
		_browser.WebView.QueueScriptCall(call);
	}
}
