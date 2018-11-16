
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

    ''' <summary>
    ''' 游戏主菜单页
    ''' </summary>
    Public MainMenuPage As New GamePageProperty
    ''' <summary>
    ''' 游戏设置菜单页
    ''' </summary>
    Public SettingPage As New GamePageProperty

    Public GraphicsSettingPage As New GamePageProperty

    Public Skirmish As SkirmishGameLoop

    Public MainGameSettingRepository As New gamesettingRepository

    Private PaintThread As Thread = New Thread(AddressOf PaintGameImage)
    Private PaintSuspended As Boolean = False
    Public PaintFPS As Integer = 0

    Public Sub LoadMainMenu()
        Dim btnMissionSelect As New GameFlatButton
        With btnMissionSelect
            .BindDeviceContext(Me.CameraD2DContext)
            .BorderColour = New SolidColorBrush(Me.CameraD2DContext, New RawColor4(0.9, 0.9, 0.9, 1))
            .BasicRect = New RawRectangleF(Camera.Resolve.X / 2 - 200, 400, Camera.Resolve.X / 2 + 200, 450)
            .InitializeControlCanvas()
            .InitializeCursorLightBrush()
            .Text = "Start Game"
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

                               Camera.ActivePages.Remove(Me.MainMenuPage)

                               Me.StartLoadSave(-1)
                               Dim loadResult As Integer = 0
                               loadResult = Await Me.WaitForLoadSave()
                               Me.StartLoadSkirmish()
                               loadResult = Await Me.WaitForLoadSkirmish()
                               'Me.Skirmish.SkirmishGameMap.GenerateMoveRange(Me.Skirmish.UnitList(0))
                               Me.DrawSkirmish(False)
                               Me.StartPaint()
                           End Sub
        AddHandler btnMissionSelect.MouseEnter, tmpMouseEnter
        AddHandler btnMissionSelect.MouseLeave, tmpMouseLeave
        AddHandler btnMissionSelect.MouseDown, tmpMouseDown

        Dim btnSetting As New GameFlatButton
        With btnSetting
            .BindDeviceContext(Me.CameraD2DContext)
            .BorderColour = New SolidColorBrush(Me.CameraD2DContext, New RawColor4(0.9, 0.9, 0.9, 1))
            .BasicRect = New RawRectangleF(Camera.Resolve.X / 2 - 200, 460, Camera.Resolve.X / 2 + 200, 510)
            .InitializeControlCanvas()
            .InitializeCursorLightBrush()
            .Text = "Settings"
        End With
        Dim settingBtn_MouseDown = Sub()
                                       Debug.WriteLine("进入设置页")
                                       Me.SuspendPaint()
                                       Camera.ActivePages.Remove(Me.MainMenuPage)
                                       Camera.ActivePages.Add(Me.SettingPage)
                                       Camera.PaintingLayers.Clear()
                                       Camera.PaintingLayers.Push(AddressOf Me.SettingPage.PaintElements)
                                       Me.StartPaint()
                                   End Sub
        AddHandler btnSetting.MouseDown, settingBtn_MouseDown

        Dim btnExitGame As New GameFlatButton
        With btnExitGame
            .BindDeviceContext(Me.CameraD2DContext)
            .BorderColour = New SolidColorBrush(Me.CameraD2DContext, New RawColor4(0.9, 0.9, 0.9, 1))
            .BasicRect = New RawRectangleF(Camera.Resolve.X / 2 - 200, 520, Camera.Resolve.X / 2 + 200, 570)
            .InitializeControlCanvas()
            .InitializeCursorLightBrush()
            .Text = "End Game"
        End With

        Me.MainMenuPage.UIElements.Add(btnMissionSelect)
        Me.MainMenuPage.UIElements.Add(btnSetting)
        Me.MainMenuPage.UIElements.Add(btnExitGame)

        Me.MainMenuPage.GenerateElementsQuadtree(Me.Camera.Resolve)
        Me.MainMenuPage.InitializeCursor(Me.Camera.CurrentCursorPosition)
    End Sub

    Public Sub LoadSettingPage()


        '1 图像设置
        Dim menu_1 As New GameFlatButton
        With menu_1
            .BindDeviceContext(Me.CameraD2DContext)
            .BorderColour = New SolidColorBrush(Me.CameraD2DContext, New RawColor4(0.9, 0.9, 0.9, 1))
            .BasicRect = New RawRectangleF(100, 300, 500, 350)
            .InitializeControlCanvas()
            .InitializeCursorLightBrush()
            .Text = "Graphics"       'TODO： 使用TextResource
        End With
        Dim menu_1_click = Sub()
                               Debug.WriteLine("图像设置")
                               Me.SuspendPaint()
                               Camera.ActivePages.Remove(Me.SettingPage)
                               Camera.ActivePages.Add(Me.GraphicsSettingPage)
                               Camera.PaintingLayers.Clear()
                               Camera.PaintingLayers.Push(AddressOf Me.GraphicsSettingPage.PaintElements)
                               Me.StartPaint()
                           End Sub
        AddHandler menu_1.MouseDown, menu_1_click

        '1 声音设置
        Dim menu_2 As New GameFlatButton
        With menu_2
            .BindDeviceContext(Me.CameraD2DContext)
            .BorderColour = New SolidColorBrush(Me.CameraD2DContext, New RawColor4(0.9, 0.9, 0.9, 1))
            .BasicRect = New RawRectangleF(100, 360, 500, 410)
            .InitializeControlCanvas()
            .InitializeCursorLightBrush()
            .Text = "Audio"       'TODO： 使用TextResource
        End With
        Dim menu_2_click = Sub()
                               Debug.WriteLine("声音设置")
                           End Sub
        AddHandler menu_2.MouseDown, menu_2_click


        Dim scrollPage1 As New GameScrollViewer(True)
        Dim scrollWidth As Integer = CInt(Me.Camera.Resolve.X * 0.8)
        Dim scrollLeft As Integer = CInt((Me.Camera.Resolve.X - scrollWidth) / 2)
        With scrollPage1
            .BasicRect = New RawRectangleF(scrollLeft, 0, scrollLeft + scrollWidth, Me.Camera.Resolve.Y)
            .BindingContext = Me.CameraD2DContext
            .InitializeControlCanvas()
        End With

        '1-1 窗口设置
        Dim menu_1_1 As New GameComboBox
        With menu_1_1
            .BasicRect = New RawRectangleF(0, 0, 800, 100)
            .TitleString = "显示模式"
            .SelectionStrings.Add("窗口模式")
            .SelectionStrings.Add("全屏模式")
            .SelectedIndex = 0
            .BindingContext = Me.CameraD2DContext
            .InitializeComboBox(500)
        End With

        Dim menu_1_2 As New GameComboBox
        With menu_1_2
            .BasicRect = New RawRectangleF(0, 100, 800, 200)
            .TitleString = "分辨率"
            .SelectionStrings.Add("800*600")
            .SelectionStrings.Add("1024*768")
            .SelectionStrings.Add("1280*768")
            .SelectionStrings.Add("1280*800")
            .SelectionStrings.Add("1440*900")
            .BindingContext = Me.CameraD2DContext
            .InitializeComboBox(500)
        End With

        menu_1_1.RelativeNextItem = menu_1_2
        menu_1_2.RelativeLastItem = menu_1_1


        With scrollPage1
            .Children.Add(menu_1_1)
            .Children.Add(menu_1_2)
            .GenerateShownChildren()
        End With

        GraphicsSettingPage.UIElements.Add(scrollPage1)
        GraphicsSettingPage.GenerateElementsQuadtree(Me.Camera.Resolve)
        GraphicsSettingPage.InitializeCursor(Me.Camera.CurrentCursorPosition)

        Me.SettingPage.UIElements.Add(menu_1)
        Me.SettingPage.UIElements.Add(menu_2)
        Me.SettingPage.GenerateElementsQuadtree(Me.Camera.Resolve)
        Me.SettingPage.InitializeCursor(Me.Camera.CurrentCursorPosition)
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
        Call Me.LoadSettingPage()
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

            .PaintingLayers.Push(AddressOf Me.MainMenuPage.PaintElements)
            .PaintingLayersDescription.Push(GameImageLayer.MainMenu)
            .ActivePages.Add(Me.MainMenuPage)
        End With
    End Sub

    Public Sub DrawSkirmish(Optional overwrite As Boolean = True)
        With Me.Camera
            If overwrite Then
                .PaintingLayers.Clear()
                .PaintingLayersDescription.Clear()
            End If
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
