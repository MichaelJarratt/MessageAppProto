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
            messageTextLabel.MaximumSize = new Size(400, 0); //0 means no restriction, so the label can be as long as needed, but not exceed the width of the control (which overrides setting max width of control)
            messageTextLabel.Text = messageText;

            //this.MaximumSize = new Size(10000, 400); //I just don't want it to stretch too wide
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
                //Location = new Point(Parent.Width - (this.Width + 10), y); //align right side with right side of parent, with 5px padding
                Location = new Point(866 - (this.Width + 10), y); //align right side with right side of parent, with 5px padding
                //magic number, but it prevents the panel expanding when the scrollbar is added and making new messages move
                //more and more to the right because I can't use a right anchor
            }
            else //needs to be on the left
            {
                Location = new Point(5, y); //positions itself with 5 px padding from the left
            }
        }
    }
}
