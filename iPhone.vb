
Imports System.Collections
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Text

Namespace Manzana
	''' <summary>
	''' Exposes access to the Apple iPhone
	''' </summary>
	Public Class iPhone
		#Region "Locals"
		Private dnc As DeviceNotificationCallback
		Private drn1 As DeviceRestoreNotificationCallback
		Private drn2 As DeviceRestoreNotificationCallback
		Private drn3 As DeviceRestoreNotificationCallback
		Private drn4 As DeviceRestoreNotificationCallback

		Friend iPhoneHandle As AMDevice
		Friend hAFC As IntPtr
		Private connected As Boolean
		Private current_directory As String
		#End Region

		#Region "Constructors"
		''' <summary>
		''' Creates a new iPhone object. If an iPhone is connected to the computer, a connection will automatically be opened.
		''' </summary>
		Public Sub New()
			Dim notification As AMDeviceNotification
			Dim ret As Integer = 0

			dnc = New DeviceNotificationCallback(AddressOf NotifyCallback)
			drn1 = New DeviceRestoreNotificationCallback(AddressOf DfuConnectCallback)
			drn2 = New DeviceRestoreNotificationCallback(AddressOf RecoveryConnectCallback)
			drn3 = New DeviceRestoreNotificationCallback(AddressOf DfuDisconnectCallback)
			drn4 = New DeviceRestoreNotificationCallback(AddressOf RecoveryDisconnectCallback)

			notification = New AMDeviceNotification()
			ret = MobileDevice.AMDeviceNotificationSubscribe(dnc, 0, 0, 0, notification)
			If ret <> 0 Then
				Throw New Exception("AMDeviceNotificationSubscribe failed with error " & ret)
			End If

            ret = MobileDevice.AMRestoreRegisterForDeviceNotifications(drn1, drn2, drn3, drn4, 0, IntPtr.Zero)
			If ret <> 0 Then
				Throw New Exception("AMRestoreRegisterForDeviceNotifications failed with error " & ret)
			End If
			current_directory = "/"
		End Sub
		#End Region

		#Region "Properties"
		''' <summary>
		''' Gets the current activation state of the phone
		''' </summary>
		Public ReadOnly Property ActivationState() As String
			Get
                Return MobileDevice.AMDeviceCopyValue(iPhoneHandle, 0, "ActivationState")
			End Get
		End Property

		''' <summary>
		''' Returns true if an iPhone is connected to the computer
		''' </summary>
		Public ReadOnly Property IsConnected() As Boolean
			Get
				Return connected
			End Get
		End Property


        ''' <summary>
        ''' Returns the Device information about the connected iPhone
        ''' </summary>
        Public ReadOnly Property Device() As AMDevice
            Get
                Return Me.iPhoneHandle
            End Get
        End Property

        ''' <summary>
        ''' Returns the handle to the iPhone com.apple.afc service
        ''' </summary>
        Public ReadOnly Property AFCHandle() As IntPtr
            Get
                Return Me.hAFC
            End Get
        End Property

        ''' <summary>
        ''' Gets/Sets the current working directory, used by all file and directory methods
        ''' </summary>
        Public Property CurrentDirectory() As String
            Get
                Return current_directory
            End Get

            Set(ByVal value As String)
                current_directory = value
            End Set
        End Property
#End Region

#Region "Events"
        ''' <summary>
        ''' The <c>Connect</c> event is triggered when a iPhone is connected to the computer
        ''' </summary>
        Public Event Connect As ConnectEventHandler

        ''' <summary>
        ''' Raises the <see>Connect</see> event.
        ''' </summary>
        ''' <param name="args">A <see cref="ConnectEventArgs"/> that contains the event data.</param>
        Protected Sub OConnect(ByVal args As ConnectEventArgs)
            RaiseEvent Connect(Me, args)
        End Sub

        ''' <summary>
        ''' The <c>Disconnect</c> event is triggered when the iPhone is disconnected from the computer
        ''' </summary>
        Public Event Disconnect As ConnectEventHandler

        ''' <summary>
        ''' Raises the <see>Disconnect</see> event.
        ''' </summary>
        ''' <param name="args">A <see cref="ConnectEventArgs"/> that contains the event data.</param>
        Protected Sub OnDisconnect(ByVal args As ConnectEventArgs)
            RaiseEvent Disconnect(Me, args)
        End Sub

        ''' <summary>
        ''' Write Me
        ''' </summary>
        Public Event DfuConnect As EventHandler

        ''' <summary>
        ''' Raises the <see>DfuConnect</see> event.
        ''' </summary>
        ''' <param name="args">A <see cref="DeviceNotificationEventArgs"/> that contains the event data.</param>
        Protected Sub OnDfuConnect(ByVal args As DeviceNotificationEventArgs)
            RaiseEvent DfuConnect(Me, args)
        End Sub

        ''' <summary>
        ''' Write Me
        ''' </summary>
        Public Event DfuDisconnect As EventHandler

        ''' <summary>
        ''' Raises the <see>DfiDisconnect</see> event.
        ''' </summary>
        ''' <param name="args">A <see cref="DeviceNotificationEventArgs"/> that contains the event data.</param>
        Protected Sub OnDfuDisconnect(ByVal args As DeviceNotificationEventArgs)
            RaiseEvent DfuDisconnect(Me, args)
        End Sub

        ''' <summary>
        ''' The RecoveryModeEnter event is triggered when the attached iPhone enters Recovery Mode
        ''' </summary>
        Public Event RecoveryModeEnter As EventHandler

        ''' <summary>
        ''' Raises the <see>RecoveryModeEnter</see> event.
        ''' </summary>
        ''' <param name="args">A <see cref="DeviceNotificationEventArgs"/> that contains the event data.</param>
        Protected Sub OnRecoveryModeEnter(ByVal args As DeviceNotificationEventArgs)
            RaiseEvent RecoveryModeEnter(Me, args)
        End Sub

        ''' <summary>
        ''' The RecoveryModeLeave event is triggered when the attached iPhone leaves Recovery Mode
        ''' </summary>
        Public Event RecoveryModeLeave As EventHandler

        ''' <summary>
        ''' Raises the <see>RecoveryModeLeave</see> event.
        ''' </summary>
        ''' <param name="args">A <see cref="DeviceNotificationEventArgs"/> that contains the event data.</param>
        Protected Sub OnRecoveryModeLeave(ByVal args As DeviceNotificationEventArgs)
            RaiseEvent RecoveryModeLeave(Me, args)
        End Sub

#End Region

#Region "Filesystem"
        ''' <summary>
        ''' Returns the names of files in a specified directory
        ''' </summary>
        ''' <param name="path">The directory from which to retrieve the files.</param>
        ''' <returns>A <c>String</c> array of file names in the specified directory. Names are relative to the provided directory</returns>
        Public Function GetFiles(ByVal path As String) As String()
            Dim hAFCDir As IntPtr
            Dim buffer As String
            Dim paths As ArrayList
            Dim full_path As String

            If Not connected Then
                Throw New Exception("Not connected to phone")
            End If

            hAFCDir = New IntPtr()
            full_path = FullPath(current_directory, path)

            If MobileDevice.AFCDirectoryOpen(hAFC, full_path, hAFCDir) <> 0 Then
                Throw New Exception("Path does not exist")
            End If

            buffer = Nothing
            paths = New ArrayList()
            MobileDevice.AFCDirectoryRead(hAFC, hAFCDir, buffer)

            While buffer IsNot Nothing
                If Not IsDirectory(FullPath(full_path, buffer)) Then
                    paths.Add(buffer)
                End If
                MobileDevice.AFCDirectoryRead(hAFC, hAFCDir, buffer)
            End While
            MobileDevice.AFCDirectoryClose(hAFC, hAFCDir)
            Return DirectCast(paths.ToArray(GetType(String)), String())
        End Function

        ''' <summary>
        ''' Returns the size and type of the specified file or directory.
        ''' </summary>
        ''' <param name="path">The file or directory for which to retrieve information.</param>
        ''' <param name="size">Returns the size of the specified file or directory</param>
        ''' <param name="directory">Returns <c>true</c> if the given path describes a directory, false if it is a file.</param>
        Public Sub GetFileInfo(ByVal path As String, ByVal size As Integer, ByVal directory As Boolean)
            Dim data As IntPtr
            Dim current_data As IntPtr
            Dim data_size As UInteger
            Dim offset As UInteger
            Dim name As String
            Dim value As String
            Dim ret As Integer

            data = IntPtr.Zero

            size = 0
            directory = False
            ret = MobileDevice.AFCGetFileInfo(hAFC, path, data, data_size)
            If ret <> 0 Then
                Return
            End If

            offset = 0
            While offset < data_size
                current_data = New IntPtr(data.ToInt32() + offset)
                name = Marshal.PtrToStringAnsi(current_data)
                offset += CUInt(name.Length) + 1

                current_data = New IntPtr(data.ToInt32() + offset)
                value = Marshal.PtrToStringAnsi(current_data)
                offset += CUInt(value.Length) + 1
                Select Case name
                    Case "st_size"
                        size = Int32.Parse(value)
                        Exit Select
                    Case "st_blocks"
                        Exit Select
                    Case "st_ifmt"
                        If value = "S_IFDIR" Then
                            directory = True
                        End If
                        Exit Select
                End Select
            End While

        End Sub

        ''' <summary>
        ''' Returns the size of the specified file or directory.
        ''' </summary>
        ''' <param name="path">The file or directory for which to obtain the size.</param>
        ''' <returns></returns>
        Public Function FileSize(ByVal path As String) As Integer
            Dim is_dir As Boolean
            Dim size As Integer

            GetFileInfo(path, size, is_dir)
            Return size
        End Function

        ''' <summary>
        ''' Creates the directory specified in path
        ''' </summary>
        ''' <param name="path">The directory path to create</param>
        ''' <returns>true if directory was created</returns>
        Public Function CreateDirectory(ByVal path As String) As Boolean
            Dim full_path As String

            full_path = FullPath(current_directory, path)
            If MobileDevice.AFCDirectoryCreate(hAFC, full_path) <> 0 Then
                Return False
            End If

            Return True
        End Function

        ''' <summary>
        ''' Gets the names of subdirectories in a specified directory.
        ''' </summary>
        ''' <param name="path">The path for which an array of subdirectory names is returned.</param>
        ''' <returns>An array of type <c>String</c> containing the names of subdirectories in <c>path</c>.</returns>
        Public Function GetDirectories(ByVal path As String) As String()
            Dim hAFCDir As IntPtr
            Dim buffer As String
            Dim paths As ArrayList
            Dim full_path As String

            If Not connected Then
                Throw New Exception("Not connected to phone")
            End If

            hAFCDir = New IntPtr()
            full_path = FullPath(current_directory, path)

            If MobileDevice.AFCDirectoryOpen(hAFC, full_path, hAFCDir) <> 0 Then
                Throw New Exception("Path does not exist")
            End If

            buffer = Nothing
            paths = New ArrayList()
            MobileDevice.AFCDirectoryRead(hAFC, hAFCDir, buffer)

            While buffer IsNot Nothing
                If (buffer <> ".") AndAlso (buffer <> "..") AndAlso IsDirectory(FullPath(full_path, buffer)) Then
                    paths.Add(buffer)
                End If
                MobileDevice.AFCDirectoryRead(hAFC, hAFCDir, buffer)
            End While
            MobileDevice.AFCDirectoryClose(hAFC, hAFCDir)
            Return DirectCast(paths.ToArray(GetType(String)), String())
        End Function

        ''' <summary>
        ''' Moves a file or a directory and its contents to a new location or renames a file or directory if the old and new parent path matches.
        ''' </summary>
        ''' <param name="sourceName">The path of the file or directory to move or rename.</param>
        ''' <param name="destName">The path to the new location for <c>sourceName</c>.</param>
        '''	<remarks>Files cannot be removed across filesystem boundaries.</remarks>
        Public Sub Rename(ByVal sourceName As String, ByVal destName As String)
            MobileDevice.AFCRenamePath(hAFC, FullPath(current_directory, sourceName), FullPath(current_directory, destName))
        End Sub

        ''' <summary>
        ''' FIXME
        ''' </summary>
        ''' <param name="sourceName"></param>
        ''' <param name="destName"></param>
        Public Sub Copy(ByVal sourceName As String, ByVal destName As String)

        End Sub

        ''' <summary>
        ''' Returns the root information for the specified path. 
        ''' </summary>
        ''' <param name="path">The path of a file or directory.</param>
        ''' <returns>A string containing the root information for the specified path. </returns>
        Public Function GetDirectoryRoot(ByVal path As String) As String
            Return "/"
        End Function

        ''' <summary>
        ''' Determines whether the given path refers to an existing file or directory on the phone. 
        ''' </summary>
        ''' <param name="path">The path to test.</param>
        ''' <returns><c>true</c> if path refers to an existing file or directory, otherwise <c>false</c>.</returns>
        Public Function Exists(ByVal path As String) As Boolean
            Dim data_size As UInteger
            Dim data As IntPtr

            data = IntPtr.Zero

            If MobileDevice.AFCGetFileInfo(hAFC, path, data, data_size) <> 0 Then
                Return False
            End If

            Return True
        End Function

        ''' <summary>
        ''' Determines whether the given path refers to an existing directory on the phone. 
        ''' </summary>
        ''' <param name="path">The path to test.</param>
        ''' <returns><c>true</c> if path refers to an existing directory, otherwise <c>false</c>.</returns>
        Public Function IsDirectory(ByVal path As String) As Boolean
            Dim is_dir As Boolean
            Dim size As Integer

            GetFileInfo(path, size, is_dir)
            Return is_dir
        End Function

        ''' <summary>
        ''' Deletes an empty directory from a specified path.
        ''' </summary>
        ''' <param name="path">The name of the empty directory to remove. This directory must be writable and empty.</param>
        Public Sub DeleteDirectory(ByVal path As String)
            Dim full_path As String

            full_path = FullPath(current_directory, path)
            If IsDirectory(full_path) Then
                MobileDevice.AFCRemovePath(hAFC, full_path)
            End If
        End Sub

        ''' <summary>
        ''' Deletes the specified directory and, if indicated, any subdirectories in the directory.
        ''' </summary>
        ''' <param name="path">The name of the directory to remove.</param>
        ''' <param name="recursive"><c>true</c> to remove directories, subdirectories, and files in path; otherwise, <c>false</c>. </param>
        Public Sub DeleteDirectory(ByVal path As String, ByVal recursive As Boolean)
            Dim full_path As String

            If Not recursive Then
                DeleteDirectory(path)
                Return
            End If

            full_path = FullPath(current_directory, path)
            If IsDirectory(full_path) Then
                InternalDeleteDirectory(path)
            End If

        End Sub

        ''' <summary>
        ''' Deletes the specified file.
        ''' </summary>
        ''' <param name="path">The name of the file to remove.</param>
        Public Sub DeleteFile(ByVal path As String)
            Dim full_path As String

            full_path = FullPath(current_directory, path)
            If Exists(full_path) Then
                MobileDevice.AFCRemovePath(hAFC, full_path)
            End If
        End Sub

        ''' <summary>
        ''' Gets the current working directory of the object. 
        ''' </summary>
        ''' <returns>A <c>string</c> containing the path of the current working directory. </returns>
        Public Function GetCurrentDirectory() As String
            Return current_directory
        End Function

        ''' <summary>
        ''' Sets the application's current working directory to the specified directory.
        ''' </summary>
        ''' <param name="path">The path to which the current working directory should be set.</param>
        Public Sub SetCurrentDirectory(ByVal path As String)
            Dim new_path As String

            new_path = FullPath(current_directory, path)
            If Not IsDirectory(new_path) Then
                Throw New Exception("Invalid directory specified")
            End If
            current_directory = new_path
        End Sub
