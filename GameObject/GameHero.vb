
''' <summary>
''' 英雄类
''' </summary>
Public Class GameHero
    Inherits GameUnit
    ''' <summary>
    ''' 描述
    ''' </summary>
    Protected Property Description As String
    ''' <summary>
    ''' 等级
    ''' </summary>
    Protected Level As IntegerProperty
    ''' <summary>
    ''' 经验
    ''' </summary>
    Protected EXP As IntegerProperty
    ''' <summary>
    ''' 基础升级所需经验：EXPNeeded = Level * BaseEXPNeeded
    ''' </summary>
    Protected BaseEXPNeeded As IntegerProperty

    Protected LevelUpItems As List(Of LevelUpItem)
    ''' <summary>
    ''' 技能点
    ''' </summary>
    Protected SkillPoint As IntegerProperty
    ''' <summary>
    ''' 技能树
    ''' </summary>
    Protected SkillTree As HeroSkillTree
    ''' <summary>
    ''' 已习得的技能
    ''' </summary>
    Protected MySkills As List(Of HeroSkill)
    ''' <summary>
    ''' 装备栏
    ''' </summary>
    Protected MyEquipments As List(Of GameItem)

    Public Sub InitializeDescription(input As String)
        Me.Description = input
    End Sub

    Public Sub InitializeBaseEXPNeeded(value As Integer)
        If Me.BaseEXPNeeded IsNot Nothing Then Throw New Exception("baseexpneeded has been initialized!")
        Me.BaseEXPNeeded = New IntegerProperty(value)
    End Sub

    Public Sub InitializeLVUpItems(list As List(Of LevelUpItem))
        If Me.LevelUpItems IsNot Nothing Then Throw New Exception("lvupitem has been initialized!")
        Me.LevelUpItems = list
    End Sub

    Public Overrides Function GlobalSaveUnit() As String
        Dim result As String = ""
        result = "{" & Me.TemplateId & COMMA
        result = result & Me.Level.GetValue & COMMA
        result = result & Me.EXP.GetValue & COMMA
        result = result & Me.FullHP.GetValue & COMMA
        result = result & Me.MovementType & COMMA

        '...

    End Function

End Class

Public Class LevelUpItem
    Public Target As String
    Public Pace As SingleProperty
    Public Rate As SingleProperty

    Public Sub New(item As String, inputPace As Single, inputRate As Single)
        With Me
            .Target = item
            .Pace = New SingleProperty(inputPace)
            .Rate = New SingleProperty(inputRate)
        End With
    End Sub
End Class