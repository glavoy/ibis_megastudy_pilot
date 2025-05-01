Imports System.Data
Imports System.Data.OleDb
Imports System.Globalization
Imports System.Configuration
Imports System.IO
Imports System.Text

Public Class SMSSchedule
    ' Use connection string from configuration file
    Private dataAdapter As OleDbDataAdapter
    Private dataTable As DataTable

    Public Sub New()
        InitializeComponent()
        ' Initialize components manually
        SetupComponents()

        ' Set form properties
        Me.FormBorderStyle = FormBorderStyle.None
        Me.WindowState = FormWindowState.Maximized

        ' Initialize date filters with default values (current month)
        dtpStartDate.Value = New DateTime(DateTime.Now.Year, DateTime.Now.Month, 1)
        dtpEndDate.Value = DateTime.Now.AddDays(28)

        ' Load data on form load
        LoadData()
    End Sub

    Private Sub SetupComponents()
        ' Initialize components
        Me.dgvBaselineData = New DataGridView()
        Me.btnClose = New Button()
        Me.lblStartDate = New Label()
        Me.lblEndDate = New Label()
        Me.dtpStartDate = New DateTimePicker()
        Me.dtpEndDate = New DateTimePicker()
        Me.btnApplyFilter = New Button()
        Me.pnlTop = New Panel()
        Me.pnlGrid = New Panel()

        ' Panel for top controls
        Me.pnlTop.SuspendLayout()
        Me.pnlTop.Dock = DockStyle.Top
        Me.pnlTop.Height = 80
        Me.pnlTop.BackColor = Color.LightGray

        ' Close button
        Me.btnClose.Text = "Close"
        Me.btnClose.Location = New Point(10, 20)
        Me.btnClose.Size = New Size(100, 30)
        Me.btnClose.BackColor = Color.Firebrick
        Me.btnClose.ForeColor = Color.White
        Me.btnClose.Font = New Font("Arial", 9, FontStyle.Bold)
        AddHandler btnClose.Click, AddressOf btnClose_Click

        ' Start date label and picker
        Me.lblStartDate.Text = "Start Date:"
        Me.lblStartDate.Location = New Point(150, 10)
        Me.lblStartDate.AutoSize = True

        Me.dtpStartDate.Location = New Point(150, 30)
        Me.dtpStartDate.Size = New Size(150, 25)
        Me.dtpStartDate.Format = DateTimePickerFormat.Short

        ' End date label and picker
        Me.lblEndDate.Text = "End Date:"
        Me.lblEndDate.Location = New Point(320, 10)
        Me.lblEndDate.AutoSize = True

        Me.dtpEndDate.Location = New Point(320, 30)
        Me.dtpEndDate.Size = New Size(150, 25)
        Me.dtpEndDate.Format = DateTimePickerFormat.Short

        ' Apply filter button
        Me.btnApplyFilter.Text = "Apply Filter"
        Me.btnApplyFilter.Location = New Point(490, 30)
        Me.btnApplyFilter.Size = New Size(100, 25)
        AddHandler btnApplyFilter.Click, AddressOf btnApplyFilter_Click

        ' Reset filter button
        Me.btnResetFilter = New Button()
        Me.btnResetFilter.Text = "Reset Filter"
        Me.btnResetFilter.Location = New Point(600, 30)
        Me.btnResetFilter.Size = New Size(100, 25)
        AddHandler btnResetFilter.Click, AddressOf btnResetFilter_Click

        ' Export to CSV button
        Me.btnExportCSV = New Button()
        Me.btnExportCSV.Text = "Export to CSV"
        Me.btnExportCSV.Location = New Point(710, 30)
        Me.btnExportCSV.Size = New Size(120, 25)
        AddHandler btnExportCSV.Click, AddressOf btnExportCSV_Click

        ' Add controls to top panel
        Me.pnlTop.Controls.Add(btnClose)
        Me.pnlTop.Controls.Add(lblStartDate)
        Me.pnlTop.Controls.Add(dtpStartDate)
        Me.pnlTop.Controls.Add(lblEndDate)
        Me.pnlTop.Controls.Add(dtpEndDate)
        Me.pnlTop.Controls.Add(btnApplyFilter)
        Me.pnlTop.Controls.Add(btnResetFilter)
        Me.pnlTop.Controls.Add(btnExportCSV)

        ' Panel for GridView
        Me.pnlGrid.Dock = DockStyle.Fill
        Me.pnlGrid.BackColor = Color.White

        ' GridView settings
        Me.dgvBaselineData.Dock = DockStyle.Fill
        Me.dgvBaselineData.AllowUserToAddRows = False
        Me.dgvBaselineData.AllowUserToDeleteRows = False
        Me.dgvBaselineData.ReadOnly = True
        Me.dgvBaselineData.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        Me.dgvBaselineData.RowHeadersVisible = False
        Me.dgvBaselineData.BackgroundColor = Color.White
        Me.dgvBaselineData.BorderStyle = BorderStyle.None
        Me.dgvBaselineData.SelectionMode = DataGridViewSelectionMode.FullRowSelect

        ' Set alternating row colors using the DefaultCellStyle.BackColor for odd rows
        Me.dgvBaselineData.AlternatingRowsDefaultCellStyle.BackColor = Color.AliceBlue

        ' Add GridView to grid panel
        Me.pnlGrid.Controls.Add(dgvBaselineData)

        ' Set up main form
        Me.Controls.Add(pnlGrid)
        Me.Controls.Add(pnlTop)
        Me.Text = "Baseline Data View"
        Me.pnlTop.ResumeLayout(False)
        Me.pnlTop.PerformLayout()
    End Sub

    Private Function getSMS(arm_code As Integer, visitweek As Integer, pref_lang As Integer, Optional appt_date As String = "19/04/2025") As String
        Dim sms As String = ""
        Try
            Using connection As New OleDbConnection(ConfigurationManager.ConnectionStrings("ConnString").ConnectionString)
                Dim query As String

                query = "SELECT sms FROM sms_services WHERE arm_code = ? AND visitweek = ? AND preferred_language = ?"

                Using command As New OleDbCommand(query, connection)
                    ' Add parameters in the exact order they appear in the query
                    command.Parameters.AddWithValue("?", arm_code)
                    command.Parameters.AddWithValue("?", visitweek)
                    command.Parameters.AddWithValue("?", pref_lang)

                    connection.Open()
                    Using reader As OleDbDataReader = command.ExecuteReader()
                        If reader.Read() AndAlso Not IsDBNull(reader("sms")) Then
                            sms = reader("sms").ToString()
                        End If
                    End Using
                End Using
            End Using

            'add appointment date to the default appointment date arm
            If arm_code = 8 Then
                sms = Replace(sms, "[date]", appt_date)
            End If
            Return sms
        Catch ex As Exception
            MsgBox("Error: " & ex.Message)
            Return sms
        End Try


    End Function
    Private Sub LoadData()
        Try
            ' Create connection using the connection string from configuration
            Using connection As New OleDbConnection(ConfigurationManager.ConnectionStrings("ConnString").ConnectionString)
                ' Format dates for query
                Dim startDateStr As String = dtpStartDate.Value.ToString("dd/MM/yyyy")
                Dim endDateStr As String = dtpEndDate.Value.ToString("dd/MM/yyyy")

                ' SQL query
                Dim query As String = "SELECT subjid, mobile_number,arm,arm_text, preferred_language,preferred_language_text, starttime,dflt_appt_arm_schd_appt_date " &
                                    "FROM baseline " &
                                    "WHERE consent = 1 AND starttime BETWEEN #" & startDateStr & "# AND #" & endDateStr & "#"

                ' Create data adapter and table
                dataAdapter = New OleDbDataAdapter(query, connection)
                dataTable = New DataTable()

                ' Fill data table
                dataAdapter.Fill(dataTable)

                ' Add calculated columns
                dataTable.Columns.Add("eight_week_followup", GetType(DateTime))
                dataTable.Columns.Add("eleven_week_followup", GetType(DateTime))
                dataTable.Columns.Add("sms_message_wk8", GetType(String))
                dataTable.Columns.Add("sms_message_wk11", GetType(String))

                ' Calculate follow-up dates and add SMS message
                For Each row As DataRow In dataTable.Rows
                    If Not IsDBNull(row("starttime")) Then
                        Dim baselineDate As DateTime = CDate(row("starttime"))
                        row("eight_week_followup") = baselineDate.AddDays(8 * 7) ' 8 weeks
                        row("eleven_week_followup") = baselineDate.AddDays(11 * 7) ' 11 weeks
                    End If

                    ' Add specific SMS message based on arm_text
                    Dim armText As String = If(IsDBNull(row("arm_text")), "", row("arm_text").ToString())
                    If CInt(row("arm")) = 8 Then
                        row("sms_message_wk8") = getSMS(CInt(row("arm")), 8, CInt(row("preferred_language")), row("dflt_appt_arm_schd_appt_date"))
                        row("sms_message_wk11") = getSMS(CInt(row("arm")), 11, CInt(row("preferred_language")), row("dflt_appt_arm_schd_appt_date"))
                    Else
                        row("sms_message_wk8") = getSMS(CInt(row("arm")), 8, CInt(row("preferred_language")))
                        row("sms_message_wk11") = getSMS(CInt(row("arm")), 11, CInt(row("preferred_language")))
                    End If

                    'Select Case armText.Trim()
                    '    Case "Fresh start effect"
                    '        row("sms_message") = "It's a perfect time for a fresh start with your health journey. Your appointment is important."
                    '    Case "U=U messaging"
                    '        row("sms_message") = "Remember: Undetectable = Untransmittable. Your regular check-up helps maintain your health."
                    '    Case "Reserved for you"
                    '        row("sms_message") = "You have an appointment reserved especially for you. We're looking forward to seeing you."
                    '    Case "Community benefits"
                    '        row("sms_message") = "Your visit helps our community stay healthy. Thank you for being part of the solution."
                    '    Case "Education-based #2(gamification)"
                    '        row("sms_message") = "You're making progress on your health journey! Keep the momentum going with your upcoming appointment."
                    '    Case "Risk assessment"
                    '        row("sms_message") = "Regular check-ups are key to staying healthy. Your appointment helps manage any potential risks."
                    '    Case "Default appointment"
                    '        row("sms_message") = "This is a reminder about your upcoming appointment. We look forward to seeing you."
                    '    Case "Healthy living"
                    '        row("sms_message") = "Your commitment to healthy living includes regular check-ups. Your appointment is coming up."
                    '    Case "Social norms"
                    '        row("sms_message") = "Most people in your community attend their scheduled appointments. Join them at your upcoming visit."
                    '    Case "Empowerment"
                    '        row("sms_message") = "You have the power to manage your health. Your upcoming appointment is an important step."
                    '    Case "Education-based #1"
                    '        row("sms_message") = "Did you know regular visits improve long-term health outcomes? Your appointment is coming up."
                    '    Case Else
                    '        row("sms_message") = "Thank you for participating in our study. Your upcoming appointment is scheduled soon."
                    'End Select
                Next

                ' Set data source for grid view
                dgvBaselineData.DataSource = dataTable

                ' Customize column headers
                dgvBaselineData.Columns("subjid").HeaderText = "Study ID"
                dgvBaselineData.Columns("mobile_number").HeaderText = "Phone Number"
                dgvBaselineData.Columns("arm_text").HeaderText = "Study Arm"
                dgvBaselineData.Columns("preferred_language_text").HeaderText = "Preferred Language"
                dgvBaselineData.Columns("starttime").HeaderText = "BL Date"
                dgvBaselineData.Columns("eight_week_followup").HeaderText = "8-week Follow-up Date"
                dgvBaselineData.Columns("eleven_week_followup").HeaderText = "11-week Follow-up Date"
                dgvBaselineData.Columns("sms_message_wk8").HeaderText = "Wk 8 SMS"
                dgvBaselineData.Columns("sms_message_wk11").HeaderText = "Wk 11 SMS"

                ' Format date columns
                dgvBaselineData.Columns("starttime").DefaultCellStyle.Format = "dd/MM/yyyy"
                dgvBaselineData.Columns("eight_week_followup").DefaultCellStyle.Format = "dd/MM/yyyy"
                dgvBaselineData.Columns("eleven_week_followup").DefaultCellStyle.Format = "dd/MM/yyyy"
            End Using

        Catch ex As Exception
            MessageBox.Show("Error loading data: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnApplyFilter_Click(sender As Object, e As EventArgs)
        ' Reload data with new date filters
        LoadData()
    End Sub

    Private Sub btnResetFilter_Click(sender As Object, e As EventArgs)
        ' Reset date filters to default values
        dtpStartDate.Value = New DateTime(DateTime.Now.Year, DateTime.Now.Month, 1)
        dtpEndDate.Value = DateTime.Now

        ' Reload data with default date filters
        LoadData()
    End Sub

    Private Sub btnExportCSV_Click(sender As Object, e As EventArgs)
        Try
            ' Check if there's data to export
            If dgvBaselineData.Rows.Count = 0 Then
                MessageBox.Show("No data to export.", "Export to CSV", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            ' Create SaveFileDialog to allow user to choose where to save the file
            Dim saveFileDialog As New SaveFileDialog()
            saveFileDialog.Filter = "CSV files (*.csv)|*.csv"
            saveFileDialog.Title = "Export to CSV"
            saveFileDialog.FileName = "SMS_Schedule_Export_" & DateTime.Now.ToString("yyyy-MM-dd") & ".csv"

            If saveFileDialog.ShowDialog() = DialogResult.OK Then
                ' Create the CSV file
                Using writer As New StreamWriter(saveFileDialog.FileName, False, Encoding.UTF8)
                    ' Write header row
                    Dim headerLine As New StringBuilder()
                    For i As Integer = 0 To dgvBaselineData.Columns.Count - 1
                        If i > 0 Then headerLine.Append(",")
                        headerLine.Append("""" & dgvBaselineData.Columns(i).HeaderText & """")
                    Next
                    writer.WriteLine(headerLine.ToString())

                    ' Write data rows
                    For Each row As DataGridViewRow In dgvBaselineData.Rows
                        If Not row.IsNewRow Then
                            Dim dataLine As New StringBuilder()
                            For i As Integer = 0 To dgvBaselineData.Columns.Count - 1
                                If i > 0 Then dataLine.Append(",")

                                ' Check if cell value is null or empty
                                If row.Cells(i).Value IsNot Nothing Then
                                    ' Format date cells properly
                                    If TypeOf row.Cells(i).Value Is DateTime Then
                                        dataLine.Append("""" & DirectCast(row.Cells(i).Value, DateTime).ToString("dd/MM/yyyy") & """")
                                    Else
                                        ' Escape double quotes in text fields by doubling them
                                        Dim cellValue As String = row.Cells(i).Value.ToString().Replace("""", """""")
                                        dataLine.Append("""" & cellValue & """")
                                    End If
                                Else
                                    dataLine.Append("""""")
                                End If
                            Next
                            writer.WriteLine(dataLine.ToString())
                        End If
                    Next
                End Using

                MessageBox.Show("Data exported successfully to: " & saveFileDialog.FileName,
                                "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
        Catch ex As Exception
            MessageBox.Show("Error exporting data: " & ex.Message, "Export Error",
                           MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs)
        ' Close the form
        Me.Close()
    End Sub

    ' Components
    Private WithEvents dgvBaselineData As DataGridView
    Private WithEvents btnClose As Button
    Private WithEvents lblStartDate As Label
    Private WithEvents lblEndDate As Label
    Private WithEvents dtpStartDate As DateTimePicker
    Private WithEvents dtpEndDate As DateTimePicker
    Private WithEvents btnApplyFilter As Button
    Private WithEvents btnResetFilter As Button
    Private WithEvents btnExportCSV As Button
    Private WithEvents pnlTop As Panel
    Private WithEvents pnlGrid As Panel
End Class

' Form to launch the application
' The main application entry point
Public Module MainModule
    <STAThread()>
    Public Sub Main()
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)
        Application.Run(New SMSSchedule())
    End Sub
End Module