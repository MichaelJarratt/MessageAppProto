namespace MessageAppGUI
{
    partial class ContactControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.contactNameLabel = new System.Windows.Forms.Label();
            this.contactIPLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // contactNameLabel
            // 
            this.contactNameLabel.AutoSize = true;
            this.contactNameLabel.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.contactNameLabel.Location = new System.Drawing.Point(105, 32);
            this.contactNameLabel.Name = "contactNameLabel";
            this.contactNameLabel.Size = new System.Drawing.Size(50, 20);
            this.contactNameLabel.TabIndex = 1;
            this.contactNameLabel.Text = "label1";
            // 
            // contactIPLabel
            // 
            this.contactIPLabel.AutoSize = true;
            this.contactIPLabel.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.contactIPLabel.Location = new System.Drawing.Point(105, 75);
            this.contactIPLabel.Name = "contactIPLabel";
            this.contactIPLabel.Size = new System.Drawing.Size(50, 20);
            this.contactIPLabel.TabIndex = 1;
            this.contactIPLabel.Text = "label1";
            // 
            // ContactControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.contactIPLabel);
            this.Controls.Add(this.contactNameLabel);
            this.Name = "ContactControl";
            this.Size = new System.Drawing.Size(257, 141);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label contactNameLabel;
        private System.Windows.Forms.Label contactIPLabel;
    }
}
