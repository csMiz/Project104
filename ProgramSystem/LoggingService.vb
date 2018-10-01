Imports System.IO
''' <summary>
''' 对程序内各属性的监听服务类
''' </summary>
Public Class LoggingService
    Private Shared Buffer As Stream


    ''' <summary>
    ''' 记录一条日志
    ''' </summary>
    ''' <param name="senderType">操作类型</param>
    ''' <param name="senderId">属性Id</param>
    ''' <param name="message">内容类型</param>
    ''' <param name="value">值</param>
    Public Shared Sub Log(senderType As LogSenderType, senderId As Integer, message As LogMessageType, value As Object)

    End Sub



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
