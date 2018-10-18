
Imports System.Threading
Imports SharpDX.Direct2D1
''' <summary>
''' 游戏主流程
''' </summary>
Public Class MainGameLoop

    Private GameLoaded As MapLoadStatus = MapLoadStatus.NotLoaded
    Private GameLoadingProcess As Task

    Private SaveLoaded As MapLoadStatus = MapLoadStatus.NotLoaded
    Private SaveLoadingProcess As Task
    Private SelectedSaveIndex As Short = -1

    Private Camera As SpectatorCamera
    Private CameraD2DContext As DeviceContext
    Private Delegate Sub SimplePaint()
    Private CameraPaint As SimplePaint

    Private Skirmish As SkirmishGameLoop

    Private PaintThread As Thread = New Thread(AddressOf PaintGameImage)
    Private PaintFPS As Integer = 0


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
        If Me.PaintThread.ThreadState = ThreadState.Suspended Then
            Me.PaintThread.Resume()
            Return
        End If
        Me.PaintThread.Start()
    End Sub

    Public Sub SuspendPaint()
        Me.PaintThread.Suspend()
    End Sub

    Public Sub EndPaint()
        Try
            Me.PaintThread.Abort()
        Catch ex As Exception
        End Try
    End Sub

    Public Sub DrawSkirmish()
        With Me.Camera
            .PaintingLayers.Clear()
            .PaintingLayersDescription.Clear()

            .PaintingLayers.Push(AddressOf Me.Skirmish.SkirmishGameMap.DrawHexMap)
            .PaintingLayersDescription.Push(GameImageLayer.SkirmishMap)
        End With

    End Sub

    Public Sub PaintGameImage()
        Dim startTime As Date
        Dim endTime As Date
        Dim span As TimeSpan
        Do
            startTime = DateTime.Now
            CameraPaint.Invoke()
            Do
                endTime = DateTime.Now
                span = endTime - startTime
            Loop Until span.TotalMilliseconds > 33
            Me.PaintFPS = CInt(1000 / span.TotalMilliseconds)
        Loop
    End Sub

    Public Function GetFPS() As Integer
        Return Me.PaintFPS
    End Function

End Class
