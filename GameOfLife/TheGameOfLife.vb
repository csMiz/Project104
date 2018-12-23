' -----------------------------------------
' Copyright (c) 2018 All Rights Reserved.
' 
' Filename: TheGameOfLife
' Author: Miz
' Date: 2018/12/22 20:40:42
' -----------------------------------------

Public Class TheGameOfLife

    Public MapBuffer(,) As Byte
    Public Width As Short = 0
    Public Height As Short = 0

    Public Sub Initialize(inputWidth As Short, inputHeight As Short)
        Me.Width = inputWidth
        Me.Height = inputHeight
        ReDim Me.MapBuffer((inputWidth \ 8) - 1, inputHeight - 1)
    End Sub

    Public Sub Process()
        Dim n_map(,) As Byte = Nothing
        ReDim n_map(Width - 1, Height - 1)
        For i = 0 To Width - 1
            For j = 0 To Height - 1
                n_map(i, j) = 0
            Next
        Next

        Dim targetX As Short, targetY As Short
        For j = 0 To Height - 1
            For i = 0 To Width - 1
                If (MapBuffer(i \ 8, j) >> (7 - (i Mod 8))) And &H1 Then
                    For n = -1 To 1
                        targetY = j + n
                        If targetY < 0 Then
                            targetY += Height
                        ElseIf targetY >= Height Then
                            targetY -= Height
                        End If
                        For m = -1 To 1
                            targetX = i + m
                            If targetX < 0 Then
                                targetX += Width
                            ElseIf targetX >= Width Then
                                targetX -= Width
                            End If
                            If n <> 0 OrElse m <> 0 Then
                                n_map(targetX, targetY) += 1
                            End If
                        Next
                    Next
                End If
            Next
        Next

        For j = 0 To Height - 1
            For i = 0 To Width - 1
                If n_map(i, j) = 3 Then    'revive the cell
                    If ((MapBuffer(i \ 8, j) >> (7 - (i Mod 8))) And &H1) = 0 Then
                        MapBuffer(i \ 8, j) += &H1 << (7 - (i Mod 8))
                    End If
                ElseIf n_map(i, j) = 2 Then
                    'do nothing
                Else    'decease
                    If (MapBuffer(i \ 8, j) >> (7 - (i Mod 8))) And &H1 Then
                        MapBuffer(i \ 8, j) -= &H1 << (7 - (i Mod 8))
                    End If
                End If
            Next
        Next

    End Sub

    Public Function GetResult() As Byte()
        Dim result(Height * (Width \ 8) - 1) As Byte
        For j = 0 To Height - 1
            For i = 0 To (Width \ 8) - 1
                result(j * (Width \ 8) + i) = MapBuffer(i, j)
            Next
        Next
        Return result
    End Function

    Public Sub DebugResult()
        Debug.Write("-----------" & vbCrLf)
        For j = 0 To Height - 1
            For i = 0 To Width - 1
                If (MapBuffer(i \ 8, j) >> (7 - (i Mod 8))) And &H1 Then
                    Debug.Write("# ")
                Else
                    Debug.Write("_ ")
                End If
            Next
            Debug.Write(vbCrLf)
        Next
        Debug.Write("-----------" & vbCrLf)
    End Sub

End Class
