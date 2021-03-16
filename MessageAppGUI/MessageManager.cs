﻿using System;
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
        //private const string DB_NAME = "MessageAppDB"; //name of database the application will work with
        DatabaseInterface db;

        public MessageManager()
        {
            db = new DatabaseInterface(Globals.DB_NAME); //creates database interface which connects to <DB_NAME>
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

            string messageString = message.message;

            //run query to store it
            db.update($"INSERT INTO Messages (conversationID, sent, message) VALUES (\"{conversationID}\",\"{sent}\",\"{messageString}\")");
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
