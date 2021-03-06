﻿' -----------------------------------------
' Copyright (c) 2018 All Rights Reserved.
' 
' Filename: GameTextBox
' Author: Miz
' Date: 2018/11/12 10:16:58
' -----------------------------------------

Imports p104
Imports SharpDX.Direct2D1
Imports SharpDX.Mathematics.Interop

Public Class GameTextBox
    Inherits GameBasicUIElement

    Public TextItem As New TextItem2

    Public Editable As Boolean = True
    Public Pressed As Boolean = False

    Public BackgroundBrush As SolidColorBrush = Nothing
    Public BorderBrush As SolidColorBrush = Nothing
    Public ContentLightBrush As RadialGradientBrush = Nothing
    Public BorderLightBrush As RadialGradientBrush = Nothing

    Public Sub New(Optional defaultEvent As Boolean = True)
        If defaultEvent Then
            Me.RegisterRelativeMouseEvents()
        End If
    End Sub

    Public Sub ChangeText(text As String)
        Me.TextItem.Text = text
        Me.TextItem.GenerateTextLayout()
    End Sub

    Public Sub InitializeLightBrush()
        Dim r_brushProperty As New RadialGradientBrushProperties()
        With r_brushProperty
            .Center = New RawVector2(-150, -150)
            .GradientOriginOffset = New RawVector2(0, 0)
            .RadiusX = 150
            .RadiusY = 150
        End With
        Dim stops(1) As GradientStop
        stops(0) = New GradientStop() With {
            .Color = New RawColor4(1, 1, 1, 0.37),         '全局光
            .Position = 0.0F}
        stops(1) = New GradientStop() With {
            .Color = New RawColor4(1, 1, 1, 0),
            .Position = 1.0F}

        Dim stopCollection As New GradientStopCollection(Me.BindingContext, stops)
        Me.ContentLightBrush = New RadialGradientBrush(Me.BindingContext, r_brushProperty, NORMAL_BRUSH_PROPERTY, stopCollection)

        Dim r_brushProperty_2 As New RadialGradientBrushProperties()
        With r_brushProperty_2
            .Center = New RawVector2(-100, -100)
            .GradientOriginOffset = New RawVector2(0, 0)
            .RadiusX = 50
            .RadiusY = 50
        End With
        Dim stops2(1) As GradientStop
        stops2(0) = New GradientStop() With {
            .Color = New RawColor4(1, 1, 1, 0.78),         '全局光
            .Position = 0.0F}
        stops2(1) = New GradientStop() With {
            .Color = New RawColor4(1, 1, 1, 0),
            .Position = 1.0F}
        Dim stopCollection2 As New GradientStopCollection(Me.BindingContext, stops2)
        Me.BorderLightBrush = New RadialGradientBrush(Me.BindingContext, r_brushProperty_2, NORMAL_BRUSH_PROPERTY, stopCollection2)

    End Sub

    Private Sub RegisterRelativeMouseEvents()
        AddHandler Me.MouseEnter, AddressOf DefaultMouseEnter
        AddHandler Me.MouseLeave, AddressOf DefaultMouseLeave
        AddHandler Me.MouseDown, AddressOf DefaultMouseDown
        AddHandler Me.MouseUp, AddressOf DefaultMouseUp
        AddHandler Me.GlobalMouseMove, AddressOf RelativeFluentCursorLight
    End Sub

    Public Overrides Sub DrawControlAtSelfCanvas(ByRef context As DeviceContext, ByRef spec As SpectatorCamera, canvasBitmap As Bitmap1)
        With context
            .Target = Me.ControlCanvas
            .BeginDraw()
            .Clear(Nothing)

            '.FillRectangle(Me.SelfCanvasRect, BackgroundBrush)
            If Me.HaveFocus Then
                .FillRectangle(Me.SelfCanvasRect, WHITE_COLOUR_BRUSH(0))
                .FillRectangle(Me.SelfCanvasRect, Me.ContentLightBrush)
            End If
            Me.TextItem.DrawText(context, New RawVector2(0, 0))
            .DrawRectangle(Me.SelfCanvasRect, BorderBrush, 3.0F)
            .DrawRectangle(Me.SelfCanvasRect, BorderLightBrush, 3.0F)

            .EndDraw()
        End With
    End Sub

    Private Sub DefaultMouseEnter()
        Me.HaveFocus = True
    End Sub

    Private Sub DefaultMouseLeave()
        Me.HaveFocus = False
    End Sub

    Private Sub DefaultMouseDown()
        Me.Pressed = True
    End Sub

    Private Sub DefaultMouseUp()
        Me.Pressed = False
    End Sub

    Public Sub RelativeFluentCursorLight(e As GameMouseEventArgs)
        Dim relativeCursorX As Single = e.X - Me.AbsoluteRect.Left
        Dim relativeCursorY As Single = e.Y - Me.AbsoluteRect.Top
        Dim position As New RawVector2(relativeCursorX, relativeCursorY)

        Me.ContentLightBrush.Center = position
        Me.BorderLightBrush.Center = position
    End Sub

End Class

