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
    ''' <para>布局不发生动态改变时与FatherViewRect相同（大多数情况）。动态变化时表示相对于父对象的原始位置</para>
    ''' <para>例如GameScrollViewer(100,80,900,800)中第一个Button的BasicRect为(0,0,800,50)，AbsoluteRect为(100,80,900,130)，
    ''' 滚动条位置处于100时FatherViewRect为(0,-100,800,-50)，SelfCanvasRect为(0,0,800,50)</para>
    ''' </summary>
    Public BasicRect As RawRectangleF = Nothing
    ''' <summary>
    ''' 控件位置绝对矩形
    ''' </summary>
    Public AbsoluteRect As RawRectangleF = Nothing
    ''' <summary>
    ''' 父对象画布相对矩形
    ''' <para>例如子对象画在GameScrollViewer画布上的相对矩形，见<see cref="BasicRect"/>的例子</para>
    ''' </summary>
    Public FatherViewRect As RawRectangleF = Nothing
    ''' <summary>
    ''' 控件外围绝对矩形，主要用来控制全局光的刷新
    ''' <para>例如FlatButton的AbsoluteRect为(400,100,600,150)，全局光半径为30，则SkirtRect为(370,70,630,180)</para>
    ''' <para>如果需要实现“无论指针在哪都进行更新”的功能，可以将SkirtRect设置为(0,0,Resolve.X,Resolve.Y)</para>
    ''' </summary>
    Public SkirtRect As RawRectangleF = Nothing
    ''' <summary>
    ''' 自身画布矩形
    ''' <para>一般情况下为(0,0,Width,Height)</para>
    ''' </summary>
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
    Public DefaultBackground As Brush = TRANSPARENT_BRUSH
    Public Visible As Boolean = True
    Public FreezeEvents As Boolean = False
    Public Opacity As Single = 1.0F

    Public RelativeLastItem As GameBasicUIElement = Nothing
    Public RelativeNextItem As GameBasicUIElement = Nothing

    Public Event MouseEnter(e As GameMouseEventArgs) Implements IMouseArea.MouseEnter
    Public Event MouseLeave(e As GameMouseEventArgs) Implements IMouseArea.MouseLeave
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
    ''' <summary>
    ''' 当前鼠标是否在控件内
    ''' </summary>
    Public HaveFocus As Boolean = False
    ''' <summary>
    ''' 控件参数更新后标记为NeedRepaint，在自己的ControlCanvas上重画
    ''' </summary>
    Public NeedRepaint As Boolean = True

    Public Shared ZIndexComparison As New Comparison(Of GameBasicUIElement)(AddressOf CompareZIndex)
    Public Shared ZIndexComparisonReverse As New Comparison(Of GameBasicUIElement)(AddressOf CompareZIndexReverse)

    Public Sub InitializeControlCanvas()
        Me.ControlCanvas = New Bitmap1(Me.BindingContext, New SharpDX.Size2(Me.Width, Me.Height), NORMAL_BITMAP_PROPERTY)
        Me.SelfCanvasRect = New RawRectangleF(0, 0, Me.Width, Me.Height)

    End Sub

    ''' <summary>
    ''' 从外部更新Rect后，在这里调用对子对象Rect的更新
    ''' </summary>
    Public Overridable Sub RefreshRects()
    End Sub

    Public Overridable Sub TriggerDrawSelfCanvas(ByRef context As DeviceContext, ByRef spec As SpectatorCamera, canvasBitmap As Bitmap1)
        If Me.NeedRepaint Then
            Me.DrawControlAtSelfCanvas(context, spec, canvasBitmap)
            Me.NeedRepaint = False
        End If
    End Sub

    ''' <summary>
    ''' 在控件独立画布上绘制控件
    ''' </summary>
    Public MustOverride Sub DrawControlAtSelfCanvas(ByRef context As DeviceContext, ByRef spec As SpectatorCamera, canvasBitmap As Bitmap1)

    ''' <summary>
    ''' 使用默认位置绘制控件
    ''' </summary>
    Public Overridable Overloads Sub DrawControl(ByRef context As DeviceContext, ByRef spec As SpectatorCamera, canvasBitmap As Bitmap1)
        Call Me.DrawControl(context, spec, canvasBitmap, Me.BasicRect)
    End Sub

    ''' <summary>
    ''' 使用新的位置绘制控件
    ''' </summary>
    Public Overridable Overloads Sub DrawControl(ByRef context As DeviceContext, ByRef spec As SpectatorCamera, canvasBitmap As Bitmap1, newRect As RawRectangleF)
        context.DrawBitmap(Me.ControlCanvas, newRect, NOT_TRANSPARENT, BitmapInterpolationMode.Linear)
    End Sub

    ''' <summary>
    ''' 判断输入点是否在控件内
    ''' </summary>
    ''' <param name="input">绝对输入点</param>
    Public Function IsInside(input As PointF2) As Boolean Implements IMouseArea.IsInside
        Return (input.X >= Me.AbsoluteRect.Left AndAlso input.X <= Me.AbsoluteRect.Right AndAlso input.Y >= Me.AbsoluteRect.Top AndAlso input.Y <= Me.AbsoluteRect.Bottom)
    End Function
    ''' <summary>
    ''' 判断输入点是否在控件内
    ''' </summary>
    ''' <param name="input">绝对输入点</param>
    Public Function IsInside(input As PointI) As Boolean Implements IQuadtreeRecognizable.IsInside
        Return (input.X >= Me.AbsoluteRect.Left AndAlso input.X <= Me.AbsoluteRect.Right AndAlso input.Y >= Me.AbsoluteRect.Top AndAlso input.Y <= Me.AbsoluteRect.Bottom)
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
    Public Sub RaiseMouseEnter(e As GameMouseEventArgs)
        If Not FreezeEvents Then RaiseEvent MouseEnter(e)
    End Sub
    Public Sub RaiseMouseLeave(e As GameMouseEventArgs)
        If Not FreezeEvents Then RaiseEvent MouseLeave(e)
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

    Public Shared Function CompareZIndex(a As GameBasicUIElement, b As GameBasicUIElement) As Integer
        Return a.Z_Index - b.Z_Index
    End Function

    Public Shared Function CompareZIndexReverse(a As GameBasicUIElement, b As GameBasicUIElement) As Integer
        Return b.Z_Index - a.Z_Index
    End Function

    Public Shared Function ConvertQuadElements(input As List(Of IQuadtreeRecognizable)) As List(Of GameBasicUIElement)
        Dim result As New List(Of GameBasicUIElement)
        For Each element As GameBasicUIElement In input
            result.Add(element)
        Next
        Return result
    End Function

End Class
