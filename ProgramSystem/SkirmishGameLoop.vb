Imports System.IO
Imports System.Xml
Imports p104
Imports SharpDX.Direct2D1
Imports SharpDX.Mathematics.Interop
''' <summary>
''' 遭遇战GameLoop
''' </summary>
Public Class SkirmishGameLoop

    Private GameLoopStatus As SingleGameLoopStage = SingleGameLoopStage.MyTurnStart
    Private MapLoaded As MapLoadStatus = MapLoadStatus.NotLoaded
    Private LoadingProcess As Task
    Private GameEnded As GameEndInfo

    Private CampaignIndex As Short = -1
    Public SkirmishGameMap As New SkirmishMap
    Public BindingCamera As SpectatorCamera

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
    Private SkirmishGamePhases As New SkirmishStateMachine
    ''' <summary>
    ''' 当前阶段
    ''' </summary>
    Public SkirmishGlobalStateIndex As SingleGameLoopStage = SingleGameLoopStage.OutOfTurn

    Public SkirmishGlobalStateStatus As StateMachineSingleProcessStatus = StateMachineSingleProcessStatus.NA

    Public UnitStateCompleteCounter As Integer = 0

    Public UnitStateCompleteCounterExpectValue As Integer = 0
    ''' <summary>
    ''' 当前玩家
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
            SkirmishGameMap.LoadFromFile(stream)
        End If

        SkirmishGameMap.LoadAccessories(BindingCamera.GetDevceContext, BindingCamera.Zoom)

        Me.LoadUnitsFromXMLAndTemplates(GetCampaignScript(0))
        Me.InitializeSkirmishGamePhases()

        SkirmishGameMap.ResourcesLoaded = True
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
                    Else

                    End If
                Next
            Else

            End If
        Next

    End Sub

    Public Sub DrawSkirmishMapLayer(ByRef context As SharpDX.Direct2D1.DeviceContext, ByRef spectator As SpectatorCamera, canvasBitmap As Bitmap1)
        Me.SkirmishGameMap.DrawHexMap(context, spectator, canvasBitmap)
        Me.DrawUnitLayer(context, spectator, canvasBitmap)
    End Sub


    Private Sub DrawUnitLayer(ByRef context As SharpDX.Direct2D1.DeviceContext, ByRef spectator As SpectatorCamera, canvasBitmap As Bitmap1)
        Dim tmpUnit As GameUnit = Me.UnitList(0)
        Dim tmpImage As Bitmap1 = tmpUnit.GetSkirmishChessImage().GetImage

        context.DrawBitmap(tmpImage, New RawRectangleF(220, 80, 320, 180), NOT_TRANSPARENT, BitmapInterpolationMode.Linear)


    End Sub

    ''' <summary>
    ''' 初始化GameLoop状态机并启动
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
        Dim turn_start_phase As New SkirmishTurnStartPhase
        With turn_start_phase
            .InitializeStateIndex(0)
        End With
        AddHandler turn_start_phase.UnitStateComplete, AddressOf UnitGoNextState1
        AddHandler turn_start_phase.GlobalStateComplete, AddressOf GlobalGoNextState1

        '准备阶段
        Dim prepare_phase As New SkirmishPreparePhase
        With prepare_phase
            .InitializeStateIndex(1)
        End With
        AddHandler prepare_phase.UnitStateComplete, AddressOf UnitGoNextState1
        AddHandler prepare_phase.GlobalStateComplete, AddressOf GlobalGoNextState1

        '主要阶段1
        Dim main_a_phase As New SkirmishMainAPhase
        With main_a_phase
            .InitializeStateIndex(2)
        End With
        AddHandler main_a_phase.UnitStateComplete, AddressOf UnitGoNextState2
        AddHandler main_a_phase.GlobalStateComplete, AddressOf GlobalGoNextState2

        '主要阶段2
        Dim main_b_phase As New SkirmishMainBPhase
        With main_b_phase
            .InitializeStateIndex(3)
        End With
        AddHandler main_b_phase.UnitStateComplete, AddressOf UnitGoNextState2
        AddHandler main_b_phase.GlobalStateComplete, AddressOf GlobalGoNextState2

        '回合结束阶段
        Dim turn_end_phase As New SkirmishTurnEndPhase
        With turn_end_phase
            .InitializeStateIndex(4)
        End With
        AddHandler turn_end_phase.UnitStateComplete, AddressOf UnitGoNextState1
        AddHandler turn_end_phase.GlobalStateComplete, AddressOf GlobalGoNextState1

        '回合外阶段
        Dim out_of_turn_phase As New SkirmishOutOfTurnPhase
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
        Me.SkirmishGlobalStateIndex = SingleGameLoopStage.OutOfTurn
        Me.ResetUnitStateCounter()

        '全局状态进入回合开始阶段
        Dim tmpGlobalPhase As SkirmishPhaseSingleState = Me.SkirmishGamePhases.NextState(Me.SkirmishGlobalStateIndex, StateMachineInputAlphabet.StateGo)
        tmpGlobalPhase.Trigger(Me)
        '每个单位状态进入回合开始阶段
        For Each tmpUnit As GameUnit In UnitList
            If tmpUnit.Player = 0 Then
                Dim tmpPhase As SkirmishPhaseSingleState = Me.SkirmishGamePhases.NextState(tmpUnit.UnitPhase, StateMachineInputAlphabet.StateGo)
                tmpPhase.Trigger(tmpUnit)
            End If
        Next


    End Sub

    ''' <summary>
    ''' 用于回合开始阶段，准备阶段和回合结束阶段。全局状态需要与单位状态同步。
    ''' </summary>
    Public Sub UnitGoNextState1(sender As GameUnit)
        Me.MarkUnitStateCompleteCounter()
    End Sub
    Public Sub GlobalGoNextState1(sender As SkirmishGameLoop)
        Dim processContent = Async Sub()
                                 Do While (Not Me.AllUnitStateComplete())
                                     Await Task.Delay(50)
                                 Loop
                                 Me.ResetUnitStateCounter()

                                 Dim tmpGlobalPhase As SkirmishPhaseSingleState = Me.SkirmishGamePhases.NextState(sender.SkirmishGlobalStateIndex, StateMachineInputAlphabet.StateGo)
                                 tmpGlobalPhase.Trigger(sender)
                                 For Each tmpUnit As GameUnit In sender.UnitList
                                     If tmpUnit.Player = 0 Then
                                         Dim tmpPhase As SkirmishPhaseSingleState = Me.SkirmishGamePhases.NextState(tmpUnit.UnitPhase, StateMachineInputAlphabet.StateGo)
                                         tmpPhase.Trigger(tmpUnit)
                                     End If
                                 Next

                             End Sub
        Dim tmpProcess As New Task(processContent)
        tmpProcess.Start()
    End Sub

    ''' <summary>
    ''' 用于主要阶段。全局状态停留在主要阶段1直到所有单位完成行动，单位可以处于主要阶段，战斗阶段。
    ''' </summary>
    Public Sub UnitGoNextState2(sender As GameUnit)
        Throw New NotImplementedException()
    End Sub
    Public Sub GlobalGoNextState2(sender As SkirmishGameLoop)
        Throw New NotImplementedException()
    End Sub

    Public Sub ChangeTurn(Optional delta As Integer = 1)
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
        Debug.WriteLine("单位状态计数器： " & Me.UnitStateCompleteCounter & " / " & Me.UnitStateCompleteCounterExpectValue)
        Return Not CBool(Me.UnitStateCompleteCounter - Me.UnitStateCompleteCounterExpectValue)
    End Function

    Public Function CheckTurnCountWin() As Boolean
        'TODO
        Return False

    End Function

    Public Sub Dispose()
        'TODO

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
    '''' <summary>
    '''' 战斗阶段
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

