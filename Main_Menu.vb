Imports System.Configuration
Imports System.Data.OleDb
Imports System.Net

Public Class Main_Menu
    'Private originalNamesTable As DataTable
    Private allParticipants As New List(Of String)
    Private allSubjectIDs As New Dictionary(Of String, String)  ' Map name to subjid

    ' Form-level variables to track placeholder states
    Private TextBoxFilter_IsPlaceholder As Boolean = True
    Private TextBoxPhoneNumber_IsPlaceholder As Boolean = True



    Private Sub Main_Menu_Load(sender As Object, e As EventArgs) Handles Me.Load
        ' Setup day of birth combo box
        ' Setup month of birth combo box
        Me.Text = SW_VER
        ComboBoxMonthOfBirth.Items.Clear()
        ComboBoxMonthOfBirth.Items.Add("SELECT MONTH")


        ' Create a list of month names with their corresponding numbers
        Dim monthNames As New List(Of KeyValuePair(Of Integer, String))
        monthNames.Add(New KeyValuePair(Of Integer, String)(1, "January"))
        monthNames.Add(New KeyValuePair(Of Integer, String)(2, "February"))
        monthNames.Add(New KeyValuePair(Of Integer, String)(3, "March"))
        monthNames.Add(New KeyValuePair(Of Integer, String)(4, "April"))
        monthNames.Add(New KeyValuePair(Of Integer, String)(5, "May"))
        monthNames.Add(New KeyValuePair(Of Integer, String)(6, "June"))
        monthNames.Add(New KeyValuePair(Of Integer, String)(7, "July"))
        monthNames.Add(New KeyValuePair(Of Integer, String)(8, "August"))
        monthNames.Add(New KeyValuePair(Of Integer, String)(9, "September"))
        monthNames.Add(New KeyValuePair(Of Integer, String)(10, "October"))
        monthNames.Add(New KeyValuePair(Of Integer, String)(11, "November"))
        monthNames.Add(New KeyValuePair(Of Integer, String)(12, "December"))

        ' Add month names to the ComboBox
        For Each monthItem In monthNames
            ComboBoxMonthOfBirth.Items.Add(monthItem)
        Next
        ' Set default selection and style
        ComboBoxMonthOfBirth.SelectedIndex = 0
        ComboBoxMonthOfBirth.DropDownStyle = ComboBoxStyle.DropDownList
        ComboBoxMonthOfBirth.DisplayMember = "Value"
        ComboBoxMonthOfBirth.ValueMember = "Key"

        ' Load all participants from database
        LoadAllParticipants()

        ' Initial population of the name combo box with all names
        PopulateNameComboBox()

        ' Set ComboBoxName to DropDownList style for consistency
        ComboBoxName.DropDownStyle = ComboBoxStyle.DropDownList

        ' Setup placeholder text for TextBoxFilter
        TextBoxFilter.Text = "TYPE NAME TO SEARCH"
        TextBoxFilter.ForeColor = Color.Gray
        TextBoxFilter.Font = New Font(TextBoxFilter.Font, FontStyle.Italic)

        ' Setup placeholder text for TextBoxPhoneNumber
        TextBoxPhoneNumber.Text = "ENTER PHONE NUMBER"
        TextBoxPhoneNumber.ForeColor = Color.Gray
        TextBoxPhoneNumber.Font = New Font(TextBoxPhoneNumber.Font, FontStyle.Italic)
        ButtonFollowupSurvey.Text = "Follow-up Survey"

        Dim originalFont = TextBoxPhoneNumber.Parent.Font  ' Use parent font as reference
        Dim smallerFont = New Font(originalFont.FontFamily, originalFont.Size - 2, FontStyle.Italic)
        TextBoxPhoneNumber.Font = smallerFont

        HideLabels()

    End Sub

    Private Sub LoadAllParticipants()
        allParticipants.Clear()
        allSubjectIDs.Clear()

        Using connection As New OleDbConnection(ConfigurationManager.ConnectionStrings("ConnString").ConnectionString)
            Dim query As String = "SELECT participants_name, subjid FROM baseline ORDER BY participants_name"
            Using command As New OleDbCommand(query, connection)
                connection.Open()
                Dim reader As OleDbDataReader = command.ExecuteReader()

                While reader.Read()
                    ' Store names in uppercase
                    Dim name As String = reader("participants_name").ToString().ToUpper()
                    Dim id As String = reader("subjid").ToString()

                    allParticipants.Add(name)
                    allSubjectIDs(name) = id  ' Store the mapping
                End While

                reader.Close()
            End Using
        End Using
    End Sub

    ' Modified TextChanged to handle placeholder text
    Private Sub TextBoxFilter_TextChanged(sender As Object, e As EventArgs) Handles TextBoxFilter.TextChanged
        ' Skip filtering if this is placeholder text
        If TextBoxFilter_IsPlaceholder Then
            Return
        End If

        ' Convert input to uppercase
        If Not TextBoxFilter.Text.Equals(TextBoxFilter.Text.ToUpper()) Then
            Dim selStart As Integer = TextBoxFilter.SelectionStart
            TextBoxFilter.Text = TextBoxFilter.Text.ToUpper()
            TextBoxFilter.SelectionStart = selStart
        End If

        ' Populate with filtered results
        PopulateNameComboBox()

        ' Show the dropdown list if we have results and the textbox has content
        If ComboBoxName.Items.Count > 1 AndAlso Not String.IsNullOrWhiteSpace(TextBoxFilter.Text) Then
            ComboBoxName.DroppedDown = True
        Else
            ComboBoxName.DroppedDown = False
        End If
    End Sub


    ' TextBoxFilter placeholder handling
    Private Sub TextBoxFilter_GotFocus(sender As Object, e As EventArgs) Handles TextBoxFilter.GotFocus
        If TextBoxFilter_IsPlaceholder Then
            TextBoxFilter.Text = ""
            TextBoxFilter.ForeColor = Color.Black
            TextBoxFilter.Font = New Font(TextBoxFilter.Font, FontStyle.Regular)
            TextBoxFilter_IsPlaceholder = False
        End If
    End Sub


    Private Sub TextBoxFilter_LostFocus(sender As Object, e As EventArgs) Handles TextBoxFilter.LostFocus
        If String.IsNullOrWhiteSpace(TextBoxFilter.Text) Then
            TextBoxFilter.Text = "TYPE NAME TO SEARCH"
            TextBoxFilter.ForeColor = Color.Gray
            TextBoxFilter.Font = New Font(TextBoxFilter.Font, FontStyle.Italic)
            TextBoxFilter_IsPlaceholder = True
        End If
    End Sub




    ' TextBoxPhoneNumber placeholder handling
    Private Sub TextBoxPhoneNumber_GotFocus(sender As Object, e As EventArgs) Handles TextBoxPhoneNumber.GotFocus
        If TextBoxPhoneNumber_IsPlaceholder Then
            TextBoxPhoneNumber.Text = ""
            TextBoxPhoneNumber.ForeColor = Color.Black
            TextBoxPhoneNumber.Font = New Font(TextBoxPhoneNumber.Font, FontStyle.Regular)
            TextBoxPhoneNumber_IsPlaceholder = False
        End If
    End Sub

    Private Sub TextBoxPhoneNumber_LostFocus(sender As Object, e As EventArgs) Handles TextBoxPhoneNumber.LostFocus
        If String.IsNullOrWhiteSpace(TextBoxPhoneNumber.Text) Then
            TextBoxPhoneNumber.Text = "ENTER PHONE NUMBER"
            TextBoxPhoneNumber.ForeColor = Color.Gray  ' Darker gray

            ' Maintain the smaller font size when placeholder reappears
            Dim originalFont = TextBoxPhoneNumber.Parent.Font  ' Use parent font as reference
            Dim smallerFont = New Font(originalFont.FontFamily, originalFont.Size - 2, FontStyle.Italic)
            TextBoxPhoneNumber.Font = smallerFont

            TextBoxPhoneNumber_IsPlaceholder = True
        End If
    End Sub

    ' Make sure to convert phone numbers to uppercase too
    Private Sub TextBoxPhoneNumber_TextChanged(sender As Object, e As EventArgs) Handles TextBoxPhoneNumber.TextChanged
        ' Skip if this is placeholder text
        If TextBoxPhoneNumber_IsPlaceholder Then
            Return
        End If

        ' Convert to uppercase
        If Not TextBoxPhoneNumber.Text.Equals(TextBoxPhoneNumber.Text.ToUpper()) Then
            Dim selStart As Integer = TextBoxPhoneNumber.SelectionStart
            TextBoxPhoneNumber.Text = TextBoxPhoneNumber.Text.ToUpper()
            TextBoxPhoneNumber.SelectionStart = selStart
        End If
    End Sub


    Private Sub ComboBoxMonthOfBirth_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBoxMonthOfBirth.SelectedIndexChanged
        PopulateNameComboBox()
    End Sub



    ' Populate the name combo box with filtered results
    Private Sub PopulateNameComboBox()
        ComboBoxName.Items.Clear()

        ' Add "SELECT NAME" prompt as the first item
        ComboBoxName.Items.Add("SELECT NAME")

        ' Get filter text (already in uppercase from TextBoxFilter_TextChanged)
        Dim filterText As String = TextBoxFilter.Text

        Dim selectedMonth As Integer = -1
        If ComboBoxMonthOfBirth.SelectedIndex > 0 Then
            Dim monthItem As KeyValuePair(Of Integer, String) = DirectCast(ComboBoxMonthOfBirth.SelectedItem, KeyValuePair(Of Integer, String))
            selectedMonth = monthItem.Key
        End If

        ' If day is selected, we need to query the database for day-specific filtering
        If selectedMonth > 0 Then
            Using connection As New OleDbConnection(ConfigurationManager.ConnectionStrings("ConnString").ConnectionString)
                ' Update query to include subjid
                Dim query As String = "SELECT participants_name, subjid FROM baseline WHERE UCASE(participants_name) LIKE ? OR month_of_birth = ? ORDER BY participants_name"
                Using command As New OleDbCommand(query, connection)
                    command.Parameters.AddWithValue("?", "%" & filterText & "%")
                    command.Parameters.AddWithValue("?", selectedMonth)

                    connection.Open()
                    Dim reader As OleDbDataReader = command.ExecuteReader()

                    While reader.Read()
                        ' Add names in uppercase
                        Dim name As String = reader("participants_name").ToString().ToUpper()
                        Dim id As String = reader("subjid").ToString()

                        ComboBoxName.Items.Add(name)

                        ' Update the mapping if not already present or if changed
                        If Not allSubjectIDs.ContainsKey(name) Or allSubjectIDs(name) <> id Then
                            allSubjectIDs(name) = id
                        End If
                    End While

                    reader.Close()
                End Using
            End Using
        Else
            ' No day selected, just filter by name from our cached list
            For Each name As String In allParticipants
                If name.Contains(filterText) Then
                    ComboBoxName.Items.Add(name)
                End If
            Next
        End If

        ' Select "SELECT NAME" item (index 0)
        ComboBoxName.SelectedIndex = 0
    End Sub



    Private Sub FilterParticipants()
        ' Clear current items in the name combobox
        ComboBoxName.Items.Clear()

        ' Get filter text (convert to uppercase for case-insensitive comparison)
        Dim filterText As String = TextBoxFilter.Text.ToUpper()

        ' Get selected day (if any)
        Dim selectedDay As Integer = -1
        If ComboBoxMonthOfBirth.SelectedIndex > 0 Then ' Skip "Select day" item
            selectedDay = CInt(ComboBoxMonthOfBirth.SelectedItem)
        End If

        ' Query to get filtered participants
        Dim query As String = "SELECT participants_name FROM baseline WHERE UCASE(participants_name) LIKE ?"

        ' Add day filter if selected
        If selectedDay > 0 Then
            query += " AND day_of_birth = ?"
        End If

        ' Create connection and command
        Using connection As New OleDbConnection(ConfigurationManager.ConnectionStrings("ConnString").ConnectionString)
            Using command As New OleDbCommand(query, connection)
                ' Add parameters
                command.Parameters.AddWithValue("?", "%" & filterText & "%")

                If selectedDay > 0 Then
                    command.Parameters.AddWithValue("?", selectedDay)
                End If

                ' Open connection and execute query
                connection.Open()
                Dim reader As OleDbDataReader = command.ExecuteReader()

                ' Add matching names to combobox
                While reader.Read()
                    ComboBoxName.Items.Add(reader("participants_name").ToString())
                End While

                reader.Close()
            End Using
        End Using

        ' Select first item if any results
        If ComboBoxName.Items.Count > 0 Then
            ComboBoxName.SelectedIndex = 0
        End If
    End Sub






    ' Update the ComboBoxName_SelectedIndexChanged event to set the global SUBJID variable
    Private Sub ComboBoxName_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBoxName.SelectedIndexChanged
        ' Check if a name is selected (not the "SELECT NAME" prompt)
        If ComboBoxName.SelectedIndex > 0 Then
            Dim selectedName As String = ComboBoxName.Text

            ' Update the global SUBJID variable if we have a matching subjid
            If allSubjectIDs.ContainsKey(selectedName) Then
                SUBJID = allSubjectIDs(selectedName)
                LoadParticipantDetails()
                ShowLabels()
                ButtonBaseline.Enabled = False
            End If

            ' Update the button text with the selected name in uppercase
            ButtonFollowupSurvey.Text = "Follow-up Survey for:" & vbNewLine & selectedName
            ButtonFollowupSurvey.Enabled = True
            ButtonEditFollowup.Enabled = True
        Else
            ' Reset button text and SUBJID if "SELECT NAME" is selected
            ButtonFollowupSurvey.Text = "Follow-up Survey"
            SUBJID = ""
            HideLabels()
            ButtonBaseline.Enabled = True
        End If
    End Sub











    Private Sub ButtonBaseline_Click(sender As Object, e As EventArgs) Handles ButtonBaseline.Click
        ModifyingSurvey = False
        Survey = "baseline"
        NewSurvey.ShowDialog()
        NewSurvey.Dispose()
    End Sub


    Private Sub ButtonQuit_Click(sender As Object, e As EventArgs) Handles ButtonQuit.Click
        End
    End Sub

    Private Sub ButtonEditBaseline_Click(sender As Object, e As EventArgs) Handles ButtonEditBaseline.Click
        Try
            ModifyingSurvey = True
            Survey = "baseline"
            SelectFormToEdit.ShowDialog()
            SelectFormToEdit.Dispose()
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub

    Private Sub ButtonSMSSchedule_Click(sender As Object, e As EventArgs) Handles ButtonSMSSchedule.Click
        SMSSchedule.ShowDialog()
        SMSSchedule.Dispose()
    End Sub




    ' Function to Load Household Details for Control Arm
    Private Sub LoadParticipantDetails()
        Try
            Using Connection As New OleDbConnection(ConfigurationManager.ConnectionStrings("ConnString").ConnectionString)
                Connection.Open()

                ' Query to fetch household details from precensus
                Dim strSQL As String = "
                SELECT subjid,screening_id, respondants_age, participants_name, 
                       NickName, mobile_number, client_sex, health_facility, county,
                        subcounty, village, next_appt_3m, next_appt_6m, appt_w1_2m, appt_w2_8m
                FROM baseline
                WHERE subjid = @subjid"

                Using cmd As New OleDbCommand(strSQL, Connection)
                    cmd.Parameters.AddWithValue("@subjid", SUBJID)

                    Using reader As OleDbDataReader = cmd.ExecuteReader()
                        If reader.Read() Then
                            ' Populate global variables
                            SUBJID = If(reader("subjid").ToString() = "-9", reader("screening_id").ToString(), reader("subjid").ToString())
                            Community = reader("health_facility").ToString()
                            ParticipantsName = reader("participants_name").ToString()
                            ParticipantsOtherName = If(reader("NickName").ToString() = "-6", "N/A", reader("NickName").ToString())
                            ParticipantsAge = reader("respondants_age").ToString()
                            ParticipantsGender = reader("client_sex").ToString()
                            County = reader("county").ToString()
                            Subcounty = reader("subcounty").ToString()
                            Village = reader("village").ToString()

                            ' Populate Labels
                            LabelSubjid.Text = "SUBJID: " & SUBJID
                            LabelParticipants_name.Text = "Participants Name: " & ParticipantsName
                            LabelNickname.Text = "Other Names: " & ParticipantsOtherName
                            LabelAge.Text = "Age: " & ParticipantsAge
                            LabelSex.Text = "Gender: " & If(ParticipantsGender = "1", "Male", "Female")
                            LabelCounty.Text = "County: " & County
                            LabelSubcounty.Text = "Sub County: " & Subcounty
                            LabelVillage.Text = "Village: " & Village
                            LabelFuwindow.Text = "Target follow-up window: " & Microsoft.VisualBasic.Left(reader("next_appt_3m").ToString(), 10) & " to " & Microsoft.VisualBasic.Left(reader("next_appt_6m").ToString(), 10)
                            LabelFuAllow.Text = "Allowable follow-up window: " & Microsoft.VisualBasic.Left(reader("appt_w1_2m").ToString(), 10) & " to " & Microsoft.VisualBasic.Left(reader("appt_w2_8m").ToString(), 10)


                            ' Handle NULL values for phone numbers
                            Dim phone1 As String = If(IsDBNull(reader("mobile_number")), "N/A", reader("mobile_number").ToString())


                            LabelPhone_number.Text = "Phone Number: " & phone1


                            ' Show Control-specific Labels
                            ShowLabels()

                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Error loading Participant details: " & ex.Message)
        End Try
    End Sub

    ' Show Control-specific labels
    Private Sub ShowLabels()
        LabelSubjid.Visible = True
        LabelParticipants_name.Visible = True
        LabelNickname.Visible = True
        LabelSex.Visible = True
        LabelPhone_number.Visible = True
        LabelAge.Visible = True
        LabelCounty.Visible = True
        LabelSubcounty.Visible = True
        LabelVillage.Visible = True
        LabelFuAllow.Visible = True
        LabelFuwindow.Visible = True
    End Sub

    ' Hide Control-specific labels
    Private Sub HideLabels()
        LabelSubjid.Visible = False
        LabelParticipants_name.Visible = False
        LabelNickname.Visible = False
        LabelSex.Visible = False
        LabelPhone_number.Visible = False
        LabelAge.Visible = False
        LabelCounty.Visible = False
        LabelSubcounty.Visible = False
        LabelVillage.Visible = False
        LabelFuAllow.Visible = False
        LabelFuwindow.Visible = False
    End Sub

    Private Sub ButtonCannotFind_Click(sender As Object, e As EventArgs) Handles ButtonCannotFind.Click
        ' Reset the form to its initial state, similar to Form_Load

        ' Reset ComboBoxes to initial state
        ComboBoxMonthOfBirth.SelectedIndex = 0
        ComboBoxName.SelectedIndex = 0

        ' Reset TextBoxes to placeholder state
        ' Reset TextBoxFilter
        TextBoxFilter.Text = "TYPE NAME TO SEARCH"
        TextBoxFilter.ForeColor = Color.Gray
        TextBoxFilter.Font = New Font(TextBoxFilter.Font, FontStyle.Italic)
        TextBoxFilter_IsPlaceholder = True

        ' Reset TextBoxPhoneNumber
        TextBoxPhoneNumber.Text = "ENTER PHONE NUMBER"
        TextBoxPhoneNumber.ForeColor = Color.Gray
        Dim originalFont = TextBoxPhoneNumber.Parent.Font
        Dim smallerFont = New Font(originalFont.FontFamily, originalFont.Size - 2, FontStyle.Italic)
        TextBoxPhoneNumber.Font = smallerFont
        TextBoxPhoneNumber_IsPlaceholder = True

        ' Reset button text and variables as requested
        ButtonFollowupSurvey.Text = "Follow-up Survey"
        SUBJID = ""

        ' Hide labels
        HideLabels()

        ' Enable baseline button
        ButtonBaseline.Enabled = True
    End Sub

    Private Sub ButtonSearchPhone_Click(sender As Object, e As EventArgs) Handles ButtonSearchPhone.Click
        Dim phoneNumber As String = TextBoxPhoneNumber.Text.Trim()

        ' Ensure phone number is entered before searching
        If phoneNumber = "" Then
            MessageBox.Show("Please enter a phone number.", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        ' Call the search function
        SearchByPhoneNumber(phoneNumber)
    End Sub



    Private Sub SearchByPhoneNumber(phoneNumber As String)
        ' Validate phone number format
        If Not IsValidPhoneNumber(phoneNumber) Then
            MessageBox.Show("Invalid phone number format. Phone number should be 10 digits and start with 0.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Try
            Using connection As New OleDbConnection(ConfigurationManager.ConnectionStrings("ConnString").ConnectionString)
                connection.Open()

                ' Query to search by phone number
                Dim query As String = "SELECT subjid, health_facility, participants_name, NickName, " &
                                     "respondants_age, client_sex, county, subcounty, village, mobile_number, " &
                                     "next_appt_3m, next_appt_6m, appt_w1_2m, appt_w2_8m " &
                                     "FROM baseline WHERE mobile_number = ?"

                Using command As New OleDbCommand(query, connection)
                    command.Parameters.AddWithValue("?", phoneNumber)

                    Using reader As OleDbDataReader = command.ExecuteReader()
                        If reader.Read() Then
                            ' Participant found - update the global variables
                            SUBJID = reader("subjid").ToString()
                            Community = reader("health_facility").ToString()
                            ParticipantsName = reader("participants_name").ToString()
                            ParticipantsOtherName = reader("NickName").ToString()
                            ParticipantsAge = reader("respondants_age").ToString()
                            ParticipantsGender = reader("client_sex").ToString()
                            County = reader("county").ToString()
                            Subcounty = reader("subcounty").ToString()
                            Village = reader("village").ToString()

                            ' Populate Labels
                            LabelSubjid.Text = "SUBJID: " & SUBJID
                            LabelParticipants_name.Text = "Participants Name: " & ParticipantsName
                            LabelNickname.Text = "Other Names: " & ParticipantsOtherName
                            LabelAge.Text = "Age: " & ParticipantsAge
                            LabelSex.Text = "Gender: " & If(ParticipantsGender = "1", "Male", "Female")
                            LabelCounty.Text = "County: " & County
                            LabelSubcounty.Text = "Sub County: " & Subcounty
                            LabelVillage.Text = "Village: " & Village
                            LabelFuwindow.Text = "Target follow-up window: " & Microsoft.VisualBasic.Left(reader("next_appt_3m").ToString(), 10) & " to " & Microsoft.VisualBasic.Left(reader("next_appt_6m").ToString(), 10)
                            LabelFuAllow.Text = "Allowable follow-up window: " & Microsoft.VisualBasic.Left(reader("appt_w1_2m").ToString(), 10) & " to " & Microsoft.VisualBasic.Left(reader("appt_w2_8m").ToString(), 10)

                            ' Handle NULL values for phone number
                            Dim phone1 As String = If(IsDBNull(reader("mobile_number")), "N/A", reader("mobile_number").ToString())
                            LabelPhone_number.Text = "Phone Number: " & phone1

                            ' Show the labels
                            ShowLabels()

                            ' Update the button text with the found name
                            ButtonFollowupSurvey.Text = "Follow-up Survey for:" & vbNewLine & ParticipantsName
                            ButtonFollowupSurvey.Enabled = True
                            ButtonEditFollowup.Enabled = True
                            ' Disable baseline button since we've found a participant
                            ButtonBaseline.Enabled = False
                        Else
                            ' No participant found with that phone number
                            MessageBox.Show("No participant found with the phone number: " & phoneNumber, "No Results", MessageBoxButtons.OK, MessageBoxIcon.Information)
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Error searching for participant: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Function IsValidPhoneNumber(phoneNumber As String) As Boolean
        ' Check if the phone number is exactly 10 digits and starts with 0
        Return phoneNumber.Length = 10 AndAlso phoneNumber.StartsWith("0") AndAlso IsNumeric(phoneNumber)
    End Function

    Private Sub ButtonBackupDB_Click(sender As Object, e As EventArgs) Handles ButtonBackupDB.Click
        Cursor = Cursors.WaitCursor
        Application.DoEvents()
        Try
            If CheckForInternetConnection() = True Then
                ' Set the path to the Python interpreter
                Dim pythonPath As String = getPythonPath()

                ' Set the path to the Python script
                Dim scriptPath As String = "C:\IBIS_pilot\Scripts\upload_to_ftp_server_IBIS.py"
                ' Use the Shell function to execute the command
                Shell(pythonPath & " " & scriptPath, vbNormalFocus, True)

            Else
                MsgBox("You are not connected to the Internet. Please connect to the Internet and try again.")
            End If

        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
        Cursor = Cursors.Default
    End Sub

    ' Get Python Path
    Private Function GetPythonPath() As String
        Dim config As Configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None)
        Dim section As ConnectionStringsSection = DirectCast(config.GetSection("connectionStrings"), ConnectionStringsSection)
        Dim ConnectionString As New OleDbConnection(section.ConnectionStrings("ConnString").ConnectionString)
        ' Set the path to the Python interpreter
        Dim pythonPath As String = ""
        ConnectionString.Open()
        Dim strSQL As String = "select pythonpath from config"
        Dim daPP As New OleDbDataAdapter(strSQL, ConnectionString)
        Dim dsPP As New DataSet
        daPP.Fill(dsPP)
        For Each row As DataRow In dsPP.Tables(0).Rows
            pythonPath = row.Item("pythonpath")
        Next
        daPP.Dispose()
        dsPP.Dispose()

        ConnectionString.Close()

        Return pythonPath
    End Function
    Public Shared Function CheckForInternetConnection() As Boolean
        Try
            Using client = New WebClient()
                Using stream = client.OpenRead("http://www.google.com")
                    Return True
                End Using
            End Using
        Catch
            Return False
        End Try
    End Function

    Private Sub ButtonFollowupSurvey_Click(sender As Object, e As EventArgs) Handles ButtonFollowupSurvey.Click
        ModifyingSurvey = False
        Survey = "followup"
        NewSurvey.ShowDialog()
        NewSurvey.Dispose()
    End Sub

    Private Sub ButtonEditFollowup_Click(sender As Object, e As EventArgs) Handles ButtonEditFollowup.Click
        Try
            ModifyingSurvey = True
            Survey = "followup"
            SelectFormToEdit.ShowDialog()
            SelectFormToEdit.Dispose()
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub
End Class

