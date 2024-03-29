﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
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
		private WebView2 webView;
		private Uri webView2downloadUri = new Uri("https://go.microsoft.com/fwlink/p/?LinkId=2124703");

		public async Task<UIElement> GetUiElement()
		{
			try
			{
				var version = CoreWebView2Environment.GetAvailableBrowserVersionString();
				if (version == null)
				{
					await DownloadAndInstallWebView2Runtime();
				}
			}
			catch (WebView2RuntimeNotFoundException)
			{
				await DownloadAndInstallWebView2Runtime();
			}

			if (webView == null)
			{
				webView = new WebView2();
				webView.WebMessageReceived += WebView_WebMessageReceived;
				webView.ZoomFactorChanged += WebView_ZoomFactorChanged;
				webView.NavigationStarting += WebView_NavigationStarting;
				webView.NavigationCompleted += WebView_NavigationCompleted;
			}
			return webView;
		}

		/// <summary>
		/// Sets up the environment for WebView2. 
		/// We only get one chance to do this for some reason.
		/// If we run CoreWebView2Environment.CreateAsync() a second time, it will fail with an error
		/// telling us that the environment is already set up.
		/// </summary>
		/// <returns></returns>
		public async Task Initialize()
		{
			var cacheFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ExorLive");
			var environment = await CoreWebView2Environment.CreateAsync(null, cacheFolderPath);
			await webView.EnsureCoreWebView2Async(environment);
		}

		public WebViewBrowser()
		{
		}

		/// <summary>
		/// Needs to run before the webview is created.
		/// </summary>
		/// <returns></returns>
		private async Task DownloadAndInstallWebView2Runtime()
		{
			var tmpfile = Path.GetTempFileName();
			var newname = tmpfile.Replace(".tmp", ".exe");
			if (!File.Exists(newname))
			{
				var wc = new TlsWebClient();
				await wc.DownloadFileTaskAsync(webView2downloadUri, tmpfile);
				File.Move(tmpfile, newname);
			}
			var process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = newname
				}
			};
			process.Start();
			process.WaitForExit();
			File.Delete(newname);
		}

		public bool Navigate(Uri url)
		{
			if(webView == null)
			{
				return false;
			}
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

		private static void Log(string format, params object[] args) => Debug.WriteLine(format, args);

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

		public void SelectPerson(string externalId, string firstname, string lastname, string email, string dateOfBirth, string caseid) => Call("selectPerson", externalId, firstname, lastname, email, dateOfBirth, caseid);
		public void SelectPersonById(int id) => Call("selectPersonById", id);
		public void RegisterWebwrapperSignon(string signon) => Call("registerWebwrapperSignon", signon);
		public void SelectTab(string tab) => Call("selectTab", tab);
		public void QueryWorkouts(string query) => Call("queryWorkouts", query);
		public void QueryExercises(string query) => Call("queryExercises", query);
		public void GetSignonDetails() => Call("getOsloSignonDetails");
		public void GetListOfUsers(string customId) => Call("getListOfUsers", customId);
		public void OpenWorkout(int id) => Call("openWorkout", id);
		public void SelectPerson2(string externalId, string firstname, string lastname, string email, string dateOfBirth, string address, string zipCode, string location, string mobile, string phoneWork, int gender, string homepage, string employer, string comment, string country, string phonehome, string caseid) => Call("selectPerson2", externalId, firstname, lastname, email, dateOfBirth, address, zipCode, location, mobile, phoneWork, gender, homepage, employer, comment, caseid);
		public void SelectPerson3(int userId, string externalId, string firstname, string lastname, string email, string dateOfBirth, string address, string zipCode, string location, string mobile, string phoneWork, int gender, string homepage, string employer, string comment, string country, string phoneHome, string profiledata, string source, string caseid) => Call("selectPerson3", userId, externalId, firstname, lastname, email, dateOfBirth, phoneHome, phoneWork, mobile, address, zipCode, location, country, gender, homepage, employer, comment, profiledata, source, caseid);
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
