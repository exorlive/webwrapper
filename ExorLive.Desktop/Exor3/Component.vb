Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Runtime.Serialization.Formatters
Imports System.Xml
Imports System.Xml.XPath

Namespace Desktop.Exor3
	' ReSharper disable once UnusedMember.Global
	Public Class Component
		Inherits Desktop.Component
		Implements IHosted
		Private WithEvents _host As IHost
		Private _currentDirectory As String
		Public Function GetName() As String Implements IHosted.GetName
			Return "Exor3 XML bridge"
		End Function

		''' <summary>
		''' Read command line and process anything that should happen before user sign in to ExorLive.
		''' Used to tell which user may do automatic signin.
		''' </summary>
		''' <param name="args"></param>
		''' <returns></returns>
		Public Function GetSignonUser(args() As String) As String Implements IHosted.GetSignonUser
			Return Nothing
		End Function

		Public Sub ReadCommandline(args() As String) Implements IHosted.ReadCommandline
			If args.Length > 0 Then
				Dim filepatharg = (From arg In args
								   Where arg.IndexOf(".xml", StringComparison.Ordinal) <> -1
								   Where ValidateFilePath(arg) = True).FirstOrDefault()

				If Not String.IsNullOrEmpty(filepatharg) Then
					Dim path As String = filepatharg
					If Not IO.Path.IsPathRooted(path) Then
						path = IO.Path.Combine(_currentDirectory, path)
					End If
					If File.Exists(path) Then
						Try
							Dim doc As New XPathDocument(path)
							Dim nav As XPathNavigator = doc.CreateNavigator()
							Dim ni As XPathNodeIterator
							Dim ns = New XmlNamespaceManager(nav.NameTable)
							Dim delete = True
							ni = nav.Select("/exorinput")
							ni.MoveNext()
							If ni.Current.GetAttribute("delete", ns.DefaultNamespace).ToLower() = "false" Then
								delete = False
							End If

							Dim exerciseQuery As String = ni.Current.GetAttribute("exercisequery", ns.DefaultNamespace).ToLower()
							Dim workoutQuery As String = ni.Current.GetAttribute("workoutquery", ns.DefaultNamespace).ToLower()

							Dim externalId = GetXmlValueOrNull(nav, "/exorinput/source_customerno")
							If externalId IsNot Nothing Then
								Dim dto As New PersonDTO
								dto.ExternalId = externalId

								dto.Address = GetXmlValueOrNull(nav, "/exorinput/address")
								dto.Country = GetXmlValueOrNull(nav, "/exorinput/country")

								Dim parsedDate As DateTime
								' Lets first try to parse it using ISO 8601:
								If Date.TryParseExact(GetXmlValueOrNull(nav, "/exorinput/born"), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, parsedDate) Then
									dto.DateOfBirth = parsedDate.ToString("yyyy-MM-dd")
								Else
									' I would prefer people to use ISO 8601, but let the OS try to parse it instead:
									If Date.TryParse(GetXmlValueOrNull(nav, "/exorinput/born"), parsedDate) Then
										' And THEN i transform it back to a ISO 8601 valid date string:
										dto.DateOfBirth = parsedDate.ToString("yyyy-MM-dd")
									End If
								End If

								dto.Email = GetXmlValueOrNull(nav, "/exorinput/email")
								dto.Firstname = GetXmlValueOrNull(nav, "/exorinput/firstname")
								dto.Lastname = GetXmlValueOrNull(nav, "/exorinput/lastname")
								dto.Location = GetXmlValueOrNull(nav, "/exorinput/place")
								dto.Mobile = GetXmlValueOrNull(nav, "/exorinput/mobile")
								dto.PhoneHome = GetXmlValueOrNull(nav, "/exorinput/phone_home")
								dto.PhoneWork = GetXmlValueOrNull(nav, "/exorinput/phone_work")
								dto.ZipCode = GetXmlValueOrNull(nav, "/exorinput/postalcode")

								_host.SelectPerson(dto)
							End If

							If delete Then
								Try
									My.Computer.FileSystem.DeleteFile(path)
								Catch ex As Exception
									_host.LogException(ex, "Could not delete file")
									' The file must still be opened or something
								End Try
							End If

							If Not String.IsNullOrEmpty(exerciseQuery) Then _host.QueryExercises(exerciseQuery)
							If Not String.IsNullOrEmpty(workoutQuery) Then _host.QueryWorkouts(workoutQuery)

						Catch ex As Exception
							_host.LogException(ex, "Unknown error reading XML")
							'The file must be opend exclusively or something
						End Try
					End If
				End If
			End If
		End Sub
		Public Sub Initialize(host As IHost, currentDirectory As String) Implements IHosted.Initialize
			_host = host
			_currentDirectory = currentDirectory
		End Sub
		Private Sub _host_WindowMinified() Handles _host.WindowMinified
		End Sub
	End Class
End Namespace

Namespace Desktop
	Public Class Component
		Protected Function ValidateFilePath(path As String) As Boolean
			If String.IsNullOrEmpty(path) Then
				Return False
			End If
			Dim filename As String
			Try
				' ReSharper disable UnusedVariable
				'throw ArgumentException   - The path parameter contains invalid characters, is empty, or contains only white spaces.
				Dim root = IO.Path.GetPathRoot(path)
				'throw ArgumentException   - path contains one or more of the invalid characters defined in GetInvalidPathChars.
				Dim directory = IO.Path.GetDirectoryName(path)
				' ReSharper restore UnusedVariable

				'path contains one or more of the invalid characters defined in GetInvalidPathChars
				filename = IO.Path.GetFileName(path)
			Catch generatedExceptionName As ArgumentException
				Return False
			End Try
			'f the last character of path is a directory or volume separator character, this method returns String.Empty
			If [String].IsNullOrEmpty(filename) Then
				Return False
			End If
			'check for illegal chars in filename
			If filename.IndexOfAny(IO.Path.GetInvalidFileNameChars()) >= 0 Then
				Return False
			End If
			Return True
		End Function

		Protected Function GetXmlValueOrNull(navigator As XPathNavigator, path As String) As String
			Dim ni As XPathNodeIterator
			Dim val As String = Nothing
			ni = navigator.Select(path)
			If ni.Count > 0 Then
				ni.MoveNext()
				val = ni.Current.Value
			End If
			Return val
		End Function
	End Class
End Namespace