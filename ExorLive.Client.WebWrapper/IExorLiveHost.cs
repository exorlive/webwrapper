﻿using System;
using ExorLive;
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMemberInSuper.Global
public interface IExorLiveHost
{
	event IsLoadedEventHandler IsLoaded;
	event IsUnloadingEventHandler IsUnloading;
	event SelectedUserChangedEventHandler SelectedUserChanged;
	event ExportUsersDataEventHandler ExportUsersDataEvent;
	event ExportUserListEventHandler ExportUserListEvent;
	void SelectPerson(PersonDTO person);
	void SelectPerson2(PersonDTO person);
	void SelectPersonById(int id);
	void SelectTab(string tab);
	void QueryWorkouts(string query);
	void QueryExercises(string query);
	void Restore();
	bool Loaded { get; }

	void GetWorkoutsForClient(int userId, string customId, DateTime from);
	void GetListOfUsers(string customId);
	void OpenWorkout(int id);
}
public delegate void SelectedUserChangedEventHandler(object sender, SelectedUserEventArgs args);
public delegate void IsUnloadingEventHandler(object sender);
public delegate void IsLoadedEventHandler(object sender);
public delegate void ExportUsersDataEventHandler(object sender, JsonEventArgs args);
public delegate void ExportUserListEventHandler(object sender, JsonEventArgs args);
