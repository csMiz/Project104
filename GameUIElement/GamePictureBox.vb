﻿' -----------------------------------------
' Copyright (c) 2018 All Rights Reserved.
' 
' Filename: GamePictureBox
' Author: Miz
' Date: 2018/11/5 14:23:17
' -----------------------------------------

Imports p104
Imports SharpDX.Direct2D1
Imports SharpDX.Mathematics.Interop

''' <summary>
''' 游戏图片框控件类
''' </summary>
Public Class GamePictureBox
    Inherits GameBasicUIElement

    '图片绘制会超过控件边缘
    Public ImageSource As IGameImage

    Public Overrides Sub DrawControlAtSelfCanvas(ByRef context As DeviceContext, ByRef spec As SpectatorCamera, canvasBitmap As Bitmap1)
        With context
            .Target = Me.ControlCanvas
            .BeginDraw()
            .Clear(Nothing)

            Me.ImageSource.PaintFullImage(context)

            .EndDraw()
        End With
    End Sub
End Class
