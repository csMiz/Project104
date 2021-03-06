﻿Imports System.IO
Imports System.Xml
Imports p104
Imports SharpDX.Direct2D1
Imports SharpDX.Mathematics.Interop

''' <summary>
''' 遭遇战GameLoop
''' </summary>
Public Class SkirmishGameLoop
    Implements IStateMachineRecognizable

    Private GameLoopStatus As SingleGameLoopStage = SingleGameLoopStage.MyTurnStart
    Private MapLoaded As MapLoadStatus = MapLoadStatus.NotLoaded
    Private LoadingProcess As Task
    Private GameEnded As GameEndInfo

    Private CampaignIndex As Short = -1

    <Obsolete("use SkirmishMap2", True)>
    Public SkirmishGameMap As SkirmishMap = Nothing
    ''' <summary>
    ''' 遭遇战地图类
    ''' </summary>
    Public SkirmishGameMap2 As SkirmishMap2 = Nothing
    ''' <summary>
    ''' 绑定的观察者类
    ''' </summary>
    Public BindingCamera As SpectatorCamera

    ''' <summary>
    ''' 处理控件面板等鼠标点击事件
    ''' </summary>
    Public SkirmishPage As GamePageProperty = Nothing
    ''' <summary>
    ''' 地图鼠标交互层
    ''' </summary>
    Public UI_SkirmishBoard As GameChessboard = Nothing

    Public UI_BottomLeftUnitAvatar As GameSkirmishAvatarBox = Nothing
    ''' <summary>
    ''' 顶部资源区
    ''' </summary>
    Public UI_TopLeftResourceBar As GameSkirmishResourceBar = Nothing

    Public UI_BottomActionBar As GameContentFrame = Nothing

    Public UI_BottomRightMiniMap As GamePictureBox = Nothing
    ''' <summary>
    ''' MouseDown对应的Block坐标，用于检查与MouseUp时是否一致
    ''' </summary>
    Private SkirmishBoardMouseDownPosition As PointI = New PointI(-1, -1)


    ''' <summary>
    ''' 势力
    ''' </summary>
    Public Sides As New List(Of GameSideInfo)
    ''' <summary>
    ''' 当前回合数
    ''' </summary>
    Private NowTurn As IntegerProperty
    ''' <summary>
    ''' 阶段状态机
    ''' </summary>
    Private SkirmishGamePhases As New StateMachine
    ''' <summary>
    ''' 当前全局阶段
    ''' </summary>
    Public SkirmishGlobalStateIndex As SingleGameLoopStage = SingleGameLoopStage.OutOfTurn

    Public SkirmishGlobalStateStatus As StateMachineSingleProcessStatus = StateMachineSingleProcessStatus.NA

    Public UnitStateCompleteCounter As Integer = 0

    Public UnitStateCompleteCounterExpectValue As Integer = 0
    ''' <summary>
    ''' 当前玩家，对应Sides
    ''' </summary>
    Private NowPlayer As Short = 0
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

    Public SkirmishUnitDetailDialog As UnitDetailDialog = UnitDetailDialog.Instance
    ''' <summary>
    ''' 每个玩家当前选中的单位在列表里对应的索引
    ''' </summary>
    Public SelectedUnitIndex() As Integer




    Public Sub StartLoadSkirmishMapResources(missionIndex As Short)
        If missionIndex < 0 Then Throw New Exception("invalid mission")
        CampaignIndex = missionIndex
        '建立独立线程
        LoadingProcess = New Task(AddressOf LoadResources)
        LoadingProcess.Start()

    End Sub

    Private Sub LoadResources()
        MapLoaded = MapLoadStatus.Loading

        If CampaignIndex = 0 Then
            Dim stream As FileStream = New FileStream(Application.StartupPath & "\newmap.txt", FileMode.Open)
            'If SkirmishGameMap Is Nothing Then SkirmishGameMap = New SkirmishMap
            'SkirmishGameMap.LoadFromFile(stream)
            If SkirmishGameMap2 Is Nothing Then SkirmishGameMap2 = New SkirmishMap2
            SkirmishGameMap2.LoadFromFile(stream, BindingCamera.GetDeviceContext)
            SkirmishGameMap2.Register3DObjects(BindingCamera.Camera3D)
        End If

        'test object remi
        Dim remi As Game3DObject2 = Object3DLoaderInstance.ObjectRepository2(1)
        remi.ShaderIndex = 1
        For i = 0 To remi.SourceFaces.Count - 1
            Dim tmpFace As Game3dFace2_1 = remi.SourceFaces(i)
            For j = 0 To tmpFace.Vertices.Count - 1
                remi.SourceFaces(i).Vertices(j).X *= 5
                remi.SourceFaces(i).Vertices(j).Y *= 5
                remi.SourceFaces(i).Vertices(j).Z *= 5

                remi.SourceFaces(i).Vertices(j).X += 50
                remi.SourceFaces(i).Vertices(j).Y += 50
                remi.SourceFaces(i).Vertices(j).Z += 80
            Next
        Next
        remi.RegionCheckSign = {New PointF3(0, 0, 80)}
        BindingCamera.Camera3D.WorldContainer.Add(remi)

        With BindingCamera.Camera3D
            .Position = New RawVector3(50, -850, 1000)
            .Rotation = New PointF3(-0.7236, 0, 0)
            .CalculateViewP()
            .CalculateViewRX()
            .CalculateViewRY()
            .CalculateViewRZ()
            .RefreshProjection()
            .CalculateWVP()
            .SetCursorRayStatus(0.0F)
        End With
        BindingCamera.RefreshCamera3D()

        ''预先绘制地图装饰物
        'SkirmishGameMap.LoadAccessories(BindingCamera.GetDevceContext, BindingCamera.Zoom)
        '载入预设单位
        Me.LoadUnitsFromXMLAndTemplates(GetCampaignScript(0))
        '初始化选择单位字段为空(-1)
        ReDim Me.SelectedUnitIndex(Me.Sides.Count - 1)
        For i = 0 To Me.Sides.Count - 1
            Me.SelectedUnitIndex(i) = -1
        Next
        Me.InitializeSkirmishPage()
        '初始化流程状态机
        Me.InitializeSkirmishGamePhases()

        '初始化单位详情面板
        Dim dialog As UnitDetailDialog = UnitDetailDialog.Instance
        With dialog
            .InitializeDialog(BindingCamera.Resolve.X, BindingCamera.Resolve.Y, BindingCamera)
            .InitializeConetentBox()
            .InitializeEffects()
            .Visible = False
        End With

        'SkirmishGameMap.ResourcesLoaded = True
        SkirmishGameMap2.ResourcesLoaded = True
        MapLoaded = MapLoadStatus.Loaded

    End Sub

    Public Async Function WaitForLoad() As Task(Of Integer)
        While (Me.MapLoaded <> MapLoadStatus.Loaded)
            Await Task.Delay(100)
        End While
        LoadingProcess.Dispose()
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
            .NowTurn = New IntegerProperty(0)
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
                Dim tmpSide As New GameSideInfo(inputSideName, inputPlayerType, inputTeamIndex)
                Me.Sides.Add(tmpSide)

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

                                Dim children4 As XmlNodeList = element3.ChildNodes
                                For Each item4 As XmlNode In children4
                                    Dim element4 As XmlElement = CType(item4, XmlElement)
                                    If element4.Name = "startpos" Then
                                        Dim unitPosition As PointI3 = MathHelper.ParsePointI3(element4.GetAttribute("value"))
                                        tmpHero.Position = unitPosition
                                    End If

                                Next

                                tmpHero.InitializeUnitId(Me.UnitList.Count)
                                Me.UnitList.Add(tmpHero)
                            End If
                        Next
                    ElseIf element2.Name = "resources" Then
                        tmpSide.Power = New IntegerProperty(CInt(element2.GetAttribute("power")))
                        tmpSide.Point = New IntegerProperty(CInt(element2.GetAttribute("point")))
                        tmpSide.Burden = New IntegerProperty(0)
                        tmpSide.CurrentTurn = New IntegerProperty(0)

                    Else

                    End If
                Next
            Else

            End If
        Next

    End Sub

    <Obsolete("render 3d-skirmish map and 2d-ui separately", False)>
    Public Sub DrawSkirmishMapLayer(ByRef context As SharpDX.Direct2D1.DeviceContext, ByRef spectator As SpectatorCamera, canvasBitmap As Bitmap1)
        'Me.SkirmishGameMap.DrawHexMap(context, spectator, canvasBitmap)    'this is ver1.0
        spectator.Camera3D.DrawContainer(context, spectator, canvasBitmap)    'this is ver1.1

        'Me.DrawUnitLayer(context, spectator, canvasBitmap)

        Me.SkirmishPage.PaintElements(context, spectator, canvasBitmap)
    End Sub

    Public Sub Register2DUILayer()

        Me.BindingCamera.PaintingLayers.Push(AddressOf Me.SkirmishPage.PaintElements)
        Me.BindingCamera.PaintingLayersDescription.Push(GameImageLayer.SkirmishMap2DUI_Top_Resources)

        'TODO: other controls

    End Sub

    <Obsolete("render 3d-skirmish map and 2d-ui separately", False)>
    Private Sub DrawUnitLayer(ByRef context As SharpDX.Direct2D1.DeviceContext, ByRef spectator As SpectatorCamera, canvasBitmap As Bitmap1)

        'TODO: 完善单位棋子显示
        Dim tmpUnit As GameUnit = Me.UnitList(0)
        Dim tmpImage As Bitmap1 = tmpUnit.GetSkirmishChessImage().GetImage
        Dim imageHalfSize As Single = 50
        'FIXME:
        'Dim tmpCentre As RawVector2 = SkirmishGameMap.Blocks(tmpUnit.Position.X, tmpUnit.Position.Y).V_O
        'Dim drawRect As New RawRectangleF(tmpCentre.X - imageHalfSize, tmpCentre.Y - imageHalfSize, tmpCentre.X + imageHalfSize, tmpCentre.Y + imageHalfSize)
        'context.DrawBitmap(tmpImage, drawRect, NOT_TRANSPARENT, BitmapInterpolationMode.Linear)


    End Sub

    ''' <summary>
    ''' 初始化GameLoop状态机
    ''' </summary>
    Public Sub InitializeSkirmishGamePhases()
        Dim phases As New List(Of StateMachineSingleState)
        Dim transition(,) As Short

        ReDim transition(5, 2)
        transition(0, SkirmishStateMachineInputAlphabet.NormalGo) = 1
        transition(1, SkirmishStateMachineInputAlphabet.NormalGo) = 2
        transition(2, SkirmishStateMachineInputAlphabet.NormalGo) = 3    '主1到主2
        transition(2, SkirmishStateMachineInputAlphabet.Stay) = 2        '主1循环
        transition(3, SkirmishStateMachineInputAlphabet.NormalGo) = 4    '主2到结束
        transition(3, SkirmishStateMachineInputAlphabet.GoBackOne) = 2   '主2回到主1
        transition(4, SkirmishStateMachineInputAlphabet.NormalGo) = 5
        transition(5, SkirmishStateMachineInputAlphabet.NormalGo) = 0

        '回合开始阶段
        Dim turn_start_phase As New StateMachineSingleState
        turn_start_phase.InitializeStateIndex(0)
        Dim turn_start_phase_s = Sub(sender As IStateMachineRecognizable, senderType As StateInputType)
                                     Debug.WriteLine("回合开始阶段")
                                     turn_start_phase.ToProcess(sender, senderType)
                                 End Sub
        Dim turn_start_phase_p = Sub(sender As IStateMachineRecognizable, senderType As StateInputType)
                                     CType(sender, SkirmishGameLoop).ChangeGlobalTurn()
                                     Me.Sides(NowPlayer).CurrentTurn.SetValue(Me.Sides(NowPlayer).CurrentTurn.GetValue + 1, LogSenderType.Change_Program)
                                     Me.UI_TopLeftResourceBar.NeedRepaint = True
                                     turn_start_phase.ToEnd(sender, senderType)
                                 End Sub
        Dim turn_start_phase_e = Sub(sender As IStateMachineRecognizable, senderType As StateInputType)
                                     ResetUnitStateCounter()    '重置计数器
                                     '进入准备阶段
                                     Dim tmpGlobalPhase As StateMachineSingleState = Me.SkirmishGamePhases.NextState(sender.GetState, SkirmishStateMachineInputAlphabet.NormalGo)
                                     tmpGlobalPhase.Trigger(sender, senderType)
                                     '从准备阶段开始启动各单位状态机
                                     For Each tmpUnit As GameUnit In UnitList
                                         If tmpUnit.Player = 0 Then
                                             tmpGlobalPhase.Trigger(tmpUnit, StateInputType.SkirmishGameUnit)
                                         End If
                                     Next

                                 End Sub
        AddHandler turn_start_phase.StateStart, turn_start_phase_s
        AddHandler turn_start_phase.StateProcess, turn_start_phase_p
        AddHandler turn_start_phase.StateEnd, turn_start_phase_e

        '准备阶段
        Dim prepare_phase As New StateMachineSingleState
        prepare_phase.InitializeStateIndex(1)
        Dim prepare_phase_s = Sub(sender As IStateMachineRecognizable, senderType As StateInputType)
                                  If senderType = StateInputType.SkirmishGameLoop Then
                                      Debug.WriteLine("准备阶段")
                                  End If
                                  prepare_phase.ToProcess(sender, senderType)
                              End Sub
        Dim prepare_phase_p = Sub(sender As IStateMachineRecognizable, senderType As StateInputType)
                                  If senderType Then    '全局
                                      'TODO: 资源变动

                                      '等待各单位执行完
                                      Dim tmpGameLoop As SkirmishGameLoop = CType(sender, SkirmishGameLoop)
                                      If tmpGameLoop.AllUnitStateComplete Then
                                          prepare_phase.ToEnd(sender, senderType)
                                      Else
                                          sender.SetProcessTag(StateMachineSingleProcessStatus.Suspended)
                                      End If
                                  Else    '单位
                                      'TODO: 检查效果队列

                                      'TODO: 检查成就等

                                      '重置单位行动状态
                                      CType(sender, GameUnit).ResetUnitStatus(UnitStatus.Waiting, 1, 1)
                                      prepare_phase.ToEnd(sender, senderType)
                                  End If
                              End Sub
        Dim prepare_phase_e = Sub(sender As IStateMachineRecognizable, senderType As StateInputType)
                                  If senderType Then    '全局
                                      ResetUnitStateCounter()    '重置计数器
                                      '进入主要阶段1
                                      Dim tmpGlobalPhase As StateMachineSingleState = Me.SkirmishGamePhases.NextState(sender.GetState, SkirmishStateMachineInputAlphabet.NormalGo)
                                      tmpGlobalPhase.Trigger(sender, senderType)
                                      For Each tmpUnit As GameUnit In UnitList
                                          If tmpUnit.Player = 0 Then
                                              tmpGlobalPhase.Trigger(tmpUnit, StateInputType.SkirmishGameUnit)
                                          End If
                                      Next
                                  Else    '单位
                                      MarkUnitStateCompleteCounter()
                                      prepare_phase.TryResume(Me, StateInputType.SkirmishGameLoop)
                                  End If
                              End Sub
        Dim prepare_phase_r = Sub(sender As IStateMachineRecognizable, senderType As StateInputType)
                                  prepare_phase.ToProcess(sender, senderType)
                              End Sub
        AddHandler prepare_phase.StateStart, prepare_phase_s
        AddHandler prepare_phase.StateProcess, prepare_phase_p
        AddHandler prepare_phase.StateEnd, prepare_phase_e
        AddHandler prepare_phase.StateResume, prepare_phase_r

        '主要阶段1
        Dim main_a_phase As New StateMachineSingleState
        main_a_phase.InitializeStateIndex(2)
        Dim main_a_phase_s = Sub(sender As IStateMachineRecognizable, senderType As StateInputType)
                                 If senderType = StateInputType.SkirmishGameLoop Then
                                     Debug.WriteLine("主要阶段1")
                                 End If
                                 main_a_phase.ToProcess(sender, senderType)
                             End Sub
        Dim main_a_phase_p = Sub(sender As IStateMachineRecognizable, senderType As StateInputType)
                                 If senderType Then    '全局
                                     '等待玩家操作
                                     Dim tmpGameLoop As SkirmishGameLoop = CType(sender, SkirmishGameLoop)
                                     If tmpGameLoop.AllUnitStateComplete Then
                                         main_a_phase.ToEnd(sender, senderType)
                                     Else
                                         sender.SetProcessTag(StateMachineSingleProcessStatus.Suspended)
                                     End If
                                 Else    '单位
                                     '等待玩家操作
                                     Dim tmpUnit As GameUnit = CType(sender, GameUnit)
                                     If tmpUnit.AllActionDone Then
                                         main_a_phase.ToEnd(sender, senderType)
                                     Else
                                         sender.SetProcessTag(StateMachineSingleProcessStatus.Suspended)
                                     End If
                                 End If
                             End Sub
        Dim main_a_phase_e = Sub(sender As IStateMachineRecognizable, senderType As StateInputType)
                                 If senderType Then    '全局
                                     '进入主要阶段2
                                     Dim tmpGlobalPhase As StateMachineSingleState = Me.SkirmishGamePhases.NextState(sender.GetState, SkirmishStateMachineInputAlphabet.NormalGo)
                                     tmpGlobalPhase.Trigger(sender, senderType)
                                 Else    '单位
                                     Dim tmpNextPhase As StateMachineSingleState = Me.SkirmishGamePhases.NextState(sender.GetState, SkirmishStateMachineInputAlphabet.NormalGo)
                                     tmpNextPhase.Trigger(sender, senderType)
                                 End If
                             End Sub
        Dim main_a_phase_r = Sub(sender As IStateMachineRecognizable, senderType As StateInputType)
                                 main_a_phase.ToProcess(sender, senderType)
                             End Sub
        AddHandler main_a_phase.StateStart, main_a_phase_s
        AddHandler main_a_phase.StateProcess, main_a_phase_p
        AddHandler main_a_phase.StateEnd, main_a_phase_e
        AddHandler main_a_phase.StateResume, main_a_phase_r

        '主要阶段2
        Dim main_b_phase As New StateMachineSingleState
        main_b_phase.InitializeStateIndex(3)
        Dim main_b_phase_s = Sub(sender As IStateMachineRecognizable, senderType As StateInputType)
                                 If senderType = StateInputType.SkirmishGameLoop Then
                                     Debug.WriteLine("主要阶段2")
                                 End If
                                 main_b_phase.ToProcess(sender, senderType)
                             End Sub
        Dim main_b_phase_p = Sub(sender As IStateMachineRecognizable, senderType As StateInputType)
                                 If senderType Then    '全局
                                     ResetUnitStateCounter()

                                 Else    '单位
                                     MarkUnitStateCompleteCounter()
                                 End If

                                 main_b_phase.ToEnd(sender, senderType)
                             End Sub
        Dim main_b_phase_e = Sub(sender As IStateMachineRecognizable, senderType As StateInputType)
                                 If senderType Then    '全局
                                     '进入回合结束阶段
                                     Dim tmpGlobalPhase As StateMachineSingleState = Me.SkirmishGamePhases.NextState(sender.GetState, SkirmishStateMachineInputAlphabet.NormalGo)
                                     tmpGlobalPhase.Trigger(sender, senderType)

                                 Else    '单位
                                     sender.SetState(SingleGameLoopStage.OutOfTurn)

                                 End If
                             End Sub
        AddHandler main_b_phase.StateStart, main_b_phase_s
        AddHandler main_b_phase.StateProcess, main_b_phase_p
        AddHandler main_b_phase.StateEnd, main_b_phase_e


        '回合结束阶段
        Dim turn_end_phase As New StateMachineSingleState
        With turn_end_phase
            .InitializeStateIndex(4)
        End With

        '回合外阶段
        Dim out_of_turn_phase As New StateMachineSingleState
        With out_of_turn_phase
            .InitializeStateIndex(5)
        End With
        'AddHandler out_of_turn_phase.StateProcessComplete, AddressOf StartGoNextState

        With phases
            .Add(turn_start_phase)
            .Add(prepare_phase)
            .Add(main_a_phase)
            .Add(main_b_phase)
            .Add(turn_end_phase)
            .Add(out_of_turn_phase)
        End With

        Me.SkirmishGamePhases.InitializeStateMachine(phases, transition)

    End Sub

    ''' <summary>
    ''' 开始遭遇战游戏流程
    ''' </summary>
    Public Sub StartSkirmishGameStateMachine()

        Me.SkirmishGlobalStateIndex = SingleGameLoopStage.OutOfTurn
        Me.ResetUnitStateCounter()

        '全局状态进入回合开始阶段
        Dim tmpGlobalPhase As StateMachineSingleState = Me.SkirmishGamePhases.NextState(Me.SkirmishGlobalStateIndex, SkirmishStateMachineInputAlphabet.NormalGo)
        tmpGlobalPhase.Trigger(Me, StateInputType.SkirmishGameLoop)

    End Sub

    ''' <summary>
    ''' 设置当前回合数
    ''' </summary>
    ''' <param name="delta">改变值，默认+1</param>
    Public Sub ChangeGlobalTurn(Optional delta As Integer = 1)
        Me.NowTurn.SetValue(Me.NowTurn.GetValue + delta, LogSenderType.Change_Program)

    End Sub

    ''' <summary>
    ''' 使单位行动计数器计数
    ''' </summary>
    Public Sub MarkUnitStateCompleteCounter(Optional delta As Short = 1)
        Me.UnitStateCompleteCounter += delta
    End Sub
    ''' <summary>
    ''' 重置单位行动计数器
    ''' </summary>
    Public Sub ResetUnitStateCounter()
        Me.UnitStateCompleteCounter = 0
        '更新计数器预期值
        Me.UnitStateCompleteCounterExpectValue = 0
        For Each tmpUnit As GameUnit In Me.UnitList
            If tmpUnit.Player = 0 Then  'TODO
                Me.UnitStateCompleteCounterExpectValue += 1
            End If
        Next
    End Sub
    ''' <summary>
    ''' 检查所有单位状态已更改
    ''' </summary>
    ''' <returns></returns>
    Public Function AllUnitStateComplete() As Boolean
        Dim a As Integer = Me.UnitStateCompleteCounter
        Dim b As Integer = Me.UnitStateCompleteCounterExpectValue
        Debug.WriteLine("单位状态计数器： " & a & " / " & b)
        Return Not CBool(a - b)
    End Function

    Public Function CheckTurnCountWin() As Boolean
        'TODO
        Return False

    End Function

    Public Sub InitializeSkirmishPage()
        Me.SkirmishPage = New GamePageProperty
        Dim myContext As DeviceContext = Me.BindingCamera.GetDeviceContext

        Me.UI_SkirmishBoard = New GameChessboard
        With Me.UI_SkirmishBoard
            .BasicRect = Me.BindingCamera.ResolveRectangle
            .AbsoluteRect = .BasicRect
            .BindingContext = myContext
            .Z_Index = 0
        End With
        AddHandler Me.UI_SkirmishBoard.MouseDown, AddressOf Me.GameBoardMouseDown
        AddHandler Me.UI_SkirmishBoard.MouseMove, AddressOf Me.GameBoardMouseMove
        Me.SkirmishPage.UIElements.Add(Me.UI_SkirmishBoard)
        'TODO
        Me.UI_BottomLeftUnitAvatar = New GameSkirmishAvatarBox
        With Me.UI_BottomLeftUnitAvatar
            Dim avatarHeight As Single = BindingCamera.Resolve.Y * 0.3
            .BasicRect = New RawRectangleF(0, BindingCamera.Resolve.Y - avatarHeight, avatarHeight, BindingCamera.Resolve.Y)
            .AbsoluteRect = .BasicRect
            .BindingContext = myContext
            .InitializeControlCanvas()
            .Z_Index = 1
        End With
        Me.SkirmishPage.UIElements.Add(Me.UI_BottomLeftUnitAvatar)

        UI_TopLeftResourceBar = New GameSkirmishResourceBar
        With Me.UI_TopLeftResourceBar
            .BasicRect = New RawRectangleF(0, 0, Me.BindingCamera.Resolve.X, Me.BindingCamera.Resolve.Y / 20.0F)
            .AbsoluteRect = .BasicRect
            .BindingContext = myContext
            .Z_Index = 1
            .InitializeControlCanvas()
            .BindingSide = Sides(0)
            .InitializeBar()
        End With
        Me.SkirmishPage.UIElements.Add(Me.UI_TopLeftResourceBar)


        Me.SkirmishPage.UIElements.Add(UnitDetailDialog.Instance)

        Me.SkirmishPage.GenerateElementsQuadtree(Me.BindingCamera.Resolve)
        Me.SkirmishPage.InitializeCursor(Me.BindingCamera.CurrentCursorPosition, Me.BindingCamera.Resolve)

    End Sub

    ''' <summary>
    ''' 将鼠标点击位置转换为实际地图位置
    ''' </summary>
    ''' <param name="input">鼠标位置</param>
    Public Function ConvertToWorldCursor(input As PointI) As PointF2
        Dim focus As PointF2 = Me.BindingCamera.CameraTopLeft
        Dim zoom As Single = Me.BindingCamera.Zoom
        Return New PointF2(focus.X + input.X / zoom, focus.Y + input.Y / zoom)
    End Function

    ''' <summary>
    ''' 将实际地图位置转换为坐标
    ''' </summary>
    <Obsolete("使用DrawPositionToChessboard"， False)>
    Public Function MousePositionToChessboard(mouse As PointF2) As PointI
        Dim estimateX As Integer = Math.Floor((mouse.X - SIX_TWO_FIVE) / THREE_SEVEN_FIVE)
        Dim estimateY As Integer = Math.Floor((mouse.Y - SIX_TWO_FIVE_ROOT3) / TWO_FIFTY_ROOT3)
        If estimateX < -1 OrElse estimateX > SkirmishGameMap.MapSizeXMax + 1 OrElse estimateY < -1 OrElse estimateY > SkirmishGameMap.MapSizeYMax + 1 Then
            Return New PointI(-1, -1)
        End If
        Dim pX, pY As Integer
        Dim minDistance As Single = 9999.9
        Dim candidate As New PointI
        For i = -1 To 1
            pX = estimateX + i
            If pX < 0 OrElse pX > SkirmishGameMap.MapSizeXMax Then Continue For
            For j = -1 To 1
                pY = estimateY + j
                If pY < 0 OrElse pY > SkirmishGameMap.MapSizeYMax Then Continue For
                Dim target As PointF2 = Me.SkirmishGameMap.Blocks(pX, pY).World_V_O
                Dim tmpDistance As Single = Math.Sqrt((target.X - mouse.X) ^ 2 + (target.Y - mouse.Y) ^ 2)
                If tmpDistance < minDistance Then
                    minDistance = tmpDistance
                    candidate.X = pX
                    candidate.Y = pY
                End If
            Next
        Next
        If minDistance < TWO_FIFTY Then
            Return candidate
        Else
            Return New PointI(-1, -1)
        End If

    End Function

    ''' <summary>
    ''' 将鼠标点击位置（绘图位置）直接转换为坐标
    ''' </summary>
    ''' <param name="mouse"></param>
    <Obsolete>
    Public Function DrawPositionToChessboard(mouse As PointI) As PointI
        'Dim worldPos As PointF2 = ConvertToWorldCursor(mouse)
        'Dim estimateX As Integer = Math.Floor((worldPos.X - SIX_TWO_FIVE) / THREE_SEVEN_FIVE)
        'Dim estimateY As Integer = Math.Floor((worldPos.Y - SIX_TWO_FIVE_ROOT3) / TWO_FIFTY_ROOT3)
        'If estimateX < -1 OrElse estimateX > SkirmishGameMap.MapSizeXMax + 1 OrElse estimateY < -1 OrElse estimateY > SkirmishGameMap.MapSizeYMax + 1 Then
        '    Return New PointI(-1, -1)
        'End If
        'Dim pX, pY As Integer
        'For i = 1 To -1 Step -1
        '    pX = estimateX + i
        '    If pX < 0 OrElse pX > SkirmishGameMap.MapSizeXMax Then Continue For
        '    For j = 1 To -1 Step -1
        '        pY = estimateY + j
        '        If pY < 0 OrElse pY > SkirmishGameMap.MapSizeYMax Then Continue For
        '        If Me.SkirmishGameMap.Blocks(pX, pY).Outline.IsInsideRaw(New PointF2(mouse.X, mouse.Y)) Then
        '            Return New PointI(pX, pY)
        '        End If
        '    Next
        'Next
        Return New PointI(-1, -1)
    End Function

    Public Sub GameBoardMouseDown(e As GameMouseEventArgs)
        ''Me.SkirmishBoardMouseDownPosition = Me.MousePositionToChessboard(Me.ConvertToWorldCursor(e.Position))
        'Me.SkirmishBoardMouseDownPosition = Me.DrawPositionToChessboard(e.Position)        '改用绘图坐标直接进行定位
        'Debug.WriteLine("Pos:" & SkirmishBoardMouseDownPosition.X & ", " & SkirmishBoardMouseDownPosition.Y)

        ''test
        ''open unit detail dialog
        'Dim dialog As UnitDetailDialog = UnitDetailDialog.Instance
        'dialog.BindUnit(Me.UnitList(0))
        'dialog.Visible = True
        ''BindingCamera.PaintingLayers.Push(AddressOf dialog.DrawControl)
        ''BindingCamera.PaintingLayersDescription.Push(GameImageLayer.Skirmish_UnitDetail)

        Dim tmpObj As Game3DObject2 = Object3DLoaderInstance.ObjectRepository2(1)
        For i = 0 To tmpObj.SourceFaces.Count - 1
            For j = 0 To tmpObj.SourceFaces(i).Vertices.Count - 1
                tmpObj.SourceFaces(i).Vertices(j).Y = 50 - tmpObj.SourceFaces(i).Vertices(j).Y
                tmpObj.SourceFaces(i).Normal(j).Y = -tmpObj.SourceFaces(i).Normal(j).Y
            Next
        Next
        BindingCamera.Camera3D.UpdateOneBundle(1)

    End Sub

    Public Sub GameBoardMouseUp(e As GameMouseEventArgs)


    End Sub

    Public Sub GameBoardMouseMove(e As GameMouseEventArgs)

        Dim hr As PointF2 = BindingCamera.Camera3D.BindingHalfResolve
        BindingCamera.Camera3D.ScreenCursorOffset.X = (e.X - hr.X) / hr.X
        BindingCamera.Camera3D.ScreenCursorOffset.Y = -(e.Y - hr.Y) / hr.Y

        Dim hoverIndex As Integer = BindingCamera.Camera3DPixelInfo(0)
        Me.UI_BottomLeftUnitAvatar.DisplayMapBlock = SkirmishGameMap2.GetBlock(hoverIndex)
        Me.UI_BottomLeftUnitAvatar .EmptyMapBlock = false
        Me.UI_BottomLeftUnitAvatar.NeedRepaint = True

    End Sub

    ''' <summary>
    ''' 尝试用鼠标选中一个单位
    ''' </summary>
    ''' <param name="input">点击的格子索引</param>
    Public Sub MouseTryClickUnit(input As PointI)
        Dim tmpSelectedUnit As Integer = -1
        For i = 0 To Me.UnitList.Count - 1
            Dim tmpUnit As GameUnit = Me.UnitList(i)
            If tmpUnit.Position.X = input.X AndAlso tmpUnit.Position.Y = input.Y Then
                tmpSelectedUnit = i
                Exit For
            End If
        Next
        Me.SelectedUnitIndex(NowPlayer) = tmpSelectedUnit
        If tmpSelectedUnit = -1 Then Return

        If Me.UnitList(tmpSelectedUnit).Player = NowPlayer Then
            'ShowUnitDetailUI()
        Else
            'ShowRestrictedDetailUI()
        End If


    End Sub


    Public Sub Dispose()
        'TODO

    End Sub

    Public Sub SetState(value As Short) Implements IStateMachineRecognizable.SetState
        Me.SkirmishGlobalStateIndex = value
    End Sub

    Public Function GetState() As Short Implements IStateMachineRecognizable.GetState
        Return Me.SkirmishGlobalStateIndex
    End Function

    Public Sub SetProcessTag(value As StateMachineSingleProcessStatus) Implements IStateMachineRecognizable.SetProcessTag
        Me.SkirmishGlobalStateStatus = value
    End Sub

    Public Function GetProcessTag() As StateMachineSingleProcessStatus Implements IStateMachineRecognizable.GetProcessTag
        Return Me.SkirmishGlobalStateStatus
    End Function
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
    '''' <summary>
    '''' 战斗阶段，已整合到MainA中
    '''' </summary>
    'Battle = 3
    ''' <summary>
    ''' 主要阶段2
    ''' </summary>
    MainB = 3
    ''' <summary>
    ''' 回合结束阶段
    ''' </summary>
    MyTurnEnd = 4
    ''' <summary>
    ''' 非己方回合
    ''' </summary>
    OutOfTurn = 5
End Enum

Public Enum MapLoadStatus As Byte
    NotLoaded = 0
    Loading = 1
    Loaded = 2
End Enum

Public Enum SkirmishStateMachineInputAlphabet As Byte
    NormalGo = 0
    GoBackOne = 1
    Stay = 2
    SkipOne = 3

End Enum

Public Enum StateInputType As Byte
    SkirmishGameUnit = 0
    SkirmishGameLoop = 1
End Enum
