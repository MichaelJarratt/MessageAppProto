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
        private MessageManager messageManager;

        private Contact currentSubject; //contact who is the current focus (when a message is sent, it goes to them)

        private const int MAX_MESSAGE_LENGTH = 501; //maximum message length that can be encrypted with the chosen key length

        //Delegates - these are for calling methods on the MessageAppForm (call chains started by server) to prevent thread exception
        private delegate void MAFDisplayContactsDelegate(List<Contact> contacts);
        private delegate void MAFDisplayMessageDelegate(Message message);
        private delegate void MAFDisplayMessagesDelegate(List<Message> messags);

        /// <summary>
        /// Creates and stored contact with the provided information
        /// </summary>
        /// <param name="contactName">Name of contact to add</param>
        /// <param name="IP">IP address of contact to add</param>
        public void addContact(string contactName, string IP)
        {
            contactManager.addContact(contactName, IP);
            updateDisplayedContacts(); //tell UI to display new contacts
        }
        //tells the UI to update to show contacts (updates when there's new contacts)
        public void updateDisplayedContacts()
        {
            //creates a delegate function which represents the displayContacts message of messageAppForm
            Delegate del = new MAFDisplayContactsDelegate(messageAppForm.displayContacts);
            //tells the messageApp to run this on its own thread
            messageAppForm.Invoke(del,contactManager.contactsList);

            /* Why this is necessary:
             * The controller runs in the same thread as the MessageAppForm, this means it is free to alter form elements
             * However when the server (which runs in its own thread) receives a message from a new contact,
             * a chain of events results in this method being run, however because it was called from another thread, it 
             * causes exceptions when it tried to alter stuff in the form (Illegal Thread call or something)
             */
        }
        //takes a single Message as a parameter and sends to it MessageAppForm to be displayed
        private void displayMessage(Message message)
        {
            Delegate del = new MAFDisplayMessageDelegate(messageAppForm.displayMessage);
            messageAppForm.Invoke(del, message);
        }
        //takes a list of messages as a parameter and sends them to MessageAppForm to be displayed
        private void displayMessages(List<Message> messages)
        {
            Delegate del = new MAFDisplayMessagesDelegate(messageAppForm.displayMessages);
            messageAppForm.Invoke(del, messages);
        }

        //loads messages exchanged with <contactID> and creates clientComponent with their IP address as the target
        public void loadMessages(int contactID)
        {
            //contact stuff
            Contact contact = contactManager.getContact(contactID); //get specified Contact from contact Manager
            currentSubject = contact; //stores current focus of chat window as this contact
            string contactIP = contact.getIPString(); //extract IP (encrypted in Contact)
            setUpClient(contactIP); //creates client that will send messages to contactIP

            //message stuff
            List<Message> messageHistory = messageManager.getMessageHistory(contact); //gets list of all messages exchanged with this contact
            displayMessages(messageHistory); //clears previous messages in window and displays all past messages with <contact>

            ////temp
            //Message message = new Message("hello there", 1, 0);
            //displayMessage(message);
            //message = new Message("GeNeRaL KEnoBi", 0, 1);
            //displayMessage(message);
            //message = new Message("You're a bold one", 0, 1);
            //displayMessage(message);
            //message = new Message("no u", 1, 0);
            //displayMessage(message);
            //message = new Message("E\nE", 0, 1);
            //displayMessage(message);
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
            //the Message IP field is cleared and the sender field is set to the ID of the contact
            messageManager.commitMessage(message); //commit to database

            if (currentSubject != null) //if a subject is selected
            {
                if (contact.ID == currentSubject.ID) //if the contact who sent the message is the current focus on the messaging window, display the message
                {
                    displayMessage(message); //only get to this point if received succesfully, so display it
                }
            }

            //Console.WriteLine($"{contact.contactName} - IP: {contact.getIPString()}");
            //string messageString = message.message;
            
            //Console.WriteLine($"message received: \n{messageString}"); //print to console for now
        }
        //callback for confirming message has been send
        public void messageSentCallback(Message message)
        {
            messageManager.commitMessage(message); //commit to database
            messageAppForm.messageSentConfirmed(); //tells form to clear the input box
            displayMessage(message); //show the message
        }
        //callback for when contact manager has to create a new contact for a previously unknown IP address
        public void newContactCallback()
        {
            updateDisplayedContacts();
        }
        //error callback - creates a pop up informing the user that an error of (type) has occured
        public void errorCallback(TransmissionErrorCode errorCode)
        {
            String message = "oh no";

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
            messageManager = new MessageManager(); //manages storing and retrieving messages from the database

            Application.Run(messageAppForm); //turns out anything after this does not get executed
        }
        [STAThread]
        static void Main()
        {
            ManualResetEvent blockMain = new ManualResetEvent(false);
            //new PasswordPrompt(blockMain); //handles getting the users password, blocks until it has it

            Thread thread = new Thread(() =>
            {
                Application.Run(new PasswordPrompt(blockMain));
            });
            //thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            blockMain.WaitOne(); //unblocks when user submits a password
            new GUIController(); //now that Master key has been generated, start main application
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
