using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using ExorLive;
using ExorLive.Client.WebWrapper;
using Microsoft.Win32;

[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
// Options for PermissionSet:	https://msdn.microsoft.com/en-us/library/4652tyx7.aspx	
// Documentation:				https://msdn.microsoft.com/en-us/library/system.windows.forms.webbrowser.objectforscripting.aspx?cs-save-lang=1&cs-lang=csharp
[ComVisible(true)]
public class WindowsBrowser : IBrowser
{
	private static IBrowser _instance;
	public static IBrowser Instance => _instance ?? (_instance = new WindowsBrowser());
	private readonly WebBrowser _browser;
	private IExorLiveInterface _exorlive;

	private WindowsBrowser()
	{
		SetIEDefaultCompatibilityMode();
		_browser = new WebBrowser
		{
			ObjectForScripting = this
		};
		_browser.Navigated += Browser_navigated;
		_browser.Navigating += _browser_Navigating;
	}

	/// <summary>
	/// Sets the WebControls default compatibility mode to use IE11 Edge mode.
	/// https://stackoverflow.com/questions/38514184/how-can-i-get-the-webbrowser-control-to-show-modern-contents
	/// </summary>
	private static void SetIEDefaultCompatibilityMode()
	{
		using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", true))
		{
			var apploc = Assembly.GetExecutingAssembly().Location;
			var app = Path.GetFileName(apploc);
			key.SetValue(app, 11001, RegistryValueKind.DWord);
			key.Close();
		}
	}

	public event EventHandler Navigated;
	public event EventHandler IsLoaded;
	public event EventHandler IsUnloading;
	public event EventHandler SelectedUserChanged;
	public event EventHandler ExportUsersDataEvent;
	public event EventHandler ExportUserListEvent;
	public event EventHandler SelectPersonResultEvent;
	public event EventHandler ExportSignonDetailsEvent;
	public event BeforeNavigatingEventHandler BeforeNavigating;
	public event EventHandler ZoomFactorChanged;

	public bool Navigate(Uri url)
	{
		_browser.Navigate(url);
		return true;
	}

	public void SelectPerson(
		string externalId,
		string firstname,
		string lastname,
		string email,
		string dateOfBirth
	)
	{
		_exorlive.selectPerson(
			externalId,
			firstname,
			lastname,
			email,
			dateOfBirth);
	}

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
		_exorlive.selectPerson2(
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
			comment,
			country,
			phonehome
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
		string phonehome,
		string profiledata,
		string source
	)
	{
		_exorlive.selectPerson3(
			userId,
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
			comment,
			country,
			phonehome,
			profiledata,
			source
		);
	}

	public void SelectPersonById(int id) => _exorlive.selectPersonById(id);

	public void SelectTab(string tab) => _exorlive.selectTab(tab);

	public void QueryWorkouts(string query) => _exorlive.queryWorkouts(query);

	public void QueryExercises(string query) => _exorlive.queryExercises(query);

	public void GetWorkoutsForClient(int userId, string customId, DateTime from) => _exorlive.getWorkoutsForCustomId(userId, customId, from);

	public void GetSignonDetails() => _exorlive.getOsloSignonDetails();

	public void GetListOfUsers(string customId) => _exorlive.getListOfUsers(customId);

	public void OpenWorkout(int id) => _exorlive.openWorkout(id);

	/// <summary>
	/// Is called from the browser COM object
	/// </summary>
	public void SetInterface(object obj) => _exorlive = new ExorLiveInterface(obj);

	/// <summary>
	/// Is called from the browser COM object
	/// </summary>
	public void NotifyIsLoaded() => IsLoaded?.Invoke(this, new EventArgs());

	/// <summary>
	/// Is called from the browser COM object
	/// </summary>
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

	/// <summary>
	/// Is called from the browser COM object
	/// </summary>
	public void NotifyIsUnloading() => IsUnloading?.Invoke(this, new EventArgs());

	/// <summary>
	/// Is called from the browser COM object
	/// </summary>
	/// <param name="jsondata"></param>
	public void ExportUsersData(string jsondata) => ExportUsersDataEvent?.Invoke(this, new JsonEventArgs(jsondata));

	/// <summary>
	/// Is called from the browser COM object
	/// </summary>
	/// <param name="jsondata"></param>
	public void ExportUserList(string jsondata) => ExportUserListEvent?.Invoke(this, new JsonEventArgs(jsondata));
	public void SelectPersonResult(string jsondata) => SelectPersonResultEvent?.Invoke(this, new JsonEventArgs(jsondata));
	public void ExportSignonDetails(string jsondata) => ExportSignonDetailsEvent?.Invoke(this, new JsonEventArgs(jsondata));

	public bool Debug => App.Debug;
	public string ApplicationIdentifier => App.ApplicationIdentifier;
	public UIElement GetUiElement() => _browser;

	/// <summary>
	/// Retrieve the browsers current zoom level.
	/// </summary>
	/// <returns>The current zoom level.</returns>
	/// <exception cref="NotSupportedException">Should throw a NotImplementedException if the browser doesnt support zoom.</exception>
	public decimal GetZoomFactor() =>
		// System.Windows.Controls.WebBrowser doesnt have an API for zoom levels.
		throw new NotSupportedException();

	/// <summary>
	/// Sets the current zoom level in the browser.
	/// </summary>
	/// <param name="v">A value where 1 equals 100%.</param>
	/// <remarks>Remember to check if the browser supports zoom before setting this.</remarks>
	/// <exception cref="NotSupportedException">Should throw a NotImplementedException if the browser doesnt support zoom.</exception>
	public void SetZoomFactor(decimal v) =>
		// System.Windows.Controls.WebBrowser doesnt have an API for zoom levels.
		throw new NotSupportedException();

	private void _browser_Navigating(object sender, NavigatingCancelEventArgs e) => BeforeNavigating?.Invoke(sender, e.Uri);

	private void Browser_navigated(object sender, NavigationEventArgs e)
	{
		HideScriptErrors(_browser, true);
		Navigated?.Invoke(sender, e);
	}

	private static void HideScriptErrors(FrameworkElement wb, bool hide)
	{
		var fiComWebBrowser = typeof(WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
		if (fiComWebBrowser == null)
		{
			return;
		}
		var objComWebBrowser = fiComWebBrowser.GetValue(wb);
		if (objComWebBrowser == null)
		{
			wb.Loaded += (o, s) => HideScriptErrors(wb, hide); //In case we are to early
			return;
		}
		objComWebBrowser.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, objComWebBrowser, new object[] { hide });
	}

	public bool SupportsZoom() => false;
}
