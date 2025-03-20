Imports System.Configuration
Imports System.Data.OleDb

Public Class Main_Menu
    'Private originalNamesTable As DataTable
    Private allParticipants As New List(Of String)
    Private allSubjectIDs As New Dictionary(Of String, String)  ' Map name to subjid

    ' Form-level variables to track placeholder states
    Private TextBoxFilter_IsPlaceholder As Boolean = True
    Private TextBoxPhoneNumber_IsPlaceholder As Boolean = True

    Private Sub Main_Menu_Load(sender As Object, e As EventArgs) Handles Me.Load
        ' Setup day of birth combo box
        ComboBoxDayOfBirth.Items.Add("SELECT DAY")
        For i As Integer = 1 To 31
            ComboBoxDayOfBirth.Items.Add(i)
        Next
        ComboBoxDayOfBirth.SelectedIndex = 0
        ComboBoxDayOfBirth.DropDownStyle = ComboBoxStyle.DropDownList

        ' Load all participants from database
        LoadAllParticipants()

        ' Initial population of the name combo box with all names
        PopulateNameComboBox()

        ' Set ComboBoxName to DropDownList style for consistency
        ComboBoxName.DropDownStyle = ComboBoxStyle.DropDownList

        ' Set default button text
        ButtonFoundParticipant.Text = "USE PARTICIPANT"

        ' Setup placeholder text for TextBoxFilter
        TextBoxFilter.Text = "TYPE NAME TO SEARCH"
        TextBoxFilter.ForeColor = Color.Gray
        TextBoxFilter.Font = New Font(TextBoxFilter.Font, FontStyle.Italic)

        ' Setup placeholder text for TextBoxPhoneNumber
        TextBoxPhoneNumber.Text = "ENTER PHONE NUMBER"
        TextBoxPhoneNumber.ForeColor = Color.Gray
        TextBoxPhoneNumber.Font = New Font(TextBoxPhoneNumber.Font, FontStyle.Italic)
        ButtonFoundParticipant.Text = "USE PARTICIPANT"

        Dim originalFont = TextBoxPhoneNumber.Parent.Font  ' Use parent font as reference
        Dim smallerFont = New Font(originalFont.FontFamily, originalFont.Size - 2, FontStyle.Italic)
        TextBoxPhoneNumber.Font = smallerFont



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


    Private Sub ComboBoxDayOfBirth_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBoxDayOfBirth.SelectedIndexChanged
        PopulateNameComboBox()
    End Sub



    ' Populate the name combo box with filtered results
    Private Sub PopulateNameComboBox()
        ComboBoxName.Items.Clear()

        ' Add "SELECT NAME" prompt as the first item
        ComboBoxName.Items.Add("SELECT NAME")

        ' Get filter text (already in uppercase from TextBoxFilter_TextChanged)
        Dim filterText As String = TextBoxFilter.Text

        ' Get selected day (if any)
        Dim selectedDay As Integer = -1
        If ComboBoxDayOfBirth.SelectedIndex > 0 Then
            selectedDay = CInt(ComboBoxDayOfBirth.SelectedItem)
        End If

        ' If day is selected, we need to query the database for day-specific filtering
        If selectedDay > 0 Then
            Using connection As New OleDbConnection(ConfigurationManager.ConnectionStrings("ConnString").ConnectionString)
                ' Update query to include subjid
                Dim query As String = "SELECT participants_name, subjid FROM baseline WHERE UCASE(participants_name) LIKE ? AND day_of_birth = ? ORDER BY participants_name"
                Using command As New OleDbCommand(query, connection)
                    command.Parameters.AddWithValue("?", "%" & filterText & "%")
                    command.Parameters.AddWithValue("?", selectedDay)

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
        If ComboBoxDayOfBirth.SelectedIndex > 0 Then ' Skip "Select day" item
            selectedDay = CInt(ComboBoxDayOfBirth.SelectedItem)
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
            End If

            ' Update the button text with the selected name in uppercase
            ButtonFoundParticipant.Text = "USE" & vbNewLine & selectedName
        Else
            ' Reset button text and SUBJID if "SELECT NAME" is selected
            ButtonFoundParticipant.Text = "USE PARTICIPANT"
            SUBJID = ""
        End If
    End Sub











    Private Sub ButtonBaseline_Click(sender As Object, e As EventArgs) Handles ButtonBaseline.Click
        Try
            Dim response As Integer
            Dim enterfu As Integer
            If Len(SUBJID) > 10 Then
                response = MsgBox("The Participant that is currently selected is " & ParticipantsName & vbCrLf & "Do you want to edit the baseline previous entry?", vbYesNo, "New Participant")
                If response = vbNo Then
                    enterfu = MsgBox("The Participant that is currently selected is " & ParticipantsName & vbCrLf & "Do you want to do the Follow up Visit?", vbYesNo, "Follow up Visit")
                    If enterfu = vbNo Then
                        Survey = "baseline"
                        NewSurvey.ShowDialog()
                        NewSurvey.Dispose()
                    Else
                        'load the follow-up survey
                    End If

                Else
                    Survey = "baseline"
                    ModifyingSurvey = True
                    NewSurvey.ShowDialog()
                    NewSurvey.Dispose()
                End If
            Else
                response = MsgBox("You have not prescreened the participant, Do you want to proceed without prescreening?" & vbCrLf & "Note that this may cause duplicate enrollment", vbYesNo, "New Participant")
                If response = vbYes Then
                    Survey = "baseline"
                    NewSurvey.ShowDialog()
                    NewSurvey.Dispose()
                End If

            End If


        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
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
            Dim config As Configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None)
            Dim section As ConnectionStringsSection = DirectCast(config.GetSection("connectionStrings"), ConnectionStringsSection)
            Using Connection As New OleDbConnection(section.ConnectionStrings("ConnString").ConnectionString)
                Connection.Open()

                ' Query to fetch household details from precensus
                Dim strSQL As String = "
                SELECT subjid, respondants_age, participants_name, 
                       NickName, mobile_number, client_sex, health_facility, county,
                        subcounty, village
                FROM baseline
                WHERE subjid = @subjid"

                Using cmd As New OleDbCommand(strSQL, Connection)
                    cmd.Parameters.AddWithValue("@subjid", SUBJID)

                    Using reader As OleDbDataReader = cmd.ExecuteReader()
                        If reader.Read() Then
                            ' Populate global variables
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
    End Sub


End Class

