Imports System.IO
Imports Sharpdx.Direct2d1

''' <summary>
''' 游戏测试类
''' </summary>
Public Class GameTest

    Public TestMap As New SkirmishMap
    Public User As New SpectatorCamera
    Public Dialog As UnitDetailDialog = UnitDetailDialog.Instance


    Public Sub LoadMapTest()
        Dim stream As FileStream = New FileStream(Application.StartupPath & "\newmap.txt", FileMode.Open)
        TestMap.LoadFromFile(stream)

    End Sub

    Public Sub SkirmishMapAccessoryTest()
        TestMap.LoadAccessories(User.GetDevceContext, User.Zoom)

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

    Public Sub SpectatorTest()
        LoadMapTest()

        User.Resolve = New PointI(1024, 768)
        User.CameraFocus = New PointF2(300, 300)
        User.Zoom = 0.25

        User.InitializeDirect2d()

        SkirmishMapAccessoryTest()

        User.PaintingLayers.Push(AddressOf TestMap.DrawHexMap)
        User.PaintingLayersDescription.Push(GameImageLayer.SkirmishMap)

        'user.PaintImage()
        Application.DoEvents()



    End Sub

    Public Sub DialogTest()
        Dialog.InitializeDialog(900, 400, User)
        Dialog.BindUnit(UnitTemplates.GetHeroTemplate(0))
        Dialog.InitializeColor()
        Dialog.InitializeConetentBox()

        User.PaintingLayers.Push(AddressOf Dialog.PaintDialog)
        User.PaintingLayersDescription.Push(GameImageLayer.Skirmish_UnitDetail)

        Application.DoEvents()
    End Sub

End Class
