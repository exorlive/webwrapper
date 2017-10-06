using System;
using System.IO;

namespace Webpage.Procapita {
	public partial class Default : System.Web.UI.Page {
		protected void Page_Load(object sender, EventArgs e) {

		}

		protected string GetLatestDownload() {
			var version = GetLatestVersion();
			return $"./msi/ExorLiveWebWrapper.{version}.msi";
		}

		protected string GetLatestVersion() {
			return File.ReadAllText(Server.MapPath("msi/version.txt"));
		}
	}
}