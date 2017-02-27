Imports System.Globalization

Namespace Desktop.Procapita
    ' ReSharper disable once UnusedMember.Global
    Public Class Component
        Implements IHosted
        Private WithEvents _host As IHost
		Public Function GetName() As String Implements IHosted.GetName
			Return "Procapita API"
		End Function

		'''' Here is Procapita documentation:
		''''''''''''''''''''''''''''''''''''''
		''
		'' Uppstart av träningsprogram från Procapita VoO
		'' Procapita är en desktopapplikation som anropar en .exe-fil med ett visst antal startparametrar.
		'' Argument/ parametrar:
		'' - Personnummer
		'' - Användarnamn
		'' - Förnamn
		'' - Efternamn
		'' - Konstant Data 1
		'' - Konstant Data 2
		''

		''' <summary>
		''' Read command line and process anything that should happen before user sign in to ExorLive.
		''' Used to tell which user may do automatic signin.
		''' </summary>
		''' <param name="args"></param>
		''' <returns></returns>
		Public Function GetSignonUser(args() As String) As String Implements IHosted.GetSignonUser
			'' Pick up the user
			If args.Length > 1 Then Return args(1)
			Return Nothing
		End Function

		Public Sub ReadCommandline(args() As String) Implements IHosted.ReadCommandline
			Dim signon As String = Nothing
			Dim arg5 As String = Nothing
			Dim arg6 As String = Nothing
			Dim dto As New PersonDTO
			If args.Length > 0 Then dto.ExternalId = args(0)
			If args.Length > 1 Then signon = args(1)
			If args.Length > 2 Then dto.Firstname = args(2)
			If args.Length > 3 Then dto.Lastname = args(3)
			If args.Length > 4 Then arg5 = args(4)
			If args.Length > 5 Then arg6 = args(5)

			' Parameter 5 and 6 are ignored.
			' Parameter 2 (signon) is ignored here. It is picked up in GetSignonUser() instead.

			_host.SelectPerson(dto)
		End Sub

		Public Sub Initialize(host As IHost, currentDirectory As String) Implements IHosted.Initialize
            _host = host
        End Sub

		Private Sub _host_WindowMinified() Handles _host.WindowMinified
		End Sub

	End Class
End Namespace