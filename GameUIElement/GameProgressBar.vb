' -----------------------------------------
' Copyright (c) 2018 All Rights Reserved.
' 
' Filename: GameProgressBar
' Author: Miz
' Date: 2018/12/23 20:43:12
' -----------------------------------------

Imports p104
Imports SharpDX.Direct2D1
Imports SharpDX.Mathematics.Interop

Public Class GameProgressBar
    Inherits GameBasicUIElement

    Private ProgressBase As Single = 0.0F
    Public Property Progress As Single
        Get
            Return Me.ProgressBase
        End Get
        Set(value As Single)
            Me.ProgressBase = value
            Me.ResetProcessRect()
        End Set
    End Property
    Public ProgressRect As RawRectangleF
    Public BarBrush As Brush = Nothing

    Public Sub New()
        Me.Progress = 0.0F
        Me.DefaultBackground = BLACK_COLOUR_BRUSH(4)
        Me.BarBrush = WHITE_COLOUR_BRUSH(4)
    End Sub

    Public Sub ResetProcessRect()
        Me.ProgressRect = New RawRectangleF(0, 0, Me.Progress * Me.Width, Me.Height)
    End Sub

    Public Overrides Sub DrawControlAtSelfCanvas(ByRef context As DeviceContext, ByRef spec As SpectatorCamera, canvasBitmap As Bitmap1)
        With context
            .Target = Me.ControlCanvas
            .BeginDraw()
            .Clear(Nothing)

            .FillRectangle(Me.SelfCanvasRect, Me.DefaultBackground)
            .FillRectangle(Me.ProgressRect, Me.BarBrush)

            .EndDraw()
        End With
    End Sub
End Class
