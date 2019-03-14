Imports System.Text.RegularExpressions
Imports SharpDX.Direct2D1

''' <summary>
''' Icon仓库类
''' </summary>
Public Class GameIconRepository
    Private Shared me_instance As GameIconRepository = Nothing

    Public Icons As New List(Of BasicImagePair)

    Private Sub New()
    End Sub

    ''' <summary>
    ''' 单例
    ''' </summary>
    Public Shared Function Instance() As GameIconRepository
        If me_instance Is Nothing Then me_instance = New GameIconRepository
        Return me_instance
    End Function

    ''' <summary>
    ''' 从程序根目录下加载资源
    ''' </summary>
    Public Sub LoadFromFiles(context As SharpDX.Direct2D1.DeviceContext)
        Me.LoadToDirectory(context, Application.StartupPath & "\Resources\Images\Icon\", Icons)

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

    Public Function GetIcon(index As Integer) As Bitmap1
        For Each tmpImg As BasicImagePair In Me.Icons
            If index = CInt(tmpImg.Key.Last) Then
                Return tmpImg.Image
            End If
        Next
        Return Nothing
    End Function


End Class
