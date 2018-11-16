' -----------------------------------------
' Copyright (c) 2018 All Rights Reserved.
' 
' Filename: GameComboBox
' Author: Miz
' Date: 2018/11/12 10:14:27
' -----------------------------------------

Imports p104
Imports SharpDX.Direct2D1
Imports SharpDX.Mathematics.Interop

Public Class GameComboBox
    Inherits GameBasicUIElement

    Private TitleImage As TextItem = Nothing
    Private SelectedBox As GameTextBox = Nothing
    Private Selections As New GameContentFrame

    Public TitleString As String = vbNullString     'TODO: change to TextResource
    Public SelectionStrings As New List(Of String)

    Public PlaceHolder As String = vbNullString
    Public SelectedIndex As Integer = -1

    Private LabelRect As RawRectangleF = Nothing
    Private FlyoutCanvas As Bitmap1 = Nothing
    Private FlyoutRect As RawRectangleF = Nothing

    Public ReadOnly Property SelectionCount As Integer
        Get
            Return Me.Selections.Children.Count
        End Get
    End Property

    Public Editable As Boolean = False

    Public Sub New()
        Me.SelectedBox = New GameTextBox With {
            .Editable = Me.Editable,
            .BackgroundBrush = BLACK_COLOUR_BRUSH(2)}
    End Sub

    Public Sub InitializeComboBox(labelWidth As Single)
        Me.InitializeControlCanvas()
        Dim labelSize As New PointI(labelWidth, Me.Height - 20)
        Me.LabelRect = New RawRectangleF(0, 10, labelWidth, Me.Height - 10)
        With Me.SelectedBox
            .BasicRect = New RawRectangleF(labelWidth, 10, Me.Width, Me.Height - 10)
            .BorderBrush = TRANSPARENT_BRUSH
            .BackgroundBrush = TRANSPARENT_BRUSH
            .BindingContext = Me.BindingContext
            .InitializeControlCanvas()
            .UseTextImage = True
            If Me.SelectedIndex >= 0 Then
                '.Text = Me.SelectionStrings(Me.SelectedIndex)
                .Text = "Window Mode"
            Else
                .Text = Me.PlaceHolder
            End If
            .InitializeTextImage()
        End With
        Me.FlyoutRect = New RawRectangleF(0, 0, Me.Width, Me.SelectedBox.Height * SelectionStrings.Count)
        Me.FlyoutCanvas = New Bitmap1(Me.BindingContext, New SharpDX.Size2(CInt(Me.FlyoutRect.Right), CInt(Me.FlyoutRect.Bottom)), NORMAL_BITMAP_PROPERTY)
        Me.TitleImage = New TextItem(Me.TitleString, labelSize)
        With Me.TitleImage
            .LoadFont(GameFontHelper.GetFontFamily(0), 18, Brushes.Black, Color.Gray)
            .GenerateImage(Me.BindingContext)
        End With
    End Sub

    Public Overrides Sub DrawControl(ByRef context As DeviceContext, ByRef spec As SpectatorCamera, canvasBitmap As Bitmap1, newRect As RawRectangleF)
        With context
            .EndDraw()
            .Target = Me.ControlCanvas
            .BeginDraw()

            .DrawBitmap(Me.TitleImage.FontImage, Me.LabelRect, NOT_TRANSPARENT, BitmapInterpolationMode.Linear)
            Call Me.SelectedBox.DrawControl(context, spec, Me.ControlCanvas)

            .EndDraw()
            .Target = canvasBitmap
            .BeginDraw()

            .DrawBitmap(Me.ControlCanvas, newRect, NOT_TRANSPARENT, BitmapInterpolationMode.Linear)
        End With
    End Sub
End Class
