' -----------------------------------------
' Copyright (c) 2018 All Rights Reserved.
' 
' Filename: GameIconButton
' Author: Miz
' Date: 2018/12/29 18:17:01
' -----------------------------------------

Imports p104
Imports SharpDX.Direct2D1
Imports SharpDX.Mathematics.Interop

''' <summary>
''' 游戏图标按钮类，用于详情面板的切换主副武器等
''' </summary>
Public Class GameIconButton
    Inherits GameButton

    ''' <summary>
    ''' 未激活完整图标图片
    ''' </summary>
    Public PlainIconImage As Bitmap1 = Nothing
    ''' <summary>
    ''' 激活完整图标图片
    ''' </summary>
    Public ActiveIconImage As Bitmap1 = Nothing
    ''' <summary>
    ''' 进度遮罩画笔
    ''' </summary>
    Public CoverColorBrush As Brush = Nothing
    ''' <summary>
    ''' 鼠标悬停时进度遮罩画笔
    ''' </summary>
    Public HoverCoverColorBrush As Brush = Nothing
    ''' <summary>
    ''' 鼠标光影画笔
    ''' </summary>
    Public LightBrush As RadialGradientBrush = Nothing
    ''' <summary>
    ''' 按钮进度模式
    ''' </summary>
    Public IconButtonProgressMode As GameIconButtonProgressMode = GameIconButtonProgressMode.Button
    ''' <summary>
    ''' 按钮形状
    ''' </summary>
    Public IconButtonShape As GameIconButtonShape = GameIconButtonShape.Square

    ''' <summary>
    ''' 是否被选中（激活）
    ''' </summary>
    Public IconButtonActive As Boolean = False
    ''' <summary>
    ''' 进度值，区间为[0,1]，Button模式下无效
    ''' </summary>
    Public Progress As Single = 0.0F

    Private IconImageRect As RawRectangleF = Nothing

    Private IconImageCentreBrush As ImageBrush = Nothing

    Private Square_InnerSquare As RawRectangleF = Nothing

    Private ProgressRegion As PathGeometry

    Private ProgressRegionSink As GeometrySink = Nothing

    Private SquareAltitude_ProgressRegion As RawRectangleF = Nothing

    Private Circle_OuterCircle As Ellipse = Nothing

    Private Circle_InnerCircle As Ellipse = Nothing

    Public Sub New(progressMode As GameIconButtonProgressMode, shape As GameIconButtonShape)
        Me.IconButtonProgressMode = progressMode
        Me.IconButtonShape = shape
    End Sub

    Public Sub GeneratePaths()
        'TODO
    End Sub

    Public Overrides Sub DrawControlAtSelfCanvas(ByRef context As DeviceContext, ByRef spec As SpectatorCamera, canvasBitmap As Bitmap1)
        Throw New NotImplementedException()
        'TODO
    End Sub
End Class


Public Enum GameIconButtonProgressMode As Byte
    Button = 0
    PieProgress = 1
    AltitudeProgress = 2

End Enum

Public Enum GameIconButtonShape As Byte
    Square = 0
    Circle = 1

End Enum