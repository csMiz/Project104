' -----------------------------------------
' Copyright (c) 2018 All Rights Reserved.
' 
' Filename: GameTextBlock
' Author: Miz
' Date: 2018/11/5 14:50:29
' -----------------------------------------

Imports p104
Imports SharpDX.Direct2D1
Imports SharpDX.DirectWrite
Imports SharpDX.Mathematics.Interop

''' <summary>
''' 游戏文本标签控件类
''' </summary>
Public Class GameTextBlock
    Inherits GameBasicUIElement
    ''' <summary>
    ''' 文本
    ''' </summary>
    Public Text As String = Nothing

    Private DirectWriteMode As Boolean = True
    Private DWTextFormat As TextFormat = Nothing
    ''' <summary>
    ''' DW模式下绘制文本的笔刷
    ''' </summary>
    Public DWFontBrush As Brush = Nothing
    ''' <summary>
    ''' TextItem模式下的TI对象
    ''' </summary>
    Private TextImage As TextItem = Nothing

    ''' <summary>
    ''' 构造函数
    ''' </summary>
    ''' <param name="useDirectWrite">是否使用DW直接绘制</param>
    ''' <param name="DWFontSize">使用DW时的字号</param>
    Public Sub New(useDirectWrite As Boolean, Optional DWFontSize As Single = 20.0F)
        Me.DirectWriteMode = useDirectWrite
        If useDirectWrite Then
            Me.DWTextFormat = New TextFormat(GameResources.DirectWriteFactoryInstance, GameResources.SystemDefaultFontFamilyName, DWFontSize)
        End If
    End Sub

    Public Overrides Sub DrawControlAtSelfCanvas(ByRef context As DeviceContext, ByRef spec As SpectatorCamera, canvasBitmap As Bitmap1)
        With context
            .Target = Me.ControlCanvas
            .BeginDraw()
            .Clear(Nothing)
            If DirectWriteMode Then
                .DrawText(Me.Text, Me.DWTextFormat, Me.SelfCanvasRect, DWFontBrush)
            Else
                'TODO
            End If
            .EndDraw()
        End With
    End Sub


End Class
