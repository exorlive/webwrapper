Namespace Desktop
	''' <summary>
	''' This interface must be implemented by all external components that want to interact with the Host
	''' </summary>
	''' <remarks></remarks>
	Public Interface IHosted

		''' <summary>
		''' The name of the component
		''' </summary>
		''' <returns></returns>
		''' <remarks></remarks>
		Function GetName() As String

		''' <summary>
		''' The component will on application startup receive the supplied commandline argumens
		''' </summary>
		''' <param name="args"></param>
		''' <remarks></remarks>
		Sub ReadCommandline(args() As String)

		''' <summary>
		''' The component will be initialized with the hosting component 
		''' </summary>
		''' <param name="host"></param>
		''' <param name="currentDirectory"></param>
		''' <remarks></remarks>
		Sub Initialize(host As IHost, currentDirectory As String)

		''' <summary>
		''' Read command line and process anything that should happen before user sign in to ExorLive.
		''' Used to tell which user may do automatic signin.
		''' </summary>
		''' <param name="args"></param>
		''' <returns></returns>
		Function GetSignonUser(args() As String) As String

	End Interface
End Namespace