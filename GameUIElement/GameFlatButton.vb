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

    Public Overrides Sub DrawControl(ByRef context As DeviceContext, ByRef spec As SpectatorCamera, canvasBitmap As Bitmap1)
        With context
            .FillRectangle(Me.BasicRect, GREY_COLOUR(2))
            .DrawRectangle(Me.BasicRect, BorderColour, 1.0F)
            .DrawBitmap(Me.TextImage.FontImage, Me.BasicRect, NOT_TRANSPARENT, BitmapInterpolationMode.Linear)

        End With

    End Sub
End Class
