' -----------------------------------------
' Copyright (c) 2018 All Rights Reserved.
' 
' Filename: GamePageProperty
' Author: Miz
' Date: 2018/10/30 12:31:03
' -----------------------------------------

Imports SharpDX.Direct2D1

Public Class GamePageProperty

    Public UIElements As New List(Of GameBasicUIElement)
    Public ElementsQuadtree As Quadtree

    Public MouseLastPosition As PointI = Nothing
    'Private ElementsMouseOutside As New List(Of GameBasicUIElement)
    Private ElementsMouseInside As New List(Of GameBasicUIElement)

    Public Sub PaintElements(ByRef context As DeviceContext, ByRef spec As SpectatorCamera, canvasBitmap As Bitmap1)
        For Each element As GameBasicUIElement In Me.UIElements
            If element.Visible Then
                element.DrawControl(context, spec, canvasBitmap)
            End If
        Next

        context.DrawBitmap(TestTextImage.FontImage, New SharpDX.Mathematics.Interop.RawRectangleF(50, 50, 550, 550), NOT_TRANSPARENT, BitmapInterpolationMode.Linear)
    End Sub

    Public Sub GenerateElementsQuadtree(pageSize As PointI)
        Me.ElementsQuadtree = New Quadtree(MathHelper.Size2RawRect(pageSize))
        For Each element As GameBasicUIElement In UIElements
            Me.ElementsQuadtree.AddItem(element)
        Next
    End Sub

    Public Sub InitializeCursor(setX As Integer, setY As Integer)
        Dim cursorPoint As PointI = New PointI(setX, setY)
        'System.Windows.Forms.Cursor.Position = cursorPoint    'not correct
        Me.MouseLastPosition = cursorPoint

        Dim cursorResult As List(Of IQuadtreeRecognizable) = ElementsQuadtree.Find(cursorPoint)
        If cursorResult.Count Then
            For Each element As GameBasicUIElement In cursorResult
                element.RaiseMouseEnter()
                Me.ElementsMouseInside.Add(element)
            Next
        End If
    End Sub

    ''' <summary>
    ''' 触发MouseMove, MouseEnter, MouseLeave
    ''' </summary>
    Public Sub TriggerMouseMove(e As GameMouseEventArgs)
        Dim cursorResult As List(Of IQuadtreeRecognizable) = ElementsQuadtree.Find(e.Position)
        If cursorResult.Count Then
            Dim refreshElementsInside As New List(Of GameBasicUIElement)
            For Each element As GameBasicUIElement In cursorResult
                If Me.ElementsMouseInside.Contains(element) Then
                    element.RaiseMouseMove(e)
                    Me.ElementsMouseInside.Remove(element)
                Else
                    element.RaiseMouseEnter()
                End If
                refreshElementsInside.Add(element)
            Next
            For Each element As GameBasicUIElement In Me.ElementsMouseInside
                element.RaiseMouseLeave()
            Next
            Me.ElementsMouseInside = refreshElementsInside
        Else
            For Each element As GameBasicUIElement In Me.ElementsMouseInside
                element.RaiseMouseLeave()
            Next
            ElementsMouseInside.Clear()
        End If

        Me.MouseLastPosition = e.Position
    End Sub

    Public Sub TriggerMouseDown(e As GameMouseEventArgs)
        Dim cursorResult As List(Of IQuadtreeRecognizable) = ElementsQuadtree.Find(e.Position)
        If cursorResult.Count Then
            For Each element As GameBasicUIElement In cursorResult
                element.RaiseMouseDown(e)
            Next
        End If
    End Sub

    Public Sub TriggerMouseUp(e As GameMouseEventArgs)
        Dim cursorResult As List(Of IQuadtreeRecognizable) = ElementsQuadtree.Find(e.Position)
        If cursorResult.Count Then
            For Each element As GameBasicUIElement In cursorResult
                element.RaiseMouseUp(e)
            Next
        End If
    End Sub

End Class

