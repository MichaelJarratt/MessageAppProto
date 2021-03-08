using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using MessageApp;
using Message = MessageApp.Message;
using System.Threading;

namespace MessageAppGUI
{
    public class GUIController
    {
        private MessageAppForm messageAppForm; //view
        //private List<Contact> contacts = new List<Contact>();
        private ClientComp clientComp;
        private ServerComp serverComp;
        private ContactManager contactManager;

        private Contact currentSubject; //contact who is the current focus (when a message is sent, it goes to them)

        private const int MAX_MESSAGE_LENGTH = 501; //maximum message length that can be encrypted with the chosen key length

        private delegate void MessageAppFormDisplayContactsDelegate(List<Contact> contacts);

        //creates and stores a new contact
        public void addContact(string contactName, string IP)
        {
            //storage logic
            //int ID = contacts.Count + 1; //sets ID for new contact
            //Contact newContact = new Contact(ID, contactName, IP);
            //contacts.Add(newContact); //add it to list
            contactManager.addContact(contactName, IP);
            updateDisplayedContacts(); //tell UI to display new contacts
        }
        //tells the UI to update to show contacts (updates when there's new contacts)
        public void updateDisplayedContacts()
        {
            //MethodInvoker methodDelegate = delegate () { messageAppForm.displayContacts(contactManager.contactsList); };
            //MethodInvoker methodDelegate = delegate () { messageAppForm.displayContacts(); };

            //delegate void methodDelegate(List<Contact> contacts);
            //methodDelegate?.Invoke();

            Delegate del = new MessageAppFormDisplayContactsDelegate(messageAppForm.displayContacts);
            //Control control = new Control();
            messageAppForm.Invoke(del,contactManager.contactsList);

            //Action<List<Contact>> del = messageAppForm.displayContacts;
            //del(contactManager.contactsList);

            //messageAppForm.displayContacts(contactManager.contactsList);
        }

        //loads messages exchanged with <contactID> and creates clientComponent with their IP address as the target
        public void loadMessages(int contactID)
        {
            //logic for loading messages
            //Contact contact = contacts.ElementAt<Contact>(contactID-1); //ID is the same as position
            Contact contact = contactManager.getContact(contactID); //get specified Contact from contact Manager
            string contactIP = contact.getIPString(); //extract IP (encrypted in Contact)
            setUpClient(contactIP); //creates client that will send messages to contactIP

            currentSubject = contact;
        }
        //logic to instantiate server and give it the needed callbacks
        private void setUpServer()
        {
            serverComp = new ServerComp();
            serverComp.setMessageCallback(messageReceivedCallback);
            serverComp.setReceiveErrorCallback(errorCallback);
            serverComp.startConnectionListenLoop();
        }
        //logic to instantiate client and give it the needed callbacks
        private void setUpClient(String targetIP)
        {
            clientComp = new ClientComp(targetIP); //creates client component with IP of contact
            clientComp.setSendErrorCallBack(errorCallback);
            clientComp.setSendConfirmationCallback(messageSentCallback); //callback to confirm the message was sent
        }
        //called by sendMessage in MessageAppForm
        public void sendMessage(string messageString)
        {
            if (messageString.Length < MAX_MESSAGE_LENGTH)
            {
                Message message = new Message(messageString, currentSubject.ID);
                clientComp.sendMessage(message); //tells client component to send the message
            }
            else
            {
                messageAppForm.createPopUp($"Too long, maximum allowed message length is {MAX_MESSAGE_LENGTH} characters");
            }
        }

        //
        //callbacks
        //
        //callback when server receives a message
        public void messageReceivedCallback(Message message)
        {
            Contact contact = contactManager.identifySender(message); //uses IP of sender it identify them, creates new contact if they are unknown
            //updateDisplayedContacts(); //update displayed contacts in case
            Console.WriteLine($"{contact.contactName} - IP: {contact.getIPString()}");
            string messageString = message.message;
            
            Console.WriteLine($"message received: \n{messageString}"); //print to console for now
        }
        //callback for confirming message has been send
        public void messageSentCallback(Message message)
        {
            //logic to store it
            messageAppForm.messageSentConfirmed();
        }
        //callback for when contact manager has to create a new contact for a previously unknown IP address
        public void newContactCallback()
        {
            updateDisplayedContacts();
        }
        //error callback - creates a pop up informing the user that an error of (type) has occured
        public void errorCallback(TransmissionErrorCode errorCode)
        {
            String message = "fucc";

            switch (errorCode) //IDE autogenerated all these cases for the enum, cool!
            {
                case TransmissionErrorCode.CliNoEndPointConnection:
                    message = "Could not connect to target IP.\nPerhaps your contact is offline, or the IP address is wrong.";
                    break;
                case TransmissionErrorCode.CliKeyExchangeFail:
                    message = "Error sending message.\nFailed to exchange keys";
                    break;
                case TransmissionErrorCode.CliConnectionLost:
                    message = "Error sending message.\nConnection with target lost";
                    break;
                case TransmissionErrorCode.CliTransmissionError:
                    message = "Error sending message.\nUnspecified transmission error";
                    break;
                case TransmissionErrorCode.CliNoReceiveConfirmaton:
                    message = "Error sending message.\nDid not get confirmation of message being succesfully received by client";
                    break;
                case TransmissionErrorCode.ServTotalLengthError:
                    message = "Error receiving message.\nTotal length of transmission did not match actual length";
                    break;
                case TransmissionErrorCode.ServDecOrValError:
                    message = "Error receiving message.\nException trying to decrypt or validate message";
                    break;
                case TransmissionErrorCode.ServValidationFail:
                    message = "Error receiving message.\nSignature of received message failed to validate";
                    break;
                default:
                    break;
            }

            messageAppForm.createPopUp(message);
        }



        private GUIController()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            messageAppForm = new MessageAppForm(this); //controller passes itself as reference to the GUI

            setUpServer(); //initialises server and assings needed callback functions
            contactManager = new ContactManager(); //manages creation and retrieval of contacts, as well as identifying senders of received messages
            contactManager.setNewContactCallback(newContactCallback);
            //updateDisplayedContacts(); //gets list of exisiting contacts from ContactManager and displays them in view

            Application.Run(messageAppForm); //turns out anything after this does not get executed

            //logic to start server and give it callbacks
            //setUpServer();
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
            //ipBytes = protect(ipBytes); //encrypt bytes again
            return ip;
        }
        //uses the ProtectedData API to encrypt provided byte array
        private byte[] protect(byte[] data)
        {
            return ProtectedData.Protect(data, additionalEntropy, DataProtectionScope.LocalMachine); //data encrypted in memory
        }
    }
}
