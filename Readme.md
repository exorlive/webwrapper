[TOC]

# ExorLive WebWrapper

ExorLive is an application delivered as a single page application, and so in order for desktop applications to efficiently integrate with this, ExorLive provides an executable that wraps the web application in a controllable desktop application. This application is called "ExorLive.Client.WebWrapper", or only "WebWrapper". The WebWrapper executable is a single instance application, and if executed multiple times will pass all arguments to the running instance.

## Requirements to build

* InstallShield 2015 Limited Edition 
* License code for InstallShield 2015 LE
* Deployment password for Azure Web App exorlivewebwrapper.scm.azurewebsites.net (https://webwrapper.exorlive.com)
* Code signing certificate.
	* This file must exist: "C:\Program Files (x86)\Windows Kits\10\bin\x86\signtool.exe".
	* Install the code signing certificate "exorlive.pfx" into your personal certificate store (right click on it and select Install and select location "Current User" and "Personal"). This wiill be used by signtool to sign the assemblies.
	* Add the file "exorlive.pfx" into root of the solution folder. This is for InstallShields MSI-build.

## TODO when publishing:

1. Open WebWrapper\Properties\AssemblyInfo.cs and increase assemblyversion and assemblyfileversion.
2. Open InstallShield\Organize Your Setup\GeneralInformation and increase "Product Version".
3. Open InstallShield\Organize Your Setup\GeneralInformation and click on the button to the right of "Product Code", which creates a new product code.
4. Right-click on ExorLive.Client.WebWrapper and select the "Publish" tab. Change the version.
5. Right-click on Webpage project and select "Publish..."

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
    * __firstname__: Required. The persons firstname.
    * __lastname__: Required. The persons lastname.
    * __dateofbirth__: Required. The persons date of birth. The date will be parsed using either the ISO 8601 defaults (YYYY-MM-DD), failing that it will attempt to parse it using Windows default culture format.
    * __email__: The persons email address.

#### Usage example

```
#!cmd

ExorLive.Client.WebWrapper.exe provider="ExorLive.Desktop.Arguments.Component, ExorLive.Desktop" queryexercises="squat" id="user007" firstname="James" lastname="Bond" dateofbirth="1953-04-13"
```

### ExorLive.Desktop.Exor3

This is a file based protocol compatible with Exercise Organizer's "Exor 3" deskop application. Simply create an xml-file "input.xml" according to the following specification and execute the WebWrapper executable with the path to the xml-file as its first argument.

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
* __firstname__: Required. The persons firstname.
* __lastname__: Required. The persons lastname.
* __born__: Required. The persons date of birth, formatted using YYYY-MM-dd.
* __email__: The persons email address.

#### Usage example

```
#!cmd

ExorLive.Client.WebWrapper.exe provider="ExorLive.Desktop.Exor3.Component, ExorLive.Desktop" input.xml
```