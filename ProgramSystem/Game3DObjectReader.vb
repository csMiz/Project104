' -----------------------------------------
' Copyright (c) 2019 All Rights Reserved.
' 
' Filename: Game3DObjectReader
' Author: Miz
' Date: 2019/2/14 15:15:11
' -----------------------------------------

Imports System.Text.RegularExpressions
Imports SharpDX.Direct2D1

Public Class Game3DObjectReader

    Public TextureRepository As New List(Of Game3dObjectTexture)

    Public ObjectRepository As New List(Of Game3dObject)

    Public Sub LoadTexture(context As DeviceContext)
        Dim dirInfo As New System.IO.DirectoryInfo(Application.StartupPath & "\Resources\Images\Texture\")
        Dim allFiles() As System.IO.FileInfo = dirInfo.GetFiles
        For Each file As System.IO.FileInfo In allFiles
            Dim name As String = file.Name
            Dim args() As String = name.Remove(name.Length - 4).Split("_")
            Dim nameIndex As String = args.Last


            Dim readStream As System.IO.FileStream = file.OpenRead
            Dim tmpImage As SharpDX.Direct2D1.Bitmap1 = GameResources.LoadBitmapUsingWIC(context, readStream)

            Dim tmpTexture As New Game3dObjectTexture
            With tmpTexture
                .FilenameIndex = nameIndex
                .Image = tmpImage
            End With

            TextureRepository.Add(tmpTexture)

        Next
    End Sub

    Public Sub ReadObject(content As String)
        Dim tmpObj As Game3dObject = Nothing
        Dim faceList As New List(Of Game3dFace)
        Dim textureList As New List(Of Game3dObjectTexture)
        Dim lines As String() = Regex.Split(content, vbCrLf)
        For Each line As String In lines
            Dim args As String() = Regex.Split(line, SPACE_STRING)
            Dim cmd As String = args(0)
            If cmd = "#object" Then
                tmpObj = New Game3dObject
                faceList.Clear()
                textureList.Clear()
            ElseIf cmd = "#texture" Then
                textureList.Add(TextureRepository(CInt(args(1))))
                'HACK: wrong
            ElseIf cmd = "#f" Then
                Dim tmpFace As New Game3dFace
                With tmpFace
                    .FaceType = CInt(args(1))
                    If .FaceType = FaceType3D.Three Then
                        .Vertices = {ParsePointF3(args(2)), ParsePointF3(args(3)), ParsePointF3(args(4))}
                        .Normal = ParsePointF3(args(5))
                        .TextureIndex = CInt(args(6))
                        .TextureVertices = {ParsePointF2(args(7)), ParsePointF2(args(8)), ParsePointF2(args(9))}
                    Else
                        .Vertices = {ParsePointF3(args(2)), ParsePointF3(args(3)), ParsePointF3(args(4)), ParsePointF3(args(5))}
                        .Normal = ParsePointF3(args(6))
                        .TextureIndex = CInt(args(7))
                        .TextureVertices = {ParsePointF2(args(8)), ParsePointF2(args(9)), ParsePointF2(args(10)), ParsePointF2(args(11))}
                    End If
                End With
                faceList.Add(tmpFace)
            ElseIf cmd = "#end" Then
                tmpObj.Texture = textureList.ToArray
                tmpObj.Faces = faceList.ToArray
                ObjectRepository.Add(tmpObj)
                faceList.Clear()
                textureList.Clear()
            End If
        Next

    End Sub

End Class
