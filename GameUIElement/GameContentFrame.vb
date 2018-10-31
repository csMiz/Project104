' -----------------------------------------
' Copyright (c) 2018 All Rights Reserved.
' 
' Filename: GameContentFrame
' Author: Miz
' Date: 2018/10/29 11:08:52
' -----------------------------------------

Imports p104
Imports SharpDX.Direct2D1
Imports SharpDX.Mathematics.Interop
''' <summary>
''' 内容框控件类
''' </summary>
Public Class GameContentFrame
    Inherits GameBasicUIElement

    Public Children As New List(Of GameBasicUIElement)

    Public Overrides Sub DrawControl(ByRef context As DeviceContext, ByRef spec As SpectatorCamera, canvasBitmap As Bitmap1)
        Throw New NotImplementedException()
    End Sub

End Class
