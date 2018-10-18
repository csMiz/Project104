Imports System.Text.RegularExpressions
Imports p104
Imports SharpDX.Direct2D1
Imports SharpDX.Mathematics.Interop

''' <summary>
''' 图片素材碎片仓库
''' </summary>
Public Class BasicImageRepository
    Private Shared me_instance As BasicImageRepository = Nothing

    Private FragmentImages As New List(Of BasicImagePair)

    Private Sub New()
    End Sub

    Public Shared Function Instance() As BasicImageRepository
        If me_instance Is Nothing Then me_instance = New BasicImageRepository
        Return me_instance
    End Function

    Public Sub LoadImages(context As DeviceContext)
        Dim loadPath As String = Application.StartupPath & "\Resources\Images\UnitDetailLP\"

        Dim dirInfo As New System.IO.DirectoryInfo(loadPath)
        Dim allFiles() As System.IO.FileInfo = dirInfo.GetFiles
        For Each file As System.IO.FileInfo In allFiles
            Dim readStream As System.IO.FileStream = file.OpenRead
            Dim tmpImage As SharpDX.Direct2D1.Bitmap1 = GameResources.LoadBitmapUsingWIC(context, readStream)

            Dim filename As String = file.Name
            Dim keys() As String = Regex.Split(filename, UNDERLINE)
            Dim tmpPair As New BasicImagePair With {
                .Key = New List(Of String)(keys),
                .Image = tmpImage}

            Me.FragmentImages.Add(tmpPair)
        Next
    End Sub

End Class

Public Class BasicImagePair
    Public Key As List(Of String)
    Public Image As Bitmap1

End Class


