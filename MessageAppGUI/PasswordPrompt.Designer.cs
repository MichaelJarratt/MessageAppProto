namespace MessageAppGUI
{
    partial class PasswordPrompt
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.PasswordSubmit = new System.Windows.Forms.Button();
            this.EnterPasswordLabel = new System.Windows.Forms.Label();
            this.PasswordTextbox = new System.Windows.Forms.TextBox();
            this.PasswordHintLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // PasswordSubmit
            // 
            this.PasswordSubmit.Location = new System.Drawing.Point(327, 112);
            this.PasswordSubmit.Name = "PasswordSubmit";
            this.PasswordSubmit.Size = new System.Drawing.Size(75, 23);
            this.PasswordSubmit.TabIndex = 0;
            this.PasswordSubmit.Text = "Submit";
            this.PasswordSubmit.UseVisualStyleBackColor = true;
            this.PasswordSubmit.Click += new System.EventHandler(this.PasswordSubmit_Click);
            // 
            // EnterPasswordLabel
            // 
            this.EnterPasswordLabel.AutoSize = true;
            this.EnterPasswordLabel.Location = new System.Drawing.Point(51, 62);
            this.EnterPasswordLabel.Name = "EnterPasswordLabel";
            this.EnterPasswordLabel.Size = new System.Drawing.Size(57, 15);
            this.EnterPasswordLabel.TabIndex = 1;
            this.EnterPasswordLabel.Text = "Password";
            // 
            // PasswordTextbox
            // 
            this.PasswordTextbox.Location = new System.Drawing.Point(124, 59);
            this.PasswordTextbox.Name = "PasswordTextbox";
            this.PasswordTextbox.Size = new System.Drawing.Size(213, 23);
            this.PasswordTextbox.TabIndex = 2;
            // 
            // PasswordHintLabel
            // 
            this.PasswordHintLabel.AutoSize = true;
            this.PasswordHintLabel.Font = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.PasswordHintLabel.Location = new System.Drawing.Point(12, 138);
            this.PasswordHintLabel.Name = "PasswordHintLabel";
            this.PasswordHintLabel.Size = new System.Drawing.Size(370, 12);
            this.PasswordHintLabel.TabIndex = 3;
            this.PasswordHintLabel.Text = "*If this is your first time using the application choose a password and don\'t for" +
    "get it";
            // 
            // PasswordPrompt
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(414, 159);
            this.Controls.Add(this.PasswordHintLabel);
            this.Controls.Add(this.PasswordTextbox);
            this.Controls.Add(this.EnterPasswordLabel);
            this.Controls.Add(this.PasswordSubmit);
            this.Name = "PasswordPrompt";
            this.Text = "Enter Password";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PasswordPrompt_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button PasswordSubmit;
        private System.Windows.Forms.Label EnterPasswordLabel;
        private System.Windows.Forms.TextBox PasswordTextbox;
        private System.Windows.Forms.Label PasswordHintLabel;
    }
}