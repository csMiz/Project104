' -----------------------------------------
' Copyright (c) 2018 All Rights Reserved.
' 
' Filename: GameTreeViewItem
' Author: Miz
' Date: 2018/11/12 10:45:59
' -----------------------------------------

Imports p104
Imports SharpDX.Direct2D1
Imports SharpDX.Mathematics.Interop

Public Class GameTreeViewItem
    Inherits GameBasicUIElement

    Public Text As String = Nothing
    Public ReadOnly SubTreeBox As New GameContentFrame

    Private TextImage As TextItem = Nothing

    Public Overrides Sub DrawControlAtSelfCanvas(ByRef context As DeviceContext, ByRef spec As SpectatorCamera, canvasBitmap As Bitmap1)
        Throw New NotImplementedException()
    End Sub
End Class
