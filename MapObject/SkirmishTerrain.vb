''' <summary>
''' 游戏遭遇战地形类
''' </summary>
Public Class SkirmishTerrain

    ''' <summary>
    ''' 默认地形阻力
    ''' </summary>
    Public Shared TERRAIN_MOVE_COST() As Single

    Public Shared Sub Initialize()
        ReDim TERRAIN_MOVE_COST(128)
        TERRAIN_MOVE_COST(0) = 1.0F
        TERRAIN_MOVE_COST(1) = 4.0F
        TERRAIN_MOVE_COST(2) = 6.0F
        TERRAIN_MOVE_COST(3) = 4.0F
        TERRAIN_MOVE_COST(4) = 2.0F
        TERRAIN_MOVE_COST(5) = 1.0F
        TERRAIN_MOVE_COST(6) = 1.5F
        TERRAIN_MOVE_COST(7) = 4.0F
        TERRAIN_MOVE_COST(8) = 1.0F
        TERRAIN_MOVE_COST(128) = TERRAIN_COST_MAX
    End Sub

    Public Shared Function GetOriginalTerrainCost(terrain As TerrainType) As Single
        Return TERRAIN_MOVE_COST(terrain)
    End Function

    Public Shared Function CalculateFinalTerrainCost(nextTerrain As TerrainType, unit As GameUnit, deltaAltitude As Short, Optional allowFalling As Boolean = False) As Single
        If deltaAltitude = 1 Then
            Return TERRAIN_MOVE_COST(nextTerrain) * unit.GetMovementTerrainRatio(nextTerrain) + 1
        ElseIf deltaAltitude = 0 OrElse deltaAltitude = -1 Then
            Return TERRAIN_MOVE_COST(nextTerrain) * unit.GetMovementTerrainRatio(nextTerrain)
        ElseIf deltaAltitude <= -2 Then
            If allowFalling Then
                Return TERRAIN_MOVE_COST(nextTerrain) * unit.GetMovementTerrainRatio(nextTerrain)
            End If
        End If
        Return TERRAIN_COST_MAX
    End Function

End Class

''' <summary>
''' 遭遇战地形枚举
''' </summary>
Public Enum TerrainType As Byte
    Ground = 0
    FullWater = 1
    Mountain = 2
    Forest = 3
    Village = 4
    Grass = 5
    Sand = 6
    River = 7
    StoneRoad = 8

    None = 128
End Enum
