using EO.WebBrowser;

/// <summary>
/// Calls from javascript.
/// These methods will be available in the window.external object.
/// </summary>
class EoBrowserObject
{
	private EoBrowser eoBrowser;
	public EoBrowserObject(EoBrowser eoBrowser)
	{
		this.eoBrowser = eoBrowser;
	}
	public void SetInterface(JSObject jsobject)
	{
		eoBrowser.SetInterface(jsobject);
	}
	public void NotifyIsLoaded()
	{
		eoBrowser.NotifyIsLoaded();
	}
	public void NotifySelectingUser(int id, string externalId, string firstname, string lastname, string email, string dateofbirth)
	{
		eoBrowser.NotifySelectingUser(id, externalId, firstname, lastname, email, dateofbirth);
	}
	public void NotifyIsUnloading()
	{
		eoBrowser.NotifyIsUnloading();
	}
	public void ExportUsersData(string arg)
	{
		eoBrowser.ExportUsersData(arg);
	}
	public void ExportUserList(string customId)
	{
		eoBrowser.ExportUserList(customId);
	}
	public void SelectPersonResult(string jsonresult)
	{
		eoBrowser.SelectPersonResult(jsonresult);
	}
	public void ExportSignonDetails(string arg)
	{
		eoBrowser.ExportSignonDetails(arg);
	}
}