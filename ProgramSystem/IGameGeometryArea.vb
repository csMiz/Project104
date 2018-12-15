' -----------------------------------------
' Copyright (c) 2018 All Rights Reserved.
' 
' Filename: IGameGeometryArea
' Author: Miz
' Date: 2018/12/15 14:15:34
' -----------------------------------------

Public Interface IGameGeometryArea
    Function IsInside(input As PointF2) As Boolean
    Function IsInsideRaw(input As PointF2) As Boolean
End Interface
