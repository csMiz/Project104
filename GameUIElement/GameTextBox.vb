' -----------------------------------------
' Copyright (c) 2018 All Rights Reserved.
' 
' Filename: GameTextBox
' Author: Miz
' Date: 2018/11/12 10:16:58
' -----------------------------------------

Imports p104
Imports SharpDX.Direct2D1
Imports SharpDX.Mathematics.Interop

Public Class GameTextBox
    Inherits GameBasicUIElement

    Public Text As String = vbNullString
    Public Editable As Boolean = True

    Public BackgroundBrush As SolidColorBrush = Nothing
    Public BorderBrush As SolidColorBrush = Nothing

    'TextBox控件可以直接使用系统字体
    Public UseTextImage As Boolean = False
    Private TextImage As TextItem = Nothing

    Public Sub InitializeTextImage()
        Me.TextImage = New TextItem(Me.Text, New PointI(Me.Width, Me.Height))
        With Me.TextImage
            .LoadFont(GameFontHelper.GetFontFamily(0), 18, Brushes.Black, Color.Gray)
            .GenerateImage(Me.BindingContext)
        End With
    End Sub

    Public Sub SetText(inputText As String)
        Me.Text = inputText
        Call Me.RefreshTextImage()
    End Sub

    Public Sub RefreshTextImage()
        Me.TextImage.Text = Me.Text
        Me.TextImage.GenerateImage(Me.BindingContext)
    End Sub

    Public Overrides Sub DrawControl(ByRef context As DeviceContext, ByRef spec As SpectatorCamera, canvasBitmap As Bitmap1, newRect As RawRectangleF)
        With context
            .EndDraw()
            .Target = Me.ControlCanvas
            .BeginDraw()

            .Clear(Nothing)
            .FillRectangle(Me.SelfCanvasRect, BackgroundBrush)
            If Me.UseTextImage Then
                .DrawBitmap(Me.TextImage.FontImage, Me.SelfCanvasRect, NOT_TRANSPARENT, BitmapInterpolationMode.Linear)
            Else
                'TODO: use directwrite
            End If
            .DrawRectangle(Me.SelfCanvasRect, BorderBrush, 3.0F)

            .EndDraw()
            .Target = canvasBitmap
            .BeginDraw()
            .DrawBitmap(Me.ControlCanvas, newRect, NOT_TRANSPARENT, BitmapInterpolationMode.Linear)

        End With
    End Sub
End Class

