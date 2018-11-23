''' <summary>
''' 鼠标判定区接口
''' </summary>
Public Interface IMouseArea

    Function IsInside(input As PointF2) As Boolean

    Event MouseDown(e As GameMouseEventArgs)
    Event MouseMove(e As GameMouseEventArgs)
    Event MouseUp(e As GameMouseEventArgs)

    Event MouseEnter(e As GameMouseEventArgs)
    Event MouseLeave(e As GameMouseEventArgs)

    Event MouseWheel(e As GameMouseEventArgs)

    Event GlobalMouseMove(e As GameMouseEventArgs)

End Interface

Public Class GameMouseEventArgs

    Public Position As New PointI(0, 0)
    Public MouseButton As MouseButtons = MouseButtons.None
    Public MouseWheel As Integer = 0
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
    Public Deliver As Boolean = True

    Public Shared Function FromMouseEventArgs(source As MouseEventArgs, windowsRect As Rectangle, cameraRect As PointI) As GameMouseEventArgs
        Dim result As New GameMouseEventArgs With {
            .Position = New PointI(source.X * cameraRect.X / windowsRect.Width, source.Y * cameraRect.Y / windowsRect.Height),
            .MouseButton = source.Button,
            .MouseWheel = source.Delta}
        Return result
    End Function

    Public Function PrintPositionString() As String
        Return "(" & Me.Position.X & ", " & Me.Position.Y & ")"
    End Function

End Class
