Imports p104
Imports SharpDX.Direct2D1
Imports SharpDX.Mathematics.Interop

''' <summary>
''' Live2d图像，用于立绘等
''' </summary>
Public Class GameLive2dImage
    Implements IGameImage

    ''' <summary>
    ''' 图像描述
    ''' </summary>
    Public Description As String = vbNullString
    ''' <summary>
    ''' 整张图像的画布
    ''' </summary>
    Private FullImage As Bitmap1 = Nothing
    ''' <summary>
    ''' 绘图位置
    ''' </summary>
    Public PaintPosition As PointF2

    Public CanvasSize As SharpDX.Size2
    ''' <summary>
    ''' 分割后的所有图片素材
    ''' </summary>
    Public MeshAtlas As New List(Of SpriteLive2dImage)
    ''' <summary>
    ''' 用数组存放的MeshAtlas
    ''' </summary>
    Private ReadOnlyMesh As SpriteLive2dImage()

    Public Animations As New List(Of GameLive2dImageAnimation)

    Public Sub PaintFullImage(ByRef context As DeviceContext) Implements IGameImage.PaintFullImage
        For Each sprite In MeshAtlas
            sprite.PaintSourceSpriteImage(context, PaintPosition)
        Next
    End Sub

    Public Function GetImage() As Bitmap1 Implements IGameImage.GetImage
        Throw New NotImplementedException()
    End Function

    Public Function GetDrawRect() As RawRectangleF Implements IGameImage.GetDrawRect
        Throw New NotImplementedException()
    End Function
End Class

