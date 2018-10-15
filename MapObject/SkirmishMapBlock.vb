Imports SharpDX.Direct2D1
Imports SharpDX.Mathematics.Interop

''' <summary>
''' 单个六边形地图块类
''' </summary>
Public Class SkirmishMapBlock
    Public Property X As Short
    Public Property Y As Short

    Public Property Altitude As Short = 0
    Public Property Terrain As TerrainType
    Private Accessories As New List(Of SkirmishMapBlockAccessory)
    Private AccessoryImage As Bitmap1
    Private Shared AccessoryCompare As New Comparison(Of SkirmishMapBlockAccessory)(AddressOf CompareAccessory)

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
    Public ImgSideLength As Single

    Public DrawingBaseStartPoint As RawVector2
    Public DrawingBasePoints As New List(Of RawVector2)

    Public Sub UpdateVertices(focus As PointF2, resolve As PointI, zoom As Single)
        ImgSideLength = FIVE_HUNDRED * zoom
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
        With DrawingBasePoints
            .Clear()
            .Add(V_BL)
            .Add(V_BR)
            .Add(V_R)
            .Add(V3D_R)
            .Add(V3D_BR)
            .Add(V3D_BL)
            .Add(V3D_L)
        End With

    End Sub

    Public Sub InitializeAccessories(context As DeviceContext, zoom As Single)
        Dim SideLength As Single = FIVE_HUNDRED * zoom
        Accessories.Clear()
        AccessoryImage = New Bitmap1(context, New SharpDX.Size2(SideLength, 1.5 * SideLength), NORMAL_BITMAP_PROPERTY)
        Dim center As New PointF2(0.5 * SideLength, SideLength)
        Dim accessoryImageSize As Single = 50
        If Me.Terrain = TerrainType.Forest Then
            Dim treeCount As Short = CInt(MathHelper.GetRandom * 4 + 6)
            For i = 0 To treeCount - 1
                Dim distanceToCenter As Single = MathHelper.GetRandom * 0.5 * SideLength * HALF_ROOT3
                Dim angle As Single = 2 * Math.PI * MathHelper.GetRandom
                Dim position As New PointF2(center.X + distanceToCenter * Math.Cos(angle), center.Y - distanceToCenter * Math.Sin(angle))
                Dim rect As New RawRectangleF(position.X - 0.5 * accessoryImageSize, position.Y - accessoryImageSize, position.X + 0.5 * accessoryImageSize, position.Y)
                Dim tmpAccessory As New SkirmishMapBlockAccessory(ACCESSORY_TREE(0), rect)
                Accessories.Add(tmpAccessory)
            Next
        End If

        Accessories.Sort(SkirmishMapBlock.AccessoryCompare)

        context.Target = AccessoryImage
        context.BeginDraw()
        For i = 0 To Accessories.Count - 1
            Accessories(i).PaintSourceImage(context)
        Next
        context.EndDraw()

    End Sub

    Private Shared Function CompareAccessory(a As SkirmishMapBlockAccessory, b As SkirmishMapBlockAccessory) As Single
        Return (a.GetSourceY - b.GetSourceY)
    End Function

    Public Sub PaintMapBlock(context As DeviceContext)
        With context
            '画基底
            Dim geometry As New PathGeometry(.Factory)
            Dim sink As GeometrySink = geometry.Open
            With sink
                .SetFillMode(FillMode.Winding)
                .BeginFigure(Me.DrawingBaseStartPoint, FigureBegin.Filled)
                Dim sinkPoints() As RawVector2 = Me.DrawingBasePoints.ToArray
                .AddLines(sinkPoints)
                .EndFigure(FigureEnd.Closed)
                .Close()
            End With
            .FillGeometry(geometry, TERRAIN_BASECOLOUR(Me.Terrain))
            '画六角地图
            Dim rect As New RawRectangleF(Me.ImgLeft, Me.ImgTop, Me.ImgLeft + ImgSideLength, Me.ImgTop + ImgSideLength)
            .DrawBitmap(TERRAIN_BITMAP(Me.Terrain), rect, NOT_TRANSPARENT, BitmapInterpolationMode.Linear)
            '画装饰物
            Dim rect2 As New RawRectangleF(rect.Left, rect.Top - 0.5 * ImgSideLength, rect.Right, rect.Bottom)
            .DrawBitmap(AccessoryImage, rect2, NOT_TRANSPARENT, BitmapInterpolationMode.Linear)

            sink.Dispose()
            geometry.Dispose()

        End With

    End Sub

End Class


