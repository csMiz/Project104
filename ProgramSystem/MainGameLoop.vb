
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
    Private MainMenuGrid As GameContentFrame = Nothing
    Private MMG_Button1 As GameFlatButton = Nothing
    Private MMG_Button2 As GameFlatButton = Nothing
    Private MMG_Button3 As GameFlatButton = Nothing
    Private SettingGrid As GameContentFrame = Nothing
    Private SG_Button1 As GameFlatButton = Nothing
    Private SG_Button2 As GameFlatButton = Nothing
    Private GraphicsSettingGrid As GameContentFrame = Nothing
    Private GG_Scroll1 As GameScrollViewer = Nothing
    Private GG_Combo1 As GameComboBox = Nothing
    Private GG_Combo2 As GameComboBox = Nothing
    Private GG_Combo3 As GameComboBox = Nothing
    Private GG_Shadow1 As GameShadowPad = Nothing

    Public Skirmish As SkirmishGameLoop

    Public MainGameSettingRepository As New gamesettingRepository

    Private PaintThread As Thread = New Thread(AddressOf PaintGameImage)
    Private PaintSuspended As Boolean = False
    Public PaintFPS As Integer = 0

    Public Sub LoadMainMenuControls()
        Me.LoadMainGridControls()
        Me.LoadMainGridEvents()
        Me.LoadSettingGridControls()
        Me.LoadSettingGridEvents()
        Me.LoadGraphicsGridControls()
        Me.LoadGraphicsGridEvents()

        Me.MainMenuPage.GenerateElementsQuadtree(Me.Camera.Resolve)
        Me.MainMenuPage.InitializeCursor(Me.Camera.CurrentCursorPosition, Me.Camera.Resolve)
    End Sub

    Private Sub LoadMainGridControls()
        Me.MainMenuGrid = New GameContentFrame
        With Me.MainMenuGrid
            .BindingContext = Me.CameraD2DContext
            .BasicRect = Camera.ResolveRectangle
            .AbsoluteRect = .BasicRect
            .InitializeControlCanvas()
            .Visible = True
        End With

        'Start Game Button
        Me.MMG_Button1 = New GameFlatButton
        With Me.MMG_Button1
            .BindingContext = Me.CameraD2DContext
            .BorderColour = BLACK_COLOUR_BRUSH(2)
            .BasicRect = New RawRectangleF(Camera.Resolve.X / 2 - 200, 400, Camera.Resolve.X / 2 + 200, 450)
            .AbsoluteRect = .BasicRect
            .InitializeControlCanvas()
            .InitializeCursorLightBrush()
            .InitializeBorderStyle()
            .Text = "Start Game"
        End With

        'Settings Button
        Me.MMG_Button2 = New GameFlatButton
        With Me.MMG_Button2
            .BindingContext = Me.CameraD2DContext
            .BorderColour = BLACK_COLOUR_BRUSH(2)
            .BasicRect = New RawRectangleF(Camera.Resolve.X / 2 - 200, 460, Camera.Resolve.X / 2 + 200, 510)
            .AbsoluteRect = .BasicRect
            .InitializeControlCanvas()
            .InitializeCursorLightBrush()
            .InitializeBorderStyle()
            .Text = "Settings"
        End With

        'Exit Button
        Me.MMG_Button3 = New GameFlatButton
        With Me.MMG_Button3
            .BindingContext = Me.CameraD2DContext
            .BorderColour = BLACK_COLOUR_BRUSH(2)
            .BasicRect = New RawRectangleF(Camera.Resolve.X / 2 - 200, 520, Camera.Resolve.X / 2 + 200, 570)
            .AbsoluteRect = .BasicRect
            .InitializeControlCanvas()
            .InitializeCursorLightBrush()
            .InitializeBorderStyle()
            .Text = "End Game"
        End With

        'Dim tmpTB As New GameTextBlock(True)
        'With tmpTB
        '    .BindingContext = Me.CameraD2DContext
        '    .DWFontBrush = BLACK_COLOUR_BRUSH(4)
        '    .BasicRect = New RawRectangleF(20, 20, 300, 300)
        '    .AbsoluteRect = .BasicRect
        '    .InitializeControlCanvas()
        '    .Text = "My TextBlock"
        'End With

        With Me.MainMenuGrid
            .Children.Add(MMG_Button1)
            .Children.Add(MMG_Button2)
            .Children.Add(MMG_Button3)
            '.Children.Add(tmpTB)
            .InitializeQuadtree(Me.Camera.Resolve)
        End With
        Me.MainMenuPage.UIElements.Add(Me.MainMenuGrid)
    End Sub

    Private Sub LoadMainGridEvents()
        ' TODO: use MouseUp
        AddHandler MMG_Button1.MouseDown, AddressOf Me.MMG_Button1_MouseDown
        AddHandler MMG_Button2.MouseDown, AddressOf Me.MMG_Button2_MouseDown

    End Sub

    Private Async Sub MMG_Button1_MouseDown()
        'Debug.WriteLine("click!!")
        'TODO: Draw an image of 'Loading...'
        Me.SuspendPaint()
        Me.StartLoadSave(-1)
        Dim loadResult As Integer = 0
        loadResult = Await Me.WaitForLoadSave()
        Me.StartLoadSkirmish()
        loadResult = Await Me.WaitForLoadSkirmish()
        'Me.Skirmish.SkirmishGameMap.GenerateMoveRange(Me.Skirmish.UnitList(0))
        Me.DrawSkirmish(True)
        Me.StartPaint()
    End Sub
    Private Sub MMG_Button2_MouseDown()
        Debug.WriteLine("进入设置页")
        Me.SuspendPaint()
        MainMenuGrid.Visible = False
        SettingGrid.Visible = True
        Me.StartPaint()
    End Sub

    Private Sub LoadSettingGridControls()
        Me.SettingGrid = New GameContentFrame
        With Me.SettingGrid
            .BindingContext = Me.CameraD2DContext
            .BasicRect = Camera.ResolveRectangle
            .AbsoluteRect = .BasicRect
            .InitializeControlCanvas()
            .Visible = False
        End With

        '1 图像设置
        Me.SG_Button1 = New GameFlatButton
        With SG_Button1
            .BindingContext = Me.CameraD2DContext
            .BorderColour = BLACK_COLOUR_BRUSH(2)
            .BasicRect = New RawRectangleF(100, 300, 500, 350)
            .AbsoluteRect = .BasicRect
            .InitializeControlCanvas()
            .InitializeCursorLightBrush()
            .InitializeBorderStyle()
            .Text = "Graphics"       'TODO： 使用TextResource
        End With

        '1 声音设置
        Me.SG_Button2 = New GameFlatButton
        With SG_Button2
            .BindingContext = Me.CameraD2DContext
            .BorderColour = BLACK_COLOUR_BRUSH(2)
            .BasicRect = New RawRectangleF(100, 360, 500, 410)
            .AbsoluteRect = .BasicRect
            .InitializeControlCanvas()
            .InitializeCursorLightBrush()
            .InitializeBorderStyle()
            .Text = "Audio"       'TODO： 使用TextResource
        End With

        With SettingGrid
            .Children.Add(SG_Button1)
            .Children.Add(SG_Button2)
            .InitializeQuadtree(Me.Camera.Resolve)
        End With

        Me.MainMenuPage.UIElements.Add(Me.SettingGrid)
    End Sub

    Private Sub LoadSettingGridEvents()
        AddHandler SG_Button1.MouseDown, AddressOf SG_Button1_MouseDown
        AddHandler SG_Button2.MouseDown, AddressOf SG_Button2_MouseDown

    End Sub

    Private Sub SG_Button1_MouseDown()
        Debug.WriteLine("图像设置")
        Me.SuspendPaint()    '这里的Suspend&Start可以去掉
        Me.SettingGrid.Visible = False
        Me.GraphicsSettingGrid.Visible = True
        Me.StartPaint()
    End Sub
    Private Sub SG_Button2_MouseDown()
        Debug.WriteLine("声音设置")
    End Sub

    Private Sub LoadGraphicsGridControls()
        Me.GraphicsSettingGrid = New GameContentFrame
        With Me.GraphicsSettingGrid
            .BindingContext = Me.CameraD2DContext
            .BasicRect = Camera.ResolveRectangle
            .AbsoluteRect = .BasicRect
            .InitializeControlCanvas()
            .Visible = False
        End With

        Me.GG_Scroll1 = New GameScrollViewer
        Dim scrollWidth As Integer = CInt(Me.Camera.Resolve.X * 0.8)
        Dim scrollLeft As Integer = CInt((Me.Camera.Resolve.X - scrollWidth) / 2)
        With GG_Scroll1
            .BasicRect = New RawRectangleF(scrollLeft, 0, scrollLeft + scrollWidth, Me.Camera.Resolve.Y)
            .AbsoluteRect = .BasicRect
            .BindingContext = Me.CameraD2DContext
            .InitializeControlCanvas()
        End With

        '1-1 窗口设置
        Me.GG_Shadow1 = New GameShadowPad
        With GG_Shadow1
            .BasicRect = Me.Camera.ResolveRectangle
            .AbsoluteRect = .BasicRect
            .BindingContext = Me.CameraD2DContext
            .InitializeControlCanvas()
            .DefaultBackground = BLACK_COLOUR_BRUSH(3)
            .Visible = False
        End With

        Me.GG_Combo1 = New GameComboBox
        With GG_Combo1
            .BasicRect = New RawRectangleF(0, 0, 800, 50)
            .TitleString = "显示模式"
            .SelectionStrings.Add("窗口模式")
            .SelectionStrings.Add("全屏模式")
            .SelectedIndex = 0
            .BindingContext = Me.CameraD2DContext
            .InitializeComboBox(500)
            .ImportShadowPad(GG_Shadow1, MainMenuPage.UIElements)
        End With

        Me.GG_Combo2 = New GameComboBox
        With GG_Combo2
            .BasicRect = New RawRectangleF(0, 50, 800, 100)
            .TitleString = "分辨率"
            .SelectionStrings.Add("800*600")
            .SelectionStrings.Add("1024*768")
            .SelectionStrings.Add("1280*768")
            .SelectionStrings.Add("1280*800")
            .SelectionStrings.Add("1440*900")
            .SelectedIndex = 0
            .BindingContext = Me.CameraD2DContext
            .InitializeComboBox(500)
            .ImportShadowPad(GG_Shadow1, MainMenuPage.UIElements)
        End With

        Me.GG_Combo3 = New GameComboBox
        With GG_Combo3
            .BasicRect = New RawRectangleF(0, 100, 800, 150)
            .TitleString = "图像绘制帧速率"
            .SelectionStrings.Add("30")
            .SelectionStrings.Add("60")
            .SelectedIndex = 0
            .BindingContext = Me.CameraD2DContext
            .InitializeComboBox(500)
            .ImportShadowPad(GG_Shadow1, MainMenuPage.UIElements)
        End With

        GG_Combo1.RelativeNextItem = GG_Combo2
        GG_Combo2.RelativeLastItem = GG_Combo1
        GG_Combo2.RelativeNextItem = GG_Combo3
        GG_Combo3.RelativeLastItem = GG_Combo2

        With GG_Scroll1
            .Children.Add(GG_Combo1)
            .Children.Add(GG_Combo2)
            .Children.Add(GG_Combo3)
            .GenerateShownChildren()
        End With

        With GraphicsSettingGrid
            .Children.Add(GG_Shadow1)
            .Children.Add(GG_Scroll1)
            .InitializeQuadtree(Me.Camera.Resolve)
        End With

        Me.MainMenuPage.UIElements.Add(Me.GraphicsSettingGrid)

    End Sub

    Private Sub LoadGraphicsGridEvents()

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
        Call Me.LoadMainMenuControls()

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

    ''' <summary>
    ''' 绘制Skirmish图
    ''' </summary>
    ''' <param name="overwrite">清空所有图层</param>
    Public Sub DrawSkirmish(Optional overwrite As Boolean = True)
        With Me.Camera
            If overwrite Then
                .PaintingLayers.Clear()
                .PaintingLayersDescription.Clear()
                .ActivePages.Clear()
            End If
            .PaintingLayers.Push(AddressOf Me.Skirmish.DrawSkirmishMapLayer)
            .PaintingLayersDescription.Push(GameImageLayer.SkirmishMap)
            .ActivePages.Add(Me.Skirmish.SkirmishPage)
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
