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

    Private ContentRect As RawRectangleF = Nothing

    Public Sub New(Optional usingDefaultEvents As Boolean = True)
        If usingDefaultEvents Then
            Call Me.UseDefaultMouseEnterLeaveEvents()
        End If
        Me.BackgroundColour = BLACK_COLOUR_BRUSH(2)
    End Sub

    Public Sub UseDefaultMouseEnterLeaveEvents()
        AddHandler Me.MouseEnter, AddressOf DefaultMouseEnter
        AddHandler Me.MouseLeave, AddressOf DefaultMouseLeave
        AddHandler Me.GlobalMouseMove, AddressOf Me.FluentCursorLight

    End Sub

    Public Sub InitializeCursorLightBrush()
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
        Me.CursorLightBrush = New RadialGradientBrush(Me.BindingContext, r_brushProperty, NORMAL_BRUSH_PROPERTY, stopCollection)
        stopCollection.Dispose()

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
        stopCollection2.Dispose()

    End Sub

    Public Sub InitializeBorderStyle()
        Me.ContentRect = New RawRectangleF(2, 2, Me.SelfCanvasRect.Right - 2, Me.SelfCanvasRect.Bottom - 2)
    End Sub

    Public Overrides Sub DrawControlAtSelfCanvas(ByRef context As DeviceContext, ByRef spec As SpectatorCamera, canvasBitmap As Bitmap1)
        With context
            .Target = Me.ControlCanvas
            .BeginDraw()
            .Clear(Nothing)

            .FillRectangle(Me.ContentRect, Me.BackgroundColour)
            If Me.HaveFocus Then .FillRectangle(Me.ContentRect, Me.CursorLightBrush)
            .DrawRectangle(Me.SelfCanvasRect, Me.BorderColour, 3.0F)
            .DrawRectangle(Me.SelfCanvasRect, Me.CursorLightBorderBrush, 3.0F)
            ButtonText.DrawText(context, New RawVector2(0, 0))

            .EndDraw()
        End With
    End Sub

    Public Sub FluentCursorLight(e As GameMouseEventArgs)
        Dim relativeCursorX As Single = e.X - Me.AbsoluteRect.Left
        Dim relativeCursorY As Single = e.Y - Me.AbsoluteRect.Top
        Dim position As New RawVector2(relativeCursorX, relativeCursorY)
        Me.CursorLightBrush.Center = position
        Me.CursorLightBorderBrush.Center = position

        Me.NeedRepaint = True
    End Sub
    Private Sub DefaultMouseEnter()
        Me.HaveFocus = True
        Me.NeedRepaint = True

    End Sub
    Private Sub DefaultMouseLeave()
        Me.HaveFocus = False
        Me.NeedRepaint = True

    End Sub

End Class
