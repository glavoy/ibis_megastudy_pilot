Imports System.Configuration
Imports System.Data.OleDb

Module IBIS_Public

    Public SW_VER As String = ConfigurationManager.AppSettings("Version")

    Public Survey As String                                 'This is used to keep which survey we are doing - add household or household members
    Public SUBJID As String                                 'used to store the SUBJID
    Public UNIQUEID As String                               'used to store the UNIQUEID
    Public VDATE As String = "01/01/1899"                   'used to store the Visit Date
    Public Community As String                              'used to store the Community
    Public Village As String                                'selected village - from MainMenu
    Public ModifyingSurvey As Boolean = False               'keeps track of wether or not we are doing a new survey or modifying an existing one
    Public CurrentAutoValue As String                       'the current auto value of an automatic varaible
    Public DontKnow_Value As String                         'stores the current "Don't Know' value for a question
    Public NA_Value As String                               'stores the current "N/A' value for a question
    Public Refuse_Value As String                           'stores the current "Refuse' value for a question
    Public CurrentDateSelected As String = ""               'used to save date values selected from a calendar
    Public DEFAULT_DATE As String = "01/01/1899"            'default date
    Public InterviewCancelled As Boolean = False            'Keeps track of whether or not participant has completed the checkin
    Public RandArmText As String = ""                       'Just to store the randomization arm text
    Public RandArmID As Integer = 0                         'Just to store the randomization arm number
    Public CancelledLeaveNote As Boolean = False            'determines whether clinican recorded changes or not
    Public ParticipantsName As String
    Public ParticipantsOtherName As String
    Public ParticipantsAge As Integer
    Public ParticipantsGender As String
    Public ParticipantsPhone As Integer
    Public Subcounty As String
    Public County As String

    '*****************************************************
    ' Function to get the next line number
    '*****************************************************
    Private Function GetNextLineNum(tabletnum As Integer, strSQL As String, idColumn As String) As String
        Dim nextLineNum As String = "-9"
        Try
            Dim config As Configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None)
            Dim section As ConnectionStringsSection = DirectCast(config.GetSection("connectionStrings"), ConnectionStringsSection)
            Dim ConnectionString As New OleDbConnection(section.ConnectionStrings("ConnString").ConnectionString)
            ConnectionString.Open()

            Dim da As New OleDbDataAdapter(strSQL, ConnectionString)
            Dim ds As New DataSet
            da.Fill(ds)
            Dim dt As DataTable = ds.Tables(0)
            ConnectionString.Close()

            ' Initialize the list to hold numbers after the "-" that start with tabletnum
            Dim numbers As New List(Of Integer)

            ' Initialize MaxLinenum to start with the tabletnum prefix
            Dim MaxLinenum As Integer = tabletnum * 10

            For Each row As DataRow In dt.Rows
                If Not IsDBNull(row(idColumn)) Then
                    Dim record As String = row(idColumn).ToString()
                    Dim parts() As String = record.Split("-"c)
                    If parts.Length = 2 Then
                        Dim number As Integer
                        If Integer.TryParse(parts(1), number) AndAlso parts(1).StartsWith(tabletnum.ToString()) Then
                            numbers.Add(number)
                        End If
                    End If
                End If
            Next

            If numbers.Count > 0 Then
                Dim maxNumber As Integer = numbers.Max()
                Dim maxNumberStr As String = maxNumber.ToString()
                Dim tabletNumStr As String = tabletnum.ToString()
                ' Check if maxNumberStr is long enough for the substring operation
                If maxNumberStr.Length > tabletNumStr.Length Then
                    Dim suffix As Integer = Integer.Parse(maxNumberStr.Substring(tabletNumStr.Length))
                    MaxLinenum = Integer.Parse(tabletnum.ToString() & (suffix + 1).ToString())
                Else
                    ' Handle the case where the maxNumberStr is not long enough
                    MaxLinenum = Integer.Parse(tabletnum.ToString() & "1")
                End If
            End If

            ' Set the result
            nextLineNum = MaxLinenum.ToString()

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

        Return nextLineNum
    End Function


    '*****************************************************
    ' Function to get the next randomization arm
    '*****************************************************
    Public Function GetNextParticipantRandArm(clinic_code As Integer) As String
        Dim nextLineNum As String = "-9"
        Try
            Dim config As Configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None)
            Dim section As ConnectionStringsSection = DirectCast(config.GetSection("connectionStrings"), ConnectionStringsSection)
            Dim ConnectionString As New OleDbConnection(section.ConnectionStrings("ConnString").ConnectionString)
            ConnectionString.Open()
            Dim strSQL As String = "Select participant_randarm from baseline where consent = 1 and eligibility_check = 1 and health_facility = " & clinic_code & " order by starttime desc;"

            Dim da As New OleDbDataAdapter(strSQL, ConnectionString)
            Dim ds As New DataSet
            da.Fill(ds)
            Dim dt As DataTable = ds.Tables(0)
            ConnectionString.Close()

            ' Initialize MaxLinenum to start with the tabletnum prefix
            Dim MaxRandArm As Integer = 1

            For Each row As DataRow In dt.Rows
                If Not IsDBNull(row("participant_randarm")) Then
                    Dim arm As Integer = CInt(row("participant_randarm"))
                    MaxRandArm = arm + 1
                    If MaxRandArm > 12 Then
                        MaxRandArm = 1
                    End If
                    Exit For
                End If
            Next


            ' Set the result
            nextLineNum = MaxRandArm.ToString()

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

        Return nextLineNum
    End Function


    '*****************************************************
    ' Function to get the next randomization arm
    '*****************************************************
    Public Function GetRandArm(clinic_code As Integer, participant As Integer) As String
        Dim nextarm As Integer = -9
        Try
            Dim config As Configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None)
            Dim section As ConnectionStringsSection = DirectCast(config.GetSection("connectionStrings"), ConnectionStringsSection)
            Dim ConnectionString As New OleDbConnection(section.ConnectionStrings("ConnString").ConnectionString)
            ConnectionString.Open()
            Dim strSQL As String = "Select arm_code, participant from randomizationlist where participant = " & participant & " and health_facility = " & clinic_code

            Dim da As New OleDbDataAdapter(strSQL, ConnectionString)
            Dim ds As New DataSet
            da.Fill(ds)
            Dim dt As DataTable = ds.Tables(0)
            ConnectionString.Close()

            For Each row As DataRow In dt.Rows
                If Not IsDBNull(row("arm_code")) Then
                    nextarm = CInt(row("arm_code"))
                    Exit For
                End If
            Next

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

        Return nextarm
    End Function


    '*****************************************************
    ' Function to get the next randomization arm text
    '*****************************************************
    Public Function GetNextRandArmText(clinic_code As Integer, arm As Integer) As String
        Dim nextRandArmText As String = "-9"
        Try
            Dim config As Configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None)
            Dim section As ConnectionStringsSection = DirectCast(config.GetSection("connectionStrings"), ConnectionStringsSection)
            Dim ConnectionString As New OleDbConnection(section.ConnectionStrings("ConnString").ConnectionString)
            ConnectionString.Open()
            Dim strSQL As String = "Select arm, participant from randomizationlist where health_facility = " & clinic_code & " and participant = " & arm

            Dim da As New OleDbDataAdapter(strSQL, ConnectionString)
            Dim ds As New DataSet
            da.Fill(ds)
            Dim dt As DataTable = ds.Tables(0)
            ConnectionString.Close()


            For Each row As DataRow In dt.Rows
                If Not IsDBNull(row("arm")) Then
                    ' Set the result
                    nextRandArmText = row("arm")
                End If
            Next

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

        Return nextRandArmText
    End Function


    '**********************************************************
    'Function to get a Line Number for a new IBIS Screening ID
    '**********************************************************
    Public Function GetIBISLineNum(hhid As String, tabletnum As Integer) As String
        Dim hhid_len As Integer = Len(hhid)
        Dim strSQL As String = "SELECT screening_id FROM baseline WHERE left(screening_id," & hhid_len & ") = '" & hhid & "'"
        Return GetNextLineNum(tabletnum, strSQL, "screening_id")
    End Function

    '***************************************************************
    'Function to get a Line Number for a new IBIS Study Participant
    '***************************************************************
    Public Function GetIBISEnrollmentLineNum(hhid As String, tabletnum As Integer) As String
        Dim hhid_len As Integer = Len(hhid)
        Dim strSQL As String = "SELECT subjid FROM baseline WHERE left(subjid," & hhid_len & ") = '" & hhid & "' AND eligibility_check = 1"
        Return GetNextLineNum(tabletnum, strSQL, "subjid")
    End Function


    Public Function GetDBConnection() As OleDbConnection
        Try
            Dim config As Configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None)
            Dim section As ConnectionStringsSection = DirectCast(config.GetSection("connectionStrings"), ConnectionStringsSection)
            Return New OleDbConnection(section.ConnectionStrings("ConnString").ConnectionString)
        Catch ex As Exception
            MessageBox.Show("Error retrieving database connection: " & ex.Message)
            Return Nothing
        End Try
    End Function

    Public Function SetAppointmentDateByMonths(baseinterval As Integer, Optional isWeeks As Boolean = False) As String
        Dim result As String
        Try
            VDATE = GetValue("starttime")

            ' Try to parse with full date/time format
            Dim visit_date As DateTime

            ' First attempt with full format
            If DateTime.TryParseExact(VDATE, "dd/MM/yyyy HH:mm:ss",
                System.Globalization.CultureInfo.InvariantCulture,
                Globalization.DateTimeStyles.None, visit_date) Then
                ' Success with full format

                ' Second attempt with date only format
            ElseIf DateTime.TryParseExact(VDATE, "dd/MM/yyyy",
                    System.Globalization.CultureInfo.InvariantCulture,
                    Globalization.DateTimeStyles.None, visit_date) Then
                ' Success with date only format

                ' Third attempt with generic parsing (culture dependent)
            ElseIf DateTime.TryParse(VDATE, visit_date) Then
                ' Success with generic parsing

            Else
                ' If all parsing attempts fail
                ' Log the error
                Console.WriteLine("Could not parse date: " & VDATE)
                ' Set a default value or throw exception based on your requirements
                visit_date = DateTime.Now ' Default to current date/time
            End If

            ' Add 3 months
            Dim newDate As DateTime

            If isWeeks Then
                newDate = visit_date.AddDays(baseinterval * 7) ' Add weeks
            Else
                newDate = visit_date.AddMonths(baseinterval) ' Add months
            End If

            ' Convert back to string in the desired format
            result = newDate.ToString("dd/MM/yyyy")

        Catch ex As Exception
            ' Handle any other exceptions
            ' Set a default value or rethrow based on requirements
            result = DateTime.Now.AddMonths(baseinterval).ToString("dd/MM/yyyy")
            ' Alternatively: Throw
        End Try

        Return result

    End Function
End Module
