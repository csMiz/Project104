
Imports System.Xml
''' <summary>
''' 单位模板仓库类
''' </summary>
Public Class UnitTemplateRepository
    Private Shared My_Instance As UnitTemplateRepository = Nothing

    Private unit_templates As New List(Of GameUnit)
    Private hero_templates As New List(Of GameHero)

    Private Sub New()
    End Sub

    ''' <summary>
    ''' 单例模式
    ''' </summary>
    Public Shared Function Instance()
        If My_Instance Is Nothing Then
            My_Instance = New UnitTemplateRepository
        End If
        Return My_Instance
    End Function

    ''' <summary>
    ''' 加载所有单位模板
    ''' </summary>
    Public Sub LoadTemplates(xmlContent As String)
        Dim xmlDoc As New XmlDocument()
        xmlDoc.LoadXml(xmlContent)
        Dim root As XmlNode = xmlDoc.SelectSingleNode("content")
        Dim xnl As XmlNodeList = root.ChildNodes
        For Each item As XmlNode In xnl
            Dim element As XmlElement = CType(item, XmlElement)

            Dim atk As New AttackType
            Dim subatk As AttackType = Nothing
            Dim def As New DefendType
            Dim elementChild As XmlNodeList = element.ChildNodes
            For Each ec As XmlNode In elementChild
                Dim ecItem As XmlElement = CType(ec, XmlElement)
                If ecItem.Name = "attack" OrElse ecItem.Name = "subatk" Then
                    Dim target As AttackType = Nothing
                    If ecItem.Name = "attack" Then
                        target = atk
                    Else
                        subatk = New AttackType
                        target = subatk
                    End If
                    With target
                        .InitializeDamageType(ecItem.GetAttribute("damage"))
                        .InitializeAttackValue(ecItem.GetAttribute("value"))
                        .InitializeAtkRange(ecItem.GetAttribute("atkrange"))
                        .InitializeHitRange(ecItem.GetAttribute("hitrange"))
                    End With
                ElseIf ecItem.Name = "defend" Then
                    With def
                        .InitializeDefValue(CShort(ecItem.GetAttribute("value")))
                        .InitializeResistance(ecItem.GetAttribute("resistance"))
                    End With
                End If
            Next

            If (element.Name = "ut") Then
                Dim unitTemplate As New GameUnit
                With unitTemplate
                    .ShownName = element.GetAttribute("name")
                    .InitializeUnitType(element.GetAttribute("unittype"))
                    .InitializeHP(CShort(element.GetAttribute("hp")))
                    .InitializeMove(element.GetAttribute("move"))
                    .InitializeBurden(CShort(element.GetAttribute("burden")))
                    .InitializeView(CShort(element.GetAttribute("view")))
                    .InitializeAttackPoint(atk)
                    .InitializeSubAttack(subatk)
                    .InitializeDefendPoint(def)
                End With
                unit_templates.Add(unitTemplate)
            ElseIf (element.Name = "ht") Then
                Dim lvupItemList As New List(Of LevelUpItem)
                Dim eChildHero As XmlNodeList = element.ChildNodes
                For Each ec As XmlNode In eChildHero
                    Dim ecItem As XmlElement = CType(ec, XmlElement)
                    If ecItem.Name = "lvup" Then
                        Dim targetProperty As String = ecItem.GetAttribute("item")
                        Dim addPace As Single = 1.0F
                        If ecItem.HasAttribute("pace") Then
                            addPace = CSng(ecItem.GetAttribute("pace"))
                        End If
                        Dim addRate As Single = CSng(ecItem.GetAttribute("rate"))
                        Dim lvup As New LevelUpItem(targetProperty, addPace, addRate)
                        lvupItemList.Add(lvup)
                    End If
                Next

                Dim heroTemplate As New GameHero
                With heroTemplate
                    .ShownName = element.GetAttribute("name")
                    .InitializeDescription(element.GetAttribute("description"))
                    .InitializeUnitType(element.GetAttribute("unittype"))
                    .InitializeHP(CShort(element.GetAttribute("hp")))
                    .InitializeMove(element.GetAttribute("move"))
                    .InitializeBurden(CShort(element.GetAttribute("burden")))
                    .InitializeView(CShort(element.GetAttribute("view")))
                    .InitializeBaseEXPNeeded(CInt(element.GetAttribute("lvupexp")))
                    .InitializeAttackPoint(atk)
                    .InitializeSubAttack(subatk)
                    .InitializeDefendPoint(def)
                    .InitializeLVUpItems(lvupItemList)
                End With
                hero_templates.Add(heroTemplate)
            End If
        Next

    End Sub

    ''' <summary>
    ''' 根据index获取普通单位模板
    ''' </summary>
    Public Function GetUnitTemplate(templateIndex As Short) As GameUnit
        Return unit_templates(templateIndex)
    End Function

    ''' <summary>
    ''' 根据index获取英雄模板
    ''' </summary>
    Public Function GetHeroTemplate(templateIndex As Short) As GameHero
        Return hero_templates(templateIndex)
    End Function

    '''' <summary>
    '''' 毛玉
    '''' </summary>
    'Public Class UnitL1
    '    Inherits GameUnit

    '    Public Sub New()
    '        With Me
    '            .UnitType.Add(GameUnitType.Kedama)
    '            .FullHP = New IntegerProperty(10)
    '            .MovementType = UnitMoveMentType.LandAndWater
    '            .AttackPoint = New AttackType With {
    '            .DamageType = UnitDamageType.Physical,
    '            .AttackValue = New PointF2M(5, 7),
    '            .AttackRange = New PointI(1, 1),
    '            .HitRange = New PointI(1, 1)}
    '            .DefendPoint = New DefendType With {
    '            .BaseDefend = New IntegerProperty(0),
    '            .Resistance = {New SingleProperty(0.3), New SingleProperty(0), New SingleProperty(0)}}
    '            .Status = UnitStatus.NotAvailable
    '        End With

    '    End Sub

    'End Class


End Class
