using System;
using System.Threading.Tasks;
using System.Windows;

namespace ExorLive.WebWrapper.Interface
{
	public interface IBrowser
	{
		bool Navigate(Uri url);
		UIElement GetUiElement();
		Task InitializeAsync();
		void SetInterface(object obj);
		event BeforeNavigatingEventHandler BeforeNavigating;
		event EventHandler Navigated;
		event EventHandler IsLoaded;
		event EventHandler IsUnloading;
		event EventHandler SelectedUserChanged;
		event EventHandler ExportUsersDataEvent;
		event EventHandler ExportUserListEvent;
		event EventHandler SelectPersonResultEvent;
		event EventHandler ExportSignonDetailsEvent;
		event EventHandler ZoomFactorChanged;

		void SelectPerson(
			string externalId,
			string firstname,
			string lastname,
			string email,
			string dateOfBirth
		);
		void SelectPerson2(
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
		);
		void SelectPerson3(
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
		);
		void SelectPersonById(int id);
		void SelectTab(string tab);
		void QueryWorkouts(string query);
		void QueryExercises(string query);
		void NotifyIsLoaded();
		void NotifySelectingUser(int id, string externalId, string firstname, string lastname, string email, string dateofbirth);
		void NotifyIsUnloading();
		void GetWorkoutsForClient(int userId, string customId, DateTime from);
		void GetListOfUsers(string customId);
		void OpenWorkout(int id);
		void GetSignonDetails();

		/// <summary>
		/// Retrieve the browsers current zoom level.
		/// </summary>
		/// <returns>The current zoom level.</returns>
		/// <exception cref="NotSupportedException">Should throw a NotImplementedException if the browser doesnt support zoom.</exception>
		decimal GetZoomFactor();

		/// <summary>
		/// Check if the browser supports zooming.
		/// </summary>
		/// <returns>False if browser does not support zooming.</returns>
		bool SupportsZoom();

		/// <summary>
		/// Sets the current zoom level in the browser.
		/// </summary>
		/// <param name="v">A value where 1 equals 100%.</param>
		/// <remarks>Remember to check if the browser supports zoom before setting this.</remarks>
		/// <exception cref="NotSupportedException">Should throw a NotSupportedException if the browser doesnt support zoom.</exception>
		void SetZoomFactor(decimal v);
	}
}