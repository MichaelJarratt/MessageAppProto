using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SQLite;

namespace MessageAppGUI.DatabaseStuff
{
    //this class creates the database and nothing more
    class DBCreate
    {
        static void Main()
        {
            test();
        }

        //runs given query on given database connection, no returns
        private static void update(string queryString,SQLiteConnection db)
        {
            SQLiteCommand command = new SQLiteCommand(queryString, db); //object manages execution
            command.ExecuteNonQuery();
        }

        //runs given query on given database connection, returns SQLiteDataReader
        private static SQLiteDataReader retrieve(String queryString, SQLiteConnection db)
        {
            SQLiteCommand command = new SQLiteCommand(queryString, db);
            SQLiteDataReader reader = command.ExecuteReader();
            return reader;
        }

        //creates a database, and runs some queries
        private static void test()
        {
            SQLiteConnection.CreateFile("testDB.sqlite"); //creates new database file
            SQLiteConnection db = new SQLiteConnection("Data Source=testDB.sqlite;Version=3;"); //connect to database, ref as db
            db.Open();

            update("CREATE TABLE users (ID INT, name VARCHAR(20))",db); //runs query on DB
            update("INSERT INTO users VALUES (1,'Michael')",db);
            update("INSERT INTO users VALUES (2,'Tom')", db);
            update("INSERT INTO users VALUES (3,'Lewis')", db);

            SQLiteDataReader reader = retrieve("SELECT * FROM users ORDER BY ID DESC", db);
            Console.WriteLine("Users:");
            while (reader.Read())
            {
                Console.WriteLine($"{reader["name"]}: {reader["ID"]}");
            }
        }
    }
}
