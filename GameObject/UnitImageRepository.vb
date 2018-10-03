Imports Sharpdx.Direct2d1

''' <summary>
''' 单位图片资源仓库类
''' </summary>
Public Class UnitImageRepository
    Private Shared me_instance As UnitImageRepository = Nothing

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
    ''' 从程序根目录下加载资源
    ''' </summary>
    Public Sub LoadFromFiles(rt As RenderTarget)

    End Sub

    ''' <summary>
    ''' 获取英雄立绘组
    ''' </summary>
    Public Function GetHeroLeftPanelArtwork(heroId As Short) As LeftPanelImageSet
        Return DetailLeftPanel(heroId)
    End Function

    ''' <summary>
    ''' 获取普通单位立绘组
    ''' </summary>
    Public Function GetUnitLeftPanelArtwork(unitId As Short) As LeftPanelImageSet
        Return DetailLeftPanel(unitId + 256)
    End Function

End Class

''' <summary>
''' 立绘图片组类
''' </summary>
Public Class LeftPanelImageSet
    Public Fine As GameLive2dImage
    Public Normal As GameLive2dImage
    Public Injured As GameLive2dImage

End Class
