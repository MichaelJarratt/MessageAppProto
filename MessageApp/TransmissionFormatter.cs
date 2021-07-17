using System;
using System.Collections.Generic;
using System.Text;

namespace MessageApp
{
    public static class TransmissionFormatter
    {
        /// <summary>
        /// Takes a raw transmission and returns a CryptographyType identifying if it is a
        /// plaintext, RSA or AES encrypted transmission
        /// </summary>
        /// <param name="transmission">Raw received bytes</param>
        /// <returns>Crytography type</returns>
        public static CryptographyType identifyCryptographyType(byte[] transmission)
        {
            throw new NotImplementedException();
        }

        private static int CrytoTypeToInt(CryptographyType cryptoType)
        {
            throw new NotImplementedException();
        }

        private static CryptographyType intToCryptoType(int i)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Prepares the transmission format for an RSA encrypted message.
        /// Format is: first 6 bytes are total length, signature length and message length respectively.
        /// Followed by the signature, followed by the encrypted data.
        /// Give the returned byte array to NetworkUtility for transmission.
        /// </summary>
        /// <param name="data">The data to be transmitted</param>
        /// <param name="key">The public key of the messages recipient</param>
        /// <returns>byte[] formatted for transmission</returns>
        public static byte[] encodeRSATransmission(byte[] data, string key)
        {
            Byte[] signatureBytes = CryptoUtility.signMessage(data); //creates signature for data
            Byte[] messageBytes = Encoding.UTF8.GetBytes(CryptoUtility.RSAEncryptData(data, key)); //creates byte array for encrypted message

            //calculate length headers
            int totalLength = signatureBytes.Length + messageBytes.Length + 6; //length of signature, message, itself and signature and message lengths //also I know it's a magic number but this is a prototype, no point setting up constants class for only this
            Byte[] totalLengthBytes = lengthIntToBytes(totalLength); //two bytes
            Byte[] signatureLengthBytes = lengthIntToBytes(signatureBytes.Length); //two bytes
            Byte[] messageLengthBytes = lengthIntToBytes(messageBytes.Length); //two bytes

            //temporary - testing what happens when corruption occurs
            //totalLengthBytes = lengthIntToBytes(1500);
            //signatureLengthBytes = lengthIntToBytes(260); 
            //messageLengthBytes = lengthIntToBytes(2000);
            //signatureBytes[0] = 0; // 1 in 256 chance that [0] would actually be 0
            //messageBytes[0] = 0;

            //stick lengths, signature and message together
            Byte[] transmissionBytes = new byte[totalLength]; //signature is always 253 bytes
            Array.Copy(totalLengthBytes, transmissionBytes, 2); //first two bytes - copies the two bytes from total length into transmission bytes
            Array.Copy(signatureLengthBytes, 0, transmissionBytes, 2, 2); //bytes 3&4 (2&3)
            Array.Copy(messageLengthBytes, 0, transmissionBytes, 4, 2); //bytes 5&6 (4&5)
            Array.Copy(signatureBytes, 0, transmissionBytes, 6, signatureBytes.Length); //starting after lengths, insert signature bytes (always 253)
            Array.Copy(messageBytes, 0, transmissionBytes, 6 + signatureBytes.Length, messageBytes.Length); //starting in position after signature bytes, add all message bytes

            return transmissionBytes;
        }

        /// <summary>
        /// Takes a TransmissionState object containing a received RSA transmission and extracts the signature
        /// and encrypted message.
        /// Returns a tuple<byte[],byte[]> containing the signature, followed by the message.
        /// </summary>
        /// <param name="transState">TransmissionState object containing received RSA transmission</param>
        /// <returns>tuple<byte[],byte[]> containing the signature, followed by the message.</returns>
        public static Tuple<Byte[],Byte[]> decodeRSATransmission(TransmissionState transState)
        {
            //extract signature length and message length
            transState.signatureLength = lengthBytesToInt(new ArraySegment<Byte>(transState.bytes, 2, 2).ToArray());
            transState.messageLength = lengthBytesToInt(new ArraySegment<Byte>(transState.bytes, 4, 2).ToArray());

            //get signature bytes from bufferState
            Byte[] signatureBytes = new ArraySegment<Byte>(transState.bytes, 6, transState.signatureLength).ToArray();
            //get encrypted message bytes from bufferState
            Byte[] messageBytes = new ArraySegment<Byte>(transState.bytes, 6 + transState.signatureLength, transState.messageLength).ToArray();


            return new Tuple<byte[], byte[]>(signatureBytes, messageBytes);
        }

        public static Tuple<Byte[], Byte[]> decodeRSATransmission(byte[] transmissionBytes)
        {
            //extract signature length and message length
            int signatureLength = lengthBytesToInt(new ArraySegment<Byte>(transmissionBytes, 2, 2).ToArray());
            int messageLength = lengthBytesToInt(new ArraySegment<Byte>(transmissionBytes, 4, 2).ToArray());

            //get signature bytes from bufferState
            Byte[] signatureBytes = new ArraySegment<Byte>(transmissionBytes, 6, signatureLength).ToArray();
            //get encrypted message bytes from bufferState
            Byte[] messageBytes = new ArraySegment<Byte>(transmissionBytes, 6 + signatureLength, messageLength).ToArray();


            return new Tuple<byte[], byte[]>(signatureBytes, messageBytes);
        }


        /// <summary>
        /// takes int32 number, typecasts to int16, converts it to Byte[] with two elements and returns it
        /// </summary>
        /// <param name="length">int32 representation of number of bytes</param>
        /// <returns>Byte[] representing an int 16 with two elements</returns>
        public static Byte[] lengthIntToBytes(int length)
        {
            short shortLength = (short)length; //typecast to short (anything greater than 65,536 will either throw exception or lose precision)
            return BitConverter.GetBytes(shortLength);
        }

        /// <summary>
        /// Takes an array of two bytes, representing a short, and converts it to an integer.
        /// </summary>
        /// <param name="bytes">Byte[2] representing the short</param>
        /// <returns>Integer conversion of the short</returns>
        public static int lengthBytesToInt(Byte[] bytes)
        {
            return (int)BitConverter.ToInt16(bytes); //convert two bytes to a short then typecast to int
        }
    }
}
