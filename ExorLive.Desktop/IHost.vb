Namespace Desktop
	''' <summary>
	''' This interface defines the  methods available on the Host.
	''' </summary>
	''' <remarks></remarks>
	Public Interface IHost
		''' <summary>
		''' Will set the given person as the selected user in ExoLive.
		''' The user will be created if it does not already exist.
		''' </summary>
		''' <param name="person">The object containing the needed data.</param>
		''' <remarks>The ExternalId is used as the identifier</remarks>
		Sub SelectPerson(person As PersonDTO)

		''' <summary>
		''' Makes an attempt to delete the user identified by ExternalId.
		''' </summary>
		''' <param name="externalId">The persons id in the external journal.</param>
		''' <remarks>The attempt might not be successfull due to ACL restrictions.</remarks>
		Sub DeletePerson(externalId As String)

		''' <summary>
		''' Select the selected tab if possible
		''' </summary>
		''' <param name="tab">The name of the tab to select</param>
		''' <remarks>Not all tabs are visible and so not all may be selected</remarks>
		Sub SelectTab(tab As String)

		''' <summary>
		''' Request that a query is made for matching workouts
		''' </summary>
		''' <param name="query">The tags to be queried for</param>
		''' <remarks></remarks>
		Sub QueryWorkouts(query As String)

		''' <summary>
		''' Request that a query is made for matching exercises
		''' </summary>
		''' <param name="query">The tags to be queried for</param>
		''' <remarks></remarks>
		Sub QueryExercises(query As String)

		Sub OpenWorkout(workoutIdAsString As String)

		''' <summary>
		''' This event will be fired if the main window is being minified
		''' </summary>
		''' <remarks></remarks>
		Event WindowMinified()

		''' <summary>
		''' This event will be fired when the window is closing
		''' </summary>
		''' <remarks></remarks>
		Event WindowClosing()

		''' <summary>
		''' Method for accessing the log facilitis of the host
		''' </summary>
		''' <param name="ex">The Exception to log</param>
		''' <param name="message">The message</param>
		''' <remarks></remarks>
		Sub LogException(ex As Exception, message As String)

	End Interface
End Namespace
