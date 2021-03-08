using System;
using System.Collections.Generic;
using System.Text;

namespace MessageAppGUI
{
    class ContactManager
    {
        private const string DB_Name = "MessageAppDB"; //name of database the application will work with
        DatabaseInterface db;

        public ContactManager()
        {
            db = new DatabaseInterface(DB_Name);
        }
    }
}
