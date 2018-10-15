Public Class IntegerProperty
    Inherits MonitoredProperty

    Private Value As Integer = 0

    Public Sub New(input As Integer)
        Me.RegisterId()
        LoggingService.Log(LogSenderType.Initialize, Me.GetId, LogMessageType.IntegerValue, input)
        Value = input
    End Sub

    Public Sub SetValue(input As Integer, method As LogSenderType)
        If method = LogSenderType.Initialize Then Throw New Exception("rejected")
        LoggingService.Log(method, Me.GetId, LogMessageType.IntegerValue, input)
        Value = input
    End Sub

    Public Function GetValue() As Integer
        Dim result As Integer = Me.Value
        If Not CBool(LoggingService.Match(Me.GetId, Value)) Then
            result = CInt(LoggingService.GetValueFromRecord(Me.GetId))
        End If
        Return result
    End Function

End Class