Public Enum SkirmishStateMachineRecognizableInfo As Integer
    Player = 0
    SingleUnit = 1

End Enum

Public Class SkirmishStateMachine
    Inherits StateMachine

    Public Overloads Function NextState(nowState As Short, input As SkirmishStateMachineInputAlphabet) As SkirmishPhaseSingleState
        Dim nextStateIndex As Short = Me.TransitionFunction(nowState, input)
        Return Me.States(nextStateIndex)
    End Function

End Class

Public Enum SkirmishStateMachineInputAlphabet As Byte
    NormalGo = 0
    GoBackOne = 1
    Stay = 2
    SkipOne = 3

End Enum

''' <summary>
''' 遭遇战阶段类，继承状态机单个状态类
''' </summary>
Public MustInherit Class SkirmishPhaseSingleState
    Inherits StateMachineSingleState

    Public Event UnitStateComplete(sender As GameUnit)
    Public Event GlobalStateComplete(sender As SkirmishGameLoop)
    Public Event StateProcessAbort()

    ''' <summary>
    ''' 开始执行
    ''' </summary>
    Public Overloads Sub Trigger(sender As GameUnit)
        sender.UnitPhase = Me.StateIndex
        sender.UnitPhaseStatus = StateMachineSingleProcessStatus.Starting
        Me.StateStart(sender, sender.UnitPhaseStatus)
        If sender.UnitPhaseStatus = StateMachineSingleProcessStatus.Normal Then Me.StateProcess(sender, sender.UnitPhaseStatus)
        If sender.UnitPhaseStatus = StateMachineSingleProcessStatus.Suspended Then Return
        Me.StateEnd(sender, sender.UnitPhaseStatus)
        If sender.UnitPhaseStatus = StateMachineSingleProcessStatus.NA Then
            RaiseEvent UnitStateComplete(sender)
        ElseIf sender.UnitPhaseStatus = StateMachineSingleProcessStatus.Abort Then
            RaiseEvent StateProcessAbort()
        End If

    End Sub
    ''' <summary>
    ''' 开始执行
    ''' </summary>
    Public Overloads Sub Trigger(sender As SkirmishGameLoop)
        sender.SkirmishGlobalStateIndex = Me.StateIndex
        sender.SkirmishGlobalStateStatus = StateMachineSingleProcessStatus.Starting
        Me.StateStart(sender, sender.SkirmishGlobalStateStatus)
        If sender.SkirmishGlobalStateStatus = StateMachineSingleProcessStatus.Normal Then Me.StateProcess(sender, sender.SkirmishGlobalStateStatus)
        If sender.SkirmishGlobalStateStatus = StateMachineSingleProcessStatus.Suspended Then Return
        Me.StateEnd(sender, sender.SkirmishGlobalStateStatus)
        If sender.SkirmishGlobalStateStatus = StateMachineSingleProcessStatus.NA Then
            RaiseEvent GlobalStateComplete(sender)
        ElseIf sender.SkirmishGlobalStateStatus = StateMachineSingleProcessStatus.Abort Then
            RaiseEvent StateProcessAbort()
        End If

    End Sub
    ''' <summary>
    ''' 状态恢复为Normal后尝试重新启动
    ''' </summary>
    Public Overloads Sub TryResumeProcess(sender As GameUnit)
        If sender.UnitPhaseStatus = StateMachineSingleProcessStatus.Normal Then
            Me.StateProcess(sender, sender.UnitPhaseStatus)
            If sender.UnitPhaseStatus = StateMachineSingleProcessStatus.Suspended Then Return
            Me.StateEnd(sender, sender.UnitPhaseStatus)
            If sender.UnitPhaseStatus = StateMachineSingleProcessStatus.NA Then
                RaiseEvent UnitStateComplete(sender)
            ElseIf sender.UnitPhaseStatus = StateMachineSingleProcessStatus.Abort Then
                RaiseEvent StateProcessAbort()
            End If

        End If
    End Sub
    ''' <summary>
    ''' 状态恢复为Normal后尝试重新启动
    ''' </summary>
    Public Overloads Sub TryResumeProcess(sender As SkirmishGameLoop)
        If sender.SkirmishGlobalStateStatus = StateMachineSingleProcessStatus.Normal Then
            Me.StateProcess(sender, sender.SkirmishGlobalStateStatus)
            If sender.SkirmishGlobalStateStatus = StateMachineSingleProcessStatus.Suspended Then Return
            Me.StateEnd(sender, sender.SkirmishGlobalStateStatus)
            If sender.SkirmishGlobalStateStatus = StateMachineSingleProcessStatus.NA Then
                RaiseEvent GlobalStateComplete(sender)
            ElseIf sender.SkirmishGlobalStateStatus = StateMachineSingleProcessStatus.Abort Then
                RaiseEvent StateProcessAbort()
            End If
        End If
    End Sub

    Public MustOverride Overloads Sub StateStart(sender As GameUnit, ByRef status As StateMachineSingleProcessStatus)
    Public MustOverride Overloads Sub StateStart(sender As SkirmishGameLoop, ByRef status As StateMachineSingleProcessStatus)
    Public MustOverride Overloads Sub StateProcess(sender As GameUnit, ByRef status As StateMachineSingleProcessStatus)
    Public MustOverride Overloads Sub StateProcess(sender As SkirmishGameLoop, ByRef status As StateMachineSingleProcessStatus)
    Public MustOverride Overloads Sub StateEnd(sender As GameUnit, ByRef status As StateMachineSingleProcessStatus)
    Public MustOverride Overloads Sub StateEnd(sender As SkirmishGameLoop, ByRef status As StateMachineSingleProcessStatus)

End Class

''' <summary>
''' 遭遇战回合开始阶段类
''' </summary>
Public Class SkirmishTurnStartPhase
    Inherits SkirmishPhaseSingleState

    Public Overrides Sub StateStart(sender As GameUnit, ByRef status As StateMachineSingleProcessStatus)
        status = StateMachineSingleProcessStatus.Ending
    End Sub

    Public Overrides Sub StateStart(sender As SkirmishGameLoop, ByRef status As StateMachineSingleProcessStatus)
        Debug.WriteLine("回合开始阶段")
        status = StateMachineSingleProcessStatus.Normal
    End Sub

    Public Overrides Sub StateProcess(sender As GameUnit, ByRef status As StateMachineSingleProcessStatus)
    End Sub

    Public Overrides Sub StateProcess(sender As SkirmishGameLoop, ByRef status As StateMachineSingleProcessStatus)
        sender.ChangeTurn()
    End Sub

    Public Overrides Sub StateEnd(sender As GameUnit, ByRef status As StateMachineSingleProcessStatus)
        status = StateMachineSingleProcessStatus.NA
    End Sub

    Public Overrides Sub StateEnd(sender As SkirmishGameLoop, ByRef status As StateMachineSingleProcessStatus)
        Dim ifWin As Boolean = sender.CheckTurnCountWin()
        If ifWin Then
            status = StateMachineSingleProcessStatus.Abort
        Else
            status = StateMachineSingleProcessStatus.NA
        End If
    End Sub
