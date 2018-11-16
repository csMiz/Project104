' -----------------------------------------
' Copyright (c) 2018 All Rights Reserved.
' 
' Filename: GameInteractiveRectangle
' Author: Miz
' Date: 2018/11/5 11:10:37
' -----------------------------------------

Imports p104
Imports SharpDX.Direct2D1
Imports SharpDX.Mathematics.Interop

Public Class GameInteractiveRectangle
    Inherits GameBasicUIElement

    Public Overrides Sub DrawControl(ByRef context As DeviceContext, ByRef spec As SpectatorCamera, canvasBitmap As Bitmap1, newRect As RawRectangleF)
        'Interactive Rectangle is only a layer for mouse events.
        'It is invisible.
    End Sub
End Class
