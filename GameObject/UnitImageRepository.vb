Imports System.Xml
Imports SharpDX.Direct2D1
Imports SharpDX.Mathematics.Interop

''' <summary>
''' 单位图片资源仓库类
''' </summary>
Public Class UnitImageRepository
    Private Shared me_instance As UnitImageRepository = Nothing
    Private FragmentImages As BasicImageRepository = BasicImageRepository.Instance

    ''' <summary>
    ''' 详情面板左侧立绘
    ''' </summary>
    Private DetailLeftPanel As New List(Of ThreeStatesImageSet)
    ''' <summary>
    ''' 遭遇战地图人物图标图片
    ''' </summary>
    Private SkirmishChess As New List(Of ThreeStatesImageSet)


    Private Sub New()
    End Sub

    ''' <summary>
    ''' 单例
    ''' </summary>
    Public Shared Function Instance() As UnitImageRepository
        If me_instance Is Nothing Then me_instance = New UnitImageRepository
        Return me_instance
    End Function

    ''' <summary>
    ''' 从xml读取数据并依此组装FragmentImage为IGameImage
    ''' </summary>
    Public Sub LoadFromFiles(context As SharpDX.Direct2D1.DeviceContext)

        'load XML.
        'analyze rects of fragments which decides the rect of the wrapped image.
        'paint fragments in layers -> return bitmap1 object

        Dim xmlDoc As New XmlDocument()
        xmlDoc.LoadXml(My.Resources.AssembleImages)
        Dim root As XmlNode = xmlDoc.SelectSingleNode("content")
        Dim xnl As XmlNodeList = root.ChildNodes
        For Each item As XmlNode In xnl
            Dim element As XmlElement = CType(item, XmlElement)

            Dim targetSize As PointI = MathHelper.ParsePointI(element.GetAttribute("size"))
            Dim resultBitmap As Bitmap1 = New Bitmap1(context, New SharpDX.Size2(targetSize.X, targetSize.Y), NORMAL_BITMAP_PROPERTY)
            Dim children As XmlNodeList = element.ChildNodes
            For Each item2 As XmlNode In children
                Dim element2 As XmlElement = CType(item2, XmlElement)
                If element2.Name = "fragment" Then
                    Dim fromString As String = element2.GetAttribute("from")
                    Dim fragmentIndex As Integer = CInt(element2.GetAttribute("id"))
                    Dim tmpFragment As Bitmap1 = FragmentImages.GetFragment(fragmentIndex, fromString)
                    Dim fragmentPosition As PointI = MathHelper.ParsePointI(element2.GetAttribute("pos"))
                    Dim fragmentSize As PointI = MathHelper.ParsePointI(element2.GetAttribute("size"))
                    Dim tmpRect As New RawRectangleF(fragmentPosition.X, fragmentPosition.Y, fragmentPosition.X + fragmentSize.X, fragmentPosition.Y + fragmentSize.Y)
                    'Draw the fragment
                    context.Target = resultBitmap
                    context.BeginDraw()
                    context.DrawBitmap(tmpFragment, tmpRect, NOT_TRANSPARENT, BitmapInterpolationMode.Linear)
                    context.EndDraw()
                End If
            Next

            If element.Name = "static" Then
                If element.GetAttribute("type") = TYPE_THREE_IMAGES Then
                    Dim pairIndex As Integer = CInt(element.GetAttribute("id"))
                    Dim pairGroup As String = element.GetAttribute("group")
                    Dim pairDomain As String = element.GetAttribute("domain")
                    Dim targetPair As ThreeStatesImageSet = Me.GetOrCreatePair(pairIndex, pairGroup)

                    If pairDomain = "fine" Then
                        Dim tmpStaticImage As New GameStaticImage
                        tmpStaticImage.Initialize(resultBitmap, New RawRectangleF(0, 0, 200, 200))
                        targetPair.Fine(tmpStaticImage)

                    Else

                    End If

                End If
            End If
        Next


    End Sub

    ''' <summary>
    ''' 获取单位立绘组
    ''' </summary>
    Public Function GetUnitLeftPanelArtwork(unitId As Short) As ThreeStatesImageSet
        Return DetailLeftPanel(unitId)
    End Function

    Public Function GetOrCreatePair(index As Integer, groupString As String) As ThreeStatesImageSet
        If groupString = SKIRMISH_IMAGE_GROUP Then
            If index > SkirmishChess.Count - 1 Then
                Do
                    SkirmishChess.Add(New ThreeStatesImageSet())
                Loop Until index <= SkirmishChess.Count - 1
            End If
            Return SkirmishChess(index)
        End If
        Return Nothing
    End Function

    Public Function GetChessImage(index As Integer, status As TachieStatus) As IGameImage
        Return Me.SkirmishChess(index).GetImage(status)
    End Function

End Class

''' <summary>
''' 三状态图片组类
''' </summary>
Public Class ThreeStatesImageSet
    Private Images(2) As IGameImage

    Public Sub Fine(input As IGameImage)
        Me.Images(0) = input
    End Sub
    Public Sub Normal(input As IGameImage)
        Me.Images(1) = input
    End Sub
    Public Sub Injured(input As IGameImage)
        Me.Images(2) = input
    End Sub

    Public Function GetImage(status As TachieStatus) As IGameImage
        Return Me.Images(status)
    End Function

End Class

''' <summary>
''' 人物立绘状态枚举
''' </summary>
Public Enum TachieStatus As Byte
    Fine = 0
    Normal = 1
    Injured = 2
End Enum
