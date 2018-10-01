Public Class SingleProperty
    Inherits MonitoredProperty

    Private Value As Single

    Public Sub New(input As Single)
        Me.RegisterId()
        LoggingService.Log(LogSenderType.Initialize, Me.GetId, LogMessageType.SingleValue, input)
        Value = input
    End Sub

    Public Sub SetValue(input As Single)
        LoggingService.Log(LogSenderType.Change, Me.GetId, LogMessageType.SingleValue, input)
        Value = input
    End Sub

    Public Function GetValue() As Single
        Return Value
    End Function

End Class
