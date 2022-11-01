using System;
public interface IExorLiveInterface
{
#pragma warning disable IDE1006 // Naming Styles
	void alert(string title, string message);

	void selectPersonById(int id);

	void registerWebwrapperSignon(string signon);

	void selectPerson(
		string externalId,
		string firstname,
		string lastname,
		string email,
		string dateofbirth,
		string caseid
	);
	/// <summary>
	/// Select / Create / Update contact in ExorLive
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
		string phonehome,
		string caseid
	);
	void selectPerson3(
		int userId,
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
		string phonehome,
		string profiledata,
		string source,
		string caseid
	);
	void selectTab(string tab);
	void queryWorkouts(string query);
	void queryExercises(string query);
	void getWorkoutsForCustomId(int userId, string customId, DateTime from);
	void getListOfUsers(string customId);
	void openWorkout(int id);
	void getOsloSignonDetails();
#pragma warning restore IDE1006 // Naming Styles
}
