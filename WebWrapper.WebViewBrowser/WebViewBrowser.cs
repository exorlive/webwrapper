using System;
using System.Windows;
using ExorLive.WebWrapper.Interface;
using Microsoft.Web.WebView2.Wpf;
using Microsoft.Web.WebView2.Core;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Data;
using System.Collections.Generic;
using ExorLive;

namespace WebWrapper
{
	public class WebViewBrowser : IBrowser
	{
		private readonly WebView2 webView;
		public UIElement GetUiElement() => webView;

		public WebViewBrowser()
		{
			webView = new WebView2();

			webView.ContentLoading += WebView_ContentLoading;
			webView.ContextMenuClosing += WebView_ContextMenuClosing;
			webView.ContextMenuOpening += WebView_ContextMenuOpening;
			webView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;
			webView.DataContextChanged += WebView_DataContextChanged;
			webView.FocusableChanged += WebView_FocusableChanged;
			webView.GiveFeedback += WebView_GiveFeedback;
			webView.GotFocus += WebView_GotFocus;
			webView.Initialized += WebView_Initialized;
			webView.IsEnabledChanged += WebView_IsEnabledChanged;
			webView.IsHitTestVisibleChanged += WebView_IsHitTestVisibleChanged;
			webView.IsKeyboardFocusedChanged += WebView_IsKeyboardFocusedChanged;
			webView.IsKeyboardFocusWithinChanged += WebView_IsKeyboardFocusWithinChanged;
			webView.IsVisibleChanged += WebView_IsVisibleChanged;
			webView.TargetUpdated += WebView_TargetUpdated;
			webView.TextInput += WebView_TextInput;
			webView.ToolTipClosing += WebView_ToolTipClosing;
			webView.ToolTipOpening += WebView_ToolTipOpening;
			webView.Unloaded += WebView_Unloaded;
			webView.WebMessageReceived += WebView_WebMessageReceived;
			webView.LayoutUpdated += WebView_LayoutUpdated;
			webView.LostFocus += WebView_LostFocus;
			webView.RequestBringIntoView += WebView_RequestBringIntoView;
			webView.SizeChanged += WebView_SizeChanged;
			webView.SourceChanged += WebView_SourceChanged;
			webView.SourceUpdated += WebView_SourceUpdated;
			webView.Loaded += WebView_Loaded;

			// Changed
			webView.ZoomFactorChanged += WebView_ZoomFactorChanged;

			// Navigation
			webView.NavigationStarting += WebView_NavigationStarting;
			webView.NavigationCompleted += WebView_NavigationCompleted;


			// Keyboard
			webView.GotKeyboardFocus += WebView_GotKeyboardFocus;
			webView.KeyDown += WebView_KeyDown;
			webView.KeyUp += WebView_KeyUp;
			webView.LostKeyboardFocus += WebView_LostKeyboardFocus;

			// Manipulation Processor
			webView.ManipulationBoundaryFeedback += WebView_ManipulationBoundaryFeedback;
			webView.ManipulationCompleted += WebView_ManipulationCompleted;
			webView.ManipulationDelta += WebView_ManipulationDelta;
			webView.ManipulationInertiaStarting += WebView_ManipulationInertiaStarting;
			webView.ManipulationStarted += WebView_ManipulationStarted;
			webView.ManipulationStarting += WebView_ManipulationStarting;

			// Mouse
			webView.DragEnter += WebView_DragEnter;
			webView.DragLeave += WebView_DragLeave;
			webView.DragOver += WebView_DragOver;
			webView.Drop += WebView_Drop;
			webView.LostMouseCapture += WebView_LostMouseCapture;
			webView.MouseDown += WebView_MouseDown;
			webView.MouseEnter += WebView_MouseEnter;
			webView.MouseLeave += WebView_MouseLeave;
			webView.MouseLeftButtonDown += WebView_MouseLeftButtonDown;
			webView.MouseLeftButtonUp += WebView_MouseLeftButtonUp;
			webView.MouseMove += WebView_MouseMove;
			webView.MouseRightButtonDown += WebView_MouseRightButtonDown;
			webView.MouseRightButtonUp += WebView_MouseRightButtonUp;
			webView.MouseUp += WebView_MouseUp;
			webView.MouseWheel += WebView_MouseWheel;
			webView.GotMouseCapture += WebView_GotMouseCapture;
			webView.IsMouseCapturedChanged += WebView_IsMouseCapturedChanged;
			webView.IsMouseCaptureWithinChanged += WebView_IsMouseCaptureWithinChanged;
			webView.IsMouseDirectlyOverChanged += WebView_IsMouseDirectlyOverChanged;
			webView.QueryContinueDrag += WebView_QueryContinueDrag;
			webView.QueryCursor += WebView_QueryCursor;

			// Preview
			webView.PreviewDragEnter += WebView_PreviewDragEnter;
			webView.PreviewDragLeave += WebView_PreviewDragLeave;
			webView.PreviewDragOver += WebView_PreviewDragOver;
			webView.PreviewDrop += WebView_PreviewDrop;
			webView.PreviewGiveFeedback += WebView_PreviewGiveFeedback;
			webView.PreviewGotKeyboardFocus += WebView_PreviewGotKeyboardFocus;
			webView.PreviewKeyDown += WebView_PreviewKeyDown;
			webView.PreviewKeyUp += WebView_PreviewKeyUp;
			webView.PreviewLostKeyboardFocus += WebView_PreviewLostKeyboardFocus;
			webView.PreviewMouseDown += WebView_PreviewMouseDown;
			webView.PreviewMouseLeftButtonDown += WebView_PreviewMouseLeftButtonDown;
			webView.PreviewMouseLeftButtonUp += WebView_PreviewMouseLeftButtonUp;
			webView.PreviewMouseMove += WebView_PreviewMouseMove;
			webView.PreviewMouseRightButtonDown += WebView_PreviewMouseRightButtonDown;
			webView.PreviewMouseRightButtonUp += WebView_PreviewMouseRightButtonUp;
			webView.PreviewMouseUp += WebView_PreviewMouseUp;
			webView.PreviewMouseWheel += WebView_PreviewMouseWheel;
			webView.PreviewQueryContinueDrag += WebView_PreviewQueryContinueDrag;
			webView.PreviewTextInput += WebView_PreviewTextInput;
			webView.PreviewTouchDown += WebView_PreviewTouchDown;
			webView.PreviewTouchMove += WebView_PreviewTouchMove;
			webView.PreviewTouchUp += WebView_PreviewTouchUp;

			// Stylus
			webView.GotStylusCapture += WebView_GotStylusCapture;
			webView.IsStylusCapturedChanged += WebView_IsStylusCapturedChanged;
			webView.IsStylusCaptureWithinChanged += WebView_IsStylusCaptureWithinChanged;
			webView.IsStylusDirectlyOverChanged += WebView_IsStylusDirectlyOverChanged;
			webView.LostStylusCapture += WebView_LostStylusCapture;
			webView.PreviewStylusButtonDown += WebView_PreviewStylusButtonDown;
			webView.PreviewStylusButtonUp += WebView_PreviewStylusButtonUp;
			webView.PreviewStylusDown += WebView_PreviewStylusDown;
			webView.PreviewStylusInAirMove += WebView_PreviewStylusInAirMove;
			webView.PreviewStylusInRange += WebView_PreviewStylusInRange;
			webView.PreviewStylusMove += WebView_PreviewStylusMove;
			webView.PreviewStylusOutOfRange += WebView_PreviewStylusOutOfRange;
			webView.PreviewStylusSystemGesture += WebView_PreviewStylusSystemGesture;
			webView.PreviewStylusUp += WebView_PreviewStylusUp;
			webView.StylusButtonDown += WebView_StylusButtonDown;
			webView.StylusButtonUp += WebView_StylusButtonUp;
			webView.StylusDown += WebView_StylusDown;
			webView.StylusEnter += WebView_StylusEnter;
			webView.StylusInAirMove += WebView_StylusInAirMove;
			webView.StylusInRange += WebView_StylusInRange;
			webView.StylusLeave += WebView_StylusLeave;
			webView.StylusMove += WebView_StylusMove;
			webView.StylusOutOfRange += WebView_StylusOutOfRange;
			webView.StylusSystemGesture += WebView_StylusSystemGesture;
			webView.StylusUp += WebView_StylusUp;

			// Touch
			webView.TouchDown += WebView_TouchDown;
			webView.TouchEnter += WebView_TouchEnter;
			webView.TouchLeave += WebView_TouchLeave;
			webView.TouchMove += WebView_TouchMove;
			webView.TouchUp += WebView_TouchUp;
			webView.LostTouchCapture += WebView_LostTouchCapture;
			webView.GotTouchCapture += WebView_GotTouchCapture;
		}

