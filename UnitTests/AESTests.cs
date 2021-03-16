using System;
using System.Collections.Generic;
using System.Text;
using MessageApp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class AESTests
    {
        [TestCategory(TestGlobals.UNIT_TEST)]
        [TestMethod]
        //this method tests the derivation of an AES key from a password, then using it to encrypt and decrypt a message
        public void testMasterKeyEncryption()
        {
            string data = "hello, good morning, I hope all is well. blahblahblahblahblah";
            byte[] dataBytes = Encoding.UTF8.GetBytes(data); //convert data to bytes

            byte[] keyBytes = CryptoUtility.generateAESMasterKey("solarwinds123"); //gets key derived from password

            //tuple contains encrypted bytes plus initialisation vector needed to decrypt it
            Tuple<byte[], byte[]> enc = CryptoUtility.AESEncrypt(dataBytes, keyBytes); //encypts and gets encrypted bytes + IV

            byte[] encBytes = enc.Item1; //encrypted bytes
            byte[] IV = enc.Item2; //Instantiation vector

            string decryptedString = CryptoUtility.AESDecrypt(encBytes, keyBytes, IV);

            Assert.AreEqual<string>(data, decryptedString); //compare original data to the decrypted string
        }

        [TestCategory(TestGlobals.UNIT_TEST)]
        [TestMethod]
        //this method tests the creation of a random AES key, then using it encrypt and decrypt a message
        public void testRandomKeyEncryption()
        {
            string data = "hello, good morning, I hope all is well. blahblahblahblahblah";
            byte[] dataBytes = Encoding.UTF8.GetBytes(data); //convert data to bytes

            byte[] keyBytes = CryptoUtility.GenerateAESRandomKey(); //gets randomly generated

            //tuple contains encrypted bytes plus initialisation vector needed to decrypt it
            Tuple<byte[], byte[]> enc = CryptoUtility.AESEncrypt(dataBytes, keyBytes); //encypts and gets encrypted bytes + IV

            byte[] encBytes = enc.Item1; //encrypted bytes
            byte[] IV = enc.Item2; //Instantiation vector

            string decryptedString = CryptoUtility.AESDecrypt(encBytes, keyBytes, IV);

            Assert.AreEqual<string>(data, decryptedString); //compare original data to the decrypted string
        }
    }
}
