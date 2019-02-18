' -----------------------------------------
' Copyright (c) 2019 All Rights Reserved.
' 
' Filename: Game3DObject
' Author: Miz
' Date: 2019/2/14 14:27:34
' -----------------------------------------

Imports SharpDX.Direct2D1

Public Class Game3dObject

    Public Faces As Game3dFace()

    Public Texture As Game3dObjectTexture()

End Class

Public Class Game3dObjectTexture

    Public FilenameIndex As String = vbNullString

    Public Image As Bitmap1 = Nothing

End Class


Public Class Game3dFace

    Public FaceType As FaceType3D = FaceType3D.Three

    Public Vertices As PointF3()

    Public Normal As PointF3

    Public TextureIndex As Integer = -1

    Public TextureVertices As PointF2()



End Class

Public Enum FaceType3D
    Three = 3
    Four = 4
End Enum