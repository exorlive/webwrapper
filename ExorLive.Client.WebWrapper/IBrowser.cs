using System;
using System.Windows;

public interface IBrowser
{
	bool Navigate(Uri url);
	UIElement GetUiElement();
	void SetInterface(object obj);
	event BeforeNavigatingEventHandler BeforeNavigating;
	event EventHandler Navigated;
	event EventHandler IsLoaded;
	event EventHandler IsUnloading;
	event EventHandler SelectedUserChanged;
	event EventHandler ExportUsersDataEvent;

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
	void SelectPersonById(int id);
	void SelectTab(string tab);
	void QueryWorkouts(string query);
	void QueryExercises(string query);
	void NotifyIsLoaded();
	void NotifySelectingUser(int id, string externalId, string firstname, string lastname, string email, string dateofbirth);
	void NotifyIsUnloading();
	bool Debug { get; }
	string ApplicationIdentifier { get; }

	void GetWorkoutsForClient(int userId, DateTime from);
	void OpenWorkout(int id);
}