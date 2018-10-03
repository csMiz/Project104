''' <summary>
''' 字体助手类
''' </summary>
Public Class FontHelper
    Private Shared me_instance As FontHelper = Nothing

    Private TextRepository As New List(Of TextResource)
    Private FontRepository As New System.Drawing.Text.PrivateFontCollection
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

End Class

''' <summary>
''' 多语言文字资源类
''' </summary>
Public Class TextResource
    Private TextResourceId As Integer
    Private Items(2) As TextItem

End Class

Public Class TextItem
    Private Text As String
    Private UsingFontFamily As System.Drawing.FontFamily
    Private FontSize As Single
    Private FontColor As SolidBrush
    Private FontImage As Sharpdx.Direct2d1.Bitmap
    Private ImageSize As PointI

    Public Sub New(printText As String, rectSize As PointI)
        Text = printText
        ImageSize = rectSize
    End Sub

    Public Sub LoadFont(family As FontFamily, size As Single, color As SolidBrush)
        UsingFontFamily = family
        FontSize = size
        FontColor = color
    End Sub

    Public Sub GenerateImage(rt As Sharpdx.Direct2d1.RenderTarget)
        Dim bitmap As New System.Drawing.Bitmap(ImageSize.X, ImageSize.Y)
        Dim G As Graphics = Graphics.FromImage(bitmap)
        G.DrawString(Text, New Drawing.Font(UsingFontFamily, FontSize), FontColor, 0, 0)
        FontImage = GameResources.LoadBitmap(rt, bitmap)
    End Sub

    Public Function GetImage() As Sharpdx.Direct2d1.Bitmap
        Return FontImage
    End Function

End Class

Public Enum LocaleType As Short
    Others = 0
    S_Chinese = 1
    English = 2
End Enum