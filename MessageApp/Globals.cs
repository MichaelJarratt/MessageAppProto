using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace MessageApp
{
    public static class Globals
    {
        private static byte[] masterKey; //used to protect keys for messages and contact details
        private static byte[] additionalEntropy = { 10, 5, 67, 1 }; //used in some way to protect data in memory

        public const int AES_KEY_LENGTH = 128;
        public const string DB_NAME = "MessageAppDB"; //name of database file


        /// <summary>
        /// takes a password entered by the user and uses it as the seed to generate a symmetric key
        /// </summary>
        /// <param name="password">Password entered by user on startup</param>
        public static void setMasterKey(string password)
        {
            masterKey = CryptoUtility.generateAESMasterKey(password); //converts password to key bytes
            masterKey = ProtectedData.Protect(masterKey, additionalEntropy, DataProtectionScope.LocalMachine); //encrypts in memory
            password = string.Empty; //clear plaintext password from memory
        }
        /// <summary>
        /// Decrypts and returns master key
        /// </summary>
        /// <returns>Byte array AES key</returns>
        public static byte[] getMasterKey()
        {
            byte[] keyBytes = ProtectedData.Unprotect(masterKey, additionalEntropy, DataProtectionScope.LocalMachine); //decrypts in memory
            //return Encoding.UTF8.GetString(keyBytes);
            return keyBytes;
        }

        /// <summary>
        /// Returns true if the master key is set and ready to be retreived, false is not.
        /// </summary>
        /// <returns>bool master key is set or not</returns>
        public static bool isMasterKeySet()
        {
            if (masterKey == null) //if the key is being gotten before being set
                return false;
            else
                return true;
        }
    }
}
