using System.Configuration;

namespace ExorLive.Properties
{
	/// <summary>
	/// This class allows you to handle specific events on the settings class:
	///  The SettingChanging event is raised before a setting's value is changed.
	///  The PropertyChanged event is raised after a setting's value is changed.
	///  The SettingsLoaded event is raised after the setting values are loaded.
	///  The SettingsSaving event is raised before the setting values are saved.
	/// </summary>
	public sealed partial class Settings
	{
		[UserScopedSetting()]
		public string OsloSettings
		{
			get => ((string)this[nameof(OsloSettings)]);
			set => this[nameof(OsloSettings)] = value;
		}

	}
}
