<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class NewSurvey
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(NewSurvey))
        Me.Button_Previous = New System.Windows.Forms.Button()
        Me.Button_Next = New System.Windows.Forms.Button()
        Me.lblQuestion = New System.Windows.Forms.Label()
        Me.ButtonCancelInterview = New System.Windows.Forms.Button()
        Me.LabelHHID = New System.Windows.Forms.Label()
        Me.Button_DK = New System.Windows.Forms.Button()
        Me.Button_Refuse = New System.Windows.Forms.Button()
        Me.Button_NA = New System.Windows.Forms.Button()
        Me.ListBox_hh_members = New System.Windows.Forms.ListBox()
        Me.LabelPhone1 = New System.Windows.Forms.Label()
        Me.LabelPhone2 = New System.Windows.Forms.Label()
        Me.PictureBoxStatic = New System.Windows.Forms.PictureBox()
        Me.LabelArm = New System.Windows.Forms.Label()
        Me.PictureBoxAnimated = New System.Windows.Forms.PictureBox()
        CType(Me.PictureBoxStatic, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.PictureBoxAnimated, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'Button_Previous
        '
        Me.Button_Previous.BackColor = System.Drawing.SystemColors.Control
        Me.Button_Previous.Enabled = False
        Me.Button_Previous.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Button_Previous.Image = Global.ibis_megastudy_pilot.My.Resources.Resources.previous1
        Me.Button_Previous.Location = New System.Drawing.Point(13, 713)
        Me.Button_Previous.Margin = New System.Windows.Forms.Padding(4)
        Me.Button_Previous.Name = "Button_Previous"
        Me.Button_Previous.Size = New System.Drawing.Size(145, 111)
        Me.Button_Previous.TabIndex = 0
        Me.Button_Previous.UseVisualStyleBackColor = False
        '
        'Button_Next
        '
        Me.Button_Next.BackColor = System.Drawing.SystemColors.Control
        Me.Button_Next.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Button_Next.Image = Global.ibis_megastudy_pilot.My.Resources.Resources.next1
        Me.Button_Next.Location = New System.Drawing.Point(260, 713)
        Me.Button_Next.Margin = New System.Windows.Forms.Padding(4)
        Me.Button_Next.Name = "Button_Next"
        Me.Button_Next.Size = New System.Drawing.Size(145, 111)
        Me.Button_Next.TabIndex = 1
        Me.Button_Next.UseVisualStyleBackColor = False
        '
        'lblQuestion
        '
        Me.lblQuestion.AutoSize = True
        Me.lblQuestion.Font = New System.Drawing.Font("Arial", 20.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel, CType(0, Byte))
        Me.lblQuestion.Location = New System.Drawing.Point(16, 11)
        Me.lblQuestion.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblQuestion.MaximumSize = New System.Drawing.Size(1000, 0)
        Me.lblQuestion.Name = "lblQuestion"
        Me.lblQuestion.Size = New System.Drawing.Size(57, 24)
        Me.lblQuestion.TabIndex = 2
        Me.lblQuestion.Text = "IDRC"
        '
        'ButtonCancelInterview
        '
        Me.ButtonCancelInterview.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ButtonCancelInterview.Location = New System.Drawing.Point(1199, 13)
        Me.ButtonCancelInterview.Margin = New System.Windows.Forms.Padding(4)
        Me.ButtonCancelInterview.Name = "ButtonCancelInterview"
        Me.ButtonCancelInterview.Size = New System.Drawing.Size(145, 59)
        Me.ButtonCancelInterview.TabIndex = 3
        Me.ButtonCancelInterview.Text = "Cancel Interview"
        Me.ButtonCancelInterview.UseVisualStyleBackColor = True
        '
        'LabelHHID
        '
        Me.LabelHHID.AutoSize = True
        Me.LabelHHID.Font = New System.Drawing.Font("Arial", 20.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel, CType(0, Byte))
        Me.LabelHHID.Location = New System.Drawing.Point(1051, 96)
        Me.LabelHHID.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.LabelHHID.Name = "LabelHHID"
        Me.LabelHHID.Size = New System.Drawing.Size(191, 24)
        Me.LabelHHID.TabIndex = 4
        Me.LabelHHID.Text = "HHID: 00000000000"
        Me.LabelHHID.Visible = False
        '
        'Button_DK
        '
        Me.Button_DK.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Button_DK.Location = New System.Drawing.Point(1199, 656)
        Me.Button_DK.Margin = New System.Windows.Forms.Padding(4)
        Me.Button_DK.Name = "Button_DK"
        Me.Button_DK.Size = New System.Drawing.Size(145, 59)
        Me.Button_DK.TabIndex = 5
        Me.Button_DK.Text = "Don't Know"
        Me.Button_DK.UseVisualStyleBackColor = True
        Me.Button_DK.Visible = False
        '
        'Button_Refuse
        '
        Me.Button_Refuse.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Button_Refuse.Location = New System.Drawing.Point(1199, 590)
        Me.Button_Refuse.Margin = New System.Windows.Forms.Padding(4)
        Me.Button_Refuse.Name = "Button_Refuse"
        Me.Button_Refuse.Size = New System.Drawing.Size(145, 59)
        Me.Button_Refuse.TabIndex = 6
        Me.Button_Refuse.Text = "Refuse to Answer"
        Me.Button_Refuse.UseVisualStyleBackColor = True
        Me.Button_Refuse.Visible = False
        '
        'Button_NA
        '
        Me.Button_NA.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Button_NA.Location = New System.Drawing.Point(1199, 523)
        Me.Button_NA.Margin = New System.Windows.Forms.Padding(4)
        Me.Button_NA.Name = "Button_NA"
        Me.Button_NA.Size = New System.Drawing.Size(145, 59)
        Me.Button_NA.TabIndex = 7
        Me.Button_NA.Text = "Not Applicable"
        Me.Button_NA.UseVisualStyleBackColor = True
        Me.Button_NA.Visible = False
        '
        'ListBox_hh_members
        '
        Me.ListBox_hh_members.BackColor = System.Drawing.SystemColors.Menu
        Me.ListBox_hh_members.Enabled = False
        Me.ListBox_hh_members.FormattingEnabled = True
        Me.ListBox_hh_members.ItemHeight = 16
        Me.ListBox_hh_members.Location = New System.Drawing.Point(1055, 191)
        Me.ListBox_hh_members.Margin = New System.Windows.Forms.Padding(4)
        Me.ListBox_hh_members.Name = "ListBox_hh_members"
        Me.ListBox_hh_members.Size = New System.Drawing.Size(289, 324)
        Me.ListBox_hh_members.TabIndex = 8
        Me.ListBox_hh_members.Visible = False
        '
        'LabelPhone1
        '
        Me.LabelPhone1.AutoSize = True
        Me.LabelPhone1.Font = New System.Drawing.Font("Arial", 20.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel, CType(0, Byte))
        Me.LabelPhone1.Location = New System.Drawing.Point(1051, 125)
        Me.LabelPhone1.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.LabelPhone1.Name = "LabelPhone1"
        Me.LabelPhone1.Size = New System.Drawing.Size(77, 24)
        Me.LabelPhone1.TabIndex = 9
        Me.LabelPhone1.Text = "Phone:"
        Me.LabelPhone1.Visible = False
        '
        'LabelPhone2
        '
        Me.LabelPhone2.AutoSize = True
        Me.LabelPhone2.Font = New System.Drawing.Font("Arial", 20.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel, CType(0, Byte))
        Me.LabelPhone2.Location = New System.Drawing.Point(1051, 149)
        Me.LabelPhone2.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.LabelPhone2.Name = "LabelPhone2"
        Me.LabelPhone2.Size = New System.Drawing.Size(77, 24)
        Me.LabelPhone2.TabIndex = 10
        Me.LabelPhone2.Text = "Phone:"
        Me.LabelPhone2.Visible = False
        '
        'PictureBoxStatic
        '
        Me.PictureBoxStatic.Image = Global.ibis_megastudy_pilot.My.Resources.Resources.lucky_spin
        Me.PictureBoxStatic.Location = New System.Drawing.Point(92, 77)
        Me.PictureBoxStatic.Name = "PictureBoxStatic"
        Me.PictureBoxStatic.Size = New System.Drawing.Size(600, 370)
        Me.PictureBoxStatic.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        Me.PictureBoxStatic.TabIndex = 11
        Me.PictureBoxStatic.TabStop = False
        '
        'LabelArm
        '
        Me.LabelArm.AutoSize = True
        Me.LabelArm.Font = New System.Drawing.Font("Arial", 20.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel, CType(0, Byte))
        Me.LabelArm.Location = New System.Drawing.Point(107, 625)
        Me.LabelArm.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.LabelArm.Name = "LabelArm"
        Me.LabelArm.Size = New System.Drawing.Size(193, 24)
        Me.LabelArm.TabIndex = 12
        Me.LabelArm.Text = "Randomization Arm"
        Me.LabelArm.Visible = False
        '
        'PictureBoxAnimated
        '
        Me.PictureBoxAnimated.Image = Global.ibis_megastudy_pilot.My.Resources.Resources.spin_12315_128
        Me.PictureBoxAnimated.Location = New System.Drawing.Point(196, 68)
        Me.PictureBoxAnimated.Name = "PictureBoxAnimated"
        Me.PictureBoxAnimated.Size = New System.Drawing.Size(389, 406)
        Me.PictureBoxAnimated.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        Me.PictureBoxAnimated.TabIndex = 13
        Me.PictureBoxAnimated.TabStop = False
        '
        'NewSurvey
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1371, 838)
        Me.ControlBox = False
        Me.Controls.Add(Me.PictureBoxAnimated)
        Me.Controls.Add(Me.LabelArm)
        Me.Controls.Add(Me.PictureBoxStatic)
        Me.Controls.Add(Me.LabelPhone2)
        Me.Controls.Add(Me.LabelPhone1)
        Me.Controls.Add(Me.ListBox_hh_members)
        Me.Controls.Add(Me.Button_NA)
        Me.Controls.Add(Me.Button_Refuse)
        Me.Controls.Add(Me.Button_DK)
        Me.Controls.Add(Me.LabelHHID)
        Me.Controls.Add(Me.ButtonCancelInterview)
        Me.Controls.Add(Me.lblQuestion)
        Me.Controls.Add(Me.Button_Next)
        Me.Controls.Add(Me.Button_Previous)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.Name = "NewSurvey"
        Me.Text = "--"
        Me.WindowState = System.Windows.Forms.FormWindowState.Maximized
        CType(Me.PictureBoxStatic, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.PictureBoxAnimated, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Button_Previous As System.Windows.Forms.Button
    Friend WithEvents Button_Next As System.Windows.Forms.Button
    Friend WithEvents lblQuestion As System.Windows.Forms.Label
    Friend WithEvents ButtonCancelInterview As System.Windows.Forms.Button
    Friend WithEvents LabelHHID As System.Windows.Forms.Label
    Friend WithEvents Button_DK As System.Windows.Forms.Button
    Friend WithEvents Button_Refuse As System.Windows.Forms.Button
    Friend WithEvents Button_NA As System.Windows.Forms.Button
    Friend WithEvents ListBox_hh_members As System.Windows.Forms.ListBox
    Friend WithEvents LabelPhone1 As Label
    Friend WithEvents LabelPhone2 As Label
    Friend WithEvents PictureBoxStatic As PictureBox
    Friend WithEvents LabelArm As Label
    Friend WithEvents PictureBoxAnimated As PictureBox
End Class
