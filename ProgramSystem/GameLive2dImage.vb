Imports p104
Imports SharpDX.Direct2D1
Imports SharpDX.Mathematics.Interop

''' <summary>
''' Live2d图像，用于立绘等
''' </summary>
Public Class GameLive2dImage
    Implements IGameImage

    ''' <summary>
    ''' 原始图像
    ''' </summary>
    Private FullImage As Bitmap1 = Nothing
    ''' <summary>
    ''' 原始坐标
    ''' </summary>
    Private FullPos As RawVector2
    ''' <summary>
    ''' 分割后的所有图片素材
    ''' </summary>
    Private MeshAtlas As New List(Of SpriteLive2dImage)

    Public Sub PaintFullImage(ByRef context As DeviceContext) Implements IGameImage.PaintFullImage
        For Each sprite In MeshAtlas
            sprite.PaintSpriteImage(context)
        Next
    End Sub

    Public Function GetImage() As Bitmap1 Implements IGameImage.GetImage
        Throw New NotImplementedException()
    End Function

    Public Function GetDrawRect() As RawRectangleF Implements IGameImage.GetDrawRect
        Throw New NotImplementedException()
    End Function
End Class

''' <summary>
''' Live2d精灵图片类
''' </summary>
Public Class SpriteLive2dImage
    Public SourceVertex(2) As RawVector2
    Public SourceImage As Bitmap1 = Nothing
    Public DrawRect As RawRectangleF

    Public PaintVertex(2) As RawVector2
    Public OutputImage As Bitmap1 = Nothing
    Public OutputRect As RawRectangleF

    Public Sub Initialize(inputTriangle As RawVector2(), inputBitmap As Bitmap1)
        If inputBitmap Is Nothing Then Throw New Exception("image is null!")
        SourceVertex = inputTriangle
        SourceImage = inputBitmap
        CalculateRect()
    End Sub

    ''' <summary>
    ''' 通过三个顶点计算外围矩形
    ''' </summary>
    Public Sub CalculateRect()

    End Sub

    Public Sub Transform()

    End Sub

    Public Sub PaintSpriteImage(context As DeviceContext)
        context.DrawBitmap(OutputImage, OutputRect, 1.0F, BitmapInterpolationMode.Linear)
    End Sub

End Class
