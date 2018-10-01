''' <summary>
''' 遭遇战GameLoop
''' </summary>
Public Class SkirmishGameLoop

End Class

''' <summary>
''' 回合阶段枚举，参考“游戏王大师规则”
''' </summary>
Public Enum SingleGameLoopStage As Byte
    ''' <summary>
    ''' 回合开始阶段
    ''' </summary>
    MyTurnStart = 0
    ''' <summary>
    ''' 准备阶段
    ''' </summary>
    Prepare = 1
    ''' <summary>
    ''' 主要阶段1
    ''' </summary>
    MainA = 2
    ''' <summary>
    ''' 战斗阶段
    ''' </summary>
    Battle = 3
    ''' <summary>
    ''' 主要阶段2
    ''' </summary>
    MainB = 4
    ''' <summary>
    ''' 回合结束阶段
    ''' </summary>
    MyTurnEnd = 5
End Enum