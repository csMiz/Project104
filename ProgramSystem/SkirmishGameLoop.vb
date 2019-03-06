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

    <Obsolete("use SkirmishMap2", True)>
    Public SkirmishGameMap As SkirmishMap = Nothing
    Public SkirmishGameMap2 As SkirmishMap2 = Nothing

    Public BindingCamera As SpectatorCamera

    ''' <summary>
    ''' 处理控件面板等鼠标点击事件
    ''' </summary>
    Public SkirmishPage As GamePageProperty = Nothing
    ''' <summary>
    ''' 地图鼠标交互层
    ''' </summary>
    Public UI_SkirmishBoard As GameChessboard = Nothing

    Public UI_BottomLeftUnitAvatar As GameContentFrame = Nothing

    Public UI_TopLeftResourceBar As GameContentFrame = Nothing

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
    ''' 当前阶段
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
        With BindingCamera.Camera3D
            .Position = New RawVector3(200, -350, 1000)
            .Rotation = New PointF3(-0.5236, 0, 0)
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

    <Obsolete("render 3d-skirmish map and 2d-ui separately", False)>
    Public Sub DrawSkirmishMapLayer(ByRef context As SharpDX.Direct2D1.DeviceContext, ByRef spectator As SpectatorCamera, canvasBitmap As Bitmap1)
        'Me.SkirmishGameMap.DrawHexMap(context, spectator, canvasBitmap)    'this is ver1.0
        spectator.Camera3D.DrawContainer(context, spectator, canvasBitmap)    'this is ver1.1

        'Me.DrawUnitLayer(context, spectator, canvasBitmap)

        Me.SkirmishPage.PaintElements(context, spectator, canvasBitmap)
    End Sub

    Public Sub DrawSkirmish2DUILayer(ByRef context As SharpDX.Direct2D1.DeviceContext, ByRef spectator As SpectatorCamera, canvasBitmap As Bitmap1)
        'TODO: DrawSkirmish2DUILayer

    End Sub

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
                                     If senderType Then
                                         Debug.WriteLine("回合开始阶段")
                                         turn_start_phase.ToProcess(sender, senderType)
                                     Else
                                         turn_start_phase.ToEnd(sender, senderType)
                                     End If
                                 End Sub
        Dim turn_start_phase_p = Sub(sender As IStateMachineRecognizable, senderType As StateInputType)
                                     CType(sender, SkirmishGameLoop).ChangeTurn()
                                     turn_start_phase.ToEnd(sender, senderType)
                                 End Sub
        Dim turn_start_phase_e = Sub(sender As IStateMachineRecognizable, senderType As StateInputType)
                                     If senderType Then
                                         Dim c_sender As SkirmishGameLoop = CType(sender, SkirmishGameLoop)
                                         Dim ifWin As Boolean = c_sender.CheckTurnCountWin()
                                         If ifWin Then
                                             sender.SetProcessTag(StateMachineSingleProcessStatus.Abort)
                                         Else
                                             sender.SetProcessTag(StateMachineSingleProcessStatus.NA)
                                         End If
                                         Dim processContent = Async Sub()
                                                                  Do While (Not c_sender.AllUnitStateComplete())
                                                                      Await Task.Delay(50)
                                                                  Loop
                                                                  c_sender.ResetUnitStateCounter()
                                                                  Dim tmpGlobalPhase As StateMachineSingleState = Me.SkirmishGamePhases.NextState(sender.GetState, SkirmishStateMachineInputAlphabet.NormalGo)
                                                                  tmpGlobalPhase.Trigger(sender, senderType)
                                                                  For Each tmpUnit As GameUnit In c_sender.UnitList
                                                                      If tmpUnit.Player = 0 Then
                                                                          Dim tmpPhase As SkirmishPhaseSingleState = Me.SkirmishGamePhases.NextState(tmpUnit.UnitPhase, SkirmishStateMachineInputAlphabet.NormalGo)
                                                                          tmpPhase.Trigger(tmpUnit)
                                                                          Await Task.Delay(50)
                                                                      End If
                                                                  Next
                                                              End Sub
                                         Dim tmpProcess As New Task(processContent)
                                         tmpProcess.Start()
                                     Else
                                         sender.SetProcessTag(StateMachineSingleProcessStatus.NA)
                                         Me.MarkUnitStateCompleteCounter()
                                         Debug.WriteLine("已完成:" & CType(sender, GameUnit).ShownName)
                                     End If
                                 End Sub
        AddHandler turn_start_phase.StateStart, turn_start_phase_s
        AddHandler turn_start_phase.StateProcess, turn_start_phase_p
        AddHandler turn_start_phase.StateEnd, turn_start_phase_e

        'HACK: Rewrite those states
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

    End Sub

    ''' <summary>
    ''' 开始遭遇战游戏流程
    ''' </summary>
    Public Sub StartSkirmishGameStateMachine()

        Me.SkirmishGlobalStateIndex = SingleGameLoopStage.OutOfTurn
        Me.ResetUnitStateCounter()

        '全局状态进入回合开始阶段
        Dim tmpGlobalPhase As SkirmishPhaseSingleState = Me.SkirmishGamePhases.NextState(Me.SkirmishGlobalStateIndex, SkirmishStateMachineInputAlphabet.NormalGo)
        tmpGlobalPhase.Trigger(Me)
        '每个单位状态进入回合开始阶段
        For Each tmpUnit As GameUnit In UnitList
            If tmpUnit.Player = 0 Then
                Dim tmpPhase As SkirmishPhaseSingleState = Me.SkirmishGamePhases.NextState(tmpUnit.UnitPhase, SkirmishStateMachineInputAlphabet.NormalGo)
                tmpPhase.Trigger(tmpUnit)
            End If
        Next

    End Sub

    ''' <summary>
    ''' 用于回合开始阶段，准备阶段和回合结束阶段。全局状态需要与单位状态同步。
    ''' </summary>
    Public Sub UnitGoNextState1(sender As GameUnit)
        Me.MarkUnitStateCompleteCounter()
        Debug.WriteLine("已完成:" & sender.ShownName)
    End Sub
    Public Sub GlobalGoNextState1(sender As SkirmishGameLoop)
        Dim processContent = Async Sub()
                                 Do While (Not Me.AllUnitStateComplete())
                                     Await Task.Delay(50)
                                 Loop
                                 Me.ResetUnitStateCounter()

                                 Dim tmpGlobalPhase As SkirmishPhaseSingleState = Me.SkirmishGamePhases.NextState(sender.SkirmishGlobalStateIndex, SkirmishStateMachineInputAlphabet.NormalGo)
                                 tmpGlobalPhase.Trigger(sender)
                                 For Each tmpUnit As GameUnit In sender.UnitList
                                     If tmpUnit.Player = 0 Then
                                         Dim tmpPhase As SkirmishPhaseSingleState = Me.SkirmishGamePhases.NextState(tmpUnit.UnitPhase, SkirmishStateMachineInputAlphabet.NormalGo)
                                         tmpPhase.Trigger(tmpUnit)
                                         Await Task.Delay(50)
                                     End If
                                 Next

                             End Sub
        Dim tmpProcess As New Task(processContent)
        tmpProcess.Start()
    End Sub

    ''' <summary>
    ''' 用于主要阶段。全局状态停留在主要阶段1直到所有单位完成行动。
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

        Dim tmpBottomLeftUnitAvatar As New GameContentFrame
        With tmpBottomLeftUnitAvatar
            .BindingContext = myContext
            .BasicRect = New RawRectangleF(0, 568, 200, 768)
            .AbsoluteRect = .BasicRect
            .DefaultBackground = BLACK_COLOUR_BRUSH(2)
            .InitializeControlCanvas()
        End With

        Dim tmpAvatarPicture As New GamePictureBox
        With tmpAvatarPicture
            .Visible = False
            .BasicRect = New RawRectangleF(0, 568, 200, 768)
            .AbsoluteRect = .BasicRect
            .BindingContext = myContext
            .Opacity = NOT_TRANSPARENT
            .Z_Index = 1
            .InitializeControlCanvas()
            .ImageSource = Nothing
        End With
        tmpBottomLeftUnitAvatar.Children.Add(tmpAvatarPicture)
        Dim tmpLeftAvatarMouseDown = Sub()
                                         Debug.WriteLine("avatar clicked!")
                                     End Sub

        AddHandler tmpBottomLeftUnitAvatar.MouseDown, tmpLeftAvatarMouseDown
        'AddHandler tmpBottomLeftUnitAvatar .MouseMove , AddressOf ...
        Me.SkirmishPage.UIElements.Add(tmpBottomLeftUnitAvatar)

        Me.SkirmishPage.UIElements.Add(UnitDetailDialog.Instance)

        Me.SkirmishPage.GenerateElementsQuadtree(Me.BindingCamera.Resolve)
        Me.SkirmishPage.InitializeCursor(Me.BindingCamera.CurrentCursorPosition, Me.BindingCamera.Resolve)

        Me.BindingCamera.PaintingLayers.Push(AddressOf Me.SkirmishPage.PaintElements)
        'TODO: Add painting description
        Me.BindingCamera.ActivePages.Add(Me.SkirmishPage)

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
    ''' <returns></returns>
    Public Function DrawPositionToChessboard(mouse As PointI) As PointI
        'FIXME:
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
        'Me.SkirmishBoardMouseDownPosition = Me.MousePositionToChessboard(Me.ConvertToWorldCursor(e.Position))
        Me.SkirmishBoardMouseDownPosition = Me.DrawPositionToChessboard(e.Position)        '改用绘图坐标直接进行定位
        Debug.WriteLine("Pos:" & SkirmishBoardMouseDownPosition.X & ", " & SkirmishBoardMouseDownPosition.Y)

        'test
        'open unit detail dialog
        Dim dialog As UnitDetailDialog = UnitDetailDialog.Instance
        dialog.BindUnit(Me.UnitList(0))
        dialog.Visible = True
        'BindingCamera.PaintingLayers.Push(AddressOf dialog.DrawControl)
        'BindingCamera.PaintingLayersDescription.Push(GameImageLayer.Skirmish_UnitDetail)

    End Sub

    Public Sub GameBoardMouseUp(e As GameMouseEventArgs)
        Dim position As PointI = Me.DrawPositionToChessboard(e.Position)
        If position.X = Me.SkirmishBoardMouseDownPosition.X AndAlso position.Y = Me.SkirmishBoardMouseDownPosition.Y Then
            'is click
            'show move range

        Else    'is drag

        End If

    End Sub

    Public Sub GameBoardMouseMove(e As GameMouseEventArgs)

        'Debug.WriteLine(e.X & "-" & e.Y)
        Dim hr As PointF2 = BindingCamera.Camera3D.BindingHalfResolve
        BindingCamera.Camera3D.ScreenCursorOffset.X = (e.X - hr.X) / hr.X
        BindingCamera.Camera3D.ScreenCursorOffset.Y = -(e.Y - hr.Y) / hr.Y

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
