Imports p104
Imports SharpDX.Direct2D1
Imports SharpDX.Mathematics.Interop
''' <summary>
''' Live2d动画类
''' </summary>
Public Class GameLive2dAnimation
    Implements IGameImage

    Private Frames As New List(Of Bitmap1)
    Private PlayingIndex As Integer = 0

    Public Sub AddFrame(frame As Bitmap1)
        Frames.Add(frame)
    End Sub

    Public Sub ClearFrames()
        Frames.Clear()
    End Sub

    Public Sub PaintFullImage(ByRef context As DeviceContext) Implements IGameImage.PaintFullImage
        Throw New NotImplementedException()
    End Sub

    Public Function GetImage() As Bitmap1 Implements IGameImage.GetImage
        Throw New NotImplementedException()
    End Function

    Public Function GetDrawRect() As RawRectangleF Implements IGameImage.GetDrawRect
        Throw New NotImplementedException()
    End Function
End Class
