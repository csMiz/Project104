Imports System.IO
Imports SharpDX.Direct2D1
Imports System.Math

Public Class SkirmishMap
    Public Blocks(49, 49) As SkirmishMapBlock
    Public MapSizeXMax As Short = 49
    Public MapSizeYMax As Short = 49

    Private PaintOrderList As New List(Of SkirmishMapBlock)

    Public Property ResourcesLoaded As Boolean = False

    Public MovementGlance As New List(Of MovementGlanceNode)

    Public Sub New()
        For i = 0 To 49
            For j = 0 To 49
                Blocks(i, j) = New SkirmishMapBlock
                With Blocks(i, j)
                    .X = i
                    .Y = j
                    .InitializeWorldPosition()
                End With
            Next
        Next
    End Sub

    ''' <summary>
    ''' 载入地图二进制文件并销毁stream对象
    ''' </summary>
    ''' <param name="stream">文件流</param>
    Public Sub LoadFromFile(ByRef stream As FileStream)
        Dim content() As Byte
        Using br As New BinaryReader(stream)
            content = br.ReadBytes(5000)
        End Using
        stream.Close()
        stream.Dispose()

        For i = 0 To 4998 Step 2
            Dim blockIndex As Short = i / 2
            With Blocks(blockIndex Mod 50, blockIndex \ 50)
                .Terrain = content(i)
            End With
        Next
        For i = 1 To 4999 Step 2
            Dim blockIndex As Short = (i - 1) / 2
            With Blocks(blockIndex Mod 50, blockIndex \ 50)
                .Altitude = content(i)
            End With
        Next
        content = Nothing

    End Sub

    Public Sub LoadAccessories(context As DeviceContext, zoom As Single)
        For i = 0 To 49
            For j = 0 To 49
                Blocks(i, j).InitializeAccessories(context, zoom)
            Next
        Next
    End Sub

    ''' <summary>
    ''' 按高度分层
    ''' </summary>
    Private Sub DivideLayers(ByVal X1 As Short, ByVal X2 As Short, ByVal Y1 As Short, ByVal Y2 As Short)
        PaintOrderList.Clear()
        Dim middleX As Short = (X1 + X2) / 2
        If X1 < 0 Then X1 = 0
        If X2 > MapSizeXMax Then X2 = MapSizeXMax
        If Y1 < 0 Then Y1 = 0
        If Y2 > MapSizeYMax Then Y2 = MapSizeYMax

        For j = Y1 * 2 To Y2 * 2   '把锯齿状的一行错开，分成两行
            If j Mod 2 = 0 Then
                Dim tj As Short = j / 2
                For i1 = X1 To middleX - 1
                    Dim block As SkirmishMapBlock = Blocks(i1, tj)
                    If i1 Mod 2 = 0 Then
                        PaintOrderList.Add(block)
                    End If
                Next
                For i2 = X2 To middleX Step -1
                    Dim block As SkirmishMapBlock = Blocks(i2, tj)
                    If i2 Mod 2 = 0 Then
                        PaintOrderList.Add(block)
                    End If
                Next
            Else
                Dim tj As Short = (j - 1) / 2
                For i1 = X1 To middleX - 1
                    Dim block As SkirmishMapBlock = Blocks(i1, tj)
                    If i1 Mod 2 = 1 Then
                        PaintOrderList.Add(block)
                    End If
                Next
                For i2 = X2 To middleX Step -1
                    Dim block As SkirmishMapBlock = Blocks(i2, tj)
                    If i2 Mod 2 = 1 Then
                        PaintOrderList.Add(block)
                    End If
                Next
            End If

        Next


    End Sub

    ''' <summary>
    ''' 更新基底绘制参数
    ''' </summary>
    Public Sub UpdateBlocksInCamera(X1 As Short, X2 As Short, Y1 As Short, Y2 As Short, cameraFocus As PointF2, resolve As PointI, zoom As Single)
        For i = X1 To X2
            For j = Y1 To Y2
                If i >= 0 AndAlso i <= MapSizeXMax AndAlso j >= 0 AndAlso j <= MapSizeYMax Then
                    Dim block As SkirmishMapBlock = Blocks(i, j)
                    block.UpdateVertices(cameraFocus, resolve, zoom)
                End If
            Next
        Next
    End Sub

    ''' <summary>
    ''' 画遭遇战地图
    ''' </summary>
    ''' <param name="context">d2dContext对象</param>
    ''' <param name="spectator">观察者对象</param>
    ''' <param name="canvasBitmap">传入的原始Bitmap1对象</param>
    Public Sub DrawHexMap(ByRef context As SharpDX.Direct2D1.DeviceContext, ByRef spectator As SpectatorCamera, canvasBitmap As Bitmap1)
        'Dim brush1 As New SolidColorBrush(context, New RawColor4(0.9, 0.2, 0.2, 1.0))
        If Not ResourcesLoaded Then Exit Sub

        Dim cameraX As Single = spectator.CameraFocus.X
        Dim cameraY As Single = spectator.CameraFocus.Y
        Dim zoom As Single = spectator.Zoom

        Dim drawRangeX As Short = Truncate((cameraX - SIX_TWO_FIVE) / THREE_SEVEN_FIVE)
        Dim drawRangeY As Short = Truncate((cameraY - SIX_TWO_FIVE_ROOT3) / TWO_FIFTY_ROOT3)
        DivideLayers(drawRangeX - 5, drawRangeX + 5, drawRangeY - 5, drawRangeY + 5)
        UpdateBlocksInCamera(drawRangeX - 5, drawRangeX + 5, drawRangeY - 5, drawRangeY + 5, spectator.CameraFocus, spectator.Resolve, zoom)

        With context
            For i = 0 To PaintOrderList.Count - 1
                '画地图块
                Dim block As SkirmishMapBlock = PaintOrderList(i)
                If block.Terrain <> TerrainType.None Then
                    block.PaintMapBlock(context)
                End If
            Next

            '画参考中央圈
            '.DrawEllipse(New Ellipse With {
            '             .Point = New RawVector2(spectator.Resolve.X / 2, spectator.Resolve.Y / 2),
            '             .RadiusX = 25,
            '             .RadiusY = 25}, brush1)

        End With

    End Sub

    ''' <summary>
    ''' 生成角色行动范围
    ''' </summary>
    ''' <param name="targetUnit">选定操作的角色</param>
    Public Sub GenerateMoveRange(targetUnit As GameUnit)
        Me.MovementGlance.Clear()
        Dim startPoint As New MovementGlanceNode With {
            .Position = targetUnit.Position2,
            .Parent = Nothing,
            .CumulativeMovePoint = 0.0F}
        Me.MovementGlance.Add(startPoint)

        Call Me.ExpandMovementNode(startPoint, targetUnit)
        Call PrintMovementGlance()
    End Sub

    ''' <summary>
    ''' 打印角色行动范围结果
    ''' </summary>
    Public Sub PrintMovementGlance()
        Dim result As String = "MOV Glance: [" & vbCrLf
        For j = 0 To 9
            For i = 0 To 9
                Dim tmpPoint As MovementGlanceNode = GlanceContains(New PointI(i, j))
                If tmpPoint IsNot Nothing Then
                    result = result & tmpPoint.CumulativeMovePoint & ","
                Else
                    result = result & "x,"
                End If
            Next
            result = result & vbCrLf
        Next
        Debug.Print(result & "]" & vbCrLf)
    End Sub

    Public Sub DrawMovementGlanceLayer(ByRef context As SharpDX.Direct2D1.DeviceContext, ByRef spectator As SpectatorCamera, canvasBitmap As Bitmap1)

        '单向描边：
        '1. 从所有可到达的格子中随机取出一个格子
        '2. 如果此格子所有边缘均有相邻的格子，则将其标记为“内部”，否则为“边缘”
        '若为内部， 则标记完后重新取。若为边缘， 标记所有裸露的边， 随机取裸露边上一点作为起点。
        '3. 按顺时针方向检测下一个点，走完每一条裸露边后立即标记已走过，遇到非裸露边时按边的方向计算下一个格子
        '4. 若检测到下一个点为起点，则这条边缘检测结束，将其加入边缘列表
        '5. 遍历所有格子，若存在格子有裸露边未被标记为已走过，则说明存在空洞
        '6. 将所有未走完的格子组成新的集合，从过程1开始重复，直到所有格子都走完
        '7. 绘制边缘列表中的所有边缘


        'calculate outlines
        Dim edgeList As New List(Of List(Of SharpDX.Mathematics.Interop.RawVector2))
        Dim outlineList As New List(Of List(Of MovementGlanceNode))
        Dim allGlanceNodes As New List(Of MovementGlanceNode)(Me.MovementGlance)
        outlineList.Add(allGlanceNodes)
        Do
            Dim tmpList As List(Of MovementGlanceNode) = outlineList.First
            outlineList.RemoveAt(0)
            Call Me.ExtractGlanceOutline(tmpList, outlineList, edgeList)
        Loop While outlineList.Count
        'draw


    End Sub

    Private Sub ExtractGlanceOutline(nodes As List(Of MovementGlanceNode), hollowList As List(Of List(Of MovementGlanceNode)), edgeList As List(Of List(Of SharpDX.Mathematics.Interop.RawVector2)))
        'Remove all 'Inside' Block
        For j = nodes.Count - 1 To 0 Step -1
            Dim getNode As MovementGlanceNode = nodes(j)
            For i As HexBlockDirection = HexBlockDirection.Top To HexBlockDirection.TopLeft
                If GlanceContains(GetNextBlockPosition(getNode.Position, i)) IsNot Nothing Then
                    getNode.NeighbourType += 2 ^ i
                End If
            Next
            If getNode.NeighbourType = 63 Then    '是内部块
                nodes.RemoveAt(j)
            End If
        Next
        'Choose 'Start' Block
        Dim startNode As MovementGlanceNode = nodes.First
        Dim allNodePoints As New List(Of SharpDX.Mathematics.Interop.RawVector2)
        With allNodePoints
            Dim tmpBlock As SkirmishMapBlock = Blocks(startNode.Position.X, startNode.Position.Y)
            .Add(tmpBlock.V_TR)
            .Add(tmpBlock.V_R)
            .Add(tmpBlock.V_BR)
            .Add(tmpBlock.V_BL)
            .Add(tmpBlock.V_L)
            .Add(tmpBlock.V_TL)
            .Add(tmpBlock.V3D_TR)
            .Add(tmpBlock.V3D_R)
            .Add(tmpBlock.V3D_BR)
            .Add(tmpBlock.V3D_BL)
            .Add(tmpBlock.V3D_L)
            .Add(tmpBlock.V3D_TL)
        End With
        Dim nodePoints As New List(Of SharpDX.Mathematics.Interop.RawVector2)
        For i = 0 To 5
            If i <> 5 Then
                If (startNode.NeighbourType >> (4 - i)) = 3 Then
                    nodePoints.Add(allNodePoints(i))
                    nodePoints.Add(allNodePoints(i + 6))
                End If
            Else

            End If
        Next


        'HACK: 不可行


    End Sub

    Public Function GlanceContains(position As PointI) As MovementGlanceNode
        For Each item In Me.MovementGlance
            If item.Position.X = position.X AndAlso item.Position.Y = position.Y Then
                Return item
            End If
        Next
        Return Nothing
    End Function

    Private Sub ExpandMovementNode(targetNode As MovementGlanceNode, targetUnit As GameUnit)
        Dim nodePosition As PointI = targetNode.Position
        Dim cumulativePoint As Single = targetNode.CumulativeMovePoint
        Dim pointUBound As Single = targetUnit.FinalMovePoint

        'top(0,-1) tl(-1,0)or(-1,-1) tr(1,0)or(1,-1) bl(-1,1)or(-1,0) br(1,1)or(1,0) bottom(0,1)
        Dim neighbour As New List(Of PointI)
        If nodePosition.Y <> 0 Then neighbour.Add(New PointI(0, -1))    'top
        If nodePosition.Y <> 49 Then neighbour.Add(New PointI(0, 1))    'bottom
        If CBool(nodePosition.X Mod 2) Then    'type 2
            With neighbour
                If nodePosition.X <> 0 Then .Add(New PointI(-1, 0))                                 'top_left
                If nodePosition.X <> 49 Then .Add(New PointI(1, 0))                                 'top_right
                If nodePosition.X <> 0 AndAlso nodePosition.Y <> 49 Then .Add(New PointI(-1, 1))    'bottom_left
                If nodePosition.X <> 49 AndAlso nodePosition.Y <> 49 Then .Add(New PointI(1, 1))    'bottom_right
            End With
        Else    'type 1
            With neighbour
                If nodePosition.X <> 0 AndAlso nodePosition.Y <> 0 Then .Add(New PointI(-1, -1))    'top_left
                If nodePosition.X <> 49 AndAlso nodePosition.Y <> 0 Then .Add(New PointI(1, -1))    'top_right
                If nodePosition.X <> 0 Then .Add(New PointI(-1, 0))                                 'bottom_left
                If nodePosition.X <> 49 Then .Add(New PointI(1, 0))                                 'bottom_right
            End With
        End If

        For Each delta As PointI In neighbour
            Dim tmpPosition As New PointI(nodePosition.X + delta.X, nodePosition.Y + delta.Y)
            Dim neighbour_terrain As TerrainType = Blocks(tmpPosition.X, tmpPosition.Y).Terrain
            Dim deltaAltitude As Short = Blocks(tmpPosition.X, tmpPosition.Y).Altitude - Blocks(nodePosition.X, nodePosition.Y).Altitude
            Dim cost As Single = cumulativePoint + SkirmishTerrain.CalculateFinalTerrainCost(neighbour_terrain, targetUnit, deltaAltitude)
            'checkUnitOccupied(cost,new pointi(nodePosition.X, nodePosition.Y - 1))
            '              => function(byref cost, pos):if occupied then cost = 999
            If cost <= pointUBound Then
                Dim comparePoint As MovementGlanceNode = GlanceContains(tmpPosition)
                If comparePoint Is Nothing Then
                    Dim tmpNewNode As New MovementGlanceNode With {
                        .Position = tmpPosition,
                        .Parent = targetNode,
                        .CumulativeMovePoint = cost}
                    Me.MovementGlance.Add(tmpNewNode)
                    Call ExpandMovementNode(tmpNewNode, targetUnit)
                Else
                    If cost < comparePoint.CumulativeMovePoint Then
                        With comparePoint
                            .Parent = targetNode
                            .CumulativeMovePoint = cost
                        End With
                        Call ExpandMovementNode(comparePoint, targetUnit)
                    End If
                End If
            End If

        Next

    End Sub


    Public Sub Dispose()
        'TODO
    End Sub

    Public Shared Function GetNextBlockPosition(inputPosition As PointI, direction As HexBlockDirection) As PointI
        If direction = HexBlockDirection.Top Then
            Return New PointI(inputPosition.X, inputPosition.Y - 1)
        ElseIf direction = HexBlockDirection.Bottom Then
            Return New PointI(inputPosition.X, inputPosition.Y + 1)
        Else
            If inputPosition.X Mod 2 Then    '奇数列
                Select Case direction
                    Case HexBlockDirection.TopLeft
                        Return New PointI(inputPosition.X - 1, inputPosition.Y)
                    Case HexBlockDirection.BottomLeft
                        Return New PointI(inputPosition.X - 1, inputPosition.Y + 1)
                    Case HexBlockDirection.TopRight
                        Return New PointI(inputPosition.X + 1, inputPosition.Y)
                    Case Else
                        Return New PointI(inputPosition.X + 1, inputPosition.Y + 1)
                End Select
            Else    '偶数列
                Select Case direction
                    Case HexBlockDirection.TopLeft
                        Return New PointI(inputPosition.X - 1, inputPosition.Y - 1)
                    Case HexBlockDirection.BottomLeft
                        Return New PointI(inputPosition.X - 1, inputPosition.Y)
                    Case HexBlockDirection.TopRight
                        Return New PointI(inputPosition.X + 1, inputPosition.Y - 1)
                    Case Else
                        Return New PointI(inputPosition.X + 1, inputPosition.Y)
                End Select
            End If
        End If
    End Function

End Class

''' <summary>
''' 行动预览节点类
''' </summary>
Public Class MovementGlanceNode
    Public Position As PointI = Nothing
    Public Parent As MovementGlanceNode = Nothing
    Public CumulativeMovePoint As Single = 0.0F

    ''' <summary>
    ''' 用6bits表示周围六格，从右到左分别为T, TR, BR, B, BL, TL
    ''' <para> 如(binary)00111111表示周围六格均有效
    ''' </para>
    ''' </summary>
    Public NeighbourType As Byte = 0

End Class

Public Enum HexBlockDirection As Byte
    Top = 0
    TopRight = 1
    BottomRight = 2
    Bottom = 3
    BottomLeft = 4
    TopLeft = 5
End Enum