		internal void ExportSignonDetails(string arg) => throw new NotImplementedException();
		internal void SelectPersonResult(string jsonresult) => throw new NotImplementedException();
		internal void ExportUserList(string customId) => throw new NotImplementedException();
		internal void ExportUsersData(string arg) => throw new NotImplementedException();

		public bool Navigate(Uri url)
		{
			webView.Source = url;
			return true;
		}

		private void WebView_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
		{
			Log("WebView_BeforeNavigate");
			BeforeNavigating?.Invoke(sender, new Uri(e.Uri));
		}

		private void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
		{
			Log("WebView_NavigationCompleted");
			Navigated?.Invoke(sender, e);
		}

		private void WebView_ZoomFactorChanged(object sender, EventArgs e)
		{
			Log("WebView_ZoomFactorChanged");
			ZoomFactorChanged?.Invoke(sender, e);
		}

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

		public void GetWorkoutsForClient(int userId, string customId, DateTime from) { }
		public decimal GetZoomFactor()
		{
			return (decimal)webView.ZoomFactor;
		}

		// Notify methods
		public void NotifyIsLoaded()
		{
			Log("NotifyIsLoaded");
			IsLoaded?.Invoke(this, new EventArgs());
		}

		public void NotifyIsUnloading()
		{
			Log("NotifyIsUnloading");
			IsUnloading?.Invoke(this, new EventArgs());
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

		public void SelectPerson(string externalId, string firstname, string lastname, string email, string dateOfBirth) => Call("selectPerson", externalId, firstname, lastname, email, dateOfBirth);

		public void SelectPersonById(int id) => Call("selectPersonById", id);

		public void SelectTab(string tab) => Call("selectTab", tab);

		public void QueryWorkouts(string query) => Call("queryWorkouts", query);

		public void QueryExercises(string query) => Call("queryExercises", query);

		public void GetSignonDetails() => Call("getOsloSignonDetails");

		public void GetListOfUsers(string customId) => Call("getListOfUsers", customId);

		public void OpenWorkout(int id) => Call("openWorkout", id);
		public void SelectPerson2(string externalId, string firstname, string lastname, string email, string dateOfBirth, string address, string zipCode, string location, string mobile, string phoneWork, int gender, string homepage, string employer, string comment, string country, string phonehome)
		{
			Call("selectPerson2", externalId, firstname, lastname, email, dateOfBirth, address, zipCode, location, mobile, phoneWork, gender, homepage, employer, comment);
		}
		public void SelectPerson3(int userId, string externalId, string firstname, string lastname, string email, string dateOfBirth, string address, string zipCode, string location, string mobile, string phoneWork, int gender, string homepage, string employer, string comment, string country, string phoneHome, string profiledata, string source)
		{
			Call("selectPerson3", userId, externalId, firstname, lastname, email, dateOfBirth, phoneHome, phoneWork, mobile, address, zipCode, location, country, gender, homepage, employer, comment, profiledata, source);
		}

		public void SetInterface(object obj)
		{
			Log("SetInterface");
		}

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
			webView.CoreWebView2.ExecuteScriptAsync(call);
		}

