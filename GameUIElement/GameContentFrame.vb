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


    Public Sub New(Optional defaultEvents As Boolean = True)
        If defaultEvents Then
            Me.UseDefaultMouseEvents()
        End If
    End Sub

    Public Sub UseDefaultMouseEvents()
        AddHandler Me.MouseEnter, AddressOf DefaultMouseEnter
        AddHandler Me.MouseLeave, AddressOf DefaultMouseLeave
        AddHandler Me.MouseMove, AddressOf DefaultMouseMove
        AddHandler Me.MouseDown, AddressOf DefaultMouseDown
        AddHandler Me.MouseUp, AddressOf DefaultMouseUp
        AddHandler Me.GlobalMouseMove, AddressOf DefaultGlobalMouseMove
    End Sub

    Public Sub InitializeQuadtree(pageSize As PointI)
        Me.Children.Sort(GameBasicUIElement.ZIndexComparison)    'z_index由小到大
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
    ''' 重新计算所有子元素的Rect
    ''' </summary>
    Public Sub RefreshChildrenRelativeRects()
        'TODO
    End Sub

    Public Overrides Sub TriggerDrawSelfCanvas(ByRef context As DeviceContext, ByRef spec As SpectatorCamera, canvasBitmap As Bitmap1)
        Me.DrawControlAtSelfCanvas(context, spec, canvasBitmap)
    End Sub

    Public Overrides Sub DrawControlAtSelfCanvas(ByRef context As DeviceContext, ByRef spec As SpectatorCamera, canvasBitmap As Bitmap1)
        'context.FillRectangle(Me.SelfCanvasRect, Me.DefaultBackground)

        For Each element As GameBasicUIElement In Me.Children
            If element.IsValid Then
                element.TriggerDrawSelfCanvas(context, spec, canvasBitmap)
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

    Public Sub DefaultMouseEnter()
        Me.HaveFocus = True
    End Sub
    Public Sub DefaultMouseLeave()
        Me.HaveFocus = False
    End Sub
    Public Sub DefaultMouseMove(e As GameMouseEventArgs)
        Dim cursorResult As List(Of GameBasicUIElement) = GameBasicUIElement.ConvertQuadElements(ChildrenQuadTree.Find(e.Position))
        cursorResult.Sort(GameBasicUIElement.ZIndexComparisonReverse)
        If cursorResult.Count Then
            Dim refreshElementsInside As New List(Of GameBasicUIElement)
            For i = 0 To cursorResult.Count - 1
                If Not e.Deliver Then Exit For
                Dim element As GameBasicUIElement = cursorResult(i)
                If Me.ElementsMouseInside.Contains(element) Then
                    element.RaiseMouseMove(e)
                    Me.ElementsMouseInside.Remove(element)
                Else
                    element.RaiseMouseEnter(e)
                End If
                refreshElementsInside.Add(element)
            Next
            For Each element As GameBasicUIElement In Me.ElementsMouseInside
                element.RaiseMouseLeave(e)
            Next
            Me.ElementsMouseInside = refreshElementsInside
        Else
            For Each element As GameBasicUIElement In Me.ElementsMouseInside
                element.RaiseMouseLeave(e)
            Next
            Me.ElementsMouseInside.Clear()
        End If
    End Sub
    Public Sub DefaultGlobalMouseMove(e As GameMouseEventArgs)
        If Me.Children.Count Then
            For i = Me.Children.Count - 1 To 0 Step -1
                If Not e.Deliver Then Exit For
                Dim element As GameBasicUIElement = Me.Children(i)
                If element.IsValid Then element.RaiseGlobalMouseMove(e)
            Next
        End If
    End Sub
    Public Sub DefaultMouseDown(e As GameMouseEventArgs)
        Dim cursorResult As List(Of GameBasicUIElement) = GameBasicUIElement.ConvertQuadElements(ChildrenQuadTree.Find(e.Position))
        cursorResult.Sort(GameBasicUIElement.ZIndexComparisonReverse)
        If cursorResult.Count Then
            For i = 0 To cursorResult.Count - 1
                If Not e.Deliver Then Exit For
                Dim element As GameBasicUIElement = cursorResult(i)
                element.RaiseMouseDown(e)
            Next
        End If
    End Sub
    Public Sub DefaultMouseUp(e As GameMouseEventArgs)
        Dim cursorResult As List(Of GameBasicUIElement) = GameBasicUIElement.ConvertQuadElements(ChildrenQuadTree.Find(e.Position))
        cursorResult.Sort(GameBasicUIElement.ZIndexComparisonReverse)
        If cursorResult.Count Then
            For i = 0 To cursorResult.Count - 1
                If Not e.Deliver Then Exit For
                Dim element As GameBasicUIElement = cursorResult(i)
                element.RaiseMouseUp(e)
            Next
        End If
    End Sub

End Class
