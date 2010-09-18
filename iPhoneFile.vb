
Imports System.Collections.Generic
Imports System.IO
Imports System.Text

Namespace Manzana
	''' <summary>
	''' Exposes a stream to a file on an iPhone, supporting both synchronous and asynchronous read and write operations
	''' </summary>
	Public Class iPhoneFile
		Inherits Stream
		Private Enum OpenMode
			None = 0
			Read = 2
			Write = 3
		End Enum

		#Region "Fields"
		Private mode As OpenMode
		Private handle As Long
		Private phone As iPhone
		#End Region

		#Region "Constructors"
		Private Sub New(phone As iPhone, handle As Long, mode As OpenMode)
			MyBase.New()
			Me.phone = phone
			Me.mode = mode
			Me.handle = handle
		End Sub

		#End Region

		#Region "Public Properties"
		''' <summary>
		''' gets a value indicating whether the current stream supports reading.
		''' </summary>
		Public Overrides ReadOnly Property CanRead() As Boolean
			Get
				If mode = OpenMode.Read Then
					Return True
				End If
				Return False
			End Get
		End Property

		''' <summary>
		''' Gets a value indicating whether the current stream supports seeking.
		''' </summary>
		Public Overrides ReadOnly Property CanSeek() As Boolean
			Get
				Return False
			End Get
		End Property

		''' <summary>
		''' Gets a value that determines whether the current stream can time out. 
		''' </summary>
		Public Overrides ReadOnly Property CanTimeout() As Boolean
			Get
				Return True
			End Get
		End Property

		''' <summary>
		''' Gets a value indicating whether the current stream supports writing
		''' </summary>
		Public Overrides ReadOnly Property CanWrite() As Boolean
			Get
				If mode = OpenMode.Write Then
					Return True
				End If
				Return False
			End Get
		End Property

		''' <summary>
		''' Gets the length in bytes of the stream . 
		''' </summary>
		Public Overrides ReadOnly Property Length() As Long
			Get
				Throw New Exception("The method or operation is not implemented.")
			End Get
		End Property

		''' <summary>
		''' Gets or sets the position within the current stream
		''' </summary>
		Public Overrides Property Position() As Long
			Get
				Dim ret As UInteger
				ret = 0

				MobileDevice.AFCFileRefTell(phone.AFCHandle, handle, ret)
				Return CLng(ret)
			End Get
			Set
				Me.Seek(value, SeekOrigin.Begin)
			End Set
		End Property

		''' <summary>
		''' Sets the length of this stream to the given value. 
		''' </summary>
		''' <param name="value">The new length of the stream.</param>
		Public Overrides Sub SetLength(value As Long)
			Dim ret As Integer

			ret = MobileDevice.AFCFileRefSetFileSize(phone.AFCHandle, handle, CUInt(value))
		End Sub
		#End Region

		#Region "Public Methods"
		''' <summary>
		''' Releases the unmanaged resources used by iPhoneFile
		''' </summary>
		''' <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		Protected Overrides Sub Dispose(disposing As Boolean)
			If disposing Then
				If handle <> 0 Then
					MobileDevice.AFCFileRefClose(phone.AFCHandle, handle)
					handle = 0
				End If
			End If
			MyBase.Dispose(disposing)
		End Sub

		''' <summary>
		''' Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read
		''' </summary>
        ''' <param name="buffer__1">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between offset and (offset + count - 1) replaced by the bytes read from the current source. </param>
		''' <param name="offset">The zero-based byte offset in buffer at which to begin storing the data read from the current stream.</param>
		''' <param name="count">The maximum number of bytes to be read from the current stream.</param>
		''' <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.</returns>
		Public Overrides Function Read(buffer__1 As Byte(), offset As Integer, count As Integer) As Integer
			Dim len As UInteger
			Dim ret As Integer
			Dim temp As Byte()

			If mode <> OpenMode.Read Then
				Throw New NotImplementedException("Stream open for writing only")
			End If

			If offset = 0 Then
				temp = buffer__1
			Else
				temp = New Byte(count - 1) {}
			End If
			len = CUInt(count)
			ret = MobileDevice.AFCFileRefRead(phone.AFCHandle, handle, temp, len)
			If ret <> 0 Then
				Throw New IOException("AFCFileRefRead error = " & ret.ToString())
			End If
			If temp IsNot buffer__1 Then
				Buffer.BlockCopy(temp, 0, buffer__1, offset, CInt(len))
			End If
			Return CInt(len)
		End Function

		''' <summary>
		''' Writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written. 
		''' </summary>
        ''' <param name="buffer__1">An array of bytes. This method copies count bytes from buffer to the current stream.</param>
		''' <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
		''' <param name="count">The number of bytes to be written to the current stream.</param>
		Public Overrides Sub Write(buffer__1 As Byte(), offset As Integer, count As Integer)
			Dim ret As Integer
			Dim len As UInteger
			Dim temp As Byte()

			If mode <> OpenMode.Write Then
				Throw New NotImplementedException("Stream open for reading only")
			End If

			If offset = 0 Then
				temp = buffer__1
			Else
				temp = New Byte(count - 1) {}
				Buffer.BlockCopy(buffer__1, offset, temp, 0, count)
			End If
			len = CUInt(count)
			ret = MobileDevice.AFCFileRefWrite(phone.AFCHandle, handle, temp, len)
		End Sub

		''' <summary>
		''' Sets the position within the current stream
		''' </summary>
		''' <param name="offset">A byte offset relative to the <c>origin</c> parameter</param>
		''' <param name="origin">A value of type <see cref="SeekOrigin"/> indicating the reference point used to obtain the new position</param>
		''' <returns>The new position within the stream</returns>
		Public Overrides Function Seek(offset As Long, origin As SeekOrigin) As Long
			Dim ret As Integer

			ret = MobileDevice.AFCFileRefSeek(phone.AFCHandle, handle, CUInt(offset), 0)
			Console.WriteLine("ret = {0}", ret)
			Return offset
		End Function

		''' <summary>
		''' Clears all buffers for this stream and causes any buffered data to be written to the underlying device. 
		''' </summary>
		Public Overrides Sub Flush()
			MobileDevice.AFCFlushData(phone.AFCHandle, handle)
		End Sub
		#End Region

		#Region "Static Methods"
		''' <summary>
		''' Opens an iPhoneFile stream on the specified path
		''' </summary>
		''' <param name="phone">A valid iPhone object</param>
		''' <param name="path">The file to open</param>
        ''' <param name="openmode__1">A <see cref="FileAccess"/> value that specifies the operations that can be performed on the file</param>
		''' <returns></returns>
		Public Shared Function Open(phone As iPhone, path As String, openmode__1 As FileAccess) As iPhoneFile
			Dim mode As OpenMode
			Dim ret As Integer
			Dim handle As Long
			Dim full_path As String

			mode = OpenMode.None
			Select Case openmode__1
				Case FileAccess.Read
					mode = OpenMode.Read
					Exit Select
				Case FileAccess.Write
					mode = OpenMode.Write
					Exit Select
				Case FileAccess.ReadWrite
					Throw New NotImplementedException("Read+Write not (yet) implemented")
			End Select

			full_path = phone.FullPath(phone.GetCurrentDirectory(), path)
			ret = MobileDevice.AFCFileRefOpen(phone.AFCHandle, full_path, CInt(mode), 0, handle)
			If ret <> 0 Then
				Throw New IOException("AFCFileRefOpen failed with error " & ret.ToString())
			End If
			Return New iPhoneFile(phone, handle, mode)
		End Function

		''' <summary>
		''' Opens a file for reading
		''' </summary>
		''' <param name="phone">A valid iPhone object</param>
		''' <param name="path">The file to be opened for reading</param>
		''' <returns>An unshared <c>iPhoneFile</c> object on the specified path with Write access. </returns>
		Public Shared Function OpenRead(phone As iPhone, path As String) As iPhoneFile
			Return iPhoneFile.Open(phone, path, FileAccess.Read)
		End Function

		''' <summary>
		''' Opens a file for writing
		''' </summary>
		''' <param name="phone">A valid iPhone object</param>
		''' <param name="path">The file to be opened for writing</param>
		''' <returns>An unshared <c>iPhoneFile</c> object on the specified path with Write access. </returns>
		Public Shared Function OpenWrite(phone As iPhone, path As String) As iPhoneFile
			Return iPhoneFile.Open(phone, path, FileAccess.Write)
		End Function
		#End Region
	End Class
End Namespace
