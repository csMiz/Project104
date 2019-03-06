' -----------------------------------------
' Copyright (c) 2019 All Rights Reserved.
' 
' Filename: Game3DObject2
' Author: Miz
' Date: 2019/3/2 22:42:05
' -----------------------------------------

Imports SharpDX.Mathematics.Interop

''' <summary>
''' Game3DObject ver1.2
''' <para>Fit the interface of '*.pob'</para>
''' </summary>
Public Class Game3DObject2

    Public Faces As Game3dFace2_1()

    ''' <summary>
    ''' 用于检测是否处于视野外以避免多余绘制的顶点
    ''' </summary>
    Public RegionCheckSign As PointF3()

    Public ShaderIndex As Integer = 0

    Public Tag As Integer = 0

    ''' <summary>
    ''' 返回复制各顶点的新模型
    ''' </summary>
    Public Function CopyTemplate() As Game3DObject2
        Dim result As New Game3DObject2
        Dim faceArray(Me.Faces.Length - 1) As Game3dFace2_1
        For i = 0 To Me.Faces.Length - 1
            faceArray(i) = New Game3dFace2_1
            With faceArray(i)
                .Colour = Me.Faces(i).Colour
                ReDim .Normal(2)
                For j = 0 To 2
                    .Normal(j) = Me.Faces(i).Normal(j)
                Next
                ReDim .Vertices(2)
                For j = 0 To 2
                    .Vertices(j) = Me.Faces(i).Vertices(j)
                Next
            End With
        Next
        result.Faces = faceArray
        result.RegionCheckSign = Me.RegionCheckSign
        Return result
    End Function

End Class

Public Class Game3dFace2_1

    Public Vertices As RawVector3()

    Public Normal As RawVector3()

    Public Colour As RawColor4

    Public Tag As Integer = 0

End Class


Public Class Game3DFace2_1Bundle

    Public ShaderIndex As Integer = 0

    Public Faces As New List(Of Game3dFace2_1)

    Public Buffer As SharpDX.DataStream = Nothing

    Public Sub Dispose()
        If Buffer IsNot Nothing Then Buffer.Dispose()
    End Sub

End Class
