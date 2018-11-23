' -----------------------------------------
' Copyright (c) 2018 All Rights Reserved.
' 
' Filename: GameTextBlock
' Author: Miz
' Date: 2018/11/5 14:50:29
' -----------------------------------------

Imports p104
Imports SharpDX.Direct2D1
Imports SharpDX.Mathematics.Interop

Public Class GameTextBlock
    Inherits GameBasicUIElement

    Public Text As String = Nothing

    Private TextImage As TextItem = Nothing


    Public Overrides Sub DrawControlAtSelfCanvas(ByRef context As DeviceContext, ByRef spec As SpectatorCamera, canvasBitmap As Bitmap1)
        Throw New NotImplementedException()
    End Sub
End Class
