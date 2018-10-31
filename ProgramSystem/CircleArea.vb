Imports p104

Public Class CircleArea
    Implements IMouseArea

    Public Centre As PointF2
    Public Radius As Single
    Public Event MouseDown(e As MouseEventArgs) Implements IMouseArea.MouseDown
    Public Event MouseMove(e As MouseEventArgs) Implements IMouseArea.MouseMove
    Public Event MouseUp(e As MouseEventArgs) Implements IMouseArea.MouseUp
    Public Event MouseEnter() Implements IMouseArea.MouseEnter
    Public Event MouseLeave() Implements IMouseArea.MouseLeave

    Public Function IsInside(input As PointF2) As Boolean Implements IMouseArea.IsInside
        Return (MathHelper.GetDistance(input, Me.Centre) <= Me.Radius)
    End Function
End Class
