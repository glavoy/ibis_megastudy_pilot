<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class LeaveNote
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
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.ButtonLeaveNote = New System.Windows.Forms.Button()
        Me.ButtonCancel = New System.Windows.Forms.Button()
        Me.TextBoxNote = New System.Windows.Forms.TextBox()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.2!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.Location = New System.Drawing.Point(72, 40)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(429, 25)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "You have made changes to the questionnaire"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.2!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label2.Location = New System.Drawing.Point(72, 65)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(374, 25)
        Me.Label2.TabIndex = 1
        Me.Label2.Text = "Briefly describe the changes you made"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.2!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label3.Location = New System.Drawing.Point(72, 139)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(278, 20)
        Me.Label3.TabIndex = 2
        Me.Label3.Text = "Click here when you are finished -->"
        '
        'ButtonLeaveNote
        '
        Me.ButtonLeaveNote.Location = New System.Drawing.Point(412, 139)
        Me.ButtonLeaveNote.Name = "ButtonLeaveNote"
        Me.ButtonLeaveNote.Size = New System.Drawing.Size(191, 42)
        Me.ButtonLeaveNote.TabIndex = 3
        Me.ButtonLeaveNote.Text = "Done"
        Me.ButtonLeaveNote.UseVisualStyleBackColor = True
        '
        'ButtonCancel
        '
        Me.ButtonCancel.Location = New System.Drawing.Point(469, 12)
        Me.ButtonCancel.Name = "ButtonCancel"
        Me.ButtonCancel.Size = New System.Drawing.Size(319, 47)
        Me.ButtonCancel.TabIndex = 4
        Me.ButtonCancel.Text = "I did not intend to make changes"
        Me.ButtonCancel.UseVisualStyleBackColor = True
        '
        'TextBoxNote
        '
        Me.TextBoxNote.CausesValidation = False
        Me.TextBoxNote.Location = New System.Drawing.Point(60, 210)
        Me.TextBoxNote.Multiline = True
        Me.TextBoxNote.Name = "TextBoxNote"
        Me.TextBoxNote.Size = New System.Drawing.Size(727, 203)
        Me.TextBoxNote.TabIndex = 5
        '
        'LeaveNote
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(800, 450)
        Me.Controls.Add(Me.TextBoxNote)
        Me.Controls.Add(Me.ButtonCancel)
        Me.Controls.Add(Me.ButtonLeaveNote)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Name = "LeaveNote"
        Me.Text = "Describe Changes made"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents Label1 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents Label3 As Label
    Friend WithEvents ButtonLeaveNote As Button
    Friend WithEvents ButtonCancel As Button
    Friend WithEvents TextBoxNote As TextBox
End Class
