
Imports System.Collections.Generic
Imports System.Text

Namespace Manzana
	''' <summary>
	''' Provides data for the <see>Connected</see> and <see>Disconnected</see> events.
	''' </summary>
	Public Class ConnectEventArgs
		Inherits EventArgs
		Private m_message As NotificationMessage
		Private m_device As AMDevice

		Friend Sub New(cbi As AMDeviceNotificationCallbackInfo)
			m_message = cbi.msg
			m_device = cbi.dev
		End Sub

		''' <summary>
		''' Returns the information for the device that was connected or disconnected.
		''' </summary>
		Public ReadOnly Property Device() As AMDevice
			Get
				Return m_device
			End Get
		End Property

		''' <summary>
		''' Returns the type of event.
		''' </summary>
		Public ReadOnly Property Message() As NotificationMessage
			Get
				Return m_message
			End Get
		End Property
	End Class
End Namespace
