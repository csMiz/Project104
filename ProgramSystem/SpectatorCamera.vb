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
    Public ReadOnly Property CameraTopLeft As PointF2
        Get
            Return New PointF2(CameraFocus.X - 0.5 * Resolve.X / Zoom, CameraFocus.Y - 0.5 * Resolve.Y / Zoom)
        End Get
    End Property
    ''' <summary>
    ''' 缩放倍率，大于零并且小于等于1
    ''' </summary>
    Public Zoom As Single = 1.0F
    ''' <summary>
    ''' 分辨率
    ''' </summary>
    Public Resolve As PointI = Nothing
    Public ReadOnly Property ResolveRectangle As Mathematics.Interop.RawRectangleF
        Get
            Return New Mathematics.Interop.RawRectangleF(0, 0, Me.Resolve.X, Me.Resolve.Y)
        End Get
    End Property
    ''' <summary>
    ''' 定义绘图委托，仅用于D2D
    ''' </summary>
    Public Delegate Sub Draw(ByRef context As SharpDX.Direct2D1.DeviceContext, ByRef spectator As SpectatorCamera, canvasBitmap As Bitmap1)
    ''' <summary>
    ''' 分层绘图，不考虑小窗口穿透的模式，仅用于D2D
    ''' </summary>
    Public PaintingLayers As New Stack(Of Draw)
    ''' <summary>
    ''' 图层说明，仅用于D2D
    ''' </summary>
    Public PaintingLayersDescription As New Stack(Of GameImageLayer)
    ''' <summary>
    ''' 图层控件页，用于鼠标点击判定，可以不与绘图图层同步
    ''' </summary>
    Public ActivePages As New List(Of GamePageProperty)
    ''' <summary>
    ''' 当前鼠标位置
    ''' </summary>
    Public CurrentCursorPosition As PointI
    ''' <summary>
    ''' d2d画布对象
    ''' </summary>
    <Obsolete("升级d2d1.1后不再使用，改用d2dContext（D2D.DeviceContext继承了RenderTarget）", True)>
    Private RT As WindowRenderTarget = Nothing

    Private GlobalDevice As SharpDX.Direct3D11.Device1
    Private D3DContext As SharpDX.Direct3D11.DeviceContext1
    Private D3DRenderTargetView As SharpDX.Direct3D11.RenderTargetView
    Private D3DDepthStencilView As Direct3D11.DepthStencilView
    Private GlobalSwapChain As DXGI.SwapChain1
    ''' <summary>
    ''' Direct2D 1.1 画布对象
    ''' </summary>
    Private D2DContext As SharpDX.Direct2D1.DeviceContext
    ''' <summary>
    ''' 用于Direct2d显示的画布
    ''' </summary>
    Private D2DTarget As SharpDX.Direct2D1.Bitmap1
    ''' <summary>
    ''' D2D空白背景画布
    ''' </summary>
    Private BitmapForOriginalSkirmishMap As Bitmap1
    ''' <summary>
    ''' D3D图像
    ''' </summary>
    Public D3DRenderImage As Bitmap1

    Public Camera3D As New GameCamera3D

    Public Camera3DPixelInfo(0) As Integer

    Public Sub InitializeDirectComponents()
        '-------setup d3d11--------
        'create device and swapchain
        Dim tmpGlobalDevice As Direct3D11.Device
        Dim tmpGlobalSwapChain As DXGI.SwapChain
        Dim sc_description As New SharpDX.DXGI.SwapChainDescription
        With sc_description
            .BufferCount = 1
            .ModeDescription = New DXGI.ModeDescription(Resolve.X, Resolve.Y, New DXGI.Rational(60, 1), DXGI.Format.B8G8R8A8_UNorm)
            .SampleDescription = New DXGI.SampleDescription(1, 0)
            .Usage = SharpDX.DXGI.Usage.BackBuffer Or DXGI.Usage.RenderTargetOutput
            .Flags = DXGI.SwapChainFlags.AllowModeSwitch
            .IsWindowed = True
            .OutputHandle = Form1.Handle
            .SwapEffect = DXGI.SwapEffect.Discard
        End With
        Direct3D11.Device.CreateWithSwapChain(SharpDX.Direct3D.DriverType.Hardware, Direct3D11.DeviceCreationFlags.BgraSupport, {SharpDX.Direct3D.FeatureLevel.Level_11_1}, sc_description, tmpGlobalDevice, tmpGlobalSwapChain)
        GlobalDevice = tmpGlobalDevice.QueryInterface(Of SharpDX.Direct3D11.Device1)()
        GlobalSwapChain = tmpGlobalSwapChain.QueryInterface(Of DXGI.SwapChain1)
        'create render target
        Using resource = Direct3D11.Resource.FromSwapChain(Of Direct3D11.Texture2D)(GlobalSwapChain, 0)
            D3DRenderTargetView = New Direct3D11.RenderTargetView(GlobalDevice, resource)
        End Using
        'get device context
        D3DContext = GlobalDevice.ImmediateContext.QueryInterface(Of SharpDX.Direct3D11.DeviceContext1)()
        'create rasterizer
        Dim rsd = New Direct3D11.RasterizerStateDescription With {
            .CullMode = Direct3D11.CullMode.None,
            .FillMode = Direct3D11.FillMode.Solid}
        D3DContext.Rasterizer.State = New Direct3D11.RasterizerState(GlobalDevice, rsd)
        'create depth stencil
        Dim depthBuffer_description As New Direct3D11.Texture2DDescription With {
                .ArraySize = 1,
                .BindFlags = Direct3D11.BindFlags.DepthStencil,
                .Format = SharpDX.DXGI.Format.D32_Float,
                .Width = Resolve.X,
                .Height = Resolve.Y,
                .MipLevels = 1,
                .SampleDescription = New DXGI.SampleDescription(1, 0)
            }
        Using depthBuffer As Direct3D11.Texture2D = New Direct3D11.Texture2D(GlobalDevice, depthBuffer_description)
            D3DDepthStencilView = New Direct3D11.DepthStencilView(GlobalDevice, depthBuffer)
        End Using
        'set viewport
        Dim Viewport = New Mathematics.Interop.RawViewportF
        With Viewport
            .X = 0
            .Y = 0
            .Width = Resolve.X
            .Height = Resolve.Y
            .MaxDepth = 1.0F
            .MinDepth = 0.0F
        End With
        D3DContext.OutputMerger.SetTargets(D3DDepthStencilView, D3DRenderTargetView)
        D3DContext.Rasterizer.SetViewport(Viewport)

        '-------link to d2d1.1-----------
        Dim backBuffer As DXGI.Surface = GlobalSwapChain.GetBackBuffer(Of DXGI.Surface)(0)

        'Dim D3D11Device1 As Direct3D11.Device1 = GlobalDevice.QueryInterface(Of SharpDX.Direct3D11.Device1)()
        Dim DXGIDevice2 As SharpDX.DXGI.Device2 = GlobalDevice.QueryInterface(Of SharpDX.DXGI.Device2)()
        Dim d2dDevice As SharpDX.Direct2D1.Device = New SharpDX.Direct2D1.Device(DXGIDevice2)
        D2DContext = New SharpDX.Direct2D1.DeviceContext(d2dDevice, SharpDX.Direct2D1.DeviceContextOptions.EnableMultithreadedOptimizations)

        Dim dpiX As Single, dpiY As Single
        Using myGraphics As Graphics = Form1.CreateGraphics()    'use Graphics class to get the Dpi args
            dpiX = myGraphics.DpiX
            dpiY = myGraphics.DpiY
        End Using
        Dim Properties As BitmapProperties1 = New BitmapProperties1(New PixelFormat(SharpDX.DXGI.Format.B8G8R8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied), dpiX, dpiY, BitmapOptions.Target Or BitmapOptions.CannotDraw)
        D2DTarget = New Bitmap1(D2DContext, backBuffer, Properties)

        '-------load shaders-------
        Me.Camera3D.LoadAllShaders(GlobalDevice, D3DContext)

        '-------other stuff---------
        Dim normalProp As New BitmapProperties1() With {
                      .PixelFormat = New SharpDX.Direct2D1.PixelFormat(SharpDX.DXGI.Format.B8G8R8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied),
                      .BitmapOptions = BitmapOptions.Target}
        D3DRenderImage = New Bitmap1(D2DContext, New Size2(Resolve.X, Resolve.Y), normalProp)
        BitmapForOriginalSkirmishMap = New Bitmap1(D2DContext, New Size2(Resolve.X, Resolve.Y), normalProp)

    End Sub

    <Obsolete("use 'InitializeDirectComponents' to initialize both d3d11 and d2d1.1", False)>
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
        GlobalDevice = defaultDevice.QueryInterface(Of SharpDX.Direct3D11.Device1)()
        D3DContext = GlobalDevice.ImmediateContext.QueryInterface(Of SharpDX.Direct3D11.DeviceContext1)()

        Dim dxgiDevice2 As SharpDX.DXGI.Device2 = GlobalDevice.QueryInterface(Of SharpDX.DXGI.Device2)()
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
        GlobalSwapChain = New DXGI.SwapChain1(dxgiFactory2, GlobalDevice, Form1.Handle, description, Nothing)
        Dim d2dDevice As SharpDX.Direct2D1.Device = New SharpDX.Direct2D1.Device(dxgiDevice2)
        D2DContext = New SharpDX.Direct2D1.DeviceContext(d2dDevice, SharpDX.Direct2D1.DeviceContextOptions.EnableMultithreadedOptimizations)
        'Dim Properties As BitmapProperties1 = New BitmapProperties1(New PixelFormat(SharpDX.DXGI.Format.B8G8R8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied), DisplayProperties.LogicalDpi, DisplayProperties.LogicalDpi, BitmapOptions.Target Or BitmapOptions.CannotDraw)
        Dim dpiX As Single, dpiY As Single
        Using myGraphics As Graphics = Form1.CreateGraphics()
            dpiX = myGraphics.DpiX
            dpiY = myGraphics.DpiY
        End Using
        Dim Properties As BitmapProperties1 = New BitmapProperties1(New PixelFormat(SharpDX.DXGI.Format.B8G8R8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied), dpiX, dpiY, BitmapOptions.Target Or BitmapOptions.CannotDraw)
        Dim backBuffer As DXGI.Surface = GlobalSwapChain.GetBackBuffer(Of DXGI.Surface)(0)
        D2DTarget = New Bitmap1(D2DContext, backBuffer, Properties)

        BitmapForOriginalSkirmishMap = New Bitmap1(D2DContext, New Size2(Resolve.X, Resolve.Y), New BitmapProperties1() With {
                              .PixelFormat = New SharpDX.Direct2D1.PixelFormat(SharpDX.DXGI.Format.B8G8R8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied),
                              .BitmapOptions = BitmapOptions.Target})
        'BitmapForBlurDialog = New Bitmap1(D2DContext, New Size2(Resolve.X, Resolve.Y), New BitmapProperties1() With {
        '                      .PixelFormat = New SharpDX.Direct2D1.PixelFormat(SharpDX.DXGI.Format.B8G8R8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied),
        '                      .BitmapOptions = BitmapOptions.Target})

        'Call GameResources.LoadResources(d2dContext)   单独放到mainGameloop里了
    End Sub

    ''' <summary>
    ''' 刷新摄像机容器
    ''' </summary>
    Public Sub RefreshCamera3D()
        Me.Camera3D.FixContainer3D(Me.GlobalDevice, Me.D3DContext)
    End Sub

    ''' <summary>
    ''' 在当前D2D图层直接绘制D3D生成的图像
    ''' </summary>
    Public Sub DrawLink3DImage(ByRef context As SharpDX.Direct2D1.DeviceContext, ByRef spectator As SpectatorCamera, canvasBitmap As Bitmap1)
        context.DrawImage(D3DRenderImage)

    End Sub

    ''' <summary>
    ''' 获取3D图像左上角的像素点
    ''' <para>返回值为RGBA</para>
    ''' </summary>
    ''' <param name="context">D2DContext</param>
    Public Function GetLink3DImagePixelInfo(context As Direct2D1.DeviceContext) As Byte()
        Dim tmpBitmap As Bitmap1 = New Bitmap1(context, New Size2(D3DRenderImage.Size.Width, D3DRenderImage.Size.Height), MAP_CPU_BITMAP_PROPERTY)
        tmpBitmap.CopyFromBitmap(D3DRenderImage)

        Dim data As DataRectangle = tmpBitmap.Map(MapOptions.Read)
        Dim container(3) As Byte
        Runtime.InteropServices.Marshal.Copy(data.DataPointer, container, 0, 4)
        tmpBitmap.Unmap()

        tmpBitmap.Dispose()
        Return container
    End Function

    ''' <summary>
    ''' 将图像绘制到窗体
    ''' </summary>
    Public Sub PaintImage()
        If Me.Camera3D.Enable Then
            'clear canvas
            D3DContext.ClearRenderTargetView(D3DRenderTargetView, New Mathematics.Interop.RawColor4(0.35, 0.5, 0.35, 1))
            D3DContext.ClearDepthStencilView(D3DDepthStencilView, Direct3D11.DepthStencilClearFlags.Depth, 1, 0)
            'draw d3d
            Me.Camera3D.DrawContainer3D(GlobalDevice, D3DContext)
            'copy the 3d image to memory in order to add further 2d graphics
            D3DRenderImage.CopyFromBitmap(D2DTarget)

            Dim firstPixel As Byte() = GetLink3DImagePixelInfo(D2DContext)
            Camera3DPixelInfo(0) = firstPixel(0) + firstPixel(1) * 256
            Camera3D.PointingAt = Camera3DPixelInfo(0)

        End If
        'draw d2d
        If CBool(PaintingLayers.Count) Then
            With D2DContext
                .Target = BitmapForOriginalSkirmishMap
                .BeginDraw()
                .Clear(WHITE_COLOUR)
                For i = PaintingLayers.Count - 1 To 0 Step -1
                    PaintingLayers(i).Invoke(D2DContext, Me, BitmapForOriginalSkirmishMap)
                Next
                .EndDraw()

                .Target = D2DTarget
                .BeginDraw()
                .Clear(WHITE_COLOUR)
                .DrawImage(BitmapForOriginalSkirmishMap)

                .EndDraw()
            End With
        End If
        'display
        GlobalSwapChain.Present(0, DXGI.PresentFlags.None)
    End Sub

    ''' <summary>
    ''' 获取D2DDeviceContext
    ''' </summary>
    Public Function GetDeviceContext() As DeviceContext
        Return D2DContext
    End Function

    ''' <summary>
    ''' 获取一半的Resolve值，即屏幕中心
    ''' </summary>
    Public Function GetCenter() As PointI
        Return New PointI(CInt(Me.Resolve.X / 2), CInt(Me.Resolve.Y / 2))
    End Function

    Public Function IsMouseClick(startPosition As PointI, endPosition As PointI) As Boolean
        Dim distance As Single = Math.Sqrt((startPosition.X - endPosition.X) ^ 2 + (startPosition.Y - endPosition.Y) ^ 2)
        If distance <= 5 Then
            Return True
        End If
        Return False
    End Function

    Public Sub Dispose() Implements IDisposable.Dispose
        'TODO: dispose all directx resources
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

    <Obsolete("use 34 and 35", False)>
    SkirmishMap = 32

    Skirmish_UnitDetail = 33

    SkirmishMap3DOnly = 34

    SkirmishMap2DUI = 35

    SkirmishMap2DUI_Top_Resources = 36

    BattleAnimation = 64
End Enum
