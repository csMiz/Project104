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

    <Obsolete>
    Public Async Function MainGameLoopTest() As Task
        MainGame = New MainGameLoop

        Dim myCamera As New SpectatorCamera
        With myCamera
            .Resolve = New PointI(1024, 768)
            .CameraFocus = New PointF2(600, 600)
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
        'Dialog.InitializeColor()
        Dialog.InitializeConetentBox()
        Dialog.InitializeEffects()

        'TODO: put painting code into SkirmishGameLoop
        myCamera.PaintingLayers.Push(AddressOf Dialog.DrawControl)
        myCamera.PaintingLayersDescription.Push(GameImageLayer.Skirmish_UnitDetail)

        Application.DoEvents()

    End Function

    'Public Sub DWTest()
    '    Dim pDWriteFactory As New SharpDX.DirectWrite.Factory
    '    Dim fCollection As SharpDX.DirectWrite.FontCollection
    '    Dim pTextFormat As SharpDX.DirectWrite.TextFormat

    '    Dim fContext As MyFontContext = New MyFontContext(pDWriteFactory)
    '    Dim filepaths As New List(Of String)
    '    Dim fontFileFilePath As String = "C:\Users\asdfg\Desktop\Project104\p104\p104\bin\Debug\P104_Font1.ttf"
    '    filepaths.Add(fontFileFilePath)
    '    Dim hr As Integer = fContext.CreateFontCollection(filepaths, fCollection)
    '    pTextFormat = New SharpDX.DirectWrite.TextFormat(pDWriteFactory, "P104_Font1", fCollection, SharpDX.DirectWrite.FontWeight.Regular, SharpDX.DirectWrite.FontStyle.Normal, SharpDX.DirectWrite.FontStretch.Normal, 16)

    'End Sub

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
        Dim points As New List(Of PointF2)
        With points
            .Clear()
            .Add(New PointF2(0, 1))
            .Add(New PointF2(1, 1))
            .Add(New PointF2(1, -1))
            .Add(New PointF2(0, -0.5))
            .Add(New PointF2(-1, -1))
            .Add(New PointF2(-1, 0))
        End With
        area.Points = points.ToArray

        Dim testPoint As New PointF2(0, 0)
        If Not (area.IsInside(testPoint)) Then
            Throw New Exception("assertion failed")
        End If

        Dim pointsRaw As New List(Of SharpDX.Mathematics.Interop.RawVector2)
        With pointsRaw
            .Clear()
            .Add(New SharpDX.Mathematics.Interop.RawVector2(0, 1))
            .Add(New SharpDX.Mathematics.Interop.RawVector2(1, 1))
            .Add(New SharpDX.Mathematics.Interop.RawVector2(1, -1))
            .Add(New SharpDX.Mathematics.Interop.RawVector2(0, -0.5))
            .Add(New SharpDX.Mathematics.Interop.RawVector2(-1, -1))
            .Add(New SharpDX.Mathematics.Interop.RawVector2(-1, 0))
        End With
        area.PointsRaw = pointsRaw.ToArray

        If Not (area.IsInsideRaw(testPoint)) Then
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
        'Dialog.InitializeColor()
        Dialog.InitializeConetentBox()

        User.PaintingLayers.Push(AddressOf Dialog.DrawControl)
        User.PaintingLayersDescription.Push(GameImageLayer.Skirmish_UnitDetail)

        Application.DoEvents()
    End Sub

    Public Sub GoLTest()
        Dim gol As New TheGameOfLife
        gol.Initialize(16, 16)
        For j = 0 To 15
            For i = 0 To 1
                gol.MapBuffer(i, j) = 0
            Next
        Next
        gol.MapBuffer(0, 5) = &H8
        gol.MapBuffer(0, 6) = &H4
        gol.MapBuffer(0, 7) = &H1C

        For i = 0 To 60
            gol.Process()
            If i > 40 Then
                Debug.WriteLine("turn:" & i)
                gol.DebugResult()
            End If
        Next


    End Sub

    Public Sub OOPTest()
        'Dim animalList As New List(Of Animal)
        'Dim tmpAnimal As New Animal
        'Dim tmpDog As New Dog
        'animalList.Add(tmpAnimal)
        'animalList.Add(tmpDog)
        'Debug.WriteLine(AnimalControl.GetAnimalType(animalList(0)))
        'Debug.WriteLine(AnimalControl.GetAnimalType(animalList(1)))
        ''这样输出均为Animal

        Dim animalList As New List(Of Animal)
        Dim tmpAnimal As New Animal
        Dim tmpDog As New Dog
        animalList.Add(tmpAnimal)
        animalList.Add(tmpDog)
        For i = 0 To 1
            Call animalList(i).Eat()
        Next
        '这样输出是不同的，可行！
    End Sub

End Class

Public Class Animal
    Public Name As String = vbNullString

    Public Overridable Sub Eat()
        Debug.WriteLine("animal eat")
    End Sub
End Class

Public Class Dog
    Inherits Animal

    Public Overrides Sub Eat()
        Debug.WriteLine("dog eat")
    End Sub
End Class

Public Class AnimalControl
    Public Overloads Shared Function GetAnimalType(item As Animal) As String
        Return "animal"
    End Function

    Public Overloads Shared Function GetAnimalType(item As Dog) As String
        Return "dog"
    End Function

End Class
