' -----------------------------------------
' Copyright (c) 2018 All Rights Reserved.
' 
' Filename: GameFlatButton
' Author: Miz
' Date: 2018/10/29 11:42:39
' -----------------------------------------

Imports p104
Imports SharpDX.Direct2D1
Imports SharpDX.Mathematics.Interop

''' <summary>
''' 扁平化按钮控件类
''' </summary>
Public Class GameFlatButton
    Inherits GameButton

    Public BackgroundColour As SolidColorBrush = Nothing
    Public BorderColour As SolidColorBrush = Nothing
    Public FocusBorderColour As SolidColorBrush = Nothing

    Public CursorLightBrush As RadialGradientBrush = Nothing
    Public CursorLightBorderBrush As RadialGradientBrush = Nothing

    Public Sub New()
        AddHandler Me.GlobalMouseMove, AddressOf Me.FluentCursorLight
        Me.BackgroundColour = BLACK_COLOUR_BRUSH(2)
    End Sub

    Public Sub InitializeCursorLightBrush()
        Dim r_brushProperty As New RadialGradientBrushProperties()
        With r_brushProperty
            .Center = New RawVector2(-100, -100)
            .GradientOriginOffset = New RawVector2(0, 0)
            .RadiusX = 100
            .RadiusY = 100
        End With
        Dim stops(1) As GradientStop
        stops(0) = New GradientStop() With {
            .Color = New RawColor4(1, 1, 1, 0.17),         '全局光
            .Position = 0.0F}
        stops(1) = New GradientStop() With {
            .Color = New RawColor4(1, 1, 1, 0),
            .Position = 1.0F}

        Dim stopCollection As New GradientStopCollection(Me.BindingContext, stops)
        Me.CursorLightBrush = New RadialGradientBrush(Me.BindingContext, r_brushProperty, NORMAL_BRUSH_PROPERTY, stopCollection)

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
        Me.CursorLightBorderBrush = New RadialGradientBrush(Me.BindingContext, r_brushProperty_2, NORMAL_BRUSH_PROPERTY, stopCollection2)

    End Sub

    Public Overrides Sub DrawControl(ByRef context As DeviceContext, ByRef spec As SpectatorCamera, canvasBitmap As Bitmap1, newRect As RawRectangleF)
        With context
            .EndDraw()

            .Target = Me.ControlCanvas
            .BeginDraw()

            .Clear(Nothing)
            .FillRectangle(Me.SelfCanvasRect, Me.BackgroundColour)
            .FillRectangle(Me.SelfCanvasRect, Me.CursorLightBrush)
            .DrawRectangle(Me.SelfCanvasRect, Me.BorderColour, 3.0F)
            .DrawRectangle(Me.SelfCanvasRect, Me.CursorLightBorderBrush, 3.0F)
            .DrawBitmap(Me.TextImage.FontImage, Me.SelfCanvasRect, NOT_TRANSPARENT, BitmapInterpolationMode.Linear)

            .EndDraw()
            .Target = canvasBitmap
            .BeginDraw()

            .DrawBitmap(Me.ControlCanvas, newRect, NOT_TRANSPARENT, BitmapInterpolationMode.Linear)
        End With
    End Sub

    Public Sub FluentCursorLight(e As GameMouseEventArgs)
        Dim relativeCursorX As Single = e.X - Me.BasicRect.Left
        Dim relativeCursorY As Single = e.Y - Me.BasicRect.Top
        Dim position As New RawVector2(relativeCursorX, relativeCursorY)
        Me.CursorLightBrush.Center = position
        Me.CursorLightBorderBrush.Center = position
    End Sub

End Class
