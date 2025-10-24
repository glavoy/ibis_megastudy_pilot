'**********************************************************************************
' This Module is for all the Public variables and functions.
' Add any new variable and functions to help in managing database changes
'**********************************************************************************

Imports System.Configuration
Imports System.Data.OleDb
Imports System.IO
Imports System.Text.RegularExpressions

Module DatabaseChanges


    '**********************************************************************************
    ' This Class will be used add all the db changes and execute them automatically
    '**********************************************************************************
    Public Class ChangeSet

        Public Property Author As String
        Public Property Changesetname As String
        Public Property Changeset As String
        Public Property Uuid As String
        Public Property Comments As String



        ' Constructor to initialize a new instance of ChangeSet
        Public Sub New()
        End Sub

        ' Overloaded constructor to initialize with values
        Public Sub New(author As String, changesetname As String, changeset As String, uuid As String, comments As String)
            Me.Author = author
            Me.Changesetname = changesetname
            Me.Changeset = changeset
            Me.Uuid = uuid
            Me.Comments = comments
        End Sub

        ' Method to return a string representation of the ChangeSet
        Public Overrides Function ToString() As String
            Return $"author: {Author}, changesetname: {Changesetname}, changeset: {Changeset}, uuid: {Uuid}, comments: {Comments}"
        End Function

        ' Method to return a string representation of the ChangeSet
        Public Function CreateInsert() As String
            Return $"INSERT INTO gistchangeset (author,changesetname,changeset, uuid, comments) VALUES({Author},{Changesetname}, {Changeset}, {Uuid}, {Comments};"
        End Function
    End Class

    '**********************************************************************************
    ' Add Your Change sets Here
    '**********************************************************************************
    Public Function GetDatabaseChangeSets() As ChangeSet()
        Try
            Dim db_changeSets() As ChangeSet = {
                New ChangeSet("werick", "Add columns baseline", CreateAlterTableSQL("baseline", "service_provider Integer, service_provider_oth TEXT(80)"), Guid.NewGuid().ToString(), "Add new columns to the baseline crf"),
                New ChangeSet("werick", "Add columns followup", CreateAlterTableSQL("followup", "hiv_risk_status Integer, knows_art Integer, knows_prep Integer, 
                                                                                    on_prep Integer, ready_for_hivtest Integer, motivation Integer, 
                                                                                    motivation_oth TEXT(255), multiple_partners Integer"), Guid.NewGuid().ToString(), "Add new columns to the followup crf"),
                New ChangeSet("werick", "Add columns followup", CreateAlterTableSQL("followup", "new_partner Integer, unprotected_sex Integer, hiv_positive_partner Integer, 
                                                                                    sti_history Integer, sex_for_compensation Integer, paid_for_sex Integer, 
                                                                                    dice_clinic Integer, on_pep Integer"), Guid.NewGuid().ToString(), "Add new columns to the followup crf"),
                New ChangeSet("werick", "Add columns followup", CreateAlterTableSQL("followup", "recent_hiv_exposure Integer, hiv_positive_referral_accept Integer, prep_pep_eligible Integer, 
                                                                                    prep_pep_referral_accept Integer"), Guid.NewGuid().ToString(), "Add new columns to the followup crf")
            }

            'phonenum_lab
            Return db_changeSets
        Catch ex As Exception
            MessageBox.Show(ex.Message)
            Return New ChangeSet() {} ' Return an empty array
        End Try

    End Function

    Function CreateAlterTableSQL(table_name As String, colNames As String) As String
        Return $"ALTER TABLE {table_name} ADD COLUMN {colNames} ;"
    End Function

    Function CreateUpdateTableSQL(table_name As String, colNames As String, Optional condition As String = Nothing) As String
        If condition IsNot Nothing Then
            Return $"UPDATE {table_name} SET {colNames} WHERE {condition};"
        Else
            Return $"UPDATE {table_name} SET {colNames} ;"
        End If

    End Function

    Function CreateAlterColumnSQL(table_name As String, colNames As String) As String
        Return $"ALTER TABLE {table_name} ALTER COLUMN {colNames};"
    End Function

    ' Function to check if a table exists
    Function CheckTableExists(connection As OleDbConnection, tableName As String) As Boolean

        Try
            'Get the schema information for tables in the database
            Dim schemaTable As DataTable = connection.GetSchema("Tables")

            ' Check if the table exists in the schema information
            For Each row As DataRow In schemaTable.Rows
                If row("TABLE_NAME").ToString().Equals(tableName, StringComparison.OrdinalIgnoreCase) Then
                    Return True
                End If
            Next

            Return False
        Catch ex As Exception
            MessageBox.Show(ex.Message)
            Return False
        End Try


    End Function

    Public Sub RunDatabaseChanges()


        ' Call the function to get the array of ChangeSets
        Dim changeSets() As ChangeSet = GetDatabaseChangeSets()

        Dim existing_uuids As New Dictionary(Of String, Integer)()

        Dim ConnectionString As OleDbConnection = GetDBConnection()
        ConnectionString.Open()

        If CheckTableExists(ConnectionString, "gistchangeset") Then
            Dim da As New OleDbDataAdapter("select uuid, executed from gistchangeset;", ConnectionString)
            Dim ds As New DataSet
            da.Fill(ds)


            For Each row As DataRow In ds.Tables(0).Rows
                existing_uuids.Add(row.Item("uuid"), row.Item("executed"))
            Next
            da.Dispose()
            ds.Dispose()
        Else
            Dim strSQL As String = "CREATE TABLE gistchangeset (id AUTOINCREMENT, author TEXT(20), changesetname TEXT,  changeset LONGTEXT, changedate DateTime,dateexecuted DateTime, executed Integer, comments TEXT, uuid TEXT(36));"
            Using cmdCreateTables As New OleDbCommand(strSQL, ConnectionString)
                Try
                    cmdCreateTables.ExecuteNonQuery()

                Catch ex As Exception
                    MessageBox.Show(ex.Message)
                    MessageBox.Show($"This Changeset: {strSQL}, was not successfully Executed, contact the data team ")

                End Try
            End Using
        End If

        For Each change As ChangeSet In changeSets

            ' Display the details of each ChangeSet in the array
            Console.WriteLine(change.ToString())
            If Not existing_uuids.ContainsKey(change.Uuid) Then
                Dim rowsAffected As Integer = 0
                Dim rowsAffected_update As Integer = 0
                Dim strquery As String = "INSERT INTO gistchangeset (changedate, author, changesetname, changeset, executed, uuid, comments) VALUES (?, ?, ?, ?, ?, ?, ?)"
                Using cmdInsert As New OleDbCommand(strquery, ConnectionString)
                    Try
                        ' Set the parameter values
                        cmdInsert.Parameters.AddWithValue("@changedate", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"))
                        cmdInsert.Parameters.AddWithValue("@author", change.Author)
                        cmdInsert.Parameters.AddWithValue("@changesetname", change.Changesetname)
                        cmdInsert.Parameters.AddWithValue("@changeset", change.Changeset)
                        cmdInsert.Parameters.AddWithValue("@executed", 0)
                        cmdInsert.Parameters.AddWithValue("@uuid", change.Uuid)
                        cmdInsert.Parameters.AddWithValue("@comments", change.Comments)

                        ' Execute the command
                        rowsAffected = cmdInsert.ExecuteNonQuery()


                    Catch ex As Exception
                        MessageBox.Show(ex.Message)
                        MessageBox.Show($"This Changeset: {change.Changesetname}, was not added to the changeset table, contact the data team")

                    End Try
                End Using

                ' Check if the command executed successfully
                If rowsAffected > 0 Then
                    Dim colExists As Boolean = False
                    Using cmdInsert As New OleDbCommand(change.Changeset, ConnectionString)
                        Try
                            rowsAffected = cmdInsert.ExecuteNonQuery()
                            rowsAffected = 1

                        Catch ex As Exception
                            If ex.Message.Contains("already exists") Then
                                colExists = True

                            Else
                                rowsAffected = 0
                                MessageBox.Show(ex.Message)
                                MessageBox.Show($"This Changeset: {change.Changesetname}, was not successfully Executed, contact the data team ")
                            End If


                        End Try
                    End Using

                    If colExists Then
                        rowsAffected = UpdateChangeSetTableColExists(change.Uuid, ConnectionString)
                    End If

                End If

                'update if successfuly executed the changeset
                If rowsAffected > 0 Then
                    rowsAffected = UpdateChangeSetTable(change.Uuid, ConnectionString)
                End If

            Else
                'Check if the changeset was executed, if not, proceeed to execute
                If Not existing_uuids(change.Uuid) = 1 Then
                    Dim rowsAffected As Integer = 0
                    Dim colExists As Boolean = False
                    Using cmdInsert As New OleDbCommand(change.Changeset, ConnectionString)
                        Try
                            rowsAffected = cmdInsert.ExecuteNonQuery()
                            rowsAffected = 1
                        Catch ex As Exception
                            If ex.Message.Contains("already exists") Then
                                colExists = True

                            Else
                                rowsAffected = 0
                                MessageBox.Show(ex.Message)
                                MessageBox.Show($"This Changeset: {change.Changesetname}, was not successfully Executed, contact the data team")
                            End If
                        End Try
                    End Using

                    If rowsAffected > 0 Then
                        rowsAffected = UpdateChangeSetTable(change.Uuid, ConnectionString)
                    End If

                    If colExists Then
                        rowsAffected = UpdateChangeSetTableColExists(change.Uuid, ConnectionString)
                    End If
                End If

            End If

        Next

        ConnectionString.Close()

    End Sub

    Public Function UpdateChangeSetTable(uuid As String, ConnectionString As OleDbConnection) As Integer

        Dim rowsAffected As Integer = 0
        'update if successfuly executed the changeset
        Dim strquery As String = "UPDATE gistchangeset SET executed = @executed, dateexecuted = @dateexecuted WHERE uuid = @uuid;"

        Using cmdUpdate As New OleDbCommand(strquery, ConnectionString)
            ' Add parameters to the command
            cmdUpdate.Parameters.AddWithValue("@executed", 1)
            cmdUpdate.Parameters.AddWithValue("@dateexecuted", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"))
            cmdUpdate.Parameters.AddWithValue("@uuid", uuid)
            Try
                rowsAffected = cmdUpdate.ExecuteNonQuery()
            Catch ex As Exception
                MessageBox.Show(ex.Message)
            End Try

        End Using
        Return rowsAffected
    End Function

    Public Function UpdateChangeSetTableColExists(uuid As String, ConnectionString As OleDbConnection) As Integer

        Dim rowsAffected As Integer = 0
        'update if successfuly executed the changeset
        Dim strquery As String = "UPDATE gistchangeset SET executed = @executed, dateexecuted = @dateexecuted, comments = @comments WHERE uuid = @uuid;"

        Using cmdUpdate As New OleDbCommand(strquery, ConnectionString)
            ' Add parameters to the command
            cmdUpdate.Parameters.AddWithValue("@executed", 1)
            cmdUpdate.Parameters.AddWithValue("@dateexecuted", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"))
            cmdUpdate.Parameters.AddWithValue("@comments", "Table/Columns Already exists in the db/table")
            cmdUpdate.Parameters.AddWithValue("@uuid", uuid)
            Try
                rowsAffected = cmdUpdate.ExecuteNonQuery()
            Catch ex As Exception
                MessageBox.Show(ex.Message)
                MessageBox.Show(ex.Message)
            End Try

        End Using
        Return rowsAffected
    End Function

    Public Function Opal_SapphireIDDict(opal_id As String) As String

        Dim opal_sapphire_dict As New Dictionary(Of String, String)()

        Dim ConnectionString As OleDbConnection = GetDBConnection()
        ConnectionString.Open()

        Dim da As New OleDbDataAdapter("SELECT subjid, opal_id FROM checkin WHERE opal_id <> '- 9';", ConnectionString)
        Dim ds As New DataSet
        da.Fill(ds)


        For Each row As DataRow In ds.Tables(0).Rows
            If Not opal_sapphire_dict.ContainsKey(row.Item("opal_id")) Then
                opal_sapphire_dict.Add(row.Item("opal_id"), row.Item("subjid"))
            End If

        Next
        da.Dispose()
        ds.Dispose()
        ConnectionString.Close()

        Return opal_sapphire_dict(opal_id)
    End Function

End Module
