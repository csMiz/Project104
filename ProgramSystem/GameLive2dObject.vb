' -----------------------------------------
' Copyright (c) 2019 All Rights Reserved.
' 
' Filename: GameLive2dObject
' Author: Miz
' Date: 2019/1/4 16:51:20
' -----------------------------------------

Imports p104
Imports SharpDX.Direct2D1
Imports SharpDX.Mathematics.Interop
''' <summary>
''' 可互动Live2d对象类
''' </summary>
Public Class GameLive2dObject
    Implements IGameImage

    Private AllVertices As New List(Of GameLive2dSpriteVertex)

    Private AllSpriteImages As New List(Of SpriteLive2dImage)

    Private AllBones As New List(Of GameLive2dBone)

    Public Sub New()
    End Sub

    Public Sub LoadFromXml(xml As String)

    End Sub


    Public Sub PaintFullImage(ByRef context As DeviceContext) Implements IGameImage.PaintFullImage
        Throw New NotImplementedException()
    End Sub

    Public Function GetImage() As Bitmap1 Implements IGameImage.GetImage
        Throw New NotImplementedException()
    End Function

    Public Function GetDrawRect() As RawRectangleF Implements IGameImage.GetDrawRect
        Throw New NotImplementedException()
    End Function
End Class


Public Class GameLive2dSpriteVertex
    Public OldPosition As RawVector2
    Public NewPosition As RawVector2
    Public ParentImageIndex As New List(Of Integer)

End Class

Public Class GameLive2dBone
    Public SourcePoint As GameLive2dBoneNode
    Public DestinationPoint As GameLive2dBoneNode
    ''' <summary>
    ''' 获取骨骼方向（弧度）
    ''' </summary>
    Public ReadOnly Property BoneDirection As Single
        Get
            If DestinationPoint Is Nothing Then
                Return 0
            End If
            Return Math.Atan2(DestinationPoint.NewPosition.Y - SourcePoint.NewPosition.Y, DestinationPoint.NewPosition.X - SourcePoint.NewPosition.X)
        End Get
    End Property
    ''' <summary>
    ''' 获取骨骼长度
    ''' </summary>
    Public ReadOnly Property BoneLength As Single
        Get
            If DestinationPoint Is Nothing Then
                Return 0
            End If
            Return MathHelper.GetDistance(DestinationPoint.NewPosition, SourcePoint.NewPosition)
        End Get
    End Property
    Public FixLength As Boolean = True

End Class

Public Class GameLive2dBoneNode
    Public OldPosition As PointF2
    Public NewPosition As PointF2
    Public VerticesWeight As New List(Of KVP_IF)

End Class