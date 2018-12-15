' -----------------------------------------
' Copyright (c) 2018 All Rights Reserved.
' 
' Filename: GameListBox
' Author: Miz
' Date: 2018/11/23 14:15:42
' -----------------------------------------

Imports p104
Imports SharpDX.Direct2D1
Imports SharpDX.Mathematics.Interop

Public Class GameListBox
    Inherits GameBasicUIElement

    Public Items As New List(Of String)

    Public SelectedIndex As Integer = -1
    Public HoveringIndex As Integer = -1
    Public TextItemImages As List(Of TextItem) = Nothing
    Public SelectionHeight As Single = 0
    Private SelectionBoxTemplate As PointI = Nothing

    Public Sub New(Optional defaultEvents As Boolean = True)
        If defaultEvents Then
            Me.UseDefaultMouseEvents()
        End If
    End Sub

    Private Sub UseDefaultMouseEvents()
        AddHandler Me.MouseMove, AddressOf DefaultMouseMove
        AddHandler Me.MouseEnter, AddressOf DefaultMouseEnter
        AddHandler Me.MouseLeave, AddressOf DefaultMouseLeave

    End Sub

    Public Sub GenerateTextItems()
        If Me.TextItemImages IsNot Nothing Then
            Me.DisposeTextImages()
        End If
        Me.TextItemImages = New List(Of TextItem)
        If Me.Items.Count Then
            Me.SelectionHeight = Me.Height / Me.Items.Count
            Me.SelectionBoxTemplate = New PointI(Me.Width, Me.SelectionHeight)
            For i = 0 To Me.Items.Count - 1
                Dim tmpImage As New TextItem(Me.Items(i), Me.SelectionBoxTemplate)
                With tmpImage
                    .LoadFont(GameFontHelper.GetFontFamily(0), 18, Brushes.Black, Color.Gray)
                    .GenerateImage(Me.BindingContext)
                End With
                Me.TextItemImages.Add(tmpImage)
            Next
        End If
    End Sub

    Public Overrides Sub DrawControlAtSelfCanvas(ByRef context As DeviceContext, ByRef spec As SpectatorCamera, canvasBitmap As Bitmap1)
        With context
            .Target = Me.ControlCanvas
            .BeginDraw()
            .Clear(Nothing)

            .FillRectangle(Me.SelfCanvasRect, Me.DefaultBackground)
            If Me.HaveFocus AndAlso Me.HoveringIndex <> -1 Then
                Dim tmpRect As New RawRectangleF(0, Me.HoveringIndex * Me.SelectionHeight, Me.SelectionBoxTemplate.X, (Me.HoveringIndex + 1) * Me.SelectionHeight)
                .FillRectangle(tmpRect, WHITE_COLOUR_BRUSH(2))
            End If
            If Me.TextItemImages.Count Then
                For i = 0 To Me.TextItemImages.Count - 1
                    Dim drawRect As New RawRectangleF(0, i * Me.SelectionHeight, Me.SelectionBoxTemplate.X, (i + 1) * Me.SelectionHeight)
                    .DrawBitmap(Me.TextItemImages(i).FontImage, drawRect, NOT_TRANSPARENT, BitmapInterpolationMode.Linear)
                Next
            End If

            .EndDraw()
        End With
    End Sub

    ''' <summary>
    ''' 销毁所有缓存的文字图片对象
    ''' </summary>
    Public Sub DisposeTextImages()
        'TODO
    End Sub

    Public Sub DefaultMouseMove(e As GameMouseEventArgs)
        Dim tmpIndex As Integer = (e.Y - Me.AbsoluteRect.Top) \ Me.SelectionHeight
        If tmpIndex >= 0 AndAlso tmpIndex < Me.Items.Count Then
            Me.HoveringIndex = tmpIndex
        Else
            Me.HoveringIndex = -1
        End If
        e.Deliver = False
    End Sub
    Public Sub DefaultMouseEnter(e As GameMouseEventArgs)
        Me.HaveFocus = True
        e.Deliver = False
    End Sub
    Public Sub DefaultMouseLeave(e As GameMouseEventArgs)
        Me.HaveFocus = False
        Me.HoveringIndex = -1
    End Sub


End Class
