Imports p104
''' <summary>
''' 游戏物品基类，用于消耗品，装备，合成素材等等
''' </summary>
Public Class GameItem
    Implements ISaveProperty

    Public Function GetSaveString() As String Implements ISaveProperty.GetSaveString
        Throw New NotImplementedException()
    End Function

    Public Function Copy() As GameItem
        Dim result As New GameItem

        Return result
    End Function

End Class
