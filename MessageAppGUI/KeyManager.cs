using System;
using System.Collections.Generic;
using System.Text;
using MessageApp;
using System.Data.SQLite;

namespace MessageAppGUI
{
    /// <summary>
    /// This class is responsible for getting and storing keys in the database
    /// </summary>
    class KeyManager
    {
        DatabaseInterface db;

        public KeyManager()
        {
            db = DatabaseInterface.getInstance(Globals.DB_NAME); //creates database interface which connects to <DB_NAME>
        }

        /// <summary>
        /// Takes the ID of a key in the database and returns a byte array containing the key
        /// </summary>
        /// <param name="keyID">ID of key to fetch from keys table</param>
        /// <returns>byte[] containing key</returns>
        public byte[] getKey(int keyID)
        {
            SQLiteDataReader reader = db.retrieve($"SELECT AESKey, IV FROM Keys WHERE keyID = {keyID}"); //will get only one entry
            if(!reader.HasRows) //no entry with <keyID>
            {
                return new byte[0]; //return empty byte array
            }
            reader.Read(); //prepares reader to extract values

            byte[] encKey = (byte[])reader["AESKey"]; //extract encrypted key
            byte[] IV = (byte[])reader["IV"]; //extract initialisation vector it was encrypted with

            byte[] key = decryptKey(encKey, IV); //decrypt key
            return key;
        }

        /// <summary>
        /// Encapsulates logic to decrypt keys so that it is not repeated.
        /// </summary>
        /// <param name="encKey">Encrypted key that needs decrypting with programs master key</param>
        /// <param name="IV">Initialisation vector that key was encrypted with</param>
        /// <returns>byte array representing key</returns>
        private byte[] decryptKey(byte[] encKey, byte[] IV)
        {
            byte[] masterKey = Globals.getMasterKey(); //gets master key to decrypt these keys with
            byte[] decryptedKey = CryptoUtility.AESDecryptBytes(encKey, masterKey, IV); //decrypts key

            return decryptedKey;
        }

        /// <summary>
        /// Returns the key whos date field is equal to todays date. If a key does not yet exist for todays date, it is created
        /// and stored.
        /// </summary>
        /// <returns>byte[] containing todays key</returns>
        public Key getTodaysKey()
        {
            SQLiteDataReader reader = db.retrieve("SELECT keyID, AESKey, IV FROM Keys WHERE date = CURRENT_DATE"); //gets key with todays date
            if(reader.HasRows) //if there were any keys with todays date
            {
                reader.Read(); //prepares reader to extract data

                int keyID = Convert.ToInt32((long)reader["keyID"]); //sqlite stored primary keys as int64s
                byte[] encKey = (byte[])reader["AESKey"]; //extract encrypted key
                byte[] IV = (byte[])reader["IV"]; //extract initialisation vector it was encrypted with

                byte[] keyBytes = decryptKey(encKey, IV); //decrypt key
                Key key = new Key(keyID, keyBytes); //creates key object to return

                return key;
            }
            else //must generate new key and create DB entry
            {
                byte[] newKey = CryptoUtility.GenerateAESRandomKey(); //generate random AES key

                byte[] masterKey = Globals.getMasterKey(); //gets master key to encrypt the new key
                Tuple<byte[], byte[]> encResult = CryptoUtility.AESEncrypt(newKey, masterKey); //encrypts new key with master key
                byte[] encNewKey = encResult.Item1; //extract encrypted key
                byte[] IV = encResult.Item2; //extract initialisation vector

                insertNewKey(encNewKey, IV); //insert it into database

                return getTodaysKey(); //1 level of recursion, creates entry then fetches and returns it
            }
        }

        //encapsulates logic for inserting new key into database. For tidyness.
        private void insertNewKey(byte[] encKeyBytes, byte[] IV)
        {
            db.keyInsert("INSERT INTO Keys (AESKey, IV) VALUES (@AESKey, @IV)", encKeyBytes, IV);
        }
    }

    //used to temporarily hold a key and it's ID
    public class Key
    {
        public int keyID;
        public byte[] keyBytes;

        public Key(int keyID, byte[] keyBytes)
        {
            this.keyID = keyID;
            this.keyBytes = keyBytes;
        }
    }
}
