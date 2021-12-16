Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Xml.XPath

Namespace Desktop.Promed
	Public Class Component
		Inherits Desktop.Component
		Implements IHosted
		Private WithEvents _host As IHost
		Private _currentDirectory As String

		Public Function GetName() As String Implements IHosted.GetName
			Return "Promed XML bridge"
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
							Dim dto As New PersonDTO

							Dim doc As New XPathDocument(path)
							Dim nav As XPathNavigator = doc.CreateNavigator()

							Dim fullname = GetXmlValueOrNull(nav, "/VFPData/curexor/navn")
							Dim name() As String = fullname.Split(","c)
							dto.Firstname = name(1)
							dto.Lastname = name(0)

							dto.Address = GetXmlValueOrNull(nav, "/VFPData/curexor/addresse")
							dto.Country = GetXmlValueOrNull(nav, "/VFPData/curexor/land")

							Dim parsedDate As DateTime
							' Lets first try to parse it using ISO 8601:
							If Date.TryParseExact(GetXmlValueOrNull(nav, "/VFPData/curexor/fodt"), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, parsedDate) Then
								dto.DateOfBirth = parsedDate.ToString("yyyy-MM-dd")
							Else
								' I would prefer people to use ISO 8601, but let the OS try to parse it instead:
								If Date.TryParse(GetXmlValueOrNull(nav, "/VFPData/curexor/fodt"), parsedDate) Then
									' And THEN i transform it back to a ISO 8601 valid date string:
									dto.DateOfBirth = parsedDate.ToString("yyyy-MM-dd")
								End If
							End If
							dto.Email = GetXmlValueOrNull(nav, "/VFPData/curexor/epost")
							dto.ExternalId = GetXmlValueOrNull(nav, "/VFPData/curexor/nr")
							dto.Location = GetXmlValueOrNull(nav, "/VFPData/curexor/poststed")
							dto.Mobile = GetXmlValueOrNull(nav, "/VFPData/curexor/mobil")
							dto.PhoneHome = GetXmlValueOrNull(nav, "/VFPData/curexor/hjemme")
							dto.PhoneWork = GetXmlValueOrNull(nav, "/VFPData/curexor/jobb")
							dto.ZipCode = GetXmlValueOrNull(nav, "/VFPData/curexor/postnr")

							_host.SelectPerson(dto)
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