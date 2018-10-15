Public Class SingleProperty
    Inherits MonitoredProperty

    Private Value As Single

    Public Sub New(input As Single)
        Me.RegisterId()
        LoggingService.Log(LogSenderType.Initialize, Me.GetId, LogMessageType.SingleValue, input)
        Value = input
    End Sub

    Public Sub SetValue(input As Single， method As LogSenderType)
        If method = LogSenderType.Initialize Then Throw New Exception("rejected")
        LoggingService.Log(method, Me.GetId, LogMessageType.SingleValue, input)
        Value = input
    End Sub

    Public Function GetValue() As Single
        Dim result As Single = Me.Value
        If Not CBool(LoggingService.Match(Me.GetId, Value)) Then
            result = CSng(LoggingService.GetValueFromRecord(Me.GetId))
        End If
        Return result
    End Function

End Class
