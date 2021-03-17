using System;
using System.Collections.Generic;
using System.Text;
using MessageApp;
using System.Data.SQLite;
using System.Security.Cryptography;

namespace MessageAppGUI
{
    /// <summary>
    /// This class is responsible for getting and storing keys in the database
    /// </summary>
    class KeyManager
    {
        private DatabaseInterface db;
        private Dictionary<int, byte[]> keyCache; //when keys are retrieved they are (protected) stored in the cache incase they are needed again
        private byte[] entropy = new byte[] { 1, 6, 7, 14 }; //used in ProtectedData

        public KeyManager()
        {
            db = DatabaseInterface.getInstance(Globals.DB_NAME); //creates database interface which connects to <DB_NAME>
            keyCache = new Dictionary<int, byte[]>();
        }

        /// <summary>
        /// Takes the ID of a key in the database and returns a byte array containing the key.
        /// This method uses caching as it will be used frequently, this is to prevent unnecessary database reads.
        /// </summary>
        /// <param name="keyID">ID of key to fetch from keys table</param>
        /// <returns>byte[] containing key</returns>
        public byte[] getKey(int keyID)
        {
            if (isCached(keyID)) //check if the key has been cached already
            {
                return retrieveFromCache(keyID); //return cached key
            }
            else //otherwise get key from database and store it in cache
            {
                SQLiteDataReader reader = db.retrieve($"SELECT AESKey, IV FROM Keys WHERE keyID = {keyID}"); //will get only one entry
                if (!reader.HasRows) //no entry with <keyID>
                {
                    return new byte[0]; //return empty byte array
                }
                reader.Read(); //prepares reader to extract values

                byte[] encKey = (byte[])reader["AESKey"]; //extract encrypted key
                byte[] IV = (byte[])reader["IV"]; //extract initialisation vector it was encrypted with

                byte[] key = decryptKey(encKey, IV); //decrypt key
                storeInCache(keyID, key); //store in cache
                return key; //return decrypted key
            }
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
        /// Takes the bytes of a key and it's ID, if the key is not already cached, it is encrypted and cached.
        /// </summary>
        /// <param name="keyID">ID of key to cache</param>
        /// <param name="keyBytes">non-encrypted key bytes to cache</param>
        private void storeInCache(int keyID, byte[] keyBytes)
        {
            if(!keyCache.ContainsKey(keyID)) //if the key is not already cached
            {
                keyBytes = ProtectedData.Protect(keyBytes, entropy, DataProtectionScope.LocalMachine); //encrypt it
                keyCache.Add(keyID, keyBytes);
            }
        }

        /// <summary>
        /// Takes the ID of the key to retrieve from the cache, if it does not exist then null is returned.
        /// </summary>
        /// <param name="keyID">ID of key to retrieve from cache</param>
        /// <returns>byte array containing non-encrypted key, dispose it of when finished.</returns>
        private byte[] retrieveFromCache(int keyID)
        {
            if(keyCache.ContainsKey(keyID)) //if it has it
            {
                byte[] keyBytes = keyCache.GetValueOrDefault(keyID);
                keyBytes = ProtectedData.Unprotect(keyBytes, entropy, DataProtectionScope.LocalMachine);
                return keyBytes;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// Takes the ID of a key and returns bool if it is cached
        /// </summary>
        /// <param name="keyID">The key to check for</param>
        /// <returns>bool if cache entry with keyID exists</returns>
        private bool isCached(int keyID)
        {
            return keyCache.ContainsKey(keyID);
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
