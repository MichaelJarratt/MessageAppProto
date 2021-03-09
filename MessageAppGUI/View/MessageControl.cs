using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace MessageAppGUI.View
{
    public partial class MessageControl : UserControl
    {
        //private int messageID; //ID of message in database
        private string messageText; //text message is composed of
        private bool sent; //if the message was sent by or received by the application

        public MessageControl(string messageText, bool sent)
        {
            //this.messageID = messageID;
            this.messageText = messageText;
            this.sent = sent;
            InitializeComponent();
            
            if(sent) //if it was sent by this instance
            {
                Anchor = AnchorStyles.Right; //anchor message to the right
                BackColor = Color.Blue; //sets colour
            }
            else //if it was received by this instance
            {
                Anchor = AnchorStyles.Left; //anchor message to the left
                BackColor = Color.Gray;
            }

            messageTextLabel.Text = messageText;
        }
    }
}
