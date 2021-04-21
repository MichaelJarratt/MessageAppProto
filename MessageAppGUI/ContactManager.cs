using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SQLite;
using MessageApp;

namespace MessageAppGUI
{
    /// <summary>
    /// This class is responsible for anything to do with contacts. This includes identifying message senders, creating contacts,
    /// and maintaining a list for use by the controller
    /// </summary>
    class ContactManager
    {
        //private const string DB_NAME = "MessageAppDB"; //name of database the application will work with
        DatabaseInterface db;

        public List<Contact> contactsList { get; } = new List<Contact>();//keeps list of contacts in memory

        Action newContactControllerCallback;

        public ContactManager()
        {
            db = DatabaseInterface.getInstance(Globals.DB_NAME);
            loadContacts(); //loads contacts from database into list
        }
       /// <summary>
       /// Takes an ID and returns the matching contact, if a contact does not exist with that ID null is returned.
       /// </summary>
       /// <param name="ID">ID of contact to retrieve</param>
       /// <returns>Contact with corresponding ID</returns>
        public Contact getContact(int ID)
        {
            return contactsList.Find(x => x.ID == ID); //for each entry, get the ID and compare it to the supplied ID, returns first match (of which there should only be one)
        }
        /// <summary>
        /// Returns list of contacts. Controller should not keep a list locally, and should instead refer to the list
        /// mainained by the ContactManager
        /// </summary>
        public List<Contact> getContacts()
        {
            return contactsList; //returns list (presumably just pointer)
        }

        /// <summary>
        /// Takes a message with an IP and returns the associated Contact. If the contact does not exist, it creates a new one
        /// by the name "unkown". The supplied Message has it's IP field cleared and its sender ID set.
        /// </summary>
        /// <param name="message">Received message with IPString field</param>
        /// <returns>Contact associated with messages IP</returns>
        public Contact identifySender(Message message)
        {
            String IPString = message.IPString;

            //Console.WriteLine($"Sender IP: {IPString}");
            Contact contact = contactsList.Find(x => x.getIPString() == IPString); //returns first (should only be one anyway) contact that has the IP address associated with the message
            
            if(contact != null) //contact with that IP exists
            {
                //Console.WriteLine($"Sender known: {contact.contactName}");
                message.IPString = null; //unset IP to protect against memory imaging
                message.sender = contact.ID; //sets ID of message so it can be passes straight to MessageManager for storage
                return contact; //return it
            }
            else //contact does not exist
            {
                //Console.WriteLine($"Sender unknown");
                addContact("unknown", IPString); //create new contact for IP
                reportNewContact(); //inform controller that a new contact had to be created
                return identifySender(message); //contact is now in the list, do one level of recursion and return the contact
            }
        }

        /// <summary>
        /// creates new DB entry for contact and reloads contacts list. To get updated list of contacts use getContacts().
        /// </summary>
        /// <param name="name">Name of contact</param>
        /// <param name="IPAddress">IP address of contact</param>
        public void addContact(string name, string IPAddress)
        {
            //non-return query on database
            //db.update($"INSERT INTO Users (username, IPAddress) VALUES (\"{name}\",\"{IPAddress}\")"); //ID incremented by DBMS
            //loadContacts(); //reload list from DB

            //create list of byte arrays to be encrypted with the same initialisation vector
            List<byte[]> data = new List<byte[]>() { Encoding.UTF8.GetBytes(name), Encoding.UTF8.GetBytes(IPAddress) };
            Tuple<IEnumerable<byte[]>, byte[]> encResult = CryptoUtility.AESEncryptCollection(data, Globals.getMasterKey());

            data = (List<byte[]>)encResult.Item1; //extracts list of encrypted bytes from result
            byte[] encName = data[0];
            byte[] encIPAddress = data[1];
            byte[] IV = encResult.Item2; //initialisation vector used to encrypt both values

            //non-return query on database
            db.userInsert($"INSERT INTO Users (username, IPAddress, IV) VALUES (@username,@IPAddress,@IV)",encName,encIPAddress,IV);
            loadContacts(); //reload list form DB
        }

        //reads contact info from database and creates Contact objects
        private void loadContacts()
        {
            contactsList.Clear(); //empty list, if reading from database might as well reload everything

            SQLiteDataReader reader = db.retrieve("SELECT * FROM Users"); //gets all user records
            //if(reader.HasRows) //if any data was read from the DB
            
            Contact contact; //temporary contact object to add to list
            while(reader.Read()) //iterate over each tuple returned from the database
            {
                int ID = Convert.ToInt32((long)reader["userID"]); //sqlite handles primary key field as int64
                byte[] encUsername = (byte[])reader["username"];
                byte[] encIPAddress = (byte[])reader["IPAddress"];
                byte[] IV = (byte[])reader["IV"]; //initialisation vector encUsername and ecnIPAddress were encrypted with

                byte[] masterKey = Globals.getMasterKey(); //decrypts and gets master key

                string username = CryptoUtility.AESDecrypt(encUsername, masterKey, IV); //decrypt username
                string IPAddress = CryptoUtility.AESDecrypt(encIPAddress, masterKey, IV); //decrypt IPAddress

                contact = new Contact(ID, username, IPAddress); //puts contact info into object
                contactsList.Add(contact);
            }
            
        }
        /// <summary>
        /// Sets the callback to inform controller when a new contact is created
        /// </summary>
        /// <param name="action">The callback to use when a new contact is created</param>
        public void setNewContactCallback(Action action)
        {
            newContactControllerCallback = action;
        }

        //uses the callback to inform the controller a new contact was created
        private void reportNewContact()
        {
            newContactControllerCallback();
        }
    }
}
