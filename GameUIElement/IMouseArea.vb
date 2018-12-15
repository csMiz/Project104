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


