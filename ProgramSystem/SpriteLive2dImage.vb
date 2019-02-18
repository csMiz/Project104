Imports SharpDX.Direct2D1
Imports SharpDX.Mathematics.Interop

''' <summary>
''' Live2d精灵图片类
''' </summary>
Public Class SpriteLive2dImage
    ''' <summary>
    ''' 四个控制点，包括原始点和变形后点
    ''' </summary>
    Public Vertices(3) As GameLive2dSpriteVertex
    ''' <summary>
    ''' 原始图像
    ''' </summary>
    Public SourceImage As Bitmap1 = Nothing

    Public Visible As Boolean = True

    Public Opacity As Single = 1.0F

    Private TransMatrix As MathMatrixS


    Public Sub Initialize(inputQuadrilateral As RawVector2(), inputBitmap As Bitmap1)
        If inputBitmap Is Nothing Then Throw New Exception("image is null!")
        For i = 0 To 3
            Vertices(i) = New GameLive2dSpriteVertex() With {.OldPosition = inputQuadrilateral(i)}
        Next
        SourceImage = inputBitmap
    End Sub

    Public Sub CalcultateTransformMatrix()

    End Sub

    Public Sub PaintSourceSpriteImage(context As DeviceContext, positionOffset As PointF2)
        context.DrawBitmap(SourceImage, New RawRectangleF(positionOffset.X + Vertices(0).OldPosition.X, positionOffset.Y + Vertices(0).OldPosition.Y, positionOffset.X + Vertices(2).OldPosition.X, positionOffset.Y + Vertices(2).OldPosition.Y), 1.0F, BitmapInterpolationMode.Linear)

    End Sub

End Class

