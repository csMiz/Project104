''' <summary>
''' 英雄技能类
''' </summary>
Public MustInherit Class HeroSkill
    ''' <summary>
    ''' 技能id
    ''' </summary>
    Public Property Id As Integer
    ''' <summary>
    ''' 技能名称
    ''' </summary>
    Public Property Name As String
    ''' <summary>
    ''' 技能描述
    ''' </summary>
    Public Property Description As String
    ''' <summary>
    ''' 技能类型
    ''' </summary>
    Public Property SkillType As HeroSkillType
    ''' <summary>
    ''' 技能效果
    ''' </summary>
    Protected Effects As New List(Of SkillEffect)

    ''' <summary>
    ''' 触发buff或使用符卡
    ''' </summary>
    Public MustOverride Sub Activate()


End Class

Public Enum HeroSkillType As Byte
    Buff = 0
    SC = 1
    TriggerSC = 2
End Enum
