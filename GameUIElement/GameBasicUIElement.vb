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
    Protected SelfCanvasRect As RawRectangleF = Nothing
    Public ReadOnly Property Height As Integer
        Get
            Return CInt(BasicRect.Bottom - BasicRect.Top)
        End Get
    End Property
    Public ReadOnly Property Width As Integer
        Get
            Return CInt(BasicRect.Right - BasicRect.Left)
        End Get
    End Property
    Public Z_Index As Short = 0
    Public DefaultBackground As Brush = Nothing
    Public Visible As Boolean = True
    Public FreezeEvents As Boolean = False
    Public Opacity As Single = 1.0F

    Public RelativeLastItem As GameBasicUIElement = Nothing
    Public RelativeNextItem As GameBasicUIElement = Nothing

    Public Event MouseEnter() Implements IMouseArea.MouseEnter
    Public Event MouseLeave() Implements IMouseArea.MouseLeave
    Public Event MouseDown(e As GameMouseEventArgs) Implements IMouseArea.MouseDown
    Public Event MouseMove(e As GameMouseEventArgs) Implements IMouseArea.MouseMove
    Public Event MouseUp(e As GameMouseEventArgs) Implements IMouseArea.MouseUp
    Public Event MouseWheel(e As GameMouseEventArgs) Implements IMouseArea.MouseWheel
    Public Event GlobalMouseMove(e As GameMouseEventArgs) Implements IMouseArea.GlobalMouseMove

    ''' <summary>
    ''' 绑定的D2dDeviceContext
    ''' </summary>
    Public BindingContext As DeviceContext = Nothing
    Protected ControlCanvas As Bitmap1 = Nothing

    Public Sub InitializeControlCanvas()
        Me.ControlCanvas = New Bitmap1(Me.BindingContext, New SharpDX.Size2(Me.Width, Me.Height), NORMAL_BITMAP_PROPERTY)
        Me.SelfCanvasRect = New RawRectangleF(0, 0, Me.Width, Me.Height)
    End Sub

    Public Overridable Overloads Sub DrawControl(ByRef context As DeviceContext, ByRef spec As SpectatorCamera, canvasBitmap As Bitmap1)
        Call Me.DrawControl(context, spec, canvasBitmap, Me.BasicRect)
    End Sub

    Public MustOverride Overloads Sub DrawControl(ByRef context As DeviceContext, ByRef spec As SpectatorCamera, canvasBitmap As Bitmap1, newRect As RawRectangleF)

    Public Function IsInside(input As PointF2) As Boolean Implements IMouseArea.IsInside
        Return (input.X >= Me.BasicRect.Left AndAlso input.X <= Me.BasicRect.Right AndAlso input.Y >= Me.BasicRect.Top AndAlso input.Y <= Me.BasicRect.Bottom)
    End Function
    Public Function IsInside(input As PointI) As Boolean Implements IQuadtreeRecognizable.IsInside
        Return (input.X >= Me.BasicRect.Left AndAlso input.X <= Me.BasicRect.Right AndAlso input.Y >= Me.BasicRect.Top AndAlso input.Y <= Me.BasicRect.Bottom)
    End Function

    Public Function CompareRegionDirection(inputPivot As PointI) As QuadtreeDirection Implements IQuadtreeRecognizable.CompareRegionDirection
        Dim h_grid As Short = 0
        Dim v_grid As Short = 0
        If inputPivot.X > Me.BasicRect.Right Then
            h_grid = 0
        ElseIf inputPivot.X < Me.BasicRect.left Then
            h_grid = 2
        Else
            h_grid = 1
        End If
        If inputPivot.Y > Me.BasicRect.Bottom Then
            v_grid = 0
        ElseIf inputPivot.Y < Me.BasicRect.top Then
            v_grid = 2
        Else
            v_grid = 1
        End If
        Return Quadtree.NinePatchMapping(v_grid * 3 + h_grid)

    End Function

    Public Sub RaiseMouseDown(e As GameMouseEventArgs)
        If Not FreezeEvents Then RaiseEvent MouseDown(e)
    End Sub
    Public Sub RaiseMouseMove(e As GameMouseEventArgs)
        If Not FreezeEvents Then RaiseEvent MouseMove(e)
    End Sub
    Public Sub RaiseMouseUp(e As GameMouseEventArgs)
        If Not FreezeEvents Then RaiseEvent MouseUp(e)
    End Sub
    Public Sub RaiseMouseEnter()
        If Not FreezeEvents Then RaiseEvent MouseEnter()
    End Sub
    Public Sub RaiseMouseLeave()
        If Not FreezeEvents Then RaiseEvent MouseLeave()
    End Sub
    Public Sub RaiseMouseWheel(e As GameMouseEventArgs)
        If Not FreezeEvents Then RaiseEvent MouseWheel(e)
    End Sub
    Public Sub RaiseGlobalMouseMove(e As GameMouseEventArgs)
        If Not FreezeEvents Then RaiseEvent GlobalMouseMove(e)
    End Sub

    Public Function IsValid() As Boolean Implements IQuadtreeRecognizable.IsValid
        Return Me.Visible
    End Function
End Class
