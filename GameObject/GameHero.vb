
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
    Protected Level As IntegerProperty = New IntegerProperty(1)
    ''' <summary>
    ''' 经验
    ''' </summary>
    Protected EXP As IntegerProperty = New IntegerProperty(0)
    ''' <summary>
    ''' 基础升级所需经验：EXPNeeded = Level * BaseEXPNeeded
    ''' </summary>
    Protected BaseEXPNeeded As IntegerProperty
    ''' <summary>
    ''' 升级影响参数
    ''' </summary>
    Protected LevelUpItems As List(Of LevelUpItem)
    ''' <summary>
    ''' 技能点
    ''' </summary>
    Protected SkillPoint As IntegerProperty = New IntegerProperty(0)
    ''' <summary>
    ''' 技能树
    ''' </summary>
    Protected SkillTree As HeroSkillTree
    ''' <summary>
    ''' 已选中的技能
    ''' </summary>
    Protected MySkills(2) As HeroSkill
    ''' <summary>
    ''' 装备栏
    ''' </summary>
    Protected MyEquipments(2) As GameItem
    ''' <summary>
    ''' 是否已解锁
    ''' </summary>
    Protected LockStatus As HeroLockStatus = HeroLockStatus.Locked

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

    Public Sub InitializeSkillTree(tree As HeroSkillTree)
        If Me.SkillTree IsNot Nothing Then Throw New Exception("skilltree has been initialized!")
        Me.SkillTree = tree
    End Sub

    Public Overrides Function GlobalSaveUnit() As String
        Dim result As String = ""
        result = "{" & Me.WrappedTemplateId & COMMA
        result = result & Me.Level.GetValue & COMMA
        result = result & Me.EXP.GetValue & COMMA
        result = result & Me.FullHP.GetValue & COMMA
        result = result & Me.MovementType & COMMA
        result = result & Me.MovePoint.GetValue & COMMA
        result = result & Me.AttackPoint.GetSaveString & COMMA
        result = result & Me.SubAttackPoint.GetSaveString & COMMA
        result = result & Me.DefendPoint.GetSaveString & COMMA
        result = result & Me.Spirit.GetValue & COMMA
        result = result & Me.UnitBurden.GetValue & COMMA
        result = result & Me.View.GetValue & COMMA
        result = result & Me.Hide.GetValue & COMMA
        result = result & Me.SkillPoint.GetValue & COMMA
        result = result & Me.SkillTree.GetSaveString & COMMA
        For i = 0 To 2
            result = result & Me.MySkills(i).GetSaveString & COMMA
        Next
        For i = 0 To 2
            result = result & Me.MyEquipments(i).GetSaveString & COMMA
        Next
        result = result & "}"

        Return result
    End Function

    Public Overloads Function Copy() As GameHero
        Dim tmpHero As New GameHero
        With tmpHero
            .UnitId = -1
            .SetTemplateId(Me.WrappedTemplateId)
            .ShownName = Me.ShownName
            For Each ut As GameUnitType In Me.UnitType
                .UnitType.Add(ut)
            Next
            .InitializeHP(Me.FullHP.GetValue)
            .MovementType = Me.MovementType
            .MovePoint = New IntegerProperty(Me.MovePoint.GetValue)
            .AttackPoint = Me.AttackPoint.Copy
            .SubAttackPoint = Me.SubAttackPoint.Copy
            .DefendPoint = Me.DefendPoint.Copy
            .Spirit = New SingleProperty(Me.Spirit.GetValue)
            .UnitBurden = New IntegerProperty(Me.UnitBurden.GetValue)
            .View = New SingleProperty(Me.View.GetValue)
            .Hide = New SingleProperty(Me.Hide.GetValue)

            .Description = Me.Description
            .SetLevel(Me.Level.GetValue, LogSenderType.Change_Load)
            .SetEXP(Me.EXP.GetValue, LogSenderType.Change_Load)
            .InitializeBaseEXPNeeded(Me.BaseEXPNeeded.GetValue)
            .LevelUpItems = New List(Of LevelUpItem)
            For Each lui As LevelUpItem In Me.LevelUpItems
                .LevelUpItems.Add(lui.copy)
            Next
            .SetSkillPoint(Me.GetSkillPoint, LogSenderType.Change_Load)
            .SkillTree = Me.SkillTree.Copy
            For i = 0 To 2
                .MySkills(i) = Me.MySkills(i)       '复制的技能是单例
                If .MyEquipments(i) IsNot Nothing Then
                    .MyEquipments(i) = Me.MyEquipments(i).Copy
                End If
            Next
        End With
        Return tmpHero
    End Function

    Public Sub SetLevel(value As Short, method As LogSenderType)
        Me.Level.SetValue(value, method)
    End Sub

    Public Function GetLevel() As Short
        Return Me.Level.GetValue
    End Function

    Public Sub SetEXP(value As Integer, method As LogSenderType)
        Me.EXP.SetValue(value, method)
    End Sub

    Public Function GetEXP() As Integer
        Return Me.EXP.GetValue
    End Function

    Public Sub SetSkillPoint(value As Short, method As LogSenderType)
        Me.SkillPoint.SetValue(value, method)
    End Sub

    Public Function GetSkillPoint() As Short
        Return Me.SkillPoint.GetValue
    End Function

    Public Sub SetLockStatus(value As HeroLockStatus)
        Me.LockStatus = value
    End Sub

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

    Public Function Copy() As LevelUpItem
        Dim result As New LevelUpItem(Me.Target, Me.Pace.GetValue, Me.Rate.GetValue)
        Return result
    End Function
End Class

Public Enum HeroLockStatus As Byte
    Unlocked = 0
    Locked = 1

End Enum