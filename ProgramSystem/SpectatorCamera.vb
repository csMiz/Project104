Imports SlimDX
Imports SlimDX.Direct2D

''' <summary>
''' 观察者类
''' </summary>
Public Class SpectatorCamera
    Implements IDisposable
    ''' <summary>
    ''' 视图中心
    ''' </summary>
    Public CameraFocus As PointF2
    ''' <summary>
    ''' 缩放倍率，大于零并且小于等于1
    ''' </summary>
    Public Zoom As Single = 1.0F
    ''' <summary>
    ''' 分辨率
    ''' </summary>
    Public Resolve As PointI
    ''' <summary>
    ''' 定义绘图委托
    ''' </summary>
    Public Delegate Sub Draw(ByRef renderTarget As WindowRenderTarget, ByRef spectator As SpectatorCamera)

    ''' <summary>
    ''' 分层绘图，不考虑小窗口穿透的模式
    ''' </summary>
    Public PaintingLayers As New Stack(Of Draw)
    ''' <summary>
    ''' 图层说明，用于鼠标点击判定
    ''' </summary>
    Public PaintingLayersDescription As New Stack(Of GameImageLayer)
    ''' <summary>
    ''' d2d画布对象
    ''' </summary>
    Private RT As WindowRenderTarget

    Public Sub InitializeDirect2d()
        Dim factory As New Factory
        Dim wrtp As New WindowRenderTargetProperties With {
            .Handle = Form1.Handle,
            .PixelSize = New Size(Resolve.X, Resolve.Y)}
        Dim rtp As New RenderTargetProperties With {
            .PixelFormat = New PixelFormat(DXGI.Format.B8G8R8A8_UNorm, AlphaMode.Ignore),
            .Type = RenderTargetType.Default}
        RT = New WindowRenderTarget(factory, rtp, wrtp)

        Call GameResources.LoadResources(RT)
    End Sub

    Public Sub PaintImage()
        If CBool(PaintingLayers.Count) Then
            RT.BeginDraw()
            For i = PaintingLayers.Count - 1 To 0 Step -1
                PaintingLayers(i).Invoke(RT, Me)
            Next
            RT.EndDraw()
        End If
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        RT.Dispose()
    End Sub
End Class

''' <summary>
''' 图层分类枚举
''' </summary>
Public Enum GameImageLayer As Byte
    BaseScreen = 0

    SplashScreen = 1

    MainMenu = 2

    Loading = 16

    WorldMap = 21

    SkirmishMap = 32
    Skirmish_UnitDetail = 33

    BattleAnimation = 64
End Enum