End Class

''' <summary>
''' 遭遇战准备阶段类
''' </summary>
Public Class SkirmishPreparePhase
    Inherits SkirmishPhaseSingleState

    Public Overrides Sub StateStart(sender As GameUnit, ByRef status As StateMachineSingleProcessStatus)
        status = StateMachineSingleProcessStatus.Normal
    End Sub

    Public Overrides Sub StateStart(sender As SkirmishGameLoop, ByRef status As StateMachineSingleProcessStatus)
        Debug.WriteLine("准备阶段")
        status = StateMachineSingleProcessStatus.Normal
    End Sub

    Public Overrides Sub StateProcess(sender As GameUnit, ByRef status As StateMachineSingleProcessStatus)
        If sender.Status = UnitStatus.OutOfTurn Then
            sender.ResetUnitStatus(UnitStatus.Waiting, 1, 1)
        End If
    End Sub

    Public Overrides Sub StateProcess(sender As SkirmishGameLoop, ByRef status As StateMachineSingleProcessStatus)
    End Sub

    Public Overrides Sub StateEnd(sender As GameUnit, ByRef status As StateMachineSingleProcessStatus)
        status = StateMachineSingleProcessStatus.NA
    End Sub

    Public Overrides Sub StateEnd(sender As SkirmishGameLoop, ByRef status As StateMachineSingleProcessStatus)
        status = StateMachineSingleProcessStatus.NA
    End Sub
End Class

''' <summary>
''' 遭遇战主要阶段1类
''' </summary>
Public Class SkirmishMainAPhase
    Inherits SkirmishPhaseSingleState

    Public Overrides Sub StateStart(sender As GameUnit, ByRef status As StateMachineSingleProcessStatus)
        status = StateMachineSingleProcessStatus.Normal
    End Sub

    Public Overrides Sub StateStart(sender As SkirmishGameLoop, ByRef status As StateMachineSingleProcessStatus)
        Debug.WriteLine("主要阶段1")
        status = StateMachineSingleProcessStatus.Normal
    End Sub

    Public Overrides Sub StateProcess(sender As GameUnit, ByRef status As StateMachineSingleProcessStatus)
        '暂停状态以等待用户操作
        If Not sender.AllActionDone() Then
            status = StateMachineSingleProcessStatus.Suspended
            Return
        End If
        '已完成行动，进入主要阶段2
        status = StateMachineSingleProcessStatus.Ending
    End Sub


    Public Overrides Sub StateProcess(sender As SkirmishGameLoop, ByRef status As StateMachineSingleProcessStatus)
        If Not sender.AllUnitStateComplete Then
            status = StateMachineSingleProcessStatus.Suspended
            Return
        End If
        status = StateMachineSingleProcessStatus.Ending
    End Sub

    Public Overrides Sub StateEnd(sender As GameUnit, ByRef status As StateMachineSingleProcessStatus)
        status = StateMachineSingleProcessStatus.NA
    End Sub

    Public Overrides Sub StateEnd(sender As SkirmishGameLoop, ByRef status As StateMachineSingleProcessStatus)
        status = StateMachineSingleProcessStatus.NA
    End Sub
