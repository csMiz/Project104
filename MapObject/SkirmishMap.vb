Imports System.IO
Imports SharpDX.Direct2D1
Imports System.Math
Imports SharpDX.Mathematics.Interop

Public Class SkirmishMap
    Private Blocks(49, 49) As SkirmishMapBlock
    Private Property MapSizeXMax As Short = 49
    Private Property MapSizeYMax As Short = 49
    Public Property SideCount As Short = 2
    Public Property NowTurn As Short = 0

    Public UnitList As New List(Of GameUnit)

    Public FlagList As New List(Of GameFlag)

    Public BuildingFlag As New List(Of GameBuilding)

    Private BlockLayers As New List(Of List(Of SkirmishMapBlock))

    Public Sub New()
        For i = 0 To 49
            For j = 0 To 49
                Blocks(i, j) = New SkirmishMapBlock
                With Blocks(i, j)
                    .X = i
                    .Y = j
                End With
            Next
        Next
    End Sub

    Public Sub LoadFromFile(stream As FileStream)
        Dim content() As Byte
        Using br As New BinaryReader(stream)
            content = br.ReadBytes(5000)
        End Using
        stream.Close()
        stream.Dispose()

        For i = 0 To 4998 Step 2
            Dim blockIndex As Short = i / 2
            With Blocks(blockIndex Mod 50, blockIndex \ 50)
                .Terrain = content(i)
            End With
        Next
        For i = 1 To 4999 Step 2
            Dim blockIndex As Short = (i - 1) / 2
            With Blocks(blockIndex Mod 50, blockIndex \ 50)
                .Altitude = content(i)
            End With
        Next
        content = Nothing

    End Sub

    ''' <summary>
    ''' 按高度分层
    ''' </summary>
    Private Sub DivideLayers(X1 As Short, X2 As Short, Y1 As Short, Y2 As Short)
        BlockLayers.Clear()
        For k = 0 To 9
            Dim list As New List(Of SkirmishMapBlock)
            For i = X1 To X2
                For j = Y1 To Y2
                    If i >= 0 AndAlso i <= MapSizeXMax AndAlso j >= 0 AndAlso j <= MapSizeYMax Then
                        Dim block As SkirmishMapBlock = Blocks(i, j)
                        If block.Altitude = k Then
                            list.Add(block)
                        End If
                    End If
                Next
            Next
            If CBool(list.Count) Then
                BlockLayers.Add(list)
            End If
        Next
    End Sub

    Public Sub DrawHexMap(ByRef renderTarget As WindowRenderTarget, ByRef spectator As SpectatorCamera)
        Dim brush1 As New SolidColorBrush(renderTarget, New RawColor4(1.0, 0.9, 0.2, 0.2))
        Dim cameraX As Single = spectator.CameraFocus.X
        Dim cameraY As Single = spectator.CameraFocus.Y
        Dim centreX As Single = spectator.Resolve.X / 2
        Dim centreY As Single = spectator.Resolve.Y / 2
        Dim zoom As Single = spectator.Zoom
        Dim bitmap_side_length As Single = FIVE_HUNDRED * zoom

        Dim drawRangeX As Short = Truncate((cameraX - SIX_TWO_FIVE * zoom) / (THREE_SEVEN_FIVE * zoom))
        Dim drawRangeY As Short = Truncate((cameraY - SIX_TWO_FIVE_ROOT3 * zoom) / (TWO_FIFTY_ROOT3 * zoom))
        DivideLayers(drawRangeX - 5, drawRangeX + 5, drawRangeY - 5, drawRangeY + 5)

        With renderTarget
            For i = 0 To BlockLayers.Count - 1
                Dim blockLayer As List(Of SkirmishMapBlock) = BlockLayers(i)
                For j = 0 To blockLayer.Count - 1
                    Dim block As SkirmishMapBlock = blockLayer(j)
                    Dim blockCentreX As Single = block.X * THREE_SEVEN_FIVE * zoom
                    Dim blockCentreY As Single = block.Y * TWO_FIFTY_ROOT3 * zoom + ONE_TWO_FIVE_ROOT3 * zoom * (block.X Mod 2)
                    '实现伪3d效果
                    Dim fake3dHorizontalOffset As Single = block.Altitude * 0.02 * (blockCentreX - cameraX)
                    Dim fake3dVerticalOffset As Single = block.Altitude * 0.08 * (blockCentreY - cameraY)
                    Dim rectLeft As Single = centreX - cameraX + blockCentreX + fake3dHorizontalOffset
                    Dim rectTop As Single = centreY - cameraY + blockCentreY + fake3dVerticalOffset

                    Dim rect As New RawRectangleF(rectLeft, rectTop, rectLeft + bitmap_side_length, rectTop + bitmap_side_length)
                    .DrawBitmap(TERRAIN_BITMAP(block.Terrain), rect, 1.0F, BitmapInterpolationMode.Linear)



                Next
            Next
            '.DrawEllipse(brush1, New Ellipse With {
            '             .Center = New PointF(spectator.Resolve.X / 2, spectator.Resolve.Y / 2),
            '             .RadiusX = 25,
            '             .RadiusY = 25})
            '.DrawBitmap(BITMAP_HEX_GRASS, New Rectangle(spectator.Resolve.X / 2 + drawRangeX * 500 * zoom, spectator.Resolve.Y / 2 - 33.5 * zoom, 500 * zoom, 500 * zoom))
        End With
    End Sub


End Class
