using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using EO.WebBrowser;
using EO.WebBrowser.Wpf;
using ExorLive;
using ExorLive.Client.WebWrapper;
using ExorLive.Properties;
using WebView = EO.WebBrowser.WebView;

/// <summary>
/// A interface for the Eo.WebBrowser object.
/// http://www.essentialobjects.com/Products/WebBrowser/
/// </summary>
public class EoBrowser : IBrowser
{
	private static IBrowser _instance;
	public static IBrowser Instance => _instance ?? (_instance = new EoBrowser());
	private readonly WebControl _browser;
	private JSObject _obj;
	/// <summary>
	/// Navigates to the specified URL.
	/// </summary>
	/// <param name="url">The URL.</param>
	/// <returns>Returns true.</returns>
	public bool Navigate(Uri url)
	{
		_browser.WebView.LoadUrlAndWait(url.ToString());
		return true;
	}

	/// <summary>
	/// Private constructor that sets up the WebControl, then attaches a javascript to it.
	/// THis javascript object gets added to every page the webview navigates to.
	/// </summary>
	private EoBrowser()
	{
		if (!string.IsNullOrWhiteSpace(Resources.EoBrowserLicenseFile))
		{
			Runtime.AddLicense(Resources.EoBrowserLicenseFile);
		}
		_browser = new WebControl();
		_browser.WebView.BeforeNavigate += WebView_BeforeNavigate;
		_browser.WebView.IsLoadingChanged += WebView_IsLoadingChanged;
		_browser.WebView.JSExtInvoke += WebView_JSExtension;
		_browser.WebView.NewWindow += WebView_NewWindow;

		// This javascript file is added to every page you navigate to.
		var jsfile = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "eoBrowserObject.js");
		using (var streamreader = new StreamReader(jsfile.OpenText().BaseStream))
		{
			var jsstream = streamreader.ReadToEnd();
			_browser.WebView.JSInitCode =
				$"{jsstream}; " +
				$"window.external.ApplicationIdentifier = '{EncodeJsString(ApplicationIdentifier)}'; " +
				$"window.external.Debug = { EncodeJsString(Debug.ToString().ToLower()) }; " +
				$"window.external.DistributorName = '{ EncodeJsString(Settings.Default.DistributorName) }'; " +
				$"window.external.CheckForUpdates = '{ EncodeJsString(App.UserSettings.CheckForUpdates.ToString()) }'; ";
		}
	}

	private static void WebView_NewWindow(object sender, NewWindowEventArgs e)
	{
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
		if (_isNavigating && ((WebView)sender).IsLoading == false)
		{
			Navigated?.Invoke(sender, e);
			_isNavigating = false;
		}
	}

	private bool _isNavigating;
	/// <summary>
	/// Workaround to trigger the Navigated event, because EO's webview only had a BeforeNavigating event.
	/// </summary>
	/// <param name="sender">The source of the event.</param>
	/// <param name="e">The <see cref="EO.WebBrowser.BeforeNavigateEventArgs" /> instance containing the event data.</param>
	private void WebView_BeforeNavigate(object sender, BeforeNavigateEventArgs e)
	{
		_isNavigating = true;
		BeforeNavigating?.Invoke(sender, new Uri(e.NewUrl));
	}
	public void NotifyIsLoaded()
	{
		IsLoaded?.Invoke(this, new EventArgs());
	}
	/// <summary>
	/// When the Eo.WebBrowser.WebView invokes the js method "eoWebBrowser.extInvoke('MethodName', arguments)",
	/// this method handles its JSExtInvoke event.
	/// </summary>
	/// <param name="sender">The sender.</param>
	/// <param name="arg">The argument.</param>
	private void WebView_JSExtension(object sender, JSExtInvokeArgs arg)
	{
		switch (arg.FunctionName.ToLower())
		{
			case "setinterface":
				SetInterface(arg);
				break;
			case "notifyisloaded":
				NotifyIsLoaded();
				break;
			case "notifyselectinguser":
				NotifySelectingUser(arg);
				break;
			case "notifyisunloading":
				NotifyIsUnloading();
				break;
			case "exportuserlist":
				// Callback of the call 'getListOfUsers'
				ExportUserList(arg);
				break;
			case "exportusersdata":
				// Callback of the call 'getWorkoutsForCustomId'
				ExportUsersData(arg);
				break;
		}
	}
	public void SetInterface(object arguments)
	{
		_obj = (JSObject)((JSExtInvokeArgs)arguments).Arguments[0];
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


	public void SelectPerson(string externalId, string firstname, string lastname, string email, string dateOfBirth)
	{
		_obj.InvokeFunction("selectPerson", externalId, firstname, lastname, email, dateOfBirth);
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
		_obj.InvokeFunction("selectPerson2",
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
			comment,
			country,
			phonehome
		);
	}
	public void SelectPersonById(int id)
	{
		_obj.InvokeFunction("selectPersonById", id.ToString());
	}
	public void SelectTab(string tab)
	{
		_obj.InvokeFunction("selectTab", tab);
	}
	public void QueryWorkouts(string query)
	{
		_obj.InvokeFunction("queryWorkouts", query);
	}
	public void QueryExercises(string query)
	{
		_obj.InvokeFunction("queryExercises", query);
	}
	public void GetWorkoutsForClient(int userId, string customId, DateTime from)
	{
		try
		{
			if (userId <= 0)
			{
				// Call a Javascript method in ExorLive
				_obj.InvokeFunction("getWorkoutsForCustomId", customId, from);
			}
			else
			{
				// Call a Javascript method in ExorLive
				_obj.InvokeFunction("getWorkoutsForUserId", userId, from);
			}
		}
		catch (Exception)
		{
			// Ignore any error in ExorLive. Just to make WebWrapper don't crash in case of a problem in ExorLive.
		}
	}

	public void GetListOfUsers(string customId)
	{
		try
		{
			// Call a Javascript method in ExorLive
			var arr = _obj.GetPropertyNames();
			_obj.InvokeFunction("getListOfUsers", customId);
		}
		catch (Exception ex)
		{
			MessageBox.Show(ex.Message);
			// Ignore any error in ExorLive. Just to make WebWrapper don't crash in case of a problem in ExorLive.
		}
	}

	public void OpenWorkout(int id)
	{
		try
		{
			// Call a Javascript method in ExorLive
			_obj.InvokeFunction("openWorkout", id);
		}
		catch (Exception)
		{
			// Ignore any error in ExorLive. Just to make WebWrapper don't crash in case of a problem in ExorLive.
		}
	}

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

	public void NotifySelectingUser(int id, string externalId, string firstname, string lastname, string email, string dateofbirth)
	{
		var person = new PersonDTO
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

	private void ExportUsersData(JSExtInvokeArgs arg)
	{
		var jsondata = arg.Arguments[0] as string;
		if (!string.IsNullOrWhiteSpace(jsondata))
		{
			ExportUsersDataEvent?.Invoke(this, new JsonEventArgs(jsondata));
		}
	}
	private void ExportUserList(JSExtInvokeArgs arg)
	{
		var jsondata = arg.Arguments[0] as string;
		if (!string.IsNullOrWhiteSpace(jsondata))
		{
			ExportUserListEvent?.Invoke(this, new JsonEventArgs(jsondata));
		}
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
	/// <summary>
	/// Encodes a string to be represented as a string literal. The format
	/// is essentially a JSON string.
	/// </summary>
	/// <param name="str">The string.</param>
	/// <returns></returns>
	/// <remarks>
	/// http://weblog.west-wind.com/posts/2007/Jul/14/Embedding-JavaScript-Strings-from-an-ASPNET-Page
	/// </remarks>
	private static string EncodeJsString(string str)
	{
		var sb = new StringBuilder();
		foreach (var c in str)
		{
			switch (c)
			{
				case '\"':
					sb.Append("\\\"");
					break;
				case '\\':
					sb.Append("\\\\");
					break;
				case '\b':
					sb.Append("\\b");
					break;
				case '\f':
					sb.Append("\\f");
					break;
				case '\n':
					sb.Append("\\n");
					break;
				case '\r':
					sb.Append("\\r");
					break;
				case '\t':
					sb.Append("\\t");
					break;
				default:
					var i = (int)c;
					if (i < 32 || i > 127)
					{
						sb.AppendFormat("\\u{0:X04}", i);
					}
					else
					{
						sb.Append(c);
					}
					break;
			}
		}
		return sb.ToString();
	}

}