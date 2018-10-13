Imports System.Math

''' <summary>
''' 数学助手类
''' </summary>
Public Class MathHelper

    Private Shared Rnd As Random = New Random()

    ''' <summary>
    ''' 计算两点间距离
    ''' </summary>
    Public Shared Function GetDistance(a As PointF2, b As PointF2) As Single
        Return Sqrt((a.X - b.X) ^ 2 + (a.Y - b.Y) ^ 2)
    End Function

    Public Shared Function GetRandom() As Double
        Return Rnd.NextDouble
    End Function

End Class

''' <summary>
''' 简化的PointF结构体
''' </summary>
Public Structure PointF2
    Public Property X As Single
    Public Property Y As Single

    Public Sub New(inputX As Single, inputY As Single)
        X = inputX
        Y = inputY
    End Sub
End Structure

''' <summary>
''' 受监控的PointF2类
''' </summary>
Public Class PointF2M
    Public X As SingleProperty
    Public Y As SingleProperty

    Public Sub New(inputX As Single, inputY As Single)
        X = New SingleProperty(inputX)
        Y = New SingleProperty(inputY)
    End Sub
End Class

''' <summary>
''' XY均为Short类型的Point结构体
''' </summary>
Public Structure PointI
    Public Property X As Short
    Public Property Y As Short

    Public Sub New(inputX As Short, inputY As Short)
        X = inputX
        Y = inputY
    End Sub
End Structure


