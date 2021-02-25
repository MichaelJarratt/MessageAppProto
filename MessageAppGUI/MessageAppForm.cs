using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MessageAppGUI
{
    public partial class MessageAppForm : Form
    {
        private bool addContactPanelHidden = true; //hidden by default

        public MessageAppForm()
        {
            InitializeComponent();
            addContactsPanel.Hide(); //start with this panel hidden
        }

        public void hideAddContactsPanel()
        {
            //addContactsPanel.Enabled = false;
            addContactsPanel.Hide();
            addContactPanelHidden = true;
        }
        public void showAddContactsPanel()
        {
            //addContactsPanel.Enabled = true;
            addContactsPanel.Show();
            addContactPanelHidden = false;
        }
        public void toggleHideContactsPanel()
        {
            if (addContactPanelHidden==false)
            {
                hideAddContactsPanel();
            }
            else
            {
                showAddContactsPanel();
            }
        }

        private void toolTip1_Popup(object sender, PopupEventArgs e)
        {
            //manages tool tips, this reference is needed for some reason
        }

        //toggles visbility of panel
        private void contactAddButton_Click(object sender, EventArgs e)
        {
            toggleHideContactsPanel(); 
        }
    }
}
