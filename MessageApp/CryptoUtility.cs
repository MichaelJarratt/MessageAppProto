using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MessageApp
{
    //this class is used to provide cryptography utilities to this application
    public static class CryptoUtility
    {

        static RSACryptoServiceProvider rcsp = new RSACryptoServiceProvider(4096); //keeps the key to use during instance lifetime
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
        /// <summary>
        /// RSA - Takes a string message and a key and returns the message encrypted with the key
        /// </summary>
        /// <param name="message">string of message to be encrypted</param>
        /// <param name="key">key to encrypt message with</param>
        /// <returns>base64 encoded string representation of encrypted message</returns>
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

        ////https://stackoverflow.com/questions/17128038/c-sharp-rsa-encryption-decryption-with-transmission
        //public static void encrypt(string message)
        //{
        //    Byte[] messageBytes = Encoding.UTF8.GetBytes(message);

        //    RSACryptoServiceProvider rcsp = new RSACryptoServiceProvider(2024); //2024 bit key
        //    RSAParameters privkey = rcsp.ExportParameters(true); //get private to use for this isntance of program
        //    RSAParameters pubKey = rcsp.ExportParameters(false); //get public to use for this isntance of program

        //    rcsp = new RSACryptoServiceProvider(); //generates with random key
        //    rcsp.ImportParameters(pubKey);

        //    Byte[] messageCypher = rcsp.Encrypt(messageBytes, false); //encrypt with public key
        //    Console.WriteLine(Convert.ToBase64String(messageCypher));

        //}

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
        /// Takes decrypted messageBytes, signed hash (signature) and senders public key.
        /// Hashes messageBytes and signs with senders public key, compares received signature to new signature. They should be the same.
        /// </summary>
        /// <param name="messageBytes"> The decrypted bytes of the received message</param>
        /// <param name="receivedSignature"> The signature that was received in the transmission </param>
        /// <param name="senderPubKey"> The sender public key that was received before transmission </param>
        /// <returns></returns>
        public static bool validateSignature(Byte[] messageBytes, Byte[] receivedSignature, string senderPubKey)
        {
            RSACryptoServiceProvider RSACSP = new RSACryptoServiceProvider(); //create CSP to perform functions
            RSACSP.ImportParameters(keyStringToRSAParam(senderPubKey)); //import public key of sender

            //Byte[] signedMessage = signMessage(messageBytes);

            bool valid;

            try
            {
                //valid = RSACSP.VerifyData(messageBytes, SHA1.Create(), receivedSignature);
                valid = RSACSP.VerifyData(messageBytes, receivedSignature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1); //I have no idea why this one works and the other doesn't. fuck it.
            }
            catch (Exception)
            {

                return false;
            }

            return valid;
        }

        //allows a new provider to be set, used for locally simulating encrypting, sending, receiving and decrypting data
        public static void setServiceProvider(RSACryptoServiceProvider provider)
        {
            rcsp = provider;
        }

        //
        //
        //
        //
        // AES encryption/decryption
        // https://www.youtube.com/watch?v=LOmgFxPHop0&ab_channel=GaurAssociates -this resource was very helpful here

        private static AesCryptoServiceProvider aesCSP = new AesCryptoServiceProvider();
        public static int AESkeyLength = Globals.AES_KEY_LENGTH; //this is public incase length needs to be overwritten for testing

        /// <summary>
        /// Takes password entered by user and generates and AES 128 key for it
        /// </summary>
        /// <param name="password">password entered by user</param>
        /// <returns>Byte[] AES 128 key</returns>
        public static byte[] generateAESMasterKey(string password)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            string salt = "same value every time because the same key must always be derived from the same password";
            byte[] saltBytes = Encoding.UTF8.GetBytes(salt);

            return new PasswordDeriveBytes(passwordBytes, saltBytes, "SHA1", 2).GetBytes(AESkeyLength / 8); //number of bits / bits in a byte
        }
        /// <summary>
        /// Generates a new random AES key
        /// </summary>
        /// <returns>Byte[] containing AES key </returns>
        public static byte[] GenerateAESRandomKey()
        {
            aesCSP.BlockSize = 128;
            aesCSP.KeySize = AESkeyLength;
            aesCSP.GenerateKey();

            return aesCSP.Key; //return key
        }
        /// <summary>
        /// Takes any provided key and uses it to ecnrypt the provided data. Returns the encrypted data plus initialisation vector.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key"></param>
        /// <returns>tuple containing encrypted bytes and initialisation vector</returns>
        public static Tuple<byte[],byte[]> AESEncrypt(byte[] plainDataBytes, byte[] key)
        {
            //aesCSP.BlockSize = 128;
            //aesCSP.KeySize = Globals.AES_KEY_LENGTH;
            //aesCSP.Key = key; //set key for encrption
            //aesCSP.GenerateIV(); //create initialisation vector
            //aesCSP.Mode = CipherMode.CBC;
            //aesCSP.Padding = PaddingMode.PKCS7;

            ////byte[] plainDataBytes = Encoding.UTF8.GetBytes(data); //convert string to bytes

            //ICryptoTransform transform = aesCSP.CreateEncryptor(); //object that does encryption
            //byte[] encryptedBytes = transform.TransformFinalBlock(plainDataBytes, 0, plainDataBytes.Length); //do the encryption

            //byte[] IV = aesCSP.IV;

            //return new Tuple<byte[], byte[]>(encryptedBytes, IV);
            
            aesCSP.GenerateIV(); //generate Initialisation vector
            byte[] IV = aesCSP.IV;  //extract it
            return AESEncryptWithIV(plainDataBytes, key, IV);
        }

        /// <summary>
        /// Takes any provided key and initialisation vector and uses it to encrypt the provided data. returns the encrypted data plus initialisation vector
        /// </summary>
        /// <param name="plainDataBytes"></param>
        /// <param name="key"></param>
        /// <param name="IV"></param>
        /// <returns>tuple containing encrypted bytes and initialisation vector</returns>
        public static Tuple<byte[], byte[]> AESEncryptWithIV(byte[] plainDataBytes, byte[] key, byte[] IV)
        {
            aesCSP.BlockSize = 128;
            aesCSP.KeySize = AESkeyLength;
            aesCSP.Key = key; //set key for encrption
            aesCSP.Mode = CipherMode.CBC;
            aesCSP.Padding = PaddingMode.PKCS7;

            aesCSP.IV = IV;
            //byte[] plainDataBytes = Encoding.UTF8.GetBytes(data); //convert string to bytes

            ICryptoTransform transform = aesCSP.CreateEncryptor(); //object that does encryption
            byte[] encryptedBytes = transform.TransformFinalBlock(plainDataBytes, 0, plainDataBytes.Length); //do the encryption


            return new Tuple<byte[], byte[]>(encryptedBytes, IV);
        }
        /// <summary>
        /// Takes an IEnumerable collection of byte[] of data to be encrypted and the key to encrypt them with.
        /// returns a Tuple with an IEnumerable collection with the encrypted bytes and a byte[] containing the initialisation vector.
        /// All items in collection will be encrypted with the same initialisation vector.
        /// </summary>
        /// <param name="collection">IEnumerable collection of byte[] to be encrypted</param>
        /// <param name="key">key to encrypt collection with</param>
        /// <returns>Tuple containing collection of encrypted byte[] and the initialisation vector</returns>
        public static Tuple<IEnumerable<byte[]>,byte[]> AESEncryptCollection(IEnumerable<byte[]> collection, byte[] key)
        {
            aesCSP.GenerateIV();    //generate Initialisation vector to be used for collection
            byte[] IV = aesCSP.IV;  //extract it

            Tuple<byte[], byte[]> enc; //will hold return from AESEncryptWithIV
            List<byte[]> encryptedList = new List<byte[]>(); //will hold just the encrypted bytes that are returned

            foreach (byte[] item in collection) //iterate over collection to be encrypted
            {
                enc = AESEncryptWithIV(item, key, IV); //encrypt
                encryptedList.Add(enc.Item1); //takes encrypted bytes from returned tuple and stores them in list
            }

            return new Tuple<IEnumerable<byte[]>, byte[]>(encryptedList, IV); //return list + IV
        }


        /// <summary>
        /// Takes encrypted data, AES key and the initialisation vector and returns the decrypted string
        /// </summary>
        /// <param name="data">The encrypted string bytes</param>
        /// <param name="key">The key bytes that encrypted the string</param>
        /// <param name="IV">The initialisation vector used to encrypt the string</param>
        /// <returns>Decrypted string</returns>
        public static string AESDecrypt(byte[] encDataBytes, byte[] key, byte[] IV)
        {
            aesCSP.BlockSize = 128;
            aesCSP.KeySize = AESkeyLength; //128 at the moment
            aesCSP.Key = key; //set key for encrption
            aesCSP.IV = IV; //set the initialisation vector to what was used to originally encrypt the data
            aesCSP.Mode = CipherMode.CBC;
            aesCSP.Padding = PaddingMode.PKCS7;

            ICryptoTransform transform = aesCSP.CreateDecryptor(); //object that does decryption
            byte[] decryptedBytes = transform.TransformFinalBlock(encDataBytes, 0, encDataBytes.Length);

            return Encoding.UTF8.GetString(decryptedBytes);
        }

    }
}
