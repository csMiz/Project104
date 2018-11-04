Imports p104

Public Class CircleArea
    Implements IMouseArea

    Public Centre As PointF2
    Public Radius As Single

    Public Event MouseEnter() Implements IMouseArea.MouseEnter
    Public Event MouseLeave() Implements IMouseArea.MouseLeave
    Public Event MouseDown(e As GameMouseEventArgs) Implements IMouseArea.MouseDown
    Public Event MouseMove(e As GameMouseEventArgs) Implements IMouseArea.MouseMove
    Public Event MouseUp(e As GameMouseEventArgs) Implements IMouseArea.MouseUp

    Public Function IsInside(input As PointF2) As Boolean Implements IMouseArea.IsInside
        Return (MathHelper.GetDistance(input, Me.Centre) <= Me.Radius)
    End Function
End Class
