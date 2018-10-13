Imports p104
Imports SharpDX.Direct2D1
Imports SharpDX.Mathematics.Interop

''' <summary>
''' 静止图像资源（相对于Live2d）
''' </summary>
Public Class GameStaticImage
    Implements IGameImage

    Private Image As Bitmap1 = Nothing
    Private Rect As RawRectangleF
    Public Property Visible As Boolean = True

    ''' <summary>
    ''' 初始化
    ''' </summary>
    ''' <param name="sourceBitmap">图像</param>
    ''' <param name="sourceRect">原始矩形</param>
    Public Sub Initialize(sourceBitmap As Bitmap1, sourceRect As RawRectangleF)
        If sourceBitmap Is Nothing Then Throw New Exception("image is null!")
        Image = sourceBitmap
        Rect = sourceRect
    End Sub

    Public Sub PaintFullImage(ByRef context As DeviceContext) Implements IGameImage.PaintFullImage
        If Visible Then
            context.DrawBitmap(Image, Rect, 1.0F, BitmapInterpolationMode.Linear)
        End If
    End Sub

    Public Function GetImage() As Bitmap1 Implements IGameImage.GetImage
        Return Image
    End Function

    Public Function GetDrawRect() As RawRectangleF Implements IGameImage.GetDrawRect
        Return Rect
    End Function
End Class
