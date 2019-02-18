Imports System.Math
Imports System.Text.RegularExpressions

''' <summary>
''' 数学助手模块
''' </summary>
Public Module MathHelper

    Private Rnd As Random = New Random()

    ''' <summary>
    ''' 计算两点间距离
    ''' </summary>
    Public Function GetDistance(a As PointF2, b As PointF2) As Single
        Return Sqrt((a.X - b.X) ^ 2 + (a.Y - b.Y) ^ 2)
    End Function

    ''' <summary>
    ''' 获取0到1之间的随机数
    ''' </summary>
    Public Function GetRandom() As Double
        Return Rnd.NextDouble
    End Function

    ''' <summary>
    ''' 按英文逗号分隔符转换PointI3对象
    ''' </summary>
    Public Function ParsePointI3(input As String) As PointI3
        Dim value() As String = Regex.Split(input, COMMA)
        Dim result As New PointI3 With {
            .X = CShort(value(0)),
            .Y = CShort(value(1)),
            .Z = CShort(value(2))}
        Return result
    End Function
    ''' <summary>
    ''' 按英文逗号分隔符转换PointI对象
    ''' </summary>
    Public Function ParsePointI(input As String) As PointI
        Dim value() As String = Regex.Split(input, COMMA)
        Dim result As New PointI With {
            .X = CShort(value(0)),
            .Y = CShort(value(1))}
        Return result
    End Function
    ''' <summary>
    ''' 按英文逗号分隔符转换PointF3对象
    ''' </summary>
    Public Function ParsePointF3(input As String) As PointF3
        Dim value() As String = Regex.Split(input, COMMA)
        Dim result As New PointF3 With {
            .X = CSng(value(0)),
            .Y = CSng(value(1)),
            .Z = CSng(value(2))}
        Return result
    End Function
    ''' <summary>
    ''' 按英文逗号分隔符转换PointF2对象
    ''' </summary>
    Public Function ParsePointF2(input As String) As PointF2
        Dim value() As String = Regex.Split(input, COMMA)
        Dim result As New PointF2 With {
            .X = CSng(value(0)),
            .Y = CSng(value(1))}
        Return result
    End Function

    Public Function ParseSize(input As String) As SharpDX.Size2
        Dim value() As String = Regex.Split(input, COMMA)
        Dim result As New SharpDX.Size2 With {
            .Width = CShort(value(0)),
            .Height = CShort(value(1))}
        Return result
    End Function

    Public Function Size2RawRect(input As PointI) As SharpDX.Mathematics.Interop.RawRectangleF
        Dim result As New SharpDX.Mathematics.Interop.RawRectangleF With {
            .Left = 0,
            .Top = 0,
            .Right = input.X,
            .Bottom = input.Y}
        Return result
    End Function

    Public Function GetAverage2(a As Single, b As Single) As Single
        Return (a + b) / 2
    End Function

    Public Function RawVec2PointF2(input As SharpDX.Mathematics.Interop.RawVector2) As PointF2
        Return New PointF2(input.X, input.Y)
    End Function

    ''' <summary>
    ''' 简化的PointF结构体
    ''' </summary>
    Public Structure PointF2
        Public X As Single
        Public Y As Single

        Public Sub New(inputX As Single, inputY As Single)
            X = inputX
            Y = inputY
        End Sub
    End Structure

    ''' <summary>
    ''' 受监控的PointF2类
    ''' </summary>
    Public Class PointF2M
        Public X As SingleProperty
        Public Y As SingleProperty

        Public Sub New(inputX As Single, inputY As Single)
            X = New SingleProperty(inputX)
            Y = New SingleProperty(inputY)
        End Sub
    End Class

    ''' <summary>
    ''' XY均为Short类型的Point结构体
    ''' </summary>
    Public Structure PointI
        Public X As Short
        Public Y As Short

        Public Sub New(inputX As Short, inputY As Short)
            X = inputX
            Y = inputY
        End Sub

    End Structure

    ''' <summary>
    ''' XYZ均为Short类型的Point结构体
    ''' </summary>
    Public Structure PointI3
        Public X As Short
        Public Y As Short
        Public Z As Short

        Public Sub New(inputX As Short, inputY As Short, inputZ As Short)
            X = inputX
            Y = inputY
            Z = inputZ
        End Sub

    End Structure

    ''' <summary>
    ''' XYZ均为Single类型的Point结构体
    ''' </summary>
    Public Structure PointF3
        Public X As Single
        Public Y As Single
        Public Z As Single

        Public Sub New(inputX As Single, inputY As Single, inputZ As Single)
            X = inputX
            Y = inputY
            Z = inputZ
        End Sub

    End Structure

    ''' <summary>
    ''' 简化的整形-浮点型键值对
    ''' </summary>
    Public Class KVP_IF
        Public Key As Integer
        Public Value As Single
    End Class

    ''' <summary>
    ''' 三个PointF2结构体
    ''' </summary>
    Public Structure ThreePointF2
        Public Left As PointF2
        Public Middle As PointF2
        Public Right As PointF2
    End Structure

    ''' <summary>
    ''' 四个PointF2结构体
    ''' </summary>
    Public Structure FourPointF2
        Public TL As PointF2
        Public TR As PointF2
        Public BR As PointF2
        Public BL As PointF2
    End Structure

    ''' <summary>
    ''' 矩阵结构体
    ''' </summary>
    Public Structure MathMatrixS
        Public Width As Byte
        Public Height As Byte
        Public Value(,) As Single

        Private IsSquare As Boolean

        Public Sub New(Optional matrixWidth As Byte = 4, Optional matrixHeight As Byte = 4)
            With Me
                .Width = matrixWidth
                .Height = matrixHeight
                If matrixWidth = matrixHeight Then
                    .IsSquare = True
                Else
                    .IsSquare = False
                End If
                ReDim .Value(matrixWidth - 1, matrixHeight - 1)
                For j = 0 To matrixHeight - 1
                    For i = 0 To matrixWidth - 1
                        .Value(i, j) = 0
                    Next
                Next
            End With
        End Sub

        Public Sub SetAsIdentity()
            If IsSquare Then
                For j = 0 To Height - 1
                    For i = 0 To Width - 1
                        If i = j Then
                            Value(i, j) = 1
                        Else
                            Value(i, j) = 0
                        End If
                    Next
                Next
            Else
                Throw New Exception("Identity matrix is not available")
            End If
        End Sub

        Public Function ToRawMatrix() As SharpDX.Mathematics.Interop.RawMatrix
            Dim result As New SharpDX.Mathematics.Interop.RawMatrix
            If Me.Height = 4 AndAlso Me.Width = 4 Then
                With result
                    .M11 = Value(0, 0)
                    .M12 = Value(1, 0)
                    .M13 = Value(2, 0)
                    .M14 = Value(3, 0)
                    .M21 = Value(0, 1)
                    .M22 = Value(1, 1)
                    .M23 = Value(2, 1)
                    .M24 = Value(3, 1)
                    .M31 = Value(0, 2)
                    .M32 = Value(1, 2)
                    .M33 = Value(2, 2)
                    .M34 = Value(3, 2)
                    .M41 = Value(0, 3)
                    .M42 = Value(1, 3)
                    .M43 = Value(2, 3)
                    .M44 = Value(3, 3)
                End With
            End If
            Return result
        End Function

        Public Function ToMatrix32() As SharpDX.Mathematics.Interop.RawMatrix3x2
            Dim result As New SharpDX.Mathematics.Interop.RawMatrix3x2
            With result
                .M11 = Value(0, 0)
                .M12 = Value(0, 1)
                .M21 = Value(1, 0)
                .M22 = Value(1, 1)
                .M31 = Value(2, 0)
                .M32 = Value(2, 1)
            End With
            Return result
        End Function

        Public Shared Operator *(a As MathMatrixS, b As MathMatrixS) As MathMatrixS
            If a.Width <> b.Height Then Return Nothing
            Dim result As New MathMatrixS(b.Width, a.Height)
            For j = 0 To a.Height - 1
                For i = 0 To b.Width - 1
                    For k = 0 To b.Height - 1
                        result.Value(i, j) += a.Value(k, j) * b.Value(i, k)
                    Next
                Next
            Next
            Return result
        End Operator

    End Structure

    ''' <summary>
    ''' 贝塞尔钢笔曲线
    ''' </summary>
    Public Class BezierPenCurve
        ''' <summary>
        ''' 从(0,0)到(1,1)的锚点
        ''' </summary>
        Public Anchor As ThreePointF2()

        ''' <summary>
        ''' 根据x计算y，y属于[0,1]
        ''' </summary>
        ''' <param name="x">横坐标，x属于[0,1]</param>
        Public Function GetValue(x As Single) As Single
            'HACK: Not implemented yet
            Return x
        End Function

        Public Sub ParseAnchors(input As String)
            Dim anchorList As New List(Of ThreePointF2)
            Dim anchorPairs() As String = Regex.Split(input, SEMICOLON)
            For Each tmpPair As String In anchorPairs
                Dim args() As String = Regex.Split(tmpPair, COMMA)
                anchorList.Add(New ThreePointF2 With {
                                 .Left = New PointF2(CSng(args(0)), CSng(args(1))),
                                 .Middle = New PointF2(CSng(args(2)), CSng(args(3))),
                                 .Right = New PointF2(CSng(args(4)), CSng(args(5)))
                               })
            Next
            Me.Anchor = anchorList.ToArray
            anchorList = Nothing
        End Sub

    End Class

End Module


