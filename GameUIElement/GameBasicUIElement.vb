' -----------------------------------------
' Copyright (c) 2018 All Rights Reserved.
' 
' Filename: GameBasicUIElement
' Author: Miz
' Date: 2018/10/29 11:17:45
' -----------------------------------------

Imports p104
Imports SharpDX.Direct2D1
Imports SharpDX.Mathematics.Interop
''' <summary>
''' 游戏UI控件基类
''' </summary>
Public MustInherit Class GameBasicUIElement
    Implements IMouseArea
    Implements IQuadtreeRecognizable

    ''' <summary>
    ''' 内容框矩形
    ''' </summary>
    Public BasicRect As RawRectangleF = Nothing
    Public Z_Index As Short = 0

    Public Visible As Boolean = True
    Public Opacity As Single = 1.0F

    Public Event MouseEnter() Implements IMouseArea.MouseEnter
    Public Event MouseLeave() Implements IMouseArea.MouseLeave
    Public Event MouseDown(e As GameMouseEventArgs) Implements IMouseArea.MouseDown
    Public Event MouseMove(e As GameMouseEventArgs) Implements IMouseArea.MouseMove
    Public Event MouseUp(e As GameMouseEventArgs) Implements IMouseArea.MouseUp

    ''' <summary>
    ''' 初始化矩形框
    ''' </summary>
    Public Sub InitializeBasicRect(inputLeft As Integer, inputTop As Integer, inputRight As Integer, inputBottom As Integer)
        Me.BasicRect = New RawRectangleF(inputLeft, inputTop, inputRight, inputBottom)
    End Sub

    Public MustOverride Sub DrawControl(ByRef context As DeviceContext, ByRef spec As SpectatorCamera, canvasBitmap As Bitmap1)

    Public Function IsInside(input As PointF2) As Boolean Implements IMouseArea.IsInside
        Return (input.X >= Me.BasicRect.Left AndAlso input.X <= Me.BasicRect.Right AndAlso input.Y >= Me.BasicRect.Top AndAlso input.Y <= Me.BasicRect.Bottom)
    End Function
    Public Function IsInside(input As PointI) As Boolean Implements IQuadtreeRecognizable.IsInside
        Return (input.X >= Me.BasicRect.Left AndAlso input.X <= Me.BasicRect.Right AndAlso input.Y >= Me.BasicRect.Top AndAlso input.Y <= Me.BasicRect.Bottom)
    End Function

    Public Function CompareRegionDirection(input As PointI) As QuadtreeDirection Implements IQuadtreeRecognizable.CompareRegionDirection
        Dim h_grid As Short = 0
        Dim v_grid As Short = 0
        If input.X < Me.BasicRect.Left Then
            h_grid = 0
        ElseIf input.X > Me.BasicRect.Right Then
            h_grid = 2
        Else
            h_grid = 1
        End If
        If input.Y < Me.BasicRect.Top Then
            v_grid = 0
        ElseIf input.Y > Me.BasicRect.Bottom Then
            v_grid = 2
        Else
            v_grid = 1
        End If
        Return Quadtree.NinePatchMapping(v_grid * 3 + h_grid)

    End Function

    Public Sub RaiseMouseDown(e As GameMouseEventArgs)
        RaiseEvent MouseDown(e)
    End Sub
    Public Sub RaiseMouseMove(e As GameMouseEventArgs)
        RaiseEvent MouseMove(e)
    End Sub
    Public Sub RaiseMouseUp(e As GameMouseEventArgs)
        RaiseEvent MouseUp(e)
    End Sub
    Public Sub RaiseMouseEnter()
        RaiseEvent MouseEnter()
    End Sub
    Public Sub RaiseMouseLeave()
        RaiseEvent MouseLeave()
    End Sub


End Class
