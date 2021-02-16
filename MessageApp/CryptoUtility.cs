using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace MessageApp
{
    //this class is used to provide cryptography utilities to this application
    public static class CryptoUtility
    {
        static RSACng RSAKey = new RSACng(3072); //creates new RSA thing with random key of size 3072 (public/private keys will be different each launch)
        //static RSACng remoteRSAKey = new RSACng();

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
            Byte[] keyBytes = rcsp.ExportRSAPrivateKey();

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

        public static void badencrypt(string message)
        {
            Byte[] messageBytes = Encoding.UTF8.GetBytes(message); //convert message to byte[]

            //get "remote" keys
            RSACng remoteRSAKey = new RSACng(); //represents keys from other machine
            Byte[] remotePrivateKey = remoteRSAKey.Key.Export(CngKeyBlobFormat.GenericPrivateBlob);
            Byte[] remotePublicKey = remoteRSAKey.Key.Export(CngKeyBlobFormat.GenericPublicBlob);

            //get local keys
            Byte[] privateKey = RSAKey.Key.Export(CngKeyBlobFormat.GenericPrivateBlob); //exports private key as byte[]
            Byte[] publicKey = RSAKey.Key.Export(CngKeyBlobFormat.GenericPublicBlob); //exports public key as byte

            //encrypt with private key
            CngKey privateCNGKey = CngKey.Import(privateKey, CngKeyBlobFormat.GenericPrivateBlob); //imports private key byte[]
            RSACng encryptor = new RSACng(privateCNGKey); //creates RSACng with the key
            messageBytes = encryptor.Encrypt(messageBytes, RSAEncryptionPadding.OaepSHA1);
            Console.WriteLine("Encrypted message (private): "+Encoding.UTF8.GetString(messageBytes)+"\n\n");

            //messageBytes = encryptor.Decrypt(messageBytes, RSAEncryptionPadding.OaepSHA512); //somehow works???
            //Console.WriteLine("Encrypted message: " + Encoding.UTF8.GetString(messageBytes) + "\n\n"); 

            //encrypt with public key
            CngKey publicCNGKey = CngKey.Import(publicKey, CngKeyBlobFormat.GenericPublicBlob); //imports public key byte[]
            encryptor = new RSACng(publicCNGKey); //creates RSACng with the key
            messageBytes = encryptor.Encrypt(messageBytes, RSAEncryptionPadding.OaepSHA1);
            Console.WriteLine("Encrypted message (public): " + Encoding.UTF8.GetString(messageBytes) + "\n\n");







            publicCNGKey = CngKey.Import(publicKey, CngKeyBlobFormat.GenericPublicBlob); //imports private key byte[]
            RSACng decryptor = new RSACng(privateCNGKey); //creates RSACng with the key
            messageBytes = decryptor.Decrypt(messageBytes, RSAEncryptionPadding.OaepSHA512);
            Console.WriteLine("Decrypted message: " + Encoding.UTF8.GetString(messageBytes) + "\n\n");

            messageBytes = decryptor.Encrypt(messageBytes, RSAEncryptionPadding.OaepSHA512);
            Console.WriteLine("Encrypted message: " + Encoding.UTF8.GetString(messageBytes) + "\n\n");












            //Byte[] privateKey = RSAKey.Key.Export(CngKeyBlobFormat.GenericPrivateBlob);
            //Byte[] publicKey = RSAKey.Key.Export(CngKeyBlobFormat.GenericPublicBlob); 
            // RSAParameters publicKey = RSAKey.ExportParameters(false);
            // RSAParameters privateKey = RSAKey.ExportParameters(true);


            // RSACng encryptor = new RSACng(); //creates with random key
            //Console.WriteLine(Encoding.UTF8.GetString(encryptor.ExportRSAPrivateKey())); //prints generated key

            //  encryptor.ImportParameters(privateKey);
            //Console.WriteLine(Encoding.UTF8.GetString(encryptor.ExportRSAPrivateKey())); //prints generated key
            //  messageBtyes = encryptor.Encrypt(messageBtyes, RSAEncryptionPadding.OaepSHA512);
            //  Console.WriteLine(Encoding.UTF8.GetString(messageBtyes));

            //  messageBtyes = encryptor.Decrypt(messageBtyes, RSAEncryptionPadding.OaepSHA512);
            //  Console.WriteLine(Encoding.UTF8.GetString(messageBtyes));





















            //byte[] publickey = rsacrypto.exportrsapublickey();
            //byte[] privatekey = rsacrypto.exportrsaprivatekey();
            //cngkey key = cngkey.create(cngalgorithm.rsa);
            //key.
            ////encryptparams.
            //console.writeline(message);
            //byte[] messagebytes = encoding.utf8.getbytes(message); //write message
            //messagebytes = rsacrypto.encrypt(messagebytes,rsaencryptionpadding.oaepsha512);
            //console.writeline(encoding.utf8.getstring(messagebytes)); //write encrypted message
            //messagebytes = rsacrypto.decrypt(messagebytes, rsaencryptionpadding.oaepsha512);
            //console.writeline(encoding.utf8.getstring(messagebytes));
        }
    }
}
