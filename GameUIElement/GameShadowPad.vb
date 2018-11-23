' -----------------------------------------
' Copyright (c) 2018 All Rights Reserved.
' 
' Filename: GameShadowPad
' Author: Miz
' Date: 2018/11/20 15:16:20
' -----------------------------------------

Imports p104
Imports SharpDX.Direct2D1
Imports SharpDX.Mathematics.Interop

''' <summary>
''' 阴影层控件类
''' </summary>
Public Class GameShadowPad
    Inherits GameInteractiveRectangle

    Public BindingFlyoutControl As GameBasicUIElement = Nothing

    Public Sub New(Optional defaultEvent As Boolean = True)
        If defaultEvent Then
            AddHandler Me.MouseDown, AddressOf Me.DefaultMouseDown
            AddHandler Me.MouseEnter, AddressOf Me.DefaultMouseEnter
            AddHandler Me.MouseLeave, AddressOf Me.DefaultMouseLeave
            AddHandler Me.MouseMove, AddressOf Me.DefaultMouseMove
            AddHandler Me.MouseWheel, AddressOf Me.DefaultMouseWheel
            AddHandler Me.GlobalMouseMove, AddressOf Me.DefaultGlobalMouseMove
        End If
    End Sub

    Public Overrides Sub DrawControlAtSelfCanvas(ByRef context As DeviceContext, ByRef spec As SpectatorCamera, canvasBitmap As Bitmap1)
        With context
            .Target = Me.ControlCanvas
            .BeginDraw()
            .Clear(Nothing)
            .FillRectangle(Me.BasicRect, Me.DefaultBackground)
            .EndDraw()
        End With
    End Sub

    Public Overrides Sub DrawControl(ByRef context As DeviceContext, ByRef spec As SpectatorCamera, canvasBitmap As Bitmap1, newRect As RawRectangleF)
        With context
            .DrawBitmap(Me.ControlCanvas, newRect, NOT_TRANSPARENT, BitmapInterpolationMode.Linear)
        End With
    End Sub

    Public Sub DefaultMouseDown(e As GameMouseEventArgs)
        Me.BindingFlyoutControl.Visible = False
        Me.Visible = False
        e.Deliver = False
    End Sub
    Public Sub DefaultMouseMove(e As GameMouseEventArgs)
        e.Deliver = False
    End Sub
    Public Sub DefaultMouseEnter(e As GameMouseEventArgs)
        e.Deliver = False
    End Sub
    Public Sub DefaultMouseLeave(e As GameMouseEventArgs)
        e.Deliver = False
    End Sub
    Public Sub DefaultGlobalMouseMove(e As GameMouseEventArgs)
        e.Deliver = False
    End Sub
    Public Sub DefaultMouseWheel(e As GameMouseEventArgs)
        e.Deliver = False
    End Sub
End Class
