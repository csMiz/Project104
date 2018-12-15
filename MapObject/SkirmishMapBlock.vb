Imports SharpDX.Direct2D1
Imports SharpDX.Mathematics.Interop

''' <summary>
''' 单个六边形地图块类
''' </summary>
Public Class SkirmishMapBlock
    Public X As Short
    Public Y As Short

    Public Altitude As Short = 0
    Public Terrain As TerrainType
    Private Accessories As New List(Of SkirmishMapBlockAccessory)
    Private AccessoryImage As Bitmap1
    Private Shared AccessoryCompare As New Comparison(Of SkirmishMapBlockAccessory)(AddressOf CompareAccessory)

    Public World_V_O As New PointF2
    Public V_O As New RawVector2
    Public V_L As New RawVector2
    Public V_BL As New RawVector2
    Public V_BR As New RawVector2
    Public V_R As New RawVector2
    Public V_TL As New RawVector2
    Public V_TR As New RawVector2
    Public V3D_L As New RawVector2
    Public V3D_BL As New RawVector2
    Public V3D_BR As New RawVector2
    Public V3D_R As New RawVector2
    Public V3D_TL As New RawVector2
    Public V3D_TR As New RawVector2
    ''' <summary>
    ''' 贴图左端
    ''' </summary>
    Public ImgLeft As Single
    ''' <summary>
    ''' 贴图顶端
    ''' </summary>
    Public ImgTop As Single
    Public ImgSideLength As Single
    Public DrawingBox As RawRectangleF
    Public Outline As New PolygonArea

    Public DrawingBaseStartPoint As RawVector2
    Public DrawingBasePoints As New List(Of RawVector2)
    Public DrawingBaseBox As RawRectangleF
    Public BaseGeometry As PathGeometry = Nothing
    Public BaseSink As GeometrySink = Nothing

    Public Sub New()
        ReDim Me.Outline.PointsRaw(7)
    End Sub

    Public Sub InitializeWorldPosition()
        World_V_O.X = X * THREE_SEVEN_FIVE + TWO_FIFTY
        World_V_O.Y = Y * TWO_FIFTY_ROOT3 + ONE_TWO_FIVE_ROOT3 + ONE_TWO_FIVE_ROOT3 * (X Mod 2)
    End Sub

    Public Sub UpdateVertices(focus As PointF2, resolve As PointI, zoom As Single)
        With Me
            Dim topLeft As New PointF2(focus.X - 0.5 * resolve.X / zoom, focus.Y - 0.5 * resolve.Y / zoom)

            .ImgSideLength = FIVE_HUNDRED * zoom
            .V_O.X = (.World_V_O.X - topLeft.X) * zoom
            .V_O.Y = (.World_V_O.Y - topLeft.Y) * zoom

            Dim halfSide As Single = 0.5 * .ImgSideLength
            Dim fake3dHorizontalOffset As Single = Altitude * 0.01 * (.World_V_O.X - focus.X) * zoom
            Dim fake3dVerticalOffset As Single = Altitude * 0.03 * (.World_V_O.Y - focus.Y - 3000) * zoom

            .ImgLeft = .V_O.X + fake3dHorizontalOffset - halfSide
            .ImgTop = .V_O.Y + fake3dVerticalOffset - halfSide

            .V_L.X = .V_O.X - halfSide
            .V_L.Y = .V_O.Y
            .V3D_L.X = .V_L.X + fake3dHorizontalOffset
            .V3D_L.Y = .V_L.Y + fake3dVerticalOffset

            .V_BL.X = .V_O.X - ONE_TWO_FIVE * zoom
            .V_BL.Y = .V_O.Y + ONE_TWO_FIVE_ROOT3 * zoom
            .V3D_BL.X = .V_BL.X + fake3dHorizontalOffset
            .V3D_BL.Y = .V_BL.Y + fake3dVerticalOffset

            .V_BR.X = .V_O.X + ONE_TWO_FIVE * zoom
            .V_BR.Y = .V_BL.Y
            .V3D_BR.X = .V_BR.X + fake3dHorizontalOffset
            .V3D_BR.Y = .V_BR.Y + fake3dVerticalOffset

            .V_R.X = .V_O.X + halfSide
            .V_R.Y = .V_O.Y
            .V3D_R.X = .V_R.X + fake3dHorizontalOffset
            .V3D_R.Y = .V_R.Y + fake3dVerticalOffset

            .V_TL.X = .V_BL.X
            .V_TL.Y = .V_O.Y - ONE_TWO_FIVE_ROOT3 * zoom
            .V3D_TL.X = .V_TL.X + fake3dHorizontalOffset
            .V3D_TL.Y = .V_TL.Y + fake3dVerticalOffset

            .V_TR.X = .V_BR.X
            .V_TR.Y = .V_TL.Y
            .V3D_TR.X = .V_TR.X + fake3dHorizontalOffset
            .V3D_TR.Y = .V_TR.Y + fake3dVerticalOffset

            If Math.Abs(fake3dVerticalOffset) >= ROOT_THREE * Math.Abs(fake3dHorizontalOffset) Then
                If .Altitude Then
                    .DrawingBaseStartPoint = .V_L
                    With .DrawingBasePoints
                        .Clear()
                        .Add(V_BL)
                        .Add(V_BR)
                        .Add(V_R)
                        .Add(V3D_R)
                        .Add(V3D_BR)
                        .Add(V3D_BL)
                        .Add(V3D_L)
                    End With
                End If
                With .Outline
                    .PointsRaw(0) = V3D_TL
                    .PointsRaw(1) = V3D_L
                    .PointsRaw(2) = V_L
                    .PointsRaw(3) = V_BL
                    .PointsRaw(4) = V_BR
                    .PointsRaw(5) = V_R
                    .PointsRaw(6) = V3D_R
                    .PointsRaw(7) = V3D_TR
                End With
            ElseIf fake3dHorizontalOffset > 0 Then
                If .Altitude Then
                    .DrawingBaseStartPoint = .V_TL
                    With .DrawingBasePoints
                        .Clear()
                        .Add(V_L)
                        .Add(V_BL)
                        .Add(V_BR)
                        .Add(V3D_BR)
                        .Add(V3D_BL)
                        .Add(V3D_L)
                        .Add(V3D_TL)
                    End With
                End If
                With .Outline
                    .PointsRaw(0) = V3D_TL
                    .PointsRaw(1) = V_TL
                    .PointsRaw(2) = V_L
                    .PointsRaw(3) = V_BL
                    .PointsRaw(4) = V_BR
                    .PointsRaw(5) = V3D_BR
                    .PointsRaw(6) = V3D_R
                    .PointsRaw(7) = V3D_TR
                End With
            Else
                If .Altitude Then
                    .DrawingBaseStartPoint = .V_TR
                    With .DrawingBasePoints
                        .Clear()
                        .Add(V_R)
                        .Add(V_BR)
                        .Add(V_BL)
                        .Add(V3D_BL)
                        .Add(V3D_BR)
                        .Add(V3D_R)
                        .Add(V3D_TR)
                    End With
                End If
                With .Outline
                    .PointsRaw(0) = V3D_TR
                    .PointsRaw(1) = V_TR
                    .PointsRaw(2) = V_R
                    .PointsRaw(3) = V_BR
                    .PointsRaw(4) = V_BL
                    .PointsRaw(5) = V3D_BL
                    .PointsRaw(6) = V3D_L
                    .PointsRaw(7) = V3D_TL
                End With
            End If

            .DrawingBox = New RawRectangleF(.ImgLeft, .ImgTop, .ImgLeft + .ImgSideLength, .ImgTop + .ImgSideLength)
            .DrawingBaseBox = New RawRectangleF(.ImgLeft, .ImgTop - 0.5 * .ImgSideLength, .DrawingBox.Right, .DrawingBox.Bottom)

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
            If Me.Altitude Then
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
                sink.Dispose()
                geometry.Dispose()
            End If
            '画六角地图
            .DrawBitmap(TERRAIN_BITMAP(Me.Terrain), Me.DrawingBox, NOT_TRANSPARENT, BitmapInterpolationMode.Linear)
            '画装饰物
            .DrawBitmap(AccessoryImage, Me.DrawingBaseBox, NOT_TRANSPARENT, BitmapInterpolationMode.Linear)


        End With

    End Sub

End Class


