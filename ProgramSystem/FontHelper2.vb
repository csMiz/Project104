' -----------------------------------------
' Copyright (c) 2019 All Rights Reserved.
' 
' Filename: FontHelper2
' Author: Miz
' Date: 2019/3/9 22:09:41
' -----------------------------------------

Imports SharpDX
Imports SharpDX.DirectWrite

''' <summary>
''' 使用DirectWrite的字体助手类ver.2
''' </summary>
Public Class FontHelper2

    Private Shared me_instance As FontHelper2 = Nothing

    Private DWFactory As New Factory

    Public CurrentResourceFontLoader As CustomFontLoader

    Public CurrentFontCollection As FontCollection


    Private Sub New()
        InitializeCustomFontComponents()
    End Sub

    Public Sub InitializeCustomFontComponents()

        CurrentResourceFontLoader = New CustomFontLoader(DWFactory)

        CurrentFontCollection = New FontCollection(DWFactory, CurrentResourceFontLoader, CurrentResourceFontLoader.Key)

    End Sub

    Public Shared Function Instance() As FontHelper2
        If me_instance Is Nothing Then me_instance = New FontHelper2
        Return me_instance
    End Function

    Public Function GetSystemTextFormat(name As String, size As Single, Optional fontWeight As FontWeight = FontWeight.Normal, Optional fontStyle As FontStyle = FontStyle.Normal) As TextFormat
        Dim result As New TextFormat(DWFactory, name, fontWeight, fontStyle, size)
        Return result
    End Function

    Public Function GetSystemTextLayout(text As String, name As String, size As Single, maxSize As PointF2) As TextLayout
        Dim tmpFont As New TextFormat(DWFactory, name, size)
        Dim result As New TextLayout(DWFactory, text, tmpFont, maxSize.X, maxSize.Y)
        Return result
    End Function

    Public Function GetCustomTextFormat(name As String, size As Single, Optional fontWeight As FontWeight = FontWeight.Normal, Optional fontStyle As FontStyle = FontStyle.Normal) As TextFormat
        Dim tmpFont As New TextFormat(DWFactory, name, CurrentFontCollection, fontWeight, fontStyle, FontStretch.Normal, size)
        Return tmpFont
    End Function

    Public Function GetCustomTextLayout(text As String, name As String, size As Single, maxSize As PointF2) As TextLayout
        Dim tmpFont As New TextFormat(DWFactory, name, CurrentFontCollection, FontWeight.Normal, FontStyle.Normal, FontStretch.Normal, size)
        Dim result As New TextLayout(DWFactory, text, tmpFont, maxSize.X, maxSize.Y)
        Return result
    End Function



End Class


''' <summary>
''' Revised from Sharpdx-sample
''' </summary>
Public Class CustomFontLoader
    Inherits CallbackBase
    Implements FontCollectionLoader
    Implements FontFileLoader

    Private ReadOnly _fontStreams As New List(Of CustomFontFileStream)
    Private ReadOnly _enumerators As New List(Of CustomFontFileEnumerator)
    Private ReadOnly _keyStream As DataStream
    Private ReadOnly _factory As Factory

    ''' <summary>
    ''' Initializes a New instance of the <see cref="CustomFontLoader"/> class.
    ''' </summary>
    ''' <param name="factory">The factory.</param>
    Public Sub New(factory As Factory)
        _factory = factory

        '在这里导入自定义字体的二进制流
        Dim data As Byte()
        data = My.Resources.P104_Font1
        Dim tmpStream As DataStream = New DataStream(data.Length, True, True)
        tmpStream.Write(data, 0, data.Length)
        tmpStream.Position = 0
        _fontStreams.Add(New CustomFontFileStream(tmpStream))

        'Sample原版代码：
        'For Each name As String In GetType(CustomFontLoader).Assembly.GetManifestResourceNames()
        '    If name.EndsWith(".ttf") Then
        '        Dim fontBytes As Byte() = Utilities.ReadStream(GetType(CustomFontLoader).Assembly.GetManifestResourceStream(name))
        '        Dim stream As DataStream = New DataStream(fontBytes.Length, True, True)
        '        stream.Write(fontBytes, 0, fontBytes.Length)
        '        stream.Position = 0
        '        _fontStreams.Add(New CustomFontFileStream(stream))
        '    End If
        'Next

        ' Build a Key storage that stores the index of the font
        _keyStream = New DataStream(Len(New Integer) * _fontStreams.Count, True, True)
        For i As Integer = 0 To _fontStreams.Count - 1
            _keyStream.Write(i)
        Next
        _keyStream.Position = 0

        ' Register the 
        _factory.RegisterFontFileLoader(Me)
        _factory.RegisterFontCollectionLoader(Me)

    End Sub

    ''' <summary>
    ''' Gets the key used to identify the FontCollection as well as storing index for fonts.
    ''' </summary>
    ''' <value>The key.</value>
    Public ReadOnly Property Key As DataStream
        Get
            Return _keyStream
        End Get
    End Property

    Public Property Shadow As IDisposable Implements ICallbackable.Shadow

    ''' <summary>
    ''' Creates a font file enumerator object that encapsulates a collection of font files. The font system calls back to this interface to create a font collection.
    ''' </summary>
    ''' <param name="factory">Pointer to the <see cref="SharpDX.DirectWrite.Factory"/> object that was used to create the current font collection.</param>
    ''' <param name="collectionKey">A font collection key that uniquely identifies the collection of font files within the scope of the font collection loader being used. The buffer allocated for this key must be at least  the size, in bytes, specified by collectionKeySize.</param>
    ''' <returns>
    ''' a reference to the newly created font file enumerator.
    ''' </returns>
    ''' <unmanaged>HRESULT IDWriteFontCollectionLoader:CreateEnumeratorFromKey([None] IDWriteFactory* factory,[In, Buffer] const void* collectionKey,[None] int collectionKeySize,[Out] IDWriteFontFileEnumerator** fontFileEnumerator)</unmanaged>
    Public Function CreateEnumeratorFromKey(factory As Factory, collectionKey As DataPointer) As FontFileEnumerator Implements FontCollectionLoader.CreateEnumeratorFromKey
        Dim enumerator = New CustomFontFileEnumerator(factory, Me, collectionKey)
        _enumerators.Add(enumerator)

        Return enumerator
    End Function

    ''' <summary>
    ''' Creates a font file stream object that encapsulates an open file resource.
    ''' </summary>
    ''' <param name="fontFileReferenceKey">A reference to a font file reference key that uniquely identifies the font file resource within the scope of the font loader being used. The buffer allocated for this key must at least be the size, in bytes, specified by  fontFileReferenceKeySize.</param>
    ''' <returns>
    ''' a reference to the newly created <see cref="SharpDX.DirectWrite.FontFileStream"/> object.
    ''' </returns>
    ''' <remarks>
    ''' The resource Is closed when the last reference to fontFileStream Is released.
    ''' </remarks>
    ''' <unmanaged>HRESULT IDWriteFontFileLoader:CreateStreamFromKey([In, Buffer] const void* fontFileReferenceKey,[None] int fontFileReferenceKeySize,[Out] IDWriteFontFileStream** fontFileStream)</unmanaged>
    Public Function CreateStreamFromKey(fontFileReferenceKey As DataPointer) As FontFileStream Implements FontFileLoader.CreateStreamFromKey
        Dim index As Integer = Utilities.Read(Of Integer)(fontFileReferenceKey.Pointer)
        Return _fontStreams(index)
    End Function

    Public Function IUnknown_QueryInterface(ByRef guid As Guid, ByRef comObject As IntPtr) As Result Implements IUnknown.QueryInterface
        Return QueryInterface(guid, comObject)
    End Function

    Public Function IUnknown_AddReference() As Integer Implements IUnknown.AddReference
        Return AddReference()
    End Function

    Public Function IUnknown_Release() As Integer Implements IUnknown.Release
        Return MyBase.Release()
    End Function


