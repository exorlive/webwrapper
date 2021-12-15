using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using ExorLive;
using ExorLive.WebWrapper.Interface;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace WebWrapper
{
	public class WebViewBrowser : IBrowser
	{
		private readonly WebView2 webView;
		public UIElement GetUiElement() => webView;

		public WebViewBrowser()
		{
			webView = new WebView2();

			webView.WebMessageReceived += WebView_WebMessageReceived;
			webView.ZoomFactorChanged += WebView_ZoomFactorChanged;
			webView.NavigationStarting += WebView_NavigationStarting;
			webView.NavigationCompleted += WebView_NavigationCompleted;
		}

		public bool Navigate(Uri url)
		{
			webView.Source = url;
			return true;
		}

		private bool _hostObjectAdded = false;
		private void AddHostObject()
		{
			if (!_hostObjectAdded)
			{
				_hostObjectAdded = true;
				webView.CoreWebView2.AddHostObjectToScript("external", new BrowserObject(this));
			}
		}

		private async void WebView_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
		{
			Log("WebView_BeforeNavigate");
			await webView.EnsureCoreWebView2Async();
			AddHostObject();

			BeforeNavigating?.Invoke(sender, new Uri(e.Uri));
		}

		private void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
		{
			Log("WebView_NavigationCompleted");
			Navigated?.Invoke(sender, e);
		}

		private void WebView_ZoomFactorChanged(object sender, EventArgs e) => ZoomFactorChanged?.Invoke(sender, e);

		private static void Log(string format, params object[] args) => System.Diagnostics.Debug.WriteLine(format, args);

		public event BeforeNavigatingEventHandler BeforeNavigating;
		public event EventHandler Navigated;
		public event EventHandler IsLoaded;
		public event EventHandler IsUnloading;
		public event EventHandler SelectedUserChanged;
		public event EventHandler ExportUsersDataEvent;
		public event EventHandler ExportUserListEvent;
		public event EventHandler SelectPersonResultEvent;
		public event EventHandler ExportSignonDetailsEvent;
		public event EventHandler ZoomFactorChanged;

		public decimal GetZoomFactor() => (decimal)webView.ZoomFactor;

		public void NotifyIsLoaded() => IsLoaded?.Invoke(this, new EventArgs());

		public void NotifyIsUnloading() => IsUnloading?.Invoke(this, new EventArgs());

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

		public void SelectPerson(string externalId, string firstname, string lastname, string email, string dateOfBirth) => Call("selectPerson", externalId, firstname, lastname, email, dateOfBirth);
		public void SelectPersonById(int id) => Call("selectPersonById", id);
		public void SelectTab(string tab) => Call("selectTab", tab);
		public void QueryWorkouts(string query) => Call("queryWorkouts", query);
		public void QueryExercises(string query) => Call("queryExercises", query);
		public void GetSignonDetails() => Call("getOsloSignonDetails");
		public void GetListOfUsers(string customId) => Call("getListOfUsers", customId);
		public void OpenWorkout(int id) => Call("openWorkout", id);
		public void SelectPerson2(string externalId, string firstname, string lastname, string email, string dateOfBirth, string address, string zipCode, string location, string mobile, string phoneWork, int gender, string homepage, string employer, string comment, string country, string phonehome) => Call("selectPerson2", externalId, firstname, lastname, email, dateOfBirth, address, zipCode, location, mobile, phoneWork, gender, homepage, employer, comment);
		public void SelectPerson3(int userId, string externalId, string firstname, string lastname, string email, string dateOfBirth, string address, string zipCode, string location, string mobile, string phoneWork, int gender, string homepage, string employer, string comment, string country, string phoneHome, string profiledata, string source) => Call("selectPerson3", userId, externalId, firstname, lastname, email, dateOfBirth, phoneHome, phoneWork, mobile, address, zipCode, location, country, gender, homepage, employer, comment, profiledata, source);
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
		public void SetInterface(object obj) { }
		public void SetZoomFactor(decimal v)
		{
			webView.ZoomFactor = (double)v;
			ZoomFactorChanged?.Invoke(this, new EventArgs());
		}
		public bool SupportsZoom() => true;

		private readonly string externalPath = "app.api._host.externalInterface";
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

			Application.Current.Dispatcher.Invoke(async () =>
			{
				await webView.ExecuteScriptAsync(call);
			}, DispatcherPriority.ContextIdle);
		}

		private void WebView_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
		{
			Log($"WebView_WebMessageReceived");
			Log($"From: '{e.Source}'");
			Log($"Message (json):");
			Log($"{e.WebMessageAsJson}");
		}
	}
}
