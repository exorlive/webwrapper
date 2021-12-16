using System;
using ExorLive;

namespace ExorLive.WebWrapper.Interface
{
	public class SelectedUserEventArgs : EventArgs
	{
		public readonly PersonDTO Person;
		public SelectedUserEventArgs(PersonDTO person)
		{
			Person = person;
		}
	}
}