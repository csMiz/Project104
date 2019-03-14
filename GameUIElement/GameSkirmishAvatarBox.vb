' -----------------------------------------
' Copyright (c) 2019 All Rights Reserved.
' 
' Filename: GameSkirmishAvatarBox
' Author: Miz
' Date: 2019/3/12 14:57:59
' -----------------------------------------

Imports p104
Imports SharpDX.Direct2D1
Imports SharpDX.DirectWrite

Public Class GameSkirmishAvatarBox
    Inherits GameBasicUIElement

    Public DisplayUnit As GameUnit = Nothing

    Public EmptyUnit As Boolean = True

    Public DisplayMapBlock As SkirmishMapBlock2 = Nothing

    Public EmptyMapBlock As Boolean = True

    Public Overrides Sub DrawControlAtSelfCanvas(ByRef context As DeviceContext, ByRef spec As SpectatorCamera, canvasBitmap As Bitmap1)
        Dim text_block As TextLayout = Nothing
        If Not EmptyMapBlock Then
            text_block = GameFontHelper2.GetCustomTextLayout(DisplayMapBlock.X & " , " & DisplayMapBlock.Y, "P104_Font1", 20, New PointF2(200, 200))

        End If

        With context
            .Target = Me.ControlCanvas
            .BeginDraw()
            .Clear(Nothing)

            .FillRectangle(Me.SelfCanvasRect, BLACK_COLOUR_BRUSH(2))
            If Not EmptyMapBlock Then
                .DrawTextLayout(New SharpDX.Mathematics.Interop.RawVector2(0, 0), text_block, PURE_WHITE_BRUSH)
            End If

            .EndDraw()
        End With

        If Not EmptyMapBlock Then
            text_block.Dispose()
        End If
    End Sub

    Public Sub AvatarMouseEnter(e As GameMouseEventArgs)
        Me.HaveFocus = True
    End Sub
    Public Sub AvatarMouseLeave(e As GameMouseEventArgs)
        Me.HaveFocus = False
    End Sub
    Public Sub AvatarMouseMove(e As GameMouseEventArgs)
        e.Deliver = False

    End Sub

End Class
