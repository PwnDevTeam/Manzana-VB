

Imports System.Collections.Generic
Imports System.Text

Namespace Manzana
	''' <summary>
	''' Represents the method that will handle the <see>Connected</see> and <see>Disconnected</see> event.
	''' </summary>
	''' <param name="sender">The source of the event.</param>
	''' <param name="args">A <see>ConnectEventArgs</see> that contains the data.</param>
	Public Delegate Sub ConnectEventHandler(sender As Object, args As ConnectEventArgs)
End Namespace
