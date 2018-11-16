' -----------------------------------------
' Copyright (c) 2018 All Rights Reserved.
' 
' Filename: GameAchievement
' Author: Miz
' Date: 2018/11/13 17:56:12
' -----------------------------------------

Public Class GameAchievement

    Public Acquired As Boolean = False

    Public Title As String = vbNullString

    Public Description As String = vbNullString

    Public Credit As IntegerProperty = Nothing

    Public Event Check()


    Public Sub CheckCondition()

    End Sub

End Class
