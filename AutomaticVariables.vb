'**********************************************************************************
' This Module is for generating the values of automatic variables.
' Every automatic variable in the Data Dictionary must have code to generate it.
' The value of the automatic variable is saved to the 'CurrentAutoValue' variable
' and will be written to the database.
'**********************************************************************************

Imports System.Configuration
Imports System.Data.OleDb


Module AutomaticVariables
    Public Sub AddAutomatic(ByVal fieldname As String)
        Try
            Select Case fieldname

                'initialtimestamp
                Case Is = "starttime"
                    CurrentAutoValue = GetValue("starttime")
                    If CurrentAutoValue = "" Or
                        CurrentAutoValue = DEFAULT_DATE Or
                        CurrentAutoValue = "01/01/1899 00:00:00" Or
                        CurrentAutoValue = "01/01/1899 12:00:00 AM" Or
                        CurrentAutoValue = "-9" Then
                        CurrentAutoValue = Now().ToString
                    End If



                Case Is = "subjid"
                    If ModifyingSurvey = True Then
                        CurrentAutoValue = SUBJID
                    Else
                        CurrentAutoValue = SUBJID
                        'Dim tabletnum As String = GetValue("tabletnum")
                        'CurrentAutoValue = Microsoft.VisualBasic.Left(SUBJID, 11) & "-" & GetLineNum(Microsoft.VisualBasic.Left(SUBJID, 11), tabletnum)
                        'SUBJID = CurrentAutoValue
                    End If





                Case Is = "swver"
                    If ModifyingSurvey = True Then
                        CurrentAutoValue = GetValue("swver")
                    Else
                        CurrentAutoValue = SW_VER
                    End If

                Case Is = "stoptime"
                    CurrentAutoValue = GetValue("stoptime")
                    If CurrentAutoValue = "" Or CurrentAutoValue = DEFAULT_DATE Or
                        CurrentAutoValue = "01/01/1899 00:00:00" Or
                        CurrentAutoValue = "01/01/1899 12:00:00 AM" Or
                        CurrentAutoValue = "-9" Or CurrentAutoValue = "01/11/2018 12:00:00 AM" Then
                        CurrentAutoValue = Now().ToString
                    End If


                Case Is = "lastmod"
                    If ModifyingSurvey = True Then
                        Dim lastModValue As String = GetValue("lastmod").ToString()

                        If Not String.IsNullOrEmpty(lastModValue) Then
                            CurrentAutoValue = lastModValue
                        Else
                            CurrentAutoValue = Now().ToString()
                        End If
                    Else
                        CurrentAutoValue = Now().ToString()
                    End If


            End Select

        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub







    'sub to calculate automatic variable: age
    Public Function CalculateAge(dob) As String
        CalculateAge = 0
        If Len(dob) > 10 Then
            dob = Microsoft.VisualBasic.Left(dob, 10)
        End If

        Try
            Dim format As String = "d/MM/yyyy"
            Dim provider As Globalization.CultureInfo = Globalization.CultureInfo.InvariantCulture
            Dim dt As Date = Date.ParseExact(dob, format, provider)

            Dim d1, d2 As Date
            Dim days, months, years As Long

            d1 = dt
            d2 = Now.ToShortDateString

            years = Year(d1)
            months = Month(d1)
            days = d1.Day

            years = Year(d2) - years
            months = Month(d2) - months
            days = d2.Day - days

            If Math.Sign(days) = -1 Then
                days = 30 - Math.Abs(days)
                months -= 1
            End If

            If Math.Sign(months) = -1 Then
                months = 12 - Math.Abs(months)
                years -= 1
            End If

            CalculateAge = years.ToString

        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Function



    'function to get community code
    Public Function GetCommunityCode() As String
        GetCommunityCode = "99"
        'Try
        '    Dim config As Configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None)
        '    Dim section As ConnectionStringsSection = DirectCast(config.GetSection("connectionStrings"), ConnectionStringsSection)
        '    Dim ConnectionString As New OleDbConnection(section.ConnectionStrings("ConnString").ConnectionString)
        '    Dim StrSQL As String = "select top 1 community_code from census_survey"
        '    Dim da As New OleDbDataAdapter(StrSQL, ConnectionString)
        '    Dim ds As New DataSet
        '    da.Fill(ds)

        '    For Each row As DataRow In ds.Tables(0).Rows
        '        GetCommunityCode = CStr(row("community_code"))
        '    Next

        '    ConnectionString.Close()

        'Catch ex As Exception
        '    MessageBox.Show(ex.Message)
        'End Try
    End Function


End Module
