Imports Sharpdx.Direct2d1

''' <summary>
''' 单位图片资源仓库类
''' </summary>
Public Class UnitImageRepository
    Private Shared me_instance As UnitImageRepository = Nothing
    Private FragmentImages As BasicImageRepository = BasicImageRepository.Instance

    ''' <summary>
    ''' 详情面板左侧立绘
    ''' </summary>
    Private DetailLeftPanel As New List(Of LeftPanelImageSet)



    Private Sub New()
    End Sub

    ''' <summary>
    ''' 单例
    ''' </summary>
    Public Shared Function Instance() As UnitImageRepository
        If me_instance Is Nothing Then me_instance = New UnitImageRepository
        Return me_instance
    End Function

    ''' <summary>
    ''' 从xml读取数据并依此组装FragmentImage为IGameImage
    ''' </summary>
    Public Sub LoadFromFiles(context As SharpDX.Direct2D1.DeviceContext)


    End Sub

    ''' <summary>
    ''' 获取单位立绘组
    ''' </summary>
    Public Function GetUnitLeftPanelArtwork(unitId As Short) As LeftPanelImageSet
        Return DetailLeftPanel(unitId)
    End Function

End Class

''' <summary>
''' 立绘图片组类
''' </summary>
Public Class LeftPanelImageSet
    Public Fine As New List(Of IGameImage)
    Public Normal As New List(Of IGameImage)
    Public Injured As New List(Of IGameImage)

End Class
