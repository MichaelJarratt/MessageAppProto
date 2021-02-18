using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace MessageApp
{
    //this class is used to provide cryptography utilities to this application
    public static class CryptoUtility
    {

        static RSACryptoServiceProvider rcsp = new RSACryptoServiceProvider(2024); //keeps the key to use during instance lifetime
        //static RSAEncryptionPadding padding = new RSAEncryptionPadding();

        /// <summary>
        /// Returns this program instances private key as a Byte array
        /// </summary>
        /// <returns>Bute[] private key</returns>
        public static string getPrivateKey()
        {
            //return RSAKey.ExportRSAPublicKey();
            RSAParameters privKey = rcsp.ExportParameters(true); //get public to use for this isntance of program
            return RSAParamToKeyString(privKey); //converts it to string and returns it
        }
        /// <summary>
        /// Returns this program instances public key as a byte array
        /// </summary>
        /// <returns></returns>
        public static string getPublicKey()
        {
            //return RSAKey.ExportRSAPublicKey();
            RSAParameters pubKey = rcsp.ExportParameters(false); //get public to use for this isntance of program
            return RSAParamToKeyString(pubKey); //converts it to string and returns it
        }

        //turns a key into a string represenation
        private static string RSAParamToKeyString(RSAParameters key)
        {
            //we need some buffer
            var sw = new System.IO.StringWriter();
            //we need a serializer
            var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
            //serialize the key into the stream
            xs.Serialize(sw, key);
            //get the string from the stream
            return sw.ToString();
        }

        //turns a string representation of a key to a usable key
        private static RSAParameters keyStringToRSAParam(string keyString)
        {
            //get a stream from the string
            var sr = new System.IO.StringReader(keyString);
            //we need a deserializer
            var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
            //get the object back from the stream
            return (RSAParameters)xs.Deserialize(sr);
        }

        //takes a message and a key and returns the message encrypted with the key
        public static string encryptData(string message, string key)
        {
            Byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            RSACryptoServiceProvider encrypter = new RSACryptoServiceProvider(); //create instance that will do the encryption
            encrypter.ImportParameters(keyStringToRSAParam(key)); //converts the keystring to a key and sets it
            messageBytes = encrypter.Encrypt(messageBytes, false);

            //String encryptedMessage = Encoding.UTF8.GetString(messageBytes);
            string encryptedMessage = System.Convert.ToBase64String(messageBytes);
            return encryptedMessage;
        }

        //takes an encrypted message and key and decrypts it
        public static string decryptData(string message, string key)
        {
            //Byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            Byte[] messageBytes = System.Convert.FromBase64String(message);
            //Byte[] keyBytes = rcsp.ExportRSAPrivateKey();

            RSACryptoServiceProvider decrypter = new RSACryptoServiceProvider(); //create instance that will do the decryption
            decrypter.ImportParameters(keyStringToRSAParam(key)); //converts the keystring to a key and sets it
            messageBytes = decrypter.Decrypt(messageBytes, false);
            
            String decryptedMessage = Encoding.UTF8.GetString(messageBytes); //turns bytes back into string
            return decryptedMessage;
        }

        //https://stackoverflow.com/questions/17128038/c-sharp-rsa-encryption-decryption-with-transmission
        public static void encrypt(string message)
        {
            Byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            RSACryptoServiceProvider rcsp = new RSACryptoServiceProvider(2024); //2024 bit key
            RSAParameters privkey = rcsp.ExportParameters(true); //get private to use for this isntance of program
            RSAParameters pubKey = rcsp.ExportParameters(false); //get public to use for this isntance of program

            rcsp = new RSACryptoServiceProvider(); //generates with random key
            rcsp.ImportParameters(pubKey);

            Byte[] messageCypher = rcsp.Encrypt(messageBytes, false); //encrypt with public key
            Console.WriteLine(Convert.ToBase64String(messageCypher));

        }

        /// <summary>
        /// Creates a signature of messageString using this instances private key and SHA256
        /// </summary>
        /// <param name="messageString"></param>
        /// <returns>
        /// Byte array containing the signed hash of the message
        /// </returns>
        public static Byte[] signMessage(string messageString)
        {
            Byte[] messageBytes = Encoding.UTF8.GetBytes(messageString); //convert string to bytes

            return signMessage(messageBytes);
        }

        /// <summary>
        /// Creates a signature of messageByte using this instances private key and SHA256
        /// </summary>
        /// <param name="messageBytes"></param>
        /// <returns>
        /// Byte array containing the signed hash of the message
        /// </returns>
        public static Byte[] signMessage(Byte[] messageBytes)
        {
            RSACryptoServiceProvider RSACSP = new RSACryptoServiceProvider(); //create CSP to perform functions
            RSACSP.ImportParameters(rcsp.ExportParameters(true)); //give it applications private key

            //Byte[] signature = RSACSP.SignData(messageBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1); //creates key
            Byte[] signature = RSACSP.SignData(messageBytes, SHA256.Create());
            return signature;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageBytes"> The decrypted bytes of the received message</param>
        /// <param name="receivedSignature"> The signature that was received in the transmission </param>
        /// <param name="senderPubKey"> The sender public key that was received before transmission </param>
        /// <returns></returns>
        public static bool validateSignature(Byte[] messageBytes, Byte[] receivedSignature, string senderPubKey)
        {
            RSACryptoServiceProvider RSACSP = new RSACryptoServiceProvider(); //create CSP to perform functions
            RSACSP.ImportParameters(keyStringToRSAParam(senderPubKey)); //import public key of sender

            Byte[] signedMessage = signMessage(messageBytes);

            bool valid;

            try
            {
               //valid = RSACSP.VerifyData(messageBytes, SHA1.Create(), receivedSignature);
               valid = RSACSP.VerifyData(messageBytes, receivedSignature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            }
            catch (Exception)
            {

                return false;
            }

            return valid;
        }

    }
}
