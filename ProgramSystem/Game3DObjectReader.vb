' -----------------------------------------
' Copyright (c) 2019 All Rights Reserved.
' 
' Filename: Game3DObjectReader
' Author: Miz
' Date: 2019/2/14 15:15:11
' -----------------------------------------

Imports System.IO
Imports System.Text.RegularExpressions
Imports SharpDX.Direct2D1
Imports SharpDX.Mathematics.Interop

Public Class Game3DObjectReader

    Private Shared me_instance As Game3DObjectReader = Nothing
    <Obsolete>
    Public TextureRepository As New List(Of Game3dObjectTexture)
    <Obsolete>
    Public ObjectRepository As New List(Of Game3dObject)

    Public ObjectRepository2(255) As Game3DObject2

    Private Sub New()
    End Sub
    ''' <summary>
    ''' 单例模式
    ''' </summary>
    Public Shared Function Instance() As Game3DObjectReader
        If me_instance Is Nothing Then me_instance = New Game3DObjectReader
        Return me_instance
    End Function

    Public Sub ReadAll()
        Dim dirInfo As New System.IO.DirectoryInfo(Application.StartupPath & "\Resources\Models\")
        Dim allFiles() As System.IO.FileInfo = dirInfo.GetFiles

        For Each file As System.IO.FileInfo In allFiles
            Dim path As String = file.FullName
            If path.EndsWith(".pob") Then

                ReadObject2FromPath(path)

            End If
        Next

    End Sub

    Public Sub ReadObject2FromPath(path As String)
        Dim name As String = path
        Dim args() As String = name.Remove(name.Length - 4).Split("_")
        Dim nameIndex As String = args.Last
        Dim fileStr As New FileStream(path, FileMode.Open)
        Dim content As String = vbNullString
        Using sr As New StreamReader(fileStr)
            content = sr.ReadToEnd
        End Using
        fileStr.Close()
        fileStr.Dispose()
        ReadObject2(content, CInt(nameIndex))
    End Sub

    Public Sub ReadObject2(content As String, targetIndex As Integer)
        Dim tmpObj As Game3DObject2 = Nothing
        Dim faceList As New List(Of Game3dFace2_1)
        Dim colourList As New List(Of RawColor4)
        Dim regionCheckList As New List(Of PointF3)
        Dim lines As String() = Regex.Split(content, vbCrLf)
        For Each line As String In lines
            Dim args As String() = Regex.Split(line, SPACE_STRING)
            Dim cmd As String = args(0)
            If cmd = "#object" Then
                tmpObj = New Game3DObject2
                faceList.Clear()
                colourList.Clear()
                regionCheckList.Clear()
            ElseIf cmd = "#colorRGBA" Then
                colourList.Add(ParseRawColor4(args(1)))
            ElseIf cmd = "#f" Then
                Dim tmpFace As New Game3dFace2_1
                With tmpFace
                    .Vertices = {ParseRawVector3(args(1)), ParseRawVector3(args(2)), ParseRawVector3(args(3))}
                    .Normal = {ParseRawVector3(args(4)), ParseRawVector3(args(5)), ParseRawVector3(args(6))}
                    .Colour = colourList(CInt(args(7)))
                End With
                faceList.Add(tmpFace)
            ElseIf cmd = "#end" Then
                tmpObj.SourceFaces = faceList.ToArray
                tmpObj.RegionCheckSign = regionCheckList.ToArray
                If tmpObj.RegionCheckSign.Length = 0 Then
                    tmpObj.RegionCheckSign = {New PointF3(0, 0, 0)}
                End If
                ObjectRepository2(targetIndex) = tmpObj
                faceList.Clear()
                colourList.Clear()
                regionCheckList.Clear()
            End If
        Next

    End Sub


    ''' <summary>
    ''' 加载材质文件夹下的所有贴图
    ''' </summary>
    <Obsolete>
    Public Sub LoadTexture(context As DeviceContext)
        TextureRepository.Clear()
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

    <Obsolete>
    Public Sub ReadObject(content As String)
        Dim tmpObj As Game3dObject = Nothing
        Dim faceList As New List(Of Game3dFace)
        Dim textureList As New List(Of Game3dObjectTexture)
        Dim colourList As New List(Of RawColor4)
        Dim regionCheckList As New List(Of PointF3)
        Dim lines As String() = Regex.Split(content, vbCrLf)
        For Each line As String In lines
            Dim args As String() = Regex.Split(line, SPACE_STRING)
            Dim cmd As String = args(0)
            If cmd = "#object" Then
                tmpObj = New Game3dObject
                faceList.Clear()
                textureList.Clear()
            ElseIf cmd = "#texture" Then
                Dim targetTexture As Game3dObjectTexture = Nothing
                For Each tmpTexture As Game3dObjectTexture In TextureRepository
                    If tmpTexture.FilenameIndex = args(1) Then
                        targetTexture = tmpTexture
                        Exit Sub
                    End If
                Next
                If targetTexture Is Nothing Then
                    Throw New Exception("texture not found!")
                End If
                textureList.Add(targetTexture)
            ElseIf cmd = "#colorRGBA" Then
                colourList.Add(ParseRawColor4(args(1)))
            ElseIf cmd = "#f" Then
                Dim tmpFace As New Game3dFace
                With tmpFace
                    .FaceType = CInt(args(1))
                    If .FaceType = FaceType3D.Three Then
                        .Vertices = {ParsePointF3(args(2)), ParsePointF3(args(3)), ParsePointF3(args(4))}
                        .Normal = ParsePointF3(args(5))
                        .Colour = colourList(CInt(args(6)))
                        .TextureIndex = CInt(args(7))
                        .TextureVertices = {ParsePointF2(args(8)), ParsePointF2(args(9)), ParsePointF2(args(10))}
                    Else
                        .Vertices = {ParsePointF3(args(2)), ParsePointF3(args(3)), ParsePointF3(args(4)), ParsePointF3(args(5))}
                        .Normal = ParsePointF3(args(6))
                        .Colour = colourList(CInt(args(7)))
                        .TextureIndex = CInt(args(8))
                        .TextureVertices = {ParsePointF2(args(9)), ParsePointF2(args(10)), ParsePointF2(args(11)), ParsePointF2(args(12))}
                    End If
                End With
                faceList.Add(tmpFace)
            ElseIf cmd = "#end" Then
                tmpObj.Texture = textureList.ToArray
                tmpObj.Faces = faceList.ToArray
                tmpObj.RegionCheckSign = regionCheckList.ToArray
                If tmpObj.RegionCheckSign.Length = 0 Then
                    tmpObj.RegionCheckSign = {New PointF3(0, 0, 0)}
                End If
                ObjectRepository.Add(tmpObj)
                faceList.Clear()
                textureList.Clear()
            End If
        Next

    End Sub

    <Obsolete>
    Public Sub ReadObjectFromPath(path As String)
        Dim fileStr As New FileStream(path, FileMode.Open)
        Dim content As String = vbNullString
        Using sr As New StreamReader(fileStr)
            content = sr.ReadToEnd
        End Using
        fileStr.Close()
        fileStr.Dispose()
        ReadObject(content)
    End Sub

    <Obsolete>
    Public Function GetTexture(index As Integer) As Bitmap1
        For Each tmpTexture As Game3dObjectTexture In Me.TextureRepository
            If CInt(tmpTexture.FilenameIndex) = index Then
                Return tmpTexture.Image
            End If
        Next
        Return Nothing
    End Function

End Class
