Imports System.Text.RegularExpressions

Public Class AttackType
    Public Property DamageType As UnitDamageType
    Private AttackValue As PointF2M
    Public AttackRange As PointI
    Public HitRange As PointI

    Public Sub InitializeDamageType(input As String)
        Me.DamageType = [Enum].Parse(Me.DamageType.GetType, input)
    End Sub

    Public Sub InitializeAttackValue(input As String)
        Dim values() As String = Regex.Split(input, ",")
        Dim low As Single = CSng(values(0))
        Dim high As Single = CSng(values(1))
        If Me.AttackValue IsNot Nothing Then Throw New Exception("AtkValue has been initialized!")
        Me.AttackValue = New PointF2M(low, high)
    End Sub

    Public Sub InitializeAtkRange(input As String)
        Dim values() As String = Regex.Split(input, ",")
        Dim low As Short = CShort(values(0))
        Dim high As Short = CShort(values(1))
        Me.AttackRange = New PointI(low, high)
    End Sub

    Public Sub InitializeHitRange(input As String)
        Dim values() As String = Regex.Split(input, ",")
        Dim low As Short = CShort(values(0))
        Dim high As Short = CShort(values(1))
        Me.HitRange = New PointI(low, high)
    End Sub

End Class

Public Enum UnitDamageType As Byte
    Physical = 0
    Danmaku = 1
    Laser = 2
End Enum