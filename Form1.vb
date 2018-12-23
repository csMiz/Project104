Imports System.ComponentModel
Imports System.Threading
Imports SharpDX.XInput
Imports SharpDX.RawInput

Public Class Form1
    Public test As New GameTest
    Private PaintThread As Thread
    Private Delegate Sub paintGame()

    Private Async Sub Form1_Load(sender As Object, e As EventArgs) Handles Me.Load
        Me.Show()
        'Call test.SpectatorTest()
        'Call test.UnitTest()
        'Call test.CopyHeroTest()
        ''Call test.DialogTest()

        'PaintThread = New Thread(AddressOf d2dPaint)

        'Dim waitResult As Integer = Await test.TestGameLoop.WaitForLoad()
        'PaintThread.Start()

        'test.GoLTest()

        Await test.MainGameLoopTest()

        'test.OOPTest()

        'Device.RegisterDevice(SharpDX.Multimedia.UsagePage.Generic, SharpDX.Multimedia.UsageId.GenericKeyboard, DeviceFlags.None)
        'AddHandler Device.KeyboardInput, AddressOf test_keydown

    End Sub

    Public Sub d2dPaint()
        Dim paintGame As New paintGame(AddressOf test.User.PaintImage)
        Dim starttime As Date
        Dim endtime As Date
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
        'PaintThread.Abort()
        test.MainGame.EndPaint()
    End Sub

    Private Sub Form1_MouseDown(sender As Object, e As MouseEventArgs) Handles Me.MouseDown
        Dim args As GameMouseEventArgs = GameMouseEventArgs.FromMouseEventArgs(e, Me.ClientRectangle, test.MainGame.Camera.Resolve)
        test.MainGame.Camera.CurrentCursorPosition = args.Position
        Debug.WriteLine("Mouse Clicked at: " & args.PrintPositionString)
        For i = 0 To test.MainGame.Camera.ActivePages.Count - 1
            Dim page As GamePageProperty = test.MainGame.Camera.ActivePages(i)
            page.TriggerMouseDown(args)
        Next

        'Dim controller As Controller = New Controller(UserIndex.One)
        'Dim gamePad As Gamepad = controller.GetState().Gamepad  'xinput->手柄
        Debug.WriteLine("Monitor Count: " & LoggingService.GetRecordCount & ", FPS: " & test.MainGame.PaintFPS)

    End Sub

    Private Sub Form1_KeyDown(sender As Object, e As KeyEventArgs) Handles Me.KeyDown
        If e.KeyCode = Keys.S Then
            test.User.CameraFocus.Y += 20
        End If
    End Sub

    Private Sub Form1_MouseMove(sender As Object, e As MouseEventArgs) Handles Me.MouseMove
        Dim args As GameMouseEventArgs = GameMouseEventArgs.FromMouseEventArgs(e, Me.ClientRectangle, test.MainGame.Camera.Resolve)
        Dim args2 As GameMouseEventArgs = GameMouseEventArgs.FromMouseEventArgs(e, Me.ClientRectangle, test.MainGame.Camera.Resolve)
        test.MainGame.Camera.CurrentCursorPosition = args.Position
        For i = 0 To test.MainGame.Camera.ActivePages.Count - 1
            Dim page As GamePageProperty = test.MainGame.Camera.ActivePages(i)
            page.TriggerMouseMove(args)
            page.TriggerGlobalMouseMove(args2)
        Next
    End Sub

    Private Sub Form1_MouseUp(sender As Object, e As MouseEventArgs) Handles Me.MouseUp
        'TODO
        Dim args As GameMouseEventArgs = GameMouseEventArgs.FromMouseEventArgs(e, Me.ClientRectangle, test.MainGame.Camera.Resolve)
        test.MainGame.Camera.CurrentCursorPosition = args.Position
        For i = 0 To test.MainGame.Camera.ActivePages.Count - 1
            Dim page As GamePageProperty = test.MainGame.Camera.ActivePages(i)
            page.TriggerMouseUp(args)
        Next
    End Sub

    'Public Sub test_keydown(sender As Object, args As KeyboardInputEventArgs)

    '    If (args.Key = Keys.W) Then
    '        MsgBox("match!")
    '    End If   '为什么不断变化的？

    'End Sub
End Class
