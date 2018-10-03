
Imports Sharpdx.Direct2d1
''' <summary>
''' 游戏对话框基类
''' </summary>
Public MustInherit Class GameDialog
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

    Public MustOverride Sub PaintDialog(ByRef rt As WindowRenderTarget, ByRef spec As SpectatorCamera)

    Public Sub MouseDown(e As MouseEventArgs)
    End Sub

    Public Sub MouseMove(e As MouseEventArgs)
    End Sub

    Public Sub MouseUp(e As MouseEventArgs)
    End Sub

End Class
