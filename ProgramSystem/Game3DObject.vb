' -----------------------------------------
' Copyright (c) 2019 All Rights Reserved.
' 
' Filename: Game3DObject
' Author: Miz
' Date: 2019/2/14 14:27:34
' -----------------------------------------

Imports SharpDX.Direct2D1
Imports SharpDX.Mathematics.Interop

Public Class Game3dObject

    Public Faces As Game3dFace()

    Public Texture As Game3dObjectTexture()

    Public Location_World As PointF3 = New PointF3(0, 0, 0)

    Public Rotation_World As PointF3 = New PointF3(0, 0, 0)

    Public Scale_World As PointF3 = New PointF3(1, 1, 1)
    ''' <summary>
    ''' 用于检测是否处于视野外以避免多余绘制的顶点
    ''' </summary>
    Public RegionCheckSign As PointF3()

    ''' <summary>
    ''' 返回复制各顶点的新模型，但材质库仍为指向材质库的指针
    ''' </summary>
    Public Function CopyTemplate() As Game3dObject
        Dim result As New Game3dObject
        Dim faceArray(Me.Faces.Length - 1) As Game3dFace
        For i = 0 To Me.Faces.Length - 1
            faceArray(i) = New Game3dFace
            With faceArray(i)
                .Colour = Me.Faces(i).Colour
                .FaceType = Me.Faces(i).FaceType
                .Normal = Me.Faces(i).Normal
                .TextureIndex = Me.Faces(i).TextureIndex
                .TextureVertices = Me.Faces(i).TextureVertices
                Dim tmpVertices(Me.Faces(i).Vertices.Length - 1) As PointF3
                For j = 0 To Me.Faces(i).Vertices.Length - 1
                    tmpVertices(j) = New PointF3(Me.Faces(i).Vertices(j))
                Next
                .Vertices = tmpVertices
            End With
        Next
        result.Faces = faceArray
        result.Texture = Me.Texture
        result.Location_World = Me.Location_World
        result.Rotation_World = Me.Rotation_World
        result.Scale_World = Me.Scale_World
        result.RegionCheckSign = Me.RegionCheckSign
        Return result
    End Function


End Class

Public Class Game3dObjectTexture

    Public FilenameIndex As String = vbNullString

    Public Image As Bitmap1 = Nothing

End Class


Public Class Game3dFace

    Public FaceType As FaceType3D

    Public Vertices As PointF3()

    Public Normal As PointF3

    Public Colour As RawColor4

    Public TextureIndex As Integer

    Public TextureVertices As PointF2()

    Public FixedTexture As Bitmap1 = Nothing
    Public FixedTextureVertices As PointF2()

    Public Sub ApplyTexture(context As DeviceContext)
        Return    'testing

        Dim textureLeft As Integer = 9999
        Dim textureRight As Integer = 0
        Dim textureTop As Integer = 9999
        Dim textureBottom As Integer = 0
        For Each tmpVertex As PointF2 In TextureVertices
            If Math.Floor(tmpVertex.X) < textureLeft Then
                textureLeft = Math.Floor(tmpVertex.X)
            End If
            If Math.Ceiling(tmpVertex.X) > textureRight Then
                textureRight = Math.Ceiling(tmpVertex.X)
            End If
            If Math.Floor(tmpVertex.Y) < textureTop Then
                textureTop = Math.Floor(tmpVertex.Y)
            End If
            If Math.Ceiling(tmpVertex.Y) > textureBottom Then
                textureBottom = Math.Ceiling(tmpVertex.Y)
            End If
        Next
        If FixedTexture IsNot Nothing Then FixedTexture.Dispose()
        FixedTexture = New Bitmap1(context, New SharpDX.Size2(textureRight - textureLeft, textureBottom - textureTop), NORMAL_BITMAP_PROPERTY)
        Dim tmpArray(TextureVertices.Length - 1) As PointF2
        For i = 0 To TextureVertices.Length - 1
            tmpArray(i) = New PointF2(TextureVertices(i).X - textureLeft, TextureVertices(i).Y - textureTop)
        Next
        FixedTextureVertices = tmpArray
        With context

            Dim geo As New PathGeometry(.Factory)
            Dim sink As GeometrySink = geo.Open
            With sink
                .SetFillMode(FillMode.Winding)
                .BeginFigure(MathHelper.PointF22RawVec(tmpArray(0)), FigureBegin.Filled)
                If FaceType = FaceType3D.Three Then
                    .AddLines({MathHelper.PointF22RawVec(tmpArray(1)), MathHelper.PointF22RawVec(tmpArray(2))})
                Else
                    .AddLines({MathHelper.PointF22RawVec(tmpArray(1)), MathHelper.PointF22RawVec(tmpArray(2)), MathHelper.PointF22RawVec(tmpArray(3))})
                End If
                .EndFigure(FigureEnd.Closed)
                .Close()
            End With

            Dim sourceImage As Bitmap1 = Object3DLoaderInstance.GetTexture(Me.TextureIndex)
            Dim ibProperty As New ImageBrushProperties
            With ibProperty
                .ExtendModeX = ExtendMode.Wrap
                .ExtendModeY = ExtendMode.Wrap
                .InterpolationMode = InterpolationMode.Linear
                .SourceRectangle = New RawRectangleF(0, 0, sourceImage.Size.Width, sourceImage.Size.Height)
            End With
            Dim imgBrush As New ImageBrush(context, sourceImage, ibProperty)
            Dim transMat As New RawMatrix3x2()
            With transMat
                .M11 = 1
                .M12 = 0
                .M21 = 0
                .M22 = 1
                .M31 = -textureLeft
                .M32 = -textureTop
            End With
            imgBrush.Transform = transMat

            .Target = FixedTexture
            .BeginDraw()
            .FillGeometry(geo, imgBrush)
            .EndDraw()

            sink.Dispose()
            geo.Dispose()
            imgBrush.Dispose()
        End With

    End Sub

End Class

Public Enum FaceType3D
    Three = 3
    Four = 4
End Enum