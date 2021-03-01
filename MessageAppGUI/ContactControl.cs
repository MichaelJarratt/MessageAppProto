using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace MessageAppGUI
{
    //this class is the control that represents a contact, when clicked its information (ID) is used to get the information
    //needed to exchange messages with another application instance
    public partial class ContactControl : UserControl
    {
        public int ID;
        public string contactName;
        public string ip; //set when user clicks on this control

        public ContactControl(int ID, string contactName)
        {
            this.ID = ID;
            this.contactName = contactName;
            InitializeComponent();

            //set text boxes
            contactNameLabel.Text = contactName;
            contactIPLabel.Text = "[IP hidden]";
        }
    }
}
