''' <summary>
''' 受到监视的属性类
''' </summary>
Public MustInherit Class MonitoredProperty
    Private Shared ObjectCounter As Integer = 0

    Private Id As Integer

    Protected Sub RegisterId()
        MonitoredProperty.ObjectCounter += 1
        Me.Id = MonitoredProperty.ObjectCounter
    End Sub

    Public Function GetId() As Integer
        Return Id
    End Function


End Class
