' -----------------------------------------
' Copyright (c) 2019 All Rights Reserved.
' 
' Filename: GameCamera3D
' Author: Miz
' Date: 2019/1/12 10:29:23
' -----------------------------------------

Imports SharpDX
Imports SharpDX.Direct3D11
Imports SharpDX.Direct2D1
Imports SharpDX.Mathematics.Interop
Imports SharpDX.D3DCompiler

''' <summary>
''' 游戏三维摄像机类
''' </summary>
Public Class GameCamera3D
    ''' <summary>
    ''' 是否开机
    ''' </summary>
    Public Enable As Boolean = False
    ''' <summary>
    ''' 摄像机位置
    ''' </summary>
    Public Position As RawVector3
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
    Public ZNear As Single = 100
    ''' <summary>
    ''' 远处屏幕，即最远可见位置
    ''' </summary>
    Public ZFar As Single = 5000

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
    ''' 世界容器，所有绘制的物体都要放在容器内(1)
    ''' </summary>
    Public WorldContainer As New List(Of Game3DObject2)
    ''' <summary>
    ''' 临时处理容器(2)
    ''' </summary>
    Public ProcessingContainer As New List(Of Game3DFace2_1Bundle)
    ''' <summary>
    ''' 区域容器(3)
    ''' </summary>
    Public RegionContainer As Game3DFace2_1Bundle()
    ''' <summary>
    ''' 区域容器锁，True时阻止对RegionContainer的修改
    ''' </summary>
    Public ContainerLock As Boolean = False

    ''' <summary>
    ''' 鼠标指针在屏幕上的位置偏移值，用于选取三维物体
    ''' </summary>
    Public ScreenCursorOffset As RawVector3
    ''' <summary>
    ''' 当前鼠标指针悬停的对象ID
    ''' </summary>
    Public PointingAt As Integer = -1


    Public BindingHalfResolve As PointF2

    Public InputElementRepository As New List(Of Direct3D11.InputElement())

    Public InputSignatureRepository As ShaderSignature()

    Public HLSLFileEffectRepository As Direct3D11.Effect()

    'Public VertexShaderRepository As Direct3D11.VertexShader()

    'Public PixelShaderRepository As Direct3D11.PixelShader()

    Public Sub LoadAllShaders(d3dDevice As Direct3D11.Device1, context As Direct3D11.DeviceContext1)
        'set input element
        Dim ie() As Direct3D11.InputElement = {
            New Direct3D11.InputElement("POSITION", 0, DXGI.Format.R32G32B32_Float, 0, 0),
            New Direct3D11.InputElement("NORMAL", 0, DXGI.Format.R32G32B32_Float, 12, 0),
            New Direct3D11.InputElement("COLOR", 0, DXGI.Format.R32G32B32A32_Float, 24, 0),
            New Direct3D11.InputElement("Output", 0, DXGI.Format.R32_UInt, 40, 0)}
        InputElementRepository.Add(ie)
        Dim ie2() As Direct3D11.InputElement = {
            New Direct3D11.InputElement("POSITION", 0, DXGI.Format.R32G32B32_Float, 0, 0),
            New Direct3D11.InputElement("NORMAL", 0, DXGI.Format.R32G32B32_Float, 12, 0),
            New Direct3D11.InputElement("COLOR", 0, DXGI.Format.R32G32B32A32_Float, 24, 0)}
        InputElementRepository.Add(ie2)

        'set input signature and load vertex shaders, pixel shader
        Dim fileList As New List(Of KeyValuePair(Of Integer, String))
        Dim dirInfo As New System.IO.DirectoryInfo(Application.StartupPath & "\Resources\Models\ModelConfig\")
        Dim allFiles() As System.IO.FileInfo = dirInfo.GetFiles
        For Each file As System.IO.FileInfo In allFiles
            Dim name As String = file.Name
            Dim args() As String = name.Remove(name.Length - 4).Split("_")
            Dim nameIndex As Integer = CInt(args.Last)
            fileList.Add(New KeyValuePair(Of Integer, String)(nameIndex, name))
        Next
        fileList.Sort(New Comparison(Of KeyValuePair(Of Integer, String))(Function(a As KeyValuePair(Of Integer, String), b As KeyValuePair(Of Integer, String)) As Integer
                                                                              Return a.Key - b.Key
                                                                          End Function))

        'ReDim VertexShaderRepository(fileList.Count - 1)
        ReDim InputSignatureRepository(fileList.Count - 1)
        'ReDim PixelShaderRepository(fileList.Count - 1)
        ReDim HLSLFileEffectRepository(fileList.Count - 1)
        For Each tmpPair In fileList
            Dim tmpIndex As Integer = tmpPair.Key
            Dim tmpName As String = Application.StartupPath & "\Resources\Models\ModelConfig\" & tmpPair.Value

            Using bytecode As ShaderBytecode = SharpDX.D3DCompiler.ShaderBytecode.CompileFromFile(tmpName, "VShader", "vs_4_0", ShaderFlags.None, EffectFlags.None)
                InputSignatureRepository(tmpIndex) = ShaderSignature.GetInputSignature(bytecode)
                'VertexShaderRepository(tmpIndex) = New VertexShader(d3dDevice, bytecode)
            End Using
            'Using bytecode As ShaderBytecode = SharpDX.D3DCompiler.ShaderBytecode.CompileFromFile(tmpName, "PShader", "ps_4_0", ShaderFlags.None, EffectFlags.None)
            '    PixelShaderRepository(tmpIndex) = New PixelShader(d3dDevice, bytecode)
            'End Using
            Using bytecode As ShaderBytecode = SharpDX.D3DCompiler.ShaderBytecode.CompileFromFile(tmpName, "fx_5_0", ShaderFlags.None, EffectFlags.None)
                HLSLFileEffectRepository(tmpIndex) = New Direct3D11.Effect(d3dDevice, bytecode)
            End Using
        Next

    End Sub

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
            .Value(1, 2) = Math.Sin(Rotation.X)
            .Value(2, 1) = - .Value(1, 2)
            .Value(2, 2) = .Value(1, 1)
        End With
    End Sub
    Public Sub CalculateViewRY()
        With View_RY
            .SetAsIdentity()
            .Value(0, 0) = Math.Cos(Rotation.Y)
            .Value(2, 0) = Math.Sin(Rotation.Y)
            .Value(0, 2) = - .Value(2, 0)
            .Value(2, 2) = .Value(0, 0)
        End With
    End Sub
    Public Sub CalculateViewRZ()
        With View_RZ
            .SetAsIdentity()
            .Value(0, 0) = Math.Cos(Rotation.Z)
            .Value(0, 1) = Math.Sin(Rotation.Z)
            .Value(1, 0) = - .Value(0, 1)
            .Value(1, 1) = .Value(0, 0)
        End With
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
        WVP_Source = Projection * (View_RZ * View_RY) * (View_RX * View_Position)
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
            Return BindingHalfResolve
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

    Public Sub SetCursorRayStatus(status As Single)
        ScreenCursorOffset.Z = status
    End Sub

    Public Sub FixContainer3D(d3dDevice As Direct3D11.Device1, context As Direct3D11.DeviceContext1)
        '采用动态载入
        '世界容器分3层：
        '1-总世界容器，包含了所有3D对象
        '2-临时区域容器，包含了正在载入的候选区3D对象
        '3-区域容器，用于实际渲染
        '当视角变化时，对1号总世界容器内的对象进行遍历，重新计算候选区，将处于候选区内的3D对象添加到2号临时容器内
        '只有个别物体变化时，可以仅调用Game3DFace2_1Bundle.RefreshBuffer()进行更新（例如骨骼动画）
        '当计算完成后，将2号临时容器写入3号区域容器，清空2号容器

        '此方法需要另开一个线程

        ProcessingContainer.Clear()
        For Each tmpObj As Game3DObject2 In Me.WorldContainer
            '判断是否绘制
            Dim checkArray As PointF3() = tmpObj.RegionCheckSign
            Dim isInside As Boolean = False
            Dim border As PointF2 = BindingHalfResolve
            For Each tmpCheck As PointF3 In checkArray
                Dim screenPoint As PointF2 = ApplyWVP_PF2(tmpCheck)
                If Math.Abs(screenPoint.X - border.X) < 1.5 * border.X AndAlso Math.Abs(screenPoint.Y - border.Y) < 1.5 * border.Y Then
                    isInside = True
                    Exit For
                End If
            Next
            '写入2号容器
            If isInside Then
                Dim matchShaderIndex As Integer = -1
                If ProcessingContainer.Count Then
                    For i = 0 To ProcessingContainer.Count - 1
                        If ProcessingContainer(i).ShaderIndex = tmpObj.ShaderIndex Then
                            matchShaderIndex = i
                            Exit For
                        End If
                    Next
                End If
                If matchShaderIndex = -1 Then
                    ProcessingContainer.Add(New Game3DFace2_1Bundle With {.ShaderIndex = tmpObj.ShaderIndex})
                    matchShaderIndex = ProcessingContainer.Count - 1
                End If
                ProcessingContainer(matchShaderIndex).Faces.AddRange(tmpObj.SourceFaces)
            End If
        Next

        For Each tmpBundle As Game3DFace2_1Bundle In ProcessingContainer
            tmpBundle.RefreshBuffer()
        Next
        '写入3号容器
        While ContainerLock
            'wait
        End While
        ContainerLock = True
        If RegionContainer IsNot Nothing Then
            For Each oldBundle As Game3DFace2_1Bundle In RegionContainer
                oldBundle.Dispose()
            Next
        End If
        RegionContainer = ProcessingContainer.ToArray
        ContainerLock = False

    End Sub

    Public Sub UpdateOneBundle(bundleIndex As Integer)
        While ContainerLock
            'wait
        End While
        ContainerLock = True
        Dim tmpBundle As Game3DFace2_1Bundle = ProcessingContainer(bundleIndex)
        tmpBundle.RefreshBuffer()
        ContainerLock = False
    End Sub

    ''' <summary>
    ''' 绘制3号容器内的三维面，使用D3D11
    ''' </summary>
    Public Sub DrawContainer3D(d3dDevice As Direct3D11.Device1, context As Direct3D11.DeviceContext1)
        While ContainerLock
            Application.DoEvents()    'wait
        End While
        ContainerLock = True

        'Draw d3d11
        For Each tmpBundle As Game3DFace2_1Bundle In RegionContainer
            Dim shaderIndex As Integer = tmpBundle.ShaderIndex
            Dim layout As InputLayout = New InputLayout(d3dDevice, InputSignatureRepository(shaderIndex).Data, InputElementRepository(shaderIndex))
            Dim vertexBuffer As Buffer = New Buffer(d3dDevice, tmpBundle.Buffer, tmpBundle.Buffer.Length, ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0)

            With context
                'set input format
                .InputAssembler.InputLayout = layout
                .InputAssembler.PrimitiveTopology = Direct3D.PrimitiveTopology.TriangleList
                .InputAssembler.SetVertexBuffers(0, New VertexBufferBinding(vertexBuffer, GetShaderInputLength(shaderIndex), 0))


                '[method1: use VS/PS and set ConstantBuffer separately]-------------
                'set VS/PS
                '.VertexShader.Set(VertexShaderRepository(shaderIndex))
                '.PixelShader.Set(PixelShaderRepository(shaderIndex))
                '-------------------------------------------------------------------
                '[method2: use direct3d11.effect class]-----------------------------

                With HLSLFileEffectRepository(shaderIndex)
                    .GetVariableByName("GlobalWVP").AsMatrix().SetMatrix(Me.WVP)
                    If shaderIndex = 0 Then
                        .GetVariableByName("CursorInput").AsVector().Set(Me.ScreenCursorOffset)
                        .GetVariableByName("SelectedObjectId").AsScalar().Set(Me.PointingAt)
                        .GetVariableByName("ScreenHHeight").AsScalar().Set(Me.BindingHalfResolve.Y)
                        .GetVariableByName("ScreenHWidth").AsScalar().Set(Me.BindingHalfResolve.X)
                    End If
                    .GetTechniqueByIndex(0).GetPassByIndex(0).Apply(context)
                End With
                '-------------------------------------------------------------------
                'render
                .Draw(tmpBundle.Faces.Count * 3, 0)
                '                .Draw(tmpBundle.Faces.Count * 3, 0)

            End With

            layout.Dispose()
            vertexBuffer.Dispose()
        Next

        ContainerLock = False
    End Sub

    ''' <summary>
    ''' 绘制世界容器内的物体，仅用于D2D
    ''' </summary>
    ''' <param name="context">d2dContext对象</param>
    ''' <param name="spectator">观察者对象</param>
    ''' <param name="canvasBitmap">原画布对象</param>
    <Obsolete("bad performance", False)>
    Public Sub DrawContainer(ByRef context As SharpDX.Direct2D1.DeviceContext, ByRef spectator As SpectatorCamera, canvasBitmap As SharpDX.Direct2D1.Bitmap1)
        Dim light As New PointF3(0, 0.5, -0.866)
        context.EndDraw()

        If WorldContainer.Count = 0 Then Exit Sub
        For objIndex As Integer = WorldContainer.Count - 1 To 0 Step -1
            Dim tmpObject As Game3dObject = Nothing 'Me.WorldContainer(objIndex)
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
                            .SetFillMode(Direct2D1.FillMode.Winding)
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

    Public Function GetShaderInputLength(shaderIndex As Integer) As Integer
        Select Case shaderIndex
            Case 0
                Return VertexShaderInputLength.Position_Normal_Color_Tag
            Case 1
                Return VertexShaderInputLength.Position_Normal_Color

        End Select
        Return 0
    End Function
End Class

Public Enum VertexShaderInputLength As Integer
    Position_Normal_Color_Tag = 44
    Position_Normal_Color = 40

End Enum
