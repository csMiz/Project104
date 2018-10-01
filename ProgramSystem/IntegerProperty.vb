Public Class IntegerProperty
    Inherits MonitoredProperty

    Private Value As Integer = 0

    Public Sub New(input As Integer)
        Me.RegisterId()
        LoggingService.Log(LogSenderType.Initialize, Me.GetId, LogMessageType.IntegerValue, input)
        Value = input
    End Sub

    Public Sub SetValue(input As Integer)
        LoggingService.Log(LogSenderType.Change, Me.GetId, LogMessageType.IntegerValue, input)
        Value = input
    End Sub

    Public Function GetValue() As Integer
        Return Value
    End Function

End Class
