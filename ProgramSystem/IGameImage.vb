''' <summary>
''' 游戏图片素材类，包括静态图片和动态Live2d，仅用于立绘等，不用于UI
''' </summary>
Public Interface IGameImage
    ''' <summary>
    ''' 获取图像
    ''' </summary>
    Function GetImage() As SharpDX.Direct2D1.Bitmap1
    ''' <summary>
    ''' 获取绘图时的默认正确位置
    ''' </summary>
    Function GetDrawRect() As SharpDX.Mathematics.Interop.RawRectangleF
    ''' <summary>
    ''' 直接绘图
    ''' </summary>
    ''' <param name="context"></param>
    Sub PaintFullImage(ByRef context As SharpDX.Direct2D1.DeviceContext)


End Interface
