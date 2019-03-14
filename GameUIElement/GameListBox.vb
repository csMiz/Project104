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

    Public ChildrenFontStyle As New TextItem2

    Public SelectedIndex As Integer = -1
    Public HoveringIndex As Integer = -1
    Public TextItems As List(Of TextItem2) = Nothing
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
        If Me.TextItems IsNot Nothing Then
            Me.DisposeTextImages()
        End If
        Me.TextItems = New List(Of TextItem2)
        If Me.Items.Count Then
            Me.SelectionHeight = Me.Height / Me.Items.Count
            Me.SelectionBoxTemplate = New PointI(Me.Width, Me.SelectionHeight)
            For i = 0 To Me.Items.Count - 1
                Dim tmpImage As New TextItem2
                With tmpImage
                    .Text = Me.Items(i)
                    .FontType = ChildrenFontStyle.FontType
                    .FontName = ChildrenFontStyle.FontName
                    .FontBrush = ChildrenFontStyle.FontBrush
                    .FontSize = ChildrenFontStyle.FontSize
                    .CanvasSize = New PointF2(Me.Width, Me.SelectionHeight)
                    .GenerateTextLayout()
                End With
                Me.TextItems.Add(tmpImage)
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
            If Me.TextItems.Count Then
                For i = 0 To Me.TextItems.Count - 1
                    TextItems(i).DrawText(context, New RawVector2(0, i * Me.SelectionHeight))
                Next
            End If

            .EndDraw()
        End With
    End Sub

    ''' <summary>
    ''' 销毁所有缓存的文字图片对象
    ''' <para>注意，此处画笔对象不会被销毁</para>
    ''' </summary>
    Public Sub DisposeTextImages()
        For Each item As TextItem2 In Me.TextItems
            item.Dispose(False)
        Next
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
