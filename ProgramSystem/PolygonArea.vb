Imports p104
Imports System.Math

Public Class PolygonArea
    Implements IMouseArea

    Public Points As New List(Of PointF2)

    Public Event MouseEnter(e As GameMouseEventArgs) Implements IMouseArea.MouseEnter
    Public Event MouseLeave(e As GameMouseEventArgs) Implements IMouseArea.MouseLeave
    Public Event MouseDown(e As GameMouseEventArgs) Implements IMouseArea.MouseDown
    Public Event MouseMove(e As GameMouseEventArgs) Implements IMouseArea.MouseMove
    Public Event MouseUp(e As GameMouseEventArgs) Implements IMouseArea.MouseUp
    Public Event MouseWheel(e As GameMouseEventArgs) Implements IMouseArea.MouseWheel
    Public Event GlobalMouseMove(e As GameMouseEventArgs) Implements IMouseArea.GlobalMouseMove

    Public Function IsInside(input As PointF2) As Boolean Implements IMouseArea.IsInside
        'http://www.html-js.com/article/1538
        '回转数法

        If Points.Count < 3 Then Throw New Exception("非法的多边形")

        Dim total As Single = 0    '弧度
        Dim angles As New List(Of Double)
        For i = 0 To Points.Count - 1
            Dim angle As Double = Atan2(Points(i).Y - input.Y, Points(i).X - input.X)
            angles.Add(angle)
        Next
        For i = 0 To Points.Count - 2
            Dim delta As Double = angles(i) - angles(i + 1)
            If delta <= -PI Then
                delta += 2 * PI
            ElseIf delta >= PI Then
                delta -= 2 * PI
            End If
            total += delta
        Next
        '最后封闭
        Dim deltaLast As Double = angles(Points.Count - 1) - angles(0)
        If deltaLast <= -PI Then
            deltaLast += 2 * PI
        ElseIf deltaLast >= PI Then
            deltaLast -= 2 * PI
        End If
        total += deltaLast

        Return (Abs(total - 2 * PI) <= 0.1)

    End Function
End Class
