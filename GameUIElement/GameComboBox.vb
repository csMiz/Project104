﻿' -----------------------------------------
' Copyright (c) 2018 All Rights Reserved.
' 
' Filename: GameComboBox
' Author: Miz
' Date: 2018/11/12 10:14:27
' -----------------------------------------

Imports p104
Imports SharpDX.Direct2D1
Imports SharpDX.Mathematics.Interop

''' <summary>
''' 组合框控件类
''' </summary>
Public Class GameComboBox
    Inherits GameBasicUIElement

    ''' <summary>
    ''' 标签文字
    ''' </summary>
    Public TitleText As New TextItem2
    ''' <summary>
    ''' 当前所选内容文本框控件
    ''' </summary>
    Private SelectedBox As GameTextBox = Nothing

    ''' <summary>
    ''' 所有选项
    ''' </summary>
    Public SelectionStrings As New List(Of String)
    ''' <summary>
    ''' SelectedIndex为-1时显示的提示文字
    ''' </summary>
    Public PlaceHolder As String = vbNullString
    ''' <summary>
    ''' 所选选项索引
    ''' </summary>
    Public SelectedIndex As Integer = -1

    ''' <summary>
    ''' 标签矩形
    ''' </summary>
    Private LabelRect As RawRectangleF = Nothing
    ''' <summary>
    ''' 弹出的子菜单控件
    ''' </summary>
    Private FlyoutControl As GameListBox = Nothing
    ''' <summary>
    ''' 定义FlyoutControl的BasicRect
    ''' </summary>
    Private FlyoutRect As RawRectangleF = Nothing
    ''' <summary>
    ''' 弹出子菜单阴影层控件
    ''' </summary>
    Public FlyoutShadowPad As GameShadowPad = Nothing

    Public Event SelectionChanged(nowIndex As Integer, nowContent As String)



    ''' <summary>
    ''' 选项数
    ''' </summary>
    Public ReadOnly Property SelectionCount As Integer
        Get
            Return Me.SelectionStrings.Count
        End Get
    End Property
    ''' <summary>
    ''' ComboBox是否接受自定义输入
    ''' </summary>
    Public Editable As Boolean = False

    Public Sub New(Optional defaultEvent As Boolean = True)
        Me.SelectedBox = New GameTextBox With {
            .Editable = Me.Editable,
            .BackgroundBrush = BLACK_COLOUR_BRUSH(2)}
        If defaultEvent Then
            Me.RegisterDefaultMouseEvents()

        End If
    End Sub

    Private Sub RegisterDefaultMouseEvents()
        'AddHandler Me.GlobalMouseMove, AddressOf RelativeGlobalMouseMove
        AddHandler Me.GlobalMouseMove, AddressOf DefaultGlobalMouseMove
        AddHandler Me.MouseEnter, AddressOf DefaultMouseEnter
        AddHandler Me.MouseLeave, AddressOf DefaultMouseLeave
        AddHandler Me.MouseDown, AddressOf DefaultMouseDown
        AddHandler Me.MouseUp, AddressOf DefaultMouseUp
        'AddHandler Me.MouseMove, AddressOf RelativeMouseMove
        AddHandler Me.MouseMove, AddressOf DefaultMouseMove
    End Sub

    Public Sub InitializeComboBox(labelWidth As Single)
        Me.InitializeControlCanvas()
        Dim labelSize As New PointI(labelWidth, Me.Height - 20)
        Me.LabelRect = New RawRectangleF(0, 10, labelWidth, Me.Height - 10)
        With Me.SelectedBox
            .BasicRect = New RawRectangleF(labelWidth, 10, Me.Width, Me.Height - 10)
            .AbsoluteRect = New RawRectangleF(Me.AbsoluteRect.Left + .BasicRect.Left, Me.AbsoluteRect.Top + .BasicRect.Top, Me.AbsoluteRect.Left + .BasicRect.Right, Me.AbsoluteRect.Top + .BasicRect.Bottom)
            .FatherViewRect = .BasicRect
            .BorderBrush = TRANSPARENT_BRUSH
            .BackgroundBrush = TRANSPARENT_BRUSH
            .BindingContext = Me.BindingContext
            .InitializeControlCanvas()
            If Me.SelectedIndex >= 0 Then
                .TextItem.Text = Me.SelectionStrings(Me.SelectedIndex)
            Else
                .TextItem.Text = Me.PlaceHolder
            End If
            With .TextItem
                .CanvasSize = New PointF2(Me.Width - labelWidth, Me.Height - 20)
                .FontType = GameFontType.CustomFont
                .FontName = Me.TitleText.FontName
                .FontSize = 18.0F
                .FontBrush = PURE_BLACK_BRUSH
                .GenerateTextLayout()
            End With
            .InitializeLightBrush()
        End With

        Me.FlyoutRect = New RawRectangleF(Me.SelectedBox.AbsoluteRect.Left, Me.SelectedBox.AbsoluteRect.Bottom + 1, Me.SelectedBox.AbsoluteRect.Right, Me.SelectedBox.AbsoluteRect.Bottom + 1 + Me.SelectedBox.Height * Me.SelectionCount)
        'Me.FlyoutRect = New RawRectangleF(0, 0, 200, Me.SelectedBox.Height * SelectionStrings.Count)

        Me.FlyoutControl = New GameListBox
        With Me.FlyoutControl
            .BindingContext = Me.BindingContext
            .BasicRect = Me.FlyoutRect
            .AbsoluteRect = .BasicRect
            .DefaultBackground = WHITE_COLOUR_BRUSH(1)
            .InitializeControlCanvas()
            .Z_Index = 5
            .Visible = False
            .Items = Me.SelectionStrings
            With .ChildrenFontStyle
                .FontType = Me.TitleText.FontType
                .FontName = Me.TitleText.FontName
                .FontBrush = PURE_BLACK_BRUSH
                .FontSize = Me.TitleText.FontSize
            End With
            .GenerateTextItems()
        End With
        AddHandler Me.FlyoutControl.MouseDown, AddressOf Me.FlyoutMouseDown

        Me.TitleText.CanvasSize = New PointF2(labelWidth, Me.Height - 20)
        Me.TitleText.GenerateTextLayout()

    End Sub

    Public Overrides Sub RefreshRects()
        Me.RefreshFlyoutPosition()
    End Sub

    Public Sub RefreshFlyoutPosition()
        With Me.SelectedBox
            .AbsoluteRect = New RawRectangleF(Me.AbsoluteRect.Left + .BasicRect.Left, Me.AbsoluteRect.Top + .BasicRect.Top, Me.AbsoluteRect.Left + .BasicRect.Right, Me.AbsoluteRect.Top + .BasicRect.Bottom)
        End With
        Me.FlyoutRect = New RawRectangleF(Me.SelectedBox.AbsoluteRect.Left, Me.SelectedBox.AbsoluteRect.Bottom + 1, Me.SelectedBox.AbsoluteRect.Right, Me.SelectedBox.AbsoluteRect.Bottom + 1 + Me.SelectedBox.Height * SelectionStrings.Count)
        Me.FlyoutControl.BasicRect = Me.FlyoutRect
        Me.FlyoutControl.AbsoluteRect = Me.FlyoutRect
    End Sub

    ''' <summary>
    ''' 导入ShadowPad并设定Flyout至GlobalControlList内，调用此方法前需要先调用InitializeComboBox
    ''' </summary>
    ''' <param name="shadowPad">阴影层控件</param>
    ''' <param name="globalControlList">全局控件列表</param>
    Public Sub ImportShadowPad(shadowPad As GameShadowPad, globalControlList As List(Of GameBasicUIElement))
        Me.FlyoutShadowPad = shadowPad
        With shadowPad
            .BindingFlyoutControl = Me.FlyoutControl
            .Z_Index = 4
        End With
        globalControlList.Add(Me.FlyoutControl)
    End Sub

    Public Overrides Sub DrawControlAtSelfCanvas(ByRef context As DeviceContext, ByRef spec As SpectatorCamera, canvasBitmap As Bitmap1)
        With context
            .Target = Me.ControlCanvas
            .BeginDraw()
            .Clear(Nothing)

            Me.TitleText.DrawText(context, New RawVector2(0, 0))
            .EndDraw()
            Me.SelectedBox.DrawControlAtSelfCanvas(context, spec, Me.ControlCanvas)
            .Target = Me.ControlCanvas
            .BeginDraw()
            Me.SelectedBox.DrawControl(context, spec, Me.ControlCanvas)

            .EndDraw()
        End With
    End Sub

    Public Overrides Sub DrawControl(ByRef context As DeviceContext, ByRef spec As SpectatorCamera, canvasBitmap As Bitmap1, newRect As RawRectangleF)
        'TODO: 绘制基本图形，如果是打开状态，再绘制子菜单(FlyoutCanvas)
        MyBase.DrawControl(context, spec, canvasBitmap, newRect)

    End Sub

    Public Sub OpenSubMenu()
        '子菜单使用一个ShadowPad，上面盖一个GameListBox实现
        '将这两个控件置于最顶层
        '当ShadowPad被点击时，同时隐藏这两个控件

        Me.FlyoutShadowPad.BindingFlyoutControl = Me.FlyoutControl
        Me.FlyoutShadowPad.Visible = True
        Me.FlyoutControl.Visible = True
    End Sub

    Public Sub CloseSubMenu()
        Me.FlyoutControl.Visible = False
        Me.FlyoutShadowPad.Visible = False
    End Sub

    Public Sub DefaultMouseEnter(e As GameMouseEventArgs)
        Me.HaveFocus = True
    End Sub

    Public Sub DefaultMouseLeave(e As GameMouseEventArgs)
        Me.HaveFocus = False
        If Me.SelectedBox.HaveFocus Then
            Me.SelectedBox.RaiseMouseLeave(e)
        End If
    End Sub

    Public Sub DefaultMouseDown(e As GameMouseEventArgs)
        If Me.SelectedBox.HaveFocus Then
            Me.SelectedBox.RaiseMouseDown(e)
        End If
    End Sub

    Public Sub DefaultMouseUp(e As GameMouseEventArgs)
        If Me.SelectedBox.HaveFocus Then
            If Me.SelectedBox.Pressed Then
                Me.OpenSubMenu()
            End If
            Me.SelectedBox.RaiseMouseUp(e)
        End If
    End Sub

    Public Sub DefaultGlobalMouseMove(e As GameMouseEventArgs)
        Me.SelectedBox.RaiseGlobalMouseMove(e)
    End Sub

    Public Sub DefaultMouseMove(e As GameMouseEventArgs)
        Dim isInsideBox As Boolean = Me.SelectedBox.IsInside(e.Position)
        If isInsideBox Then
            If Me.SelectedBox.HaveFocus Then
                Me.SelectedBox.RaiseMouseMove(e)
            Else
                Me.SelectedBox.RaiseMouseEnter(e)
            End If
        Else
            If Me.SelectedBox.HaveFocus Then
                Me.SelectedBox.RaiseMouseLeave(e)
            End If
        End If
    End Sub

    Public Sub FlyoutMouseDown(e As GameMouseEventArgs)
        Dim tmpIndex As Integer = (e.Y - Me.FlyoutControl.AbsoluteRect.Top) \ Me.FlyoutControl.SelectionHeight
        If tmpIndex >= 0 AndAlso tmpIndex < Me.FlyoutControl.Items.Count Then
            Me.SelectedIndex = tmpIndex
        Else
            Me.SelectedIndex = -1
        End If
        If Me.SelectedIndex <> -1 Then
            Me.SelectedBox.ChangeText(Me.SelectionStrings(Me.SelectedIndex))
            RaiseEvent SelectionChanged(Me.SelectedIndex, Me.SelectionStrings(Me.SelectedIndex))
        End If
        Me.CloseSubMenu()
        e.Deliver = False
    End Sub

    ''' <summary>
    ''' 作为子对象时的默认GMM事件处理
    ''' </summary>
    ''' <param name="e">RelativeEventArgs，对应自己的FatherRect</param>
    <Obsolete("GameMouseEventArgs不能从外部构造，改用DefaultGlobalMouseMove", True)>
    Public Sub RelativeGlobalMouseMove(e As GameMouseEventArgs)
        'Dim relativeCursorX As Single = e.X - Me.FatherViewRect.Left
        'Dim relativeCursorY As Single = e.Y - Me.FatherViewRect.Top
        'Dim position2 As New PointI(CInt(relativeCursorX), CInt(relativeCursorY))
        'Dim relativeArgs As New GameMouseEventArgs With {.Position = position2}
        ''因为使用了New，所以在ComboBox的TextBox中令e.Deliver=False没有任何作用
        'Me.SelectedBox.RaiseGlobalMouseMove(relativeArgs)
    End Sub
    <Obsolete("GameMouseEventArgs不能从外部构造，改用DefaultMouseMove", True)>
    Public Sub RelativeMouseMove(e As GameMouseEventArgs)
        'Dim relativeCursorX As Single = e.X - Me.FatherViewRect.Left
        'Dim relativeCursorY As Single = e.Y - Me.FatherViewRect.Top
        ''Dim position As New RawVector2(relativeCursorX, relativeCursorY)
        'Dim position2 As New PointI(CInt(relativeCursorX), CInt(relativeCursorY))
        'Dim relativeArgs As New GameMouseEventArgs With {.Position = position2}
        'Dim isInsideBox As Boolean = relativeCursorX >= Me.SelectedBox.BasicRect.Left AndAlso relativeCursorX <= Me.SelectedBox.BasicRect.Right
        'isInsideBox = isInsideBox AndAlso relativeCursorY >= Me.SelectedBox.BasicRect.Top AndAlso relativeCursorY <= Me.SelectedBox.BasicRect.Bottom
        'If isInsideBox Then
        '    If Me.SelectedBox.HaveFocus Then
        '        Me.SelectedBox.RaiseMouseMove(relativeArgs)
        '    Else
        '        Me.SelectedBox.RaiseMouseEnter(e)
        '    End If
        'Else
        '    If Me.SelectedBox.HaveFocus Then
        '        Me.SelectedBox.RaiseMouseLeave(e)
        '    End If
        'End If
    End Sub

End Class
