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

                    Dim studyid As String = GetValue("subjid")
                    If ModifyingSurvey = True And studyid <> "-9" Then
                        CurrentAutoValue = studyid
                        SUBJID = studyid
                    Else
                        Dim tabletnum As String = GetValue("tabletnum")
                        Dim countrycode As String = GetValue("countrycode")
                        Dim clinic As String = GetValue("health_facility")
                        Dim hhid As String = ""
                        Select Case Len(tabletnum)
                            Case Is = 1
                                hhid = "IBIS" & countrycode & clinic & "999" & tabletnum
                            Case Is = 2
                                hhid = "IBIS" & countrycode & clinic & "99" & tabletnum
                            Case Is = 3
                                hhid = "IBIS" & countrycode & clinic & "9" & tabletnum
                        End Select
                        CurrentAutoValue = hhid & "-" & GetIBISEnrollmentLineNum(hhid, tabletnum)
                        SUBJID = CurrentAutoValue
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

                Case Is = "screening_id"
                    Dim screening_id As String = GetValue("screening_id")
                    If ModifyingSurvey = True And screening_id <> "-9" Then
                        CurrentAutoValue = screening_id
                    Else
                        Dim tabletnum As String = GetValue("tabletnum")
                        Dim countrycode As String = GetValue("countrycode")
                        Dim clinic As String = GetValue("health_facility")
                        Dim hhid As String = ""
                        Select Case Len(tabletnum)
                            Case Is = 1
                                hhid = "SCRN" & countrycode & clinic & "999" & tabletnum
                            Case Is = 2
                                hhid = "SCRN" & countrycode & clinic & "99" & tabletnum
                            Case Is = 3
                                hhid = "SCRN" & countrycode & clinic & "9" & tabletnum
                        End Select
                        CurrentAutoValue = hhid & "-" & GetIBISLineNum(hhid, tabletnum)
                    End If


                Case Is = "eligibility_check1"
                    Dim age_check As Integer = CInt(GetValue("age_check"))
                    Dim stay_3_months As Integer = CInt(GetValue("stay_3_months"))
                    Dim negative_hiv As Integer = CInt(GetValue("negative_hiv"))
                    Dim mobile_phone As Integer = CInt(GetValue("mobile_phone"))
                    Dim reading_language As Integer = CInt(GetValue("reading_language"))


                    CurrentAutoValue = 1
                    If age_check = 2 Or stay_3_months = 2 Or negative_hiv = 2 Or mobile_phone = 2 Or reading_language = 2 Then
                        CurrentAutoValue = 0
                    End If

                Case Is = "eligibility_check2"
                    Dim eligibility_check1 As Integer = CInt(GetValue("eligibility_check1"))
                    Dim multiple_partners As Integer = CInt(GetValue("multiple_partners"))
                    Dim new_partner As Integer = CInt(GetValue("new_partner"))
                    Dim unprotected_sex As Integer = CInt(GetValue("unprotected_sex"))
                    Dim hiv_positive_partner As Integer = CInt(GetValue("hiv_positive_partner"))
                    Dim sti_history As Integer = CInt(GetValue("sti_history"))
                    Dim tb_history As Integer = CInt(GetValue("tb_history"))
                    Dim sex_for_compensation As Integer = CInt(GetValue("sex_for_compensation"))
                    Dim paid_for_sex As Integer = CInt(GetValue("paid_for_sex"))
                    Dim dice_clinic As Integer = CInt(GetValue("dice_clinic"))
                    Dim on_prep As Integer = CInt(GetValue("on_prep"))
                    Dim on_pep As Integer = CInt(GetValue("on_pep"))
                    Dim recent_hiv_exposure As Integer = CInt(GetValue("recent_hiv_exposure"))

                    CurrentAutoValue = 0
                    If eligibility_check1 = 1 And (multiple_partners = 1 Or new_partner = 1 Or unprotected_sex = 1 Or hiv_positive_partner = 1 Or sti_history = 1 Or tb_history = 1 Or sex_for_compensation = 1 Or paid_for_sex = 1 Or dice_clinic = 1 Or on_prep = 1 Or on_pep = 1 Or recent_hiv_exposure = 1) Then
                        CurrentAutoValue = 1
                    End If

                Case Is = "uniqueid"
                    If ModifyingSurvey = True Then
                        CurrentAutoValue = GetValue("uniqueid")
                    Else
                        CurrentAutoValue = Guid.NewGuid().ToString()
                    End If

                Case Is = "arm"
                    If ModifyingSurvey = True Then
                        CurrentAutoValue = GetValue("arm")
                    Else
                        Dim clinic As Integer = CInt(GetValue("health_facility"))
                        CurrentAutoValue = GetNextRandArm(clinic)
                        RandArmID = CurrentAutoValue
                    End If

                Case Is = "arm_text"
                    If ModifyingSurvey = True Then
                        CurrentAutoValue = GetValue("arm_text")
                    Else
                        Dim clinic As Integer = CInt(GetValue("health_facility"))
                        Dim arm As Integer = CInt(GetValue("arm"))
                        CurrentAutoValue = GetNextRandArmText(clinic, arm)
                        RandArmText = CurrentAutoValue
                    End If

                Case Is = "preferred_language_text"
                    Dim preferred_language As String = GetValue("preferred_language")
                    Select Case preferred_language
                        Case Is = "1"
                            CurrentAutoValue = "DHOLUO"
                        Case Is = "2"
                            CurrentAutoValue = "SWAHILI"
                        Case Is = "3"
                            CurrentAutoValue = "ENGLISH"
                        Case Is = "4"
                            CurrentAutoValue = "RUNYONKOLE"
                        Case Is = "5"
                            CurrentAutoValue = "LUGANDA"
                        Case Is = "6"
                            CurrentAutoValue = "OTHER"
                        Case Is = "7"
                            CurrentAutoValue = "NONE"
                        Case Else
                            CurrentAutoValue = "ENGLISH"
                    End Select

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
