using System;

public class JsonEventArgs : EventArgs
{
	public readonly string JsonData;
	public JsonEventArgs(string jsondata)
	{
		JsonData = jsondata;
	}
}
