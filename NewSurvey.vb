Imports System.Xml
Imports System.Data.OleDb
Imports System.Configuration
Imports System.Runtime.InteropServices
Imports System.Globalization
'Imports AxWMPLib

Imports LibVLCSharp.Shared
Imports LibVLCSharp.WinForms


Public Class NewSurvey
    ReadOnly MaxResponses As Integer = 120   'Maximium number of responses to a question (for a radio button or checkbox)
    ReadOnly MaxOthers As Integer = 10       'Maximium number of 'other' values for a question

    'Structure to hold the question info during the survey - also used to get the values to write to the database after survey complete
    Structure QuestionInfo
        Dim QuesNum As Integer          'Question number from the xml file
        Dim FieldName As String         'Field name from the xml file
        Dim FieldType As String         'Field type from the xml file (string, integer, decimal, date, N/A - for Information screens)
        Dim QuesType As String          'Type of question [radio button (single response), checkbox (multiple response), or text (open text))
        Dim Response As String          'Text response.  All fields set to "-9" initially
        Dim Value As String             'used to store the numeric 'value' of responses.  Using a string to store multiple values. e.g. 1,3,5
        Dim PrevQues As Integer         'the previous question before this one - in case someone hits the 'Previous' button. Default = -9
        Dim HasBeenAnswered As Boolean  'has question been answered before?  If so display the previous answer
    End Structure


    'Array of QuestionInfo Structures
    Public QuestionInfoArray() As QuestionInfo      'Array of QuestionInfo structures to hold all the question information throughout the survey
    Public QuestionInfoArray_orig() As QuestionInfo 'Array of QuestionInfo structures to hold the original responses so we can compare to the new one

    'Total number of questions in the questionnaire
    Public NumQuestions As Integer         'This is the total number of questions in the questionnaire

    'Keeps track of the Current Question #
    Public CurrentQuestion As Integer

    'Previous Question (in case we have to go back)
    Dim PreviousQuestion As Integer

    'XML document which has all the questionnaire information
    Public xr As New Xml.XmlDocument

    'Declare a flag to track the image state()
    Private isSpinning As Boolean = False

    ' Counter for button clicks
    Private clickCount As Integer = 0

    'Add a media player
    'Private mediaPlayer As AxWindowsMediaPlayer

    'setting for the dynamic controls
    Private Const GroupBoxHeight As Integer = 300
    Private Const controlWidth As Integer = 760
    Private Const charPerLine As Integer = 81
    Private Const lineHeight As Integer = 19

    'settings for the Question area and the response area
    Private Const QuestionWidth As Integer = 880
    Private Const QuestionHieght As Integer = 180
    Private Const ResponseWidth As Integer = 770

    ' Set a location for the question area and response area
    Private QuestionLocation As New Point(15, 30)
    Private ResponseLocation As New Point(15, 250)

    'Used to keep track of checkboxes (multiple response)
    ReadOnly CheckBoxesValue As String

    'used as the background colour if a 'special button' is selected
    ReadOnly Selected_colour As Color = Color.PaleVioletRed

    ReadOnly DEFAULT_DATE As String = "01/01/1899"     'default date


    'this is to supress mouse clicks while the next question loads
    <StructLayout(LayoutKind.Sequential)>
    Private Structure NativeMessage
        Public handle As IntPtr
        Public msg As UInteger
        Public wParam As IntPtr
        Public lParam As IntPtr
        Public time As UInteger
        Public p As System.Drawing.Point
    End Structure
    Private Declare Auto Function PeekMessage Lib "user32.dll" (
        ByRef lpMsg As NativeMessage,
        ByVal hWnd As IntPtr,
        ByVal wMsgFilterMin As UInteger,
        ByVal wMsgFilterMax As UInteger,
        ByVal flags As UInteger
    ) As Boolean
    Private Const WM_MOUSEFIRST As UInteger = &H200
    Private Const WM_MOUSELAST As UInteger = &H20D
    Private Const PM_REMOVE As Integer = &H1

    ' Flush all pending mouse events.
    Private Sub FlushMouseMessages()
        Dim msg As NativeMessage
        ' Repeat until PeekMessage returns false.
        While (PeekMessage(msg, IntPtr.Zero, WM_MOUSEFIRST, WM_MOUSELAST, PM_REMOVE))
        End While
    End Sub




    ' Create a property for the dynamic controls
    Public ReadOnly Property SurveyFormControls() As Control.ControlCollection
        Get
            Return Me.Controls
        End Get
    End Property


    'Form loading........
    Private Sub NewSurvey_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            Dim ShowPreviousResponses As Boolean = False
            'initialize the array to hold the question information
            'if we are modifying an existing survey.....
            If ModifyingSurvey = True Then
                GetResponsesFromPreviousSurvey()
                ShowPreviousResponses = True
            Else    'new survey
                InitializeQuestionArray()
                ShowPreviousResponses = True
            End If

            'set Current and Previous question to 0 and create the first question
            PreviousQuestion = -1
            CurrentQuestion = 0
            QuestionInfoArray(CurrentQuestion).PrevQues = PreviousQuestion
            CreateQuestion(CurrentQuestion, ShowPreviousResponses)

            'Fro VLC
            Core.Initialize()
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub



    'Clicking the "Next" button
    Private Sub Button_Next_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button_Next.Click
        Try
            'check to make sure response has valid values
            If IsValidResponse() = True Then
                SaveDataToArray()
                ShowNextQuestion()
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub


    'Clicking the "Previous" button
    Private Sub Button_Previous_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button_Previous.Click

        Try
            PreviousQuestion = QuestionInfoArray(CurrentQuestion).PrevQues
            If QuestionInfoArray(PreviousQuestion).QuesType = "automatic" Then
                CurrentQuestion = QuestionInfoArray(PreviousQuestion).PrevQues 'go back another question
            Else
                CurrentQuestion = PreviousQuestion
            End If



            If CurrentQuestion = -1 Then
                CurrentQuestion = 0
            End If
            CreateQuestion(CurrentQuestion, QuestionInfoArray(CurrentQuestion).HasBeenAnswered)
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try

    End Sub


    'handles the click event for the "Not Applicable" button
    Private Sub Button_NA_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button_NA.Click
        Try
            'pause half a second before moving to next question
            System.Threading.Thread.Sleep(500)

            QuestionInfoArray(CurrentQuestion).Response = NA_Value
            QuestionInfoArray(CurrentQuestion).Value = NA_Value
            QuestionInfoArray(CurrentQuestion).HasBeenAnswered = True

            ShowNextQuestion()
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub


    'handles the click event for the "Refuse to answer" button
    Private Sub Button_Refuse_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button_Refuse.Click
        Try
            'pause half a second before moving to next question
            System.Threading.Thread.Sleep(500)

            QuestionInfoArray(CurrentQuestion).Response = Refuse_Value
            QuestionInfoArray(CurrentQuestion).Value = Refuse_Value
            QuestionInfoArray(CurrentQuestion).HasBeenAnswered = True

            ShowNextQuestion()
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub


    'handles the click event for the "Don't Know" button
    Private Sub Button_DK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button_DK.Click
        Try
            'pause half a second before moving to next question
            System.Threading.Thread.Sleep(500)

            QuestionInfoArray(CurrentQuestion).Response = DontKnow_Value
            QuestionInfoArray(CurrentQuestion).Value = DontKnow_Value
            QuestionInfoArray(CurrentQuestion).HasBeenAnswered = True

            ShowNextQuestion()
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub




    'this procedure initializes the array where we keep the question information
    'this is called when the form is loading or when we load a ne wsurvey to
    'add household members
    Private Sub InitializeQuestionArray()
        Try
            'select the proper xml file based on the language and the survey
            xr.LoadXml(My.Resources.ResourceManager.GetObject(Survey))

            'Get the total number of questions from the xml file
            NumQuestions = xr.GetElementsByTagName("question").Count

            'redimension the array with the number of questions
            ReDim QuestionInfoArray(NumQuestions - 1)

            'populate the array of questioninfo with question number, field name, question type, default value, etc
            Dim nodeList As Xml.XmlNodeList
            nodeList = xr.GetElementsByTagName("question")

            Dim i As Integer = 0
            Dim myNode As XmlNode
            For Each myNode In nodeList
                QuestionInfoArray(i).QuesNum = i
                QuestionInfoArray(i).FieldName = myNode.Attributes("fieldname").Value
                QuestionInfoArray(i).FieldType = myNode.Attributes("fieldtype").Value
                QuestionInfoArray(i).QuesType = myNode.Attributes("type").Value

                If QuestionInfoArray(i).FieldType = "date" Or QuestionInfoArray(i).FieldType = "datetime" Then
                    QuestionInfoArray(i).Response = DEFAULT_DATE
                    QuestionInfoArray(i).Value = DEFAULT_DATE
                Else
                    QuestionInfoArray(i).Response = "-9"
                    QuestionInfoArray(i).Value = "-9"
                End If

                QuestionInfoArray(i).PrevQues = -9
                QuestionInfoArray(i).HasBeenAnswered = False
                i += 1
            Next


            'set Current and Previous question to 0 and create the first question
            PreviousQuestion = -1
            CurrentQuestion = 0
            QuestionInfoArray(CurrentQuestion).PrevQues = PreviousQuestion
            CreateQuestion(CurrentQuestion, False)
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub





    'this procedure initializes the array where we keep the question information
    'this is called when the the survey information is going to be modified
    Private Sub GetResponsesFromPreviousSurvey()
        Try
            'select the proper xml file based on the language and the survey
            xr.LoadXml(My.Resources.ResourceManager.GetObject(Survey))

            'Get the total number of questions from the xml file
            NumQuestions = xr.GetElementsByTagName("question").Count

            'redimension the array with the number of questions
            ReDim QuestionInfoArray(NumQuestions - 1)
            ReDim QuestionInfoArray_orig(NumQuestions - 1)

            'populate the array of questioninfo with question number, field name, question type, default value, etc
            Dim nodeList As Xml.XmlNodeList
            nodeList = xr.GetElementsByTagName("question")

            Dim strSQL As String = "select top 1 "

            Dim i As Integer = 0
            Dim myNode As XmlNode
            For Each myNode In nodeList
                QuestionInfoArray(i).QuesNum = i
                QuestionInfoArray(i).FieldName = myNode.Attributes("fieldname").Value
                QuestionInfoArray(i).FieldType = myNode.Attributes("fieldtype").Value
                QuestionInfoArray(i).QuesType = myNode.Attributes("type").Value
                If QuestionInfoArray(i).FieldType = "date" Or QuestionInfoArray(i).FieldType = "datetime" Then
                    QuestionInfoArray(i).Response = DEFAULT_DATE
                    QuestionInfoArray(i).Value = DEFAULT_DATE
                Else
                    QuestionInfoArray(i).Response = "-9"
                    QuestionInfoArray(i).Value = "-9"
                End If
                QuestionInfoArray(i).PrevQues = -9
                QuestionInfoArray(i).HasBeenAnswered = False

                '...and build the SQL query to get data from the database
                If i < NumQuestions - 2 Then
                    If myNode.Attributes("fieldtype").Value <> "n/a" Then
                        strSQL += QuestionInfoArray(i).FieldName & ","
                    End If
                Else
                    If myNode.Attributes("fieldname").Value <> "end_of_questions" Then
                        strSQL += QuestionInfoArray(i).FieldName
                    End If
                End If

                i += 1
            Next

            Dim ConnectionString As New OleDbConnection(ConfigurationManager.ConnectionStrings("ConnString").ConnectionString)
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
            End If

            Select Case PrimaryKey
                Case "subjid"
                    strSQL += " FROM " & Survey & " WHERE subjid = '" & SUBJID & "'"
                Case "subjid,vdate"
                    strSQL += " FROM " & Survey & " WHERE subjid = '" & SUBJID & "' and vdate = #" & VDATE_US & "# order by vdate desc"
                Case "screening_id,vdate"
                    strSQL += " FROM " & Survey & " WHERE screening_id = '" & SUBJID & "'"
            End Select

            'get the data from the database
            Dim da As New OleDbDataAdapter(strSQL, ConnectionString)
            Dim ds As New DataSet
            da.Fill(ds)

            i = 0
            Dim NumQuestionsWithoutFields As Integer = 0

            Dim fieldname As String = ""
            'populate the QuestionInfoArray the current values
            For Each row As DataRow In ds.Tables(0).Rows
                For i = 0 To NumQuestions - 2
                    fieldname = QuestionInfoArray(i).FieldName

                    If QuestionInfoArray(i).FieldType = "n/a" Then
                        QuestionInfoArray(i).Value = "-9"
                        QuestionInfoArray(i).Response = "-9"
                        QuestionInfoArray(i).HasBeenAnswered = True
                        NumQuestionsWithoutFields += 1
                    Else
                        If Not IsDBNull(row(i - NumQuestionsWithoutFields)) AndAlso Not String.IsNullOrEmpty(row(i - NumQuestionsWithoutFields).ToString()) Then
                            QuestionInfoArray(i).Value = row(i - NumQuestionsWithoutFields).ToString()
                            QuestionInfoArray(i).Response = row(i - NumQuestionsWithoutFields).ToString()
                        End If
                        If row(i - NumQuestionsWithoutFields).ToString <> "-9" Then
                            QuestionInfoArray(i).HasBeenAnswered = True
                        End If
                        If row(i - NumQuestionsWithoutFields).ToString = DEFAULT_DATE & " 12:00:00 AM" Or row(i - NumQuestionsWithoutFields).ToString = DEFAULT_DATE & " 00:00:00" Then
                            QuestionInfoArray(i).HasBeenAnswered = False
                        End If
                    End If
                Next
            Next
            ConnectionString.Close()
            'copy the data from QuestionInfoArray to QuestionInfoArray_orig
            Array.Copy(QuestionInfoArray, QuestionInfoArray_orig, QuestionInfoArray.Length)

        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub






    Private Sub ShowNextQuestion()
        Try
            Set_Next_Prev_Question_Numbers()

            'Show next question if we are not at the end of the interview....
            If CurrentQuestion < NumQuestions Then
                CreateQuestion(CurrentQuestion, QuestionInfoArray(CurrentQuestion).HasBeenAnswered)
            Else
                'We have reached the end of the survey, so save the data
                SaveData(Survey)
                Me.DialogResult = System.Windows.Forms.DialogResult.OK
                Me.Close()
                Me.Dispose()
            End If

        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub



    '**********************************************************************************
    ' This subroutine is used to set the CurrentQuestion and PreviousQuestion values
    ' mainly to do with skip values, but also end of interview
    '**********************************************************************************
    Private Sub Set_Next_Prev_Question_Numbers()
        Try
            'check for skips and set next question
            Dim PostSkipConditionMet As Boolean = False
            Dim i As Integer

            'check for skips as long as we are not at the end of the survey or if it as automatic question
            If CurrentQuestion < NumQuestions - 1 Then

                'if no post skip condition was met or there are no skips for this question
                'then increment the question number by 1
                'otherwise if SkipConditionMet = True, then the SkipConditionMet routine will set CurrentQuestion 
                PostSkipConditionMet = CheckForSkip("postskip")

                If PostSkipConditionMet = False Then    'there is no skip after this question
                    'no need to set Previous question if it is 'automatic' field
                    If QuestionInfoArray(CurrentQuestion).QuesType <> "automatic" Then
                        PreviousQuestion = CurrentQuestion
                    End If
                    CurrentQuestion += 1
                    QuestionInfoArray(CurrentQuestion).PrevQues = PreviousQuestion
                Else                                    'there is a skip so make all skipped questions have a value of -9
                    If PreviousQuestion = -1 Then
                        PreviousQuestion = 0
                    End If
                    For i = PreviousQuestion + 1 To CurrentQuestion - 1
                        QuestionInfoArray(i).HasBeenAnswered = False
                        If QuestionInfoArray(i).FieldType = "date" Or QuestionInfoArray(i).FieldType = "datetime" Then
                            QuestionInfoArray(i).Response = DEFAULT_DATE
                            QuestionInfoArray(i).Value = DEFAULT_DATE
                        Else
                            QuestionInfoArray(i).Response = "-9"
                            QuestionInfoArray(i).Value = "-9"
                        End If
                        QuestionInfoArray(i).PrevQues = -9
                    Next
                End If

                'now that we have the next question - are there any conditions to skip it?
                'Preskips...
                'loop until there are no preskips in case we have several in a row.
                Dim PreSkipConditionMet As Boolean = True
                Do Until PreSkipConditionMet = False
                    PreSkipConditionMet = CheckForSkip("preskip")
                    If PreSkipConditionMet = True Then
                        'skipping some questions, so set QuestionInfoArray(CurrentQuestion).HasBeenanswered to False for those 
                        'questions that were skipped - also set the value to "-9"

                        For i = PreviousQuestion + 1 To CurrentQuestion - 1
                            If QuestionInfoArray(i).QuesType <> "automatic" Then
                                QuestionInfoArray(i).HasBeenAnswered = False
                                If QuestionInfoArray(i).FieldType = "date" Or QuestionInfoArray(i).FieldType = "datetime" Then
                                    QuestionInfoArray(i).Response = DEFAULT_DATE
                                    QuestionInfoArray(i).Value = DEFAULT_DATE
                                Else
                                    QuestionInfoArray(i).Response = "-9"
                                    QuestionInfoArray(i).Value = "-9"
                                End If
                                QuestionInfoArray(i).PrevQues = -9
                            End If
                        Next
                    End If
                Loop
            Else
                CurrentQuestion += 1
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub






    '**********************************************************************************
    ' This function checks for any skips and sets the CurrentQuestion if there are skips
    ' There are 2 types of skips:
    ' preskip: is checked immediately before the question is about to be displayed
    ' postskip: is checked after the response is captured
    '**********************************************************************************
    Private Function CheckForSkip(ByVal SkipType As String) As Boolean
        CheckForSkip = False
        Try
            Dim fieldname As String
            Dim condition As String
            Dim response As String
            Dim response_type As String
            Dim skiptofieldname As String


            Dim QuestionNode As XmlNode = xr.GetElementsByTagName("question").Item(CurrentQuestion)

            For Each SkipNode As XmlNode In QuestionNode.SelectNodes(SkipType)
                'This question has skips
                Dim i As Integer
                'loop through each of the skips - if 1 is met, do the skip
                'if it is not met, test the next condition (if one exists)
                'therefore skip conditions MUST be in order of priority
                For i = 0 To SkipNode.ChildNodes.Count - 1
                    'get skip condition and fieldname to skip to
                    fieldname = SkipNode.ChildNodes(i).Attributes("fieldname").Value
                    condition = SkipNode.ChildNodes(i).Attributes("condition").Value
                    response = SkipNode.ChildNodes(i).Attributes("response").Value
                    response_type = SkipNode.ChildNodes(i).Attributes("response_type").Value
                    skiptofieldname = SkipNode.ChildNodes(i).Attributes("skiptofieldname").Value

                    'Test is the skip condition is true
                    CheckForSkip = TestSkipCondition(fieldname, condition, response, response_type)

                    'If skip condition is true, set next question - then no need to check for the next skip condition
                    If CheckForSkip = True Then
                        'no need to set previous question if we are skipping this question (for a preskip)
                        If SkipType = "postskip" Then
                            PreviousQuestion = CurrentQuestion
                        End If
                        CurrentQuestion = GetQuestionNumber(skiptofieldname)
                        QuestionInfoArray(CurrentQuestion).PrevQues = PreviousQuestion
                        Exit Function
                    End If
                Next i
            Next
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Function



    '**********************************************************************************
    ' This subroutine is used to test the skip condition to 
    ' determine if it is True or False
    '**********************************************************************************
    Private Function TestSkipCondition(ByVal fieldname As String, ByVal condition As String, ByVal response As String, ByVal response_type As String) As Boolean
        TestSkipCondition = False
        Try
            Dim CurrentValue As String = "-9"
            Dim i As Integer

            Dim QuestionNumber As Integer
            Dim QuestionType As String = ""
            Dim FieldType As String = ""


            'get the CurrentValue stored in the QuestionInfoArray for the desired fieldname
            For i = 0 To NumQuestions - 1
                If QuestionInfoArray(i).FieldName = fieldname Then
                    QuestionNumber = QuestionInfoArray(i).QuesNum
                    CurrentValue = QuestionInfoArray(i).Value
                    QuestionType = QuestionInfoArray(i).QuesType
                    FieldType = QuestionInfoArray(i).FieldType
                    Exit For
                End If
            Next

            'if it's a numeric response....
            If IsNumeric(CurrentValue) = True Then
                If InStr(CurrentValue, ",") Then    'this contains comma's for multiple checkboxes (Isnumeric function returns true if the expression contains commas 
                    'this is used for multiple check boxes when the user selects more than option
                    TestSkipCondition = CheckMultipleResponses(CurrentValue, response, condition)
                Else
                    If response_type = "fixed" Then
                        Select Case condition
                            'Test the condition
                            Case Is = "="
                                If CInt(CurrentValue) = CInt(response) Then
                                    TestSkipCondition = True
                                End If
                            Case Is = "<"
                                If CInt(CurrentValue) < CInt(response) Then
                                    TestSkipCondition = True
                                End If

                            Case Is = ">"
                                If CInt(CurrentValue) > CInt(response) Then
                                    TestSkipCondition = True
                                End If
                            Case Is = ">="
                                If CInt(CurrentValue) >= CInt(response) Then
                                    TestSkipCondition = True
                                End If
                            Case Is = "<>", "does not contain"
                                'Case Is = "does not contain"    'this is used for multiple check boxes, but user only selected 1 option
                                If CInt(CurrentValue) <> CInt(response) Then
                                    TestSkipCondition = True
                                End If
                        End Select

                    ElseIf response_type = "dynamic" Then

                        Select Case condition
                            'Test the condition
                            Case Is = "="
                                If CInt(CurrentValue) = CInt(GetValue(response)) Then
                                    TestSkipCondition = True
                                End If
                            Case Is = "<"
                                If CInt(CurrentValue) < CInt(GetValue(response)) Then
                                    TestSkipCondition = True
                                End If

                            Case Is = ">"
                                If CInt(CurrentValue) > CInt(GetValue(response)) Then
                                    TestSkipCondition = True
                                End If

                            Case Is = "<>", "does not contain"
                                'Case Is = "does not contain"    'this is used for multiple check boxes, but user only selected 1 option
                                If CInt(CurrentValue) <> CInt(GetValue(response)) Then
                                    TestSkipCondition = True
                                End If
                        End Select


                    End If

                End If
            Else
                Select Case condition
                    'Test the condition
                    Case Is = "="
                        If CurrentValue = response Then
                            TestSkipCondition = True
                        End If
                    Case Is = "<"
                        If CurrentValue < response Then
                            TestSkipCondition = True
                        End If

                    Case Is = ">"
                        If CurrentValue > response Then
                            TestSkipCondition = True
                        End If

                    Case Is = "<>", "does not contain"
                        If CurrentValue <> response Then
                            TestSkipCondition = True
                        End If
                End Select
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Function






    'returns False if 
    Private Function CheckMultipleResponses(ByVal CurrentValue As String, ByVal response As Integer, ByVal condition As String) As Boolean
        CheckMultipleResponses = False

        Dim ResponseArray() As String
        ResponseArray = CurrentValue.Split(",")
        For i = 0 To ResponseArray.Length - 1
            Select Case condition
                Case Is = "does not contain"
                    CheckMultipleResponses = True
                    If ResponseArray(i) = response Then
                        CheckMultipleResponses = False
                        Exit For
                    End If
                Case Is = "contains"
                    CheckMultipleResponses = False
                    If ResponseArray(i) = response Then
                        CheckMultipleResponses = True
                        Exit For
                    End If
            End Select
        Next
    End Function





    'Save the survey data to the table
    Private Sub SaveData(ByVal tablename As String)
        Try
            Dim ConnectionString As New OleDbConnection(ConfigurationManager.ConnectionStrings("ConnString").ConnectionString)
            Dim i As Integer
            Dim strSaveDataToText As String = ""
            Dim strSQL As String = ""

            'check to see if we are modifying this household
            If ModifyingSurvey = True Then
                ModifyData(tablename)
            Else
                'build the SQL statement to insert the data
                strSQL = "INSERT INTO " & tablename & " ("

                For i = 0 To NumQuestions - 2   'use -2 because the last question must ALWAYS be an 'Information' screen
                    If QuestionInfoArray(i).FieldType <> "n/a" Then
                        strSQL += QuestionInfoArray(i).FieldName
                    End If

                    If i < (NumQuestions - 2) And QuestionInfoArray(i).FieldType <> "n/a" Then
                        strSQL += ", "
                    End If
                Next

                strSQL += ") values ("

                For i = 0 To NumQuestions - 2
                    Select Case QuestionInfoArray(i).FieldType
                        Case Is = "text", "datetime", "date", "text_id", "phone_num", "vl_text", "hourmin"
                            strSQL += "'" & QuestionInfoArray(i).Value.Replace("'", "''") & "'" 'escape the single quotes 
                            If i < (NumQuestions - 2) And QuestionInfoArray(i).FieldType <> "n/a" Then
                                strSQL += ", "
                            End If
                        Case Is = "integer", "text_integer", "text_decimal"
                            strSQL += QuestionInfoArray(i).Value
                            If i < (NumQuestions - 2) And QuestionInfoArray(i).FieldType <> "n/a" Then
                                strSQL += ", "
                            End If
                    End Select
                Next
                strSQL += ");"


                'Write to Text File
                strSaveDataToText = """" & Now() & """" & vbNewLine & strSQL

                Dim FILE_NAME As String = ConfigurationManager.AppSettings("BackupTextFile") & "\AddNew_" & Survey
                Dim objWriter As New System.IO.StreamWriter(FILE_NAME, True)
                objWriter.WriteLine(strSaveDataToText)
                objWriter.Close()


                'write to the database
                ConnectionString.Open()
                Dim cmd = New OleDbCommand(strSQL, ConnectionString)
                cmd.ExecuteNonQuery()
                cmd.Dispose()

                ConnectionString.Close()
                MsgBox("The data has been saved!")

                '' Load the evatar video
                '' Call the function to get the correct video
                'If GetValue("eligibility_check") = 1 And RandArmText <> "Default appointment" Then
                '    MsgBox("A short Video will be displayed shortly for this participant")

                '    Dim video_name As String = getRandVideo(GetValue("client_sex"), GetValue("respondants_age"), RandArmText, GetValue("preferred_language"))
                '    Dim videoPath As String = "C:\IBIS_pilot\rand_video\" & video_name
                '    Process.Start(New ProcessStartInfo(videoPath) With {.UseShellExecute = True})
                '    'Process.Start("wmplayer.exe", "/play C:\IBIS_pilot\rand_video\video1.mp4")
                'End If
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub

    Private Function getRandVideo(sex As Integer, age As Integer, randarmtext As String, pref_lang As Integer)
        Dim video_path As String = "-9"
        Try
            Dim config As Configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None)
            Dim section As ConnectionStringsSection = DirectCast(config.GetSection("connectionStrings"), ConnectionStringsSection)
            Dim ConnectionString As New OleDbConnection(section.ConnectionStrings("ConnString").ConnectionString)
            ConnectionString.Open()
            Dim strSQL As String = "Select video_name from videolistpath where arm = '" & randarmtext & "';"

            If randarmtext = "Social norms" Then
                Dim age_cat As Integer = 0
                If age <= 30 Then
                    age_cat = 1
                ElseIf age > 30 Then
                    age_cat = 2
                End If
                strSQL = "Select video_name from videolistpath where arm = '" & randarmtext & "' and sexcode =" & sex & " and agecategory =" & age_cat
            End If

            Dim da As New OleDbDataAdapter(strSQL, ConnectionString)
            Dim ds As New DataSet
            da.Fill(ds)
            Dim dt As DataTable = ds.Tables(0)
            ConnectionString.Close()


            For Each row As DataRow In dt.Rows
                If Not IsDBNull(row("video_name")) Then
                    ' Set the result
                    video_path = row("video_name")
                End If
            Next

            'TODO Updatr this section once we have the correct list of videos
            If pref_lang = 1 Then
                video_path &= ".mp4"
            ElseIf pref_lang = 2 Then
                video_path &= ".mp4"
            ElseIf pref_lang = 3 Then
                video_path &= ".mp4"
            Else
                video_path &= ".mp4"
            End If
            Return video_path
        Catch ex As Exception
            MsgBox(ex.Message)
            Return video_path  ' Return the default value in case of exception
        End Try

    End Function


    'modify the existing data in the household info table
    Private Sub ModifyData(ByVal tablename As String)
        Try
            'this section checks to see if any changes were made to the questionnaire
            Dim i As Integer
            Dim ChangesMade As Boolean = False
            'Compare the new responses with the old ones to see if any changes were made
            For i = 0 To QuestionInfoArray.GetUpperBound(0)
                If QuestionInfoArray(i).Value <> QuestionInfoArray_orig(i).Value And QuestionInfoArray(i).QuesType <> "date" Then
                    ChangesMade = True
                    Exit For
                    If QuestionInfoArray(i).QuesType = "date" And QuestionInfoArray(i).Value <> Microsoft.VisualBasic.Left(QuestionInfoArray_orig(i).Value, 10) Then
                        ChangesMade = True
                        Exit For
                    End If
                End If
            Next

            'if changes were made, record the changes
            If ChangesMade = True Then
                LeaveNote.ShowDialog()
                LeaveNote.Dispose()
            Else
                MsgBox("No changes were made to the data.")
                Exit Sub
            End If

            If CancelledLeaveNote = True Then
                If MessageBox.Show("S T O P!" & vbNewLine & vbNewLine & "Any changes you have made will not be saved!" & vbNewLine & vbNewLine &
            "Are you sure you want to close this form without saving the changes you have made?", "End Interview?", MessageBoxButtons.YesNo) = DialogResult.Yes Then
                    Me.Close()
                    Me.Dispose()
                    Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
                    Exit Sub
                End If
            Else
                ' Set lastmod variable to current time
                SetLastMod()
            End If

            Dim config As Configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None)
            Dim section As ConnectionStringsSection = DirectCast(config.GetSection("connectionStrings"), ConnectionStringsSection)
            Dim ConnectionString As New OleDbConnection(section.ConnectionStrings("ConnString").ConnectionString)
            ConnectionString.Open()

            Dim strSQL As String = ""
            Dim strSaveDataToText As String = ""
            Dim uniqueid As String = GetValue("uniqueid")

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
            End If


            '********************************************************************
            'Due to the nature of MS Access, an update query cannot contain more than 127 fields
            'so we need to break up the update statment into 2 parts.

            Dim FieldCounter As Integer = 0
            Dim MaxFields As Integer = 120

            'build the SQL statement to inser the data
            strSQL = "UPDATE " & tablename & " set "
            For i = 0 To NumQuestions - 2   'use -2 because the last question must ALWAYS be an 'Information' screen
                If QuestionInfoArray(i).FieldType <> "n/a" Then
                    strSQL += QuestionInfoArray(i).FieldName
                End If

                If i < (NumQuestions - 1) And QuestionInfoArray(i).FieldType <> "n/a" Then
                    strSQL += "="
                End If

                Select Case QuestionInfoArray(i).FieldType
                    Case Is = "text", "datetime", "date", "text_id", "phone_num", "vl_text", "hourmin", "datetime"
                        'strSQL += "'" & QuestionInfoArray(i).Value & "'"
                        strSQL += "'" & QuestionInfoArray(i).Value.Replace("'", "''") & "'" 'escape the single quotes 
                        If i < (NumQuestions - 2) And QuestionInfoArray(i).FieldType <> "n/a" And FieldCounter < MaxFields - 1 Then
                            strSQL += ", "
                        End If
                    Case Is = "integer", "text_integer", "text_decimal"
                        strSQL += QuestionInfoArray(i).Value
                        If i < (NumQuestions - 2) And QuestionInfoArray(i).FieldType <> "n/a" And FieldCounter < MaxFields - 1 Then
                            strSQL += ", "
                        End If

                End Select
                FieldCounter += 1

                If FieldCounter = MaxFields Then
                    Exit For
                End If
            Next

            If Len(uniqueid) = 0 Or uniqueid = "-9" Then
                ConnectionString.Close()
                MsgBox("Error Creating the Update Statement. Call the data team for guidance!")
                Exit Sub
            Else

                strSQL += " where uniqueid = '" & uniqueid & "'"
            End If


            'Write to Text File
            strSaveDataToText = """" & Now() & """" & vbNewLine & strSQL

            Dim FILE_NAME As String = ConfigurationManager.AppSettings("BackupTextFile") & "\Modify_" & Survey
            Dim objWriter As New System.IO.StreamWriter(FILE_NAME, True)
            objWriter.WriteLine(strSaveDataToText)
            objWriter.Close()

            'write to the database
            Dim cmd = New OleDbCommand(strSQL, ConnectionString)
            cmd.ExecuteNonQuery()
            cmd.Dispose()



            'now we have to create a query for the rest of the fields..........
            'only if there are more than 'MaxFields' fields
            If FieldCounter = MaxFields Then
                'build the SQL statement to inser the data
                strSQL = "UPDATE " & tablename & " set "
                For i = MaxFields To NumQuestions - 2   'use -2 because the last question must ALWAYS be an 'Information' screen
                    If QuestionInfoArray(i).FieldType <> "n/a" Then
                        strSQL += QuestionInfoArray(i).FieldName
                    End If

                    If i < (NumQuestions - 1) And QuestionInfoArray(i).FieldType <> "n/a" Then
                        strSQL += "="
                    End If

                    Select Case QuestionInfoArray(i).FieldType
                        Case Is = "text", "datetime", "date", "text_id", "phone_num", "vl_text", "hourmin", "datetime"
                            'strSQL += "'" & QuestionInfoArray(i).Value & "'"
                            strSQL += "'" & QuestionInfoArray(i).Value.Replace("'", "''") & "'" 'escape the single quotes 
                            If i < (NumQuestions - 2) And QuestionInfoArray(i).FieldType <> "n/a" Then
                                strSQL += ", "
                            End If
                        Case Is = "integer", "text_integer", "text_decimal"
                            strSQL += QuestionInfoArray(i).Value
                            If i < (NumQuestions - 2) And QuestionInfoArray(i).FieldType <> "n/a" Then
                                strSQL += ", "
                            End If
                    End Select
                Next

                'complete the SQL statement with the appropriate where clause
                'Select Case PrimaryKey
                '    Case "subjid"
                '        strSQL += " where subjid = '" & SUBJID & "'"
                '    Case "subjid,vdate", "opal_id,vdate"
                '        strSQL += " where subjid = '" & SUBJID & "' and vdate = #" & VDATE_US & "#"
                'End Select

                If Len(uniqueid) = 0 Or uniqueid = "-9" Then
                    ConnectionString.Close()
                    MsgBox("Error Creating the Update Statement. Call the data team for guidance!")
                    Exit Sub
                Else

                    strSQL += " where uniqueid = '" & uniqueid & "'"
                End If

                'Write to Text File
                strSaveDataToText = """" & Now() & """" & vbNewLine & strSQL

                FILE_NAME = ConfigurationManager.AppSettings("BackupTextFile") & "\Modify_" & Survey
                objWriter = New System.IO.StreamWriter(FILE_NAME, True)
                objWriter.WriteLine(strSaveDataToText)
                objWriter.Close()

                'write to the database
                Dim cmd2 = New OleDbCommand(strSQL, ConnectionString)
                cmd2.ExecuteNonQuery()
                cmd2.Dispose()
            End If

            ConnectionString.Close()
            MsgBox("The data hase been saved!")

        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub






    'This procedure checks to make sure the value the user entered is a valid response
    Private Function IsValidResponse() As Boolean
        IsValidResponse = True

        Try
            Dim minvalue As Integer
            Dim maxvalue As Integer
            Dim other_values As String
            Dim message As String

            Dim fieldname As String
            Dim condition As String
            Dim response As String
            Dim response_type As String
            Dim currentresponse As String

            Dim QuestionNode As XmlNode = xr.GetElementsByTagName("question").Item(CurrentQuestion)

            'numeric range and value check
            For Each ValueNode As XmlNode In QuestionNode.SelectNodes("numeric_check")
                'This question has numeric checks
                Dim i As Integer
                'get the min, max, and "other values"
                For i = 0 To ValueNode.ChildNodes.Count - 1
                    minvalue = ValueNode.ChildNodes(i).Attributes("minvalue").Value
                    maxvalue = ValueNode.ChildNodes(i).Attributes("maxvalue").Value
                    other_values = ValueNode.ChildNodes(i).Attributes("other_values").Value
                    message = ValueNode.ChildNodes(i).Attributes("message").Value

                    'Test if the response is within the numeric range allowed or other possible values
                    IsValidResponse = TestNumericCheckOK(minvalue, maxvalue, other_values, message)
                    If IsValidResponse = False Then
                        Exit Function
                    End If
                Next i
            Next



            '*****************************************************************************
            '*****************************************************************************
            '
            '*****************************************************************************
            '*****************************************************************************
            'logic check
            If IsValidResponse = True Then
                For Each ValueNode As XmlNode In QuestionNode.SelectNodes("logic_check")
                    'This question has numeric checks
                    Dim i As Integer
                    'get the logic
                    For i = 0 To ValueNode.ChildNodes.Count - 1

                        fieldname = ValueNode.ChildNodes(i).Attributes("fieldname").Value
                        condition = ValueNode.ChildNodes(i).Attributes("condition").Value
                        response = ValueNode.ChildNodes(i).Attributes("response").Value
                        response_type = ValueNode.ChildNodes(i).Attributes("response_type").Value
                        currentresponse = ValueNode.ChildNodes(i).Attributes("currentresponse").Value
                        message = ValueNode.ChildNodes(i).Attributes("message").Value

                        IsValidResponse = TesLogicCheckOK(fieldname, condition, response, response_type, currentresponse, message)
                        If IsValidResponse = False Then
                            Exit Function
                        End If
                    Next i
                Next
            End If


            'Some manual checks.............
            'these will only be checked if the previous checks are all OK
            Dim aControl As Control
            Dim CurrentValue As String = ""


            'test to see if it is a valid SUBJID (not a duplicate)
            If IsValidResponse = True And ModifyingSurvey = False Then
                If QuestionInfoArray(CurrentQuestion).FieldName = "hhnum" And Survey = "households" Then


                    'Get the current value of hnum from the textbox
                    For Each aControl In Me.Controls
                        Select Case TypeName(aControl)
                            Case "TextBox"
                                CurrentValue = aControl.Text
                        End Select
                    Next

                    Select Case Len(CurrentValue)
                        Case Is = 1
                            SUBJID = "SNT" & CInt(GetValue("village")) & CInt(GetValue("ranum")) & "00" & CurrentValue
                        Case Is = 2
                            SUBJID = "SNT" & CInt(GetValue("village")) & CInt(GetValue("ranum")) & "0" & CurrentValue
                        Case Is = 3
                            SUBJID = "SNT" & CInt(GetValue("village")) & CInt(GetValue("ranum")) & CurrentValue
                    End Select

                    'check to see if this SUBJID is already in the database
                    Dim strSQL As String = "select subjid from " & Survey & " where subjid = '" & SUBJID & "'"
                    Dim ConnectionString As New OleDbConnection(ConfigurationManager.ConnectionStrings("ConnString").ConnectionString)

                    Dim da As New OleDbDataAdapter(strSQL, ConnectionString)
                    Dim ds As New DataSet
                    da.Fill(ds)

                    If ds.Tables(0).Rows.Count > 0 Then
                        IsValidResponse = False
                        MsgBox("This household ID has already been entered!.", vbCritical, "Invalid SUBJID")
                    End If

                    da.Dispose()
                    ds.Dispose()
                    ConnectionString.Close()
                End If
            End If



            ''test to see if phonenumber is duplicate
            If IsValidResponse = True Then
                If QuestionInfoArray(CurrentQuestion).FieldName = "mobile_number" And ModifyingSurvey = False Then
                    'Get the current value from the textbox
                    For Each aControl In Me.Controls
                        Select Case TypeName(aControl)
                            Case "TextBox"
                                CurrentValue = aControl.Text
                        End Select
                    Next

                    Dim strSQL As String = "select mobile_number from " & Survey & " where mobile_number = '" & CurrentValue & "'"
                    Dim ConnectionString As New OleDbConnection(ConfigurationManager.ConnectionStrings("ConnString").ConnectionString)
                    Dim da As New OleDbDataAdapter(strSQL, ConnectionString)
                    Dim ds As New DataSet
                    da.Fill(ds)
                    If ds.Tables(0).Rows.Count > 0 Then
                        IsValidResponse = False
                        MsgBox("This mobile phone number has already been assigned to another participant.", vbCritical, "Duplicate Phone number!")
                        Exit Function
                    End If
                    ConnectionString.Close()
                End If
            End If



            'test to see if relation is Head of the Household when line unm = 1 and vice versa
            If IsValidResponse = True Then
                If QuestionInfoArray(CurrentQuestion).FieldName = "relation" Then
                    For Each aControl In Me.Controls
                        Select Case TypeName(aControl)
                            Case "GroupBox"
                                For Each aGroupControl In CType(aControl, GroupBox).Controls
                                    'Radio buttons
                                    If TypeOf aGroupControl Is RadioButton Then
                                        If CType(aGroupControl, RadioButton).Checked Then
                                            If aGroupControl.tag <> "0" Then
                                                IsValidResponse = False
                                                MsgBox("You must select 'Head of the Household' as Relationship!", vbCritical, "Invalid selection")
                                            End If
                                        End If
                                    End If
                                Next
                        End Select
                    Next
                ElseIf QuestionInfoArray(CurrentQuestion).FieldName = "relation" Then
                    For Each aControl In Me.Controls
                        Select Case TypeName(aControl)
                            Case "GroupBox"
                                For Each aGroupControl In CType(aControl, GroupBox).Controls
                                    'Radio buttons
                                    If TypeOf aGroupControl Is RadioButton Then
                                        If CType(aGroupControl, RadioButton).Checked Then
                                            If aGroupControl.tag = "0" Then
                                                IsValidResponse = False
                                                MsgBox("You cannot select 'Head of the Household' as Relationship!", vbCritical, "Invalid selection")
                                            End If
                                        End If
                                    End If
                                Next
                        End Select
                    Next
                End If
            End If




            'test to see if parent's age is less than childs age
            If IsValidResponse = True Then
                If QuestionInfoArray(CurrentQuestion).FieldName = "age_parent" Then
                    Dim age As Integer = CInt(GetValue("age"))
                    If CInt(CurrentAutoValue) <= age Then
                        IsValidResponse = False
                        MsgBox("The parent's Date of Birth cannot result in an age that is less than the child, which is recorded as " & age, vbCritical, "Invalid DOB")
                        '....and show the previous question
                        Button_Previous_Click(Nothing, Nothing)
                    End If
                End If
            End If

            ' ensure dob is not in the future
            If IsValidResponse = True Then
                If QuestionInfoArray(CurrentQuestion).FieldName = "dobday" Then
                    For Each aControl In Me.Controls
                        Select Case TypeName(aControl)
                            Case "GroupBox"
                                For Each aGroupControl In CType(aControl, GroupBox).Controls
                                    'Radio buttons
                                    If TypeOf aGroupControl Is RadioButton Then
                                        If CType(aGroupControl, RadioButton).Checked Then
                                            CurrentValue = aGroupControl.tag
                                        End If
                                    End If
                                Next
                        End Select
                    Next

                    Dim date2 As Date = Date.Parse(CurrentValue & "/" & GetValue("dobmonth") & "/" & GetValue("dobyear"))
                    Dim date1 As Date = Now

                    ' Determine the number of days between the two dates.
                    Dim span As TimeSpan = date2.Subtract(date1)
                    Dim numHours As Integer = span.TotalHours()

                    If numHours > 0 Then
                        IsValidResponse = False
                        MsgBox("Household member cannot be born in the future! Please revise the year/month/day born", vbCritical, "Invalid DOB")
                    End If
                End If
            End If


            ' ensure age is > 15 yrs
            If IsValidResponse = True Then
                If QuestionInfoArray(CurrentQuestion).FieldName = "age_check" Then
                    For Each aControl In Me.Controls
                        Select Case TypeName(aControl)
                            Case "GroupBox"
                                For Each aGroupControl In CType(aControl, GroupBox).Controls
                                    'Radio buttons
                                    If TypeOf aGroupControl Is RadioButton Then
                                        If CType(aGroupControl, RadioButton).Checked Then

                                            CurrentValue = aGroupControl.tag
                                        End If
                                    End If
                                Next
                        End Select
                    Next


                    Dim age As Integer = CInt(GetValue("respondants_age"))

                    If age >= 15 And CurrentValue = 2 Then
                        IsValidResponse = False
                        MsgBox("The Participant's age is greater than 15 years, kindly select the correct response", vbCritical, "Invalid Response")
                    ElseIf age < 15 And CurrentValue = 1 Then
                        IsValidResponse = False
                        MsgBox("The Participant's age is less than 15 years, kindly select the correct response", vbCritical, "Invalid Response")
                    End If
                End If
            End If


            ' ensure correct health facility is selected
            If IsValidResponse = True Then
                If QuestionInfoArray(CurrentQuestion).FieldName = "health_facility_ke" Or QuestionInfoArray(CurrentQuestion).FieldName = "health_facility_ug" Then
                    For Each aControl In Me.Controls
                        Select Case TypeName(aControl)
                            Case "GroupBox"
                                For Each aGroupControl In CType(aControl, GroupBox).Controls
                                    'Radio buttons
                                    If TypeOf aGroupControl Is RadioButton Then
                                        If CType(aGroupControl, RadioButton).Checked Then

                                            CurrentValue = aGroupControl.tag
                                        End If
                                    End If
                                Next
                        End Select
                    Next


                    Dim country As Integer = CInt(GetValue("countrycode"))

                    If country = 1 And CurrentValue > 14 And CurrentValue <> 99 Then
                        IsValidResponse = False
                        MsgBox("Select the correct Health Facility", vbCritical, "Invalid Response")
                    ElseIf country = 2 And CurrentValue < 21 And CurrentValue <> 99 Then
                        IsValidResponse = False
                        MsgBox("Select the correct Health Facility", vbCritical, "Invalid Response")
                    End If
                End If
            End If


            ' ensure correct health facility is selected
            If IsValidResponse = True Then
                If QuestionInfoArray(CurrentQuestion).FieldName = "preferred_language_ke" Or QuestionInfoArray(CurrentQuestion).FieldName = "preferred_language_ug" Then
                    For Each aControl In Me.Controls
                        Select Case TypeName(aControl)
                            Case "GroupBox"
                                For Each aGroupControl In CType(aControl, GroupBox).Controls
                                    'Radio buttons
                                    If TypeOf aGroupControl Is RadioButton Then
                                        If CType(aGroupControl, RadioButton).Checked Then

                                            CurrentValue = aGroupControl.tag
                                        End If
                                    End If
                                Next
                        End Select
                    Next


                    Dim country As Integer = CInt(GetValue("countrycode"))

                    If country = 1 And CurrentValue < 3 And CurrentValue <> 97 Then
                        IsValidResponse = False
                        MsgBox("Select the correct Preferred Language", vbCritical, "Invalid Response")
                    ElseIf country = 2 And CurrentValue > 3 And CurrentValue <> 97 Then
                        IsValidResponse = False
                        MsgBox("Select the correct Preferred Language", vbCritical, "Invalid Response")
                    End If
                End If
            End If


            'check to make sure 24 hour times are correct
            If IsValidResponse = True Then
                Select Case QuestionInfoArray(CurrentQuestion).FieldName
                    Case Is = "collection_time"
                        For Each aControl In Me.Controls
                            Select Case TypeName(aControl)
                                Case "TextBox"
                                    CurrentValue = aControl.Text
                            End Select
                        Next

                        'make sure it is 5 characters in length (although it should not be possible, since the 'Next' button is not enabled until user enters 5th character)
                        If Len(CurrentValue) <> 5 Then
                            IsValidResponse = False
                            MsgBox("Invalid time format.", vbCritical, "Invalid time format")
                            Exit Function
                        End If

                        'make sure there is only 1 ":"
                        If CurrentValue.Split(":").Length - 1 <> 1 Then
                            IsValidResponse = False
                            MsgBox("Invalid time format.", vbCritical, "Invalid time format")
                            Exit Function
                        End If

                        'make sure 3rd character is ":"
                        Dim thirdchar As String = Microsoft.VisualBasic.Right(Microsoft.VisualBasic.Left(CurrentValue, 3), 1)
                        If thirdchar <> ":" Then
                            IsValidResponse = False
                            MsgBox("Invalid time format.", vbCritical, "Invalid time format")
                            Exit Function
                        End If

                        'make sure the first 2 characters are between 0 and 23
                        Dim hr As Integer = CInt(Microsoft.VisualBasic.Left(CurrentValue, 2))
                        If hr < 0 Or hr > 23 Then
                            IsValidResponse = False
                            MsgBox("Invalid time format.", vbCritical, "Invalid time format")
                            Exit Function
                        End If

                        'make sure the last 2 characters are between 0 and 59
                        Dim mn As Integer = CInt(Microsoft.VisualBasic.Right(CurrentValue, 2))
                        If mn < 0 Or mn > 59 Then
                            IsValidResponse = False
                            MsgBox("Invalid time format.", vbCritical, "Invalid time format")
                            Exit Function
                        End If
                End Select
            End If


            ' ensure age is > 15 yrs
            If IsValidResponse = True Then
                If QuestionInfoArray(CurrentQuestion).FieldName = "eligibility_check" Then
                    For Each aControl In Me.Controls
                        Select Case TypeName(aControl)
                            Case "GroupBox"
                                For Each aGroupControl In CType(aControl, GroupBox).Controls
                                    'Radio buttons
                                    If TypeOf aGroupControl Is RadioButton Then
                                        If CType(aGroupControl, RadioButton).Checked Then

                                            CurrentValue = aGroupControl.tag
                                        End If
                                    End If
                                Next
                        End Select
                    Next


                    Dim eligibility_check2 As Integer = CInt(GetValue("eligibility_check2"))

                    If eligibility_check2 = 1 And CurrentValue = 3 Then
                        IsValidResponse = False
                        MsgBox("This Participant is eligible for enrolment, has he/she declined enrollment?, If yes, select the correct response", vbCritical, "Invalid Response")
                    ElseIf eligibility_check2 <> 1 And CurrentValue = 1 Then
                        IsValidResponse = False
                        MsgBox("The Participant is not eligible for enrollment, kindly double check your previous responses", vbCritical, "Invalid Response")
                    End If
                End If
            End If


            'check for duplicate barcode
            If IsValidResponse = True And ModifyingSurvey = False Then
                Select Case QuestionInfoArray(CurrentQuestion).FieldName
                    Case = "barcode"
                        For Each aControl In Me.Controls
                            Select Case TypeName(aControl)
                                Case "TextBox"
                                    CurrentValue = aControl.Text
                            End Select
                        Next
                        Dim strSQL As String = "select barcode from " & Survey & " where barcode = " & CInt(CurrentValue)
                        Dim ConnectionString As New OleDbConnection(ConfigurationManager.ConnectionStrings("ConnString").ConnectionString)
                        Dim da As New OleDbDataAdapter(strSQL, ConnectionString)
                        Dim ds As New DataSet
                        da.Fill(ds)
                        If ds.Tables(0).Rows.Count > 0 Then
                            IsValidResponse = False
                            MsgBox("This barcode has already been assigned to another participant.", vbCritical, "Duplicate Barcode!")
                            Exit Function
                        End If
                        ConnectionString.Close()
                End Select
            End If




            'test to see if the multiple selection questions have valid responses
            If IsValidResponse = True Then
                Dim surveyResponseValue As String = ""
                Select Case QuestionInfoArray(CurrentQuestion).FieldName
                    Case = "fuyear_moveout", "fuyear_death", "covid_place", "hh_mobility", "hh_smoke", "hh_alcohol"
                        For Each aControl In Me.Controls
                            Select Case TypeName(aControl)
                                Case "GroupBox"
                                    For Each aGroupControl In CType(aControl, GroupBox).Controls
                                        If TypeOf aGroupControl Is CheckBox Then
                                            If CType(aGroupControl, CheckBox).Checked Then
                                                If surveyResponseValue <> "" Then
                                                    surveyResponseValue = surveyResponseValue & "," & aGroupControl.Tag
                                                Else
                                                    surveyResponseValue = aGroupControl.Tag
                                                End If
                                            End If
                                        End If
                                    Next
                            End Select
                        Next

                        If InStr(surveyResponseValue, 99) And Len(surveyResponseValue) <> 2 Then
                            If QuestionInfoArray(CurrentQuestion).FieldName = "fuyear_moveout" Then
                                IsValidResponse = False
                                MsgBox("You cannot select 'Nobody moved out' and some household members as well!", vbCritical, "Invalid Selection")
                                Exit Function
                            ElseIf QuestionInfoArray(CurrentQuestion).FieldName = "fuyear_death" Then
                                IsValidResponse = False
                                MsgBox("You cannot select 'Nobody died' and some household members as well!", vbCritical, "Invalid Selection")
                                Exit Function
                            ElseIf QuestionInfoArray(CurrentQuestion).FieldName = "hh_mobility" Then
                                IsValidResponse = False
                                MsgBox("You cannot select 'Nobody spent more than 1 night outside of the household' and some household members as well!", vbCritical, "Invalid Selection")
                                Exit Function
                            ElseIf QuestionInfoArray(CurrentQuestion).FieldName = "hh_smoke" Then
                                IsValidResponse = False
                                MsgBox("You cannot select 'Nobody smoked tobacco' and some household members as well!", vbCritical, "Invalid Selection")
                                Exit Function
                            ElseIf QuestionInfoArray(CurrentQuestion).FieldName = "hh_alcohol" Then
                                IsValidResponse = False
                                MsgBox("You cannot select 'Nobody drank alcohol' and some household members as well!", vbCritical, "Invalid Selection")
                                Exit Function
                            ElseIf QuestionInfoArray(CurrentQuestion).FieldName = "covid_place" Then
                                IsValidResponse = False
                                MsgBox("You cannot select 'Did not stop visiting any places' and places as well!", vbCritical, "Invalid Selection")
                                Exit Function
                            End If
                        End If
                End Select
            End If

        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Function




    '**********************************************************************************************************
    ' This Function is used to test if the value entered is within range or matches other acceptable values
    '**********************************************************************************************************
    Private Function TestNumericCheckOK(ByVal minvalue As String, ByVal maxvalue As String, ByVal other_values As String, ByVal message As String) As Boolean
        TestNumericCheckOK = True
        Try
            Dim aControl As Control
            Dim CurrentValue As String = "-9"

            Dim QuestionType As String = ""
            Dim FieldType As String = ""

            'Get the current value from the text box
            For Each aControl In Me.Controls
                Select Case TypeName(aControl)
                    Case "TextBox"
                        CurrentValue = aControl.Text
                End Select
            Next

            'if it's a numeric response....
            If IsNumeric(CurrentValue) = True Then
                'if the response is numeric, but is a text question e.g. age, numer of acres of land, etc.
                If CLng(CurrentValue) >= CLng(minvalue) And CLng(CurrentValue) <= CLng(maxvalue) Then
                    TestNumericCheckOK = True
                    Exit Function
                ElseIf TestOtherValues(CLng(CurrentValue), other_values) = True Then
                    TestNumericCheckOK = True
                Else
                    TestNumericCheckOK = False
                    MsgBox(message, MsgBoxStyle.Critical, "Invalid Response")
                End If
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Function




    '**********************************************************************************************************
    ' This Function is used to test if the value entered is an acceptable logic check
    '**********************************************************************************************************
    Private Function TesLogicCheckOK(ByVal fieldname As String, ByVal condition As String, ByVal response As String, ByVal response_type As String, ByVal currentresponse As String, ByVal message As String) As Boolean
        TesLogicCheckOK = True
        Try
            Dim aControl As Control
            Dim aGroupControl As Control
            Dim CurrentValue As String = "-9"
            Dim CurrentValueDate As Date = "1/1/1899"

            Dim QuestionType As String = ""
            Dim FieldType As String = ""

            'Get the current value from the text box
            For Each aControl In Me.Controls
                Select Case TypeName(aControl)
                    Case "TextBox"
                        CurrentValue = aControl.Text
                    Case "GroupBox"
                        For Each aGroupControl In CType(aControl, GroupBox).Controls
                            'Radio buttons
                            If TypeOf aGroupControl Is RadioButton Then
                                If CType(aGroupControl, RadioButton).Checked Then
                                    CurrentValue = aGroupControl.Tag
                                End If
                            End If
                        Next

                    Case "MonthCalendar"
                        'get value from calendar
                        If CurrentDateSelected = "" Then
                            CurrentDateSelected = QuestionInfoArray(CurrentQuestion).Value
                        End If

                        CurrentValueDate = CurrentDateSelected
                        Select Case response_type
                            Case Is = "fixed"

                            Case Is = "dynamic"

                                Dim ValueToCompare As Date = GetResponse(response)
                                Select Case condition
                                    Case Is = ">"
                                        If DateDiff(DateInterval.Day, DateTime.Parse(ValueToCompare), CurrentValueDate) > 0 Then
                                            TesLogicCheckOK = False
                                            MsgBox(message, MsgBoxStyle.Critical, "Invalid Response")
                                        End If
                                    Case Is = "<"
                                        If DateDiff(DateInterval.Day, DateTime.Parse(ValueToCompare), CurrentValueDate) < 0 Then
                                            TesLogicCheckOK = False
                                            MsgBox(message, MsgBoxStyle.Critical, "Invalid Response")
                                        End If
                                End Select
                        End Select


                        Exit Function
                End Select
            Next


            Dim QuestionNum As Integer = GetQuestionNumber(fieldname)
            'if it's a numeric response....


            If IsNumeric(CurrentValue) = True Then
                Dim FieldNameValue As String = QuestionInfoArray(QuestionNum).Value


                If response_type = "fixed" Then
                    Select Case condition
                        Case Is = "="
                            If CLng(FieldNameValue) = CLng(response) And CurrentValue = CLng(currentresponse) Then
                                TesLogicCheckOK = False
                                MsgBox(message, MsgBoxStyle.Critical, "Invalid Response")
                            End If
                        Case Is = "<>"
                            If CLng(FieldNameValue) <> CLng(response) And CurrentValue = CLng(currentresponse) Then
                                TesLogicCheckOK = False
                                MsgBox(message, MsgBoxStyle.Critical, "Invalid Response")
                            End If
                        Case Is = "<"
                            If CLng(FieldNameValue) < CLng(response) And CurrentValue = CLng(currentresponse) Then
                                TesLogicCheckOK = False
                                MsgBox(message, MsgBoxStyle.Critical, "Invalid Response")
                            End If
                        Case Is = ">"
                        Case Is = "<="
                        Case Is = ">="
                    End Select

                ElseIf response_type = "dynamic" Then
                    Select Case condition
                        Case Is = "="
                        Case Is = "<>"
                            If CLng(CurrentValue) <> CLng(GetValue(response)) Then
                                TesLogicCheckOK = False
                                MsgBox(message, MsgBoxStyle.Critical, "Invalid Response")
                            End If
                        Case Is = "<"
                            If CLng(CurrentValue) < CLng(GetValue(response)) Then
                                TesLogicCheckOK = False
                                MsgBox(message, MsgBoxStyle.Critical, "Invalid Response")
                            End If
                        Case Is = ">"
                            If CLng(CurrentValue) > CLng(GetValue(response)) Then
                                TesLogicCheckOK = False
                                MsgBox(message, MsgBoxStyle.Critical, "Invalid Response")
                            End If
                        Case Is = "<="
                        Case Is = ">="
                        Case Is = "does not contain"
                            TesLogicCheckOK = CheckMultipleResponses(GetValue(response), CurrentValue, condition)
                            If TesLogicCheckOK = True Then
                                TesLogicCheckOK = False
                                MsgBox(message, MsgBoxStyle.Critical, "Invalid Response")
                            Else
                                TesLogicCheckOK = True
                            End If
                    End Select
                End If
            Else
                Dim FieldNameValue As String = QuestionInfoArray(QuestionNum).Value

                If response_type = "fixed" Then
                    Select Case condition
                        Case Is = "="
                        Case Is = "<>"
                        Case Is = "<"
                        Case Is = ">"
                        Case Is = "<="
                        Case Is = ">="
                    End Select

                ElseIf response_type = "dynamic" Then
                    Select Case condition
                        Case Is = "="
                        Case Is = "<>"
                            If String.Compare(CurrentValue, GetValue(response)) <> 0 Then
                                TesLogicCheckOK = False
                                MsgBox(message, MsgBoxStyle.Critical, "Invalid Response")
                            End If
                        Case Is = "<"
                        Case Is = ">"
                        Case Is = "<="
                        Case Is = ">="
                        Case Is = "does not contain"
                    End Select
                End If
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Function



    '**********************************************************************************
    ' This Function is used to test if the value entered is within range
    '**********************************************************************************
    Private Function TestOtherValues(ByVal current_value As Long, ByVal other_values As String) As Boolean
        TestOtherValues = False

        Try
            Dim OtherValuesArray(MaxOthers) As String
            Dim i As Integer

            If other_values = "" Then
                TestOtherValues = True
            Else
                OtherValuesArray = Split(other_values, ",")
                For i = 0 To OtherValuesArray.Length - 1
                    If current_value = CInt(OtherValuesArray(i)) Then
                        TestOtherValues = True
                    End If
                Next
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Function







    '**********************************************************************************
    ' this subroutine is used to display the question and responses
    ' it uses the question number passed to it
    ' when ShowPreviousResponse = true, it means the user has hit the 'Previous'
    ' button and we should display the previous information the user input.  It also
    ' determines if we should disable the 'Next' button - which is always disabled
    ' until the user answers teh question
    '**********************************************************************************
    Private Sub CreateQuestion(ByVal question_num As Integer, ByVal ShowPreviousResponse As Boolean)
        Try
            ' Clear all the controls - to remove the dynamic controls created.
            Controls.Clear()

            ' Add all of the original controls again.
            InitializeComponent()

            ' Get the controls collection of the Survey form.
            Dim surveyControls As Control.ControlCollection = Me.SurveyFormControls

            ' Get all the question information from the xml file (<question> node)
            Dim myNode As XmlNode
            myNode = xr.GetElementsByTagName("question").Item(question_num)

            'disable the "Previous" button if we are at the beginning of the survey
            'disable the "Previous" button if we are at the beginning of the survey
            If question_num > 0 Then
                Button_Previous.Enabled = True
            End If
            If QuestionInfoArray(CurrentQuestion).FieldName = "ranum" Then
                Button_Previous.Enabled = False
            End If

            PictureBoxArm.Visible = False
            LabelArm.Visible = False
            ' Determine what "type" of control should be created based on the question-type attribute
            ' in the xml file and pass the information to create the appropriate control
            Select Case myNode.Attributes("type").Value
                Case "radio"
                    AddRadioButtons(myNode, surveyControls, ShowPreviousResponse)
                Case "checkbox"
                    AddCheckbox(myNode, surveyControls, ShowPreviousResponse)
                Case "text"
                    AddTextBox(myNode, surveyControls, ShowPreviousResponse)
                Case "date"
                    AddCalendar(myNode, surveyControls, ShowPreviousResponse)
                Case "button"
                    AddButton(myNode, surveyControls, ShowPreviousResponse)
                Case "information"
                    AddInformation(myNode, surveyControls)
                Case "automatic"
                    AddAutomatic(myNode.Attributes("fieldname").Value)
                    Button_Next_Click(Nothing, Nothing)
                    Exit Sub
            End Select


            'If Don't Know or N/A or Refuse to answer is applicable, enable the buttons and get the value if clicked
            Button_NA.Visible = False
            Button_Refuse.Visible = False
            Button_DK.Visible = False

            If myNode.SelectSingleNode("na") IsNot Nothing Then
                'NA_Value = Integer.Parse(myNode.SelectSingleNode("na").InnerText)
                NA_Value = myNode.SelectSingleNode("na").InnerText.ToString
                Button_NA.Visible = True

                If ShowPreviousResponse = True And QuestionInfoArray(CurrentQuestion).Value = myNode.SelectSingleNode("na").InnerText.ToString Then
                    Button_NA.BackColor = Selected_colour
                End If
            End If

            If myNode.SelectSingleNode("refuse") IsNot Nothing Then
                'Refuse_Value = Integer.Parse(myNode.SelectSingleNode("refuse").InnerText)
                Refuse_Value = myNode.SelectSingleNode("refuse").InnerText.ToString
                Button_Refuse.Visible = True
                If ShowPreviousResponse = True And QuestionInfoArray(CurrentQuestion).Value = myNode.SelectSingleNode("refuse").InnerText.ToString Then
                    Button_Refuse.BackColor = Selected_colour
                End If
            End If

            If myNode.SelectSingleNode("dont_know") IsNot Nothing Then
                DontKnow_Value = myNode.SelectSingleNode("dont_know").InnerText.ToString
                Button_DK.Visible = True

                If ShowPreviousResponse = True And QuestionInfoArray(CurrentQuestion).Value = myNode.SelectSingleNode("dont_know").InnerText.ToString Then
                    Button_DK.BackColor = Selected_colour
                End If
            End If

            'show the form again with the new controls
            Show()



        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub


    '**********************************************************************************
    ' This sub adds a Button to the response
    ' with the text for the survey question.
    '**********************************************************************************
    Public Sub AddButton(ByVal inNode As XmlNode,
    ByVal inControls As Control.ControlCollection,
    ByVal ShowPreviousResponse As Boolean)


        Try

            If QuestionInfoArray(CurrentQuestion).FieldName = "arm_text_demo" Then
                PictureBoxArm.Visible = True
                ' Create a new control.
                ' Set up some properties
                Dim newButton As New Button With {
                .Text = "Click here to Spin Randomization Wheel",
                .Name = inNode.Attributes("fieldname").Value,
                .Width = 300,
                .Height = 60
            }

                'Put the question text in the label
                lblQuestion.Text = "Click the button to Spin Randomization Wheel" & vbNewLine & "Note - it only works once!"


                'Add the Textbox to the form
                newButton.Location = New Point(150, 400)
                newButton.Font = New Font("Arial", 14)
                inControls.Add(newButton)
                newButton.Focus()

                'Not showing gif
                isSpinning = False

                'Add handlers for keyup event event so we can enable the "Next" button
                AddHandler newButton.Click, AddressOf ButtonHandlerClick

                If ShowPreviousResponse = True Then


                    Button_Next.Enabled = True
                Else

                    Button_Next.Enabled = False
                End If
            End If



        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub


    '**********************************************************************************
    ' Event handler for Button click event
    '**********************************************************************************
    Private Sub ButtonHandlerClick(ByVal sender As Object, ByVal e As EventArgs)

        Dim normalImage As Image = My.Resources.lucky_spin ' Change to your actual resource/image
        Dim spinningImage As Image = My.Resources.spin_12315_128 ' Change to your actual resource/image


        Try
            ' Verify that the type of control triggering this event is a button
            If TypeOf sender Is Button Then
                ' Toggle flag
                isSpinning = Not isSpinning
                clickCount += 1

                ' Update PictureBox image based on the flag
                If isSpinning Then
                    PictureBoxArm.Image = spinningImage
                    DirectCast(sender, Button).Text = "Stop"
                    Button_Next.Enabled = False
                Else
                    PictureBoxArm.Image = normalImage
                    DirectCast(sender, Button).Text = "Click the button to Spin Randomization Wheel"
                    Button_Next.Enabled = True
                    If clickCount > 1 Then
                        LabelArm.Visible = True
                        LabelArm.Text = "Randomization Arm: " & RandArmText
                    End If
                End If

                ' Enable the next button
                'Button_Next.Enabled = True
                'Button_Next_Click(Nothing, Nothing)
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try




    End Sub


    '**********************************************************************************
    ' This sub adds a GroupBox and radio buttons to the response area along
    ' with the text for the survey question.
    '**********************************************************************************
    Public Sub AddRadioButtons(ByVal inNode As XmlNode,
        ByVal inControls As Control.ControlCollection,
        ByVal ShowPreviousResponse As Boolean)

        Try
            'Array to hold text of options
            'this is used so that we can store parishes and villages
            'based on previous responses
            Dim ResponseArray(MaxResponses, 1) As String
            Dim ResponseArraySize As Integer = 0   'number of responses
            For Each node As XmlNode In inNode.SelectNodes("responses/response")
                Dim strSQL As String = ""
                Select Case QuestionInfoArray(CurrentQuestion).FieldName

                    Case Is = "clinicname"
                        Dim ConnectionString As New OleDbConnection(ConfigurationManager.ConnectionStrings("ConnString").ConnectionString)

                        strSQL = "Select clinicname, cliniccode from clinic_names"
                        Dim da As New OleDbDataAdapter(strSQL, ConnectionString)
                        Dim ds As New DataSet
                        da.Fill(ds)
                        For Each row As DataRow In ds.Tables(0).Rows
                            ResponseArray(ResponseArraySize, 0) = row(0)
                            ResponseArray(ResponseArraySize, 1) = row(1)
                            ResponseArraySize += 1
                        Next
                        da.Dispose()
                        ds.Dispose()
                    Case Is = "community_non_census"
                        Dim ConnectionString As New OleDbConnection(ConfigurationManager.ConnectionStrings("ConnString").ConnectionString)
                        strSQL = "Select distinct communityname, community_code from villages"
                        Dim da As New OleDbDataAdapter(strSQL, ConnectionString)
                        Dim ds As New DataSet
                        da.Fill(ds)
                        For Each row As DataRow In ds.Tables(0).Rows
                            ResponseArray(ResponseArraySize, 0) = row(0)
                            ResponseArray(ResponseArraySize, 1) = row(1)
                            ResponseArraySize += 1
                        Next
                        da.Dispose()
                        ds.Dispose()
                    Case "village_non_census"
                        Dim ConnectionString As New OleDbConnection(ConfigurationManager.ConnectionStrings("ConnString").ConnectionString)
                        strSQL = "Select villagename, village_code from villages where community_code = " & CInt(GetValue("community_non_census"))
                        Dim da As New OleDbDataAdapter(strSQL, ConnectionString)
                        Dim ds As New DataSet
                        da.Fill(ds)
                        For Each row As DataRow In ds.Tables(0).Rows
                            ResponseArray(ResponseArraySize, 0) = row(0)
                            ResponseArray(ResponseArraySize, 1) = row(1)
                            ResponseArraySize += 1
                        Next
                        da.Dispose()
                        ds.Dispose()

                    Case Else
                        ResponseArray(ResponseArraySize, 0) = node.InnerText
                        ResponseArray(ResponseArraySize, 1) = node.Attributes("value").Value
                        ResponseArraySize += 1
                End Select
            Next


            'used to make sure the previous response is valid
            'mainly in case the user changes the SUBJID and makes village/sub, etc incalid
            Dim FoundPrevResponse As Boolean = False

            ' Create a GroupBox to contain the radio buttons
            ' Set up some properties
            Dim newGroupBox As New GroupBox With {
                .Text = "",
                .Name = inNode.Attributes("fieldname").Value,
                .Tag = Tag,
                .Width = ResponseWidth
            }

            ' Create some variables to use in the following block of code to keep track of location of radio buttons
            Dim newRadio As RadioButton
            Dim radioPoint As New Point(5, 10)
            Dim NumResponses As Integer = 0

            ' Loop through each response, and add it as a new radio button.

            Dim RadioWidth As Integer = 270
            If QuestionInfoArray(CurrentQuestion).FieldName = "dobday" Or QuestionInfoArray(CurrentQuestion).FieldName = "datehivtestday" Or QuestionInfoArray(CurrentQuestion).FieldName = "cgiver_datehivtestday" Then
                RadioWidth = 100
            End If

            For NumResponses = 0 To ResponseArraySize - 1
                'add maximum of 8 then start a new column
                If NumResponses = 12 Then
                    radioPoint.X += RadioWidth
                    radioPoint.Y = 10
                    'next column.....
                ElseIf NumResponses = 24 Then
                    radioPoint.X += RadioWidth
                    radioPoint.Y = 10
                ElseIf NumResponses = 36 Then
                    radioPoint.X += RadioWidth
                    radioPoint.Y = 10
                ElseIf NumResponses = 48 Then
                    radioPoint.X += RadioWidth
                    radioPoint.Y = 10
                ElseIf NumResponses = 60 Then
                    radioPoint.X += RadioWidth
                    radioPoint.Y = 10
                End If

                newRadio = New RadioButton With {
                    .Location = radioPoint,
                    .AutoSize = True
                }
                radioPoint.Y += newRadio.Height
                newRadio.Text = ResponseArray(NumResponses, 0).ToString
                newRadio.Tag = ResponseArray(NumResponses, 1).ToString   'stores the numeric 'Value' of the response



                'if showing the question again, show previous response
                If ShowPreviousResponse = True And newRadio.Tag.ToString = QuestionInfoArray(CurrentQuestion).Value.ToString Then
                    newRadio.Checked = True
                    FoundPrevResponse = True
                End If


                ' Add handlers for click event so we can automatically move forward when button is clicked
                AddHandler newRadio.Click, AddressOf MyRadioButtonHandler_Click
                ' Add the control to the group box.
                newGroupBox.Controls.Add(newRadio)

            Next

            'Set the height of the groupox
            newGroupBox.Height = GroupBoxHeight

            'Put the question text in the label
            lblQuestion.Text = SubstituteFieldNames(inNode.SelectSingleNode("text").InnerText)

            'Add the Groupbox to the form
            newGroupBox.Location = ResponseLocation
            newGroupBox.Font = New Font("Arial", 10, FontStyle.Bold)
            inControls.Add(newGroupBox)

            ' check to see if we should disable the "Next" button
            If ShowPreviousResponse = True And (FoundPrevResponse = True Or Survey = "add_bio_mother") Then
                Button_Next.Enabled = True
                Button_Next.Focus()
            Else
                Button_Next.Enabled = False
            End If

            If QuestionInfoArray(CurrentQuestion).FieldName = "village" And Survey = "households" And ModifyingSurvey = True Then
                newGroupBox.Enabled = False
            Else
                newGroupBox.Enabled = True
            End If



        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub



    '**********************************************************************************
    ' This subroutine handles the Click event for the dynamically generated
    ' radiobuttons.  It is attached to all the radiobuttons using the AddHandler function
    ' at the time of radio button creation.  It basically does the same as clicking
    ' the 'Next' button - except it automatically moves to the next question after
    ' clicking a response
    '**********************************************************************************
    Private Sub MyRadioButtonHandler_Click(ByVal sender As Object, ByVal e As EventArgs)
        Try
            ' Verify that the type of control triggering this event is indeed a Radio Button.
            If TypeOf sender Is RadioButton Then
                'add a half second delay after the click......
                System.Threading.Thread.Sleep(500)


                '....and show the next question
                Button_Next_Click(Nothing, Nothing)
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub






    '**********************************************************************************
    ' This sub adds a GroupBox and checkboxes for multiple response questions along
    ' with the text for the survey question.
    '**********************************************************************************
    Public Sub AddCheckbox(ByVal inNode As XmlNode,
        ByVal inControls As Control.ControlCollection,
        ByVal ShowPreviousResponse As Boolean)

        Try
            ' Create a GroupBox to contain the radio buttons
            ' Set up some properties
            Dim newGroupBox As New GroupBox With {
                .Text = "",
                .Name = inNode.Attributes("fieldname").Value,
                .Tag = Tag,
                .Width = ResponseWidth
            }

            ' Create some variables to use in the following block of code to keep track of location of radio buttons
            Dim newCheckBox As CheckBox
            Dim CheckBoxPoint As New Point(5, 10)
            Dim NumResponses As Integer = 0

            For Each node As XmlNode In inNode.SelectNodes("responses/response")
                'add maximum of 8 then start a new column
                If NumResponses = 12 Then
                    CheckBoxPoint.X = 250
                    CheckBoxPoint.Y = 10
                    'next column.....
                ElseIf NumResponses = 24 Then
                    CheckBoxPoint.X = 500
                    CheckBoxPoint.Y = 10
                End If

                ' Create the radio button and set some properties
                newCheckBox = New CheckBox With {
                            .Location = CheckBoxPoint,
                            .AutoSize = True
                        }
                CheckBoxPoint.Y += newCheckBox.Height
                newCheckBox.Text = node.InnerText
                newCheckBox.Tag = node.Attributes("value").Value   'stores the numeric 'Value' of the response

                ' Add handlers for click event to check number of boxes checked and 
                'to determine is this question has been answered before
                AddHandler newCheckBox.CheckedChanged, AddressOf MyCheckBoxHandler

                ' Add the control to the group box.
                newGroupBox.Controls.Add(newCheckBox)
                NumResponses += 1   'keep track of the responses
            Next

            'Set the height of the groupox
            newGroupBox.Height = GroupBoxHeight

            'Put the question text in the label
            lblQuestion.Text = SubstituteFieldNames(inNode.SelectSingleNode("text").InnerText)
            'Add the Groupbox to the form
            newGroupBox.Location = ResponseLocation
            newGroupBox.Font = New Font("Arial", 10, FontStyle.Bold)
            inControls.Add(newGroupBox)

            ' check to see if we should disable the "Next" button
            If ShowPreviousResponse = True Then
                ShowCheckBoxResponses()
                Button_Next.Enabled = True
                Button_Next.Focus()
            Else
                Button_Next.Enabled = False
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub




    '**********************************************************************************
    ' This subroutine handles the Click event for the dynamically generated
    ' checkboxes.  It is attached to all the checkboxes using the AddHandler function
    ' at the time of checkbox creation.  It is used to determine wether of not
    ' we should enable or disable the 'Next' button (if none are checked -it is disabled)
    '**********************************************************************************
    Private Sub MyCheckBoxHandler(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim NumCheckBoxesChecked As Integer = 0

            ' Verify that the type of control triggering this event is indeed a CheckBox.
            If TypeOf sender Is CheckBox Then

                Dim aControl As Control
                Dim aGroupControl As Control
                Dim CheckBoxesValue As String

                CheckBoxesValue = QuestionInfoArray(CurrentQuestion).Response

                'Determine the number of check boxes checked
                For Each aControl In Me.Controls
                    ' Differentiate output based on the type of the control
                    Select Case TypeName(aControl)
                        Case "GroupBox"
                            ' Need to go inside of the GroupBox to yank out the RadioButtons
                            For Each aGroupControl In CType(aControl, GroupBox).Controls
                                If TypeOf aGroupControl Is CheckBox Then
                                    If CType(aGroupControl, CheckBox).Checked Then
                                        NumCheckBoxesChecked += 1
                                    End If
                                End If

                            Next
                    End Select
                Next
            End If

            'if no checkboxes are checked, disable the 'Next' button
            If NumCheckBoxesChecked > 0 Then
                Button_Next.Enabled = True
            Else
                Button_Next.Enabled = False
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub



    '**********************************************************************************
    ' This sub loops through all the check boxes to see which ones should be checked.
    ' It is only called when the 'Previous' button is clicked so that it displays
    ' which checkboxes were checked
    '**********************************************************************************
    Private Sub ShowCheckBoxResponses()
        Try
            Dim aControl As Control
            Dim aGroupControl As Control

            Dim ValueArray(MaxResponses)
            ValueArray = Split(QuestionInfoArray(CurrentQuestion).Value, ",")

            'Determine the number of check bosex checked
            For Each aControl In Me.Controls
                ' Differentiate output based on the type of the control
                Select Case TypeName(aControl)
                    Case "GroupBox"
                        ' Need to go inside of the GroupBox to yank out the RadioButtons
                        For Each aGroupControl In CType(aControl, GroupBox).Controls
                            If TypeOf aGroupControl Is CheckBox Then
                                'For i = 0 To ValueArray.Length
                                If ValueArray.Contains((CType(aGroupControl, CheckBox).Tag).ToString) Then
                                    CType(aGroupControl, CheckBox).Checked = True
                                End If
                            End If

                        Next
                End Select
            Next
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub




    '**********************************************************************************
    ' This sub adds a TextBox to the response area along
    ' with the text for the survey question.
    '**********************************************************************************
    Public Sub AddCalendar(ByVal inNode As XmlNode,
        ByVal inControls As Control.ControlCollection,
        ByVal ShowPreviousResponse As Boolean)

        Try
            ' Create a new control.
            Dim newCalendar As New MonthCalendar
            CurrentDateSelected = ""

            ' Set up some properties
            newCalendar.MinDate = "1905/1/1"
            newCalendar.MaxDate = Now()
            newCalendar.MaxSelectionCount = 1
            newCalendar.ShowToday = False
            newCalendar.Name = inNode.Attributes("fieldname").Value
            newCalendar.Tag = Tag


            Select Case QuestionInfoArray(CurrentQuestion).FieldName
                Case "appt_date"
                    newCalendar.MaxDate = DateAdd(DateInterval.Day, 30, Now())
                    newCalendar.MinDate = Now()
                Case "appt_scheduled", "tca", "planned_testing_date", "hivaction_date"
                    newCalendar.MaxDate = DateAdd(DateInterval.Day, 60, Now())
                    newCalendar.MinDate = Now()
                Case "hiv_test_date"
                    newCalendar.MaxDate = Now()
                    newCalendar.MinDate = DateAdd(DateInterval.Day, -7, Now())
                Case "edd"
                    newCalendar.MaxDate = DateAdd(DateInterval.Day, 250, Now())
                    newCalendar.MinDate = Now()
            End Select



            'Put the question text in the label
            lblQuestion.Text = SubstituteFieldNames(inNode.SelectSingleNode("text").InnerText)

            ' Add handlers for click event to check number of boxes checked and 
            'to determine is this question has been answered before
            AddHandler newCalendar.DateSelected, AddressOf MyDateSelected



            'Add the Textbox to the form
            newCalendar.Location = ResponseLocation
            inControls.Add(newCalendar)
            newCalendar.Focus()



            ' check to see if we should disable the "Next" button
            If ShowPreviousResponse = True Then
                newCalendar.SetDate(QuestionInfoArray(CurrentQuestion).Response.ToString)
                CurrentDateSelected = QuestionInfoArray(CurrentQuestion).Response.ToString
                Button_Next.Enabled = True
                Button_Next.Focus()
            Else
                Button_Next.Enabled = False
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub





    '**********************************************************************************
    ' This subroutine handles the DateSelected event for the dynamically generated
    ' Calendar.  
    '**********************************************************************************
    Private Sub MyDateSelected(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Button_Next.Enabled = True
            CurrentDateSelected = CType(sender, MonthCalendar).SelectionRange.Start

        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub




    '**********************************************************************************
    ' This sub adds a TextBox to the response area along
    ' with the text for the survey question.
    '**********************************************************************************
    Public Sub AddTextBox(ByVal inNode As XmlNode,
        ByVal inControls As Control.ControlCollection,
        ByVal ShowPreviousResponse As Boolean)

        Try
            ' Create a new control.
            ' Set up some properties
            Dim newText As New TextBox With {
                .Text = "",
                .Name = inNode.Attributes("fieldname").Value,
                .Tag = Tag,
                .Width = controlWidth
            }


            ' Set the MaxLength property based off of the XML node information.
            If inNode.SelectSingleNode("maxCharacters") IsNot Nothing Then
                newText.MaxLength = Integer.Parse(inNode.SelectSingleNode("maxCharacters").InnerText)
            End If

            ' Calculate the number of lines that should be allowed for.
            If newText.MaxLength > 0 Then
                Dim numLines As Integer = (newText.MaxLength \ charPerLine) + 1

                ' Calculate how large the textbox should be, and whether scrollbars are necessary.
                If numLines = 1 Then
                    newText.Multiline = False
                Else
                    If numLines >= 8 Then
                        newText.Multiline = True
                        newText.Height = 8 * lineHeight
                        newText.ScrollBars = ScrollBars.Vertical
                    Else
                        newText.Multiline = True
                        newText.Height = numLines * lineHeight
                        newText.ScrollBars = ScrollBars.None
                    End If
                End If
            End If


            'Put the question text in the label
            lblQuestion.Text = SubstituteFieldNames(inNode.SelectSingleNode("text").InnerText)


            'Add the Textbox to the form
            newText.Location = ResponseLocation
            newText.Font = New Font("Arial", 10, FontStyle.Bold)
            inControls.Add(newText)
            newText.Focus()
            newText.CharacterCasing = CharacterCasing.Upper

            'Add handlers for keyup event event so we can enable the "Next" button
            AddHandler newText.KeyUp, AddressOf MyTextBoxHandler


            'Add handler for keypress event event so we can check for numeric values
            AddHandler newText.KeyPress, AddressOf MyTextBoxHandlerCheckNumeric




            Dim SpecialButtonSelected As Boolean = False    'indicates whether a 'specila button' was selected (DK, NA, Refuse)

            If ShowPreviousResponse = True Then
                If inNode.SelectSingleNode("na") IsNot Nothing Then
                    If QuestionInfoArray(CurrentQuestion).Value = inNode.SelectSingleNode("na").InnerText.ToString Then
                        SpecialButtonSelected = True
                    End If
                End If


                If inNode.SelectSingleNode("refuse") IsNot Nothing Then
                    If QuestionInfoArray(CurrentQuestion).Value = inNode.SelectSingleNode("refuse").InnerText.ToString Then
                        SpecialButtonSelected = True
                    End If
                End If

                If inNode.SelectSingleNode("dont_know") IsNot Nothing Then
                    If QuestionInfoArray(CurrentQuestion).Value = inNode.SelectSingleNode("dont_know").InnerText.ToString Then
                        SpecialButtonSelected = True
                    End If
                End If

                If SpecialButtonSelected = False Then
                    newText.Text = QuestionInfoArray(CurrentQuestion).Response.ToString
                Else
                    newText.Text = ""
                End If


                'newText.Text = QuestionInfoArray(CurrentQuestion).Response.ToString
                Button_Next.Enabled = True
                Button_Next.Focus()
            Else
                Button_Next.Enabled = False
            End If


        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub









    '**********************************************************************************
    ' Event handler for keyup event on textbox
    ' This is used to determine if the 'Next' button should be enabled or disabled
    '**********************************************************************************
    Private Sub MyTextBoxHandler(ByVal sender As Object, ByVal e As EventArgs)
        Try
            ' Verify that the type of control triggering this event is a text box
            If TypeOf sender Is TextBox Then

                Dim MaxLength As Integer = 1
                Select Case QuestionInfoArray(CurrentQuestion).FieldName
                    Case Is = "ranum"
                        MaxLength = 2
                    Case Is = "hhnum"
                        MaxLength = 3
                    Case Is = "phone1", "phone2", "phone3"
                        MaxLength = 10
                    Case Is = "barcode", "barcode2", "collection_time"
                        MaxLength = 5
                    Case Is = "orphan_moyr", "orphan_fayr", "tbeveryear", "tbyear", "datehivtestyr"
                        MaxLength = 4
                End Select

                'check the lenght of the text - if it > 0, then enable the "Next button"
                'for the "cwi" and "hnum" fields - don't enable it until 3 characters have been reached
                If Len(CType(sender, TextBox).Text) > MaxLength - 1 Then
                    Button_Next.Enabled = True
                Else
                    Button_Next.Enabled = False
                End If
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub

    Function GetMaxLengthByFieldName(ByVal FieldName As String) As Integer
        Dim MaxLength As Integer = 1 ' Default MaxLength

        Select Case FieldName
            Case "tabletnum", "pinitials", "respondants_age"
                MaxLength = 2
            Case "hhnum"
                MaxLength = 3
            Case "phone1", "phone2", "phone3", "mobile_number"
                MaxLength = 10
            Case "barcode", "barcode2", "collection_time"
                MaxLength = 5
            Case "orphan_moyr", "orphan_fayr", "tbeveryear", "tbyear", "datehivtestyr"
                MaxLength = 4
                ' Add more cases as needed
        End Select

        Return MaxLength
    End Function



    '**********************************************************************************
    ' Event handler for keyup event on textbox
    ' This is used to determine if the 'Next' button should be enabled or disabled
    '**********************************************************************************
    Private Sub MyTextBoxHandlerCheckNumeric(ByVal sender As Object, ByVal e As KeyPressEventArgs)
        Try
            ' Verify that the type of control triggering this event is indeed a Radio Button.
            If TypeOf sender Is TextBox Then

                Dim currentFieldName As String = QuestionInfoArray(CurrentQuestion).FieldName
                Dim MaxLength As Integer = GetMaxLengthByFieldName(currentFieldName)

                If Len(CType(sender, TextBox).Text) > MaxLength - 1 Then
                    ' Move to next question if user hits "Enter"
                    Select Case e.KeyChar
                        Case ChrW(Keys.Return)
                            Button_Next_Click(Nothing, Nothing)
                            e.Handled = True
                            Exit Sub
                    End Select
                End If

                'Check the question type - if it is integer only allow 0 - 9 characters
                If QuestionInfoArray(CurrentQuestion).FieldType = "integer" Or QuestionInfoArray(CurrentQuestion).FieldType = "text_integer" Or QuestionInfoArray(CurrentQuestion).FieldType = "text_id" Or QuestionInfoArray(CurrentQuestion).FieldType = "phone_num" Then
                    Select Case e.KeyChar
                        Case "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", vbBack, ChrW(Keys.Return)
                            e.Handled = False
                        Case Else
                            MsgBox("Only numbers are allowed!", MsgBoxStyle.Information, "Numbers only")
                            e.Handled = True
                            Exit Sub
                    End Select
                    'Check the question type - if it is decimal only allow 0 - 9 characters and "."
                ElseIf QuestionInfoArray(CurrentQuestion).FieldType = "decimal" Or QuestionInfoArray(CurrentQuestion).FieldType = "text_decimal" Then
                    Select Case e.KeyChar

                        Case "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", ".", vbBack, ChrW(Keys.Return)
                            e.Handled = False
                        Case Else
                            MsgBox("Only numbers are allowed!", MsgBoxStyle.Information, "Numbers only")
                            e.Handled = True
                            Exit Sub
                    End Select
                ElseIf QuestionInfoArray(CurrentQuestion).FieldType = "hourmin" Then
                    Select Case e.KeyChar

                        Case "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", ":", vbBack, ChrW(Keys.Return)
                            e.Handled = False
                        Case Else
                            MsgBox("Only numbers and ':' are allowed!", MsgBoxStyle.Information, "Numbers only")
                            e.Handled = True
                            Exit Sub
                    End Select
                End If
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub






    '
    '**********************************************************************************
    ' This sub adds an Information screen
    ' to the question area of the survey
    '**********************************************************************************
    Public Sub AddInformation(ByVal inNode As XmlNode,
        ByVal inControls As Control.ControlCollection)
        If inControls Is Nothing Then
            Throw New ArgumentNullException(NameOf(inControls))
        End If

        Try
            If QuestionInfoArray(CurrentQuestion).FieldName = "play_video" Then
                lblQuestion.Text = "Arm: " & RandArmText

                Dim video_name As String = ""
                Dim videoPath As String = ""

                ' Create videoView if it doesn't exist yet
                Dim videoView As New VideoView()
                videoView.Size = New Size(600, 400)
                ' Optional: center the video view in the parent control
                videoView.Location = New Point(20, 50)
                inControls.Add(videoView)

                ' Initialize LibVLC and media player
                Dim libVLC As New LibVLC()
                Dim mediaPlayer As New MediaPlayer(libVLC)

                ' Connect media player to video view
                videoView.MediaPlayer = mediaPlayer




                ' Create only the replay button
                Dim btnReplay As New Button()
                btnReplay.Text = "Replay Video"
                btnReplay.Size = New Size(120, 30)
                btnReplay.Location = New Point(200, 480)
                AddHandler btnReplay.Click, Sub(s, e)
                                                mediaPlayer.Stop()
                                                mediaPlayer.Play()
                                            End Sub
                inControls.Add(btnReplay)



                ' Load the evatar video
                ' Call the function to get the correct video
                If GetValue("eligibility_check") = 1 And RandArmText <> "Default appointment" Then
                    video_name = getRandVideo(GetValue("client_sex"), GetValue("respondants_age"), RandArmText, GetValue("preferred_language"))
                    videoPath = "C:\IBIS_pilot\rand_video\" & video_name
                End If


                ' Create media and play it
                Dim media As New Media(libVLC, New Uri(videoPath))
                mediaPlayer.Media = media
                mediaPlayer.Play()

            Else
                'Put the question text in the label
                lblQuestion.Text = SubstituteFieldNames(inNode.SelectSingleNode("text").InnerText)
            End If
            Button_Next.Focus()

        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub







    '**********************************************************************************
    ' This function returns the questions text with any field names substutued
    ' as per the xml file.  Substituted field names are in the form [[xxxx]],
    ' where xxxx is the field name whose value is to be substituted 
    '**********************************************************************************
    Private Function SubstituteFieldNames(ByVal OriginalText As String) As String

        Dim FieldName As String
        Dim FieldNameResponse As String = ""
        Try
            Dim substring As String = "[["
            Dim startIdx As Integer
            Dim endIdx As Integer
            Dim count As Integer

            ' Loop through the string and extract the text inside the square brackets
            count = 0
            Do While InStr(OriginalText, substring) > 0
                count += 1
                startIdx = InStr(OriginalText, substring) + Len(substring)
                endIdx = InStr(startIdx, OriginalText, "]]")
                FieldName = Mid(OriginalText, startIdx, endIdx - startIdx)
                FieldNameResponse = GetResponse(FieldName)
                OriginalText = Replace(OriginalText, "[[" & FieldName & "]]", FieldNameResponse)
            Loop
            SubstituteFieldNames = OriginalText
        Catch ex As Exception
            MessageBox.Show(ex.Message)
            Return OriginalText
        End Try
    End Function




    'this sub saves the data to the array for the current question
    Sub SaveDataToArray()
        Dim surveyResponseText As String = ""
        Dim surveyResponseValue As String = "-9"
        Try

            'if it's an automatic variable......
            If QuestionInfoArray(CurrentQuestion).QuesType = "automatic" Then
                QuestionInfoArray(CurrentQuestion).Response = CurrentAutoValue
                QuestionInfoArray(CurrentQuestion).Value = CurrentAutoValue
                QuestionInfoArray(CurrentQuestion).HasBeenAnswered = True
            Else    'otherwise, use the user selected value
                Dim aControl As Control
                Dim aGroupControl As Control

                'Loop through all the differnet types of possible controls and 
                'determine which type of control has the desired response
                'and save it to the array
                For Each aControl In Me.Controls
                    ' Differentiate output based on the type of the control
                    Select Case TypeName(aControl)
                        Case "GroupBox"
                            Dim AnySelectedValue As Boolean = False
                            For Each aGroupControl In CType(aControl, GroupBox).Controls

                                'Radio buttons
                                If TypeOf aGroupControl Is RadioButton Then
                                    If CType(aGroupControl, RadioButton).Checked Then
                                        surveyResponseText = aGroupControl.Text
                                        surveyResponseValue = aGroupControl.Tag
                                        AnySelectedValue = True
                                    End If

                                    'check boxes
                                ElseIf TypeOf aGroupControl Is CheckBox Then
                                    If CType(aGroupControl, CheckBox).Checked Then
                                        If surveyResponseText <> "" Then
                                            surveyResponseText = surveyResponseText & "," & aGroupControl.Text
                                            surveyResponseValue = surveyResponseValue & "," & aGroupControl.Tag
                                        Else
                                            surveyResponseText += aGroupControl.Text
                                            surveyResponseValue = aGroupControl.Tag
                                        End If
                                        AnySelectedValue = True
                                    End If
                                End If
                            Next

                            If QuestionInfoArray(CurrentQuestion).HasBeenAnswered = True And AnySelectedValue = False Then   'previous response must have been a Special Button
                                surveyResponseText = QuestionInfoArray(CurrentQuestion).Response
                                surveyResponseValue = QuestionInfoArray(CurrentQuestion).Value
                            End If


                        Case "MonthCalendar"
                            'surveyResponseText = aControl.Text
                            surveyResponseValue = CurrentDateSelected
                            surveyResponseText = CurrentDateSelected


                        Case "TextBox"
                            If aControl.Text = "READING......" Then
                                aControl.Text = "-9"
                            End If
                            If QuestionInfoArray(CurrentQuestion).HasBeenAnswered = True And aControl.Text = "" Then   'previous response must have been a Special Button
                                surveyResponseText = QuestionInfoArray(CurrentQuestion).Response
                                surveyResponseValue = QuestionInfoArray(CurrentQuestion).Value
                            Else
                                surveyResponseText = aControl.Text
                                surveyResponseValue = aControl.Text
                            End If
                    End Select
                Next

                QuestionInfoArray(CurrentQuestion).Response = surveyResponseText
                QuestionInfoArray(CurrentQuestion).Value = surveyResponseValue
                QuestionInfoArray(CurrentQuestion).HasBeenAnswered = True
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub


    Private Sub ButtonCancelInterview_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonCancelInterview.Click
        Try
            'if we haven't started the survey yet, just close it
            If QuestionInfoArray(CurrentQuestion).PrevQues = -1 And CurrentQuestion = 0 And QuestionInfoArray(CurrentQuestion).Value = "-9" Then
                InterviewCancelled = True
                Me.Close()
            Else
                If ModifyingSurvey = True Then
                    If MessageBox.Show("S T O P!" & vbNewLine & vbNewLine & "Any changes you have made will not be saved.  You need to move forward to the end of the questions in order to save any changes." & vbNewLine & vbNewLine & _
                                "Are you sure you want to cancel this interview?", "End Interview?", MessageBoxButtons.YesNo) = DialogResult.Yes Then
                        InterviewCancelled = True
                        Me.Close()
                        Me.Dispose()
                        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
                    End If
                Else

                    If MessageBox.Show("S T O P!" & vbNewLine & vbNewLine & "If you continue, the data from this interview will be permanently lost. " & vbNewLine & vbNewLine &
                                "Are you sure you want to cancel this interview?", "End Interview?", MessageBoxButtons.YesNo) = DialogResult.Yes Then
                        InterviewCancelled = True
                        Me.Close()
                        Me.Dispose()
                        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
                    End If
                End If
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub





End Class