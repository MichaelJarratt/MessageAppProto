﻿using System;
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
            byte[] masterKey = CryptoUtility.generateAESMasterKey(password); //converts password to key bytes
            //Console.WriteLine($"Master key: {Convert.ToBase64String(masterKey)}");
            masterKey = ProtectedData.Protect(masterKey, additionalEntropy, DataProtectionScope.LocalMachine); //encrypts in memory
            //Console.WriteLine($"Protected Master key: {Convert.ToBase64String(masterKey)}");
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
    }
}