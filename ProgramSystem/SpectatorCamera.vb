Imports SharpDX
Imports SharpDX.Direct2D1
Imports SharpDX.Direct3D

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
    Public Delegate Sub Draw(ByRef context As SharpDX.Direct2D1.DeviceContext, ByRef spectator As SpectatorCamera)

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
    <Obsolete("升级d2d1.1后不再使用，改用d2dContext（D2D.DeviceContext继承了RenderTarget）", True)>
    Private RT As WindowRenderTarget

    Private device As SharpDX.Direct3D11.Device1
    Private d3dContext As SharpDX.Direct3D11.DeviceContext1
    Private swapChain As DXGI.SwapChain1
    ''' <summary>
    ''' Direct2D 1.1 画布对象
    ''' </summary>
    Private d2dContext As SharpDX.Direct2D1.DeviceContext
    Private d2dTarget As SharpDX.Direct2D1.Bitmap1

    Public Sub InitializeDirect2d()
        '-------d2d1.0初始化方法---------
        'Dim factory As New Factory
        'Dim wrtp As New HwndRenderTargetProperties With {
        '    .Hwnd = Form1.Handle,
        '    .PixelSize = New Size2(Resolve.X, Resolve.Y)}
        'Dim rtp As New RenderTargetProperties With {
        '    .PixelFormat = New PixelFormat(DXGI.Format.B8G8R8A8_UNorm, AlphaMode.Ignore),
        '    .Type = RenderTargetType.Default}
        'RT = New WindowRenderTarget(factory, rtp, wrtp)
        '--------已改用d2d1.1--------
        Dim defaultDevice As SharpDX.Direct3D11.Device = New SharpDX.Direct3D11.Device(DriverType.Hardware, Direct3D11.DeviceCreationFlags.Debug Or Direct3D11.DeviceCreationFlags.BgraSupport)
        device = defaultDevice.QueryInterface(Of SharpDX.Direct3D11.Device1)()
        d3dContext = device.ImmediateContext.QueryInterface(Of SharpDX.Direct3D11.DeviceContext1)()

        Dim dxgiDevice2 As SharpDX.DXGI.Device2 = device.QueryInterface(Of SharpDX.DXGI.Device2)()
        Dim dxgiAdapter As SharpDX.DXGI.Adapter = dxgiDevice2.Adapter
        Dim dxgiFactory2 As SharpDX.DXGI.Factory2 = dxgiAdapter.GetParent(Of SharpDX.DXGI.Factory2)()

        Dim description As DXGI.SwapChainDescription1 = New DXGI.SwapChainDescription1()
        With description
            .Width = Resolve.X  '0 means to use automatic buffer sizing.
            .Height = Resolve.Y
            .Format = DXGI.Format.B8G8R8A8_UNorm  '32 bit RGBA color.
            .Stereo = False    'No stereo (3D) display.
            .SampleDescription = New DXGI.SampleDescription(1, 0)    'No multisampling.
            .Usage = DXGI.Usage.RenderTargetOutput     'Use the swap chain as a render target.
            .BufferCount = 2            'Enable double buffering to prevent flickering.
            .Scaling = DXGI.Scaling.Stretch        'No scaling.->stretch
            .SwapEffect = DXGI.SwapEffect.FlipSequential           'Flip between both buffers.
        End With
        '原来是这个： swapChain = dxgiFactory2.CreateSwapChainForCoreWindow(device, new ComObject(window), ref description, null);
        '现在变成如下语句：
        swapChain = New DXGI.SwapChain1(dxgiFactory2, device, Form1.Handle, description, Nothing)
        Dim d2dDevice As SharpDX.Direct2D1.Device = New SharpDX.Direct2D1.Device(dxgiDevice2)
        d2dContext = New SharpDX.Direct2D1.DeviceContext(d2dDevice, SharpDX.Direct2D1.DeviceContextOptions.None)
        'Dim Properties As BitmapProperties1 = New BitmapProperties1(New PixelFormat(SharpDX.DXGI.Format.B8G8R8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied), DisplayProperties.LogicalDpi, DisplayProperties.LogicalDpi, BitmapOptions.Target Or BitmapOptions.CannotDraw)
        Dim dpiX As Single, dpiY As Single
        Using myGraphics As Graphics = Form1.CreateGraphics()
            dpiX = myGraphics.DpiX
            dpiY = myGraphics.DpiY
        End Using
        Dim Properties As BitmapProperties1 = New BitmapProperties1(New PixelFormat(SharpDX.DXGI.Format.B8G8R8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied), dpiX, dpiY, BitmapOptions.Target Or BitmapOptions.CannotDraw)
        Dim backBuffer As DXGI.Surface = swapChain.GetBackBuffer(Of DXGI.Surface)(0)
        d2dTarget = New Bitmap1(d2dContext, backBuffer, Properties)



        Call GameResources.LoadResources(d2dContext)
    End Sub

    Public Sub PaintImage()
        If CBool(PaintingLayers.Count) Then
            d2dContext.Target = d2dTarget
            d2dContext.BeginDraw()
            For i = PaintingLayers.Count - 1 To 0 Step -1
                PaintingLayers(i).Invoke(d2dContext, Me)
            Next

            d2dContext.EndDraw()
            swapChain.Present(0, DXGI.PresentFlags.None)    '0 or 1, I don't know
        End If
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        'RT.Dispose()
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