		private void WebView_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
		{
			Log($"WebView_WebMessageReceived");
			Log($"From: '{e.Source}'");
			Log($"Message (json):");
			Log($"{e.WebMessageAsJson}");
		}
		private void WebView_Unloaded(object sender, RoutedEventArgs e) { }
		private void WebView_TouchUp(object sender, TouchEventArgs e) { }
		private void WebView_TouchMove(object sender, TouchEventArgs e) { }
		private void WebView_TouchLeave(object sender, TouchEventArgs e) { }
		private void WebView_TouchEnter(object sender, TouchEventArgs e) { }
		private void WebView_TouchDown(object sender, TouchEventArgs e) { }
		private void WebView_ToolTipOpening(object sender, ToolTipEventArgs e) { }
		private void WebView_ToolTipClosing(object sender, ToolTipEventArgs e) { }
		private void WebView_TextInput(object sender, TextCompositionEventArgs e) { }
		private void WebView_TargetUpdated(object sender, DataTransferEventArgs e) { }
		private void WebView_StylusUp(object sender, StylusEventArgs e) { }
		private void WebView_StylusSystemGesture(object sender, StylusSystemGestureEventArgs e) { }
		private void WebView_StylusOutOfRange(object sender, StylusEventArgs e) { }
		private void WebView_StylusMove(object sender, StylusEventArgs e) { }
		private void WebView_StylusLeave(object sender, StylusEventArgs e) { }
		private void WebView_StylusInRange(object sender, StylusEventArgs e) { }
		private void WebView_StylusInAirMove(object sender, StylusEventArgs e) { }
		private void WebView_StylusEnter(object sender, StylusEventArgs e) { }
		private void WebView_StylusDown(object sender, StylusDownEventArgs e) { }
		private void WebView_StylusButtonUp(object sender, StylusButtonEventArgs e) { }
		private void WebView_StylusButtonDown(object sender, StylusButtonEventArgs e) { }
		private void WebView_SourceUpdated(object sender, DataTransferEventArgs e) { }
		private void WebView_SourceChanged(object sender, CoreWebView2SourceChangedEventArgs e) { }
		private void WebView_SizeChanged(object sender, SizeChangedEventArgs e) { }
		private void WebView_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e) { }
		private void WebView_QueryCursor(object sender, QueryCursorEventArgs e) { }
		private void WebView_QueryContinueDrag(object sender, QueryContinueDragEventArgs e) { }
		private void WebView_PreviewTouchUp(object sender, TouchEventArgs e) { }
		private void WebView_PreviewTouchMove(object sender, TouchEventArgs e) { }
		private void WebView_PreviewTouchDown(object sender, TouchEventArgs e) { }
		private void WebView_PreviewTextInput(object sender, TextCompositionEventArgs e) { }
		private void WebView_PreviewStylusUp(object sender, StylusEventArgs e) { }
		private void WebView_PreviewStylusSystemGesture(object sender, StylusSystemGestureEventArgs e) { }
		private void WebView_PreviewStylusOutOfRange(object sender, StylusEventArgs e) { }
		private void WebView_PreviewStylusMove(object sender, StylusEventArgs e) { }
		private void WebView_PreviewStylusInRange(object sender, StylusEventArgs e) { }
		private void WebView_PreviewStylusInAirMove(object sender, StylusEventArgs e) { }
		private void WebView_PreviewStylusDown(object sender, StylusDownEventArgs e) { }
		private void WebView_PreviewStylusButtonUp(object sender, StylusButtonEventArgs e) { }
		private void WebView_PreviewStylusButtonDown(object sender, StylusButtonEventArgs e) { }
		private void WebView_PreviewQueryContinueDrag(object sender, QueryContinueDragEventArgs e) { }
		private void WebView_PreviewMouseWheel(object sender, MouseWheelEventArgs e) { }
		private void WebView_PreviewMouseUp(object sender, MouseButtonEventArgs e) { }
		private void WebView_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e) { }
		private void WebView_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e) { }
		private void WebView_PreviewMouseMove(object sender, MouseEventArgs e) { }
		private void WebView_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) { }
		private void WebView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) { }
		private void WebView_PreviewMouseDown(object sender, MouseButtonEventArgs e) { }
		private void WebView_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) { }
		private void WebView_PreviewKeyUp(object sender, KeyEventArgs e) { }
		private void WebView_PreviewKeyDown(object sender, KeyEventArgs e) { }
		private void WebView_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) { }
		private void WebView_PreviewGiveFeedback(object sender, GiveFeedbackEventArgs e) { }
		private void WebView_PreviewDrop(object sender, DragEventArgs e) { }
		private void WebView_PreviewDragOver(object sender, DragEventArgs e) { }
		private void WebView_PreviewDragLeave(object sender, DragEventArgs e) { }
		private void WebView_PreviewDragEnter(object sender, DragEventArgs e) { }
		private void WebView_MouseWheel(object sender, MouseWheelEventArgs e) { }
		private void WebView_MouseUp(object sender, MouseButtonEventArgs e) { }
		private void WebView_MouseRightButtonUp(object sender, MouseButtonEventArgs e) { }
		private void WebView_MouseRightButtonDown(object sender, MouseButtonEventArgs e) { }
		private void WebView_MouseMove(object sender, MouseEventArgs e) { }
		private void WebView_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) { }
		private void WebView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) { }
		private void WebView_MouseLeave(object sender, MouseEventArgs e) { }
		private void WebView_MouseEnter(object sender, MouseEventArgs e) { }
		private void WebView_MouseDown(object sender, MouseButtonEventArgs e) { }
		private void WebView_ManipulationStarting(object sender, ManipulationStartingEventArgs e) { }
		private void WebView_ManipulationStarted(object sender, ManipulationStartedEventArgs e) { }
		private void WebView_ManipulationInertiaStarting(object sender, ManipulationInertiaStartingEventArgs e) { }
		private void WebView_ManipulationDelta(object sender, ManipulationDeltaEventArgs e) { }
		private void WebView_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e) { }
		private void WebView_ManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e) { }
		private void WebView_LostTouchCapture(object sender, TouchEventArgs e) { }
		private void WebView_LostStylusCapture(object sender, StylusEventArgs e) { }
		private void WebView_LostMouseCapture(object sender, MouseEventArgs e) { }
		private void WebView_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) { }
		private void WebView_LostFocus(object sender, RoutedEventArgs e) { }
		private void WebView_Loaded(object sender, RoutedEventArgs e) { }
		private void WebView_LayoutUpdated(object sender, EventArgs e) { }
		private void WebView_KeyUp(object sender, KeyEventArgs e) { }
		private void WebView_KeyDown(object sender, KeyEventArgs e) { }
		private void WebView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) { }
		private void WebView_IsStylusDirectlyOverChanged(object sender, DependencyPropertyChangedEventArgs e) { }
		private void WebView_IsStylusCaptureWithinChanged(object sender, DependencyPropertyChangedEventArgs e) { }
		private void WebView_IsStylusCapturedChanged(object sender, DependencyPropertyChangedEventArgs e) { }
		private void WebView_IsMouseDirectlyOverChanged(object sender, DependencyPropertyChangedEventArgs e) { }
		private void WebView_IsMouseCaptureWithinChanged(object sender, DependencyPropertyChangedEventArgs e) { }
		private void WebView_IsMouseCapturedChanged(object sender, DependencyPropertyChangedEventArgs e) { }
		private void WebView_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e) { }
		private void WebView_IsKeyboardFocusedChanged(object sender, DependencyPropertyChangedEventArgs e) { }
		private void WebView_IsHitTestVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) { }
		private void WebView_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e) { }
		private void WebView_Initialized(object sender, EventArgs e) { }
		private void WebView_GotTouchCapture(object sender, TouchEventArgs e) { }
		private void WebView_GotStylusCapture(object sender, StylusEventArgs e) { }
		private void WebView_GotMouseCapture(object sender, MouseEventArgs e) { }
		private void WebView_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) { }
		private void WebView_GotFocus(object sender, RoutedEventArgs e) { }
		private void WebView_GiveFeedback(object sender, GiveFeedbackEventArgs e) { }
		private void WebView_FocusableChanged(object sender, DependencyPropertyChangedEventArgs e) { }
		private void WebView_Drop(object sender, DragEventArgs e) { }
		private void WebView_DragOver(object sender, DragEventArgs e) { }
		private void WebView_DragLeave(object sender, DragEventArgs e) { }
		private void WebView_DragEnter(object sender, DragEventArgs e) { }
		private void WebView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) { }
		private void WebView_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e) { }
		private void WebView_ContextMenuOpening(object sender, ContextMenuEventArgs e) { }
		private void WebView_ContextMenuClosing(object sender, ContextMenuEventArgs e) { }
		private void WebView_ContentLoading(object sender, CoreWebView2ContentLoadingEventArgs e) { }
	}
}
