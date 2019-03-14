' -----------------------------------------
' Copyright (c) 2019 All Rights Reserved.
' 
' Filename: SkirmishMap2
' Author: Miz
' Date: 2019/2/20 9:46:26
' -----------------------------------------

Imports System.IO
''' <summary>
''' 遭遇战地图类ver2
''' </summary>
Public Class SkirmishMap2

    Public Blocks(49, 49) As List(Of SkirmishMapBlock2)

    Public MapSizeXMax As Short = 49

    Public MapSizeYMax As Short = 49

    Public ResourcesLoaded As Boolean = False

    'TODO: revise-> Public MovementGlance As New List(Of MovementGlanceNode)

    ''' <summary>
    ''' 载入地图二进制文件并销毁stream对象
    ''' </summary>
    ''' <param name="stream">文件流</param>
    Public Sub LoadFromFile(ByRef stream As FileStream, context As SharpDX.Direct2D1.DeviceContext)
        Dim content() As Byte
        Using br As BinaryReader = New BinaryReader(stream)
            Dim length As Integer = stream.Length
            content = br.ReadBytes(length)
        End Using
        stream.Close()
        stream.Dispose()

        For j = 0 To 49
            For i = 0 To 49
                Blocks(i, j) = New List(Of SkirmishMapBlock2)
            Next
        Next

        Dim cur As Integer = 0
        Dim lastX As Integer = -1, lastY As Integer = -1
        Dim XYSameCounter As Integer = 0
        Do While cur < content.Length
            Dim tmpBlock As New SkirmishMapBlock2
            With tmpBlock
                .X = content(cur)
                .Y = content(cur + 1)
                .Terrain = content(cur + 2)
                .AltitudeBottom = content(cur + 3) \ 16
                .AltitudeTop = content(cur + 3) Mod 16
                '.GeneratedId = counter
                .GeneratedId = .X * 500 + .Y * 10 + XYSameCounter

                .GenerateModel(context)    'must load template models in advance

            End With
            Blocks(tmpBlock.X, tmpBlock.Y).Add(tmpBlock)
            cur += 4
            If tmpBlock.X = lastX AndAlso tmpBlock.Y = lastY Then
                XYSameCounter += 1
            Else
                XYSameCounter = 0
            End If
            lastX = tmpBlock.X
            lastY = tmpBlock.Y
        Loop

        content = Nothing
    End Sub

    ''' <summary>
    ''' 将地图块模型注册到摄像机世界容器
    ''' </summary>
    ''' <param name="camera">摄像机对象</param>
    Public Sub Register3DObjects(camera As GameCamera3D)
        Dim container As List(Of Game3DObject2) = camera.WorldContainer
        container.Clear()
        For j = 0 To MapSizeYMax
            For i = 0 To MapSizeXMax
                If Blocks(i, j).Count Then
                    For Each tmpBlock As SkirmishMapBlock2 In Blocks(i, j)
                        container.Add(tmpBlock.Model)
                    Next
                End If
            Next
        Next
    End Sub

    ''' <summary>
    ''' 绘制遭遇战地图
    ''' <para>
    ''' 不再使用DrawHexMap委托
    ''' 绘制过程已移到GameCamera3D
    ''' </para>
    ''' </summary>
    <Obsolete("不再使用DrawHexMap委托，绘制过程已移到GameCamera3D", True)>
    Public Sub DrawHexMap(ByRef context As SharpDX.Direct2D1.DeviceContext, ByRef spectator As SpectatorCamera, canvasBitmap As SharpDX.Direct2D1.Bitmap1)
    End Sub

    Public Function GetBlock(blockId As Integer) As SkirmishMapBlock2
        Dim tmpX As Integer = blockId \ 500
        Dim tmpY As Integer = (blockId Mod 500) \ 10
        Dim tmpIndex As Integer = (blockId Mod 500) Mod 10
        Return Blocks(0, 0)(0)
        'Return Blocks(tmpX, tmpY)(tmpIndex)
    End Function

End Class
