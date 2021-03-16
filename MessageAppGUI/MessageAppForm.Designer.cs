namespace MessageAppGUI
{
    partial class MessageAppForm
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
            this.components = new System.ComponentModel.Container();
            this.bindingSource1 = new System.Windows.Forms.BindingSource(this.components);
            this.contactsPanel = new System.Windows.Forms.Panel();
            this.contactsLabel = new System.Windows.Forms.Label();
            this.contactAddButton = new System.Windows.Forms.Button();
            this.addContactsPanel = new System.Windows.Forms.Panel();
            this.contactAddFinishButton = new System.Windows.Forms.Button();
            this.contactIPTextbox = new System.Windows.Forms.TextBox();
            this.contactNameTextbox = new System.Windows.Forms.TextBox();
            this.iPLabel = new System.Windows.Forms.Label();
            this.nameLabel = new System.Windows.Forms.Label();
            this.addContactsLabel = new System.Windows.Forms.Label();
            this.toolTipHandler = new System.Windows.Forms.ToolTip(this.components);
            this.messageParentPanel = new System.Windows.Forms.Panel();
            this.messageDisplayPanel = new System.Windows.Forms.Panel();
            this.messagingPanel = new System.Windows.Forms.Panel();
            this.messageSendButton = new System.Windows.Forms.Button();
            this.messageTextBox = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).BeginInit();
            this.addContactsPanel.SuspendLayout();
            this.messageParentPanel.SuspendLayout();
            this.messagingPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // contactsPanel
            // 
            this.contactsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.contactsPanel.AutoScroll = true;
            this.contactsPanel.BackColor = System.Drawing.Color.Silver;
            this.contactsPanel.Location = new System.Drawing.Point(0, 54);
            this.contactsPanel.Name = "contactsPanel";
            this.contactsPanel.Size = new System.Drawing.Size(360, 499);
            this.contactsPanel.TabIndex = 0;
            // 
            // contactsLabel
            // 
            this.contactsLabel.BackColor = System.Drawing.Color.DarkGray;
            this.contactsLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.contactsLabel.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.contactsLabel.Location = new System.Drawing.Point(0, 2);
            this.contactsLabel.Name = "contactsLabel";
            this.contactsLabel.Size = new System.Drawing.Size(173, 49);
            this.contactsLabel.TabIndex = 1;
            this.contactsLabel.Text = "Contacts";
            this.contactsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // contactAddButton
            // 
            this.contactAddButton.BackColor = System.Drawing.Color.DarkGray;
            this.contactAddButton.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.contactAddButton.Location = new System.Drawing.Point(175, 3);
            this.contactAddButton.Name = "contactAddButton";
            this.contactAddButton.Size = new System.Drawing.Size(187, 48);
            this.contactAddButton.TabIndex = 2;
            this.contactAddButton.Text = "Add";
            this.contactAddButton.UseVisualStyleBackColor = false;
            this.contactAddButton.Click += new System.EventHandler(this.contactAddButton_Click);
            // 
            // addContactsPanel
            // 
            this.addContactsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.addContactsPanel.BackColor = System.Drawing.Color.Silver;
            this.addContactsPanel.Controls.Add(this.contactAddFinishButton);
            this.addContactsPanel.Controls.Add(this.contactIPTextbox);
            this.addContactsPanel.Controls.Add(this.contactNameTextbox);
            this.addContactsPanel.Controls.Add(this.iPLabel);
            this.addContactsPanel.Controls.Add(this.nameLabel);
            this.addContactsPanel.Controls.Add(this.addContactsLabel);
            this.addContactsPanel.Location = new System.Drawing.Point(366, 0);
            this.addContactsPanel.Name = "addContactsPanel";
            this.addContactsPanel.Size = new System.Drawing.Size(868, 553);
            this.addContactsPanel.TabIndex = 3;
            // 
            // contactAddFinishButton
            // 
            this.contactAddFinishButton.BackColor = System.Drawing.Color.DarkGray;
            this.contactAddFinishButton.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.contactAddFinishButton.Location = new System.Drawing.Point(314, 367);
            this.contactAddFinishButton.Name = "contactAddFinishButton";
            this.contactAddFinishButton.Size = new System.Drawing.Size(187, 48);
            this.contactAddFinishButton.TabIndex = 2;
            this.contactAddFinishButton.Text = "Add";
            this.contactAddFinishButton.UseVisualStyleBackColor = false;
            this.contactAddFinishButton.Click += new System.EventHandler(this.contactAddFinishButton_Click);
            // 
            // contactIPTextbox
            // 
            this.contactIPTextbox.Location = new System.Drawing.Point(350, 285);
            this.contactIPTextbox.Name = "contactIPTextbox";
            this.contactIPTextbox.Size = new System.Drawing.Size(100, 23);
            this.contactIPTextbox.TabIndex = 1;
            this.toolTipHandler.SetToolTip(this.contactIPTextbox, "Contact\'s IP Address");
            // 
            // contactNameTextbox
            // 
            this.contactNameTextbox.Location = new System.Drawing.Point(350, 153);
            this.contactNameTextbox.Name = "contactNameTextbox";
            this.contactNameTextbox.Size = new System.Drawing.Size(100, 23);
            this.contactNameTextbox.TabIndex = 1;
            this.toolTipHandler.SetToolTip(this.contactNameTextbox, "Name of contact");
            // 
            // iPLabel
            // 
            this.iPLabel.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.iPLabel.Location = new System.Drawing.Point(146, 285);
            this.iPLabel.Name = "iPLabel";
            this.iPLabel.Size = new System.Drawing.Size(49, 48);
            this.iPLabel.TabIndex = 0;
            this.iPLabel.Text = "IP:";
            this.toolTipHandler.SetToolTip(this.iPLabel, "Contact\'s IP address");
            // 
            // nameLabel
            // 
            this.nameLabel.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.nameLabel.Location = new System.Drawing.Point(97, 138);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Size = new System.Drawing.Size(98, 48);
            this.nameLabel.TabIndex = 0;
            this.nameLabel.Text = "Name:";
            this.toolTipHandler.SetToolTip(this.nameLabel, "Name of contact");
            // 
            // addContactsLabel
            // 
            this.addContactsLabel.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.addContactsLabel.Location = new System.Drawing.Point(324, 20);
            this.addContactsLabel.Name = "addContactsLabel";
            this.addContactsLabel.Size = new System.Drawing.Size(177, 48);
            this.addContactsLabel.TabIndex = 0;
            this.addContactsLabel.Text = "Add Contact";
            // 
            // toolTipHandler
            // 
            this.toolTipHandler.Popup += new System.Windows.Forms.PopupEventHandler(this.toolTip1_Popup);
            // 
            // messageParentPanel
            // 
            this.messageParentPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.messageParentPanel.BackColor = System.Drawing.Color.Silver;
            this.messageParentPanel.Controls.Add(this.messageDisplayPanel);
            this.messageParentPanel.Controls.Add(this.messagingPanel);
            this.messageParentPanel.Location = new System.Drawing.Point(366, 0);
            this.messageParentPanel.Name = "messageParentPanel";
            this.messageParentPanel.Size = new System.Drawing.Size(868, 553);
            this.messageParentPanel.TabIndex = 0;
            // 
            // messageDisplayPanel
            // 
            this.messageDisplayPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.messageDisplayPanel.AutoScroll = true;
            this.messageDisplayPanel.Location = new System.Drawing.Point(2, 12);
            this.messageDisplayPanel.Name = "messageDisplayPanel";
            this.messageDisplayPanel.Size = new System.Drawing.Size(866, 417);
            this.messageDisplayPanel.TabIndex = 1;
            // 
            // messagingPanel
            // 
            this.messagingPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.messagingPanel.BackColor = System.Drawing.Color.LightGray;
            this.messagingPanel.Controls.Add(this.messageSendButton);
            this.messagingPanel.Controls.Add(this.messageTextBox);
            this.messagingPanel.Location = new System.Drawing.Point(0, 435);
            this.messagingPanel.Name = "messagingPanel";
            this.messagingPanel.Size = new System.Drawing.Size(868, 118);
            this.messagingPanel.TabIndex = 0;
            // 
            // messageSendButton
            // 
            this.messageSendButton.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.messageSendButton.Location = new System.Drawing.Point(737, 33);
            this.messageSendButton.Name = "messageSendButton";
            this.messageSendButton.Size = new System.Drawing.Size(75, 61);
            this.messageSendButton.TabIndex = 1;
            this.messageSendButton.Text = "Send";
            this.messageSendButton.UseVisualStyleBackColor = true;
            this.messageSendButton.Click += new System.EventHandler(this.sendMessageButton_Click);
            // 
            // messageTextBox
            // 
            this.messageTextBox.Location = new System.Drawing.Point(48, 22);
            this.messageTextBox.Multiline = true;
            this.messageTextBox.Name = "messageTextBox";
            this.messageTextBox.Size = new System.Drawing.Size(625, 83);
            this.messageTextBox.TabIndex = 0;
            // 
            // MessageAppForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1233, 552);
            this.Controls.Add(this.messageParentPanel);
            this.Controls.Add(this.addContactsPanel);
            this.Controls.Add(this.contactAddButton);
            this.Controls.Add(this.contactsLabel);
            this.Controls.Add(this.contactsPanel);
            this.Name = "MessageAppForm";
            this.Text = "Secure Messaging";
            this.Load += new System.EventHandler(this.MessageAppForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).EndInit();
            this.addContactsPanel.ResumeLayout(false);
            this.addContactsPanel.PerformLayout();
            this.messageParentPanel.ResumeLayout(false);
            this.messagingPanel.ResumeLayout(false);
            this.messagingPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.BindingSource bindingSource1;
        private System.Windows.Forms.Panel contactsPanel;
        private System.Windows.Forms.Panel addContactsPanel;
        private System.Windows.Forms.Label addContactsLabel;
        private System.Windows.Forms.Label nameLabel;
        private System.Windows.Forms.Label iPLabel;
        private System.Windows.Forms.Label contactsLabel;
        private System.Windows.Forms.Button contactAddButton;
       // private System.Windows.Forms.Panel contactsPanel;

        //private System.Windows.Forms.Panel ContactsPanel;
        //private System.Windows.Forms.Panel addContactsPanel;
        //private System.Windows.Forms.TextBox textBox1;
        //private System.Windows.Forms.Label iPLabel;
        //private System.Windows.Forms.Label nameLabel;
        //private System.Windows.Forms.Label addContactsLabel;
        private System.Windows.Forms.ToolTip toolTipHandler;
        private System.Windows.Forms.TextBox contactIPTextbox;
        private System.Windows.Forms.TextBox contactNameTextbox;
        private System.Windows.Forms.Button contactAddFinishButton;
        private System.Windows.Forms.Panel messageParentPanel;
        private System.Windows.Forms.Panel messagingPanel;
        private System.Windows.Forms.TextBox messageTextBox;
        private System.Windows.Forms.Button messageSendButton;
        private System.Windows.Forms.Panel messageDisplayPanel;
        //private System.Windows.Forms.Button contactAddButton;
        //private System.Windows.Forms.Button ContactAddButton;
    }
}