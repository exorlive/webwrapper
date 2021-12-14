using System.Runtime.InteropServices;

namespace WebWrapper
{
	[ClassInterface(ClassInterfaceType.AutoDual)]
	[ComVisible(true)]
	internal class BrowserObject
	{
		private WebViewBrowser eoBrowser;
		public BrowserObject(WebViewBrowser eoBrowser)
		{
			this.eoBrowser = eoBrowser;
		}
		public void SetInterface(object jsobject) => eoBrowser.SetInterface(jsobject);
		public void NotifyIsLoaded() => eoBrowser.NotifyIsLoaded();
		public void NotifySelectingUser(int id, string externalId, string firstname, string lastname, string email, string dateofbirth) => eoBrowser.NotifySelectingUser(id, externalId, firstname, lastname, email, dateofbirth);
		public void NotifyIsUnloading() => eoBrowser.NotifyIsUnloading();
		public void ExportUsersData(string arg) => eoBrowser.ExportUsersData(arg);
		public void ExportUserList(string customId) => eoBrowser.ExportUserList(customId);
		public void SelectPersonResult(string jsonresult) => eoBrowser.SelectPersonResult(jsonresult);
		public void ExportSignonDetails(string arg) => eoBrowser.ExportSignonDetails(arg);
	}
}
