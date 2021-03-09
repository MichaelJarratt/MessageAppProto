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
                //Anchor = AnchorStyles.Right; //anchor message to the right
                BackColor = Color.AliceBlue; //sets colour
            }
            else //if it was received by this instance
            {
                Anchor = AnchorStyles.Left; //anchor message to the left
                BackColor = Color.Gray;
            }
            messageTextLabel.Text = messageText;

            this.Width = 400;
            this.Height = messageTextLabel.Height + 10; //adjust height to contents
        }

        //runs when it is made a child of messageDisplayPanel, Parent is now set.
        //depending on <sent> it will position itself accordingly
        private void MessageControl_Load(object sender, EventArgs e)
        {
            int y = Location.Y; //location was set by MessageAppForm, which must be kept (so messages don't overlap)
            if(sent) //needs to be on the right
            {
                Location = new Point(Parent.Width - (this.Width + 5), y); //align right side with right side of parent, with 5px padding
            }
            else //needs to be on the left
            {
                Location = new Point(5, y); //positions itself with 5 px padding from the left
            }
        }
    }
}
