' -----------------------------------------
' Copyright (c) 2019 All Rights Reserved.
' 
' Filename: GameSkirmishResourceBar
' Author: Miz
' Date: 2019/3/11 1:10:46
' -----------------------------------------

Imports p104
Imports SharpDX.Direct2D1
Imports SharpDX.DirectWrite
Imports SharpDX.Mathematics.Interop

Public Class GameSkirmishResourceBar
    Inherits GameBasicUIElement

    Public BindingSide As GameSideInfo = Nothing

    Public BarBackGroundBrush As LinearGradientBrush

    Private BackGroundTexture As Bitmap1

    Private PadTexture As Bitmap1

    Private SeparatorTexture As Bitmap1

    Private IconEdgeLight As RadialGradientBrush

    Private IconSurfaceLight As RadialGradientBrush
    ''' <summary>
    ''' 反光矩形框距离控件边缘的长度
    ''' </summary>
    Public IconEdgeDistance As Single = 2.0F

    Private FinalImage_NoLight As Bitmap1

    Private FinalImage As Bitmap1

    Private MultiplyEffect As Effects.Blend
    ''' <summary>
    ''' render 'Power','Point',etc
    ''' </summary>
    Private DefaultFont As TextFormat
    ''' <summary>
    ''' render values
    ''' </summary>
    Private DefaultFont2 As TextFormat

    Private Rect_Turn As RawRectangleF
    Private Rect_Power As RawRectangleF
    Private Rect_Point As RawRectangleF
    Private Rect_Burden As RawRectangleF



    Public Sub New()
        AddHandler Me.MouseEnter, AddressOf TopBarMouseEnter
        AddHandler Me.MouseLeave, AddressOf TopBarMouseLeave
        AddHandler Me.MouseMove, AddressOf TopBarMouseMove
        AddHandler Me.GlobalMouseMove, AddressOf TopBarGlobalMove
    End Sub

    Public Sub InitializeBar()
        Dim prop As New LinearGradientBrushProperties
        With prop
            .StartPoint = New RawVector2(Me.Width / 2, 0)
            .EndPoint = New RawVector2(Me.Width / 2, Me.Height)
        End With
        Dim gs(2) As GradientStop
        gs(0) = New GradientStop With {
            .Position = 0.0F,
            .Color = New RawColor4(0.1, 0.1, 0.1, 1)}
        gs(1) = New GradientStop With {
            .Position = 0.3F,
            .Color = New RawColor4(0, 0, 0, 0.7)}
        gs(2) = New GradientStop With {
            .Position = 1.0F,
            .Color = New RawColor4(0, 0, 0, 0.1)}
        Dim gsc As New GradientStopCollection(BindingContext, gs)
        Me.BarBackGroundBrush = New LinearGradientBrush(BindingContext, prop, gsc)
        gsc.Dispose()

        Dim r_brushProperty As New RadialGradientBrushProperties()
        With r_brushProperty
            .Center = New RawVector2(-50, -50)
            .GradientOriginOffset = New RawVector2(0, 0)
            .RadiusX = 50
            .RadiusY = 50
        End With
        Dim stops(1) As GradientStop
        stops(0) = New GradientStop() With {
            .Color = New RawColor4(1, 1, 1, 0.78),
            .Position = 0.0F}
        stops(1) = New GradientStop() With {
            .Color = New RawColor4(1, 1, 1, 0),
            .Position = 1.0F}

        Dim stopCollection As New GradientStopCollection(Me.BindingContext, stops)
        Me.IconEdgeLight = New RadialGradientBrush(Me.BindingContext, r_brushProperty, NORMAL_BRUSH_PROPERTY, stopCollection)
        stopCollection.Dispose()

        Dim r_brushProperty2 As New RadialGradientBrushProperties()
        With r_brushProperty2
            .Center = New RawVector2(-150, -150)
            .GradientOriginOffset = New RawVector2(0, 0)
            .RadiusX = 150
            .RadiusY = 150
        End With
        Dim stops2(1) As GradientStop
        stops2(0) = New GradientStop() With {
            .Color = New RawColor4(1, 1, 1, 0.37),
            .Position = 0.0F}
        stops2(1) = New GradientStop() With {
            .Color = New RawColor4(1, 1, 1, 0),
            .Position = 1.0F}

        Dim stopCollection2 As New GradientStopCollection(Me.BindingContext, stops2)
        Me.IconSurfaceLight = New RadialGradientBrush(Me.BindingContext, r_brushProperty2, NORMAL_BRUSH_PROPERTY, stopCollection2)
        stopCollection2.Dispose()

        'h: player icon
        '2h: turn board + sep
        'img: Pow + sep
        'img: Point + sep
        'img: Bur + sep

        Dim powerTextureSource As Bitmap1 = FragmentImages.GetFragment(0, CONTROL_IMAGE_DOMAIN)
        Dim pointTextureSource As Bitmap1 = FragmentImages.GetFragment(1, CONTROL_IMAGE_DOMAIN)
        Dim burdenTextureSource As Bitmap1 = FragmentImages.GetFragment(2, CONTROL_IMAGE_DOMAIN)

        Me.PadTexture = New Bitmap1(BindingContext, New SharpDX.Size2(Me.Width, Me.Height), NORMAL_BITMAP_PROPERTY)
        Dim ratio As Single = Me.Height / powerTextureSource.Size.Height
        Dim imgBoardWidth As Single = powerTextureSource.Size.Width * ratio
        With Me.BindingContext
            .Target = Me.PadTexture
            .BeginDraw()
            .Clear(Nothing)

            .DrawBitmap(powerTextureSource, New RawRectangleF(3 * Me.Height, 0, 3 * Me.Height + imgBoardWidth, Me.Height), NOT_TRANSPARENT, BitmapInterpolationMode.Linear)
            .DrawBitmap(pointTextureSource, New RawRectangleF(3 * Me.Height + imgBoardWidth, 0, 3 * Me.Height + 2 * imgBoardWidth, Me.Height), NOT_TRANSPARENT, BitmapInterpolationMode.Linear)
            .DrawBitmap(burdenTextureSource, New RawRectangleF(3 * Me.Height + 2 * imgBoardWidth, 0, 3 * Me.Height + 3 * imgBoardWidth, Me.Height), NOT_TRANSPARENT, BitmapInterpolationMode.Linear)

            .EndDraw()
        End With

        Dim prop2 As New LinearGradientBrushProperties
        With prop2
            .StartPoint = New RawVector2(Me.Width / 2, 0)
            .EndPoint = New RawVector2(Me.Width / 2, Me.Height)
        End With
        Dim gs2(1) As GradientStop
        gs2(0) = New GradientStop With {
            .Position = 0.0F,
            .Color = New RawColor4(0.7, 0.7, 0.7, 1.0)}
        gs2(1) = New GradientStop With {
            .Position = 1.0F,
            .Color = New RawColor4(0.7, 0.7, 0.7, 0.1)}
        Dim gsc2 As New GradientStopCollection(BindingContext, gs2)
        Dim sep_brush As LinearGradientBrush = New LinearGradientBrush(BindingContext, prop2, gsc2)
        gsc2.Dispose()

        Me.SeparatorTexture = New Bitmap1(BindingContext, New SharpDX.Size2(Me.Width, Me.Height), NORMAL_BITMAP_PROPERTY)
        With Me.BindingContext
            .Target = Me.SeparatorTexture
            .BeginDraw()
            .Clear(Nothing)

            .DrawLine(New RawVector2(3 * Me.Height, 0), New RawVector2(3 * Me.Height, Me.Height), sep_brush)
            .DrawLine(New RawVector2(3 * Me.Height + imgBoardWidth, 0), New RawVector2(3 * Me.Height + imgBoardWidth, Me.Height), sep_brush)
            .DrawLine(New RawVector2(3 * Me.Height + 2 * imgBoardWidth, 0), New RawVector2(3 * Me.Height + 2 * imgBoardWidth, Me.Height), sep_brush)
            .DrawLine(New RawVector2(3 * Me.Height + 3 * imgBoardWidth, 0), New RawVector2(3 * Me.Height + 3 * imgBoardWidth, Me.Height), sep_brush)

            .EndDraw()
        End With
        sep_brush.Dispose()

        Me.BackGroundTexture = New Bitmap1(BindingContext, New SharpDX.Size2(Me.Width, Me.Height), NORMAL_BITMAP_PROPERTY)
        With Me.BindingContext
            .Target = Me.BackGroundTexture
            .BeginDraw()
            .Clear(Nothing)
            .FillRectangle(Me.SelfCanvasRect, Me.BarBackGroundBrush)
            .EndDraw()
        End With

        MultiplyEffect = New Effects.Blend(BindingContext)
        With MultiplyEffect
            .Mode = BlendMode.Multiply
            .SetInput(0, Me.BackGroundTexture, True)
            .SetInput(1, Me.PadTexture, True)
        End With

        Rect_Turn = New RawRectangleF(Me.Height + IconEdgeDistance, IconEdgeDistance, Me.Height * 3 - IconEdgeDistance, Me.Height - IconEdgeDistance)
        Rect_Power = New RawRectangleF(Me.Height * 3 + IconEdgeDistance, IconEdgeDistance, Me.Height * 3 + imgBoardWidth - IconEdgeDistance, Me.Height - IconEdgeDistance)
        Rect_Point = New RawRectangleF(Rect_Power.Left + imgBoardWidth, IconEdgeDistance, Rect_Power.Right + imgBoardWidth, Rect_Power.Bottom)
        Rect_Burden = New RawRectangleF(Rect_Point.Left + imgBoardWidth, IconEdgeDistance, Rect_Point.Right + imgBoardWidth, Rect_Power.Bottom)

        DefaultFont = GameFontHelper2.GetCustomTextFormat("P104_Font1", 14)
        DefaultFont2 = GameFontHelper2.GetCustomTextFormat("P104_Font1", 20)

        Dim layoutSize As New PointF2(imgBoardWidth, Me.Height)
        'Dim text_power As TextLayout = GameFontHelper2.GetTextLayout("Power", DefaultFont, layoutSize)
        'Dim text_point As TextLayout = GameFontHelper2.GetTextLayout("点", DefaultFont, layoutSize)
        'Dim text_burden As TextLayout = GameFontHelper2.GetTextLayout("负载", DefaultFont, layoutSize)

        Me.FinalImage = New Bitmap1(BindingContext, New SharpDX.Size2(Me.Width, Me.Height), NORMAL_BITMAP_PROPERTY)
        Me.FinalImage_NoLight = New Bitmap1(BindingContext, New SharpDX.Size2(Me.Width, Me.Height), NORMAL_BITMAP_PROPERTY)
        With Me.BindingContext
            .Target = Me.FinalImage_NoLight
            .BeginDraw()
            .Clear(Nothing)
            .DrawImage(MultiplyEffect)
            .DrawImage(SeparatorTexture)
            'draw icon
            .DrawBitmap(GameIcons.GetIcon(1), New RawRectangleF(Rect_Power.Left, Rect_Power.Top, Rect_Power.Left + Rect_Power.Bottom - Rect_Power.Top, Rect_Power.Bottom), NOT_TRANSPARENT, BitmapInterpolationMode.Linear)
            .DrawBitmap(GameIcons.GetIcon(2), New RawRectangleF(Rect_Point.Left, Rect_Point.Top, Rect_Point.Left + Rect_Point.Bottom - Rect_Point.Top, Rect_Point.Bottom), NOT_TRANSPARENT, BitmapInterpolationMode.Linear)
            .DrawBitmap(GameIcons.GetIcon(3), New RawRectangleF(Rect_Burden.Left, Rect_Burden.Top, Rect_Burden.Left + Rect_Burden.Bottom - Rect_Burden.Top, Rect_Burden.Bottom), NOT_TRANSPARENT, BitmapInterpolationMode.Linear)

            'draw text
            '.DrawTextLayout(New RawVector2(Me.Height * 3 + 6, 5), text_power, GREY_WHITE_BRUSH)
            '.DrawTextLayout(New RawVector2(Me.Height * 3 + imgBoardWidth + 6, 5), text_point, GREY_WHITE_BRUSH)
            '.DrawTextLayout(New RawVector2(Me.Height * 3 + 2 * imgBoardWidth + 6, 5), text_burden, GREY_WHITE_BRUSH)

            .EndDraw()
        End With

        'text_power.Dispose()
        'text_point.Dispose()
        'text_burden.Dispose()

        RefreshFinalImage(BindingContext)
    End Sub

    Public Sub RefreshFinalImage(context As DeviceContext)
        Dim imgBoardWidth As Single = Rect_Point.Left - Rect_Power.Left
        Dim layoutSize As New PointF2(imgBoardWidth, Me.Height)
        Dim text_power As TextLayout = GameFontHelper2.GetTextLayout(BindingSide.Power.GetValue, DefaultFont2, layoutSize)
        Dim text_point As TextLayout = GameFontHelper2.GetTextLayout(BindingSide.Point.GetValue, DefaultFont2, layoutSize)
        Dim text_burden As TextLayout = GameFontHelper2.GetTextLayout(BindingSide.Burden.GetValue, DefaultFont2, layoutSize)
        Dim text_turn As TextLayout = GameFontHelper2.GetTextLayout("回合" & GetUppercaseChineseNumbers(BindingSide.CurrentTurn.GetValue), DefaultFont2, layoutSize)

        With context
            .Target = Me.FinalImage
            .BeginDraw()
            .Clear(Nothing)
            .DrawImage(FinalImage_NoLight)
            'draw resources value
            .DrawTextLayout(New RawVector2(Rect_Power.Left + 0.5 * imgBoardWidth, 0.2 * Me.Height), text_power, PURE_WHITE_BRUSH)
            .DrawTextLayout(New RawVector2(Rect_Point.Left + 0.5 * imgBoardWidth, 0.2 * Me.Height), text_point, PURE_WHITE_BRUSH)
            .DrawTextLayout(New RawVector2(Rect_Burden.Left + 0.5 * imgBoardWidth, 0.2 * Me.Height), text_burden, PURE_WHITE_BRUSH)
            .DrawTextLayout(New RawVector2(Rect_Turn.Left + 5, 0.15 * Me.Height), text_turn, PURE_WHITE_BRUSH)

            .DrawRectangle(Rect_Turn, IconEdgeLight)
            .DrawRectangle(Rect_Power, IconEdgeLight)
            .DrawRectangle(Rect_Point, IconEdgeLight)
            .DrawRectangle(Rect_Burden, IconEdgeLight)

            .EndDraw()
        End With

        text_power.Dispose()
        text_point.Dispose()
        text_burden.Dispose()
        text_turn.Dispose()
    End Sub

    Public Overrides Sub DrawControlAtSelfCanvas(ByRef context As DeviceContext, ByRef spec As SpectatorCamera, canvasBitmap As Bitmap1)
        Me.RefreshFinalImage(context)
        With context
            .Target = Me.ControlCanvas
            .BeginDraw()
            .Clear(Nothing)

            .DrawImage(Me.FinalImage)


            .EndDraw()
        End With
    End Sub

    Public Sub TopBarMouseEnter(e As GameMouseEventArgs)
        Me.HaveFocus = True
    End Sub

    Public Sub TopBarMouseLeave(e As GameMouseEventArgs)
        Me.HaveFocus = False
    End Sub

    Public Sub TopBarMouseMove(e As GameMouseEventArgs)
        If Me.HaveFocus Then
            e.Deliver = False

        End If
    End Sub

    Public Sub TopBarGlobalMove(e As GameMouseEventArgs)
        If e.Y < Me.Height + 150 Then
            Dim position As New RawVector2(e.X, e.Y)
            Me.IconEdgeLight.Center = position
            Me.IconSurfaceLight.Center = position

            Me.NeedRepaint = True
        End If
    End Sub

End Class
