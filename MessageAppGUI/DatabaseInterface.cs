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

        /// <summary>
        /// Runs given query on given database connection, no returns. This cannot be used for BLOBs.
        /// </summary>
        /// <param name="queryString">String containing the query to execute</param>
        public void update(string queryString)
        {
            SQLiteCommand command = new SQLiteCommand(queryString, db); //object manages execution
            command.ExecuteNonQuery();
        }
        /// <summary>
        /// Takes a string represnting the query and the byte arrays to be stored as BLOBS.
        /// </summary>
        /// <param name="queryString">String containing the query to execute</param>
        /// <param name="username">Query must have "@username" in place of blob value</param>
        /// <param name="IPAddress">Query must have "@IPAddress" in place of blob value</param>
        /// <param name="IV">Query must have "@IV" in place of blob value</param>
        //blobs cannot just be inserted into a string, so this is necessary where blobs are involved.
        //there's probably a way of doing this without needed a method for each table, but that's probably more convoluted than this
        public void userInsert(string queryString, byte[] username, byte[] IPAddress, byte[] IV)
        {
            SQLiteCommand command = new SQLiteCommand(queryString, db); //object manages execution
            command.Parameters.Add(new SQLiteParameter("@username", username)); //adds username blob to parameters
            command.Parameters.Add(new SQLiteParameter("@IPAddress", IPAddress));
            command.Parameters.Add(new SQLiteParameter("@IV", IV));
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Takes a string represnting the query and the byte arrays to be stored as BLOBS.
        /// </summary>
        /// <param name="queryString">String containing the query to execute</param>
        /// <param name="AESKey">Query must have "@AESKey" in place of blob value</param>
        /// <param name="IV">Query must have "@IV" in place of blob value</param>
        public void KeyInsert(string queryString, byte[] AESKey, byte[] IV)
        {
            SQLiteCommand command = new SQLiteCommand(queryString, db); //object manages execution
            command.Parameters.Add(new SQLiteParameter("@AESKey", AESKey)); //adds username blob to parameters
            command.Parameters.Add(new SQLiteParameter("@IV", IV));
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

            //username and IPAddress are encrypted, IV (Initialisation Vector) is the raw byte array
            update("CREATE TABLE Users (userID INTEGER PRIMARY KEY, username BLOB, IPAddress BLOB, IV BLOB)");
            update("CREATE TABLE Messages (messageID INTEGER PRIMARY KEY, conversationID INTEGER REFERENCES Users(userID), sent BOOLEAN, message VARCHAR(2000))");
            update("CREATE TABLE Keys (keyID INTEGER PRIMARY KEY, AESKey BLOB, DATE date DEFAULT CURRENT_DATE, IV BLOB)");
        }
    }
}
