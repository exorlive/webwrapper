using System;
using System.Net;

namespace WebWrapper
{
	/// <summary>
	/// Below is an inheritance WebClient class which resolve a lot of general problems like this...
	/// https://stackoverflow.com/a/56015894
	/// </summary>
	public class TlsWebClient : WebClient
	{
		public TlsWebClient()
		{
			ServicePointManager.Expect100Continue = true;
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
			ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
			container = new CookieContainer();
		}
		public TlsWebClient(CookieContainer container)
		{
			this.container = container;
		}

		public CookieContainer CookieContainer {
			get => container;
			set => container = value;
		}

		private CookieContainer container = new CookieContainer();

		protected override WebRequest GetWebRequest(Uri address)
		{
			var r = base.GetWebRequest(address);
			if (r is HttpWebRequest request)
			{
				request.CookieContainer = container;
			}
			return r;
		}

		protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
		{
			var response = base.GetWebResponse(request, result);
			ReadCookies(response);
			return response;
		}

		protected override WebResponse GetWebResponse(WebRequest request)
		{
			var response = base.GetWebResponse(request);
			ReadCookies(response);
			return response;
		}

		private void ReadCookies(WebResponse r)
		{
			if (r is HttpWebResponse response)
			{
				var cookies = response.Cookies;
				container.Add(cookies);
			}
		}
	}
}
