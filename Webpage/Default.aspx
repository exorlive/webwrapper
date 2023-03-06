<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Webpage.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<meta charset="utf-8" />
	<title>ExorLive WebWrapper</title>
	<meta name="viewport" content="width=device-width, initial-scale=1, maximum-scale=1, user-scalable=no" />
	<link href="https://fonts.googleapis.com/css?family=Roboto:400,500,300" rel="stylesheet" type="text/css" />
	<link href="/Default.css" rel="stylesheet" />
	<script type="text/javascript" src="https://exorlive.atlassian.net/s/e94195f9bae6455fdafc3431f4491284-T/en_USlkp0ac/72000/0aaf53e48b3d250893b9f31948cf7b09/2.0.13/_/download/batch/com.atlassian.jira.collector.plugin.jira-issue-collector-plugin:issuecollector/com.atlassian.jira.collector.plugin.jira-issue-collector-plugin:issuecollector.js?locale=en-US&collectorId=e7536f17"></script>
	<script type="text/javascript">
		window.ATL_JQ_PAGE_PROPS = {
			'triggerFunction': function (showCollectorDialog) {
				document.getElementById('logedUser').onclick = function (e) {
					e.preventDefault();
					showCollectorDialog();
				};
			}
		};
	</script>
</head>
<body>
	<header id="pageHeader">
		<nav class="navbar" role="navigation">
			<div class="navbar-header">
				<a class="navbar-brand" href="/" title="Homepage" accesskey="2">
					<img src="/images/exorlive_logo.svg" alt="ExorLive" width="127" height="36" />
				</a>
			</div>
			<ul class="nav navbar-nav" id="menu">
				<li class="active">
					<h1>ExorLive WebWrapper <%=GetLatestVersion()%></h1>
				</li>
			</ul>
		</nav>
	</header>
	<div id="main">
		<div id="mainPage" class="container">
			<h2>Download</h2>
			<ul>
				<li><a href="<%=GetLatestDownload()%>">Latest version</a></li>
			</ul>

			<h2>Changelog</h2>
			<h3>2.6 - March 2023</h3>
			<ul>
				<li>Add a config option for default language.</li>
			</ul>
			<h3>2.5 - November 2022</h3>
			<ul>
				<li>Add a background call to ExorLive with the signon-parameter.</li>
			</ul>
			<h3>2.4 - September 2022</h3>
			<ul>
				<li>Support for WebView2/Edge browser: "WebViewBrowser"</li>
				<li>New parameter in commandline-argument api on user selection: "caseid=".</li>
				<li>Updated EoBrowser.</li>
				<li>Updated minimum requirements to .Net 4.6</li>
			</ul>
			<h3>2.2.9 - May 2021</h3>
			<ul>
				<li>Fixed a bug with the API integration.</li>
			</ul>
			<h3>2.2.8 - April 2021</h3>
			<ul>
				<li>Upgraded the built-in browser (EoBrowser) to latest version.</li>
			</ul>
			<h3>2.2.7 - Mai 2020</h3>
			<ul>
				<li>Set website language to same as user language.</li>
				<li>New zoom feature.</li>
			</ul>
			<h3>2.2.6 - November 2018</h3>
			<ul>
				<li>Fix for "Runtime Exception" on certain webpage errors.</li>
				<li>Fix for application hanging at launch caused by anti-virus or security measures blocking the browser thread.</li>
			</ul>
			<h3>2.2.5 - September 2018</h3>
			<ul>
				<li>
					<p>Fix for video playback issues:</p>
					<ul>
						<li>Updated browser engine.</li>
						<li>Turned off hardware acceleration in the browser component.</li>
					</ul>
				</li>
			</ul>
			<h3>2.2.3 - September 2017</h3>
			<ul>
				<li>Support for ADFS.</li>
				<li>Fixes Webwrapper error on ExorLive print.</li>
				<li>Fixes CertificateError dialog that occasionally was shown on application launch.</li>
			</ul>
			<h3>2.2 - May 2017</h3>
			<ul>
				<li>This includes some new configuration settings and new functionality.</li>
			</ul>
			<h3>2.1</h3>
			<ul>
				<li>Fix for terminal server issues with named pipes.</li>
				<li>Added named pipe version query</li>
			</ul>
			<h3>2.0.3</h3>
			<ul>
				<li>Added namedpipe listeners as an API interface.</li>
				<li>New API command: openWorkout, which opens a workout directly instead of going to the search box.</li>
				<li>The window remembers its size after closing.</li>
			</ul>
			<h3>2.0.1 - March 2016</h3>
			<ul>
				<li>Rebuilt the webwrapper with a built-in web-browser to get around Internet Explorer specific errors.</li>
			</ul>
		</div>
	</div>
</body>
</html>
