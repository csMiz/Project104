' -----------------------------------------
' Copyright (c) 2018 All Rights Reserved.
' 
' Filename: GameButton
' Author: Miz
' Date: 2018/10/29 11:40:59
' -----------------------------------------

Imports p104
Imports SharpDX.Direct2D1
''' <summary>
''' Button基类
''' </summary>
Public MustInherit Class GameButton
    Inherits GameBasicUIElement
    ''' <summary>
    ''' 按钮是否有效，相当于Enable
    ''' </summary>
    Protected CanBePressed As Boolean = True
    Private TextSource As String = vbNullString
    Public Property Text As String
        Get
            Return TextSource
        End Get
        Set(value As String)
            TextSource = value
            ResetTextImage(value)
        End Set
    End Property
    Protected TextImage As TextItem = Nothing

    Public Sub BindDeviceContext(context As DeviceContext)
        Me.BindingContext = context
    End Sub

    Private Sub ResetTextImage(value As String)
        If Me.TextImage Is Nothing Then
            Me.TextImage = New TextItem(value, New PointI(Me.BasicRect.Right - Me.BasicRect.Left, Me.BasicRect.Bottom - Me.BasicRect.Top))
            Me.TextImage.LoadFont(GameFontHelper.GetFontFamily(0), 18, Brushes.White, Color.Gray)
            Me.TextImage.GenerateImage(Me.BindingContext)
        Else
            Me.TextImage.Text = value
            Me.TextImage.GenerateImage(Me.BindingContext)
        End If
    End Sub


    Public MustOverride Overrides Sub DrawControl(ByRef context As DeviceContext, ByRef spec As SpectatorCamera, canvasBitmap As Bitmap1, newRect As SharpDX.Mathematics.Interop.RawRectangleF)




End Class
