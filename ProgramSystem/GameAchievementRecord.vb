' -----------------------------------------
' Copyright (c) 2018 All Rights Reserved.
' 
' Filename: GameAchievementRecord
' Author: Miz
' Date: 2018/11/13 14:45:54
' -----------------------------------------

''' <summary>
''' 用户成就记录类
''' </summary>
Public Class GameAchievementRecord

    Public Achievements(36) As GameAchievement

    Public TotalGameTime As TimeSpan = Nothing


    Public Sub New()

    End Sub

    Public Sub InitializeEmptyRecord()

    End Sub

    Public Function GetAchievementByte() As Byte()
        Dim result(7) As Byte
        For i = 0 To 7
            result(i) = &H0
        Next
        For i = 0 To 36
            If Achievements(i).Acquired Then
                result(7 - (i \ 8)) += 2 ^ (i Mod 8)
            End If
        Next
        Return result
    End Function

    Public Function GetTotalTimeByte() As Byte()
        Dim minutes As Double = TotalGameTime.TotalMinutes
        Return BitConverter.GetBytes(CSng(minutes))
    End Function

End Class
