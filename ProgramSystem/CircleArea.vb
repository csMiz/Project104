Imports p104

Public Class CircleArea
    Implements IMouseArea

    Public Centre As PointF2
    Public Radius As Single

    Public Function IsInside(input As PointF2) As Boolean Implements IMouseArea.IsInside
        Return (MathHelper.GetDistance(input, Me.Centre) <= Me.Radius)
    End Function
End Class
