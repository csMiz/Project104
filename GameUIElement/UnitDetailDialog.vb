
Imports SharpDX.Direct2D1
Imports SharpDX.DirectWrite
Imports SharpDX.Mathematics.Interop
''' <summary>
''' Skirmish模式下的单位详细信息面板类
''' </summary>
Public Class UnitDetailDialog
    Inherits GameDialog

    Private Shared me_instance As UnitDetailDialog = Nothing

    Private BindingUnit As GameUnit = Nothing
    ''' <summary>
    ''' 背景矩形，应该初始化为全屏
    ''' </summary>
    Private Rect_BG As RawRectangleF = Nothing
    ''' <summary>
    ''' 内容矩形
    ''' </summary>
    Private Rect_Content As RawRectangleF = Nothing
    ''' <summary>
    ''' 左侧立绘区矩形
    ''' </summary>
    Private Rect_LeftBar As RawRectangleF = Nothing
    ''' <summary>
    ''' 立绘区右侧分隔线
    ''' </summary>
    Private Line_LeftBar_Right(1) As RawVector2
    ''' <summary>
    ''' 攻击力区矩形
    ''' </summary>
    Private Rect_AtkBar As RawRectangleF = Nothing
    ''' <summary>
    ''' 攻击力区右侧分隔线
    ''' </summary>
    Private Line_AtkBar_Right(1) As RawVector2
    ''' <summary>
    ''' 攻击力区底部分隔线
    ''' </summary>
    Private Line_AtkBar_Bottom(1) As RawVector2
    ''' <summary>
    ''' 防御力区矩形
    ''' </summary>
    Private Rect_DefBar As RawRectangleF = Nothing
    ''' <summary>
    ''' 防御力区底部分隔线
    ''' </summary>
    Private Line_DefBar_Bottom(1) As RawVector2

    Private Rect_MoveBar As RawRectangleF = Nothing
    ''' <summary>
    ''' 技能树区矩形
    ''' </summary>
    Private Rect_SkillTreeBar As RawRectangleF = Nothing
    ''' <summary>
    ''' 技能树区右侧分隔线
    ''' </summary>
    Private Line_SkillTree_Right(1) As RawVector2
    ''' <summary>
    ''' 技能槽区矩形
    ''' </summary>
    Private Rect_SkillBar As RawRectangleF = Nothing
    ''' <summary>
    ''' 技能槽区底部分隔线
    ''' </summary>
    Private Line_SkillBar_Bottom(1) As RawVector2

    Private Rect_ItemBar As RawRectangleF = Nothing

    Private UsingColorSet As SolidColorBrushSet = Nothing
    Private SeparateLineLengthOffset As Integer = 10
    Private SeparateLineStrokeWidth1 As Single = 2.0F
    Private SeparateLineStrokeWidth2 As Single = 1.0F

    ''' <summary>
    ''' 模糊后图像画笔
    ''' </summary>
    Private SourceBGBrush As BitmapBrush = Nothing
    ''' <summary>
    ''' 绘制原图的画布对象
    ''' </summary>
    Private DialogBitmap As Bitmap1 = Nothing
    ''' <summary>
    ''' 绘制模糊背景的画布对象
    ''' </summary>
    Private BlurBitmap As Bitmap1 = Nothing
    ''' <summary>
    ''' 背景高斯模糊效果
    ''' </summary>
    Private BlurEffectLayer As Effects.GaussianBlur = Nothing

    Private LineStyle As StrokeStyle

    Private Sub New()
    End Sub

    Public Sub BindUnit(unit As GameUnit)
        If unit Is Nothing Then Throw New Exception("input unit is null!")
        Me.BindingUnit = unit
    End Sub

    Public Sub InitializeColor()
        If Me.BindingUnit Is Nothing Then Throw New Exception("binding unit is null!")
        Me.UsingColorSet = Me.BindingUnit.GetSideColorSet

        Dim lineProperty As StrokeStyleProperties = New StrokeStyleProperties() With {
            .StartCap = CapStyle.Round,
            .EndCap = CapStyle.Round}

        Me.LineStyle = New StrokeStyle(Me.Camera.GetDevceContext.Factory, lineProperty)
    End Sub

    Public Sub InitializeConetentBox()

        If Me.Camera Is Nothing Then Throw New Exception("camera is null!")

        Dim top_left_x As Integer = Camera.Resolve.X / 2 - Me.Width / 2
        Dim top_left_y As Integer = Camera.Resolve.Y / 2 - Me.Height / 2

        Rect_BG = New RawRectangleF(top_left_x, top_left_y, top_left_x + Me.Width, top_left_y + Me.Height)

        '内容边界，不显示
        '纵横比为1/1.618
        Dim content_height As Integer = 0
        Dim content_width As Integer = 0
        If (Me.Width / Me.Height) > UNIT_DETAIL_HV_RATIO Then
            content_height = Me.Height
            content_width = CInt(content_height * UNIT_DETAIL_HV_RATIO)
        Else
            content_width = Me.Width
            content_height = CInt(content_width / UNIT_DETAIL_HV_RATIO)
        End If

        '内容边界
        Dim content_left As Integer = Camera.Resolve.X / 2 - content_width / 2
        Dim content_top As Integer = Camera.Resolve.Y / 2 - content_height / 2
        Dim content_right As Integer = content_left + content_width
        Dim content_bottom As Integer = content_top + content_height
        Rect_Content = New RawRectangleF(content_left, content_top, content_right, content_bottom)

        '左侧立绘
        'Dim tachie_width As Integer = CInt(content_height / ROOT_FIVE)
        Dim tachie_width As Integer = CInt(content_height / ROOT_THREE)
        Dim tachie_height As Integer = content_height
        Dim tachie_left As Integer = content_left
        Dim tachie_top As Integer = content_top
        Dim tachie_right As Integer = tachie_left + tachie_width
        Dim tachie_bottom As Integer = content_bottom
        Rect_LeftBar = New RawRectangleF(tachie_left, tachie_top, tachie_right, tachie_bottom)
        Line_LeftBar_Right(0) = New RawVector2(tachie_right + 1, tachie_top + SeparateLineLengthOffset)
        Line_LeftBar_Right(1) = New RawVector2(tachie_right + 1, tachie_bottom - SeparateLineLengthOffset)

        '攻击力区
        Dim atk_width As Integer = CInt((content_width - tachie_width - 4) * 0.5)
        Dim atk_height As Integer = CInt((content_height - 2) / 3)
        Dim atk_left As Integer = tachie_right + 2
        Dim atk_top As Integer = tachie_top
        Dim atk_right As Integer = atk_left + atk_width
        Dim atk_bottom As Integer = atk_top + atk_height
        Rect_AtkBar = New RawRectangleF(atk_left, atk_top, atk_right, atk_bottom)
        Line_AtkBar_Right(0) = New RawVector2(atk_right + 1, atk_top + SeparateLineLengthOffset)
        Line_AtkBar_Right(1) = New RawVector2(atk_right + 1, atk_bottom - SeparateLineLengthOffset)
        Line_AtkBar_Bottom(0) = New RawVector2(atk_left + SeparateLineLengthOffset, atk_bottom + 1)
        Line_AtkBar_Bottom(1) = New RawVector2(content_right - SeparateLineLengthOffset, atk_bottom + 1)

        '防御力区
        Dim def_width As Integer = content_width - tachie_width - atk_width - 4
        Dim def_height As Integer = CInt((atk_height - 2) / 2)
        Dim def_left As Integer = atk_right + 2
        Dim def_top As Integer = atk_top
        Dim def_right As Integer = content_right
        Dim def_bottom As Integer = def_top + def_height
        Rect_DefBar = New RawRectangleF(def_left, def_top, def_right, def_bottom)
        Line_DefBar_Bottom(0) = New RawVector2(def_left + SeparateLineLengthOffset, def_bottom + 1)
        Line_DefBar_Bottom(1) = New RawVector2(def_right - SeparateLineLengthOffset, def_bottom + 1)

        'TODO:行动力区

        '技能树区
        Dim st_width As Integer = CInt((content_width - tachie_width - 4) * 0.8)
        Dim st_height As Integer = content_height - atk_height - 2
        Dim st_left As Integer = atk_left
        Dim st_top As Integer = atk_bottom + 2
        Dim st_right As Integer = st_left + st_width
        Dim st_bottom As Integer = content_bottom
        Rect_SkillTreeBar = New RawRectangleF(st_left, st_top, st_right, st_bottom)
        Line_SkillTree_Right(0) = New RawVector2(st_right + 1, st_top + SeparateLineLengthOffset)
        Line_SkillTree_Right(1) = New RawVector2(st_right + 1, st_bottom - SeparateLineLengthOffset)

        '技能槽区
        Dim skill_width As Integer = content_width - tachie_width - st_width - 4
        Dim skill_height As Integer = CInt((st_height - 2) / 2)
        Dim skill_left As Integer = st_right + 2
        Dim skill_top As Integer = st_top
        Dim skill_right As Integer = content_right
        Dim skill_bottom As Integer = skill_top + skill_height
        Rect_SkillBar = New RawRectangleF(skill_left, skill_top, skill_right, skill_bottom)
        Line_SkillBar_Bottom(0) = New RawVector2(skill_left + SeparateLineLengthOffset, skill_bottom + 1)
        Line_SkillBar_Bottom(1) = New RawVector2(skill_right - SeparateLineLengthOffset, skill_bottom + 1)




    End Sub

    Public Sub InitializeEffects()
        Dim context As DeviceContext = Me.Camera.GetDevceContext
        Me.DialogBitmap = New Bitmap1(context, New SharpDX.Size2(Me.Camera.Resolve.X, Me.Camera.Resolve.Y), NORMAL_BITMAP_PROPERTY)
        Me.BlurBitmap = New Bitmap1(context, New SharpDX.Size2(Me.Camera.Resolve.X, Me.Camera.Resolve.Y), NORMAL_BITMAP_PROPERTY)
        Me.BlurEffectLayer = New Effects.GaussianBlur(context)
        Me.BlurEffectLayer.SetInput(0, Me.DialogBitmap, True)
        Me.BlurEffectLayer.StandardDeviation = 10.0F
    End Sub

    ''' <summary>
    ''' 绘制对话框
    ''' </summary>
    Public Overrides Sub DrawControl(ByRef context As SharpDX.Direct2D1.DeviceContext, ByRef spec As SpectatorCamera, canvasBitmap As Bitmap1)
        With context

            '绘制模糊效果
            .EndDraw()

            .Target = Me.DialogBitmap
            .BeginDraw()
            .Clear(Nothing)
            .DrawImage(canvasBitmap)
            .EndDraw()

            .Target = Me.BlurBitmap
            .BeginDraw()
            .Clear(Nothing)
            .DrawImage(Me.BlurEffectLayer)
            .EndDraw()

            'TODO: 单独控制SourceBGBrush的更新
            If Me.SourceBGBrush Is Nothing Then SourceBGBrush = New BitmapBrush(context, Me.BlurBitmap)

            .Target = canvasBitmap
            .BeginDraw()
            .DrawBitmap(Me.BlurBitmap, NOT_TRANSPARENT, BitmapInterpolationMode.Linear)

            '绘制半透明黑色背景
            .FillRectangle(Rect_BG, GREY_COLOUR(3))

            '绘制分割线
            .DrawLine(Line_LeftBar_Right(0), Line_LeftBar_Right(1), SourceBGBrush, SeparateLineStrokeWidth1, LineStyle)
            .DrawLine(Line_AtkBar_Right(0), Line_AtkBar_Right(1), SourceBGBrush, SeparateLineStrokeWidth2, LineStyle)
            .DrawLine(Line_AtkBar_Bottom(0), Line_AtkBar_Bottom(1), SourceBGBrush, SeparateLineStrokeWidth2, LineStyle)
            .DrawLine(Line_DefBar_Bottom(0), Line_DefBar_Bottom(1), SourceBGBrush, SeparateLineStrokeWidth2, LineStyle)
            .DrawLine(Line_SkillTree_Right(0), Line_SkillTree_Right(1), SourceBGBrush, SeparateLineStrokeWidth2, LineStyle)
            .DrawLine(Line_SkillBar_Bottom(0), Line_SkillBar_Bottom(1), SourceBGBrush, SeparateLineStrokeWidth2, LineStyle)

            'rt.FillRectangle(UsingColorSet.BaseColor, Rect_LeftBar)
            'rt.FillRectangle(UsingColorSet.LightD1Color, Rect_AtkBar)
            'rt.FillRectangle(UsingColorSet.LightD1Color, Rect_DefBar)
            'rt.FillRectangle(UsingColorSet.LightD1Color, Rect_MoveBar)
            'rt.FillRectangle(UsingColorSet.LightD1Color, Rect_SkillTreeBar)
            'rt.FillRectangle(UsingColorSet.LightD1Color, Rect_SkillBar)
            'rt.FillRectangle(UsingColorSet.LightD1Color, Rect_ItemBar)

            'rt.DrawRectangle(UsingColorSet.DarkColor, Rect_AtkBar)
            'rt.DrawRectangle(UsingColorSet.DarkColor, Rect_DefBar)
            'rt.DrawRectangle(UsingColorSet.DarkColor, Rect_MoveBar)
            'rt.DrawRectangle(UsingColorSet.DarkColor, Rect_SkillTreeBar)
            'rt.DrawRectangle(UsingColorSet.DarkColor, Rect_SkillBar)
            'rt.DrawRectangle(UsingColorSet.DarkColor, Rect_ItemBar)


            '这段代码有内存泄漏的bug！！！
            'Dim testText As New TextItem("Remilia Scarlet", New PointI(350, 200))
            'testText.LoadFont(GameFontHelper.GetFontFamily(0), 36, Brushes.Red)
            'testText.GenerateImage(context)
            'context.DrawBitmap(testText.GetImage, New RawRectangleF(300, 300, 650, 500), 1.0F, BitmapInterpolationMode.Linear)


        End With
    End Sub

    Public Sub Dispose()

    End Sub

    ''' <summary>
    ''' 单例模式
    ''' </summary>
    Public Shared Function Instance() As UnitDetailDialog
        If me_instance Is Nothing Then me_instance = New UnitDetailDialog
        Return me_instance
    End Function




End Class
