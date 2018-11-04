
Imports System.Threading
Imports SharpDX.Direct2D1
Imports SharpDX.Mathematics.Interop
''' <summary>
''' 游戏主流程
''' </summary>
Public Class MainGameLoop

    Private GameLoaded As MapLoadStatus = MapLoadStatus.NotLoaded
    Private GameLoadingProcess As Task

    Private SaveLoaded As MapLoadStatus = MapLoadStatus.NotLoaded
    Private SaveLoadingProcess As Task
    Private SelectedSaveIndex As Short = -1

    Public Camera As SpectatorCamera
    Public CameraD2DContext As DeviceContext
    Private Delegate Sub SimplePaint()
    Private CameraPaint As SimplePaint

    Public MainMenu As New GamePageProperty
    Public Skirmish As SkirmishGameLoop

    Private PaintThread As Thread = New Thread(AddressOf PaintGameImage)
    Private PaintSuspended As Boolean = False
    Public PaintFPS As Integer = 0

    Public Sub LoadMainMenu()
        Dim btnMissionSelect As New GameFlatButton
        With btnMissionSelect
            .BindDeviceContext(Me.CameraD2DContext)
            .BorderColour = New SolidColorBrush(Me.CameraD2DContext, New RawColor4(0.9, 0.9, 0.9, 1))
            .InitializeBasicRect(Camera.Resolve.X / 2 - 200, 400, Camera.Resolve.X / 2 + 200, 450)
            .Text = "Mission Select"
        End With
        Dim tmpMouseEnter = Sub()
                                btnMissionSelect.BorderColour = New SolidColorBrush(Me.CameraD2DContext, New RawColor4(1, 0, 0, 1))
                            End Sub
        Dim tmpMouseLeave = Sub()
                                btnMissionSelect.BorderColour = New SolidColorBrush(Me.CameraD2DContext, New RawColor4(0.9, 0.9, 0.9, 1))
                            End Sub
        Dim tmpMouseDown = Async Sub()
                               Debug.WriteLine("click!!")
                               Me.SuspendPaint()

                               Camera.ActivePages.Remove(Me.MainMenu)

                               Me.StartLoadSave(-1)
                               Dim loadResult As Integer = 0
                               loadResult = Await Me.WaitForLoadSave()
                               Me.StartLoadSkirmish()
                               loadResult = Await Me.WaitForLoadSkirmish()

                               Me.Skirmish.SkirmishGameMap.GenerateMoveRange(Me.Skirmish.UnitList(0))

                               Me.DrawSkirmish()
                               Me.StartPaint()
                           End Sub
        AddHandler btnMissionSelect.MouseEnter, tmpMouseEnter
        AddHandler btnMissionSelect.MouseLeave, tmpMouseLeave
        AddHandler btnMissionSelect.MouseDown, tmpMouseDown

        Dim btnExitGame As New GameFlatButton
        With btnExitGame
            .BindDeviceContext(Me.CameraD2DContext)
            .BorderColour = New SolidColorBrush(Me.CameraD2DContext, New RawColor4(0.9, 0.9, 0.9, 1))
            .InitializeBasicRect(Camera.Resolve.X / 2 - 200, 460, Camera.Resolve.X / 2 + 200, 510)
            .Text = "End Game"
        End With

        Me.MainMenu.UIElements.Add(btnMissionSelect)
        Me.MainMenu.UIElements.Add(btnExitGame)

        Me.MainMenu.GenerateElementsQuadtree(Me.Camera.Resolve)
        Me.MainMenu.InitializeCursor(Me.Camera.Resolve.X / 2, Me.Camera.Resolve.Y / 2)
    End Sub

    ''' <summary>
    ''' 加载全局资源，用户设置，不载入存档
    ''' </summary>
    Public Sub StartLoadGlobalResources()
        GameLoadingProcess = New Task(AddressOf LoadGlobalResources)
        GameLoadingProcess.Start()
    End Sub

    Private Sub LoadGlobalResources()
        Me.GameLoaded = MapLoadStatus.Loading
        Call GameResources.LoadResources(CameraD2DContext)
        Call Me.LoadMainMenu()
        Me.GameLoaded = MapLoadStatus.Loaded
    End Sub

    Public Async Function WaitForLoad() As Task(Of Integer)
        While (Me.GameLoaded <> MapLoadStatus.Loaded)
            Await Task.Delay(100)
        End While
        GameLoadingProcess.Dispose()
        Return 0
    End Function

    ''' <summary>
    ''' 载入存档
    ''' </summary>
    Public Sub StartLoadSave(saveIndex As Short)
        Me.SelectedSaveIndex = saveIndex
        SaveLoadingProcess = New Task(AddressOf LoadSave)
        SaveLoadingProcess.Start()
    End Sub

    Private Sub LoadSave()
        Me.SaveLoaded = MapLoadStatus.Loading

        If Me.SelectedSaveIndex = -1 Then    'New Game
            UnitTemplates.WrapUnits(My.Resources.AllUnits)

        Else    'Load Save
            'Dim saveContent As String = LoadBinarySave(Me.SelectedSaveIndex)
            'TODO
        End If

        Me.SaveLoaded = MapLoadStatus.Loaded
    End Sub

    Public Async Function WaitForLoadSave() As Task(Of Integer)
        While (Me.SaveLoaded <> MapLoadStatus.Loaded)
            Await Task.Delay(100)
        End While
        SaveLoadingProcess.Dispose()
        Return 0
    End Function

    ''' <summary>
    ''' 初始化camera
    ''' </summary>
    Public Sub InitializeCamera(inputCamera As SpectatorCamera)
        If Me.Camera IsNot Nothing Then Throw New Exception("camera has been initialized!")
        Me.Camera = inputCamera
        Me.CameraD2DContext = inputCamera.GetDevceContext
        Me.CameraPaint = AddressOf inputCamera.PaintImage
    End Sub

    ''' <summary>
    ''' 载入遭遇战资源
    ''' </summary>
    Public Sub StartLoadSkirmish()
        If Me.Skirmish Is Nothing Then Me.Skirmish = New SkirmishGameLoop
        Skirmish.BindingCamera = Me.Camera

        Dim campaignIndex As Short = 0  'TODO: depends on the Save or start a new campaign

        Skirmish.StartLoadSkirmishMapResources(campaignIndex)
    End Sub

    Public Async Function WaitForLoadSkirmish() As Task(Of Integer)
        Return Await Me.Skirmish.WaitForLoad()
    End Function

    Public Sub StartPaint()
        If Me.PaintSuspended Then
            Me.PaintSuspended = False
            Return
        End If
        Me.PaintThread.Start()
    End Sub

    Public Sub SuspendPaint()
        Me.PaintSuspended = True
    End Sub

    Public Sub EndPaint()
        Try
            Me.PaintThread.Abort()
        Catch ex As Exception
        End Try
    End Sub

    Public Sub DrawMainMenu()
        With Me.Camera
            .PaintingLayers.Clear()
            .PaintingLayersDescription.Clear()
            .ActivePages.Clear()

            .PaintingLayers.Push(AddressOf Me.MainMenu.PaintElements)
            .PaintingLayersDescription.Push(GameImageLayer.MainMenu)
            .ActivePages.Add(Me.MainMenu)
        End With
    End Sub

    Public Sub DrawSkirmish()
        With Me.Camera
            .PaintingLayers.Clear()
            .PaintingLayersDescription.Clear()

            .PaintingLayers.Push(AddressOf Me.Skirmish.DrawSkirmishMapLayer)
            .PaintingLayersDescription.Push(GameImageLayer.SkirmishMap)
        End With

    End Sub

    Public Sub PaintGameImage()
        Dim startTime As Date
        Dim endTime As Date
        Dim span As TimeSpan
        Do
            startTime = DateTime.Now
            If Not PaintSuspended Then
                CameraPaint.Invoke()
            End If
            Do
                endTime = DateTime.Now
                span = endTime - startTime
            Loop Until span.TotalMilliseconds > 33
            Me.PaintFPS = CInt(1000 / span.TotalMilliseconds)
        Loop
    End Sub


End Class
