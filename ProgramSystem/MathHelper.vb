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

    Public Function ParseRawVector3(input As String) As SharpDX.Mathematics.Interop.RawVector3
        Dim value() As String = Regex.Split(input, COMMA)
        Dim result As New SharpDX.Mathematics.Interop.RawVector3 With {
            .X = CSng(value(0)),
            .Y = CSng(value(1)),
            .Z = CSng(value(2))}
        Return result
    End Function

    Public Function ParseRawColor4(input As String) As SharpDX.Mathematics.Interop.RawColor4
        Dim value() As String = Regex.Split(input, COMMA)
        Dim result As New SharpDX.Mathematics.Interop.RawColor4 With {
            .R = CShort(value(0)) / 255.0F,
            .G = CShort(value(1)) / 255.0F,
            .B = CShort(value(2)) / 255.0F,
            .A = CShort(value(3)) / 255.0F}
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

    Public Function PF3Dot(a As PointF3, b As PointF3) As Single
        Return a.X * b.X + a.Y * b.Y + a.Z * b.Z
    End Function

    Public Function RawVec2PointF2(input As SharpDX.Mathematics.Interop.RawVector2) As PointF2
        Return New PointF2(input.X, input.Y)
    End Function
    Public Function PointF22RawVec(input As PointF2) As SharpDX.Mathematics.Interop.RawVector2
        Return New SharpDX.Mathematics.Interop.RawVector2(input.X, input.Y)
    End Function

    Public Function PointF32RV3(input As PointF3) As SharpDX.Mathematics.Interop.RawVector3
        Return New SharpDX.Mathematics.Interop.RawVector3(input.X, input.Y, input.Z)
    End Function

    Public Function CalculatePerspMatrix(input() As KeyValuePair(Of PointF2, PointF2)) As Double()
        Dim x0 As Double = input(0).Key.X
        Dim y0 As Double = input(0).Key.Y
        Dim x1 As Double = input(0).Value.X
        Dim y1 As Double = input(0).Value.Y
        Dim x2 As Double = input(1).Key.X
        Dim y2 As Double = input(1).Key.Y
        Dim x3 As Double = input(1).Value.X
        Dim y3 As Double = input(1).Value.Y
        Dim x4 As Double = input(2).Key.X
        Dim y4 As Double = input(2).Key.Y
        Dim x5 As Double = input(2).Value.X
        Dim y5 As Double = input(2).Value.Y
        Dim x6 As Double = input(3).Key.X
        Dim y6 As Double = input(3).Key.Y
        Dim x7 As Double = input(3).Value.X
        Dim y7 As Double = input(3).Value.Y

        Dim args(71) As Double
        args(0) = x0
        args(1) = 0
        args(2) = -x0 * x1
        args(3) = y0
        args(4) = 0
        args(5) = -x1 * y0
        args(6) = 1
        args(7) = 0
        args(8) = x1

        args(9) = 0
        args(10) = x0
        args(11) = -x0 * y1
        args(12) = 0
        args(13) = y0
        args(14) = -y0 * y1
        args(15) = 0
        args(16) = 1
        args(17) = y1

        args(18) = x2
        args(19) = 0
        args(20) = -x2 * x3
        args(21) = y2
        args(22) = 0
        args(23) = -x3 * y2
        args(24) = 1
        args(25) = 0
        args(26) = x3

        args(27) = 0
        args(28) = x2
        args(29) = -x2 * y3
        args(30) = 0
        args(31) = y2
        args(32) = -y2 * y3
        args(33) = 0
        args(34) = 1
        args(35) = y3

        args(36) = x4
        args(37) = 0
        args(38) = -x4 * x5
        args(39) = y4
        args(40) = 0
        args(41) = -x5 * y4
        args(42) = 1
        args(43) = 0
        args(44) = x5

        args(45) = 0
        args(46) = x4
        args(47) = -x4 * y5
        args(48) = 0
        args(49) = y4
        args(50) = -y4 * y5
        args(51) = 0
        args(52) = 1
        args(53) = y5

        args(54) = x6
        args(55) = 0
        args(56) = -x6 * x7
        args(57) = y6
        args(58) = 0
        args(59) = -x7 * y6
        args(60) = 1
        args(61) = 0
        args(62) = x7

        args(63) = 0
        args(64) = x6
        args(65) = -x6 * y7
        args(66) = 0
        args(67) = y6
        args(68) = -y6 * y7
        args(69) = 0
        args(70) = 1
        args(71) = y7

        Dim matrix As Double() = Gauss_Jordan_Elimination(8, args)
        Return matrix
    End Function

    ''' <summary>
    ''' 高斯约旦消元法
    ''' </summary>
    Public Function Gauss_Jordan_Elimination(varCount As Integer, args() As Double) As Double()
        '读取系数
        Dim matrix_x As Integer = varCount + 1
        Dim matrix_y As Integer = varCount
        Dim equations(matrix_x - 1, matrix_y - 1) As Double
        Dim readIndex As Integer = 0
        For j = 0 To matrix_y - 1
            For i = 0 To matrix_x - 1
                equations(i, j) = args(readIndex)
                readIndex += 1
            Next
        Next
        '计算
        For totalLoopIndex = 0 To matrix_y - 2
            If equations(totalLoopIndex, totalLoopIndex) = 0 Then
                For elseLine = totalLoopIndex + 1 To matrix_y - 1
                    If equations(totalLoopIndex, elseLine) <> 0 Then
                        '交换两行
                        Dim tmpLine(matrix_x - 1) As Double
                        For i = 0 To matrix_x - 1
                            tmpLine(i) = equations(i, totalLoopIndex)
                        Next
                        For i = 0 To matrix_x - 1
                            equations(i, totalLoopIndex) = equations(i, elseLine)
                        Next
                        For i = 0 To matrix_x - 1
                            equations(i, elseLine) = tmpLine(i)
                        Next
                        Exit For
                    End If
                Next
            End If
            '系数归一
            For lineIndex = totalLoopIndex To matrix_y - 1
                Dim div As Double = equations(totalLoopIndex, lineIndex)
                If div <> 0 AndAlso div <> 1 Then
                    For factorIndex = totalLoopIndex To matrix_x - 1
                        equations(factorIndex, lineIndex) /= div
                    Next
                End If
            Next
            '消元
            For lineIndex = totalLoopIndex + 1 To matrix_y - 1
                If equations(totalLoopIndex, lineIndex) <> 0 Then
                    For factorIndex = totalLoopIndex To matrix_x - 1
                        equations(factorIndex, lineIndex) -= equations(factorIndex, totalLoopIndex)
                    Next
                End If
            Next
        Next

        Dim result(varCount - 1) As Double
        result(varCount - 1) = equations(matrix_x - 1, matrix_y - 1) / equations(matrix_x - 2, matrix_y - 1)
        For j = varCount - 2 To 0 Step -1
            Dim tmpValue As Double = equations(matrix_x - 1, j)
            For i = j + 1 To matrix_x - 2
                tmpValue -= equations(i, j) * result(i)
            Next
            result(j) = tmpValue
        Next

        Return result

    End Function

    Public Function WrapD2dMatrix(input As Double()) As SharpDX.Mathematics.Interop.RawMatrix
        Dim mat As New SharpDX.Mathematics.Interop.RawMatrix()
        '
        '之前计算出的3x3矩阵需要转换，不能直接用
        '3x3扩展到4x4：
        '  a11  a12  0    a13
        '  a21  a22  0    a23
        '  0    0    0或1 0
        '  a31  a32  0    a33(恒为1)
        '
        With mat
            .M11 = input(0)
            .M12 = input(1)
            .M13 = 0
            .M14 = input(2)
            .M21 = input(3)
            .M22 = input(4)
            .M23 = 0
            .M24 = input(5)
            .M31 = 0
            .M32 = 0
            .M33 = 1
            .M34 = 0
            .M41 = input(6)
            .M42 = input(7)
            .M43 = 0
            .M44 = 1
        End With
        Return mat
    End Function


End Module


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

        Public Sub New(copy As PointF3)
            X = copy.X
            Y = copy.Y
            Z = copy.Z
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



