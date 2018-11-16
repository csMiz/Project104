' -----------------------------------------
' Copyright (c) 2018 All Rights Reserved.
' 
' Filename: GameSettingRepository
' Author: Miz
' Date: 2018/11/12 12:54:44
' -----------------------------------------

Imports System.IO
''' <summary>
''' 用户存档设置信息类
''' </summary>
Public Class GameSettingRepository
    ''' <summary>
    ''' 档案用户名
    ''' </summary>
    Public Username As String = vbNullString
    ''' <summary>
    ''' 用户权限
    ''' </summary>
    Public UserDomain As UserDomainType = UserDomainType.Master
    ''' <summary>
    ''' 用户语言偏好
    ''' </summary>
    Public UserLanguageIndex As Integer = 0
    ''' <summary>
    ''' 窗口模式
    ''' </summary>
    Public GameWindowType As WindowType = WindowType.Window
    ''' <summary>
    ''' 分辨率
    ''' </summary>
    Public UserResolve As PointI = Nothing



    ''' <summary>
    ''' 对应用户成就记录
    ''' </summary>
    Public GameUserRecord As GameAchievementRecord = Nothing

    Public Sub InitializeEmptySettings()
        With Me
            .Username = "Player"
            .UserDomain = UserDomainType.Retail
            .UserLanguageIndex = GameFontHelper.findLanguageIndex("ENGL")
            .GameWindowType = WindowType.Window
            .UserResolve = New PointI(1024, 768)

            .GameUserRecord = New GameAchievementRecord()
            .GameUserRecord.InitializeEmptyRecord()
        End With
    End Sub


    ''' <summary>
    ''' 读取用户设置
    ''' </summary>
    Public Sub LoadSettingsFromBinaryFile(inputStream As Stream)
        'TODO
        If inputStream Is Nothing Then
            InitializeEmptySettings()
            Dim tmpStream As New FileStream(Application.StartupPath & "\Save\s00.psav", FileMode.Create)
            SaveSettingsIntoBinaryFile(tmpStream)
            tmpStream.Close()
            tmpStream.Dispose()
        Else
            Throw New NotImplementedException
        End If
    End Sub

    ''' <summary>
    ''' 保存用户设置
    ''' </summary>
    Public Sub SaveSettingsIntoBinaryFile(saveStream As Stream)
        'TODO
        '格式见SaveFileFormat_psav.md
        Dim headBuffer(31) As Byte
        headBuffer(0) = &H50
        headBuffer(1) = &H53
        headBuffer(2) = &H41
        headBuffer(3) = &H56

        headBuffer(4) = &H30
        headBuffer(5) = &H31
        headBuffer(6) = &H30
        headBuffer(7) = &H34

        Dim randomNumber As Integer = CInt(MathHelper.GetRandom() * &HFFFFFFFF)
        Dim randomCode() As Byte = BitConverter.GetBytes(randomNumber)
        For i = 8 To 11
            headBuffer(i) = randomCode(i - 8)
        Next

        For i = 12 To 31
            headBuffer(i) = &H0
        Next

        '---------------

        Dim contentBuffer(32) As Byte
        For i = 0 To 3
            contentBuffer(i) = &H0
        Next
        Dim usernameUnicode() As Char = Me.Username.ToCharArray
        Dim usernameLength As Integer = usernameUnicode.Length
        If usernameLength > 16 Then usernameLength = 16
        For i = 4 To 19
            contentBuffer(i) = &H0
        Next
        For i = 4 To 4 + usernameLength - 1
            contentBuffer(i) = AscW(usernameUnicode(i - 4))
        Next

        Dim domainCode() As Byte = GetUserDomainCode(Me.UserDomain)
        For i = 20 To 23
            contentBuffer(i) = domainCode(i - 20)
        Next

        Dim usingLanguageCode() As Byte = GameFontHelper.GetLanguageCode(Me.UserLanguageIndex)
        For i = 24 To 27
            contentBuffer(i) = usingLanguageCode(i - 24)
        Next

        contentBuffer(28) = Me.GameWindowType

        Dim resolveX() As Byte = BitConverter.GetBytes(UserResolve.X)
        Dim resolveY() As Byte = BitConverter.GetBytes(UserResolve.Y)
        contentBuffer(29) = resolveX(0)
        contentBuffer(30) = resolveX(1)
        contentBuffer(31) = resolveY(0)
        contentBuffer(32) = resolveY(1)

        '--------------

        Dim systemBuffer() As Byte
        For i = 0 To 3
            systemBuffer(i) = &H0
        Next

        Dim achievementSource() As Byte = GameUserRecord.GetAchievementByte
        For i = 4 To 11
            systemBuffer(i) = achievementSource(i - 4)
        Next

        For i = 12 To 23    '校验码
            systemBuffer(i) = &H0
        Next

        Dim totalTimeSource() As Byte = GameUserRecord.GetTotalTimeByte
        For i = 24 To 27
            systemBuffer(i) = totalTimeSource(i - 24)
        Next

        For i = 28 To 31    '校验码
            systemBuffer(i) = &H0
        Next

        Using bw As BinaryWriter = New BinaryWriter(saveStream)
            bw.Seek(0, SeekOrigin.Begin)
            bw.Write(headBuffer)
            bw.Write(contentBuffer)
            bw.Write(systemBuffer)
            'TODO
        End Using

    End Sub

    Public Shared Function GetUserDomainCode(value As UserDomainType) As Byte()
        Dim result(3) As Byte
        If value = UserDomainType.Retail Then
            result(0) = &H52    'RETA
            result(1) = &H45
            result(2) = &H54
            result(3) = &H41
        ElseIf value = UserDomainType.PublicTest Then
            result(0) = &H50    'PTET
            result(1) = &H54
            result(2) = &H45
            result(3) = &H54
        ElseIf value = UserDomainType.Develop Then
            result(0) = &H44    'DEVP
            result(1) = &H45
            result(2) = &H56
            result(3) = &H50
        ElseIf value = UserDomainType.Master Then
            result(0) = &H4D    'MAST
            result(1) = &H41
            result(2) = &H53
            result(3) = &H54
        Else value = UserDomainType.None
            result(0) = &H4E    'NONE
            result(1) = &H4F
            result(2) = &H4E
            result(3) = &H45
        End If
        Return result
    End Function

End Class

Public Enum UserDomainType As Byte
    None = 0
    PublicTest = 1
    Retail = 2
    Develop = 3
    Master = 4

End Enum

Public Enum WindowType As Byte
    Window = 0
    FullScreen = 1
End Enum

