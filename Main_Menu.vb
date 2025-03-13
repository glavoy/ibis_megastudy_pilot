Public Class Main_Menu
    Private Sub ButtonBaseline_Click(sender As Object, e As EventArgs) Handles ButtonBaseline.Click
        Try
            Survey = "baseline"
            NewSurvey.ShowDialog()
            NewSurvey.Dispose()
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
End Class

