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
    Public MovementGlanceBorder As PathGeometry = Nothing

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

    Public Function GlanceContains(position As PointI) As MovementGlanceNode
        For Each item In Me.MovementGlance
            If item.Position.X = position.X AndAlso item.Position.Y = position.Y Then
                Return item
            End If
        Next
        Return Nothing
    End Function

    Public Sub ExpandMovementNode(targetNode As MovementGlanceNode, targetUnit As GameUnit)
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

    ''' <summary>
    ''' 根据已生成的GlanceNodes绘制最大行动范围圈
    ''' </summary>
    Public Sub GenerateMoveRangeBorder()
        If Me.MovementGlanceBorder IsNot Nothing Then
            Me.MovementGlanceBorder.Dispose()    '销毁缓存
        End If
        'TODO

    End Sub


    Public Sub Dispose()

    End Sub

End Class

''' <summary>
''' 行动预览节点类
''' </summary>
Public Class MovementGlanceNode
    Public Position As PointI = Nothing
    Public Parent As MovementGlanceNode = Nothing
    Public CumulativeMovePoint As Single = 0.0F

End Class
