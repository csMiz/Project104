' -----------------------------------------
' Copyright (c) 2019 All Rights Reserved.
' 
' Filename: SkirmishMapBlock2
' Author: Miz
' Date: 2019/2/14 14:24:44
' -----------------------------------------

''' <summary>
''' 遭遇战地图块ver2类
''' </summary>
Public Class SkirmishMapBlock2

    Public X As Short

    Public Y As Short
    ''' <summary>
    ''' 高度下限
    ''' </summary>
    Public AltitudeBottom As Short = 0
    ''' <summary>
    ''' 高度上限
    ''' </summary>
    Public AltitudeTop As Short = 0

    Public Terrain As TerrainType
    ''' <summary>
    ''' 地图块模型
    ''' </summary>
    Public Model As Game3dObject = Nothing

    ''' <summary>
    ''' 生成地图块模型
    ''' </summary>
    Public Sub GenerateModel(context As SharpDX.Direct2D1.DeviceContext)
        If Me.Model IsNot Nothing Then
            Me.Model = Nothing
        End If
        'generate object with X, Y, AltitudeB/T
        Me.Model = Object3DLoaderInstance.ObjectRepository(0).CopyTemplate
        Dim pos As New PointF3(150 * X, 173.2 * Y + 86.6 * (X Mod 2), 80 * AltitudeBottom)
        Me.Model.RegionCheckSign = {pos}
        Dim scale_z As Integer = (AltitudeTop - AltitudeBottom + 1)
        For i = 0 To Me.Model.Faces.Length - 1
            For j = 0 To Me.Model.Faces(i).Vertices.Length - 1
                'Dim tmpOldVtx As PointF3 = Me.Model.Faces(i).Vertices(j)
                'Dim tmpNewVtx As New PointF3(tmpOldVtx.X + pos.X, tmpOldVtx.Y + pos.Y, tmpOldVtx.Z * scale_z + pos.Z)
                'Me.Model.Faces(i).Vertices(j) = tmpNewVtx
                With Me.Model.Faces(i).Vertices(j)
                    .Z *= scale_z
                    .X += pos.X
                    .Y += pos.Y
                    .Z += pos.Z
                End With
            Next
        Next
        For i = 8 To 9
            If Me.Terrain = TerrainType.Grass Then
                Me.Model.Faces(i).TextureIndex = 1
                Me.Model.Faces(i).ApplyTexture(context)

            End If
        Next


        'bind texture
        'TODO: bind texture

    End Sub

End Class
