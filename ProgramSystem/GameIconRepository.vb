Imports Sharpdx.Direct2d1

''' <summary>
''' Icon仓库类
''' </summary>
Public Class GameIconRepository
    Private Shared me_instance As GameIconRepository = Nothing

    Public Icons As New List(Of Bitmap)

    Private Sub New()
    End Sub

    ''' <summary>
    ''' 单例
    ''' </summary>
    Public Shared Function Instance() As GameIconRepository
        If me_instance Is Nothing Then me_instance = New GameIconRepository
        Return me_instance
    End Function

    ''' <summary>
    ''' 从程序根目录下加载资源
    ''' </summary>
    Public Sub LoadFromFiles(rt As RenderTarget)

    End Sub

End Class
