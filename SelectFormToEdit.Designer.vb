<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class SelectFormToEdit
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
        Me.Button_Cancel = New System.Windows.Forms.Button()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.RadioButton_ScrnID = New System.Windows.Forms.RadioButton()
        Me.RadioButton_studyid = New System.Windows.Forms.RadioButton()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.TextBoxSubjid = New System.Windows.Forms.TextBox()
        Me.Button_Verify = New System.Windows.Forms.Button()
        Me.ButtonGO = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'Button_Cancel
        '
        Me.Button_Cancel.Location = New System.Drawing.Point(35, 24)
        Me.Button_Cancel.Name = "Button_Cancel"
        Me.Button_Cancel.Size = New System.Drawing.Size(160, 38)
        Me.Button_Cancel.TabIndex = 0
        Me.Button_Cancel.Text = "Cancel"
        Me.Button_Cancel.UseVisualStyleBackColor = True
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 13.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.Location = New System.Drawing.Point(247, 89)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(336, 36)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "Screening and Baseline"
        '
        'RadioButton_ScrnID
        '
        Me.RadioButton_ScrnID.AutoSize = True
        Me.RadioButton_ScrnID.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RadioButton_ScrnID.Location = New System.Drawing.Point(270, 154)
        Me.RadioButton_ScrnID.Name = "RadioButton_ScrnID"
        Me.RadioButton_ScrnID.Size = New System.Drawing.Size(242, 29)
        Me.RadioButton_ScrnID.TabIndex = 2
        Me.RadioButton_ScrnID.TabStop = True
        Me.RadioButton_ScrnID.Text = "Search By Screening ID"
        Me.RadioButton_ScrnID.UseVisualStyleBackColor = True
        '
        'RadioButton_studyid
        '
        Me.RadioButton_studyid.AutoSize = True
        Me.RadioButton_studyid.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RadioButton_studyid.Location = New System.Drawing.Point(270, 196)
        Me.RadioButton_studyid.Name = "RadioButton_studyid"
        Me.RadioButton_studyid.Size = New System.Drawing.Size(246, 29)
        Me.RadioButton_studyid.TabIndex = 3
        Me.RadioButton_studyid.TabStop = True
        Me.RadioButton_studyid.Text = "Search By IBIS Study ID"
        Me.RadioButton_studyid.UseVisualStyleBackColor = True
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label2.Location = New System.Drawing.Point(256, 254)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(103, 31)
        Me.Label2.TabIndex = 4
        Me.Label2.Text = "Enter ID"
        '
        'TextBoxSubjid
        '
        Me.TextBoxSubjid.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TextBoxSubjid.Location = New System.Drawing.Point(235, 297)
        Me.TextBoxSubjid.Name = "TextBoxSubjid"
        Me.TextBoxSubjid.Size = New System.Drawing.Size(305, 30)
        Me.TextBoxSubjid.TabIndex = 5
        '
        'Button_Verify
        '
        Me.Button_Verify.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Button_Verify.Location = New System.Drawing.Point(576, 288)
        Me.Button_Verify.Name = "Button_Verify"
        Me.Button_Verify.Size = New System.Drawing.Size(165, 39)
        Me.Button_Verify.TabIndex = 6
        Me.Button_Verify.Text = "Verify ID"
        Me.Button_Verify.UseVisualStyleBackColor = True
        '
        'ButtonGO
        '
        Me.ButtonGO.Enabled = False
        Me.ButtonGO.Location = New System.Drawing.Point(294, 371)
        Me.ButtonGO.Name = "ButtonGO"
        Me.ButtonGO.Size = New System.Drawing.Size(149, 45)
        Me.ButtonGO.TabIndex = 7
        Me.ButtonGO.Text = "Go"
        Me.ButtonGO.UseVisualStyleBackColor = True
        '
        'SelectFormToEdit
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(800, 450)
        Me.Controls.Add(Me.ButtonGO)
        Me.Controls.Add(Me.Button_Verify)
        Me.Controls.Add(Me.TextBoxSubjid)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.RadioButton_studyid)
        Me.Controls.Add(Me.RadioButton_ScrnID)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.Button_Cancel)
        Me.Name = "SelectFormToEdit"
        Me.Text = "SelectFormToEdit"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents Button_Cancel As Button
    Friend WithEvents Label1 As Label
    Friend WithEvents RadioButton_ScrnID As RadioButton
    Friend WithEvents RadioButton_studyid As RadioButton
    Friend WithEvents Label2 As Label
    Friend WithEvents TextBoxSubjid As TextBox
    Friend WithEvents Button_Verify As Button
    Friend WithEvents ButtonGO As Button
End Class
