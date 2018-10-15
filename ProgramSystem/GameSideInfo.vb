''' <summary>
''' 遭遇战势力信息类
''' </summary>
Public Class GameSideInfo
    ''' <summary>
    ''' 势力名
    ''' </summary>
    Public Property SideName As String = DEFAULT_STRING
    ''' <summary>
    ''' 操作者类型
    ''' </summary>
    Public Property Control As PlayerType

    Public Property Team As GameTeamInfo


    Public Sub New(name As String, inputPlayer As PlayerType, teamIndex As Short)
        With Me
            .SideName = name
            .Control = inputPlayer
            If teamIndex >= GameTeamInfo.GetTeamCount Then
                GameTeamInfo.CreateTeam(teamIndex)
            End If
            .Team = GameTeamInfo.GetTeam(teamIndex)
        End With

    End Sub


End Class

Public Enum PlayerType As Byte
    Closed = 0
    Open = 1
    AllAccess = 2
    Player = 3
    AI = 4

End Enum
