' -----------------------------------------
' Copyright (c) 2018 All Rights Reserved.
' 
' Filename: GameMouseEventArgs
' Author: Miz
' Date: 2018/11/24 14:55:55
' -----------------------------------------

''' <summary>
''' 游戏鼠标输入参数类
''' <para>不允许从外部直接构造GameMouseEventArgs，应该使用FromMouseEventArgs</para>
''' </summary>
Public Class GameMouseEventArgs
    ''' <summary>
    ''' 指针位置
    ''' <para>左上角为(0,0)，右下角为Camera的Resolve</para>
    ''' </summary>
    Public Position As New PointI(0, 0)
    ''' <summary>
    ''' 鼠标按键
    ''' </summary>
    Public MouseButton As MouseButtons = MouseButtons.None
    ''' <summary>
    ''' 鼠标滚轮
    ''' </summary>
    Public MouseWheel As Integer = 0
    Public ReadOnly Property X As Short
        Get
            Return Position.X
        End Get
    End Property
    Public ReadOnly Property Y As Short
        Get
            Return Position.Y
        End Get
    End Property
    ''' <summary>
    ''' 是否传递到下一层（指Z_Index）控件
    ''' </summary>
    Public Deliver As Boolean = True

    ''' <summary>
    ''' 不允许从外部直接构造GameMouseEventArgs
    ''' </summary>
    Private Sub New()
    End Sub

    ''' <summary>
    ''' 从系统鼠标事件参数生成GameMouseEventArgs
    ''' </summary>
    ''' <param name="source">系统鼠标事件参数</param>
    ''' <param name="windowsRect">当前窗体大小</param>
    ''' <param name="cameraRect">游戏Camera分辨率</param>
    Public Shared Function FromMouseEventArgs(source As MouseEventArgs, windowsRect As Rectangle, cameraRect As PointI) As GameMouseEventArgs
        Dim result As New GameMouseEventArgs With {
            .Position = New PointI(source.X * cameraRect.X / windowsRect.Width, source.Y * cameraRect.Y / windowsRect.Height),
            .MouseButton = source.Button,
            .MouseWheel = source.Delta}
        Return result
    End Function

    Public Shared Function GetCentreArgs(resolve As PointI)
        Dim result As New GameMouseEventArgs With {
            .Position = New PointI(resolve.X / 2, resolve.Y / 2)}
        Return result
    End Function

    Public Function PrintPositionString() As String
        Return "(" & Me.Position.X & ", " & Me.Position.Y & ")"
    End Function

End Class
