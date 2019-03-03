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
    Public Model As Game3DObject2 = Nothing

    ''' <summary>
    ''' 生成地图块模型
    ''' </summary>
    Public Sub GenerateModel(context As SharpDX.Direct2D1.DeviceContext)
        If Me.Model IsNot Nothing Then
            Me.Model = Nothing
        End If
        'generate object with X, Y, AltitudeB/T
        Me.Model = Object3DLoaderInstance.ObjectRepository2(0).CopyTemplate
        Dim pos As New PointF3(75 * X, 86.6 * Y + 43.3 * (X Mod 2), 19.1 * AltitudeBottom)
        Me.Model.RegionCheckSign = {pos}
        Dim scale_z As Integer = (AltitudeTop - AltitudeBottom + 1)
        For i = 0 To Me.Model.Faces.Length - 1
            For j = 0 To Me.Model.Faces(i).Vertices.Length - 1
                With Me.Model.Faces(i).Vertices(j)
                    .Z *= scale_z
                    .X += pos.X
                    .Y += pos.Y
                    .Z += pos.Z
                End With
            Next
        Next
        'bind texture
        'TODO: bind texture

    End Sub

End Class
