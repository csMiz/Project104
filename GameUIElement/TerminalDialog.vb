' -----------------------------------------
' Copyright (c) 2019 All Rights Reserved.
' 
' Filename: TerminalDialog
' Author: Miz
' Date: 2019/3/7 20:46:17
' -----------------------------------------

Imports p104
Imports SharpDX.Direct2D1

''' <summary>
''' 控制台控件类
''' </summary>
Public Class TerminalDialog
    Inherits GameDialog
    Private Shared me_instance As TerminalDialog = Nothing
    ''' <summary>
    ''' 绑定的控制台组件
    ''' </summary>
    Public BindingTerminal As GameTerminal = Nothing

    Private Sub New()
        Me.BindingTerminal = GameTerminal.Instance
    End Sub

    Public Overrides Sub DrawControlAtSelfCanvas(ByRef context As DeviceContext, ByRef spec As SpectatorCamera, canvasBitmap As Bitmap1)
        Throw New NotImplementedException()
    End Sub

End Class
