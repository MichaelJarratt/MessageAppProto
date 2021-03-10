using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SQLite;
using System.IO;
using System.Reflection;

namespace MessageAppGUI
{
    ///<summary>
    ///this class is responsible for creating the database (if needed) and executing queries on it
    ///</summary>
    public class DatabaseInterface
    {
        SQLiteConnection db;

        //initialises database with name dbName
        public DatabaseInterface(string dbName)
        {
            string localDir = Environment.CurrentDirectory; //absolute path of local directory
            dbName += ".sqlite"; //add file extension to filename
            string databaseFile = $"{localDir}/{dbName}"; //absolute path to file

            if (!File.Exists(databaseFile))
            {
                createDB(dbName); //logic to create file and set up tables and rules
            }
            db = db = new SQLiteConnection($"Data Source={databaseFile};Version=3;"); //creates connection to database
            db.Open(); //opens connection (aka it's ready to use)
        }

        //runs given query on given database connection, no returns
        public void update(string queryString)//, SQLiteConnection db)
        {
            SQLiteCommand command = new SQLiteCommand(queryString, db); //object manages execution
            command.ExecuteNonQuery();
        }

        //runs given query on given database connection, returns SQLiteDataReader
        public SQLiteDataReader retrieve(String queryString)//, SQLiteConnection db)
        {
            SQLiteCommand command = new SQLiteCommand(queryString, db);
            SQLiteDataReader reader = command.ExecuteReader();
            return reader;
        }

        //closes the database
        public void closeDB()
        {
            db.Close();
        }

        //creates the database, tables and rules
        private void createDB(string dbName)
        {
            SQLiteConnection.CreateFile(dbName); //creates the  file (would also wipe if file existed already)
            db = new SQLiteConnection($"Data Source={dbName};Version=3;"); //creates connection to database
            db.Open(); //opens connection (aka it's ready to use)

            update("CREATE TABLE Users (userID INTEGER PRIMARY KEY, username VARCHAR(50) DEFAULT \"unknown\", IPAddress VARCHAR(20) DEFAULT \"0.0.0.0\")");
            update("CREATE TABLE Messages (messageID INTEGER PRIMARY KEY, conversationID INTEGER REFERENCES Users(userID), sent BOOLEAN, message VARCHAR(2000))");
        }
    }
}