End Class

''' <summary>
''' 遭遇战战斗阶段类
''' </summary>
<Obsolete("取消战斗阶段，命令均在主要阶段1进行，完成后进入主要阶段2", True)>
Public Class SkirmishBattlePhase
    Inherits SkirmishPhaseSingleState

    Public Overrides Sub StateStart(sender As GameUnit, ByRef status As StateMachineSingleProcessStatus)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub StateStart(sender As SkirmishGameLoop, ByRef status As StateMachineSingleProcessStatus)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub StateProcess(sender As GameUnit, ByRef status As StateMachineSingleProcessStatus)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub StateProcess(sender As SkirmishGameLoop, ByRef status As StateMachineSingleProcessStatus)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub StateEnd(sender As GameUnit, ByRef status As StateMachineSingleProcessStatus)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub StateEnd(sender As SkirmishGameLoop, ByRef status As StateMachineSingleProcessStatus)
        Throw New NotImplementedException()
    End Sub
End Class

''' <summary>
''' 遭遇战主要阶段2类
''' </summary>
Public Class SkirmishMainBPhase
    Inherits SkirmishPhaseSingleState

    Public Overrides Sub StateStart(sender As GameUnit, ByRef status As StateMachineSingleProcessStatus)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub StateStart(sender As SkirmishGameLoop, ByRef status As StateMachineSingleProcessStatus)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub StateProcess(sender As GameUnit, ByRef status As StateMachineSingleProcessStatus)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub StateProcess(sender As SkirmishGameLoop, ByRef status As StateMachineSingleProcessStatus)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub StateEnd(sender As GameUnit, ByRef status As StateMachineSingleProcessStatus)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub StateEnd(sender As SkirmishGameLoop, ByRef status As StateMachineSingleProcessStatus)
        Throw New NotImplementedException()
    End Sub
