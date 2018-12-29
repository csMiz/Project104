' -----------------------------------------
' Copyright (c) 2018 All Rights Reserved.
' 
' Filename: GamePageProperty
' Author: Miz
' Date: 2018/10/30 12:31:03
' -----------------------------------------

Imports SharpDX.Direct2D1
Imports SharpDX.Mathematics.Interop

''' <summary>
''' 控件页面类，交互的基本容器，需要手动注册四叉树
''' </summary>
Public Class GamePageProperty
    ''' <summary>
    ''' 子控件
    ''' </summary>
    Public UIElements As New List(Of GameBasicUIElement)
    ''' <summary>
    ''' 子控件四叉树
    ''' </summary>
    Public ElementsQuadtree As Quadtree = Nothing

    'Public MouseLastPosition As PointI = Nothing
    'Private ElementsMouseOutside As New List(Of GameBasicUIElement)

    ''' <summary>
    ''' 当前状态为“指针在控件内”的控件
    ''' </summary>
    Private ElementsMouseInside As New List(Of GameBasicUIElement)

    Public Sub PaintElements(ByRef context As DeviceContext, ByRef spec As SpectatorCamera, canvasBitmap As Bitmap1)
        Dim ElementCount As Integer = Me.UIElements.Count
        If CBool(ElementCount) Then
            context.EndDraw()
            For Each element As GameBasicUIElement In Me.UIElements
                If element.Visible Then
                    element.TriggerDrawSelfCanvas(context, spec, canvasBitmap)
                End If
            Next
            context.Target = canvasBitmap
            context.BeginDraw()
            For Each element As GameBasicUIElement In Me.UIElements
                If element.Visible Then
                    element.DrawControl(context, spec, canvasBitmap)
                End If
            Next
        End If
    End Sub

    Public Sub GenerateElementsQuadtree(pageSize As PointI)
        Me.UIElements.Sort(GameBasicUIElement.ZIndexComparison)
        Me.ElementsQuadtree = New Quadtree(MathHelper.Size2RawRect(pageSize))
        For Each element As GameBasicUIElement In UIElements
            Me.ElementsQuadtree.AddItem(element)
        Next
    End Sub

    Public Sub InitializeCursor(setX As Integer, setY As Integer, resolve As PointI)
        Dim cursorPoint As PointI = New PointI(setX, setY)
        'System.Windows.Forms.Cursor.Position = cursorPoint    'not correct
        Call Me.InitializeCursor(cursorPoint, resolve)
    End Sub

    Public Sub InitializeCursor(cursorPoint As PointI, resolve As PointI)
        'Me.MouseLastPosition = cursorPoint
        Dim cursorResult As List(Of IQuadtreeRecognizable) = ElementsQuadtree.Find(cursorPoint)
        If cursorResult.Count Then
            For Each element As GameBasicUIElement In cursorResult
                If element.IsValid Then
                    element.RaiseMouseEnter(GameMouseEventArgs.GetCentreArgs(resolve))
                    Me.ElementsMouseInside.Add(element)
                End If
            Next
        End If
    End Sub

    ''' <summary>
    ''' 触发MouseMove, MouseEnter, MouseLeave。
    ''' 一旦将e.Deliver赋值为False，MouseMove, Enter, Leave都将停止传递
    ''' </summary>
    Public Sub TriggerMouseMove(e As GameMouseEventArgs)
        Dim cursorResult As List(Of GameBasicUIElement) = GameBasicUIElement.ConvertQuadElements(ElementsQuadtree.Find(e.Position))
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
        'Me.MouseLastPosition = e.Position
    End Sub

    Public Sub TriggerGlobalMouseMove(e As GameMouseEventArgs)
        If Me.UIElements.Count Then
            For i = Me.UIElements.Count - 1 To 0 Step -1
                If Not e.Deliver Then Exit For
                Dim element As GameBasicUIElement = Me.UIElements(i)
                If element.IsValid Then element.RaiseGlobalMouseMove(e)
            Next
        End If
    End Sub

    Public Sub TriggerMouseDown(e As GameMouseEventArgs)
        Dim cursorResult As List(Of GameBasicUIElement) = GameBasicUIElement.ConvertQuadElements(ElementsQuadtree.Find(e.Position))
        cursorResult.Sort(GameBasicUIElement.ZIndexComparisonReverse)
        If cursorResult.Count Then
            For i = 0 To cursorResult.Count - 1
                If Not e.Deliver Then Exit For
                Dim element As GameBasicUIElement = cursorResult(i)
                element.RaiseMouseDown(e)
            Next
        End If
    End Sub

    Public Sub TriggerMouseUp(e As GameMouseEventArgs)
        Dim cursorResult As List(Of GameBasicUIElement) = GameBasicUIElement.ConvertQuadElements(ElementsQuadtree.Find(e.Position))
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


