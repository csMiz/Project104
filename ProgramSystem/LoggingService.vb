Imports System.IO
''' <summary>
''' 监听服务类
''' </summary>
Public Class LoggingService
    'Private Shared Buffer As Stream
    Private Shared me_instance As LoggingService = New LoggingService

    Private Records As New List(Of List(Of GameRecord)) '可以通过覆盖优化
    Private RecordCursor As Short = 0
    Private Messages As New List(Of String)

    Private Sub New()
        Records.Add(New List(Of GameRecord))
    End Sub

    ''' <summary>
    ''' 记录一条日志
    ''' </summary>
    ''' <param name="senderType">操作类型</param>
    ''' <param name="senderId">属性Id</param>
    ''' <param name="message">内容类型</param>
    ''' <param name="value">值</param>
    Public Shared Sub Log(senderType As LogSenderType, senderId As Integer, message As LogMessageType, value As Object)
        With me_instance
            .RemoveRecord(senderId)
            .Records(.RecordCursor).Add(New GameRecord(senderType, senderId, message, value.ToString))
            If .Records(.RecordCursor).Count > THIRTY_THOUSAND Then
                .RecordCursor += 1
                .Records.Add(New List(Of GameRecord))
            End If

        End With
    End Sub


    Public Shared Function Match(senderId As Integer, inputValue As Object) As LogMatchResult

        'if do not match then log a message

    End Function

    Public Shared Function GetValueFromRecord(senderId As Integer) As String
        With me_instance
            For i = .Records.Count - 1 To 0 Step -1
                For j = .Records(i).Count - 1 To 0 Step -1
                    Dim recordObject As GameRecord = .Records(i)(j)
                    If recordObject.GetId = senderId Then
                        Return recordObject.GetStringValue
                    End If
                Next
            Next
        End With
        Throw New Exception("record not found!")
    End Function

    Private Sub RemoveRecord(senderId As Integer)
        With me_instance
            For i = .Records.Count - 1 To 0 Step -1
                For j = .Records(i).Count - 1 To 0 Step -1
                    Dim recordObject As GameRecord = .Records(i)(j)
                    If recordObject.GetId = senderId Then
                        .Records(i).RemoveAt(j)
                        Return
                    End If
                Next
            Next
        End With
    End Sub

    Public Shared Function GetRecordCount() As Integer
        Dim result As Integer = 0
        With me_instance
            For i = 0 To .Records.Count - 1
                result += .Records(i).Count
            Next
        End With
        Return result
    End Function

End Class

''' <summary>
''' 记录日志操作类型枚举
''' </summary>
Public Enum LogSenderType As Byte
    Initialize = 0
    Change = 1
End Enum

''' <summary>
''' 记录日志内容类型枚举
''' </summary>
Public Enum LogMessageType As Byte
    IntegerValue = 1
    SingleValue = 2
    StringValue = 3
End Enum

''' <summary>
''' 校验数值结果枚举
''' </summary>
Public Enum LogMatchResult As Byte
    ValueEqual = 1
    ValueNotEqual = 0
End Enum

''' <summary>
''' 数值记录类
''' </summary>
Public Class GameRecord
    Private Property RecordType As LogSenderType
    Private Property PropertyId As Integer
    Private Property RecordMessage As LogMessageType
    Private Property RecordValue As String

    Public Sub New(senderType As LogSenderType, senderId As Integer, message As LogMessageType, stringValue As String)
        RecordType = senderType
        PropertyId = senderId
        RecordMessage = message
        RecordValue = stringValue

    End Sub

    Public Function GetId() As Integer
        Return PropertyId
    End Function

    Public Function GetStringValue() As String
        Return RecordValue
    End Function

End Class
