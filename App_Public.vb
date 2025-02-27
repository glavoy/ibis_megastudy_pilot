Imports System.Configuration

Module IBIS_Public

    Public SW_VER As String = ConfigurationManager.AppSettings("Version")

    Public Survey As String                                 'This is used to keep which survey we are doing - add household or household members
    Public SUBJID As String                                 'used to store the SUBJID
    Public VDATE As String = "01/01/1899"                   'used to store the Visit Date
    Public ModifyingSurvey As Boolean = False               'keeps track of wether or not we are doing a new survey or modifying an existing one
    Public CurrentAutoValue As String                       'the current auto value of an automatic varaible
    Public DontKnow_Value As String                         'stores the current "Don't Know' value for a question
    Public NA_Value As String                               'stores the current "N/A' value for a question
    Public Refuse_Value As String                           'stores the current "Refuse' value for a question
    Public CurrentDateSelected As String = ""               'used to save date values selected from a calendar
    Public DEFAULT_DATE As String = "01/01/1899"            'default date
    Public InterviewCancelled As Boolean = False            'Keeps track of whether or not participant has completed the checkin

End Module
