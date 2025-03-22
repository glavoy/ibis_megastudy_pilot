Imports System.Data.OleDb
Imports System.Windows.Forms.VisualStyles
Imports System.Configuration
Imports System.Data.Common
Imports System.Diagnostics.Eventing.Reader

Public Class SelectFormToEdit
    Private ReadOnly ConnectionString As New OleDbConnection(ConfigurationManager.ConnectionStrings("ConnString").ConnectionString)
    Dim selectedOption As String = ""

    Private Sub SelectFormToEdit_Load(sender As Object, e As EventArgs) Handles Me.Load
        ButtonGO.Enabled = False
    End Sub

    Private Sub Button_Cancel_Click(sender As Object, e As EventArgs) Handles Button_Cancel.Click
        InterviewCancelled = True
        Me.Close()
    End Sub

    Private Sub Button_Verify_Click(sender As Object, e As EventArgs) Handles Button_Verify.Click
        Try
            Dim strSQL As String = ""
            Select Case selectedOption
                Case "byScreeningId"
                    If TextBoxSubjid.Text = "" Then
                        MsgBox("You have Not Entered the ID to be searched", vbOKOnly, "Missing ID")
                    Else
                        strSQL = "select screening_id, uniqueid from " & Survey & " where screening_id = '" & TextBoxSubjid.Text & "'"
                        Dim da As New OleDbDataAdapter(strSQL, ConnectionString)
                        Dim ds As New DataSet
                        da.Fill(ds)

                        If ds.Tables(0).Rows.Count = 0 Then
                            MsgBox("There is no Screening CRF for ID " & TextBoxSubjid.Text, vbOKOnly, "Missing ID")
                            Exit Sub
                        Else
                            ButtonGO.Enabled = True
                            UNIQUEID = ds.Tables(0).Rows(0)("uniqueid").ToString()
                        End If
                    End If

                Case "byStudyId"
                    If TextBoxSubjid.Text = "" Then
                        MsgBox("You have Not Entered the ID to be searched", vbOKOnly, "Missing ID")
                    Else
                        strSQL = "select subjid, uniqueid from " & Survey & " where subjid = '" & TextBoxSubjid.Text & "'"
                        Dim da As New OleDbDataAdapter(strSQL, ConnectionString)
                        Dim ds As New DataSet
                        da.Fill(ds)
                        Dim dt As DataTable = ds.Tables(0)
                        If ds.Tables(0).Rows.Count = 0 Then
                            MsgBox("There is no Baseline CRF for ID " & TextBoxSubjid.Text, vbOKOnly, "Missing ID")
                            Exit Sub
                        Else
                            ButtonGO.Enabled = True
                            UNIQUEID = ds.Tables(0).Rows(0)("uniqueid").ToString()
                        End If
                    End If

                Case Else
                    MsgBox("You have Not selected the Search by Category ", vbOKOnly, "Missing Search Category")
            End Select
            ConnectionString.Close()

        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub

    Private Sub RadioButton_ScrnID_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton_ScrnID.CheckedChanged
        selectedOption = "byScreeningId"
    End Sub

    Private Sub RadioButton_studyid_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton_studyid.CheckedChanged
        selectedOption = "byStudyId"
    End Sub

    Private Sub ButtonGO_Click(sender As Object, e As EventArgs) Handles ButtonGO.Click
        Try
            SUBJID = UCase(TextBoxSubjid.Text)
            ModifyingSurvey = True
            Me.Dispose()
            NewSurvey.ShowDialog()
            NewSurvey.Dispose()
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub


End Class