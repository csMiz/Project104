Imports System.IO
Imports System.Text.RegularExpressions
''' <summary>
''' 游戏世界大地图的地图类
''' </summary>
Public Class GameMap
    Private Blocks As New List(Of MapBlock)

    ''' <summary>
    ''' 加载游戏大地图
    ''' </summary>
    Public Sub LoadFromFile(file As FileStream)

    End Sub

    ''' <summary>
    ''' 搜索通路
    ''' </summary>
    Public Function GetEdges(node As Integer) As List(Of Integer)
        Return Blocks(node).Edge
    End Function


End Class
