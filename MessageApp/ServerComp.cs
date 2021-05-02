using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MessageApp
{
    public class ServerComp
    {
        private int listenPort; //port the socket will listen to

        private Socket connectionListener; //socker that listens for incoming connections
        
        Action<Message> controllerReturn; // Action can hold a reference to a method, this references the call back handler on MessageApp that prints the received message
        //Action<String> is a void return method that takes one string. Action<String,String> takes two
        //Func<String,String> is a String return method that takes one string. Func<String,String,String> takes two strings.
        Action<TransmissionErrorCode> controllerReceiveErrorReport; //used to inform controller of errors receiving transmissions

        public ServerComp()
        {
            listenPort = 65432; //default value
            init();
        }
        public ServerComp(int listenPort)
        {
            this.listenPort = listenPort;
            init();
        }

        //does set up tasks common to each constructor
        private void init()
        {
            //instantiate connection listener
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, listenPort); //binds to <listenPort> on any interface
            connectionListener = new Socket(IPAddress.Any.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            connectionListener.Bind(localEndPoint);
            connectionListener.Listen(10);

        }

        
        /// <summary>
        /// This method begins the listen loop.
        /// When this method is called incoming TCP requests will be served.
        /// </summary>
        public void startConnectionListenLoop()
        {
            connectionListener.BeginAccept(new AsyncCallback(acceptTCPRequest), connectionListener);
        }

        // THREAD //
        //started when connectionListener gets a request
        //handles creating a receiving socket and then terminates
        private void acceptTCPRequest(IAsyncResult ar)
        {
            connectionListener.BeginAccept(new AsyncCallback(acceptTCPRequest), connectionListener); //immediately tells connectionListener to begin listening again

            Socket conListenerTemp = (Socket)ar.AsyncState; //gets the connectionListener socket from the listenLoop (although couldn't this be done via the field?)
            Socket receiveHandler = conListenerTemp.EndAccept(ar); //gets socket that will receive bytes

            String keyString = receiveKey(receiveHandler); //block
            sendKey(receiveHandler); //block .keys are exchanged syncrhonously (must be completed first), then continue receiving message

            BufferState bufferState = new BufferState(); //creates new bit buffer for receiving socket
            bufferState.socket = receiveHandler; //places socket this buffer is for inside so it can be passed in the IAsyncResult
            bufferState.keyString = keyString; //stores senders public key in bufferState
            receiveHandler.BeginReceive(bufferState.bytes, 0, BufferState.bufferSize, SocketFlags.None, new AsyncCallback(receiveBytes), bufferState); //stores received bytes in bufferState.bytes
        }

        //  THREAD  //
        //started when a request is accepted, and a socket is created to receive the bits.
        //handles receiving the bytes and continuing until they are all received (see below) then passing them off to completeReceive for parsing.
        private void receiveBytes(IAsyncResult ar)
        {
            BufferState bufferState = (BufferState)ar.AsyncState; //gets buffer from argument
            Socket receiveHandler = bufferState.socket; //socket has been receivng bytes between begin receive (or a previus loop of receiveBytes) and now

            int bytesReceived = receiveHandler.EndReceive(ar); //gets number of bytes received, these bytes are stored in the bufferState
            bufferState.totalBytesReceived = bufferState.totalBytesReceived + bytesReceived; //self explanatory

            if (bytesReceived > 0) //if bytes were received during this cycle
            {
                if (bufferState.totalLength == 0 && bufferState.totalBytesReceived >= 2) //if the first two bits have been received and total length has not been extracted yet
                {
                    bufferState.totalLength = lengthBytesToInt(new ArraySegment<Byte>(bufferState.bytes, 0, 2).ToArray()); //creates array containg first two bits of transmission and converts them to int (total length) 
                }
                if (bufferState.signatureLength == 0 && bufferState.totalBytesReceived >= 4) //bytes 3&4 are signature length
                {
                    bufferState.signatureLength = lengthBytesToInt(new ArraySegment<Byte>(bufferState.bytes, 2, 2).ToArray());
                }
                if (bufferState.messageLength == 0 && bufferState.totalBytesReceived >= 6) //bytes 5&6 are message length
                {
                    bufferState.messageLength = lengthBytesToInt(new ArraySegment<Byte>(bufferState.bytes, 4, 2).ToArray());
                }
                //only the lengths need to be extracted, the rest can be dealt with by the completeReceive function
                if (bufferState.totalBytesReceived < bufferState.totalLength) //if not every byte has been received
                {
                    //repeat loop
                    receiveHandler.BeginReceive(bufferState.bytes, 0, BufferState.bufferSize, SocketFlags.None, new AsyncCallback(receiveBytes), bufferState);
                }
                else if (bufferState.totalLength == bufferState.totalBytesReceived) //every byte has been received
                {
                    try
                    {
                        completeReceive(bufferState); //pass everything off to complete receive for paring and validation
                    }
                    catch (Exception e)
                    {
                        reportReceiveError(TransmissionErrorCode.ServDecOrValError); //errors decrypting or validating
                    }
                }
                else if (bufferState.totalLength != bufferState.totalBytesReceived) //transmission error (total length too short)
                {
                    reportReceiveError(TransmissionErrorCode.ServTotalLengthError); //total length error
                }
            }
            else //if no bytes were received, then a transmission error probably ocurred (total length too long)
            {
                //arrive here if no more bytes were received
                reportReceiveError(TransmissionErrorCode.ServTotalLengthError); //total length error
            }
        }

        //callback called by receiveBytes.
        //will extract the signature and encrypted message from the transmission bytes.
        //decrypts message using received public key of sender.
        //validates the signature.
        private void completeReceive(BufferState bufferState)
        {
            //get signature bytes from bufferState
            Byte[] signatureBytes = new ArraySegment<Byte>(bufferState.bytes, 6, bufferState.signatureLength).ToArray();
            //get encrypted message bytes from bufferState
            Byte[] messageBytes = new ArraySegment<Byte>(bufferState.bytes, 6 + bufferState.signatureLength, bufferState.messageLength).ToArray();

            //decrypt message with private key and get its string
            string messageString = CryptoUtility.decryptData(Encoding.UTF8.GetString(messageBytes), CryptoUtility.getPrivateKey());

            //validate signature (inspect validateSignature to see how)
            bool validSignature = CryptoUtility.validateSignature(Encoding.UTF8.GetBytes(messageString), signatureBytes, bufferState.keyString);

            if (validSignature)
            {
                bufferState.socket.Send(Encoding.UTF8.GetBytes("received")); //send confirmation of successful receive
                string senderIP = ((System.Net.IPEndPoint)bufferState.socket.RemoteEndPoint).Address.ToString(); //pretty dumb, have to cast socket.remoteEndPoint to IPEndPoint to access the Address attribute 

                Message message = new Message(messageString, senderIP); //creates message object with content and IP of the sender
                controllerReturn(message);
            }
            else //signature invalid
            {
                reportReceiveError(TransmissionErrorCode.ServValidationFail); //report signature failed to validate
            }
            
        }

        //calls callback method of controller to inform it that some error occured 
        private void reportReceiveError(TransmissionErrorCode errorCode)
        {
            //Console.WriteLine($"receive error - code {code}");
            controllerReceiveErrorReport(errorCode);
        }

        // synchronously accepts the public key of the client
        private string receiveKey(Socket receiveHandler)
        {
            Byte[] keyBytes = new Byte[1024]; //raw bytes received
            int receivedBytes = receiveHandler.Receive(keyBytes);

            String key = Encoding.UTF8.GetString(keyBytes, 0, receivedBytes); //only converts bytes that were received to get key

            //Console.WriteLine("received key: " + key +"\n/end key\n\n");
            return key;

        }
        // gets the public key and sends it synchronously
        private void sendKey(Socket receiveHandler)
        {
            //string key = "server key";
            string publicKey = CryptoUtility.getPublicKey(); //gets keystring from utility class
            receiveHandler.Send(Encoding.UTF8.GetBytes(publicKey)); //converts key to UTF-8 Byte array and sends it synchronously
            //Console.WriteLine("sent key: " + publicKey + "\n/end key\n\n");
        }


        public void setMessageCallback(Action<Message> newObj)
        {
            controllerReturn = newObj;
        }
        public void setReceiveErrorCallback(Action<TransmissionErrorCode> newObj)
        {
            controllerReceiveErrorReport = newObj;
        }

        //converts two bytes to an integer
        public int lengthBytesToInt(Byte[] bytes)
        {
            return (int)BitConverter.ToInt16(bytes); //convert two bytes to a short then typecast to int
        }
    }

    

    //this class is responsible for representing the state of a receiving buffer
    public class BufferState
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
    }
}
