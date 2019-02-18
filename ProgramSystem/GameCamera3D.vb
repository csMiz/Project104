' -----------------------------------------
' Copyright (c) 2019 All Rights Reserved.
' 
' Filename: GameCamera3D
' Author: Miz
' Date: 2019/1/12 10:29:23
' -----------------------------------------

''' <summary>
''' 游戏三维摄像机类
''' </summary>
Public Class GameCamera3D
    ''' <summary>
    ''' 摄像机位置
    ''' </summary>
    Public Position As PointF3
    ''' <summary>
    ''' 摄像机方向，旋转顺序依次为XYZ，逆时针，弧度
    ''' </summary>
    Public Rotation As PointF3
    ''' <summary>
    ''' 摄像机视野角，使用弧度(rad)
    ''' </summary>
    Public FOV As Single = Math.PI / 6
    ''' <summary>
    ''' 近处屏幕，即最近可见位置（显示屏位置）
    ''' </summary>
    Public ZNear As Single = 1
    ''' <summary>
    ''' 远处屏幕，即最远可见位置
    ''' </summary>
    Public ZFar As Single = 100

    Private View_P As New MathMatrixS(4, 4)

    Private View_RX As New MathMatrixS(4, 4)

    Private View_RY As New MathMatrixS(4, 4)

    Private View_RZ As New MathMatrixS(4, 4)

    Private Projection As New MathMatrixS(4, 4)

    ''' <summary>
    ''' 提供给GPU的WVP变换矩阵
    ''' </summary>
    Public WVP As SharpDX.Mathematics.Interop.RawMatrix
    ''' <summary>
    ''' 世界容器，所有绘制的物体都要放在容器内
    ''' </summary>
    Public WorldContainer As New List(Of Game3dObject)

    Public Sub CalculateViewP()
        With View_P
            .SetAsIdentity()
            .Value(3, 0) = Position.X
            .Value(3, 1) = Position.Y
            .Value(3, 2) = Position.Z
        End With
    End Sub

    Public Sub CalculateViewRX()
        With View_RX
            .SetAsIdentity()
            .Value(1, 1) = Math.Cos(Rotation.X)
            .Value(2, 1) = -Math.Sin(Rotation.X)
            .Value(1, 2) = Math.Sin(Rotation.X)
            .Value(2, 2) = Math.Cos(Rotation.X)
        End With
    End Sub

    Public Sub CalculateViewRY()
        'TODO
    End Sub

    Public Sub CalculateViewRZ()
        'TODO
    End Sub

    Public Sub RefreshProjection()
        With Projection
            .Value(0, 0) = 1 / Math.Tan(FOV / 2)
            .Value(1, 1) = 1 / Math.Tan(FOV / 2)
            .Value(2, 2) = -((ZNear + ZFar) / (ZFar - ZNear))
            .Value(3, 3) = 0
            .Value(2, 3) = -1
            .Value(3, 2) = -(2 * ZFar * ZNear / (ZFar - ZNear))
        End With
    End Sub

    Public Sub CalculateWVP()
        WVP = (Projection * (View_RZ * (View_RY * (View_RX * View_P)))).ToRawMatrix
    End Sub

End Class
