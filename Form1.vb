Imports System.ComponentModel
Imports System.Threading
Imports SharpDX.XInput
Imports SharpDX.RawInput

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

        'Device.RegisterDevice(SharpDX.Multimedia.UsagePage.Generic, SharpDX.Multimedia.UsageId.GenericKeyboard, DeviceFlags.None)
        'AddHandler Device.KeyboardInput, AddressOf test_keydown

    End Sub

    Public Sub d2dPaint()
        Dim paintGame As New paintGame(AddressOf test.User.PaintImage)
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

        'Dim controller As Controller = New Controller(UserIndex.One)
        'Dim gamePad As Gamepad = controller.GetState().Gamepad  'xinput->手柄
        MsgBox(LoggingService.GetRecordCount)

    End Sub

    Private Sub Form1_KeyDown(sender As Object, e As KeyEventArgs) Handles Me.KeyDown
        If e.KeyCode = Keys.S Then
            test.User.CameraFocus.Y += 20
        End If
    End Sub

    'Public Sub test_keydown(sender As Object, args As KeyboardInputEventArgs)

    '    If (args.Key = Keys.W) Then
    '        MsgBox("match!")
    '    End If   '为什么不断变化的？

    'End Sub
End Class
