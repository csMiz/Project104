' -----------------------------------------
' Copyright (c) 2019 All Rights Reserved.
' 
' Filename: GameLive2dImageAnimation
' Author: Miz
' Date: 2019/1/22 13:22:52
' -----------------------------------------

Imports SharpDX.Direct2D1
Imports SharpDX.Mathematics.Interop

Public Class GameLive2dImageAnimation
    ''' <summary>
    ''' 动画名称
    ''' </summary>
    Public AnimationName As String = vbNullString
    ''' <summary>
    ''' 动画一个循环的帧数
    ''' </summary>
    Public AnimationLength As Integer = 1
    ''' <summary>
    ''' 每秒播放帧数，默认60
    ''' </summary>
    Public AnimationFPS As Integer = 60
    ''' <summary>
    ''' 每张精灵素材的关键帧列表
    ''' </summary>
    Public KeyFrames() As List(Of LiveImageKeyFrame)
    ''' <summary>
    ''' 存放所有帧的缓存图像
    ''' </summary>
    Private AnimationBuffer() As Bitmap1

    Public HaveRendered As Boolean = False

    Public BindingLive2dImage As GameLive2dImage = Nothing

    Public PlayingCursor As Integer = 0

    ''' <summary>
    ''' 销毁缓存中所有的bitmap1
    ''' </summary>
    Public Sub ClearBuffer()
        Me.HaveRendered = False
        For Each img As Bitmap1 In Me.AnimationBuffer
            img.Dispose()
        Next
    End Sub

    Private Function SearchKeyFrameSection(kfList As List(Of LiveImageKeyFrame), frameIndex As Integer) As LiveImageKeyFrame()
        'binary search
        Dim kf_before_index As Integer
        Dim kf_before As LiveImageKeyFrame
        Dim kf_after As LiveImageKeyFrame
        Dim lb As Integer = 0
        Dim ub As Integer = kfList.Count - 1
        Dim mid As Integer = Math.Ceiling((lb + ub) / 2)
        Dim loopCount As Integer = 0
        While loopCount < 200
            If frameIndex = kfList(mid).Frame Then
                kf_before_index = mid
                Exit While
            ElseIf frameIndex > kfList(mid).Frame Then
                lb = mid
            Else
                ub = mid
            End If
            mid = Math.Ceiling((ub + lb) / 2)
            If mid = ub Then
                kf_before_index = lb
                Exit While
            End If
            loopCount += 1
        End While
        If loopCount >= 200 Then Throw New Exception("loop error")
        kf_before = kfList(kf_before_index)
        If kf_before_index = kfList.Count - 1 Then
            kf_after = kf_before
        Else
            kf_after = kfList(kf_before_index + 1)
        End If
        Dim result(1) As LiveImageKeyFrame
        result(0) = kf_before
        result(1) = kf_after
        Return result
    End Function

    ''' <summary>
    ''' 单独渲染一帧
    ''' </summary>
    ''' <param name="context">d2dContext对象</param>
    ''' <param name="frameIndex">帧序号</param>
    Public Function RenderFrame(context As DeviceContext, frameIndex As Integer) As Bitmap1
        Dim tmpBitmap As New Bitmap1(context, BindingLive2dImage.CanvasSize, NORMAL_BITMAP_PROPERTY)
        context.Target = tmpBitmap
        context.BeginDraw()
        context.Clear(Nothing)
        For i = 0 To BindingLive2dImage.MeshAtlas.Count - 1
            Dim sprite As SpriteLive2dImage = BindingLive2dImage.MeshAtlas(i)
            Dim tweenPos As PointF2
            Dim tweenRot As Single
            'TODO: Dim tweenScale As PointF2    

            'position
            Dim positionKF As New List(Of LiveImageKeyFrame)
            For Each tmpKF As LiveImageKeyFrame In KeyFrames(i)
                If tmpKF.RegisterPosition Then
                    positionKF.Add(tmpKF)
                End If
            Next
            Dim posSection As LiveImageKeyFrame() = SearchKeyFrameSection(positionKF, frameIndex)
            'calculate tween
            If frameIndex > posSection(1).Frame Then
                tweenPos = posSection(1).PositionArgs
            Else
                Dim inputX As Single = (frameIndex - posSection(0).Frame) / (posSection(1).Frame - posSection(0).Frame)
                Dim tweenX As Single = posSection(0).PositionTween(0).GetValue(inputX)
                Dim tweenY As Single = posSection(0).PositionTween(1).GetValue(inputX)
                tweenPos.X = posSection(0).PositionArgs.X + tweenX * (posSection(1).PositionArgs.X - posSection(0).PositionArgs.X)
                tweenPos.Y = posSection(0).PositionArgs.Y + tweenY * (posSection(1).PositionArgs.Y - posSection(0).PositionArgs.Y)
            End If

            'rotation
            Dim rotationKF As New List(Of LiveImageKeyFrame)
            For Each tmpKF As LiveImageKeyFrame In KeyFrames(i)
                If tmpKF.RegisterRotation Then
                    rotationKF.Add(tmpKF)
                End If
            Next
            Dim rotSection As LiveImageKeyFrame() = SearchKeyFrameSection(rotationKF, frameIndex)
            If frameIndex > rotSection(1).Frame Then
                tweenRot = rotSection(1).RotationArgs
            Else
                Dim inputX As Single = (frameIndex - rotSection(0).Frame) / (rotSection(1).Frame - rotSection(0).Frame)
                Dim tweenR As Single = rotSection(0).RotationTween.GetValue(inputX)
                tweenRot = rotSection(0).RotationArgs + tweenR * (rotSection(1).RotationArgs - rotSection(0).RotationArgs)
            End If

            'TODO: scale, persp

            context.EndDraw()

            'Dim drawRect As New RawRectangleF(tweenPos.X, tweenPos.Y, tweenPos.X + (sprite.Vertices(2).OldPosition.X - sprite.Vertices(0).OldPosition.X), tweenPos.Y + (sprite.Vertices(2).OldPosition.Y - sprite.Vertices(0).OldPosition.Y))
            Dim eff As New Effects.AffineTransform2D(context)
            eff.SetInput(0, sprite.SourceImage, True)
            'mat1-3: move to pivot(Top-Left corner), rotate and at last, move back
            Dim mat1 As New MathMatrixS(3, 3)
            With mat1
                .SetAsIdentity()
                .Value(2, 0) = -0.5 * (BindingLive2dImage.MeshAtlas(i).Vertices(2).OldPosition.X + BindingLive2dImage.MeshAtlas(i).Vertices(0).OldPosition.X)
                .Value(2, 1) = -0.5 * (BindingLive2dImage.MeshAtlas(i).Vertices(2).OldPosition.Y + BindingLive2dImage.MeshAtlas(i).Vertices(0).OldPosition.Y)
            End With
            Dim mat2 As New MathMatrixS(3, 3)
            With mat2
                .SetAsIdentity()
                tweenRot = tweenRot * Math.PI / 180
                .Value(0, 0) = Math.Cos(tweenRot)
                .Value(1, 0) = -Math.Sin(tweenRot)
                .Value(0, 1) = Math.Sin(tweenRot)
                .Value(1, 1) = .Value(0, 0)
            End With
            Dim mat3 As New MathMatrixS(3, 3)
            With mat3
                .SetAsIdentity()
                .Value(2, 0) = -mat1.Value(2, 0)
                .Value(2, 1) = -mat1.Value(2, 1)
            End With
            'mat4: position transform
            Dim mat4 As New MathMatrixS(3, 3)
            With mat4
                .SetAsIdentity()
                .Value(2, 0) = tweenPos.X
                .Value(2, 1) = tweenPos.Y
            End With
            'TODO: scale and perspective
            Dim finalMat As MathMatrixS = mat4 * (mat3 * (mat2 * mat1))
            eff.SetValue(AffineTransform2DProperties.TransformMatrix, finalMat.ToMatrix32)

            context.BeginDraw()
            context.DrawImage(eff)

            eff.Dispose()
        Next
        context.EndDraw()
        Return tmpBitmap
    End Function

    Public Sub RenderAllToBuffer(context As DeviceContext)
        Dim sourceCanvas As Image = context.Target
        ReDim Me.AnimationBuffer(AnimationLength - 1)
        context.EndDraw()
        For i = 0 To AnimationLength - 1
            Me.AnimationBuffer(i) = RenderFrame(context, i)
        Next
        context.Target = sourceCanvas
        context.BeginDraw()
        Me.HaveRendered = True
    End Sub

    Public Sub DrawFrame(context As DeviceContext, frameIndex As Integer)
        context.DrawBitmap(Me.AnimationBuffer(frameIndex), New RawRectangleF(BindingLive2dImage.PaintPosition.X, BindingLive2dImage.PaintPosition.Y, BindingLive2dImage.PaintPosition.X + BindingLive2dImage.CanvasSize.Width, BindingLive2dImage.PaintPosition.Y + BindingLive2dImage.CanvasSize.Height), NOT_TRANSPARENT, BitmapInterpolationMode.Linear)

    End Sub


End Class

Public Class LiveImageKeyFrame
    Public Frame As Integer = 0

    Public RegisterPosition As Boolean = False
    Public PositionArgs As PointF2
    Public PositionTween(1) As BezierPenCurve

    Public RegisterRotation As Boolean = False
    Public RotationArgs As Single    '使用角度[-180,180]
    Public RotationTween As BezierPenCurve = Nothing

    Public RegisterScale As Boolean = False
    Public ScaleArgs As PointF2
    Public ScaleTween(1) As BezierPenCurve

    Public RegisterPersp As Boolean = False
    Public PerspArgs As FourPointF2
    Public PerspTween(7) As BezierPenCurve

End Class
