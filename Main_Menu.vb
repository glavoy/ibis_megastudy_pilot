Imports System.Configuration
Imports System.Data.OleDb

Public Class Main_Menu
    Private originalNamesTable As DataTable

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

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Try
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

    Private Sub ComboBoxCommunity_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBoxCommunity.SelectedIndexChanged
        If ComboBoxCommunity.SelectedIndex <> -1 Then
            If ComboBoxCommunity.Focused Then ' Ensure it's a user action
                'TextBoxPhoneSearch.Clear()
                SetTextBoxPlaceholder(True) ' Force reset the phone number
                TextBoxFilter.Text = ""
            End If
            Community = ComboBoxCommunity.Text
            Dim parts As String() = Community.Split(":"c) ' "c" indicates a character
            Community = parts(0)
            LoadNames("baseline")
        End If
    End Sub
    Private Sub SetTextBoxPlaceholder(Optional forceReset As Boolean = False)
        If forceReset OrElse String.IsNullOrWhiteSpace(TextBoxPhoneSearch.Text) Then
            TextBoxPhoneSearch.Text = "Enter phone number..."
            TextBoxPhoneSearch.ForeColor = Color.Gray
            TextBoxPhoneSearch.Font = New Font(TextBoxPhoneSearch.Font.FontFamily, 8, FontStyle.Italic)
        End If
    End Sub

    Private Sub LoadNames(ByVal tableName As String)
        Try
            Dim config As Configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None)
            Dim section As ConnectionStringsSection = DirectCast(config.GetSection("connectionStrings"), ConnectionStringsSection)
            Using Connection As New OleDbConnection(section.ConnectionStrings("ConnString").ConnectionString)
                Connection.Open()

                Dim strSQL As String = "SELECT subjid, participants_name, nickname, mobile_phone FROM " & tableName & " WHERE subjid NOT IN ('-9') AND  health_facility = @Community ORDER BY participants_name"
                Using cmd As New OleDbCommand(strSQL, Connection)
                    cmd.Parameters.AddWithValue("@Community", CInt(Community))


                    Dim da As New OleDbDataAdapter(cmd)
                    Dim dt As New DataTable
                    da.Fill(dt)

                    ' If no names exist, clear ComboBoxNames
                    If dt.Rows.Count = 0 Then
                        MessageBox.Show("No names found for this Clinic in " & tableName & ".", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information)
                        ComboBoxNames.DataSource = Nothing
                        Exit Sub
                    End If

                    ' Create a new DataTable with formatted display names
                    Dim formattedDt As New DataTable()
                    formattedDt.Columns.Add("subjid", GetType(String))
                    formattedDt.Columns.Add("DisplayName", GetType(String))

                    For Each row As DataRow In dt.Rows
                        Dim mainName As String = row("participants_name").ToString()
                        Dim otherName As String = row("nickname").ToString()
                        Dim subjid As String = row("subjid").ToString()
                        'Console.WriteLine("subjid Type: " & row("subjid").GetType().ToString())
                        Dim displayName As String
                        If Not String.IsNullOrEmpty(otherName) Then
                            displayName = mainName & " (" & otherName & ")"
                        Else
                            displayName = mainName
                        End If

                        formattedDt.Rows.Add(subjid, displayName)
                    Next

                    ' Store the original table before setting DataSource (For Filtering)
                    originalNamesTable = formattedDt.Copy()

                    ' Temporarily remove the event handler
                    RemoveHandler ComboBoxNames.SelectedIndexChanged, AddressOf ComboBoxNames_SelectedIndexChanged

                    ' Bind DataTable to ComboBox
                    ComboBoxNames.DataSource = formattedDt
                    ComboBoxNames.DisplayMember = "DisplayName"
                    ComboBoxNames.ValueMember = "subjid"

                    ' If at least one item exists, update SUBJID
                    If ComboBoxNames.Items.Count > 0 Then
                        SUBJID = ComboBoxNames.SelectedValue.ToString()
                    End If

                    ' Reattach the event handler
                    AddHandler ComboBoxNames.SelectedIndexChanged, AddressOf ComboBoxNames_SelectedIndexChanged

                    ' Manually trigger the event
                    ComboBoxNames_SelectedIndexChanged(ComboBoxNames, EventArgs.Empty)
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Error loading names from " & tableName & ": " & ex.Message)
        End Try
    End Sub

    Private Sub ComboBoxNames_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBoxNames.SelectedIndexChanged
        If ComboBoxNames.SelectedIndex <> -1 Then
            If ComboBoxNames.Focused Then ' Ensure it's a user action
                'TextBoxPhoneSearch.Clear()
                SetTextBoxPlaceholder(True) ' Force reset the phone number
                TextBoxFilter.Text = ""
            End If



            SUBJID = ComboBoxNames.SelectedValue.ToString()
            If Len(SUBJID) > 5 Then
                LoadParticipantDetails()
                ShowLabels()
            Else
                HideLabels()
            End If


        End If
    End Sub

    ' Function to Filter ComboBoxNames based on user input
    Private Sub FilterComboBoxNames()
        Dim searchText As String = TextBoxFilter.Text.ToLower().Trim()

        ' Store currently selected value before filtering
        Dim previousSubjid As String = ""
        If ComboBoxNames.SelectedValue IsNot Nothing Then
            previousSubjid = ComboBoxNames.SelectedValue.ToString()
        End If

        ' Ensure DataTable exists before filtering
        If originalNamesTable IsNot Nothing AndAlso originalNamesTable.Rows.Count > 0 Then
            ' Create a filtered DataTable
            Dim filteredTable As DataTable = originalNamesTable.Clone() ' Clone structure

            ' Add matching rows to the new filtered table
            For Each row As DataRow In originalNamesTable.Rows
                If row("DisplayName").ToString().ToLower().Contains(searchText) Then
                    filteredTable.ImportRow(row) ' Preserve both hhid & DisplayName
                End If
            Next

            ' Temporarily close dropdown to prevent flickering
            ComboBoxNames.DroppedDown = False

            ' Bind filtered DataTable while keeping DisplayMember and ValueMember
            ComboBoxNames.DataSource = If(filteredTable.Rows.Count > 0, filteredTable, Nothing)
            ComboBoxNames.DisplayMember = "DisplayName"
            ComboBoxNames.ValueMember = "subjid"

            ' Restore previous selection if it's still in the filtered list
            If previousSubjid <> "" Then
                ComboBoxNames.SelectedValue = previousSubjid
            End If

            ComboBoxNames.Refresh()
            Cursor.Show()
            Application.DoEvents()

            ' Open dropdown only when there are matches and user is typing
            If filteredTable.Rows.Count > 0 AndAlso TextBoxFilter.Focused Then
                ComboBoxNames.DroppedDown = True
            End If
        End If
    End Sub

    Private Sub TextBoxFilter_TextChanged(sender As Object, e As EventArgs) Handles TextBoxFilter.TextChanged
        FilterComboBoxNames()
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
        LabelNationalId.Visible = False
        LabelDOB.Visible = False
        LabelCounty.Visible = False
        LabelSubcounty.Visible = False
        LabelVillage.Visible = False
    End Sub

    Private Sub Main_Menu_Load(sender As Object, e As EventArgs) Handles Me.Load
        SetTextBoxPlaceholder()
    End Sub
End Class

