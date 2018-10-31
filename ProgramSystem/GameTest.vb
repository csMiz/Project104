Imports System.IO
Imports Sharpdx.Direct2d1

''' <summary>
''' 游戏测试类
''' </summary>
Public Class GameTest
    <Obsolete("use SkirmishGameloop", True)>
    Public TestMap As New SkirmishMap
    Public User As New SpectatorCamera
    Public Dialog As UnitDetailDialog = UnitDetailDialog.Instance
    Public TestGameLoop As New SkirmishGameLoop

    Public MainGame As MainGameLoop


    Public Async Function MainGameLoopTest() As Task
        MainGame = New MainGameLoop

        Dim myCamera As New SpectatorCamera
        With myCamera
            .Resolve = New PointI(1024, 768)
            .CameraFocus = New PointF2(300, 300)
            .Zoom = 0.25
            .InitializeDirect2d()
        End With
        MainGame.InitializeCamera(myCamera)

        'TODO: draw a image of "Loading..."
        'MainGame.DrawLoadingPage()

        MainGame.StartLoadGlobalResources()
        Dim loadResult As Integer = Await MainGame.WaitForLoad()

        'TODO: draw main menu
        MainGame.DrawMainMenu()

        MainGame.StartPaint()
        Return

        'user select "New Game"
        MainGame.StartLoadSave(-1)
        loadResult = Await MainGame.WaitForLoadSave()

        'draw Skirmish or Menu
        'TODO: draw a image of "Loading..."
        MainGame.StartLoadSkirmish()
        loadResult = Await MainGame.WaitForLoadSkirmish()

        'draw SkirmishMap
        MainGame.DrawSkirmish()
        MainGame.StartPaint()

        'start state machine
        MainGame.Skirmish.StartSkirmishGameStateMachine()
        'state machine should stop at Main_A_Phase

        'open unit detail dialog
        'TODO: put initialize codes into LoadResources
        Dialog.InitializeDialog(myCamera.Resolve.X, myCamera.Resolve.Y, myCamera)
        Dialog.BindUnit(UnitTemplates.GetWrappedHeroTemplate(0))
        Dialog.InitializeColor()
        Dialog.InitializeConetentBox()
        Dialog.InitializeEffects()

        'TODO: put painting code into SkirmishGameLoop
        myCamera.PaintingLayers.Push(AddressOf Dialog.DrawControl)
        myCamera.PaintingLayersDescription.Push(GameImageLayer.Skirmish_UnitDetail)

        Application.DoEvents()

    End Function

    <Obsolete("此部分已经封装在SkirmishGameLoop里了->SkirmishGameLoopTest()", False)>
    Public Sub LoadMapTest()
        Dim stream As FileStream = New FileStream(Application.StartupPath & "\newmap.txt", FileMode.Open)
        TestMap.LoadFromFile(stream)

    End Sub

    Public Sub SkirmishGameLoopTest()
        TestGameLoop.BindingCamera = User
        TestGameLoop.StartLoadSkirmishMapResources(0)

    End Sub

    <Obsolete("已经集成在SkirmishGameloop里了", True)>
    Public Sub SkirmishMapAccessoryTest()
        'TestMap.LoadAccessories(User.GetDevceContext, User.Zoom)
        TestGameLoop.SkirmishGameMap.LoadAccessories(User.GetDevceContext, User.Zoom)

    End Sub

    Public Sub AreaTest()
        Dim area As New PolygonArea
        With area.Points
            .Add(New PointF2(0, 1))
            .Add(New PointF2(1, 1))
            .Add(New PointF2(1, -1))
            .Add(New PointF2(0, -0.5))
            .Add(New PointF2(-1, -1))
            .Add(New PointF2(-1, 0))

        End With

        Dim testPoint As New PointF2(0, 0)
        If Not (area.IsInside(testPoint)) Then
            Throw New Exception("assertion failed")
        End If

    End Sub

    Public Sub UnitTest()
        Dim testKedama As GameUnit = UnitTemplates.GetUnitTemplate(0)
        Dim testYousei As GameUnit = UnitTemplates.GetUnitTemplate(1)
        Dim testYoukai As GameUnit = UnitTemplates.GetUnitTemplate(2)

        Dim testReimu As GameHero = UnitTemplates.GetHeroTemplate(0)

    End Sub

    Public Sub CopyHeroTest()
        Dim testReimu As GameHero = UnitTemplates.GetHeroTemplate(0)
        Dim testReimu2 As GameHero = testReimu.Copy
        testReimu.SetLevel(5, LogSenderType.Change_Program)
        testReimu2.SetLevel(2, LogSenderType.Change_Program)
        If (testReimu.GetLevel = testReimu2.GetLevel) Then
            Throw New Exception("assertion failed")
        End If

    End Sub

    Public Sub SpectatorTest()
        'LoadMapTest()


        User.Resolve = New PointI(1024, 768)
        User.CameraFocus = New PointF2(300, 300)
        User.Zoom = 0.25


        User.InitializeDirect2d()

        SkirmishGameLoopTest()

        'SkirmishMapAccessoryTest()

        'User.PaintingLayers.Push(AddressOf TestMap.DrawHexMap)
        User.PaintingLayers.Push(AddressOf TestGameLoop.SkirmishGameMap.DrawHexMap)
        User.PaintingLayersDescription.Push(GameImageLayer.SkirmishMap)

        'user.PaintImage()
        Application.DoEvents()



    End Sub

    Public Sub DialogTest()
        Dialog.InitializeDialog(900, 400, User)
        Dialog.BindUnit(UnitTemplates.GetHeroTemplate(0))
        Dialog.InitializeColor()
        Dialog.InitializeConetentBox()

        User.PaintingLayers.Push(AddressOf Dialog.DrawControl)
        User.PaintingLayersDescription.Push(GameImageLayer.Skirmish_UnitDetail)

        Application.DoEvents()
    End Sub

End Class
