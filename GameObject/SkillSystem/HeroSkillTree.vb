''' <summary>
''' 英雄技能树类
''' </summary>
Public Class HeroSkillTree
    Public BaseSkills As List(Of HeroSkillTreeItem)

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

End Class

Public Enum SkillTreeItemStatus As Byte
    Locked = 0
    CanLearn = 1
    Acquired = 2
End Enum
