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

    Private RenderingItems As GameBasicUIElement()
    Private RenderingItemRects As RawRectangleF()
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
    ''' <param name="defaultEvents">是否使用默认的鼠标事件</param>
    Public Sub New(defaultEvents As Boolean)
        Me.Children = Me.ChildrenContent.Children

        If defaultEvents Then
            AddHandler Me.MouseDown, AddressOf ScrollViewerMouseDown
            AddHandler Me.MouseMove, AddressOf ScrollViewerMouseMove
            AddHandler Me.MouseUp, AddressOf ScrollViewerMouseUp
            AddHandler Me.MouseEnter, AddressOf ScrollViewerMouseEnter
            AddHandler Me.MouseLeave, AddressOf ScrollViewerMouseLeave
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
        'TODO: 有问题
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

        Dim resultBound As Integer = bottomIndex - topIndex
        ReDim Me.RenderingItems(resultBound)
        ReDim Me.RenderingItemRects(resultBound)
        For i = 0 To resultBound
            Dim tmpChild As GameBasicUIElement = Me.Children(topIndex + i)
            Me.RenderingItems(i) = tmpChild
            Me.RenderingItemRects(1) = New RawRectangleF(tmpChild.BasicRect.Left, tmpChild.BasicRect.Top - Me.CurrentScrollPosition, tmpChild.BasicRect.Right, tmpChild.BasicRect.Bottom - Me.CurrentScrollPosition)
        Next

    End Sub

    ''' <summary>
    ''' 清空滚动区画布并在其上绘制控件
    ''' </summary>
    Public Sub DrawItemsAtCanvas(ByRef context As DeviceContext, ByRef spec As SpectatorCamera, canvasBitmap As Bitmap1)
        '切换context.target
        context.EndDraw()
        context.Target = Me.ControlCanvas
        context.BeginDraw()
        '清空画布
        context.Clear(Nothing)
        '画背景
        context.FillRectangle(Me.SelfCanvasRect, BLACK_COLOUR_BRUSH(2))
        '画子控件
        For i = 0 To RenderingItems.Count - 1
            Dim item As GameBasicUIElement = Me.RenderingItems(i)
            item.DrawControl(context, spec, Me.ControlCanvas, Me.RenderingItemRects(i))
        Next
        '画滚动条
        context.FillRectangle(Me.ScrollBarBGRect, BLACK_COLOUR_BRUSH(3))
        context.FillRectangle(Me.ScrollBarPositionRect, WHITE_COLOUR_BRUSH(2))
        '切换为原来的画布
        context.EndDraw()
        context.Target = canvasBitmap
        context.BeginDraw()
    End Sub

    Public Sub ScrollViewerMouseDown(e As GameMouseEventArgs)
        'TODO
    End Sub

    Public Sub ScrollViewerMouseMove(e As GameMouseEventArgs)

    End Sub

    Public Sub ScrollViewerMouseUp(e As GameMouseEventArgs)

    End Sub

    Public Sub ScrollViewerMouseEnter()

    End Sub

    Public Sub ScrollViewerMouseLeave()

    End Sub

    Public Sub ScrollViewerMouseWheel(e As GameMouseEventArgs)

    End Sub

    Public Overrides Sub DrawControl(ByRef context As DeviceContext, ByRef spec As SpectatorCamera, canvasBitmap As Bitmap1, newRect As RawRectangleF)
        If Me.Children.Count Then
            Me.DrawItemsAtCanvas(context, spec, canvasBitmap)
        End If
        context.DrawBitmap(Me.ControlCanvas, newRect, NOT_TRANSPARENT, BitmapInterpolationMode.Linear)
    End Sub
End Class
