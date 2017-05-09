Const strComputer = "." 
 Dim objWMIService, colProcessList
 Set objWMIService = GetObject("winmgmts:" & "{impersonationLevel=impersonate}!\\" & strComputer & "\root\cimv2")
 Set colProcessList = objWMIService.ExecQuery("SELECT * FROM Win32_Process WHERE Name = 'ExorLive.Client.WebWrapper.exe'")
 For Each objProcess in colProcessList 
 objProcess.Terminate() 
 Next 