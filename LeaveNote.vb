Imports System.Configuration
Imports System.Data.OleDb
Imports System.Globalization

Public Class LeaveNote
    Private Sub ButtonLeaveNote_Click(sender As Object, e As EventArgs) Handles ButtonLeaveNote.Click
        Try
            If Microsoft.VisualBasic.Len(TextBoxNote.Text) > 5 Then
                Dim strSQL As String = ""
                Dim SUBJID_Date_US As String = ""

                Dim config As Configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None)
                Dim section As ConnectionStringsSection = DirectCast(config.GetSection("connectionStrings"), ConnectionStringsSection)
                Dim ConnectionString As New OleDbConnection(section.ConnectionStrings("ConnString").ConnectionString)
                ConnectionString.Open()

                ' Get the primary key for the table
                Dim PKstrSQL As String = "select primarykey from crfs where tablename = '" & Survey & "'"
                Dim daPK As New OleDbDataAdapter(PKstrSQL, ConnectionString)
                Dim dsPK As New DataSet
                daPK.Fill(dsPK)
                Dim PrimaryKey As String = ""
                For Each row As DataRow In dsPK.Tables(0).Rows
                    PrimaryKey = row.Item("primarykey")
                Next

                Dim VDATE_US As String = ""
                'need to use this because queries in MS access can only use dates in m/d/yy format
                If InStr(PrimaryKey, "vdate") > 0 Then
                    If Len(VDATE) > 10 Then
                        VDATE_US = DateTime.ParseExact(VDATE, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture).ToString("MM/dd/yyyy HH:mm:ss")
                    Else
                        VDATE_US = DateTime.ParseExact(VDATE, "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("MM/dd/yyyy HH:mm:ss")
                    End If
                    strSQL = "insert into formchanges(tablename, subjid, formdate, changedescription) Values('" & Survey & "', '" & SUBJID & "', #" & VDATE_US & "#, '" & TextBoxNote.Text.Replace(vbCr, "").Replace(vbLf, "") & "')"
                Else
                    strSQL = "insert into formchanges(tablename, subjid, changedescription) Values('" & Survey & "', '" & SUBJID & "', '" & TextBoxNote.Text.Replace(vbCr, "").Replace(vbLf, "") & "')"
                End If

                Dim cmd = New OleDbCommand(strSQL, ConnectionString)
                cmd.ExecuteNonQuery()
                cmd.Dispose()
                ConnectionString.Close()

                'Write to Text file
                Dim FILE_NAME As String = ConfigurationManager.AppSettings("BackupTextFile") & "\formchanges_" & Survey
                Dim objWriter As New System.IO.StreamWriter(FILE_NAME, True)
                objWriter.WriteLine(strSQL)
                objWriter.Close()
                Me.Close()
            Else
                MsgBox("Please add more detail about your changes.")
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub

    Private Sub ButtonCancel_Click(sender As Object, e As EventArgs) Handles ButtonCancel.Click
        CancelledLeaveNote = True
        Me.Close()
    End Sub

    Private Sub LeaveNote_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        CancelledLeaveNote = False
    End Sub
End Class