using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;

namespace MessageApp
{
    //this class is responsible for representing the state of a receiving buffer
    public class TransmissionState
    {
        public const int bufferSize = 2048; //size of buffer used to receive bytes
        public byte[] bytes = new byte[bufferSize]; //buffer used to receive bytes
        public StringBuilder stringBuilder = new StringBuilder(); //used to build strings from bytes in the buffer
        public Socket socket = null; //the socket this is acting as the buffer for

        public string keyString = String.Empty; //holds the key used to decrypt the data
        public int totalBytesReceived = 0; //holds total number of bytes received (compared against totalLength)
        public int totalLength = 0; //holds total length of transmission
        public int signatureLength = 0; //length of signature
        public int messageLength = 0; //holds length of encrypted message

        public Action<BufferState> callback = null; //the callback used by the receive method when all the data has been received
        public CryptographyType cryptographyType;

        public Message message; //message object being sent
    }
}
