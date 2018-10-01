Imports System.Text.RegularExpressions

Public Class DefendType
    Public Property BaseDefend As IntegerProperty
    ''' <summary>
    ''' 分别对应物理抗性，弹幕抗性，镭射抗性
    ''' </summary>
    Public Resistance(2) As SingleProperty

    Public Sub InitializeDefValue(value As Short)
        If BaseDefend IsNot Nothing Then Throw New Exception("DefValue has been initialized")
        BaseDefend = New IntegerProperty(value)
    End Sub

    Public Sub InitializeResistance(input As String)
        Dim values() As String = Regex.Split(input, ",")
        Dim physics As Single = CSng(values(0))
        Dim danmaku As Single = CSng(values(1))
        Dim laser As Single = CSng(values(2))
        If Me.Resistance(0) IsNot Nothing Then Throw New Exception("resistance0 has been initialized!")
        Me.Resistance(0) = New SingleProperty(physics)
        If Me.Resistance(1) IsNot Nothing Then Throw New Exception("resistance1 has been initialized!")
        Me.Resistance(1) = New SingleProperty(danmaku)
        If Me.Resistance(2) IsNot Nothing Then Throw New Exception("resistance2 has been initialized!")
        Me.Resistance(2) = New SingleProperty(laser)
    End Sub

End Class
