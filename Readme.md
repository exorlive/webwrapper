# ExorLive WebWrapper

[ExorLive](http://exorlive.com/) is an application delivered as a single page
application, and so in order for desktop applications to efficiently integrate
with this, ExorLive provides an executable that wraps the web application in a
controllable desktop application. This application is called
"ExorLive.Client.WebWrapper", or only "WebWrapper". The WebWrapper executable is
a single instance application, and if executed multiple times will pass all
arguments to the running instance.

## Installation

An MSI installer is available at <webwrapper.exorlive.com>. It will install to
the default path of `%appdata%\ExorLive\Webwrapper` but you can change
installation path by running the MSI package using the command line:

```bat
#!bat

msiexec /i "ExorLiveWebWrapper.2.6.0.0.msi" INSTALLDIR="C:\myfolder" /q
```

Or you can install it as an
[advertised installation](<https://msdn.microsoft.com/en-us/library/windows/desktop/aa367548(v=vs.85).aspx>),
which starts the installation with administrator permissions by the user when he
runs the shortcut:

```bat
#!bat

msiexec /jm "ExorLiveWebwrapper.2.6.0.0.msi"
```

### Package an installation for other purposes

It is possible to package the installation for other purposes, ex. by storing it
on a CD, or for distribution on a network. You can do this by finding the folder
where the program is installed and packaging the content of that folder. You
have to create the installation package and create the shortcut to the program
yourself.

## Automatic update

The webwrapper application will attempt to check the website
https://webwrapper.exorlive.com/ at startup to see if there is an update. If so,
it will show a download link to the user. This check can be disabled by editing
the default configuration file (see [Configuration](#configuration)).

## Configuration

The application settings are currently saved in the default configuration file,
`ExorLive.Client.WebWrapper.exe.config`, in the application root. Once a user
have launched the application, a second per-user configuration file named
"user.config" is created in a subfolder of `%localappdata%/ExorLive/`.

### User config

Found in a subfolder of `%localappdata%/ExorLive/`. If a value is blank, it
copies the value from the global config file.

- **BrowserEngine**: Default is `EoWebBrowser`, but this can be changed to
  `InternetExplorer` if you prefer to run the local Internet Explorer browser
  engine, or `WebViewBrowser` if you prefer to run on the new Microsoft Edge
  browser engine.
- **CheckForUpdates**: Default is `True`. If the application should check for
  updates on launch, and show a banner notice if there is available updates.
- **Top** / **Left** / **Height** / **Width** / **Maximized**: The webwrapper's
  window size and location on screen, saved for next time.
- **MinimizeOnExit**: Default is `True`. This will make the WebWrapper stay open
  in the context menu if the user closes it without signing out. This is to
  prevent the user having to sign in anew.
- **UpdatePath**: Default is blank. The current update channel.
- **ZoomFactor**: The current zoom level in the browser window.

### Application config

The configuration file in the application root (usually
`"%appdata%\ExorLive\ExorLive Webwrapper\ExorLive.Client.WebWrapper.exe.config"`).
These settings might be overriden by the user's config file (above).

- **AdfsUrl**: Default is blank. For ADFS login systems.
- **AppUrl**: Default is `https:\\auth.exorlive.com\signin\`. This is the
  initial url that the webwrapper launches.
- **Culture**: Default is blank. Overrides the default language for login.
- **Debug**: Default is `False`. If set to `True`, it will write logfiles to a
  folder in _%temp%_. If you are using EoBrowser, it will enable a chrome
  debugging server on port 9223.
- **ProtocolProvider**: Default is
  `ExorLive.Desktop.Arguments.Component, ExorLive.Desktop`. This can be modified
  to use another API protocol provider. See
  [WebWrapper API Interface](#api-interface).
- **RememberLoggedInUser**: Default is **True**. See
  [Single-Sign-On](#single-sign-on) below.
- **SignonWithWindowsUser**: Default is **False**. See
  [Single-Sign-On](#single-sign-on) below.

## Requirements to build and publish

- [WiX Toolset][0], probably with WiX's Visual Studio 2017 extension
- Deployment password for Azure Web App
  "exorlivewebwrapper.scm.azurewebsites.net"
  (<https://webwrapper.exorlive.com>).
- A code signing certificate in your personal certificate store (right click on
  it and select Install and select location "Current User" and "Personal"). This
  will be used by signtool to sign the assemblies.

  [0]: http://wixtoolset.org/

## TODO when publishing

1. Open WebWrapper\Properties\AssemblyInfo.cs and increase assemblyversion.
2. Open SetupProject\Product.wxs and increase the "Version" attribute on the
   Product tag.
3. Open webwrapper\Webpage\Default.aspx and add info about the release to the
   changelog.
4. Right-click on ExorLive.Client.WebWrapper and select the "Publish" tab.
   Change the version.
5. Right-click on Webpage project and select "Publish..."

This will build the SetupProject, ExorLive.Client.WebWrapper, and
ExorLive.Desktop. It will build it 3 times, each with a different configuration:
Release, Procapita, Goteborg, Boras and VGR.

## Single Sign On

If the WebWrapper is started with a parameter telling which external user is
running this session and then this user logges into ExorLive with an ExorLive
user, the WebWrapper will remember the link between the external user and the
ExorLive user. The next time the WebWrapper is started, it logges in
automatically as this ExorLive user. This functionality is enabled with the
`RememberLoggedInUser` setting. For the `ExorLive.Desktop.Arguments` interface,
the external user is specified with the `signon` parameter.

It is also possible to use the current logged in windows user on the computer
the WebWrapper is running. It is enabled with the `SignonWithWindowsUser`
setting. It behaves the same way as the `RememberLoggedInUser`.

## WebWrapper API Interface

### Architecture

Architecturally the WebWrapper do the following:

- Hosts an instance of the Internet Explorer ActiveX object displaying the Exor
  Live web application
- Facilitates communication with the web application using a COM-API
- Dynamically loads a protocol provider in order to expose the web application's
  API to other software.

The dynamic protocol system enables third-parties to easily integrate the
WebWrapper with their systems, using their preferred IPC technology (Named
Pipes, TCP, etc), by simply creating a .net component implementing a simple
interface. This component can subsequently be loaded dynamically based on the
optional configuration file or by passing the name of the provider to the
executable using `provider="[type]"`, for example
`provider="ExorLive.Desktop.Arguments.Component, ExorLive.Desktop"`.

The WebWrapper ships with two built-in protocol providers, one exposing a file
based API, and one exposing a command line API.

### ExorLive.Desktop.Arguments

This protocol enables you to repeatedly call the WebWrapper executable with
command line arguments in the form of `key=value` or `key="value value"`.

#### Argument specification

- **queryexercises**: If filled performs a query for exercises.
- **queryworkouts**: If filled performs a query for workouts.
- Selecting a contact, or creates the contact if no contact with that CustomId
  is found.
  - **id**: Required. The foreign key used to identify this person. Gets saved
    as "CustomId" in ExorLive.
  - **firstname**: Required. The person's firstname.
  - **lastname**: Required. The person's lastname.
  - **dateofbirth**: Required. The person's date of birth. The date will be
    parsed using either the ISO 8601 defaults (YYYY-MM-DD), failing that it will
    attempt to parse it using Windows default culture format.
  - **email**: The person's email address.
- **openworkout**: If filled with a workout id, will tell ExorLive to open that
  workout.
- **signon**: The username/userid of the current user in the calling
  application.

#### Usage example

```bat
#!bat

ExorLive.Client.WebWrapper.exe provider="ExorLive.Desktop.Arguments.Component, ExorLive.Desktop" queryexercises="squat" id="user007" firstname="James" lastname="Bond" dateofbirth="1953-04-13"
ExorLive.Client.WebWrapper.exe signon=myuser id="user007" firstname="James" lastname="Bond" dateofbirth="1953-04-13"
```

### ExorLive.Desktop.Exor3

This is a file based protocol compatible with Exercise Organizer's "Exor 3"
desktop application. Simply create an xml-file "input.xml" according to the
following specification and execute the WebWrapper executable with the path to
the xml-file as its first argument.

#### XML specification

**input.xml**

```xml
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

- **delete**: Wether to delete the xml-file after reading in. Defaults to
  `true`.
- **exercisequery**: If filled performs a query for exercises.
- **workoutquery**: If filled performs a query for workouts.
- **source_customerno**: Required. The foreign key used to identify this person.
- **firstname**: Required. The person's firstname.
- **lastname**: Required. The person's lastname.
- **born**: Required. The person's date of birth, formatted using YYYY-MM-dd.
- **email**: The person's email address.

#### Usage example

```bat
#!bat

ExorLive.Client.WebWrapper.exe provider="ExorLive.Desktop.Exor3.Component, ExorLive.Desktop" input.xml
```

### ExorLive.Desktop.Procapita

A tailored version for the Procapita journal system by Tieto in Sweden is
available on request.
