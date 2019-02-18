' -----------------------------------------
' Copyright (c) 2019 All Rights Reserved.
' 
' Filename: GeometryArithmetic
' Author: Miz
' Date: 2019/2/18 13:58:23
' -----------------------------------------

''' <summary>
''' 几何运算模块
''' </summary>
Public Module GeometryArithmetic

    ''' <summary>
    ''' 并集运算
    ''' </summary>
    Public Function GA_Union(a As GA_Polygon, b As GA_Polygon) As GA_Polygon
        'TODO

    End Function

End Module

''' <summary>
''' 用于运算的几何图形结构体
''' </summary>
Public Structure GA_Polygon

    Public Vertices As PointF2()

End Structure
