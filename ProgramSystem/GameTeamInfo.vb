Public Class GameTeamInfo

    Private Shared TeamRepository As New List(Of GameTeamInfo)

    Public TeamName As String = DEFAULT_STRING

    Public Shared Function GetTeamCount() As Short
        Return TeamRepository.Count
    End Function

    Public Shared Function GetTeam(index As Short) As GameTeamInfo
        Return TeamRepository(index)
    End Function

    Public Shared Sub CreateTeam(upperBound As Short)
        While (TeamRepository.Count - 1 < upperBound AndAlso TeamRepository.Count < 16)
            Dim tmpTeam As New GameTeamInfo
            TeamRepository.Add(tmpTeam)
        End While
    End Sub
End Class
