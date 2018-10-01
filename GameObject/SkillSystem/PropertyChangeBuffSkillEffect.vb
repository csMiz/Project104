''' <summary>
''' 属性改变Buff技能效果类
''' </summary>
Public Class PropertyChangeBuffSkillEffect
    Inherits BuffSkillEffect
    ''' <summary>
    ''' 对象属性名称
    ''' </summary>
    Protected HandlePropertyName As String
    ''' <summary>
    ''' 改动值
    ''' </summary>
    Protected EffectValueText As String

    Public Overrides Sub ConvertDescriptionToFunction()
        Dim effectFunction As EffectActivate = Nothing
        If HandlePropertyName = "move" Then
            effectFunction = Sub(sender As GameUnit, target As List(Of GameUnit))
                                 Dim buff As New GameUnitBuff(Me.ParentSkillName, AllGameStages, "行动力上升")
                                 Dim paramList As New List(Of String) From {
                                     EffectValueText
                                 }
                                 buff.AddEffect(New KeyValuePair(Of String, List(Of String))("move", paramList))
                                 sender.AddBuff(buff)
                             End Sub
        ElseIf HandlePropertyName = "movetype" Then
            effectFunction = Sub(sender As GameUnit, target As List(Of GameUnit))
                                 Dim buff As New GameUnitBuff(Me.ParentSkillName, AllGameStages, "行动方式改为<ps>")   '<ps>表示参数字符串，<pv>表示参数值
                                 Dim paramList As New List(Of String)
                                 paramList.Add(EffectValueText)
                                 buff.AddEffect(New KeyValuePair(Of String, List(Of String))("move", paramList))
                                 sender.AddBuff(buff)
                             End Sub

        End If
        Me.EffectActivateFunction = effectFunction

    End Sub
End Class
