using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using ExorLive.WebWrapper.Interface;

namespace WebWrapper
{
	public class WebViewBrowser : IBrowser
	{
		public bool Debug => false;
		public string ApplicationIdentifier => string.Empty;

		// TODO: Fix:
		//public bool Debug => App.Debug;
		//public string ApplicationIdentifier => App.ApplicationIdentifier;

		private static IBrowser _instance;
		public static IBrowser Instance => _instance ?? (_instance = new WebViewBrowser());

		private readonly Microsoft.Web.WebView2.Core.CoreWebView2 _browser;
		public UIElement GetUiElement() => _browser;

		private WebViewBrowser()
		{
		}

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

		public void GetListOfUsers(string customId) => throw new NotImplementedException();
		public void GetSignonDetails() => throw new NotImplementedException();
		public void GetWorkoutsForClient(int userId, string customId, DateTime from) => throw new NotImplementedException();
		public decimal GetZoomFactor() => throw new NotImplementedException();
		public bool Navigate(Uri url) => throw new NotImplementedException();
		public void NotifyIsLoaded() => throw new NotImplementedException();
		public void NotifyIsUnloading() => throw new NotImplementedException();
		public void NotifySelectingUser(int id, string externalId, string firstname, string lastname, string email, string dateofbirth) => throw new NotImplementedException();
		public void OpenWorkout(int id) => throw new NotImplementedException();
		public void QueryExercises(string query) => throw new NotImplementedException();
		public void QueryWorkouts(string query) => throw new NotImplementedException();
		public void SelectPerson(string externalId, string firstname, string lastname, string email, string dateOfBirth) => throw new NotImplementedException();
		public void SelectPerson2(string externalId, string firstname, string lastname, string email, string dateOfBirth, string address, string zipCode, string location, string mobile, string phoneWork, int gender, string homepage, string employer, string comment, string country, string phonehome) => throw new NotImplementedException();
		public void SelectPerson3(int userId, string externalId, string firstname, string lastname, string email, string dateOfBirth, string address, string zipCode, string location, string mobile, string phoneWork, int gender, string homepage, string employer, string comment, string country, string phonehome, string profiledata, string source) => throw new NotImplementedException();
		public void SelectPersonById(int id) => throw new NotImplementedException();
		public void SelectTab(string tab) => throw new NotImplementedException();
		public void SetInterface(object obj) => throw new NotImplementedException();
		public void SetZoomFactor(decimal v) => throw new NotImplementedException();
		public bool SupportsZoom() => throw new NotImplementedException();
	}
}
