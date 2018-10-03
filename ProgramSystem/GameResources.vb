Imports System.Drawing.Imaging
Imports SharpDX
Imports SharpDX.Direct2D1
Imports SharpDX.DirectWrite
Imports System.Math
Imports System.Xml
Imports System.Text.RegularExpressions
Imports SharpDX.Mathematics.Interop

''' <summary>
''' 游戏资源模块
''' </summary>
Module GameResources

    ''' <summary>
    ''' 游戏单位模板仓库
    ''' </summary>
    Public UnitTemplates As UnitTemplateRepository = UnitTemplateRepository.Instance
    ''' <summary>
    ''' 单位图片资源仓库
    ''' </summary>
    Public UnitImages As UnitImageRepository = UnitImageRepository.Instance
    ''' <summary>
    ''' 游戏图标资源仓库
    ''' </summary>
    Public GameIcons As GameIconRepository = GameIconRepository.Instance
    ''' <summary>
    ''' 文字字体助手
    ''' </summary>
    Public GameFontHelper As FontHelper = FontHelper.Instance

    Public BITMAP_HEX_GRASS As Bitmap
    Public BITMAP_HEX_FOREST As Bitmap
    Public BITMAP_HEX_MOUNTAIN As Bitmap

    Public TERRAIN_BITMAP As New List(Of Bitmap)

    Public Const SIX_TWO_FIVE As Single = 62.5
    Public Const THREE_SEVEN_FIVE As Single = 375
    Public ReadOnly SIX_TWO_FIVE_ROOT3 As Single = 62.5 * Sqrt(3)
    Public ReadOnly TWO_FIFTY_ROOT3 As Single = 250 * Sqrt(3)
    Public ReadOnly ONE_TWO_FIVE_ROOT3 As Single = 125 * Sqrt(3)
    Public Const FIVE_HUNDRED As Single = 500

    ''' <summary>
    ''' 阵营颜色预设列表
    ''' </summary>
    Public SideColorList As List(Of SolidColorBrushSet) = Nothing

    Public ReadOnly AllGameStages As New List(Of SingleGameLoopStage) From {0, 1, 2, 3, 4, 5}

    ''' <summary>
    ''' 加载资源
    ''' </summary>
    Public Sub LoadResources(rt As RenderTarget)
        Dim gdi_hex_grass As System.Drawing.Bitmap = My.Resources.hex_grass
        BITMAP_HEX_GRASS = LoadBitmap(rt, gdi_hex_grass)
        Dim gdi_hex_forest As System.Drawing.Bitmap = My.Resources.hex_forest
        BITMAP_HEX_FOREST = LoadBitmap(rt, gdi_hex_forest)
        Dim gdi_hex_mountain As System.Drawing.Bitmap = My.Resources.hex_mountain
        BITMAP_HEX_MOUNTAIN = LoadBitmap(rt, gdi_hex_mountain)

        With TERRAIN_BITMAP
            .Add(Nothing) 'none
            .Add(Nothing)
            .Add(BITMAP_HEX_MOUNTAIN)
            .Add(BITMAP_HEX_FOREST)
            .Add(Nothing)
            .Add(BITMAP_HEX_GRASS)
        End With

        Dim unitTemplatesString As String = My.Resources.UnitTemplates
        UnitTemplates.LoadTemplates(unitTemplatesString)

        UnitImages.LoadFromFiles(rt)
        GameIcons.LoadFromFiles(rt)

        SideColorList = SolidColorBrushSet.LoadFromXml(rt, My.Resources.Colours)

        GameFontHelper.AddFontFile(Application.StartupPath & "\P104_Font1.ttf", "P104_Font1")



    End Sub

    ''' <summary>
    ''' 将system.drawing.bitmap转换为d2dbitmap
    ''' </summary>
    Public Function LoadBitmap(rt As RenderTarget, drawingBitmap As System.Drawing.Bitmap) As Bitmap
        Dim result As Bitmap = Nothing
        Dim drawingBitmapData As BitmapData = drawingBitmap.LockBits(New Rectangle(0, 0, drawingBitmap.Width, drawingBitmap.Height), ImageLockMode.ReadOnly, Imaging.PixelFormat.Format32bppPArgb)
        Dim dataStreamxx As DataStream = New DataStream(drawingBitmapData.Scan0, drawingBitmapData.Stride * drawingBitmapData.Height, True, False)
        Dim properties As Direct2D1.BitmapProperties = New Direct2D1.BitmapProperties()
        properties.PixelFormat = New Direct2D1.PixelFormat(DXGI.Format.B8G8R8A8_UNorm, Direct2D1.AlphaMode.Premultiplied)
        result = New Direct2D1.Bitmap(rt, New Size2(drawingBitmap.Width, drawingBitmap.Height), dataStreamxx, drawingBitmapData.Stride, properties)
        drawingBitmap.UnlockBits(drawingBitmapData)
        Return result
    End Function



End Module

''' <summary>
''' 游戏阵营颜色预设
''' </summary>
Public Class SolidColorBrushSet
    Public BaseColor As Brush = Nothing
    Public LightColor As Brush = Nothing
    Public DarkColor As Brush = Nothing
    Public BaseL1Color As Brush = Nothing
    Public LightL1Color As Brush = Nothing
    Public DarkL1Color As Brush = Nothing
    Public BaseD1Color As Brush = Nothing
    Public LightD1Color As Brush = Nothing
    Public DarkD1Color As Brush = Nothing

    Public Shared Function LoadFromXml(rt As RenderTarget, xml As String) As List(Of SolidColorBrushSet)
        Dim resultList As New List(Of SolidColorBrushSet)
        Dim xmlDoc As New XmlDocument()
        xmlDoc.LoadXml(xml)
        Dim root As XmlNode = xmlDoc.SelectSingleNode("content")
        Dim xnl As XmlNodeList = root.ChildNodes
        For Each itemSet As XmlNode In xnl
            Dim resultSet As New SolidColorBrushSet
            Dim elementSet As XmlElement = CType(itemSet, XmlElement)
            Dim elementChild As XmlNodeList = elementSet.ChildNodes
            For Each itemColor As XmlNode In elementChild
                Dim elementColor As XmlElement = CType(itemColor, XmlElement)
                Dim colorString As String = elementColor.GetAttribute("value")
                Dim argbString() As String = Regex.Split(colorString, ",")
                Dim colorValue As New RawColor4 With {
                    .A = CInt(argbString(0)) / 255,
                    .R = CInt(argbString(1)) / 255,
                    .G = CInt(argbString(2)) / 255,
                    .B = CInt(argbString(3)) / 255
                }
                If elementColor.Name = "base" Then
                    resultSet.BaseColor = New SolidColorBrush(rt, colorValue)
                ElseIf elementColor.Name = "light" Then
                    resultSet.LightColor = New SolidColorBrush(rt, colorValue)
                ElseIf elementColor.Name = "dark" Then
                    resultSet.DarkColor = New SolidColorBrush(rt, colorValue)
                ElseIf elementColor.Name = "lightd1" Then
                    resultSet.LightD1Color = New SolidColorBrush(rt, colorValue)

                End If
            Next
            resultList.Add(resultSet)
        Next
        Return resultList
    End Function

End Class

