using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Linq;
using MessageApp;

namespace MessageAppGUI
{
    public class GUIController
    {
        private MessageAppForm messageAppForm;
        private List<Contact> contacts = new List<Contact>();
        private ClientComp clientComp;
        private ServerComp serverComp;

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

        //loads messages exchanged with <contactID> and creates clientComponent with their IP address as the target
        public void loadMessages(int contactID)
        {
            //logic for loading messages
            //Contact contact = contacts.Select<Contact>(x => x.ID == contactID) //use lambda to get correct record
            //Contact contact = contacts.Select(contact => contact.ID == contactID).First<Contact>();
            Contact contact = contacts.ElementAt<Contact>(contactID-1); //ID is the same as position
            string contactIP = contact.getIPString(); //extract IP (encrypted in Contact)
        }
        private void setUpClient()
        {

        }
        public void sendMessage(string message)
        {
            //clientComp.sendMessage(message); //needs to be implemented
        }

        private GUIController()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            messageAppForm = new MessageAppForm(this); //controller passes itself as reference to the GUI
            Application.Run(messageAppForm);

            //logic to start server and give it callbacks
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
