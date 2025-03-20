Imports System.Configuration
Imports System.Data.OleDb

Module IBIS_Public

    Public SW_VER As String = ConfigurationManager.AppSettings("Version")

    Public Survey As String                                 'This is used to keep which survey we are doing - add household or household members
    Public SUBJID As String                                 'used to store the SUBJID
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
    Public Function GetNextRandArm(clinic_code As Integer) As String
        Dim nextLineNum As String = "-9"
        Try
            Dim config As Configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None)
            Dim section As ConnectionStringsSection = DirectCast(config.GetSection("connectionStrings"), ConnectionStringsSection)
            Dim ConnectionString As New OleDbConnection(section.ConnectionStrings("ConnString").ConnectionString)
            ConnectionString.Open()
            Dim strSQL As String = "Select arm from baseline where eligibility_check = 1 and health_facility = " & clinic_code & " order by starttime desc;"

            Dim da As New OleDbDataAdapter(strSQL, ConnectionString)
            Dim ds As New DataSet
            da.Fill(ds)
            Dim dt As DataTable = ds.Tables(0)
            ConnectionString.Close()

            ' Initialize MaxLinenum to start with the tabletnum prefix
            Dim MaxRandArm As Integer = 1

            For Each row As DataRow In dt.Rows
                If Not IsDBNull(row("arm")) Then
                    Dim arm As Integer = CInt(row("arm"))
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


End Module
