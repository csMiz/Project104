Imports System.IO
Imports System.Xml
''' <summary>
''' 遭遇战GameLoop
''' </summary>
Public Class SkirmishGameLoop

    Private GameLoopStatus As SingleGameLoopStage = SingleGameLoopStage.MyTurnStart
    Private MapLoaded As MapLoadStatus = MapLoadStatus.NotLoaded
    Private GameEnded As GameEndInfo

    Private CampaignIndex As Short = -1
    Public SkirmishGameMap As New SkirmishMap
    Public BindingCamera As SpectatorCamera

    ''' <summary>
    ''' 势力
    ''' </summary>
    Public Property Sides As New List(Of GameSideInfo)
    ''' <summary>
    ''' 当前回合数
    ''' </summary>
    Public Property NowTurn As Short = 0
    ''' <summary>
    ''' 此游戏中的单位列表
    ''' </summary>
    Public UnitList As New List(Of GameUnit)
    ''' <summary>
    ''' 此游戏中的Flag列表
    ''' </summary>
    Public FlagList As New List(Of GameFlag)
    ''' <summary>
    ''' 此游戏中的建筑物列表
    ''' </summary>
    Public BuildingList As New List(Of GameBuilding)


    Public Sub StartLoadSkirmishMapResources(missionIndex As Short)
        If missionIndex < 0 Then Throw New Exception("invalid mission")
        CampaignIndex = missionIndex
        '建立独立线程
        Dim loadProcess As New Task(AddressOf LoadResources)
        loadProcess.Start()

    End Sub

    Private Sub LoadResources()
        MapLoaded = MapLoadStatus.Loading

        If CampaignIndex = 0 Then
            Dim stream As FileStream = New FileStream(Application.StartupPath & "\newmap.txt", FileMode.Open)
            SkirmishGameMap.LoadFromFile(stream)
        End If

        SkirmishGameMap.LoadAccessories(BindingCamera.GetDevceContext, BindingCamera.Zoom)

        Me.LoadUnitsFromXMLAndTemplates(GetCampaignScript(0))

        SkirmishGameMap.ResourcesLoaded = True
        MapLoaded = MapLoadStatus.Loaded
    End Sub

    Public Async Function WaitForLoad() As Task(Of Integer)
        While (Me.MapLoaded <> MapLoadStatus.Loaded)
            Await Task.Delay(100)
        End While
        Return 0
    End Function

    ''' <summary>
    ''' 从关卡脚本文件和wrapped模板生成Skirmish初始单位
    ''' </summary>
    ''' <param name="xml">关卡脚本文件xml</param>
    Public Sub LoadUnitsFromXMLAndTemplates(xml As String)
        '初始化
        With Me
            .Sides.Clear()
            .NowTurn = 0
            .UnitList.Clear()
            .FlagList.Clear()
            .BuildingList.Clear()
        End With

        Dim xmlDoc As New XmlDocument()
        xmlDoc.LoadXml(xml)
        Dim root As XmlNode = xmlDoc.SelectSingleNode("content")
        Dim xnl As XmlNodeList = root.ChildNodes
        For Each item As XmlNode In xnl
            Dim element As XmlElement = CType(item, XmlElement)
            If element.Name = "side" Then
                Dim sideIndex As Short = CShort(element.GetAttribute("value"))
                Dim inputSideName As String = element.GetAttribute("name")
                Dim inputPlayerType As PlayerType = [Enum].Parse(inputPlayerType.GetType, element.GetAttribute("type"))
                Dim inputTeamIndex As Short = CShort(element.GetAttribute("team"))
                Me.Sides.Add(New GameSideInfo(inputSideName, inputPlayerType, inputTeamIndex))

                Dim children2 As XmlNodeList = element.ChildNodes
                For Each item2 As XmlNode In children2
                    Dim element2 As XmlElement = CType(item2, XmlElement)
                    If element2.Name = "character" Then
                        Dim children3 As XmlNodeList = element2.ChildNodes
                        For Each item3 As XmlNode In children3
                            Dim element3 As XmlElement = CType(item3, XmlElement)
                            If element3.Name = "hero" Then
                                Dim tmpHero As GameHero
                                Dim wrappedTemplateIndex As Short = CShort(element3.GetAttribute("w_index"))
                                '经过wrap的对象直接用，不再需要copy，因为在游戏里都是单例
                                tmpHero = UnitTemplates.GetWrappedHeroTemplate(wrappedTemplateIndex)

                                '第四层
                                'TODO

                                tmpHero.InitializeUnitId(Me.UnitList.Count)
                                Me.UnitList.Add(tmpHero)
                            End If
                        Next
                    Else

                    End If
                Next
            Else

            End If
        Next

    End Sub

    Public Sub Dispose()

    End Sub


End Class

''' <summary>
''' 回合阶段枚举，参考“游戏王大师规则”
''' </summary>
Public Enum SingleGameLoopStage As Byte
    ''' <summary>
    ''' 回合开始阶段
    ''' </summary>
    MyTurnStart = 0
    ''' <summary>
    ''' 准备阶段
    ''' </summary>
    Prepare = 1
    ''' <summary>
    ''' 主要阶段1
    ''' </summary>
    MainA = 2
    ''' <summary>
    ''' 战斗阶段
    ''' </summary>
    Battle = 3
    ''' <summary>
    ''' 主要阶段2
    ''' </summary>
    MainB = 4
    ''' <summary>
    ''' 回合结束阶段
    ''' </summary>
    MyTurnEnd = 5
End Enum

Public Enum MapLoadStatus As Byte
    NotLoaded = 0
    Loading = 1
    Loaded = 2
End Enum
