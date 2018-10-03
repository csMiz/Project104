Imports System.ComponentModel
Imports System.Threading

Public Class Form1
    Private test As New GameTest
    Private PaintThread As Thread
    Private Delegate Sub paintGame()

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles Me.Load
        Me.Show()
        Call test.SpectatorTest()
        Call test.UnitTest()
        'Call test.DialogTest()

        PaintThread = New Thread(AddressOf d2dPaint)
        PaintThread.Start()

    End Sub

    Public Sub d2dPaint()
        Dim paintGame As New paintGame(AddressOf test.user.PaintImage)
        Dim starttime
        Dim endtime
        Dim a1 As TimeSpan
        Do
            starttime = DateTime.Now
            Me.Invoke(paintGame)
L1:
            endtime = DateTime.Now
            a1 = endtime - starttime
            If a1.TotalMilliseconds <= 33 Then GoTo L1
        Loop
    End Sub

    Private Sub Form1_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        PaintThread.Abort()
    End Sub

    Private Sub Form1_MouseDown(sender As Object, e As MouseEventArgs) Handles Me.MouseDown

    End Sub
End Class
