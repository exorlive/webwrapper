[TOC]

# News
As of May 2017, a new version (v 2.2) is released. Thsi includes some new configuration settings and new functionality, and updated documentation.

# ExorLive WebWrapper

[ExorLive](http://exorlive.com/) is an application delivered as a single page application, and so in order for desktop applications to efficiently integrate with this, ExorLive provides an executable that wraps the web application in a controllable desktop application. This application is called "ExorLive.Client.WebWrapper", or only "WebWrapper". The WebWrapper executable is a single instance application, and if executed multiple times will pass all arguments to the running instance.

## Installation

An MSI installer is available at https://webwrapper.exorlive.com/

This will install to the default path of _"%appdata%\ExorLive\Webwrapper"_ but you can change installation path by running the MSI package using the command line:


```
#!bat

msiexec /i "ExorLiveWebWrapper.2.2.0.0.msi" INSTALLDIR="C:\myfolder" /q
```

Or you can install it as an [advertised installation](https://msdn.microsoft.com/en-us/library/windows/desktop/aa367548(v=vs.85).aspx), which starts the installation with administrator permissions by the user when he runs the shortcut:
```
#!bat

msiexec /jm "ExorLiveWebwrapper.2.2.0.0.msi"
```


## Automatic update

The webwrapper application will attempt to check the website https://webwrapper.exorlive.com/ at startup to see if there is an update. If so, it will show a download link to the user. This check can be disabled by editing the default configuration file (see [Configuration](#markdown-header-configuration)).

## Configuration

The application settings are currently saved in the default configuration file, _ExorLive.Client.WebWrapper.exe.config_, in the application root. The following options is available:

* __BrowserEngine__: Default is **EoWebBrowser**, but this can be changed to **InternetExplorer** if you prefer to run the local Internet Explorer engine.
* __MinimizeOnExit__: Default is **True**. This will make the WebWrapper stay open in the context menu if the user closes it without signing out. This is to prevent the user having to sign in anew.
* __CheckForUpdates__: Default is **True**. This makes the webwrapper check for updates and notify the user if any updates are found.
* __ProtocolProvider__: Default is **ExorLive.Desktop.Arguments.Component, ExorLive.Desktop**. This can be modified to use another API protocol provider. See [WebWrapper API Interface](#markdown-header-webwrapper-api-interface).
* __DistributorName__: This should be set to whichever company built the executable. It will get sent back to ExorLive so we can recognize the app (and maybe provide app-specific updates or options).
* __Debug__: Default is **False**. Shouldn't need to be changed.
* __AppUrl__: Default is **https://exorlive.com/app**. Can be changed to our testing environments or localhost.
* __RememberLoggedInUser__: Default is **True**. See [markdown-header-single-sign-on](#single-sign-on) below.
* __SignonWithWindowsUser__: Default is **False**. See [markdown-header-single-sign-on](#single-sign-on) below.

## Requirements to build

* [WiX Toolset][0], probably with WiX's Visual Studio 2017 extension
* Deployment password for Azure Web App exorlivewebwrapper.scm.azurewebsites.net (https://webwrapper.exorlive.com)
* This file must exist at this location: 
	- "C:\Program Files (x86)\Windows Kits\10\bin\x86\signtool.exe".
* A code signing certificate in your personal certificate store (right click on it and select Install and select location "Current User" and "Personal"). This will be used by signtool to sign the assemblies.

	[0]: http://wixtoolset.org/

## TODO when publishing:

1. Open WebWrapper\Properties\AssemblyInfo.cs and increase assemblyversion and assemblyfileversion.
2. Open SetupProject\Product.wxs and increase the "Version" attribute on the Product tag.
4. Open webwrapper\Webpage\Default.aspx and add info about the release to the changelog.
5. Right-click on ExorLive.Client.WebWrapper and select the "Publish" tab. Change the version.
6. Right-click on Webpage project and select "Publish..."

## Single Sign On

If the WebWrapper is started with an parameter telling which external user is running this session and then this user logges into ExorLive with an ExorLive user, the WebWrapper will remember the link between the external user and the ExorLive user. The next time the WebWrapper is started, it logges in automatically as this ExorLive user.
This functionality is enabled with the `RememberLoggedInUser` setting. For the `ExorLive.Desktop.Arguments` interface, the external user is specified with the `signon`-parameter.

It is also possible to use the current logged in windows user on the computer the WebWrapper is running. It is enabled with the `SignonWithWindowsUser` setting. It behaves the same way as the `RememberLoggedInUser`.

## WebWrapper API Interface

### Architecture

Architecturally the WebWrapper do the following:

* Hosts an instance of the Internet Explorer ActiveX object displaying the Exor Live web application
* Facilitates communication with the web application using a COM-API
* Dynamically loads a protocol provider in order to expose the web application's API to other software.

The dynamic protocol system enables third-parties to easily integrate the WebWrapper with their systems, using their preferred IPC technology (Named Pipes, TCP, etc), by simply creating a .net component implementing a simple interface. This component can subsequently be loaded dynamically based on the optional configuration file or by passing the name of the provider to the executable using `provider="[type]"`, for example `provider="ExorLive.Desktop.Arguments.Component, ExorLive.Desktop"`.

The WebWrapper ships with two built-in protocol providers, one exposing a file based API, and one exposing a command line API.

### ExorLive.Desktop.Arguments

This protocol enables you to repeatedly call the WebWrapper executable with command line arguments in the form of `key=value` or `key="value value"`.

#### Argument specification

* __queryexercises__: If filled performs a query for exercises.
* __queryworkouts__: If filled performs a query for workouts.
* Selecting a contact, or creates the contact if no contact with that CustomId is found.
    * __id__: Required. The foreign key used to identify this person. Gets saved as "CustomId" in ExorLive.
    * __firstname__: Required. The person's firstname.
    * __lastname__: Required. The person's lastname.
    * __dateofbirth__: Required. The person's date of birth. The date will be parsed using either the ISO 8601 defaults (YYYY-MM-DD), failing that it will attempt to parse it using Windows default culture format.
    * __email__: The person's email address.
* __openworkout__: If filled with a workout id, will tell ExorLive to open that workout.
* __signon__: The username/userid of the current user in the calling application.

#### Usage example

```
#!bat

ExorLive.Client.WebWrapper.exe provider="ExorLive.Desktop.Arguments.Component, ExorLive.Desktop" queryexercises="squat" id="user007" firstname="James" lastname="Bond" dateofbirth="1953-04-13"
ExorLive.Client.WebWrapper.exe signon=myuser id="user007" firstname="James" lastname="Bond" dateofbirth="1953-04-13"
```



### ExorLive.Desktop.Exor3

This is a file based protocol compatible with Exercise Organizer's "Exor 3" desktop application. Simply create an xml-file "input.xml" according to the following specification and execute the WebWrapper executable with the path to the xml-file as its first argument.

#### XML specification

__input.xml__
```
#!xml

<?xml version="1.0"?>
<exorinput delete="true" exercisequery="" workoutquery="">
    <source_customerno>231</source_customerno>
    <firstname>Erik</firstname>
    <lastname>Røde</lastname>
    <born>1911-11-23</born>
    <email>erik.viking@mail.com</email>
</exorinput>
```

* __delete__: Wether to delete the xml-file after reading in. Defaults to `true`.
* __exercisequery__: If filled performs a query for exercises.
* __workoutquery__: If filled performs a query for workouts.
* __source_customerno__: Required. The foreign key used to identify this person.
* __firstname__: Required. The person's firstname.
* __lastname__: Required. The person's lastname.
* __born__: Required. The person's date of birth, formatted using YYYY-MM-dd.
* __email__: The person's email address.

#### Usage example

```
#!bat

ExorLive.Client.WebWrapper.exe provider="ExorLive.Desktop.Exor3.Component, ExorLive.Desktop" input.xml
```

### ExorLive.Desktop.Procapita

A tailored version for the Procapita journal system by Tieto in Sweden is available on request.

