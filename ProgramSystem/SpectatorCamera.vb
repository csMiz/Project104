﻿Imports SharpDX
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
    Public Diagonal As Single = 0
    ''' <summary>
    ''' 定义绘图委托
    ''' </summary>
    Public Delegate Sub Draw(ByRef context As SharpDX.Direct2D1.DeviceContext, ByRef spectator As SpectatorCamera, canvasBitmap As Bitmap1)
    ''' <summary>
    ''' 分层绘图，不考虑小窗口穿透的模式
    ''' </summary>
    Public PaintingLayers As New Stack(Of Draw)
    ''' <summary>
    ''' 图层说明
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

    Private device As SharpDX.Direct3D11.Device1
    Private d3dContext As SharpDX.Direct3D11.DeviceContext1
    Private swapChain As DXGI.SwapChain1
    ''' <summary>
    ''' Direct2D 1.1 画布对象
    ''' </summary>
    Private d2dContext As SharpDX.Direct2D1.DeviceContext
    Private d2dTarget As SharpDX.Direct2D1.Bitmap1

    Private BitmapForOriginalSkirmishMap As Bitmap1
    Private BitmapForBlurDialog As Bitmap1

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

        BitmapForOriginalSkirmishMap = New Bitmap1(d2dContext, New Size2(Resolve.X, Resolve.Y), New BitmapProperties1() With {
                              .PixelFormat = New SharpDX.Direct2D1.PixelFormat(SharpDX.DXGI.Format.B8G8R8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied),
                              .BitmapOptions = BitmapOptions.Target})
        BitmapForBlurDialog = New Bitmap1(d2dContext, New Size2(Resolve.X, Resolve.Y), New BitmapProperties1() With {
                              .PixelFormat = New SharpDX.Direct2D1.PixelFormat(SharpDX.DXGI.Format.B8G8R8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied),
                              .BitmapOptions = BitmapOptions.Target})

        'Call GameResources.LoadResources(d2dContext)   单独放到mainGameloop里了
    End Sub

    Public Sub RefreshResolve()
        Me.Diagonal = Math.Sqrt(Me.Resolve.X ^ 2 + Me.Resolve.Y ^ 2)
    End Sub

    Public Sub PaintImage()
        If CBool(PaintingLayers.Count) Then
            d2dContext.Target = BitmapForOriginalSkirmishMap
            d2dContext.BeginDraw()
            d2dContext.Clear(WHITE_COLOUR)
            For i = PaintingLayers.Count - 1 To 0 Step -1
                PaintingLayers(i).Invoke(d2dContext, Me, BitmapForOriginalSkirmishMap)
            Next

            d2dContext.EndDraw()

            'Dim bru As New BitmapBrush1(d2dContext, BitmapForOriginalSkirmishMap)
            'd2dContext.Target = BitmapForBlurDialog
            'd2dContext.BeginDraw()
            'd2dContext.FillRectangle(New Mathematics.Interop.RawRectangleF(200, 200, 800, 550), bru)
            'd2dContext.EndDraw()


            'Dim eff As New Effects.GaussianBlur(d2dContext)
            'eff.SetInput(0, BitmapForBlurDialog, True)
            'eff.StandardDeviation = 5.0F

            d2dContext.Target = d2dTarget
            d2dContext.BeginDraw()
            d2dContext.DrawImage(BitmapForOriginalSkirmishMap)
            'd2dContext.DrawImage(eff)
            d2dContext.EndDraw()

            swapChain.Present(0, DXGI.PresentFlags.None)    '0 or 1, I don't know
        End If
    End Sub

    Public Function GetDevceContext() As DeviceContext
        Return d2dContext
    End Function

    Public Function GetCenter() As PointI
        Return New PointI(CInt(Me.Resolve.X / 2), CInt(Me.Resolve.Y / 2))
    End Function

    Public Function IsMouseClick(startPosition As PointI, endPosition As PointI) As Boolean
        Dim distance As Single = Math.Sqrt((startPosition.X - endPosition.X) ^ 2 + (startPosition.Y - endPosition.Y) ^ 2)
        If distance <= Me.Diagonal / 500 Then
            Return True
        End If
        Return False
    End Function

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
