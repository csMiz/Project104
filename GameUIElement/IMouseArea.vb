''' <summary>
''' 鼠标判定区接口
''' </summary>
Public Interface IMouseArea

    Function IsInside(input As PointF2) As Boolean

    Event MouseDown(e As MouseEventArgs)
    Event MouseMove(e As MouseEventArgs)
    Event MouseUp(e As MouseEventArgs)

    Event MouseEnter()
    Event MouseLeave()

End Interface
