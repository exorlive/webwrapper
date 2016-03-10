using System;
using ExorLive;

public class SelectedUserEventArgs: EventArgs
{
	public readonly PersonDTO Person;
	public SelectedUserEventArgs(PersonDTO person)
	{
		Person = person;
	}
}