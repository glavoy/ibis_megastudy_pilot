'**********************************************************************************
' This Module is for generating the values of automatic variables.
' Every automatic variable in the Data Dictionary must have code to generate it.
' The value of the automatic variable is saved to the 'CurrentAutoValue' variable
' and will be written to the database.
'**********************************************************************************

Imports System.Xml

Module QuestionInfoArray_Functions
    'gets the numeric equivalent of the 'response' passed to it for a specific question
    Public Function GetValueOfResponse(ByVal question_number As Integer, ByVal response As String) As Integer
        GetValueOfResponse = -9
        Try
            Dim myNode As XmlNode
            myNode = NewSurvey.xr.GetElementsByTagName("question").Item(question_number)

            For Each node As XmlNode In myNode.SelectNodes("responses/response")
                If node.InnerText = response Then
                    GetValueOfResponse = node.Attributes("value").Value
                End If
            Next
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Function



    'gets the text 'response' for a question number when passed the 'Value'
    Public Function GetResponseOfValue(ByVal question_number As Integer, ByVal value As String) As Integer
        GetResponseOfValue = -9
        Try
            Dim myNode As XmlNode
            myNode = NewSurvey.xr.GetElementsByTagName("question").Item(question_number)

            For Each node As XmlNode In myNode.SelectNodes("responses/response")
                If node.Attributes("value").Value = value Then
                    GetResponseOfValue = node.InnerText
                End If
            Next
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Function



    'returns the question number based on the fieldname passed to it
    Public Function GetQuestionNumber(ByVal fieldname As String) As Integer
        GetQuestionNumber = NewSurvey.CurrentQuestion + 1
        Try
            Dim i As Integer
            For i = 0 To NewSurvey.NumQuestions - 1
                If NewSurvey.QuestionInfoArray(i).FieldName = fieldname Then
                    GetQuestionNumber = NewSurvey.QuestionInfoArray(i).QuesNum
                    Exit Function
                End If
            Next
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Function



    'returns the field type based on the question number passed to it
    Public Function GetFieldType(ByVal question_number As Integer) As String
        GetFieldType = ""
        Try
            Dim i As Integer
            For i = 0 To NewSurvey.NumQuestions - 1
                If NewSurvey.QuestionInfoArray(i).QuesNum = question_number Then
                    GetFieldType = NewSurvey.QuestionInfoArray(i).FieldType
                    Exit Function
                End If
            Next
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Function



    'returns the question number based on the fieldname passed to it
    Public Function GetFieldName(ByVal question_number As Integer) As String
        GetFieldName = ""
        Try
            Dim i As Integer
            For i = 0 To NewSurvey.NumQuestions - 1
                If NewSurvey.QuestionInfoArray(i).QuesNum = question_number Then
                    GetFieldName = NewSurvey.QuestionInfoArray(i).FieldName
                    Exit Function
                End If
            Next
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Function



    'returns the value based on the field name passed to it
    Public Function GetValue(ByVal fieldname As String) As String
        GetValue = "-9"
        Try
            Dim i As Integer
            For i = 0 To NewSurvey.NumQuestions - 1
                If NewSurvey.QuestionInfoArray(i).FieldName = fieldname Then
                    GetValue = NewSurvey.QuestionInfoArray(i).Value
                    Exit Function
                End If
            Next
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Function



    'returns the Response based on the field name passed to it
    Public Function GetResponse(ByVal fieldname As String) As String
        GetResponse = "-9"
        Try
            Dim i As Integer
            For i = 0 To NewSurvey.NumQuestions - 1
                If NewSurvey.QuestionInfoArray(i).FieldName = fieldname Then
                    GetResponse = NewSurvey.QuestionInfoArray(i).Response
                    Exit Function
                End If
            Next
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Function



    'returns the Response based on the field name passed to it
    Public Sub SetLastMod()
        Try
            Dim i As Integer
            For i = 0 To NewSurvey.NumQuestions - 1
                If NewSurvey.QuestionInfoArray(i).FieldName = "lastmod" Then
                    NewSurvey.QuestionInfoArray(i).Response = Now().ToString
                    NewSurvey.QuestionInfoArray(i).Value = Now().ToString
                    Exit Sub
                End If
            Next
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub


End Module
