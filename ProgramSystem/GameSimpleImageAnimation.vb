Imports p104
Imports SharpDX.Direct2D1
Imports SharpDX.Mathematics.Interop
''' <summary>
''' 普通图片动画类
''' </summary>
Public Class GameSimpleImageAnimation
    Implements IGameImage

    Private Frames As New List(Of Bitmap1)
    Private PlayingIndex As Integer = 0
    Public Rect As RawRectangleF = Nothing

    Private FrameCount As Integer = 0

    Public Sub AddFrame(frame As Bitmap1)
        Frames.Add(frame)
        FrameCount = Frames.Count
    End Sub

    Public Sub ClearFrames()
        Frames.Clear()
    End Sub

    Public Sub PaintFullImage(ByRef context As DeviceContext) Implements IGameImage.PaintFullImage
        context.DrawBitmap(Frames(PlayingIndex), Me.Rect, NOT_TRANSPARENT, BitmapInterpolationMode.Linear)
        Call Me.GoNextFrame()
    End Sub

    Public Sub GoNextFrame()
        Me.PlayingIndex += 1
        If Me.PlayingIndex = FrameCount Then Me.PlayingIndex = 0
    End Sub

    Public Function GetImage() As Bitmap1 Implements IGameImage.GetImage
        Return Me.Frames(Me.PlayingIndex)
    End Function

    Public Function GetDrawRect() As RawRectangleF Implements IGameImage.GetDrawRect
        Return Me.Rect
    End Function
End Class
