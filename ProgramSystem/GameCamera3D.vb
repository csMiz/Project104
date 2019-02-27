' -----------------------------------------
' Copyright (c) 2019 All Rights Reserved.
' 
' Filename: GameCamera3D
' Author: Miz
' Date: 2019/1/12 10:29:23
' -----------------------------------------

Imports SharpDX.Direct2D1
Imports SharpDX.Mathematics.Interop
''' <summary>
''' 游戏三维摄像机类
''' </summary>
Public Class GameCamera3D
    ''' <summary>
    ''' 摄像机位置
    ''' </summary>
    Public Position As PointF3
    ''' <summary>
    ''' 摄像机方向，旋转顺序依次为XYZ，逆时针，弧度
    ''' </summary>
    Public Rotation As PointF3
    ''' <summary>
    ''' 摄像机视野角，使用弧度(rad)
    ''' </summary>
    Public FOV As Single = Math.PI / 6
    ''' <summary>
    ''' 近处屏幕，即最近可见位置（显示屏位置）
    ''' </summary>
    Public ZNear As Single = 1
    ''' <summary>
    ''' 远处屏幕，即最远可见位置
    ''' </summary>
    Public ZFar As Single = 1000

    Private View_Position As New MathMatrixS(4, 4)

    Private View_RX As New MathMatrixS(4, 4)

    Private View_RY As New MathMatrixS(4, 4)

    Private View_RZ As New MathMatrixS(4, 4)

    Private Projection As New MathMatrixS(4, 4)

    Private WVP_Source As New MathMatrixS(4, 4)
    ''' <summary>
    ''' 提供给GPU的WVP变换矩阵
    ''' </summary>
    Public WVP As SharpDX.Mathematics.Interop.RawMatrix
    ''' <summary>
    ''' 世界容器，所有绘制的物体都要放在容器内
    ''' </summary>
    Public WorldContainer As New List(Of Game3dObject)

    Public BindingHalfResolve As PointF

    Public Sub CalculateViewP()
        With View_Position
            .SetAsIdentity()
            .Value(3, 0) = -Position.X
            .Value(3, 1) = -Position.Y
            .Value(3, 2) = -Position.Z
        End With
    End Sub

    Public Sub CalculateViewRX()
        With View_RX
            .SetAsIdentity()
            .Value(1, 1) = Math.Cos(Rotation.X)
            .Value(2, 1) = -Math.Sin(Rotation.X)
            .Value(1, 2) = Math.Sin(Rotation.X)
            .Value(2, 2) = Math.Cos(Rotation.X)
        End With
    End Sub

    Public Sub CalculateViewRY()
        With View_RY
            .SetAsIdentity()
        End With
        'TODO
    End Sub

    Public Sub CalculateViewRZ()
        With View_RZ
            .SetAsIdentity()
        End With
        'TODO
    End Sub

    Public Sub RefreshProjection()
        With Projection
            .Value(0, 0) = 1 / Math.Tan(FOV / 2)
            .Value(1, 1) = 1 / Math.Tan(FOV / 2)
            .Value(2, 2) = -((ZNear + ZFar) / (ZFar - ZNear))
            .Value(3, 3) = 0
            .Value(2, 3) = -1
            .Value(3, 2) = -(2 * ZFar * ZNear / (ZFar - ZNear))
        End With
    End Sub

    Public Sub CalculateWVP()
        WVP_Source = (Projection * (View_RZ * (View_RY * (View_RX * View_Position))))
        WVP = WVP_Source.ToRawMatrix
    End Sub

    Public Function ApplyWVP_PF2(input As PointF3) As PointF2
        Dim inputMat As MathMatrixS = New MathMatrixS(1, 4)
        With inputMat
            .Value(0, 0) = input.X
            .Value(0, 1) = input.Y
            .Value(0, 2) = input.Z
            .Value(0, 3) = 1.0F
        End With
        Dim resultMat As MathMatrixS = WVP_Source * inputMat
        If resultMat.Value(0, 3) = 0 Then
            Return New PointF2(BindingHalfResolve.X, BindingHalfResolve.Y)
        End If
        Return New PointF2(BindingHalfResolve.X * (1 + resultMat.Value(0, 0) / resultMat.Value(0, 3)), BindingHalfResolve.Y * (1 - resultMat.Value(0, 1) / resultMat.Value(0, 3)))
    End Function

    Public Function ApplyWVP_RV2(input As PointF3) As RawVector2
        Dim inputMat As MathMatrixS = New MathMatrixS(1, 4)
        With inputMat
            .Value(0, 0) = input.X
            .Value(0, 1) = input.Y
            .Value(0, 2) = input.Z
            .Value(0, 3) = 1.0F
        End With
        Dim resultMat As MathMatrixS = WVP_Source * inputMat
        If resultMat.Value(0, 3) = 0 Then
            Return New RawVector2(BindingHalfResolve.X, BindingHalfResolve.Y)
        End If
        Return New RawVector2(BindingHalfResolve.X * (1 + resultMat.Value(0, 0) / resultMat.Value(0, 3)), BindingHalfResolve.Y * (1 - resultMat.Value(0, 1) / resultMat.Value(0, 3)))
    End Function

    ''' <summary>
    ''' 绘制世界容器内的物体
    ''' </summary>
    ''' <param name="context">d2dContext对象</param>
    ''' <param name="spectator">观察者对象</param>
    ''' <param name="canvasBitmap">原画布对象</param>
    Public Sub DrawContainer(ByRef context As SharpDX.Direct2D1.DeviceContext, ByRef spectator As SpectatorCamera, canvasBitmap As SharpDX.Direct2D1.Bitmap1)
        Dim light As New PointF3(0, 0.5, -0.866)
        context.EndDraw()

        If WorldContainer.Count = 0 Then Exit Sub
        For objIndex As Integer = WorldContainer.Count - 1 To 0 Step -1
            Dim tmpObject As Game3dObject = Me.WorldContainer(objIndex)
            '判断是否绘制
            Dim checkArray As PointF3() = tmpObject.RegionCheckSign
            Dim isInside As Boolean = False
            Dim border As New PointF(spectator.Resolve.X / 2, spectator.Resolve.Y / 2)
            For Each tmpCheck As PointF3 In checkArray
                Dim screenPoint As PointF2 = ApplyWVP_PF2(tmpCheck)
                If Math.Abs(screenPoint.X - border.X) < border.X AndAlso Math.Abs(screenPoint.Y - border.Y) < border.Y Then
                    isInside = True
                    Exit For
                End If
            Next
            '绘制
            If isInside Then
                '测试：绘制所有顶点
                For Each tmpFace As Game3dFace In tmpObject.Faces
                    Dim faceLight As Single = MathHelper.PF3Dot(tmpFace.Normal, light)
                    Dim rFaceLight As Single = -faceLight
                    If faceLight < 0 Then
                        Dim geo As PathGeometry = New PathGeometry(context.Factory)
                        Dim sink As GeometrySink = geo.Open
                        Dim screenPoints(3) As RawVector2
                        screenPoints(0) = ApplyWVP_RV2(tmpFace.Vertices(0))
                        With sink
                            .SetFillMode(FillMode.Winding)
                            .BeginFigure(screenPoints(0), FigureBegin.Filled)
                            Dim sinkPoints() As RawVector2
                            If tmpFace.FaceType = FaceType3D.Three Then
                                screenPoints(1) = ApplyWVP_RV2(tmpFace.Vertices(1))
                                screenPoints(2) = ApplyWVP_RV2(tmpFace.Vertices(2))
                                sinkPoints = {screenPoints(1), screenPoints(2)}
                            Else
                                screenPoints(1) = ApplyWVP_RV2(tmpFace.Vertices(1))
                                screenPoints(2) = ApplyWVP_RV2(tmpFace.Vertices(2))
                                screenPoints(3) = ApplyWVP_RV2(tmpFace.Vertices(3))
                                sinkPoints = {screenPoints(1), screenPoints(2), screenPoints(3)}
                            End If
                            .AddLines(sinkPoints)
                            .EndFigure(FigureEnd.Closed)
                            .Close()
                        End With

                        Dim tmpBrush As New SolidColorBrush(context, New RawColor4(0.25 + 0.5 * rFaceLight, 0.25 + 0.5 * rFaceLight, 0.25 + 0.5 * rFaceLight, 0.75))

                        Dim haveTexture As Boolean = False
                        Dim eff As Effects.Transform3D = Nothing
                        If tmpFace.FixedTexture IsNot Nothing Then
                            haveTexture = True

                            eff = New Effects.Transform3D(context)
                            eff.SetInput(0, tmpFace.FixedTexture, False)
                            Dim points(3) As KeyValuePair(Of PointF2, PointF2)
                            For i = 0 To 3
                                points(i) = New KeyValuePair(Of PointF2, PointF2)(tmpFace.FixedTextureVertices(i), MathHelper.RawVec2PointF2(screenPoints(i)))
                            Next
                            Dim rawPerpsMatrix As Double() = CalculatePerspMatrix(points)
                            Dim mat As RawMatrix = WrapD2dMatrix(rawPerpsMatrix)
                            eff.TransformMatrix = mat

                        End If

                        context.BeginDraw()

                        context.FillGeometry(geo, tmpBrush)
                        If haveTexture Then
                            context.DrawImage(eff)
                        End If

                        context.EndDraw()

                        sink.Dispose()
                        geo.Dispose()
                        tmpBrush.Dispose()
                        If haveTexture Then
                            eff.Dispose()
                        End If

                    End If


                    'context.BeginDraw()
                    'For Each tmpVertex As PointF3 In tmpFace.Vertices
                    '    Dim screenPoint As RawVector2 = ApplyWVP_RV2(tmpVertex)
                    '    context.FillEllipse(New Ellipse(screenPoint, 2, 2), BLACK_COLOUR_BRUSH(4))
                    'Next
                    'context.EndDraw()

                Next


            End If
        Next
        context.BeginDraw()
    End Sub


End Class
