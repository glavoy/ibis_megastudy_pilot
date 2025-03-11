<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Randomization_wheelspin
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
        Me.components = New System.ComponentModel.Container()
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
        Me.ButtonClose = New System.Windows.Forms.Button()
        Me.ButtonRandomize = New System.Windows.Forms.Button()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'PictureBox1
        '
        Me.PictureBox1.Location = New System.Drawing.Point(38, 33)
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.Size = New System.Drawing.Size(380, 255)
        Me.PictureBox1.TabIndex = 0
        Me.PictureBox1.TabStop = False
        '
        'Timer1
        '
        '
        'ButtonClose
        '
        Me.ButtonClose.Location = New System.Drawing.Point(258, 353)
        Me.ButtonClose.Name = "ButtonClose"
        Me.ButtonClose.Size = New System.Drawing.Size(160, 54)
        Me.ButtonClose.TabIndex = 1
        Me.ButtonClose.Text = "Close/Quit"
        Me.ButtonClose.UseVisualStyleBackColor = True
        '
        'ButtonRandomize
        '
        Me.ButtonRandomize.Location = New System.Drawing.Point(38, 359)
        Me.ButtonRandomize.Name = "ButtonRandomize"
        Me.ButtonRandomize.Size = New System.Drawing.Size(155, 48)
        Me.ButtonRandomize.TabIndex = 2
        Me.ButtonRandomize.Text = "Start"
        Me.ButtonRandomize.UseVisualStyleBackColor = True
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 13.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.Location = New System.Drawing.Point(438, 43)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(309, 29)
        Me.Label1.TabIndex = 3
        Me.Label1.Text = "Your Randomization Arm is:"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Font = New System.Drawing.Font("Microsoft Sans Serif", 13.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label2.Location = New System.Drawing.Point(444, 90)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(86, 29)
        Me.Label2.TabIndex = 4
        Me.Label2.Text = "Label2"
        '
        'Randomization_wheelspin
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.ClientSize = New System.Drawing.Size(860, 457)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.ButtonRandomize)
        Me.Controls.Add(Me.ButtonClose)
        Me.Controls.Add(Me.PictureBox1)
        Me.Name = "Randomization_wheelspin"
        Me.Text = "Select Randomization Arm"
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents PictureBox1 As PictureBox
    Friend WithEvents Timer1 As Timer
    Friend WithEvents ButtonClose As Button
    Friend WithEvents ButtonRandomize As Button
    Friend WithEvents Label1 As Label
    Friend WithEvents Label2 As Label
End Class
