Imports SharpDX.Mathematics.Interop

''' <summary>
''' 单个六边形地图块类
''' </summary>
Public Class SkirmishMapBlock
    Public Property X As Short
    Public Property Y As Short

    Public Property Altitude As Short = 0
    Public Property Terrain As TerrainType

    Public V_O As New RawVector2
    Public V_L As New RawVector2
    Public V_BL As New RawVector2
    Public V_BR As New RawVector2
    Public V_R As New RawVector2
    Public V3D_L As New RawVector2
    Public V3D_BL As New RawVector2
    Public V3D_BR As New RawVector2
    Public V3D_R As New RawVector2
    ''' <summary>
    ''' 贴图左端
    ''' </summary>
    Public ImgLeft As Single
    ''' <summary>
    ''' 贴图顶端
    ''' </summary>
    Public ImgTop As Single

    Public DrawingBaseStartPoint As RawVector2
    Public DrawingBasePoints As New List(Of RawVector2)

    Public Sub UpdateVertices(focus As PointF2, resolve As PointI, zoom As Single)
        Dim halfSide As Single = TWO_FIFTY * zoom
        Dim blockCentreX As Single = X * THREE_SEVEN_FIVE * zoom + halfSide
        Dim blockCentreY As Single = Y * TWO_FIFTY_ROOT3 * zoom + ONE_TWO_FIVE_ROOT3 * zoom + ONE_TWO_FIVE_ROOT3 * zoom * (X Mod 2)
        Dim centreX As Single = resolve.X / 2
        Dim centreY As Single = resolve.Y / 2

        Dim fake3dHorizontalOffset As Single = Altitude * 0.01 * (blockCentreX - focus.X)
        Dim fake3dVerticalOffset As Single = Altitude * 0.015 * (blockCentreY - focus.Y - resolve.Y * 2)

        V_O.X = centreX - focus.X + blockCentreX
        V_O.Y = centreY - focus.Y + blockCentreY
        ImgLeft = V_O.X + fake3dHorizontalOffset - halfSide
        ImgTop = V_O.Y + fake3dVerticalOffset - halfSide

        V_L.X = V_O.X - halfSide
        V_L.Y = V_O.Y
        V3D_L.X = V_L.X + fake3dHorizontalOffset
        V3D_L.Y = V_L.Y + fake3dVerticalOffset

        V_BL.X = V_O.X - ONE_TWO_FIVE * zoom
        V_BL.Y = V_O.Y + ONE_TWO_FIVE_ROOT3 * zoom
        V3D_BL.X = V_BL.X + fake3dHorizontalOffset
        V3D_BL.Y = V_BL.Y + fake3dVerticalOffset

        V_BR.X = V_O.X + ONE_TWO_FIVE * zoom
        V_BR.Y = V_BL.Y
        V3D_BR.X = V_BR.X + fake3dHorizontalOffset
        V3D_BR.Y = V_BR.Y + fake3dVerticalOffset

        V_R.X = V_O.X + halfSide
        V_R.Y = V_O.Y
        V3D_R.X = V_R.X + fake3dHorizontalOffset
        V3D_R.Y = V_R.Y + fake3dVerticalOffset

        'Dim fake3dLPoint As New RawVector2(rectLeft, rectTop + 0.5 * bitmap_side_length)
        'Dim baseLPoint As New RawVector2(fake3dLPoint.X - fake3dHorizontalOffset, fake3dLPoint.Y - fake3dVerticalOffset)
        'Dim fake3dBLPoint As New RawVector2(rectLeft + 0.25 * bitmap_side_length, rectTop + TWO_FIFTY_PLUS_ONE_TWO_FIVE_ROOT3 * zoom)
        'Dim baseBLPoint As New RawVector2(fake3dBLPoint.X - fake3dHorizontalOffset, fake3dBLPoint.Y - fake3dVerticalOffset)

        DrawingBaseStartPoint = V_L
        DrawingBasePoints.Clear()
        DrawingBasePoints.Add(V_BL)
        DrawingBasePoints.Add(V_BR)
        DrawingBasePoints.Add(V_R)
        DrawingBasePoints.Add(V3D_R)
        DrawingBasePoints.Add(V3D_BR)
        DrawingBasePoints.Add(V3D_BL)
        DrawingBasePoints.Add(V3D_L)

    End Sub

End Class


