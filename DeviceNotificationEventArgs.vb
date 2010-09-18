
Imports System.Collections.Generic
Imports System.Text

Namespace Manzana
	''' <summary>
	''' Provides data for the <see>DfuConnect</see>, <see>DfuDisconnect</see>, <see>RecoveryModeEnter</see> and <see>RecoveryModeLeave</see> events.
	''' </summary>
	Public Class DeviceNotificationEventArgs
		Inherits EventArgs
		Private m_device As AMRecoveryDevice

		Friend Sub New(device As AMRecoveryDevice)
			Me.m_device = device
		End Sub

		Friend ReadOnly Property Device() As AMRecoveryDevice
			Get
				Return m_device
			End Get
		End Property
	End Class
End Namespace
