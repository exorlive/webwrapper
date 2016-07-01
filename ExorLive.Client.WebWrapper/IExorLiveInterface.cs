using System;
public interface IExorLiveInterface
{
	void alert(string title, string message);
	void selectPersonById(int id);

	void selectPerson(
		string externalId,
		string firstname,
		string lastname,
		string email,
		string dateofbirth);
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
	void selectPerson2(
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
	);
	void selectTab(string tab);
	void queryWorkouts(string query);
	void queryExercises(string query);
	void getWorkoutsForCustomId(string customId, DateTime from);
	void openWorkout(int id);
}
