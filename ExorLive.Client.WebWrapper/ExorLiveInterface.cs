using System;
using System.Reflection;

public class ExorLiveInterface : IExorLiveInterface
{
	private readonly object _com;

	private readonly Type _comType;
	public void alert(string title, string msg)
	{
		_comType.InvokeMember("alert", BindingFlags.InvokeMethod, null, _com, new object[] {
			title,
			msg
		});
	}

	public void selectPerson(string externalId, string firstname, string lastname, string email, string dateofbirth)
	{
		_comType.InvokeMember("selectPerson", BindingFlags.InvokeMethod, null, _com, new object[] {
			externalId,
			firstname,
			lastname,
			email,
			dateofbirth
		});
	}

	/// <summary>
	/// Select / Create / Update contact in ExorLive
	/// Author: Vilhelm Heiberg
	/// Date: 2013-10-08
	/// </summary>
	/// <param name="externalId">The external identifier.</param>
	/// <param name="firstname">The firstname.</param>
	/// <param name="lastname">The lastname.</param>
	/// <param name="email">The email.</param>
	/// <param name="dateofbirth">The dateofbirth.</param>
	/// <param name="address">The address.</param>
	/// <param name="postalcode">The postalcode.</param>
	/// <param name="place">The place.</param>
	/// <param name="phoneMobile">The phone mobile.</param>
	/// <param name="phoneWork">The phone work.</param>
	/// <param name="gender">1=male, 2=female, 0=unknown</param>
	/// <param name="homepage">The homepage.</param>
	/// <param name="employer">The employer.</param>
	/// <param name="comment">The comment.</param>
	/// <param name="country">The country.</param>
	/// <param name="phonehome">The phonehome.</param>
	public void selectPerson2(
		string externalId,
		string firstname,
		string lastname,
		string email,
		string dateofbirth,
		string address,
		string postalcode,
		string place,
		string phoneMobile,
		string phoneWork,
		int gender,
		string homepage,
		string employer,
		string comment,
		string country,
		string phonehome
	)
	{
		_comType.InvokeMember("selectPerson2", BindingFlags.InvokeMethod, null, _com, new object[] {
			externalId,
			firstname,
			lastname,
			email,
			dateofbirth,
			address,
			postalcode,
			place,
			phoneMobile,
			phoneWork,
			gender,
			homepage,
			employer,
			comment,
			country,
			phonehome
		});
	}

	public void selectPersonById(int id)
	{
		_comType.InvokeMember("selectPersonById", BindingFlags.InvokeMethod, null, _com, new object[] { id });
	}

	public ExorLiveInterface(object com)
	{
		_com = com;
		_comType = com.GetType();
	}

	public void selectTab(string tab)
	{
		_comType.InvokeMember("selectTab", BindingFlags.InvokeMethod, null, _com, new object[] { tab });
	}

	public void queryExercises(string query)
	{
		_comType.InvokeMember("queryExercises", BindingFlags.InvokeMethod, null, _com, new object[] { query });
	}

	public void queryWorkouts(string query)
	{
		_comType.InvokeMember("queryWorkouts", BindingFlags.InvokeMethod, null, _com, new object[] { query });
	}

	public void getWorkoutsForUserId(int userId, DateTime from)
	{
		try
		{
			// Call a Javascript method in ExorLive
			_comType.InvokeMember("getWorkoutsForUserId", BindingFlags.InvokeMethod, null, _com, new object[] { userId, from });
		} catch(Exception)
		{
			// Ignore any error in ExorLive. Just to make WebWrapper don't crash in case of a problem in ExorLive.
		}
	}

}
