using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SQLite;
using MessageAppGUI;
using System.IO;
using System.Reflection;

namespace UnitTests
{
    
    [TestClass]
    public class DatabaseTest
    {
        private const string INTEGRATION_TEST = "Integration Test";
        private const string DB_NAME = "testDB"; //name of the test database file
        private string DBFilePath = $"{Environment.CurrentDirectory}/{DB_NAME}.sqlite"; //path to database file

        [TestCategory(INTEGRATION_TEST)]
        [TestMethod]
        //creates DB, inserts some data and reads it back
        public void AcreateDB() //apparently unit tests are executed in alphabetical order, I want this one done first so the second tests accessing an existing one
        {
            if(File.Exists(DBFilePath)) //(if exists) delete database file
            {
                File.Delete(DBFilePath); //get delete it so new instance can be created
            }
            DatabaseInterface db = new DatabaseInterface(DB_NAME); //constructor will make new DB as it no longer exists

            db.update("INSERT INTO Users (username,IPAddress) VALUES (\"Michael\",\"1.1.1.1\")");
            db.update("INSERT INTO Users (username,IPAddress) VALUES (\"Tom\",\"2.2.2.2\")");

            SQLiteDataReader reader = db.retrieve("SELECT * FROM users WHERE userID = 1");
            reader.Read(); //prepares tuple of data
            string username = (String)reader["username"]; //gets username from tuple

            db.closeDB(); //close as is no longer needed

            Assert.AreEqual<String>(username, "Michael"); //asserts that name was correctly retrieved
        }

        [TestCategory(INTEGRATION_TEST)]
        [TestMethod]
        //opens existing DB and retrieves some data
        public void BaccessDB()
        {
            DatabaseInterface db = new DatabaseInterface(DB_NAME); 

            SQLiteDataReader reader = db.retrieve("SELECT * FROM users WHERE userID = 2");
            reader.Read(); //prepares tuple of data
            string username = (String)reader["username"]; //gets username from tuple

            db.closeDB(); //close as is no longer needed

            Assert.AreEqual<String>(username, "Tom"); //asserts that name was correctly retrieved
        }
    }
}