End Class

''' <summary>
''' 遭遇战回合结束阶段类
''' </summary>
Public Class SkirmishTurnEndPhase
    Inherits SkirmishPhaseSingleState

    Public Overrides Sub StateStart(sender As GameUnit, ByRef status As StateMachineSingleProcessStatus)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub StateStart(sender As SkirmishGameLoop, ByRef status As StateMachineSingleProcessStatus)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub StateProcess(sender As GameUnit, ByRef status As StateMachineSingleProcessStatus)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub StateProcess(sender As SkirmishGameLoop, ByRef status As StateMachineSingleProcessStatus)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub StateEnd(sender As GameUnit, ByRef status As StateMachineSingleProcessStatus)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub StateEnd(sender As SkirmishGameLoop, ByRef status As StateMachineSingleProcessStatus)
        Throw New NotImplementedException()
    End Sub
End Class

''' <summary>
''' 遭遇战回合外阶段类
''' </summary>
Public Class SkirmishOutOfTurnPhase
    Inherits SkirmishPhaseSingleState

    Public Overrides Sub StateStart(sender As GameUnit, ByRef status As StateMachineSingleProcessStatus)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub StateStart(sender As SkirmishGameLoop, ByRef status As StateMachineSingleProcessStatus)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub StateProcess(sender As GameUnit, ByRef status As StateMachineSingleProcessStatus)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub StateProcess(sender As SkirmishGameLoop, ByRef status As StateMachineSingleProcessStatus)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub StateEnd(sender As GameUnit, ByRef status As StateMachineSingleProcessStatus)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub StateEnd(sender As SkirmishGameLoop, ByRef status As StateMachineSingleProcessStatus)
        Throw New NotImplementedException()
    End Sub
End Class
