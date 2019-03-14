' -----------------------------------------
' Copyright (c) 2018 All Rights Reserved.
' 
' Filename: GameButton
' Author: Miz
' Date: 2018/10/29 11:40:59
' -----------------------------------------

Imports p104
Imports SharpDX.Direct2D1
Imports SharpDX.DirectWrite
''' <summary>
''' Button基类
''' </summary>
Public MustInherit Class GameButton
    Inherits GameBasicUIElement
    ''' <summary>
    ''' 按钮是否有效，相当于Enable
    ''' </summary>
    Protected CanBePressed As Boolean = True
    ''' <summary>
    ''' 按钮上的文字
    ''' </summary>
    Public ButtonText As New TextItem2



End Class
