﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Webpage.Boras.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<meta charset="utf-8" />
	<title>ExorLive WebWrapper for Boras</title>
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
					<h1>ExorLive WebWrapper <%=GetLatestVersion()%> for Boras</h1>
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
		</div>
	</div>
</body>
</html>
