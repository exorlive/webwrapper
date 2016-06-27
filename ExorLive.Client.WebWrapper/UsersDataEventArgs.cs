using System;

public class UsersDataEventArgs : EventArgs
{
	public readonly string JsonData;
	public UsersDataEventArgs(string jsondata)
	{
		JsonData = jsondata;
	}
}
