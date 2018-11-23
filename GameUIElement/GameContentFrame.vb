' -----------------------------------------
' Copyright (c) 2018 All Rights Reserved.
' 
' Filename: GameContentFrame
' Author: Miz
' Date: 2018/10/29 11:08:52
' -----------------------------------------

Imports p104
Imports SharpDX.Direct2D1
Imports SharpDX.Mathematics.Interop
''' <summary>
''' 内容框控件类
''' </summary>
Public Class GameContentFrame
    Inherits GameBasicUIElement

    Public Children As New List(Of GameBasicUIElement)

    Public ChildrenQuadTree As Quadtree = Nothing
    Private ElementsMouseInside As New List(Of GameBasicUIElement)

    Public Sub InitializeQuadtree(pageSize As PointI)
        Me.ChildrenQuadTree = New Quadtree(MathHelper.Size2RawRect(pageSize))
        For Each element As GameBasicUIElement In Children
            Me.ChildrenQuadTree.AddItem(element)
        Next
    End Sub

    ''' <summary>
    ''' 返回内容总高度，同时刷新BasicRect.Bottom
    ''' </summary>
    Public Function CalculateRelativeTotalHeight() As Single
        Dim result As Single = 0.0F
        If Me.Children.Count Then
            For Each item As GameBasicUIElement In Me.Children
                result += item.Height
            Next
        End If
        Me.BasicRect.Bottom = Me.BasicRect.Top + result
        Return result
    End Function

    ''' <summary>
    ''' 重新计算所有子元素的BasicRect
    ''' </summary>
    Public Sub RefreshChildrenRelativeBasicRects()
        'TODO
    End Sub

    Public Overrides Sub DrawControlAtSelfCanvas(ByRef context As DeviceContext, ByRef spec As SpectatorCamera, canvasBitmap As Bitmap1)
        'context.FillRectangle(Me.SelfCanvasRect, Me.DefaultBackground)

        For Each element As GameBasicUIElement In Me.Children
            If element.IsValid Then
                element.DrawControlAtSelfCanvas(context, spec, canvasBitmap)
            End If
        Next
        With context
            .Target = Me.ControlCanvas
            .BeginDraw()
            .Clear(Nothing)
        End With
        For Each element As GameBasicUIElement In Me.Children
            If element.Visible Then
                element.DrawControl(context, spec, Me.ControlCanvas)
            End If
        Next
        context.EndDraw()

    End Sub
End Class
