' -----------------------------------------
' Copyright (c) 2019 All Rights Reserved.
' 
' Filename: LiveImageRepository
' Author: Miz
' Date: 2019/1/22 13:24:01
' -----------------------------------------

Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Xml
Imports SharpDX.Direct2D1
Imports SharpDX.Mathematics.Interop

''' <summary>
''' 动态图片仓库类
''' </summary>
Public Class LiveImageRepository
    Private Shared me_instance As LiveImageRepository = Nothing

    Public Sprites As New List(Of SpriteImagePair)

    Public LiveImages As New List(Of GameLive2dImage)

    Private Sub New()
    End Sub

    Public Shared Function Instance() As LiveImageRepository
        If me_instance Is Nothing Then
            me_instance = New LiveImageRepository
        End If
        Return me_instance
    End Function

    ''' <summary>
    ''' 载入所有精灵素材
    ''' </summary>
    Public Sub LoadSpritesFromFile(context As DeviceContext)
        Dim dirInfo As New System.IO.DirectoryInfo(Application.StartupPath & "\Resources\Images\Sprite\")
        Dim allFiles() As System.IO.FileInfo = dirInfo.GetFiles
        For Each file As System.IO.FileInfo In allFiles
            Dim readStream As System.IO.FileStream = file.OpenRead
            Dim tmpImage As SharpDX.Direct2D1.Bitmap1 = GameResources.LoadBitmapUsingWIC(context, readStream)

            Dim tmpPair As New SpriteImagePair With {
                .Name = file.Name,
                .SourceSize = New PointF2(tmpImage.Size.Width, tmpImage.Size.Height),
                .Image = tmpImage}
            Dim indexString() As String = Regex.Split(tmpPair.Name, UNDERLINE)
            tmpPair.NameSortIndex = CInt(indexString.Last.Remove(indexString.Last.Length - 4))
            Me.Sprites.Add(tmpPair)
        Next
        Dim tmpCompare As New Comparison(Of SpriteImagePair)(Function(a As SpriteImagePair, b As SpriteImagePair)
                                                                 Return (a.NameSortIndex - b.NameSortIndex)
                                                             End Function)
        Me.Sprites.Sort(tmpCompare)
    End Sub

    ''' <summary>
    ''' 解析所有动态图像，需要先载入完精灵素材
    ''' </summary>
    Public Sub LoadLiveConfigFromFiles()
        Dim dirInfo As New System.IO.DirectoryInfo(Application.StartupPath & "\Resources\Images\LiveDependency\")
        Dim allFiles() As System.IO.FileInfo = dirInfo.GetFiles
        For Each file As System.IO.FileInfo In allFiles
            Dim filePath As String = file.FullName

            'analyse
            Dim tmpLiveImage As New GameLive2dImage

            Dim xmlDoc As XmlDocument = New XmlDocument()
            Dim settings As XmlReaderSettings = New XmlReaderSettings()
            settings.IgnoreComments = True
            Dim reader As XmlReader = XmlReader.Create(filePath, settings)
            xmlDoc.Load(reader)

            Dim root As XmlNode = xmlDoc.SelectSingleNode("content")
            Dim xnl As XmlNodeList = root.ChildNodes
            For Each item As XmlNode In xnl
                Dim element As XmlElement = CType(item, XmlElement)
                If element.Name = "mark" Then
                    tmpLiveImage.Description = element.GetAttribute("text")
                    tmpLiveImage.CanvasSize = MathHelper.ParseSize(element.GetAttribute("fullsize"))
                ElseIf element.Name = "images" Then
                    Dim xnl2 As XmlNodeList = item.ChildNodes
                    For Each item2 As XmlNode In xnl2
                        Dim element2 As XmlElement = CType(item2, XmlElement)
                        If element2.Name = "sprite" Then
                            Dim tmpSprite As New SpriteLive2dImage
                            tmpSprite.SourceImage = Me.Sprites(CInt(element2.GetAttribute("repoindex"))).Image
                            Dim tmpPosString As String = element2.GetAttribute("pos")
                            Dim tmpSizeString As String = element2.GetAttribute("size")
                            Dim tmpPosArgs As String() = Regex.Split(tmpPosString, COMMA)
                            Dim tmpSizeArgs As String() = Regex.Split(tmpSizeString, COMMA)
                            tmpSprite.Vertices(0) = New GameLive2dSpriteVertex() With {.OldPosition = New RawVector2(CSng(tmpPosArgs(0)), CSng(tmpPosArgs(1))), .NewPosition = .OldPosition}
                            tmpSprite.Vertices(1) = New GameLive2dSpriteVertex() With {.OldPosition = New RawVector2(CSng(tmpPosArgs(0)) + CSng(tmpSizeArgs(0)), CSng(tmpPosArgs(1))), .NewPosition = .OldPosition}
                            tmpSprite.Vertices(2) = New GameLive2dSpriteVertex() With {.OldPosition = New RawVector2(CSng(tmpPosArgs(0)) + CSng(tmpSizeArgs(0)), CSng(tmpPosArgs(1)) + CSng(tmpSizeArgs(1))), .NewPosition = .OldPosition}
                            tmpSprite.Vertices(3) = New GameLive2dSpriteVertex() With {.OldPosition = New RawVector2(CSng(tmpPosArgs(0)), CSng(tmpPosArgs(1)) + CSng(tmpSizeArgs(1))), .NewPosition = .OldPosition}
                            tmpLiveImage.MeshAtlas.Add(tmpSprite)
                        End If
                    Next
                ElseIf element.Name = "animation" Then
                    Dim tmpAnimation As New GameLive2dImageAnimation
                    tmpAnimation.AnimationName = element.GetAttribute("name")
                    tmpAnimation.AnimationFPS = CInt(element.GetAttribute("fps"))
                    tmpAnimation.AnimationLength = CInt(element.GetAttribute("length"))
                    ReDim tmpAnimation.KeyFrames(tmpLiveImage.MeshAtlas.Count - 1)
                    For i = 0 To tmpLiveImage.MeshAtlas.Count - 1
                        tmpAnimation.KeyFrames(i) = New List(Of LiveImageKeyFrame)
                    Next
                    Dim xnl2 As XmlNodeList = item.ChildNodes
                    For Each item2 As XmlNode In xnl2
                        Dim element2 As XmlElement = CType(item2, XmlElement)
                        If element2.Name = "target" Then
                            Dim selectedSprite As Integer = CInt(element2.GetAttribute("index"))
                            Dim xnl3 As XmlNodeList = item2.ChildNodes
                            For Each item3 As XmlNode In xnl3
                                Dim element3 As XmlElement = CType(item3, XmlElement)
                                If element3.Name = "keyframe" Then
                                    Dim tmpKF As New LiveImageKeyFrame
                                    tmpKF.Frame = element3.GetAttribute("frame")
                                    Dim xnl4 As XmlNodeList = item3.ChildNodes
                                    For Each item4 As XmlNode In xnl4
                                        Dim element4 As XmlElement = CType(item4, XmlElement)
                                        If element4.Name = "position" Then
                                            If element4.InnerText = "undefined" Then
                                                If tmpKF.Frame = 0 Then
                                                    tmpKF.RegisterPosition = True
                                                    tmpKF.PositionArgs = MathHelper.RawVec2PointF2(tmpLiveImage.MeshAtlas(selectedSprite).Vertices(0).OldPosition)
                                                Else
                                                    tmpKF.RegisterPosition = False
                                                End If
                                            Else
                                                tmpKF.RegisterPosition = True
                                                Dim tmpPosArgs() As String = Regex.Split(element4.InnerText, COMMA)
                                                tmpKF.PositionArgs = New PointF2(CSng(tmpPosArgs(0)), CSng(tmpPosArgs(1)))
                                            End If
                                            If tmpKF.RegisterPosition Then
                                                If element4.HasAttribute("tweenX") Then
                                                    Dim tmpTweenX As String = element4.GetAttribute("tweenX")
                                                    If tmpTweenX = "undefined" Then
                                                        tmpKF.PositionTween(0) = DEFAULT_BEZIER
                                                    Else
                                                        Dim tmpTween As New BezierPenCurve
                                                        tmpTween.ParseAnchors(tmpTweenX)
                                                        tmpKF.PositionTween(0) = tmpTween
                                                    End If
                                                Else
                                                    tmpKF.PositionTween(0) = DEFAULT_BEZIER
                                                End If
                                                If element4.HasAttribute("tweenY") Then
                                                    Dim tmpTweenY As String = element4.GetAttribute("tweenY")
                                                    If tmpTweenY = "undefined" Then
                                                        tmpKF.PositionTween(1) = DEFAULT_BEZIER
                                                    Else
                                                        Dim tmpTween As New BezierPenCurve
                                                        tmpTween.ParseAnchors(tmpTweenY)
                                                        tmpKF.PositionTween(1) = tmpTween
                                                    End If
                                                Else
                                                    tmpKF.PositionTween(1) = DEFAULT_BEZIER
                                                End If
                                            End If
                                        ElseIf element4.Name = "rotation" Then
                                            If element4.InnerText = "undefined" Then
                                                If tmpKF.Frame = 0 Then
                                                    tmpKF.RegisterRotation = True
                                                    tmpKF.RotationArgs = 0
                                                Else
                                                    tmpKF.RegisterRotation = False
                                                End If
                                            Else
                                                tmpKF.RegisterRotation = True
                                                tmpKF.RotationArgs = CSng(element4.InnerText)
                                            End If
                                            If tmpKF.RegisterRotation Then
                                                If element4.HasAttribute("tween") Then
                                                    Dim tmpTweenValue As String = element4.GetAttribute("tween")
                                                    If tmpTweenValue = "undefined" Then
                                                        tmpKF.RotationTween = DEFAULT_BEZIER
                                                    Else
                                                        Dim tmpTween As New BezierPenCurve
                                                        tmpTween.ParseAnchors(tmpTweenValue)
                                                        tmpKF.RotationTween = tmpTween
                                                    End If
                                                Else
                                                    tmpKF.RotationTween = DEFAULT_BEZIER
                                                End If
                                            End If
                                        Else
                                            'TODO
                                        End If
                                    Next
                                    tmpAnimation.KeyFrames(selectedSprite).Add(tmpKF)
                                End If
                            Next
                        End If
                    Next
                    tmpAnimation.BindingLive2dImage = tmpLiveImage
                    tmpLiveImage.Animations.Add(tmpAnimation)
                End If

            Next
            Me.LiveImages.Add(tmpLiveImage)
        Next
    End Sub

End Class

Public Class SpriteImagePair
    Public Name As String = vbNullString
    Public NameSortIndex As Integer = 0
    Public SourceSize As PointF2
    Public Image As Bitmap1 = Nothing

End Class
