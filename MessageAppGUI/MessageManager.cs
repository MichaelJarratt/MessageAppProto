using System;
using System.Collections.Generic;
using System.Text;
using MessageApp;
using System.Data.SQLite;

namespace MessageAppGUI
{
    /// <summary>
    /// This class is responsible for anything to do with Messages. This includes storing messages that are sent or received
    /// and loading all past messages exchanged with a contact
    /// </summary>
    class MessageManager
    {
        DatabaseInterface db;
        KeyManager keyManager;

        public MessageManager()
        {
            db = DatabaseInterface.getInstance(Globals.DB_NAME); //creates database interface which connects to <DB_NAME>
            keyManager = new KeyManager();

            //byte[] key = keyManager.getTodaysKey();
            //Console.WriteLine(Convert.ToBase64String(key));
        }
        /// <summary>
        /// Takes message (which must have sender/recipient set) and stores it in the database.
        /// </summary>
        /// <param name="message">Message to store</param>
        public void commitMessage(Message message)
        {
            Boolean sent = false; //was received
            if (message.target != 0) //was sent
                sent = true;

            int conversationID; //ID of User the message was exchanged with
            if(sent) //was sent by this application
            {
                conversationID = message.target; //get ID of who it was sent to (ID of sender would be zero - aka local)
            }
            else //wat received by this applicaiton
            {
                conversationID = message.sender; //gets ID of sender (ID of target would be zero - aka local)
            }

            Key key = keyManager.getTodaysKey(); //get key being used at time of inserting message
            byte[] plainMessageBytes = Encoding.UTF8.GetBytes(message.message); //convert message text into byte array
            Tuple<byte[], byte[]> encResult = CryptoUtility.AESEncrypt(plainMessageBytes, key.keyBytes); //encrypt message with todays key

            byte[] encMessage = encResult.Item1; //extract encrypted message
            byte[] IV = encResult.Item2; //extract initialisation vector
            int keyID = key.keyID;

            //runs query and passes blobs to store message
            db.messageInsert($"INSERT INTO Messages (conversationID, sent, message, keyID, IV) VALUES (\"{conversationID}\",\"{sent}\",@message,\"{keyID}\",@IV)",encMessage,IV);

            //string messageString = message.message;


            //run query to store it
            //db.update($"INSERT INTO Messages (conversationID, sent, message) VALUES (\"{conversationID}\",\"{sent}\",\"{messageString}\")");
        }

        /// <summary>
        /// Takes a contact and returns a list containg all messages exchanged between local and contact.
        /// </summary>
        /// <param name="contact">Contace for which to get history of exchanges messages</param>
        /// <returns>List of messages associated with the provided contact</returns>
        public List<Message> getMessageHistory(Contact contact)
        {
            List<Message> messages = new List<Message>();
            Message message; //temp var to hold messages before being put into list

            SQLiteDataReader reader = db.retrieve($"SELECT sent, message FROM Messages WHERE (conversationID = \"{contact.ID}\")");
            //API BUG - SELECT with explicit fields fails ONLY when the fields are in brackets, lord knows why

            while(reader.Read()) //iterate over all existing records
            {
                bool sent = (bool)reader["sent"]; //extract if message was sent or received
                string messageString = (string)reader["message"]; //extract message

                int senderID;
                int targetID;

                if(sent) //message was sent by local
                {
                    senderID = 0;
                    targetID = contact.ID;
                }
                else //message was sent by contact
                {
                    senderID = contact.ID;
                    targetID = 0;
                }

                message = new Message(messageString, senderID, targetID); //create the message object
                messages.Add(message); //store it in list
            }

            return messages;
        }
    }
}
