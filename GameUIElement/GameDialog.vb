
Imports p104
Imports SharpDX.Direct2D1
''' <summary>
''' 游戏对话框基类
''' </summary>
Public MustInherit Class GameDialog
    Inherits GameBasicUIElement

    Protected Property Width As Integer
    Protected Property Height As Integer
    Protected Camera As SpectatorCamera = Nothing

    ''' <summary>
    ''' 初始化对话框
    ''' </summary>
    ''' <param name="inputWidth">宽度</param>
    ''' <param name="inputHeight">长度</param>
    ''' <param name="spec">Spectator</param>
    Public Sub InitializeDialog(inputWidth As Integer, inputHeight As Integer, spec As SpectatorCamera)
        With Me
            .Width = inputWidth
            .Height = inputHeight
            .Camera = spec
        End With
    End Sub

End Class
