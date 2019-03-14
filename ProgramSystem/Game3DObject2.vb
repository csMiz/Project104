' -----------------------------------------
' Copyright (c) 2019 All Rights Reserved.
' 
' Filename: Game3DObject2
' Author: Miz
' Date: 2019/3/2 22:42:05
' -----------------------------------------

Imports SharpDX
Imports SharpDX.Mathematics.Interop

''' <summary>
''' Game3DObject ver1.2
''' <para>Fit the interface of '*.pob'</para>
''' </summary>
Public Class Game3DObject2

    Public SourceFaces As Game3dFace2_1()
    ''' <summary>
    ''' 表示物体World内的变换，放入GPU计算
    ''' </summary>
    Public Position_Rotation_Scale As RawMatrix

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
        Dim faceArray(Me.SourceFaces.Length - 1) As Game3dFace2_1
        For i = 0 To Me.SourceFaces.Length - 1
            faceArray(i) = New Game3dFace2_1
            With faceArray(i)
                .Colour = Me.SourceFaces(i).Colour
                ReDim .Normal(2)
                For j = 0 To 2
                    .Normal(j) = Me.SourceFaces(i).Normal(j)
                Next
                ReDim .Vertices(2)
                For j = 0 To 2
                    .Vertices(j) = Me.SourceFaces(i).Vertices(j)
                Next
            End With
        Next
        result.SourceFaces = faceArray
        result.RegionCheckSign = Me.RegionCheckSign
        Return result
    End Function

    Public Sub CalculatePRS(pos As PointF3, rot As PointF3, scale As PointF3)
        Dim mat_pos As New MathMatrixS(3, 3)

        'TODO: CalculatePRS

    End Sub

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

    Public BufferFinished As Byte = 0    '0-null, 1-processing, 2-finished

    ''' <summary>
    ''' 根据已有的Face的reference更新面片数据（例如骨骼动画），此方法需要另开线程并监听BufferFinished
    ''' </summary>
    Public Sub RefreshBuffer()
        BufferFinished = 1
        If Me.Buffer IsNot Nothing Then Me.Buffer.Dispose()

        Dim structLength As VertexShaderInputLength
        If ShaderIndex = 0 Then
            structLength = VertexShaderInputLength.Position_Normal_Color_Tag
        Else
            structLength = VertexShaderInputLength.Position_Normal_Color
        End If

        Me.Buffer = New DataStream(structLength * 3 * Me.Faces.Count, True, True)
        With Me.Buffer
            For Each tmpFace As Game3dFace2_1 In Me.Faces
                .Write(tmpFace.Vertices(0))
                .Write(tmpFace.Normal(0))
                .Write(tmpFace.Colour)
                If Me.ShaderIndex = 0 Then .Write(tmpFace.Tag)
                .Write(tmpFace.Vertices(1))
                .Write(tmpFace.Normal(1))
                .Write(tmpFace.Colour)
                If Me.ShaderIndex = 0 Then .Write(tmpFace.Tag)
                .Write(tmpFace.Vertices(2))
                .Write(tmpFace.Normal(1))
                .Write(tmpFace.Colour)
                If Me.ShaderIndex = 0 Then .Write(tmpFace.Tag)
            Next
            .Position = 0
        End With

        BufferFinished = 2
    End Sub

    Public Sub Dispose()
        If Buffer IsNot Nothing Then Buffer.Dispose()
    End Sub

End Class
