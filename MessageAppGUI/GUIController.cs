using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;

namespace MessageAppGUI
{
    public class GUIController
    {
        private MessageAppForm messageAppForm;
        private List<Contact> contacts = new List<Contact>();

        //creates and stores a new contact
        public void addContact(string contactName, string IP)
        {
            //storage logic
            int ID = contacts.Count + 1; //sets ID for new contact
            Contact newContact = new Contact(ID, contactName, IP);
            contacts.Add(newContact); //add it to list
            updateDisplayedContacts(); //tell UI to display new contacts
        }
        //tells the UI to update to show contacts (updates when there's new contacts)
        private void updateDisplayedContacts()
        {
            messageAppForm.displayContacts(contacts);
        }

        public void loadMessages()
        {
            //logic for loading messages
        }

        private GUIController()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            messageAppForm = new MessageAppForm(this); //controller passes itself as reference to the GUI
            Application.Run(messageAppForm);
        }
        [STAThread]
        static void Main()
        {
            //Application.SetHighDpiMode(HighDpiMode.SystemAware);
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new MessageAppForm());
            new GUIController();
        }
   }



    //memory store of contact
    public class Contact
    {
        public int ID { get; }
        public string contactName { get; } //can be publically get but not set
        private byte[] ipBytes; //must be converted to bytes to encrypt
        private byte[] additionalEntropy = { 10, 5, 67, 1 }; //used in some way to protect data

        public Contact(int ID, string contactName, string IP)
        {
            this.ID = ID;
            this.contactName = contactName;
            this.ipBytes = Encoding.UTF8.GetBytes(IP); //convert IP string to bytes

            ipBytes = protect(ipBytes);
        }
        //decrypts the string, returns it, then encrypts it again
        public String getIPString()
        {
            //decrypt bytes
            byte[] decryptedBytes = ProtectedData.Unprotect(ipBytes, additionalEntropy, DataProtectionScope.LocalMachine);
            string ip = Encoding.UTF8.GetString(decryptedBytes); //convert to string to return
            ipBytes = protect(ipBytes); //encrypt bytes again
            return ip;
        }
        //uses the ProtectedData API to encrypt provided byte array
        private byte[] protect(byte[] data)
        {
            return ProtectedData.Protect(data, additionalEntropy, DataProtectionScope.LocalMachine); //data encrypted in memory
        }
    }
}
