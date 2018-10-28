''' <summary>
''' 状态机类
''' </summary>
Public Class StateMachine
    Protected States As List(Of StateMachineSingleState)
    ''' <summary>
    ''' (nowState,input)
    ''' </summary>
    Protected TransitionFunction(,) As Short

    Public Overridable Sub InitializeStateMachine(inputStates As List(Of StateMachineSingleState), transition As Short(,))
        Me.States = inputStates
        Me.TransitionFunction = transition
    End Sub

    Public Overridable Function NextState(nowState As Short, input As StateMachineInputAlphabet) As StateMachineSingleState
        Dim nextStateIndex As Short = Me.TransitionFunction(nowState, input)
        Return Me.States(nextStateIndex)
    End Function

End Class

''' <summary>
''' 状态机单个状态类
''' </summary>
Public MustInherit Class StateMachineSingleState
    Protected StateIndex As Short = -1

    Public Overridable Sub StateStart(sender As Object, ByRef status As StateMachineSingleProcessStatus)
    End Sub
    Public Overridable Sub StateProcess(sender As Object, ByRef status As StateMachineSingleProcessStatus)
    End Sub
    Public Overridable Sub StateEnd(sender As Object, ByRef status As StateMachineSingleProcessStatus)
    End Sub

    Public Sub InitializeStateIndex(index As Short)
        If Me.StateIndex <> -1 Then Throw New Exception("state index has been changed!")
        Me.StateIndex = index
    End Sub

    Public Function GetStateIndex() As Short
        Return StateIndex
    End Function

End Class

''' <summary>
''' 状态机改变输入参数枚举
''' </summary>
Public Enum StateMachineInputAlphabet As Byte
    StateGo = 0
    StateSkip = 1

End Enum

''' <summary>
''' 状态机单个状态进度枚举
''' </summary>
Public Enum StateMachineSingleProcessStatus As Byte
    NA = 0
    Starting = 1
    Normal = 2
    Ending = 3

    Abort = 4
    Suspended = 5


End Enum


