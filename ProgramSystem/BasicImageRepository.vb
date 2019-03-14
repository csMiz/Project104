Imports System.Text.RegularExpressions
Imports p104
Imports SharpDX.Direct2D1
Imports SharpDX.Mathematics.Interop

''' <summary>
''' 图片素材碎片仓库
''' </summary>
Public Class BasicImageRepository
    Private Shared me_instance As BasicImageRepository = Nothing

    Private LeftPanelFragmentImages As New List(Of BasicImagePair)
    Private SkirmishUnitFragmentImages As New List(Of BasicImagePair)
    Private ControlImages As New List(Of BasicImagePair)

    Private Sub New()
    End Sub

    Public Shared Function Instance() As BasicImageRepository
        If me_instance Is Nothing Then me_instance = New BasicImageRepository
        Return me_instance
    End Function

    Public Sub LoadImages(context As DeviceContext)
        Dim loadPath As String = Application.StartupPath & "\Resources\Images\"
        Me.LoadToDirectory(context, loadPath & "UnitDetailLP\", LeftPanelFragmentImages)
        Me.LoadToDirectory(context, loadPath & "SkirmishUnit\", SkirmishUnitFragmentImages)
        Me.LoadToDirectory(context, loadPath & "Control\", ControlImages)

    End Sub

    Private Sub LoadToDirectory(context As DeviceContext, folderPath As String, group As List(Of BasicImagePair))
        Dim dirInfo As New System.IO.DirectoryInfo(folderPath)
        Dim allFiles() As System.IO.FileInfo = dirInfo.GetFiles
        For Each file As System.IO.FileInfo In allFiles
            Dim readStream As System.IO.FileStream = file.OpenRead
            Dim tmpImage As SharpDX.Direct2D1.Bitmap1 = GameResources.LoadBitmapUsingWIC(context, readStream)

            Dim filename As String = file.Name
            Dim keys() As String = Regex.Split(filename.Remove(filename.Length - 4), UNDERLINE)
            Dim tmpPair As New BasicImagePair With {
                .Key = keys,
                .Image = tmpImage}

            group.Add(tmpPair)
        Next
    End Sub

    Public Function GetFragment(index As Integer, domain As String) As Bitmap1
        If domain = SKIRMISH_FRAGMENT_DOMAIN Then
            Return Me.SkirmishUnitFragmentImages(index).Image

            'TODO
        ElseIf domain = CONTROL_IMAGE_DOMAIN Then
            For Each tmpImg As BasicImagePair In Me.ControlImages
                If index = CInt(tmpImg.Key.Last) Then
                    Return tmpImg.Image
                End If
            Next

        End If
        Return Nothing
    End Function

End Class

Public Class BasicImagePair
    Public Key As String()
    Public Image As Bitmap1

End Class


