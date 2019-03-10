Imports System.IO
Imports System.Text.RegularExpressions
Imports p104
Imports SharpDX
''' <summary>
''' 字体助手类
''' </summary>
Public Class FontHelper
    Private Shared me_instance As FontHelper = Nothing
    ''' <summary>
    ''' 多语言文本仓库，TextRepo(locale)(index)
    ''' </summary>
    Public TextRepository As New List(Of List(Of String))
    Public LocaleList As New List(Of String)
    Public LocaleCodeList As New List(Of Byte())

    Public FontRepository As New System.Drawing.Text.PrivateFontCollection
    Private FontForGDI As New List(Of System.Drawing.FontFamily)

    Private Sub New()
    End Sub

    ''' <summary>
    ''' 单例
    ''' </summary>
    Public Shared Function Instance() As FontHelper
        If me_instance Is Nothing Then me_instance = New FontHelper
        Return me_instance
    End Function

    Public Sub AddFontFile(path As String, fontName As String)
        FontRepository.AddFontFile(path)
        FontForGDI.Add(New Drawing.FontFamily(fontName, FontRepository))
    End Sub

    Public Function GetFontFamily(index As Short) As System.Drawing.FontFamily
        Return FontForGDI(index)
    End Function

    ''' <summary>
    ''' 从文本解析多语言文字资源
    ''' </summary>
    ''' <param name="source">多语言文本源，需要注意文字编码和换行符的问题</param>
    Public Sub LoadLocaleTextResourcesFromText(source As String)
        Dim result As New List(Of String)
        Dim lines() As String = Regex.Split(source, vbCrLf)
        If lines.Count >= 2 Then
            Dim languageName As String = "#Undefined"
            Dim languageCode(3) As Byte
            If lines(0).Remove(1) = "#" Then
                Dim languageDefinition() As String = Regex.Split(lines(0).Substring(1), "#")
                languageName = languageDefinition(0)
                Dim languageCodeSource() As Char = languageDefinition(1).ToCharArray
                For i = 0 To 3
                    languageCode(i) = AscW(languageCodeSource(i))
                Next
            End If
            LocaleList.Add(languageName)
            LocaleCodeList.Add(languageCode)
            For i = 1 To lines.Count - 1
                Dim eachLine As String = lines(i)
                If eachLine.Contains("|") Then
                    Dim parts() As String = Regex.Split(eachLine, ESCAPE_VERTICAL_LINE)
                    'Dim tmpId As Integer = CInt(parts(0))
                    result.Add(parts(1))
                End If
            Next
            TextRepository.Add(result)
        End If
    End Sub

    Public Sub LoadTextFromFiles()
        Dim dirInfo As New System.IO.DirectoryInfo(Application.StartupPath & "\Resources\Languages\")
        Dim allFiles() As System.IO.FileInfo = dirInfo.GetFiles
        For Each file As System.IO.FileInfo In allFiles
            Dim readStream As System.IO.FileStream = file.OpenRead
            Dim tmpFileContent As String = vbNullString
            Using sr As StreamReader = New StreamReader(readStream, System.Text.Encoding.Unicode)
                tmpFileContent = sr.ReadToEnd()
            End Using
            readStream.Close()
            readStream.Dispose()
            Call LoadLocaleTextResourcesFromText(tmpFileContent)
        Next
    End Sub

    Public Function FindLanguageIndex(code As String) As Integer
        'TODO
        Throw New NotImplementedException
    End Function

    Public Function GetLanguageCode(index As Integer) As Byte()
        Return LocaleCodeList(index)
    End Function

End Class

Public Class TextItem
    Public Text As String
    Public UsingFontFamily As System.Drawing.FontFamily
    Public FontSize As Single
    Public FontColor As SolidBrush
    Public ReferenceBackgroundColor As System.Drawing.Color
    Public FontImage As SharpDX.Direct2D1.Bitmap
    Private ImageSize As PointI

    Public Sub New(printText As String, rectSize As PointI)
        Text = printText
        ImageSize = rectSize
    End Sub

    Public Sub LoadFont(family As FontFamily, size As Single, color As SolidBrush, refColor As Color)
        UsingFontFamily = family
        FontSize = size
        FontColor = color
        ReferenceBackgroundColor = refColor
    End Sub

    Public Sub GenerateImage(rt As SharpDX.Direct2D1.DeviceContext)
        Dim bitmap As New System.Drawing.Bitmap(ImageSize.X, ImageSize.Y)
        Dim G As Graphics = Graphics.FromImage(bitmap)
        G.Clear(ReferenceBackgroundColor)
        G.DrawString(Text, New Drawing.Font(UsingFontFamily, FontSize), FontColor, 0, 0)
        G.Dispose()    '存在黑边的问题
        If Me.FontImage IsNot Nothing Then Me.FontImage.Dispose()
        bitmap.MakeTransparent(ReferenceBackgroundColor)
        'FontImage = GameResources.LoadBitmap(rt, bitmap)
        '改用wic
        Dim tmpStream As New System.IO.MemoryStream()
        bitmap.Save(tmpStream, Imaging.ImageFormat.Png)
        FontImage = LoadBitmapUsingWIC(rt, tmpStream)
        tmpStream.Dispose()
        bitmap.Dispose()
    End Sub

End Class

Public Enum LocaleType As Short
    Others = 0
    S_Chinese = 1
    English = 2
    T_Chinese = 3

End Enum

'Public Class MyFontContext
'    Private context As MyFontContext
'    Private g_dwriteFactory As DirectWrite.Factory
'    Private hr As Integer
'    Private cKeys As New List(Of UInteger)



'    Public Sub New()
'        g_dwriteFactory.UnregisterFontCollectionLoader(MyFontCollectionLoader.GetLoader())
'    End Sub
'    Public Sub New(pFactory As DirectWrite.Factory)
'        hr = 0
'        g_dwriteFactory = pFactory
'    End Sub
'    Public Function Initialize() As Integer
'        If hr = 0 Then
'            hr = InitInternal()
'        End If
'        Return hr
'    End Function
'    Private Function InitInternal() As Integer
'        Dim r As Integer = 1        '1 for success
'        If (Not MyFontCollectionLoader.IsLoaderInitialized()) Then
'            Return -1       '-1 for fail
'        End If
'        'Register our custom loader with the factory object.
'        g_dwriteFactory.RegisterFontCollectionLoader(MyFontCollectionLoader.GetLoader())
'        Return r
'    End Function
'    Public Function CreateFontCollection(ByRef newCollection As List(Of String), ByRef result As DirectWrite.FontCollection) As Integer
'        result = Nothing
'        Dim r As Integer = 1

'        'save new collection in MFFontGlobalsfontCollections vector
'        Dim collectionKey As UInteger = MyFontGlobals.Push(newCollection)
'        cKeys.Add(collectionKey)

'        Dim fontCollectionKey As DataPointer = New DataPointer(cKeys.Last, 4)

'        hr = Initialize()
'        If (hr = -1) Then Return hr

'        result = New DirectWrite.FontCollection(g_dwriteFactory, MyFontCollectionLoader.GetLoader(), fontCollectionKey)

'        Return hr
'    End Function

'End Class

'Public Class MyFontCollectionLoader
'    Implements DirectWrite.FontCollectionLoader

'    Private refCount As ULong
'    Private Shared me_instance As New MyFontCollectionLoader

'    Public Shared Function GetLoader() As DirectWrite.FontCollectionLoader
'        Return me_instance
'    End Function
'    Public Shared Function IsLoaderInitialized() As Boolean
'        Return (me_instance IsNot Nothing)
'    End Function

'    Public Sub New()
'        refCount = 0
'    End Sub

'    Public Property Shadow As IDisposable Implements ICallbackable.Shadow
'        Get
'            Return Nothing 'not implemented
'        End Get
'        Set(value As IDisposable)
'            Return 'not implemented
'        End Set
'    End Property

'    Public Function CreateEnumeratorFromKey(factory As DirectWrite.Factory, collectionKey As DataPointer) As DirectWrite.FontFileEnumerator Implements DirectWrite.FontCollectionLoader.CreateEnumeratorFromKey
'        Throw New NotImplementedException()
'    End Function

'    Public Function QueryInterface(ByRef guid As Guid, ByRef comObject As IntPtr) As Result Implements IUnknown.QueryInterface
'        Dim gch As Runtime.InteropServices.GCHandle = Runtime.InteropServices.GCHandle.Alloc(Me, Runtime.InteropServices.GCHandleType.Pinned)
'        comObject = gch.AddrOfPinnedObject
'        gch.Free()
'        AddReference()
'        Return 1
'    End Function

'    Public Function AddReference() As Integer Implements IUnknown.AddReference
'        refCount += 1
'        Return refCount
'    End Function

'    Public Function Release() As Integer Implements IUnknown.Release
'        refCount -= 1
'        Dim newCount As ULong = refCount
'        If (newCount = 0) Then
'            'Me.Dispose()
'        End If
'        Return newCount
'    End Function

'#Region "IDisposable Support"
'    Private disposedValue As Boolean ' 要检测冗余调用

'    ' IDisposable
'    Protected Overridable Sub Dispose(disposing As Boolean)
'        If Not disposedValue Then
'            If disposing Then
'                ' TODO: 释放托管状态(托管对象)。
'            End If

'            ' TODO: 释放未托管资源(未托管对象)并在以下内容中替代 Finalize()。
'            ' TODO: 将大型字段设置为 null。
'        End If
'        disposedValue = True
'    End Sub

'    ' TODO: 仅当以上 Dispose(disposing As Boolean)拥有用于释放未托管资源的代码时才替代 Finalize()。
'    'Protected Overrides Sub Finalize()
'    '    ' 请勿更改此代码。将清理代码放入以上 Dispose(disposing As Boolean)中。
'    '    Dispose(False)
'    '    MyBase.Finalize()
'    'End Sub

'    ' Visual Basic 添加此代码以正确实现可释放模式。
'    Public Sub Dispose() Implements IDisposable.Dispose
'        ' 请勿更改此代码。将清理代码放入以上 Dispose(disposing As Boolean)中。
'        Dispose(True)
'        ' TODO: 如果在以上内容中替代了 Finalize()，则取消注释以下行。
'        ' GC.SuppressFinalize(Me)
'    End Sub
'#End Region

'End Class

'Public Class MyFontGlobals
'    Public Shared fontCollections As New List(Of String)

'    Public Sub New()

'    End Sub
'    Public Shared Function Push(addCollection As List(Of String)) As UInteger
'        fontCollections.AddRange(addCollection)
'        Return fontCollections.Count
'    End Function

'End Class