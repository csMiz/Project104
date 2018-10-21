
Imports System.Xml
''' <summary>
''' 单位模板仓库类
''' </summary>
Public Class UnitTemplateRepository
    Private Shared me_instance As UnitTemplateRepository = Nothing

    Private unit_templates As New List(Of GameUnit)
    Private hero_templates As New List(Of GameHero)

    'Private wrapped_unit_templates As New List(Of GameUnit)
    Private wrapped_hero_templates As New List(Of GameHero)

    Private Sub New()
    End Sub

    ''' <summary>
    ''' 单例模式
    ''' </summary>
    Public Shared Function Instance()
        If me_instance Is Nothing Then
            me_instance = New UnitTemplateRepository
        End If
        Return me_instance
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
                    .InitializeView(CSng(element.GetAttribute("view")))
                    .InitializeHide(CSng(element.GetAttribute("hide")))
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
                    .InitializeView(CSng(element.GetAttribute("view")))
                    .InitializeHide(CSng(element.GetAttribute("hide")))
                    .InitializeBaseEXPNeeded(CInt(element.GetAttribute("lvupexp")))
                    .InitializeAttackPoint(atk)
                    .InitializeSubAttack(subatk)
                    .InitializeDefendPoint(def)
                    .InitializeLVUpItems(lvupItemList)
                    .InitializeSkillTree(New HeroSkillTree)
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

    Public Sub WrapUnits(xml As String)
        Dim xmlDoc As New XmlDocument()
        xmlDoc.LoadXml(xml)
        Dim root As XmlNode = xmlDoc.SelectSingleNode("content")
        Dim xnl As XmlNodeList = root.ChildNodes
        For Each item As XmlNode In xnl
            Dim element As XmlElement = CType(item, XmlElement)
            If element.Name = "h" Then
                Dim tmpHero As GameHero = Nothing
                tmpHero = Me.GetHeroTemplate(element.GetAttribute("template")).Copy
                tmpHero.SetTemplateId(element.GetAttribute("template"))
                tmpHero.SetBindingSkirmishChessImage(CInt(element.GetAttribute("chessImage")))
                Dim children As XmlNodeList = element.ChildNodes
                For Each child As XmlNode In children
                    Dim childElement As XmlElement = CType(child, XmlElement)
                    If childElement.Name = "level" Then
                        tmpHero.SetLevel(childElement.GetAttribute("value"), LogSenderType.Change_Load)
                    ElseIf childElement.Name = "lock" Then
                        Dim tmpLockStatus As HeroLockStatus = [Enum].Parse(tmpLockStatus.GetType, childElement.GetAttribute("value"))
                        tmpHero.SetLockStatus(tmpLockStatus)
                    End If
                Next
                wrapped_hero_templates.Add(tmpHero)

            End If
        Next

    End Sub

    Public Function GetWrappedHeroTemplate(index As Short) As GameHero
        Return wrapped_hero_templates(index)
    End Function



End Class
