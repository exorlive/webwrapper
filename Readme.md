# ExorLive WebWrapper

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
4. Right-click on Webpage project and select "Publish..."