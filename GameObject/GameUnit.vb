
Imports System.Text.RegularExpressions
''' <summary>
''' 行动单位类
''' </summary>
Public Class GameUnit
    ''' <summary>
    ''' 一场游戏内单位唯一id，游戏外则为-1
    ''' </summary>
    Protected Property UnitId As Short = -1
    ''' <summary>
    ''' 存储模板对应id
    ''' </summary>
    ''' <returns></returns>
    Protected Property WrappedTemplateId As Short = -1
    ''' <summary>
    ''' 显示的名字
    ''' </summary>
    Public Property ShownName As String
    ''' <summary>
    ''' 字段
    ''' </summary>
    Protected UnitType As New List(Of GameUnitType)
    ''' <summary>
    ''' 单位所处位置
    ''' </summary>
    Protected Property Position As PointI3
    ''' <summary>
    ''' 移动单位后的临时位置
    ''' </summary>
    Protected Property PositionTmpMove As PointI3
    ''' <summary>
    ''' HP
    ''' </summary>
    Protected FullHP As IntegerProperty

    Protected RemainHP As IntegerProperty
    ''' <summary>
    ''' 行动类型
    ''' </summary>
    Protected Property MovementType As UnitMoveMentType

    Protected Property MovePoint As IntegerProperty
    ''' <summary>
    ''' 进攻点数
    ''' </summary>
    Protected AttackPoint As AttackType
    ''' <summary>
    ''' 第二攻击形式
    ''' </summary>
    Protected SubAttackPoint As AttackType
    ''' <summary>
    ''' 防守点数
    ''' </summary>
    Protected DefendPoint As DefendType
    ''' <summary>
    ''' 士气
    ''' </summary>
    Protected Spirit As SingleProperty = New SingleProperty(100)
    ''' <summary>
    ''' 
    ''' </summary>
    Protected SpiritStatus As TachieStatus = TachieStatus.Fine
    ''' <summary>
    ''' 状态
    ''' </summary>
    Protected Property Status As UnitStatus
    ''' <summary>
    ''' 此单位归属于
    ''' </summary>
    Protected Property Player As Integer

    Protected BuffList As New List(Of GameUnitBuff)

    Protected UnitBurden As IntegerProperty

    Protected View As SingleProperty

    Protected Hide As SingleProperty

    Protected SkirmishChessImageIndex As Integer


    Public Sub InitializeUnitType(input As String)
        Me.UnitType.Clear()
        Dim enumType As GameUnitType = 0
        Dim types() As String = Regex.Split(input, ",")
        For Each item As String In types
            Me.UnitType.Add([Enum].Parse(enumType.GetType, item))
        Next
    End Sub

    Public Sub InitializeHP(value As Short)
        If Me.FullHP IsNot Nothing Then Throw New Exception("HP has been initialized!")
        Me.FullHP = New IntegerProperty(value)
        If Me.RemainHP IsNot Nothing Then Throw New Exception("RemainHP has been initialized!")
        Me.RemainHP = New IntegerProperty(value)
    End Sub

    Public Sub InitializeMove(input As String)
        Dim value() As String = Regex.Split(input, ",")
        With Me
            .MovementType = [Enum].Parse(Me.MovementType.GetType, value(0))
            If (.MovePoint IsNot Nothing) Then Throw New Exception("movepoint has been initialized!")
            .MovePoint = New IntegerProperty(CInt(value(1)))
        End With
    End Sub

    Public Sub InitializeAttackPoint(atk As AttackType)
        If Me.AttackPoint IsNot Nothing Then Throw New Exception("atk has been initialized!")
        Me.AttackPoint = atk
    End Sub

    Public Sub InitializeSubAttack(subatk As AttackType)
        If subatk Is Nothing Then
            Me.SubAttackPoint = Nothing
        Else
            If Me.SubAttackPoint IsNot Nothing Then Throw New Exception("subatk has been initialized!")
            Me.SubAttackPoint = subatk
        End If
    End Sub

    Public Sub InitializeDefendPoint(def As DefendType)
        If Me.DefendPoint IsNot Nothing Then Throw New Exception("atk has been initialized!")
        Me.DefendPoint = def
    End Sub

    Public Sub InitializeBurden(value As Short)
        If Me.UnitBurden IsNot Nothing Then Throw New Exception("burden has been initialized!")
        Me.UnitBurden = New IntegerProperty(value)
    End Sub

    Public Sub InitializeView(value As Short)
        If Me.View IsNot Nothing Then Throw New Exception("view has been initialized!")
        Me.View = New SingleProperty(value)
    End Sub

    Public Sub InitializeHide(value As Short)
        If Me.Hide IsNot Nothing Then Throw New Exception("hide has been initialized!")
        Me.Hide = New SingleProperty(value)
    End Sub

    Public Sub InitializeUnitId(skirmishLoadingIndex As Short)
        Me.UnitId = skirmishLoadingIndex
    End Sub

    Public Sub SetTemplateId(value As Short)
        If Me.WrappedTemplateId = -1 Then
            Me.WrappedTemplateId = value
        Else
            Throw New Exception("illegal template")
        End If
    End Sub

    Public Function GetTemplateId() As Short
        Return Me.WrappedTemplateId
    End Function

    Public Sub SetBindingSkirmishChessImage(inputImageIndex As Integer)
        Me.SkirmishChessImageIndex = inputImageIndex
    End Sub

    Public Sub SetUnitPosition(newPosition As PointI3)
        Me.Position = newPosition
    End Sub

    Public Sub AddBuff(buff As GameUnitBuff)

    End Sub

    Public Function GetSideColorSet() As SolidColorBrushSet
        Return SIDE_COLOUR(Me.Player)
    End Function

    Public Function GetSkirmishChessImage() As IGameImage
        Return UnitImages.GetChessImage(Me.SkirmishChessImageIndex, Me.SpiritStatus)
    End Function

    Public Overridable Function GlobalSaveUnit() As String
        Dim result As String = ""
        result = "{" & Me.WrappedTemplateId & COMMA

        '...

    End Function

    Public Function Copy() As GameUnit

    End Function

End Class

Public Enum UnitMoveMentType As Byte
    LandBase = 0
    WaterBase = 1
    LandAndWater = 2
    SkyBase = 3
    LandSky = 4
    WaterSky = 5

End Enum

Public Enum UnitStatus As Byte
    NotAvailable = 0
    Waiting = 1
    Acting = 2
    Finished = 3

End Enum

''' <summary>
''' 单位类型字段枚举
''' </summary>
Public Enum GameUnitType As Byte
    ''' <summary>
    ''' 毛玉
    ''' </summary>
    Kedama = 0
    ''' <summary>
    ''' 妖精
    ''' </summary>
    Yousei = 1
    ''' <summary>
    ''' 妖怪
    ''' </summary>
    Youkai = 2
    ''' <summary>
    ''' 人类
    ''' </summary>
    Human = 3
    ''' <summary>
    ''' 吸血鬼
    ''' </summary>
    Vampire = 4

    ''' <summary>
    ''' 博丽灵梦
    ''' </summary>
    Reimu = 101
    ''' <summary>
    ''' 蕾米莉亚
    ''' </summary>
    Remilia = 102

End Enum