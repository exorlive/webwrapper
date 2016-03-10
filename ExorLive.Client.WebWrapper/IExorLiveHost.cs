using ExorLive;
public interface IExorLiveHost
{
	event IsLoadedEventHandler IsLoaded;
	event IsUnloadingEventHandler IsUnloading;
	event SelectedUserChangedEventHandler SelectedUserChanged;
	void SelectPerson(PersonDTO person);
	void SelectPerson2(PersonDTO person);
	void SelectPersonById(int id);
	void SelectTab(string tab);
	void QueryWorkouts(string query);
	void QueryExercises(string query);
	void Restore();
	bool Loaded { get; }
}
public delegate void SelectedUserChangedEventHandler(object sender, SelectedUserEventArgs args);
public delegate void IsUnloadingEventHandler(object sender);
public delegate void IsLoadedEventHandler(object sender);
