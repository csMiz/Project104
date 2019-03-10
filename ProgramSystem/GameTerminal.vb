' -----------------------------------------
' Copyright (c) 2019 All Rights Reserved.
' 
' Filename: GameTerminal
' Author: Miz
' Date: 2019/3/7 20:40:29
' -----------------------------------------

''' <summary>
''' 游戏控制台终端类
''' </summary>
Public Class GameTerminal
    Private Shared me_instance As GameTerminal = Nothing

    ''' <summary>
    ''' 显示文本
    ''' <para>显示最多100行，每行最多40个字符</para>
    ''' </summary>
    Public TerminalContent(99) As String

    Private Sub New()
    End Sub

    ''' <summary>
    ''' 单例
    ''' </summary>
    Public Shared Function Instance() As GameTerminal
        If me_instance Is Nothing Then me_instance = New GameTerminal
        Return me_instance
    End Function

    ''' <summary>
    ''' 输入指令，最大长度255
    ''' </summary>
    Public Sub CommandInput(input As String)

    End Sub



End Class
