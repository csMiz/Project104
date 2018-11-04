''' <summary>
''' 鼠标判定区接口
''' </summary>
Public Interface IMouseArea

    Function IsInside(input As PointF2) As Boolean

    Event MouseDown(e As GameMouseEventArgs)
    Event MouseMove(e As GameMouseEventArgs)
    Event MouseUp(e As GameMouseEventArgs)

    Event MouseEnter()
    Event MouseLeave()

End Interface

Public Class GameMouseEventArgs
    Private Shared me_instance As New GameMouseEventArgs

    Public Position As PointI
    Public MouseButton As MouseButtons
    Public MouseWheel As Integer
    Public ReadOnly Property X
        Get
            Return Position.X
        End Get
    End Property
    Public ReadOnly Property Y
        Get
            Return Position.Y
        End Get
    End Property

    Public Shared Function FromMouseEventArgs(source As MouseEventArgs, windowsRect As Rectangle, cameraRect As PointI) As GameMouseEventArgs
        With me_instance
            .Position = New PointI(source.X * cameraRect.X / windowsRect.Width, source.Y * cameraRect.Y / windowsRect.Height)
            .MouseButton = source.Button
            .MouseWheel = source.Delta
        End With
        Return me_instance
    End Function

End Class
