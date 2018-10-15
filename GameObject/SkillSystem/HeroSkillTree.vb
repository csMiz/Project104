Imports p104
''' <summary>
''' 英雄技能树类
''' </summary>
Public Class HeroSkillTree
    Implements ISaveProperty

    Public BaseSkills As New List(Of HeroSkillTreeItem)

    Public Function GetSaveString() As String Implements ISaveProperty.GetSaveString
        Throw New NotImplementedException()
    End Function

    Public Function Copy() As HeroSkillTree
        Dim result As New HeroSkillTree
        For Each item In Me.BaseSkills
            result.BaseSkills.Add(item.copy)
        Next
        Return result
    End Function

End Class

Public Class HeroSkillTreeItem
    ''' <summary>
    ''' 对应技能的id
    ''' </summary>
    Public Property LinkId As Integer

    Public Property Name As String

    Public Property Description As String

    Public Children As List(Of HeroSkillTreeItem)

    Public Property Status As SkillTreeItemStatus

    Public Function Acquire() As HeroSkill

    End Function

    Public Function Copy() As HeroSkillTreeItem
        Dim result As New HeroSkillTreeItem
        With result
            .LinkId = Me.LinkId
            .Name = Me.Name
            .Description = Me.Description
            For Each item In Me.Children
                .Children.Add(item.Copy)
            Next
            .Status = Me.Status
        End With
        Return result
    End Function
End Class

Public Enum SkillTreeItemStatus As Byte
    Locked = 0
    CanLearn = 1
    Acquired = 2
End Enum
