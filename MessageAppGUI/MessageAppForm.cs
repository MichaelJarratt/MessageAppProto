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
        GUIController controller; //reference to the controller for this UI
        private bool addContactPanelHidden = true; //hidden by default

        public MessageAppForm(GUIController controller)
        {
            InitializeComponent();
            addContactsPanel.Hide(); //start with this panel hidden
            messageParentPanel.Hide(); //starts hidden as well
            this.controller = controller;
        }

        //form controls (called by controller) //

        //takes a list of contacts and creates panels to display them in the contactsPanel
        public void displayContacts(List<Contact> contacts)
        {
            int contactControlCount = 0;
            contactsPanel.Controls.Clear(); //clears and then recreates
            foreach (Contact contact in contacts) //iterate over contact objects
            {
                ContactControl newContact = new ContactControl(contact.ID, contact.contactName);
                newContact.Click += new EventHandler(contactPanelClicked); //sets event handler

                newContact.Size = new Size(300, 100);

                newContact.Location = new Point(30, contactControlCount * 110); //moves each successive control down so they don't overlap
                contactControlCount++;

                contactsPanel.Controls.Add(newContact);
            }
        }
        //displays the message history of a contact
        public void loadContact()
        {

        }

        //!form controls (called by controller) //

        //Form controls (local)//
        private void hideAddContactsPanel()
        {
            //addContactsPanel.Enabled = false;
            addContactsPanel.Hide();
            addContactPanelHidden = true;
        }
        private void showAddContactsPanel()
        {
            messageParentPanel.Hide(); //if it's not hidden, it will hide it
            //addContactsPanel.Enabled = true;
            addContactsPanel.Show();
            addContactPanelHidden = false;
        }
        private void toggleHideContactsPanel()
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
        //gets information needed to create new contact and passes it to the controller
        private void createNewContact()
        {
            string contactName = contactNameTextbox.Text; //get name from input
            string contactIP = contactIPTextbox.Text;
            controller.addContact(contactName, contactIP); //gives information to controller
        }
        //!form controls (local)//

        //form events//
        private void toolTip1_Popup(object sender, PopupEventArgs e)
        {
            //manages tool tips, this reference is needed for some reason
        }

        //toggles visbility of panel
        private void contactAddButton_Click(object sender, EventArgs e)
        {
            toggleHideContactsPanel(); 
        }

        private void contactAddFinishButton_Click(object sender, EventArgs e)
        {
            createNewContact();
        }
        //handles when a ContactControl is clicked, the controller will load the message history
        public void contactPanelClicked(object sender, EventArgs e)
        {
            ContactControl contact = (ContactControl)sender; //typecast sender
            int contactID = contact.ID; //get iD from control
            //send ID to controller
            Console.WriteLine($"contact clicked: ID {contactID}");
        }
    }
}
