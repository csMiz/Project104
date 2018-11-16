' -----------------------------------------
' Copyright (c) 2018 All Rights Reserved.
' 
' Filename: Quadtree
' Author: Miz
' Date: 2018/10/30 12:40:04
' -----------------------------------------

''' <summary>
''' 四叉树类
''' </summary>
Public Class Quadtree
    Public Shared NinePatchMapping() As QuadtreeDirection = {
        QuadtreeDirection.TopLeft, QuadtreeDirection.Centre, QuadtreeDirection.TopRight,
        QuadtreeDirection.Centre, QuadtreeDirection.Centre, QuadtreeDirection.Centre,
        QuadtreeDirection.BottomLeft, QuadtreeDirection.Centre, QuadtreeDirection.BottomRight}

    Public RegionRectangle As SharpDX.Mathematics.Interop.RawRectangleF
    Private Pivot As PointI
    Public Node As New List(Of IQuadtreeRecognizable)
    Public Children(3) As Quadtree

    Public Sub New(rect As SharpDX.Mathematics.Interop.RawRectangleF)
        Me.RegionRectangle = rect
        Me.Pivot = New PointI(MathHelper.GetAverage2(rect.Right, rect.Left), MathHelper.GetAverage2(rect.Top, rect.Bottom))

    End Sub

    Public Sub AddItem(input As IQuadtreeRecognizable)
        Dim locationResult As QuadtreeDirection = input.CompareRegionDirection(Me.Pivot)
        If locationResult = QuadtreeDirection.Centre Then
            Me.Node.Add(input)
        Else
            If Me.Children(locationResult) Is Nothing Then
                Dim childRect As SharpDX.Mathematics.Interop.RawRectangleF = Nothing
                If locationResult = QuadtreeDirection.TopRight Then
                    childRect = New SharpDX.Mathematics.Interop.RawRectangleF(Me.Pivot.X, Me.RegionRectangle.Top, Me.RegionRectangle.Right, Me.Pivot.Y)
                ElseIf locationResult = QuadtreeDirection.TopLeft Then
                    childRect = New SharpDX.Mathematics.Interop.RawRectangleF(Me.RegionRectangle.Left, Me.RegionRectangle.Top, Me.Pivot.X, Me.Pivot.Y)
                ElseIf locationResult = QuadtreeDirection.BottomLeft Then
                    childRect = New SharpDX.Mathematics.Interop.RawRectangleF(Me.RegionRectangle.Left, Me.Pivot.Y, Me.Pivot.X, Me.RegionRectangle.Bottom)
                ElseIf locationResult = QuadtreeDirection.BottomRight Then
                    childRect = New SharpDX.Mathematics.Interop.RawRectangleF(Me.Pivot.X, Me.Pivot.Y, Me.RegionRectangle.Right, Me.RegionRectangle.Bottom)
                Else
                    Throw New Exception("region not correct!")
                End If
                Me.Children(locationResult) = New Quadtree(childRect)
            End If
            Me.Children(locationResult).AddItem(input)
        End If
    End Sub

    Public Function Find(point As PointI) As List(Of IQuadtreeRecognizable)

        Dim result As New List(Of IQuadtreeRecognizable)

        For Each item As IQuadtreeRecognizable In Me.Node
            If item.IsValid AndAlso item.IsInside(point) Then
                result.Add(item)
            End If
        Next

        Dim nextDirection As QuadtreeDirection = 0
        If point.X >= Me.Pivot.X AndAlso point.Y < Me.Pivot.Y Then
            nextDirection = QuadtreeDirection.TopRight
        ElseIf point.X < Me.Pivot.X AndAlso point.Y < Me.Pivot.Y Then
            nextDirection = QuadtreeDirection.TopLeft
        ElseIf point.X < Me.Pivot.X AndAlso point.Y >= Me.Pivot.Y Then
            nextDirection = QuadtreeDirection.BottomLeft
        ElseIf point.X >= Me.Pivot.X AndAlso point.Y >= Me.Pivot.Y Then
            nextDirection = QuadtreeDirection.BottomRight
        End If

        If Me.Children(nextDirection) IsNot Nothing Then
            Dim nextResult As List(Of IQuadtreeRecognizable) = Me.Children(nextDirection).Find(point)
            result.AddRange(nextResult)
        End If

        Return result
    End Function

End Class


Public Interface IQuadtreeRecognizable

    Function CompareRegionDirection(input As PointI) As QuadtreeDirection
    Function IsInside(input As PointI) As Boolean
    Function IsValid() As Boolean

End Interface

Public Enum QuadtreeDirection As Byte
    TopLeft = 1
    TopRight = 0
    BottomRight = 3
    BottomLeft = 2

    Centre = 4
End Enum
