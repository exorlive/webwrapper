# Building

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

# ExorLive WebWrapper

[ExorLive](http://exorlive.com/) is an application delivered as a single page
application, and so in order for desktop applications to efficiently integrate
with this, ExorLive provides an executable that wraps the web application in a
controllable desktop application. This application is called
"ExorLive.Client.WebWrapper", or only "WebWrapper". The WebWrapper executable is
a single instance application, and if executed multiple times will pass all
arguments to the running instance.

User documentation is available at
<a href="https://developer.exorlive.com/api/webwrapper.aspx">developer.exorlive.com/api/webwrapper.aspx</a>.
