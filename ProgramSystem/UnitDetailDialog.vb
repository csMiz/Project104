
Imports SlimDX.Direct2D
''' <summary>
''' Skirmish模式下的单位详细信息面板类
''' </summary>
Public Class UnitDetailDialog
    Inherits GameDialog

    Private Shared me_instance As UnitDetailDialog = Nothing

    Private BindingUnit As GameUnit = Nothing

    Private Rect_BG As RectangleF = Nothing
    Private Rect_LeftBar As RectangleF = Nothing
    Private Rect_AtkBar As RectangleF = Nothing
    Private Rect_DefBar As RectangleF = Nothing
    Private Rect_MoveBar As RectangleF = Nothing
    Private Rect_SkillTreeBar As RectangleF = Nothing
    Private Rect_SkillBar As RectangleF = Nothing
    Private Rect_ItemBar As RectangleF = Nothing

    Private UsingColorSet As SolidColorBrushSet

    Private Sub New()
    End Sub

    Public Sub BindUnit(unit As GameUnit)
        If unit Is Nothing Then Throw New Exception("bind unit is null!")
        Me.BindingUnit = unit
    End Sub

    Public Sub InitializeColor()
        If Me.BindingUnit Is Nothing Then Throw New Exception("bind unit is null!")
        Me.UsingColorSet = Me.BindingUnit.GetSideColorSet

    End Sub

    Public Sub InitializeConetentBox()
        If Me.Camera Is Nothing Then Throw New Exception("camera is null!")

        Dim top_left_x As Integer = Camera.Resolve.X / 2 - Me.Width / 2
        Dim top_left_y As Integer = Camera.Resolve.Y / 2 - Me.Height / 2
        '背景
        Rect_BG = New RectangleF(top_left_x, top_left_y, Me.Width, Me.Height)

        Dim leftBar_width As Single = Me.Height * 0.382
        '人物立绘区
        Rect_LeftBar = New RectangleF(top_left_x, top_left_y, leftBar_width, Me.Height)

        Dim content_width As Single = Me.Width - leftBar_width
        Dim atkBar_width As Single = content_width * 0.54   '0.9*0.6
        Dim atkBar_height As Single = Me.Height * 0.315     '0.9*0.35
        Dim atkBar_left As Single = top_left_x + leftBar_width + content_width * 0.04
        Dim atkBar_top As Single = top_left_y + Me.Height * 0.04
        '攻击力区
        Rect_AtkBar = New RectangleF(atkBar_left, atkBar_top, atkBar_width, atkBar_height)

        Dim defBar_width As Single = content_width * 0.36
        Dim defBar_height As Single = Me.Height * 0.15
        Dim defBar_left As Single = atkBar_left + atkBar_width + content_width * 0.02
        '防御力区
        Rect_DefBar = New RectangleF(defBar_left, atkBar_top, defBar_width, defBar_height)

        Dim movBar_top As Single = atkBar_top + defBar_height + Me.Height * 0.015
        '行动力区
        Rect_MoveBar = New RectangleF(defBar_left, movBar_top, defBar_width, defBar_height)

        Dim skillTreeBar_width As Single = content_width * 0.765     '0.9*0.85
        Dim skillTreeBar_height As Single = Me.Height * 0.585     '0.9*0.65
        Dim skillTreeBar_top As Single = atkBar_top + atkBar_height + Me.Height * 0.02
        '技能树区
        Rect_SkillTreeBar = New RectangleF(atkBar_left, skillTreeBar_top, skillTreeBar_width, skillTreeBar_height)

        Dim skillBar_width As Single = content_width * 0.9 - skillTreeBar_width
        Dim skillBar_height As Single = Me.Height * 0.285
        Dim skillBar_left As Single = atkBar_left + skillTreeBar_width + content_width * 0.02
        '技能区
        Rect_SkillBar = New RectangleF(skillBar_left, skillTreeBar_top, skillBar_width, skillBar_height)

        Dim itemBar_top As Single = skillTreeBar_top + skillBar_height + Me.Height * 0.015
        '物品区
        Rect_ItemBar = New RectangleF(skillBar_left, itemBar_top, skillBar_width, skillBar_height)

    End Sub

    ''' <summary>
    ''' 绘制对话框
    ''' </summary>
    Public Overrides Sub PaintDialog(ByRef rt As WindowRenderTarget, ByRef spec As SpectatorCamera)
        If Me.Camera Is Nothing AndAlso spec IsNot Nothing Then
            Me.Camera = spec
            Me.InitializeConetentBox()
        End If

        rt.FillRectangle(UsingColorSet.LightColor, Rect_BG)
        rt.FillRectangle(UsingColorSet.BaseColor, Rect_LeftBar)
        rt.FillRectangle(UsingColorSet.LightD1Color, Rect_AtkBar)
        rt.FillRectangle(UsingColorSet.LightD1Color, Rect_DefBar)
        rt.FillRectangle(UsingColorSet.LightD1Color, Rect_MoveBar)
        rt.FillRectangle(UsingColorSet.LightD1Color, Rect_SkillTreeBar)
        rt.FillRectangle(UsingColorSet.LightD1Color, Rect_SkillBar)
        rt.FillRectangle(UsingColorSet.LightD1Color, Rect_ItemBar)

        rt.DrawRectangle(UsingColorSet.DarkColor, Rect_AtkBar)
        rt.DrawRectangle(UsingColorSet.DarkColor, Rect_DefBar)
        rt.DrawRectangle(UsingColorSet.DarkColor, Rect_MoveBar)
        rt.DrawRectangle(UsingColorSet.DarkColor, Rect_SkillTreeBar)
        rt.DrawRectangle(UsingColorSet.DarkColor, Rect_SkillBar)
        rt.DrawRectangle(UsingColorSet.DarkColor, Rect_ItemBar)

    End Sub

    ''' <summary>
    ''' 单例模式
    ''' </summary>
    Public Shared Function Instance() As UnitDetailDialog
        If me_instance Is Nothing Then me_instance = New UnitDetailDialog
        Return me_instance
    End Function




End Class
