Imports SharpDX.Direct2D1
Imports SharpDX.Mathematics.Interop

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

