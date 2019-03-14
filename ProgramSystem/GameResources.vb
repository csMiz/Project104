Imports System.Drawing.Imaging
Imports SharpDX
Imports SharpDX.Direct2D1
Imports SharpDX.DirectWrite
Imports System.Math
Imports System.Xml
Imports System.Text.RegularExpressions
Imports SharpDX.Mathematics.Interop
Imports SharpDX.IO

''' <summary>
''' 游戏资源模块
''' </summary>
Module GameResources

    Private GameImagingFactory As WIC.ImagingFactory = New WIC.ImagingFactory

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


    Public FragmentImages As BasicImageRepository = BasicImageRepository.Instance

    Public Live2dImages As LiveImageRepository = LiveImageRepository.Instance
    '''' <summary>
    '''' 文字字体助手
    '''' </summary>
    '<Obsolete("use DirectWrite in FontHelper2", True)>
    'Public GameFontHelper As FontHelper = FontHelper.Instance
    ''' <summary>
    ''' 文字字体助手ver.2
    ''' </summary>
    Public GameFontHelper2 As FontHelper2 = Nothing

    ''' <summary>
    ''' 全局三维模型载入器
    ''' </summary>
    Public Object3DLoaderInstance As Game3DObjectReader = Game3DObjectReader.Instance

    ''' <summary>
    ''' 六边形地形块图片
    ''' </summary>
    Public TERRAIN_BITMAP As New List(Of Bitmap1)
    ''' <summary>
    ''' 六边形地形块底部颜色
    ''' </summary>
    Public TERRAIN_BASECOLOUR As New List(Of SolidColorBrush)

    Public ACCESSORY_TREE As New List(Of Bitmap1)

    Public Const SIX_TWO_FIVE As Single = 62.5
    Public Const THREE_SEVEN_FIVE As Single = 375
    Public ReadOnly SIX_TWO_FIVE_ROOT3 As Single = 62.5 * Sqrt(3)
    Public ReadOnly TWO_FIFTY_ROOT3 As Single = 250 * Sqrt(3)
    Public ReadOnly ONE_TWO_FIVE_ROOT3 As Single = 125 * Sqrt(3)
    Public ReadOnly TWO_FIFTY_PLUS_ONE_TWO_FIVE_ROOT3 = 250 + ONE_TWO_FIVE_ROOT3
    Public Const FIVE_HUNDRED As Single = 500
    Public Const TWO_FIFTY As Single = 250
    Public Const ONE_TWO_FIVE As Single = 125
    Public Const COMMA As String = ","
    Public Const SEMICOLON As String = ";"
    Public Const THIRTY_THOUSAND As Integer = 30000
    Public ReadOnly HALF_ROOT3 As Single = Sqrt(3) / 2
    Public Const DEFAULT_STRING As String = "Default"
    Public Const UNDERLINE As String = "_"
    Public Const SPACE_STRING As String = " "
    Public Const ESCAPE_VERTICAL_LINE As String = "\|"
    Public Const TYPE_THREE_IMAGES As String = "three"
    Public Const SKIRMISH_FRAGMENT_DOMAIN As String = "skirmish_unit"
    Public Const CONTROL_IMAGE_DOMAIN As String = "control_image"
    Public Const SKIRMISH_IMAGE_GROUP As String = "skirmish_chess"
    Public Const NOT_TRANSPARENT As Single = 1.0F
    Public Const UNIT_DETAIL_HV_RATIO As Single = 1.618
    Public ReadOnly ROOT_FIVE As Single = Sqrt(5)
    Public ReadOnly ROOT_THREE As Single = Sqrt(3)
    Public Const ONE_THIRD As Single = 1 / 3
    Public Const TERRAIN_COST_MAX As Single = 999.9F


    Public SystemDefaultFontFamilyName As String = "Segoe UI"
    ''' <summary>
    ''' 阵营颜色预设列表
    ''' </summary>
    Public SIDE_COLOUR As List(Of SolidColorBrushSet) = Nothing
    ''' <summary>
    ''' 由透明到不透明的黑色SolidBrush，索引为0到4
    ''' </summary>
    Public BLACK_COLOUR_BRUSH As List(Of SolidColorBrush)
    ''' <summary>
    ''' 由透明到不透明的白色SolidBrush，索引为0到4
    ''' </summary>
    Public WHITE_COLOUR_BRUSH As List(Of SolidColorBrush)
    Public PURE_BLACK_BRUSH As SolidColorBrush
    Public PURE_WHITE_BRUSH As SolidColorBrush
    Public GREY_WHITE_BRUSH As SolidColorBrush


    Public WHITE_COLOUR As New RawColor4(1, 1, 1, 1)
    Public TRANSPARENT_BRUSH As SolidColorBrush = Nothing

    Public NORMAL_BITMAP_PROPERTY As BitmapProperties1 = New BitmapProperties1() With {
        .PixelFormat = New SharpDX.Direct2D1.PixelFormat(SharpDX.DXGI.Format.B8G8R8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied),
        .BitmapOptions = BitmapOptions.Target}

    Public MAP_CPU_BITMAP_PROPERTY As BitmapProperties1 = New BitmapProperties1() With {
        .PixelFormat = New SharpDX.Direct2D1.PixelFormat(SharpDX.DXGI.Format.B8G8R8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied),
        .BitmapOptions = BitmapOptions.CannotDraw Or BitmapOptions.CpuRead}

    Public NORMAL_BRUSH_PROPERTY As New BrushProperties() With {
        .Opacity = NOT_TRANSPARENT,
        .Transform = New RawMatrix3x2 With {
                .M11 = 1,
                .M12 = 0,
                .M21 = 0,
                .M22 = 1,
                .M31 = 0,
                .M32 = 0}}

    Public ReadOnly ALL_GAME_STAGES As New List(Of SingleGameLoopStage) From {0, 1, 2, 3, 4, 5}

    Public DEFAULT_BEZIER As BezierPenCurve = Nothing


    ''' <summary>
    ''' 加载所有全局资源，应当异步调用此方法
    ''' </summary>
    Public Sub LoadResources(context As SharpDX.Direct2D1.DeviceContext)


        Dim bitmap_hex_grass As Bitmap1 = LoadBitmapUsingWIC(context, Application.StartupPath & "\Resources\Images\Map\hex_grass.png")
        Dim bitmap_hex_forest As Bitmap1 = LoadBitmapUsingWIC(context, Application.StartupPath & "\Resources\Images\Map\hex_forest.png")
        Dim bitmap_hex_mountain As Bitmap1 = LoadBitmapUsingWIC(context, Application.StartupPath & "\Resources\Images\Map\hex_mountain.png")

        With TERRAIN_BITMAP
            .Add(Nothing) 'none
            .Add(Nothing)
            .Add(bitmap_hex_mountain)
            .Add(bitmap_hex_forest)
            .Add(Nothing)
            .Add(bitmap_hex_grass)
        End With

        With TERRAIN_BASECOLOUR
            .Add(Nothing) 'none
            .Add(Nothing)
            .Add(New SolidColorBrush(context, New RawColor4(114 / 255, 114 / 255, 101 / 255, 1)))
            .Add(New SolidColorBrush(context, New RawColor4(52 / 255, 110 / 255, 65 / 255, 1)))
            .Add(Nothing)
            .Add(New SolidColorBrush(context, New RawColor4(90 / 255, 149 / 255, 103 / 255, 1)))
        End With

        Dim bitmap_tree2 As Bitmap1 = LoadBitmapUsingWIC(context, Application.StartupPath & "\Resources\Images\Map\tree2.png")
        ACCESSORY_TREE.Add(bitmap_tree2)

        DEFAULT_BEZIER = New BezierPenCurve
        Dim tmpBezPoints(1) As ThreePointF2
        tmpBezPoints(0).Left = New PointF2(-0.2, -0.2)
        tmpBezPoints(0).Middle = New PointF2(0, 0)
        tmpBezPoints(0).Right = New PointF2(0.2, 0.2)
        tmpBezPoints(1).Left = New PointF2(0.8, 0.8)
        tmpBezPoints(1).Middle = New PointF2(1, 1)
        tmpBezPoints(1).Right = New PointF2(1.2, 1.2)
        DEFAULT_BEZIER.Anchor = tmpBezPoints

        Dim unitTemplatesString As String = My.Resources.UnitTemplates
        UnitTemplates.LoadTemplates(unitTemplatesString)

        '载入碎片图像
        FragmentImages.LoadImages(context)
        '载入单位立绘
        UnitImages.LoadFromFiles(context)
        '载入图标
        GameIcons.LoadFromFiles(context)
        '载入动态图像
        Live2dImages.LoadSpritesFromFile(context)
        Live2dImages.LoadLiveConfigFromFiles()
        '载入字体助手
        'GameFontHelper.LoadTextFromFiles()
        GameFontHelper2 = FontHelper2.Instance    'Initialize

        Object3DLoaderInstance.ReadAll()

        SIDE_COLOUR = SolidColorBrushSet.LoadFromXml(context, My.Resources.Colours)

        BLACK_COLOUR_BRUSH = New List(Of SolidColorBrush)
        With BLACK_COLOUR_BRUSH
            .Add(New SolidColorBrush(context, New RawColor4(0, 0, 0, 0.5 * ONE_THIRD)))
            .Add(New SolidColorBrush(context, New RawColor4(0, 0, 0, ONE_THIRD)))
            .Add(New SolidColorBrush(context, New RawColor4(0, 0, 0, 0.5)))
            .Add(New SolidColorBrush(context, New RawColor4(0, 0, 0, 0.5 + 0.5 * ONE_THIRD)))
            .Add(New SolidColorBrush(context, New RawColor4(0, 0, 0, 0.5 + ONE_THIRD)))
        End With
        WHITE_COLOUR_BRUSH = New List(Of SolidColorBrush)
        With WHITE_COLOUR_BRUSH
            .Add(New SolidColorBrush(context, New RawColor4(1, 1, 1, 0.5 * ONE_THIRD)))
            .Add(New SolidColorBrush(context, New RawColor4(1, 1, 1, ONE_THIRD)))
            .Add(New SolidColorBrush(context, New RawColor4(1, 1, 1, 0.5)))
            .Add(New SolidColorBrush(context, New RawColor4(1, 1, 1, 0.5 + 0.5 * ONE_THIRD)))
            .Add(New SolidColorBrush(context, New RawColor4(1, 1, 1, 0.5 + ONE_THIRD)))
        End With
        TRANSPARENT_BRUSH = New SolidColorBrush(context, New RawColor4(1, 1, 1, 0))
        PURE_BLACK_BRUSH = New SolidColorBrush(context, New RawColor4(0, 0, 0, 1))
        PURE_WHITE_BRUSH = New SolidColorBrush(context, New RawColor4(1, 1, 1, 1))
        GREY_WHITE_BRUSH = New SolidColorBrush(context, New RawColor4(0.8, 0.8, 0.8, 1))

        'GameFontHelper.AddFontFile(Application.StartupPath & "\P104_Font1.ttf", "P104_Font1")

        SkirmishTerrain.Initialize()


    End Sub

    ''' <summary>
    ''' 将system.drawing.bitmap转换为d2dbitmap
    ''' </summary>
    <Obsolete("改用wic", False)>
    Public Function LoadBitmap(rt As RenderTarget, drawingBitmap As System.Drawing.Bitmap) As Bitmap
        Dim result As Bitmap = Nothing
        Dim drawingBitmapData As BitmapData = drawingBitmap.LockBits(New Rectangle(0, 0, drawingBitmap.Width, drawingBitmap.Height), ImageLockMode.ReadOnly, Imaging.PixelFormat.Format32bppPArgb)
        Dim dataStreamxx As DataStream = New DataStream(drawingBitmapData.Scan0, drawingBitmapData.Stride * drawingBitmapData.Height, True, False)
        Dim properties As Direct2D1.BitmapProperties = New Direct2D1.BitmapProperties()
        properties.PixelFormat = New Direct2D1.PixelFormat(DXGI.Format.B8G8R8A8_UNorm, Direct2D1.AlphaMode.Premultiplied)
        result = New Direct2D1.Bitmap(rt, New Size2(drawingBitmap.Width, drawingBitmap.Height), dataStreamxx, drawingBitmapData.Stride, properties)
        drawingBitmap.UnlockBits(drawingBitmapData)
        dataStreamxx.Dispose()
        Return result
    End Function

    ''' <summary>
    ''' 使用WIC载入图片资源
    ''' </summary>
    ''' <param name="context">d2dContext对象</param>
    ''' <param name="filePath">图片路径</param>
    Public Function LoadBitmapUsingWIC(context As DeviceContext, filePath As String) As Bitmap1
        Dim fileStream As NativeFileStream = New NativeFileStream(filePath, NativeFileMode.Open, NativeFileAccess.Read)
        Dim bitmapDecoder As WIC.BitmapDecoder = New WIC.BitmapDecoder(GameImagingFactory, fileStream, WIC.DecodeOptions.CacheOnDemand)
        Dim frame As WIC.BitmapFrameDecode = bitmapDecoder.GetFrame(0)
        Dim converter As WIC.FormatConverter = New WIC.FormatConverter(GameImagingFactory)
        converter.Initialize(frame, SharpDX.WIC.PixelFormat.Format32bppPRGBA)
        Dim newBitmap As Bitmap1 = SharpDX.Direct2D1.Bitmap1.FromWicBitmap(context, converter)
        fileStream.Dispose()
        bitmapDecoder.Dispose()
        frame.Dispose()
        converter.Dispose()
        Return newBitmap
    End Function

    ''' <summary>
    ''' 使用WIC载入图片资源
    ''' </summary>
    ''' <param name="context">d2dContext对象</param>
    ''' <param name="readStream">文件流</param>
    Public Function LoadBitmapUsingWIC(context As DeviceContext, readStream As System.IO.Stream) As Bitmap1
        Dim bitmapDecoder As WIC.BitmapDecoder = New WIC.BitmapDecoder(GameImagingFactory, readStream, WIC.DecodeOptions.CacheOnDemand)
        Dim frame As WIC.BitmapFrameDecode = bitmapDecoder.GetFrame(0)
        Dim Converter As WIC.FormatConverter = New WIC.FormatConverter(GameImagingFactory)
        Converter.Initialize(frame, SharpDX.WIC.PixelFormat.Format32bppPRGBA)
        Dim newBitmap As Bitmap1 = SharpDX.Direct2D1.Bitmap1.FromWicBitmap(context, Converter)
        bitmapDecoder.Dispose()
        frame.Dispose()
        Converter.Dispose()
        Return newBitmap
    End Function

    Public Function GetCampaignScript(index As Short) As String
        If index = 0 Then
            Return My.Resources.MapScriptTest
        End If
        Return vbNullString
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

    Public Shared Function LoadFromXml(rt As SharpDX.Direct2D1.DeviceContext, xml As String) As List(Of SolidColorBrushSet)
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

