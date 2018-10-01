Public Class SkirmishTerrain

    Public Shared Function GetOriginalTerrainCost(terrain As TerrainType) As Short
        If terrain = TerrainType.None Then
            Return 99
        ElseIf terrain = TerrainType.Ground OrElse terrain = TerrainType.Grass OrElse terrain = TerrainType.StoneRoad Then
            Return 1
        Else
            Return 2
        End If

    End Function


End Class

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
