Imports System.Globalization

Namespace Desktop.Arguments
	Public Class Component
		Implements IHosted
		Private WithEvents _host As IHost
		Public Function GetName() As String Implements IHosted.GetName
			Return "Arguments API"
		End Function

		''' <summary>
		''' Read command line and process anything that should happen before user sign in to ExorLive.
		''' Used to tell which user may do automatic signin.
		''' </summary>
		''' <param name="args"></param>
		''' <returns></returns>
		Public Function GetSignonUser(args() As String) As String Implements IHosted.GetSignonUser
			'' Pick up the user
			Dim map As New Dictionary(Of String, String)
			Dim atoms() As String
			For Each arg As String In args
				atoms = arg.Split("="c)
				If atoms.Length = 2 Then map.Add(atoms(0), atoms(1))
			Next
			If map.ContainsKey("signon") Then Return map("signon")
			Return Nothing
		End Function

		Public Sub ReadCommandline(args() As String) Implements IHosted.ReadCommandline
			Dim map As New Dictionary(Of String, String)
			Dim atoms() As String
			Dim splitchar As Char() = {"="c}
			For Each arg As String In args
				atoms = arg.Split(splitchar, 2, StringSplitOptions.None)
				If atoms.Length = 2 Then map.Add(atoms(0), atoms(1))
			Next
			Dim dto As New PersonDTO
			If map.ContainsKey("firstname") And map.ContainsKey("lastname") And map.ContainsKey("id") Then
				dto.ExternalId = map("id")
				dto.Firstname = map("firstname").Replace("""", "")
				dto.Lastname = map("lastname").Replace("""", "")
				If map.ContainsKey("dateofbirth") Then
					Dim parsedDate As DateTime
					If Date.TryParseExact(map("dateofbirth"), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, parsedDate) Then
						dto.DateOfBirth = parsedDate.ToString("yyyy-MM-dd")
					Else
						' I would prefer people to use ISO 8601, but let the OS try to parse it instead:
						If Date.TryParse(map("dateofbirth"), parsedDate) Then
							' And THEN i transform it back to a ISO 8601 valid date string:
							dto.DateOfBirth = parsedDate.ToString("yyyy-MM-dd")
						End If
					End If
				End If
				If map.ContainsKey("email") Then dto.Email = map("email")
				If map.ContainsKey("address") Then dto.Address = map("address")
				If map.ContainsKey("phonehome") Then dto.PhoneHome = map("phonehome")
				If map.ContainsKey("phonework") Then dto.PhoneWork = map("phonework")
				If map.ContainsKey("mobile") Then dto.Mobile = map("mobile")
				If map.ContainsKey("country") Then dto.Country = map("country")
				If map.ContainsKey("zipcode") Then dto.ZipCode = map("zipcode")
				If map.ContainsKey("location") Then dto.Location = map("location")
				If map.ContainsKey("caseid") Then dto.CaseId = map("caseid")
				_host.SelectPerson(dto)
			End If
			If map.ContainsKey("tab") Then _host.SelectTab(map("tab"))
			If map.ContainsKey("queryworkouts") Then _host.QueryWorkouts(map("queryworkouts").Replace("""", ""))
			If map.ContainsKey("queryexercises") Then _host.QueryExercises(map("queryexercises").Replace("""", ""))
			If map.ContainsKey("openworkout") Then _host.OpenWorkout(map("openworkout").Replace("""", ""))

			' Send a notification to ExorLive about who signed in using WebWrapper, even when signon is not specified.
			' The 'signon' parameter it also picked up and handled in GetSignonUser().
			If map.ContainsKey("signon") Then _host.RegisterWebwrapperSignon(map("signon")) Else _host.RegisterWebwrapperSignon("")
		End Sub

		Public Sub Initialize(host As IHost, currentDirectory As String) Implements IHosted.Initialize
			_host = host
		End Sub

		Private Sub _host_WindowMinified() Handles _host.WindowMinified
		End Sub
	End Class
End Namespace
