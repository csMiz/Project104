' -----------------------------------------
' Copyright (c) 2018 All Rights Reserved.
' 
' Filename: GameScrollViewer
' Author: Miz
' Date: 2018/11/9 11:00:01
' -----------------------------------------

Imports p104
Imports SharpDX.Direct2D1
Imports SharpDX.Mathematics.Interop

''' <summary>
''' 游戏滚动页面控件类
''' </summary>
Public Class GameScrollViewer
    Inherits GameBasicUIElement

    Private ChildrenContent As New GameContentFrame
    ''' <summary>
    ''' 滚动区域内的子元素，需要使用相对位置，从上到下排列
    ''' </summary>
    Public Children As List(Of GameBasicUIElement) = Nothing
    ''' <summary>
    ''' 当鼠标不在ScrollViewer区域内时禁用滚轮事件
    ''' </summary>
    Private ScrollMouseLock As Boolean = True
    Private PrivateScrollPosition As Integer = 0
    ''' <summary>
    ''' 当前滚动位置
    ''' </summary>
    Public Property CurrentScrollPosition As Integer
        Get
            Return PrivateScrollPosition
        End Get
        Set(value As Integer)
            PrivateScrollPosition = value
            Call Me.CalculateScrollBarRect()
            Call Me.GenerateShownChildren()
        End Set
    End Property
    ''' <summary>
    ''' 滚动最大位置
    ''' </summary>
    Public ScrollMax As Integer = 0
    ''' <summary>
    ''' 滚动条宽度
    ''' </summary>
    Public ScrollBarWidth As Integer = 10

    ''' <summary>
    ''' 显示的子控件
    ''' </summary>
    Private RenderingItems As GameBasicUIElement()
    ''' <summary>
    ''' 顺序同RenderingItems，绘制子控件相对于ScrollViewer的ControlCanvas的位置
    ''' </summary>
    Private RenderingItemRects As RawRectangleF()
    ''' <summary>
    ''' 当前鼠标聚焦的子控件
    ''' </summary>
    Private MouseFocusChild As GameBasicUIElement = Nothing
    Private ScrollBarBGRect As RawRectangleF = Nothing
    Private ScrollBarPositionRect As RawRectangleF = Nothing


    ''' <summary>
    ''' 滚动区总高度
    ''' </summary>
    Public ReadOnly Property ContentHeight As Integer
        Get
            Return ChildrenContent.Height
        End Get
    End Property


    ''' <summary>
    ''' 构造函数
    ''' </summary>
    Public Sub New(Optional defaultEvent As Boolean = True)
        Me.Children = Me.ChildrenContent.Children

        If defaultEvent Then
            AddHandler Me.MouseDown, AddressOf ScrollViewerMouseDown
            AddHandler Me.MouseMove, AddressOf ScrollViewerMouseMove
            AddHandler Me.MouseUp, AddressOf ScrollViewerMouseUp
            AddHandler Me.MouseEnter, AddressOf ScrollViewerMouseEnter
            AddHandler Me.MouseLeave, AddressOf ScrollViewerMouseLeave
            AddHandler Me.GlobalMouseMove, AddressOf ScrollViewerGlobalMouseMove
            AddHandler Me.MouseWheel, AddressOf ScrollViewerMouseWheel
        End If

        Me.ScrollBarBGRect = New RawRectangleF(Me.Width - Me.ScrollBarWidth, 0, Me.Width, Me.Height)

    End Sub

    ''' <summary>
    ''' 计算最大滚动高度
    ''' </summary>
    Public Sub CalculateScrollMax()
        If Me.Children.Count Then
            Dim maxBottom As Integer = Me.ContentHeight
            Dim tmpMax As Integer = maxBottom - Me.Height
            If tmpMax > 0 Then
                Me.ScrollMax = tmpMax
                Return
            End If
        End If
        Me.ScrollMax = 0
    End Sub

    Public Sub CalculateScrollBarRect()
        Dim barHeight As Integer = CInt((Me.Height ^ 2) / Me.ContentHeight)
        Dim barTop As Integer = CInt(Me.Height * Me.CurrentScrollPosition / Me.ContentHeight)
        Me.ScrollBarPositionRect = New RawRectangleF(Me.Width - Me.ScrollBarWidth, barTop, Me.Width, barTop + barHeight)

    End Sub

    ''' <summary>
    ''' 获取当前滚动区会显示的子控件
    ''' </summary>
    Public Sub GenerateShownChildren()
        '使用二分查找
        Dim topIndex As Integer = 0
        Dim bottomIndex As Integer = 0

        Dim lb As Integer = 0
        Dim ub As Integer = Me.Children.Count - 1
        topIndex = Math.Ceiling((lb + ub) / 2)

        Dim loopCount As Integer = 0
        Do
            If Me.CurrentScrollPosition >= Me.Children(topIndex).BasicRect.Bottom Then
                lb = topIndex
                topIndex = Math.Ceiling((lb + ub) / 2)
                If lb = ub AndAlso Me.CurrentScrollPosition = Me.Children(topIndex).BasicRect.Bottom Then
                    topIndex += 1
                    Exit Do
                End If
            ElseIf Me.CurrentScrollPosition < Me.Children(topIndex).BasicRect.Top Then
                ub = topIndex
                topIndex = Math.Ceiling((lb + ub) / 2)
                If ub - lb = 1 Then
                    topIndex -= 1
                    Exit Do
                End If
            Else
                Exit Do
            End If
            loopCount += 1
            If loopCount >= 255 Then Throw New Exception("loop error 1")
        Loop

        lb = topIndex
        ub = Me.Children.Count - 1
        bottomIndex = Math.Ceiling((lb + ub) / 2)

        loopCount = 0
        Do
            If Me.CurrentScrollPosition + Me.Height > Me.Children(bottomIndex).BasicRect.Bottom Then
                lb = bottomIndex
                bottomIndex = Math.Ceiling((lb + ub) / 2)
                If lb = ub Then
                    Exit Do
                End If
            ElseIf Me.CurrentScrollPosition + Me.Height <= Me.Children(bottomIndex).BasicRect.Top Then
                ub = bottomIndex
                bottomIndex = Math.Ceiling((lb + ub) / 2)
                If ub - lb = 1 OrElse ub = lb Then
                    bottomIndex -= 1
                    Exit Do
                End If
            Else
                Exit Do
            End If
            loopCount += 1
            If loopCount >= 255 Then Throw New Exception("loop error 2")
        Loop

        Dim resultCount As Integer = bottomIndex - topIndex + 1
        Me.RenderingItems = Me.Children.GetRange(topIndex, resultCount).ToArray
        ReDim Me.RenderingItemRects(resultCount - 1)
        Dim rectCursor As Integer = 0
        For Each item As GameBasicUIElement In Me.RenderingItems
            With item
                Dim fatherRect As New RawRectangleF(.BasicRect.Left, .BasicRect.Top - Me.CurrentScrollPosition, .BasicRect.Right, .BasicRect.Bottom - Me.CurrentScrollPosition)
                Me.RenderingItemRects(rectCursor) = fatherRect
                .FatherViewRect = fatherRect
                .AbsoluteRect = New RawRectangleF(Me.AbsoluteRect.Left + fatherRect.Left, Me.AbsoluteRect.Top + fatherRect.Top, Me.AbsoluteRect.Left + fatherRect.Right, Me.AbsoluteRect.Top + fatherRect.Bottom)
                .RefreshRects()
            End With
            rectCursor += 1
        Next

    End Sub

    Public Sub ScrollViewerMouseDown(e As GameMouseEventArgs)
        If MouseFocusChild IsNot Nothing Then
            MouseFocusChild.RaiseMouseDown(e)
        End If
    End Sub

    Public Sub ScrollViewerMouseMove(e As GameMouseEventArgs)
        Dim relativePosition As New PointI(e.X - Me.AbsoluteRect.Left, e.Y - Me.AbsoluteRect.Top)
        Dim focusing As GameBasicUIElement = Me.FindFocusingChild(relativePosition)

        If focusing IsNot MouseFocusChild Then
            If MouseFocusChild IsNot Nothing Then
                MouseFocusChild.RaiseMouseLeave(e)
                MouseFocusChild = focusing
                If focusing IsNot Nothing Then
                    With MouseFocusChild
                        .RaiseMouseEnter(e)
                        .RaiseMouseMove(e)
                    End With
                End If
            ElseIf focusing IsNot Nothing Then
                MouseFocusChild = focusing
                With MouseFocusChild
                    .RaiseMouseEnter(e)
                    .RaiseMouseMove(e)
                End With
            End If
        Else
            If MouseFocusChild IsNot Nothing Then
                MouseFocusChild.RaiseMouseMove(e)
            End If
        End If

    End Sub

    Public Sub ScrollViewerMouseUp(e As GameMouseEventArgs)
        If MouseFocusChild IsNot Nothing Then
            MouseFocusChild.RaiseMouseUp(e)
        End If
    End Sub

    Public Sub ScrollViewerMouseEnter()
        Me.ScrollMouseLock = False
    End Sub

    Public Sub ScrollViewerMouseLeave(e As GameMouseEventArgs)
        Me.ScrollMouseLock = True
        If MouseFocusChild IsNot Nothing Then
            MouseFocusChild.RaiseMouseLeave(e)
        End If
    End Sub

    Public Sub ScrollViewerMouseWheel(e As GameMouseEventArgs)
        'TODO
    End Sub

    Public Sub ScrollViewerGlobalMouseMove(e As GameMouseEventArgs)
        For Each element As GameBasicUIElement In Me.RenderingItems
            element.RaiseGlobalMouseMove(e)
        Next
    End Sub

    Public Overrides Sub TriggerDrawSelfCanvas(ByRef context As DeviceContext, ByRef spec As SpectatorCamera, canvasBitmap As Bitmap1)
        Me.DrawControlAtSelfCanvas(context, spec, canvasBitmap)
    End Sub

    Public Overrides Sub DrawControlAtSelfCanvas(ByRef context As DeviceContext, ByRef spec As SpectatorCamera, canvasBitmap As Bitmap1)
        If Me.Children.Count Then
            With context
                '预画子控件
                For i = 0 To RenderingItems.Count - 1
                    Dim item As GameBasicUIElement = Me.RenderingItems(i)
                    item.NeedRepaint = True    '暂无优化，总是刷新画布
                    item.TriggerDrawSelfCanvas(context, spec, Me.ControlCanvas)
                Next
                '切换context.target
                .Target = Me.ControlCanvas
                .BeginDraw()
                '清空画布
                .Clear(Nothing)
                '画背景
                .FillRectangle(Me.SelfCanvasRect, Me.DefaultBackground)
                '画子控件
                For i = 0 To RenderingItems.Count - 1
                    Dim item As GameBasicUIElement = Me.RenderingItems(i)
                    item.DrawControl(context, spec, Me.ControlCanvas, Me.RenderingItemRects(i))
                Next
                '画滚动条
                .FillRectangle(Me.ScrollBarBGRect, BLACK_COLOUR_BRUSH(3))
                .FillRectangle(Me.ScrollBarPositionRect, WHITE_COLOUR_BRUSH(2))
                '切换为原来的画布
                .EndDraw()
            End With
        End If
    End Sub

    Public Function FindFocusingChild(relativeCursor As PointI) As GameBasicUIElement
        Dim renderControlCount As Integer = Me.RenderingItems.Count
        If Not CBool(renderControlCount) Then Return Nothing
        Dim lb As Integer = 0
        Dim ub As Integer = renderControlCount - 1

        Dim loopCount As Integer = 0
        Do
            Dim middle As Integer = Math.Ceiling((lb + ub) / 2)
            If relativeCursor.Y >= Me.RenderingItems(middle).FatherViewRect.Bottom Then
                If Not CBool(lb - ub) Then Return Nothing
                lb = middle
            ElseIf relativeCursor.Y < Me.RenderingItems(middle).FatherViewRect.Top Then
                If ub - lb = 1 Then
                    ub -= 1
                ElseIf Not CBool(lb - ub) Then
                    Return Nothing
                Else
                    ub = middle
                End If
            Else
                Return Me.RenderingItems(middle)
            End If
            loopCount += 1
            If loopCount >= 255 Then Throw New Exception("loop error")
        Loop
        Return Nothing
    End Function

End Class