End Class

''' <summary>
''' Revised from Sharpdx-sample
''' </summary>
Public Class CustomFontFileStream
    Inherits CallbackBase
    Implements FontFileStream

    Private ReadOnly _stream As DataStream

    ''' <summary>
    ''' 不知道SharpDX出了什么问题，引用计数器会把引用减到0导致此对象被释放，总之用一个变量标记一下，不让此对象引用计数器减到零而莫名其妙被清掉
    ''' <para>错误代码见 https://github.com/sharpdx/SharpDX/blob/master/Source/SharpDX/CallbackBase.cs </para>
    ''' </summary>
    Private ref_mark As Integer

    Public Sub New(stream As DataStream)
        Me._stream = stream
    End Sub

    ''' <summary>
    ''' Reads a fragment from a font file.
    ''' </summary>
    ''' <param name="fragmentStart">When this method returns, contains an address of a  reference to the start of the font file fragment.  This parameter Is passed uninitialized.</param>
    ''' <param name="fileOffset">The offset of the fragment, in bytes, from the beginning of the font file.</param>
    ''' <param name="fragmentSize">The size of the file fragment, in bytes.</param>
    ''' <param name="fragmentContext">When this method returns, contains the address of</param>
    ''' <remarks>
    ''' Note that ReadFileFragment implementations must check whether the requested font file fragment Is within the file bounds. Otherwise, an error should be returned from ReadFileFragment.   {{DirectWrite}} may invoke <see cref="SharpDX.DirectWrite.FontFileStream"/> methods on the same object from multiple threads simultaneously. Therefore, ReadFileFragment implementations that rely on internal mutable state must serialize access to such state across multiple threads. For example, an implementation that uses separate Seek And Read operations to read a file fragment must place the code block containing Seek And Read calls under a lock Or a critical section.
    ''' </remarks>
    ''' <unmanaged>HRESULT IDWriteFontFileStream:ReadFileFragment([Out, Buffer] Const void** fragmentStart, [None] __int64 fileOffset, [None] __int64 fragmentSize, [Out] void** fragmentContext)</unmanaged>
    Public Sub ReadFileFragment(ByRef fragmentStart As IntPtr, fileOffset As Long, fragmentSize As Long, ByRef fragmentContext As IntPtr) Implements FontFileStream.ReadFileFragment
        SyncLock (Me)
            fragmentContext = IntPtr.Zero
            _stream.Position = fileOffset
            fragmentStart = _stream.PositionPointer
        End SyncLock
    End Sub

    ''' <summary>
    ''' Releases a fragment from a file.
    ''' </summary>
    ''' <param name="fragmentContext">A reference to the client-defined context of a font fragment returned from {{ReadFileFragment}}.</param>
    ''' <unmanaged>void IDWriteFontFileStream:ReleaseFileFragment([None] void* fragmentContext)</unmanaged>
    Public Sub ReleaseFileFragment(fragmentContext As IntPtr) Implements FontFileStream.ReleaseFileFragment
        ' Nothing to release. No context are used
    End Sub

    ''' <summary>
    ''' Obtains the total size of a file.
    ''' </summary>
    ''' <returns>the total size of the file.</returns>
    ''' <remarks>
    ''' Implementing GetFileSize() for asynchronously loaded font files may require downloading the complete file contents. Therefore, this method should be used only for operations that either require a complete font file to be loaded (for example, copying a font file) Or that need to make decisions based on the value of the file size (for example, validation against a persisted file size).
    ''' </remarks>
    ''' <unmanaged>HRESULT IDWriteFontFileStream:GetFileSize([Out] __int64* fileSize)</unmanaged>
    Public Function GetFileSize() As Long Implements FontFileStream.GetFileSize
        Return _stream.Length
    End Function

    ''' <summary>
    ''' Obtains the last modified time of the file.
    ''' </summary>
    ''' <returns>
    ''' the last modified time of the file in the format that represents the number of 100-nanosecond intervals since January 1, 1601 (UTC).
    ''' </returns>
    ''' <remarks>
    ''' The "last modified time" Is used by DirectWrite font selection algorithms to determine whether one font resource Is more up to date than another one.
    ''' </remarks>
    ''' <unmanaged>HRESULT IDWriteFontFileStream:GetLastWriteTime([Out] __int64* lastWriteTime)</unmanaged>
    Public Function GetLastWriteTime() As Long Implements FontFileStream.GetLastWriteTime
        Return 0
    End Function

    Private Function IUnknown_QueryInterface(ByRef guid As Guid, ByRef comObject As IntPtr) As Result Implements IUnknown.QueryInterface
        Return QueryInterface(guid, comObject)
    End Function

    Private Function IUnknown_AddReference() As Integer Implements IUnknown.AddReference
        Dim r As Integer = AddReference()
        Debug.WriteLine("addref called! ref:" & r)
        ref_mark = r
        Return r
    End Function

    Private Function IUnknown_Release() As Integer Implements IUnknown.Release

        If ref_mark <> 1 Then
            Dim r As Integer = Release()
            Debug.WriteLine("release called! ref:" & r)
            ref_mark = r
            Return r
        End If
        Debug.WriteLine("release BLOCKED! ref:1")
        Return 1
    End Function
End Class

''' <summary>
''' Revised from Sharpdx-sample
''' </summary>
Public Class CustomFontFileEnumerator
    Inherits CallbackBase
    Implements FontFileEnumerator

    Private _factory As Factory
    Private _loader As FontFileLoader
    Private keyStream As DataStream
    Private _currentFontFile As FontFile

    Public Sub New(factory As Factory, loader As FontFileLoader, key As DataPointer)
        _factory = factory
        _loader = loader
        keyStream = New DataStream(key.Pointer, key.Size, True, False)
    End Sub

    ''' <summary>
    ''' Gets a reference to the current font file.
    ''' </summary>
    ''' <value></value>
    ''' <returns>a reference to the newly created <see cref="SharpDX.DirectWrite.FontFile"/> object.</returns>
    ''' <unmanaged>HRESULT IDWriteFontFileEnumerator:GetCurrentFontFile([Out] IDWriteFontFile** fontFile)</unmanaged>
    Public ReadOnly Property CurrentFontFile As FontFile Implements FontFileEnumerator.CurrentFontFile
        Get
            CType(_currentFontFile, IUnknown).AddReference()
            Return _currentFontFile
        End Get
    End Property

    ''' <summary>
    ''' Advances to the next font file in the collection. When it Is first created, the enumerator Is positioned before the first element of the collection And the first call to MoveNext advances to the first file.
    ''' </summary>
    ''' <returns>
    ''' the value TRUE if the enumerator advances to a file; otherwise, FALSE if the enumerator advances past the last file in the collection.
    ''' </returns>
    ''' <unmanaged>HRESULT IDWriteFontFileEnumerator:MoveNext([Out] BOOL* hasCurrentFile)</unmanaged>
    Public Function MoveNext() As Boolean Implements FontFileEnumerator.MoveNext
        Dim result As Boolean = keyStream.RemainingLength <> 0
        If result Then
            If _currentFontFile IsNot Nothing Then _currentFontFile.Dispose()
            _currentFontFile = New FontFile(_factory, keyStream.PositionPointer, 4, _loader)
            keyStream.Position += 4
        End If
        Return result
    End Function

    Private Function IUnknown_QueryInterface(ByRef guid As Guid, ByRef comObject As IntPtr) As Result Implements IUnknown.QueryInterface
        Return QueryInterface(guid, comObject)
    End Function

    Private Function IUnknown_AddReference() As Integer Implements IUnknown.AddReference
        Return AddReference()
    End Function

    Private Function IUnknown_Release() As Integer Implements IUnknown.Release
        Return Release()
    End Function
End Class