#End Region

#Region "Private Methods"

        Private Function ConnectToPhone() As Boolean
            If MobileDevice.AMDeviceConnect(iPhoneHandle) = 1 Then
                'int connid;

                'connid = MobileDevice.AMDeviceGetConnectionID(ref iPhoneHandle);
                'MobileDevice.AMRestoreModeDeviceCreate(0, connid, 0);
                'return false;
                Throw New Exception("Phone in recovery mode, support not yet implemented")
            End If
            If MobileDevice.AMDeviceIsPaired(iPhoneHandle) = 0 Then
                Return False
            End If

            If MobileDevice.AMDeviceValidatePairing(iPhoneHandle) <> 0 Then
                Return False
            End If

            If MobileDevice.AMDeviceStartSession(iPhoneHandle) = 1 Then
                Return False
            End If

            If MobileDevice.AMDeviceStartService(iPhoneHandle, MobileDevice.StringToCFString("com.apple.afc2"), hAFC, IntPtr.Zero) <> 0 Then
                If MobileDevice.AMDeviceStartService(iPhoneHandle, MobileDevice.StringToCFString("com.apple.afc"), hAFC, IntPtr.Zero) <> 0 Then
                    Return False
                End If
            End If

            If MobileDevice.AFCConnectionOpen(hAFC, 0, hAFC) <> 0 Then
                Return False
            End If

            connected = True
            Return True
        End Function

        Private Sub NotifyCallback(ByRef callback As AMDeviceNotificationCallbackInfo)
            If callback.msg = NotificationMessage.Connected Then
                iPhoneHandle = callback.dev
                If ConnectToPhone() Then
                    '       Connect(New ConnectEventArgs(callback))
                End If
            ElseIf callback.msg = NotificationMessage.Disconnected Then
                connected = False
                OnDisconnect(New ConnectEventArgs(callback))
            End If
        End Sub

        Private Sub DfuConnectCallback(ByRef callback As AMRecoveryDevice)
            OnDfuConnect(New DeviceNotificationEventArgs(callback))
        End Sub

        Private Sub DfuDisconnectCallback(ByRef callback As AMRecoveryDevice)
            OnDfuDisconnect(New DeviceNotificationEventArgs(callback))
        End Sub

        Private Sub RecoveryConnectCallback(ByRef callback As AMRecoveryDevice)
            OnRecoveryModeEnter(New DeviceNotificationEventArgs(callback))
        End Sub

        Private Sub RecoveryDisconnectCallback(ByRef callback As AMRecoveryDevice)
            OnRecoveryModeLeave(New DeviceNotificationEventArgs(callback))
        End Sub

        Private Sub InternalDeleteDirectory(ByVal path As String)
            Dim full_path As String
            Dim contents As String()

            full_path = FullPath(current_directory, path)
            contents = GetFiles(path)
            For i As Integer = 0 To contents.Length - 1
                DeleteFile(full_path & "/" & contents(i))
            Next

            contents = GetDirectories(path)
            For i As Integer = 0 To contents.Length - 1
                InternalDeleteDirectory(full_path & "/" & contents(i))
            Next

            DeleteDirectory(path)
        End Sub

        Shared path_separators As Char() = {"/"c}
        Friend Function FullPath(ByVal path1 As String, ByVal path2 As String) As String
            Dim path_parts As String()
            Dim result_parts As String()
            Dim target_index As Integer

            If (path1 Is Nothing) OrElse (path1 = [String].Empty) Then
                path1 = "/"
            End If

            If (path2 Is Nothing) OrElse (path2 = [String].Empty) Then
                path2 = "/"
            End If

            If path2(0) = "/"c Then
                path_parts = path2.Split(path_separators)
            ElseIf path1(0) = "/"c Then
                path_parts = (path1 & "/" & path2).Split(path_separators)
            Else
                path_parts = ("/" & path1 & "/" & path2).Split(path_separators)
            End If
            result_parts = New String(path_parts.Length - 1) {}
            target_index = 0

            For i As Integer = 0 To path_parts.Length - 1
                If path_parts(i) = ".." Then
                    If target_index > 0 Then
                        target_index -= 1
                    End If
                    ' Do nothing
                ElseIf (path_parts(i) = ".") OrElse (path_parts(i) = "") Then
                Else
                    result_parts(System.Math.Max(System.Threading.Interlocked.Increment(target_index), target_index - 1)) = path_parts(i)
                End If
            Next

            Return "/" & [String].Join("/", result_parts, 0, target_index)
        End Function
#End Region

    End Class
End Namespace
