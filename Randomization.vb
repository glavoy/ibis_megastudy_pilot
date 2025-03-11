Imports System.Runtime.Remoting.Metadata.W3cXsd2001

Public Class Randomization_wheelspin
    Dim stopTime As Integer = 5000 ' Stop after 5 seconds (5000 ms)
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles ButtonRandomize.Click
        PictureBox1.Image = Image.FromFile("C:\IBIS_pilot\spinner\spin-12315_128.gif") ' Load your wheel image
        PictureBox1.SizeMode = PictureBoxSizeMode.StretchImage  ' Resize image to fit
        Timer1.Interval = stopTime
        Timer1.Start()
        ButtonRandomize.Enabled = False
        ButtonClose.Enabled = True
    End Sub

    Private Sub Randomization_wheelspin_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try
            PictureBox1.Image = Image.FromFile("C:\IBIS_pilot\spinner\lucky-spin.png") ' Load your wheel image
            PictureBox1.SizeMode = PictureBoxSizeMode.StretchImage  ' Resize image to fit
            ButtonClose.Enabled = False
            ButtonRandomize.Enabled = True
            Label2.Text = ""
        Catch ex As Exception
            MessageBox.Show("Error loading image: " & ex.Message)
        End Try
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles ButtonClose.Click
        Me.Close()
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Timer1.Stop()
        PictureBox1.Image = Image.FromFile("C:\IBIS_pilot\spinner\lucky-spin.png") ' Load your wheel image
        PictureBox1.SizeMode = PictureBoxSizeMode.StretchImage  ' Resize image to fit
        Label2.Text = RandArmText
    End Sub
End Class