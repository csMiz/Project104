Imports System.Text.RegularExpressions
Imports p104

Public Class AttackType
    Implements ISaveProperty

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

    Public Function GetSaveString() As String Implements ISaveProperty.GetSaveString
        Throw New NotImplementedException()
    End Function

    Public Function Copy() As AttackType
        Dim result As New AttackType
        With result
            .DamageType = Me.DamageType
            .AttackValue = New PointF2M(Me.AttackValue.X.GetValue, Me.AttackValue.Y.GetValue)
            .AttackRange = New PointI(Me.AttackRange.X, Me.AttackRange.Y)
            .HitRange = New PointI(Me.HitRange.X, Me.HitRange.Y)
        End With
        Return result
    End Function

End Class

Public Enum UnitDamageType As Byte
    Physical = 0
    Danmaku = 1
    Laser = 2
End Enum