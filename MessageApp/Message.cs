using System;
using System.Collections.Generic;
using System.Text;

namespace MessageAppGUI
{
    //this class represents a message and its meta data
    public class Message
    {
        public string message; //the message being wrapped
        public int sender; //ID of sender (0 = local)
        public int target; //ID of recipient (0 = local, AKA anything with target zero was sent TO this application not BY)
        public string IPString;

        //constructor used in sending
        public Message(string message, int target)
        {
            this.message = message;
            sender = 0; //sent by this application
            this.target = target; //who its going to
        }
        //constructor used in receiving 
        public Message(string message, string IPString)
        {
            this.message = message;
            target = 0; //being received by this application
            this.IPString = IPString; //IP of sender, once sender ID is worked out this is cleared to protect the information
        }
    }
}
