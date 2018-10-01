Public Class GameUnitBuff
    ''' <summary>
    ''' 效果生效委托
    ''' </summary>
    ''' <param name="sender">效果施放者</param>
    ''' <param name="target">效果接受者</param>
    Public Delegate Sub EffectActivate(sender As GameUnit, target As List(Of GameUnit))

    Public Property BuffUniqueName As String
    ''' <summary>
    ''' kvp(对象，参数)
    ''' </summary>
    Private Effects As New List(Of KeyValuePair(Of String, List(Of String)))

    Public Property BuffDescription As String

    ''' <summary>
    ''' 生效的阶段
    ''' </summary>
    Public ActiveStage As List(Of SingleGameLoopStage)

    Public Sub New(uniqueName As String, stages As List(Of SingleGameLoopStage), description As String)
        With Me
            .BuffUniqueName = uniqueName
            .BuffDescription = description
            .ActiveStage = stages
        End With
    End Sub

    Public Sub AddEffect(effect As KeyValuePair(Of String, List(Of String)))
        Me.Effects.Add(effect)
    End Sub

    Public Sub ActivateBuff(sender As GameUnit, target As List(Of GameUnit))


    End Sub

End Class
