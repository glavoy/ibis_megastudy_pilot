Imports System.Data
Imports System.Data.OleDb
Imports System.Globalization
Imports System.Configuration
Imports System.IO
Imports System.Text
Public Class PendingFollowup
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
        Me.Text = "Pending followup Data View"
        Me.pnlTop.ResumeLayout(False)
        Me.pnlTop.PerformLayout()
    End Sub

    Private Sub LoadData()
        Try
            ' Create connection using the connection string from configuration
            Using connection As New OleDbConnection(ConfigurationManager.ConnectionStrings("ConnString").ConnectionString)
                ' Format dates for query
                Dim startDateStr As String = dtpStartDate.Value.ToString("dd/MM/yyyy")
                Dim endDateStr As String = dtpEndDate.Value.ToString("dd/MM/yyyy")

                Dim query As String = "SELECT subjid, participants_name, nickname, client_sex, mobile_number, health_facility, next_appt_6m " &
                                  "FROM baseline " &
                                  "WHERE subjid <> '-9'  AND next_appt_6m < now() AND " &
                                  "((subjid NOT IN (select subjid from followup where primary_endpoint_visit = 1 )) OR (subjid NOT IN (select subjid from followup_lookup where primary_endpoint_visit = 1)))  AND " &
                                  "(next_appt_6m  IS NOT NULL AND next_appt_6m  BETWEEN #" & startDateStr & "# AND #" & endDateStr & "#) " &
                                  "UNION " &
                                  "SELECT subjid, participants_name, nickname, client_sex, mobile_number, health_facility, next_appt_6m " &
                                  "FROM baseline_lookup " &
                                  "WHERE subjid <> '-9'  AND next_appt_6m < now() AND " &
                                  "((subjid NOT IN (select subjid from followup where primary_endpoint_visit = 1 )) OR (subjid NOT IN (select subjid from followup_lookup where primary_endpoint_visit = 1)))  AND " &
                                  "(next_appt_6m  IS NOT NULL AND next_appt_6m  BETWEEN #" & startDateStr & "# AND #" & endDateStr & "#) "

                ' Create data adapter and table
                dataAdapter = New OleDbDataAdapter(query, connection)
                dataTable = New DataTable()

                ' Fill data table
                dataAdapter.Fill(dataTable)

                ' Add columns for SMS 
                dataTable.Columns.Add("Gender", GetType(String))
                dataTable.Columns.Add("Clinic", GetType(String))

                ' Calculate follow-up dates and add SMS message

                For Each row As DataRow In dataTable.Rows

                    Dim gender As Integer = CInt(row("client_sex"))
                    Dim clinic As Integer = CInt(row("health_facility"))
                    Dim nick_name As String = row("nickname")

                    If nick_name = "-6" Then
                        row("nickname") = ""
                    End If


                    If gender = 1 Then
                        row("Gender") = "Male"
                    Else
                        row("Gender") = "Female"
                    End If

                    If clinic = 21 Then
                        row("Clinic") = "Homa Bay Teaching and Referral Hospital"
                    ElseIf clinic = 22 Then
                        row("Clinic") = "Rachuonyo Sub County Hospital"
                    ElseIf clinic = 23 Then
                        row("Clinic") = "Suba Sub County Hospital"
                    ElseIf clinic = 24 Then
                        row("Clinic") = "Ndhiwa Sub County Hospital"
                    ElseIf clinic = 11 Then
                        row("Clinic") = "Bushenyi HCIV"
                    ElseIf clinic = 12 Then
                        row("Clinic") = "Ishaka Adventist Hospital (Bushenyi)"
                    ElseIf clinic = 13 Then
                        row("Clinic") = "Ishongororo HCIV (Ibanda)"
                    ElseIf clinic = 14 Then
                        row("Clinic") = "Ruhoko HCIV (Ibanda)"
                    End If

                Next


                dgvBaselineData.DataSource = dataTable

                ' Customize column headers
                dgvBaselineData.Columns("subjid").HeaderText = "Study ID"
                dgvBaselineData.Columns("mobile_number").HeaderText = "Phone Number"
                dgvBaselineData.Columns("participants_name").HeaderText = "Participants Name"
                dgvBaselineData.Columns("nickname").HeaderText = "Other Name"
                dgvBaselineData.Columns("Gender").HeaderText = "Gender"
                dgvBaselineData.Columns("Clinic").HeaderText = "Clinic"
                dgvBaselineData.Columns("next_appt_6m").HeaderText = "Month 6 Appointment Date"


                ' Format date columns
                dgvBaselineData.Columns("next_appt_6m").DefaultCellStyle.Format = "dd/MM/yyyy"
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
            saveFileDialog.FileName = "Pending_Followup_Export_" & DateTime.Now.ToString("yyyy-MM-dd") & ".csv"

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

    ' Components Defined in the code
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
Public Module MainModulePendingFollowup
    <STAThread()>
    Public Sub Main()
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)
        Application.Run(New PendingFollowup())
    End Sub
End Module