''' <summary>
''' 技能效果类
''' </summary>
Public MustInherit Class SkillEffect
    ''' <summary>
    ''' 生效英雄id
    ''' </summary>
    Protected MasterHero As Short
    ''' <summary>
    ''' 生效势力
    ''' </summary>
    Protected MasterSide As EffectMasterSide

    Protected Property ParentSkillName As String
    ''' <summary>
    ''' 效果生效委托
    ''' </summary>
    ''' <param name="sender">效果施放者</param>
    ''' <param name="target">效果接受者</param>
    Public Delegate Sub EffectActivate(sender As GameUnit, target As List(Of GameUnit))
    ''' <summary>
    ''' 效果生效方法
    ''' </summary>
    Protected EffectActivateFunction As EffectActivate
    ''' <summary>
    ''' 根据描述生成方法
    ''' </summary>
    Public MustOverride Sub ConvertDescriptionToFunction()


End Class

Public Enum EffectMasterSide As Byte
    Self = 0
    SelfSide = 1
    AlliedSide = 2
    EnemySide = 3
    All = 4
End Enum
