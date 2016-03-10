window.external = {
	SetInterface: function () { window.eoWebBrowser.extInvoke('SetInterface', arguments); },
	NotifyIsLoaded: function () { window.eoWebBrowser.extInvoke('NotifyIsLoaded', arguments); },
	NotifySelectingUser: function () { window.eoWebBrowser.extInvoke('notifySelectingUser', arguments); },
	NotifyIsUnloading: function () { window.eoWebBrowser.extInvoke('notifyIsUnloading', arguments); },
	Debug: false
};