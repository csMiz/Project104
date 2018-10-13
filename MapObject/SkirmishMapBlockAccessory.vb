Imports SharpDX.Direct2D1
Imports SharpDX.Mathematics.Interop

''' <summary>
''' 六角地图装饰物类，用于树木、岩石等
''' </summary>
Public Class SkirmishMapBlockAccessory

    Private SourceRect As RawRectangleF
    'Private OutputRect As New RawRectangleF
    Private Image As Bitmap1

    Public Sub New(objectImage As Bitmap1, inputRect As RawRectangleF)
        Image = objectImage
        SourceRect = inputRect
    End Sub

    'Public Sub CalculateRect(offset As PointF2)
    '    OutputRect.Left = SourceRect.Left + offset.X
    '    OutputRect.Top = SourceRect.Top + offset.Y
    '    OutputRect.Right = SourceRect.Right + offset.X
    '    OutputRect.Bottom = SourceRect.Bottom + offset.Y
    'End Sub

    'Public Sub PaintImage(context As DeviceContext)
    '    context.DrawBitmap(Image, OutputRect, NOT_TRANSPARENT, BitmapInterpolationMode.Linear)
    'End Sub

    Public Sub PaintSourceImage(context As DeviceContext)
        context.DrawBitmap(Image, SourceRect, NOT_TRANSPARENT, BitmapInterpolationMode.Linear)
    End Sub

    Public Function GetSourceY() As Single
        Return SourceRect.Bottom
    End Function

End Class
