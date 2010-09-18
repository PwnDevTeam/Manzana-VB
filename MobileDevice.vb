Imports System.Collections.Generic
Imports System.Runtime.InteropServices
Imports System.Text

Namespace Manzana



    'Friend Enum AppleMobileErrors
    'End Enum

    ''' <summary>
    ''' Provides the fields representing the type of notification
    ''' </summary>
    Public Enum NotificationMessage
        ''' <summary>The iPhone was connected to the computer.</summary>
        Connected = 1
        ''' <summary>The iPhone was disconnected from the computer.</summary>
        Disconnected = 2

        ''' <summary>Notification from the iPhone occurred, but the type is unknown.</summary>
        Unknown = 3
    End Enum

    ''' <summary>
    ''' Structure describing the iPhone
    ''' </summary>
    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi, Pack:=1)> _
    Public Structure AMDevice
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=16)> _
        Friend unknown0 As Byte()
        ' 0 - zero 
        Friend device_id As UInteger
        ' 16 
        Friend product_id As UInteger
        ' 20 - set to AMD_IPHONE_PRODUCT_ID 
        ''' <summary>Write Me</summary>
        Public serial As String
        ' 24 - set to AMD_IPHONE_SERIAL 
        Friend unknown1 As UInteger
        ' 28 
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=4)> _
        Friend unknown2 As Byte()
        ' 32 
        Friend lockdown_conn As UInteger
        ' 36 
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=8)> _
        Friend unknown3 As Byte()
        ' 40 
    End Structure

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi, Pack:=1)> _
    Friend Structure AMDeviceNotification
        Private unknown0 As UInteger
        ' 0 
        Private unknown1 As UInteger
        ' 4 
        Private unknown2 As UInteger
        ' 8 
        Private callback As DeviceNotificationCallback
        ' 12 
        Private unknown3 As UInteger
        ' 16 
    End Structure

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi, Pack:=1)> _
    Friend Structure AMDeviceNotificationCallbackInfo
        Public ReadOnly Property dev() As AMDevice
            Get
                Return CType(Marshal.PtrToStructure(dev_ptr, GetType(AMDevice)), AMDevice)
            End Get
        End Property
        Friend dev_ptr As IntPtr
        Public msg As NotificationMessage
    End Structure

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi, Pack:=1)> _
    Friend Structure AMRecoveryDevice
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=8)> _
        Public unknown0 As Byte()
        ' 0 
        Public callback As DeviceRestoreNotificationCallback
        ' 8 
        Public user_info As IntPtr
        ' 12 
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=12)> _
        Public unknown1 As Byte()
        ' 16 
        Public readwrite_pipe As UInteger
        ' 28 
        Public read_pipe As Byte
        ' 32 
        Public write_ctrl_pipe As Byte
        ' 33 
        Public read_unknown_pipe As Byte
        ' 34 
        Public write_file_pipe As Byte
        ' 35 
        Public write_input_pipe As Byte
        ' 36 
    End Structure

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi, Pack:=1)> _
    Friend Structure afc_directory
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=0)> _
        Private unknown As Byte()
        ' size unknown 
    End Structure

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi, Pack:=1)> _
    Friend Structure afc_connection
        Private handle As UInteger
        ' 0 
        Private unknown0 As UInteger
        ' 4 
        Private unknown1 As Byte
        ' 8 
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=3)> _
        Private padding As Byte()
        ' 9 
        Private unknown2 As UInteger
        ' 12 
        Private unknown3 As UInteger
        ' 16 
        Private unknown4 As UInteger
        ' 20 
        Private fs_block_size As UInteger
        ' 24 
        Private sock_block_size As UInteger
        ' 28: always 0x3c 
        Private io_timeout As UInteger
        ' 32: from AFCConnectionOpen, usu. 0 
        Private afc_lock As IntPtr
        ' 36 
        Private context As UInteger
        ' 40 
    End Structure


    <UnmanagedFunctionPointer(CallingConvention.Cdecl)> _
    Friend Delegate Sub DeviceNotificationCallback(ByRef callback_info As AMDeviceNotificationCallbackInfo)
    <UnmanagedFunctionPointer(CallingConvention.Cdecl)> _
    Friend Delegate Sub DeviceRestoreNotificationCallback(ByRef callback_info As AMRecoveryDevice)

    Friend Class MobileDevice
        Public Shared Function AMDeviceNotificationSubscribe(ByVal callback As DeviceNotificationCallback, ByVal unused1 As UInteger, ByVal unused2 As UInteger, ByVal unused3 As UInteger, ByRef notification As AMDeviceNotification) As Integer
            Dim ptr As IntPtr
            Dim ret As Integer

            ptr = IntPtr.Zero
            ret = AMDeviceNotificationSubscribe(callback, unused1, unused2, unused3, ptr)
            If (ret = 0) AndAlso (ptr <> IntPtr.Zero) Then
                notification = CType(Marshal.PtrToStructure(ptr, notification.[GetType]()), AMDeviceNotification)
            End If
            Return ret
        End Function

        <DllImport("iTunesMobileDevice.dll", CallingConvention:=CallingConvention.Cdecl)> _
        Private Shared Function AMDeviceNotificationSubscribe(ByVal callback As DeviceNotificationCallback, ByVal unused1 As UInteger, ByVal unused2 As UInteger, ByVal unused3 As UInteger, ByRef am_device_notification_ptr As IntPtr) As Integer
        End Function

        <DllImport("iTunesMobileDevice.dll", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function AMDeviceConnect(ByRef device As AMDevice) As Integer
        End Function

        <DllImport("iTunesMobileDevice.dll", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function AMDeviceIsPaired(ByRef device As AMDevice) As Integer
        End Function

        <DllImport("iTunesMobileDevice.dll", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function AMDeviceValidatePairing(ByRef device As AMDevice) As Integer
        End Function

        <DllImport("iTunesMobileDevice.dll", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function AMDeviceStartSession(ByRef device As AMDevice) As Integer
        End Function

        <DllImport("iTunesMobileDevice.dll", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function AMDeviceGetConnectionID(ByRef device As AMDevice) As Integer
        End Function

        <DllImport("iTunesMobileDevice.dll", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function AMRestoreModeDeviceCreate(ByVal unknown0 As UInteger, ByVal connection_id As Integer, ByVal unknown1 As UInteger) As Integer
        End Function

        <DllImport("CoreFoundation.dll", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function __CFStringMakeConstantString(ByVal s As Byte())
        End Function

        <DllImport("iTunesMobileDevice.dll", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function AFCDirectoryOpen(ByVal conn As IntPtr, ByVal path As String, ByRef dir As IntPtr) As Integer
        End Function

        <DllImport("iTunesMobileDevice.dll", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function AMDeviceEnterRecovery(ByVal device As Integer) As Integer
        End Function

        Public Shared Function AFCDirectoryRead(ByVal conn As IntPtr, ByVal dir As IntPtr, ByRef buffer As String) As Integer
            Dim ptr As IntPtr
            Dim ret As Integer

            ptr = IntPtr.Zero
            ret = AFCDirectoryRead(conn, dir, ptr)
            If (ret = 0) AndAlso (ptr <> IntPtr.Zero) Then
                buffer = Marshal.PtrToStringAnsi(ptr)
            Else
                buffer = Nothing
            End If
            Return ret
        End Function
        <DllImport("iTunesMobileDevice.dll", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function AFCDirectoryRead(ByVal conn As IntPtr, ByVal dir As IntPtr, ByRef dirent As IntPtr) As Integer
        End Function

        <DllImport("iTunesMobileDevice.dll", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function AFCDirectoryClose(ByVal conn As IntPtr, ByVal dir As IntPtr) As Integer
        End Function

        <DllImport("iTunesMobileDevice.dll", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function AMRestoreRegisterForDeviceNotifications(ByVal dfu_connect As DeviceRestoreNotificationCallback, ByVal recovery_connect As DeviceRestoreNotificationCallback, ByVal dfu_disconnect As DeviceRestoreNotificationCallback, ByVal recovery_disconnect As DeviceRestoreNotificationCallback, ByVal unknown0 As UInteger, ByVal user_info As IntPtr) As Integer
        End Function


        Public Shared Function AMDeviceStartService(ByRef device As AMDevice, ByVal service_name As String, ByRef conn As afc_connection, ByVal unknown As IntPtr) As Integer
            Dim ptr As IntPtr
            Dim ret As Integer

            ptr = IntPtr.Zero
            ret = AMDeviceStartService(device, StringToCFString(service_name), ptr, unknown)
            If (ret = 0) AndAlso (ptr <> IntPtr.Zero) Then
                conn = CType(Marshal.PtrToStructure(ptr, conn.[GetType]()), afc_connection)
            End If
            Return ret
        End Function
        <DllImport("iTunesMobileDevice.dll", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function AMDeviceStartService(ByRef device As AMDevice, ByVal service_name As Byte(), ByRef handle As IntPtr, ByVal unknown As IntPtr) As Integer
        End Function

        Public Shared Function AFCConnectionOpen(ByVal handle As IntPtr, ByVal io_timeout As UInteger, ByRef conn As afc_connection) As Integer
            Dim ptr As IntPtr
            Dim ret As Integer

            ptr = IntPtr.Zero
            ret = AFCConnectionOpen(handle, io_timeout, ptr)
            If (ret = 0) AndAlso (ptr <> IntPtr.Zero) Then
                conn = CType(Marshal.PtrToStructure(ptr, conn.[GetType]()), afc_connection)
            End If
            Return ret
        End Function
        <DllImport("iTunesMobileDevice.dll", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function AFCConnectionOpen(ByVal handle As IntPtr, ByVal io_timeout As UInteger, ByRef conn As IntPtr) As Integer
        End Function

        Public Shared Function AMDeviceCopyValue(ByRef device As AMDevice, ByVal unknown As UInteger, ByVal name As String) As String
            Dim result As IntPtr
            Dim cfstring As Byte()

            cfstring = StringToCFString(name)

            result = AMDeviceCopyValue_Int(device, unknown, cfstring)
            If result <> IntPtr.Zero Then
                Dim length As Byte

                length = Marshal.ReadByte(result, 8)
                If length > 0 Then
                    Return Marshal.PtrToStringAnsi(New IntPtr(result.ToInt64() + 9), length)
                Else
                    Return [String].Empty
                End If
            End If
            Return [String].Empty
        End Function

        <DllImport("iTunesMobileDevice.dll", EntryPoint:="AMDeviceCopyValue", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function AMDeviceCopyValue_Int(ByRef device As AMDevice, ByVal unknown As UInteger, ByVal cfstring As Byte()) As IntPtr
        End Function

        <DllImport("iTunesMobileDevice.dll", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function AFCGetFileInfo(ByVal conn As IntPtr, ByVal path As String, ByRef buffer As IntPtr, ByVal length As UInteger) As Integer
        End Function

        <DllImport("iTunesMobileDevice.dll", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function AFCRemovePath(ByVal conn As IntPtr, ByVal path As String) As Integer
        End Function

        <DllImport("iTunesMobileDevice.dll", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function AFCRenamePath(ByVal conn As IntPtr, ByVal old_path As String, ByVal new_path As String) As Integer
        End Function

        <DllImport("iTunesMobileDevice.dll", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function AFCFileRefOpen(ByVal conn As IntPtr, ByVal path As String, ByVal mode As Integer, ByVal unknown As Integer, ByVal handle As Int64) As Integer
        End Function

        <DllImport("iTunesMobileDevice.dll", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function AFCFileRefClose(ByVal conn As IntPtr, ByVal handle As Int64) As Integer
        End Function

        <DllImport("iTunesMobileDevice.dll", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function AFCFileRefRead(ByVal conn As IntPtr, ByVal handle As Int64, ByVal buffer As Byte(), ByRef len As UInteger) As Integer
        End Function

        <DllImport("iTunesMobileDevice.dll", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function AFCFileRefWrite(ByVal conn As IntPtr, ByVal handle As Int64, ByVal buffer As Byte(), ByVal len As UInteger) As Integer
        End Function

        <DllImport("iTunesMobileDevice.dll", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function AFCFlushData(ByVal conn As IntPtr, ByVal handle As Int64) As Integer
        End Function

        ' FIXME - not working, arguments? Always returns 7
        <DllImport("iTunesMobileDevice.dll", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function AFCFileRefSeek(ByVal conn As IntPtr, ByVal handle As Int64, ByVal pos As UInteger, ByVal origin As UInteger) As Integer
        End Function

        <DllImport("iTunesMobileDevice.dll", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function AFCFileRefTell(ByVal conn As IntPtr, ByVal handle As Int64, ByRef position As UInteger) As Integer
        End Function

        ' FIXME - not working, arguments?
        <DllImport("iTunesMobileDevice.dll", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function AFCFileRefSetFileSize(ByVal conn As IntPtr, ByVal handle As Int64, ByVal size As UInteger) As Integer
        End Function

        <DllImport("iTunesMobileDevice.dll", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function AFCDirectoryCreate(ByVal conn As IntPtr, ByVal path As String) As Integer
        End Function


        Friend Shared Function StringToCFString(ByVal value As String) As Byte()
            Dim b As Byte()

            b = New Byte(value.Length + 9) {}
            b(4) = &H8C
            b(5) = 7
            b(6) = 1
            b(8) = CByte(value.Length)
            Encoding.ASCII.GetBytes(value, 0, value.Length, b, 9)
            Return b

        End Function

        '      Public Shared Function StringToCFString(ByVal value As String) As Byte()
        '     Dim bytes As Byte() = New Byte(value.Length + 1) {}
        '      Encoding.ASCII.GetBytes(value, 0, value.Length, bytes, 0)
        '       Return bytes
        '    End Function

        Friend Shared Function CFStringToString(ByVal value As Byte()) As String
            Return Encoding.ASCII.GetString(value, 9, value(9))
        End Function
    End Class

End Namespace
