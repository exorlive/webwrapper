using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using ExorLive;
using ExorLive.Client.WebWrapper;

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
	public bool Navigate(Uri url)
	{
		_browser.Navigate(url);
		return true;
	}
	private WindowsBrowser()
	{
		_browser = new WebBrowser
		{
			ObjectForScripting = this
		};
		_browser.Navigated += browser_navigated;
		_browser.Navigating += _browser_Navigating;
	}

	private void _browser_Navigating(object sender, NavigatingCancelEventArgs e)
	{
		BeforeNavigating?.Invoke(sender, e.Uri);
	}

	public event EventHandler Navigated;
	public event EventHandler IsLoaded;
	public event EventHandler IsUnloading;
	public event EventHandler SelectedUserChanged;
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

	public void SelectPersonById(int id)
	{
		_exorlive.selectPersonById(id);
	}

	public void SelectTab(string tab)
	{
		_exorlive.selectTab(tab);
	}

	public void QueryWorkouts(string query)
	{
		_exorlive.queryWorkouts(query);
	}

	public void QueryExercises(string query)
	{
		_exorlive.queryExercises(query);
	}

	private void browser_navigated(object sender, NavigationEventArgs e)
	{
		HideScriptErrors(_browser, true);
		Navigated?.Invoke(sender, e);
	}

	private static void HideScriptErrors(FrameworkElement wb, bool hide)
	{
		var fiComWebBrowser = typeof(WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
		if (fiComWebBrowser == null) return;
		var objComWebBrowser = fiComWebBrowser.GetValue(wb);
		if (objComWebBrowser == null)
		{
			wb.Loaded += (o, s) => HideScriptErrors(wb, hide); //In case we are to early
			return;
		}
		objComWebBrowser.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, objComWebBrowser, new object[] { hide });
	}
	public void SetInterface(object obj)
	{
		_exorlive = new ExorLiveInterface(obj);
	}

	public event BeforeNavigatingEventHandler BeforeNavigating;

	public void NotifyIsLoaded()
	{
		IsLoaded?.Invoke(this, new EventArgs());
	}
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
	public void NotifyIsUnloading()
	{
		IsUnloading?.Invoke(this, new EventArgs());
	}
	public bool Debug => App.Debug;
	public string ApplicationIdentifier => App.ApplicationIdentifier;
	public UIElement GetUiElement()
	{
		return _browser;
	}
}
