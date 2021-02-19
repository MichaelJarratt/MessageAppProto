using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using System.Net.Sockets;

namespace MessageApp
{
    class ClientComp
    {
        private int targetPort;
        private string targetIP;

        private Socket sendSocket; //socket creating outbound connections
        IPEndPoint targetEndPoint; //ip/port combo to send messages to

        private string receivedPublicKeyString;

        public ClientComp(string targetIP)
        {
            this.targetIP = targetIP;
            targetPort = 65432;
            init();
        }
        public ClientComp(string targetIP, int targetPort)
        {
            this.targetIP = targetIP;
            this.targetPort = targetPort;
            init();
        }

        //does set up tasks common to each constructor
        private void init()
        {
            targetEndPoint = new IPEndPoint(IPAddress.Parse(targetIP), targetPort); //ip/port combo to send messages to

            //sendSocket = new Socket(IPAddress.Parse(targetIP).AddressFamily, SocketType.Stream, ProtocolType.Tcp); //instantiate socket
            //sendSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
        }

        // THREAD //
        //pauses execution while listening for input, then establishes connection with server and sends message
        private void listenToConsoleLoop()
        {
            while (true)
            {
                Console.WriteLine("Send message:");
                String message = Console.ReadLine(); //pauses execution /of this thread/ while waiting for input
                sendSocket = new Socket(IPAddress.Parse(targetIP).AddressFamily, SocketType.Stream, ProtocolType.Tcp); //has to be reinstantiated for reasons
                try
                {
                    sendSocket.Connect(targetEndPoint);


                    sendKey(sendSocket);
                    receiveKey();
                    send(message); //all synchronous and block while being done, the key exchange must be done first
                }
                catch(SocketException e)
                {
                    Console.WriteLine("Could not connect to endpoint");
                }
                sendSocket.Close();
            }
        }

        //get key from CryptoUtility and send it to paired application
        private void sendKey(Socket sendSocket)
        {
            string publicKey = CryptoUtility.getPublicKey(); //gets keystring from utility class
            sendSocket.Send(Encoding.UTF8.GetBytes(publicKey)); //converts key to UTF-8 Byte array and sends it synchronously
            //Console.WriteLine("sent key: " + publicKey + "\n/end key\n\n");
        }
        //synchronously receives key from server
        private void receiveKey()
        {
            Byte[] keyBytes = new Byte[1024]; //raw bytes received
            int receivedBytes = sendSocket.Receive(keyBytes);

            String key = Encoding.UTF8.GetString(keyBytes,0,receivedBytes); //only converts bytes that were received to get key

            //Console.WriteLine("received key: " + key +"\n/end key\n\n");
            receivedPublicKeyString = key;
        }

        //sends provided message to server
        private void send(string message)
        {
            //get signature of message
            //Byte[] signatureBytes = CryptoUtility.signMessage(message); //gets byte array representing signature of message signed with private key


            //String encMessage = CryptoUtility.encryptData(message, receivedPublicKeyString); //encrypts message with public key of recipient
            //Byte[] messageBytes = Encoding.UTF8.GetBytes(encMessage+ "<EOF>"); //adds flag to encrypted message and then converts it to bytes
            //Console.WriteLine($"Encrypted message:\n{encMessage}\n/End encrypted message");

            //bool valid = CryptoUtility.validateSignature(Encoding.UTF8.GetBytes(message), signatureBytes, CryptoUtility.getPublicKey());

            //Byte[] messageBytes = Encoding.UTF8.GetBytes(message); //gets array of bytes from message
            Byte[] signatureBytes = CryptoUtility.signMessage(message); //creates signature for message
            Byte[] messageBytes = Encoding.UTF8.GetBytes(CryptoUtility.encryptData(message,receivedPublicKeyString)); //creates byte array for encrypted message



            int totalLength = signatureBytes.Length + messageBytes.Length + 6; //length of signature, message, itself and signature and message lengths //also I know it's a magic number but this is a prototype, no point setting up constants class for only this
            Byte[] totalLengthBytes = lengthIntToBytes(totalLength); //two bytes
            Byte[] signatureLengthBytes = lengthIntToBytes(signatureBytes.Length); //two bytes
            Byte[] messageLengthBytes = lengthIntToBytes(messageBytes.Length); //two bytes

            //stick lengths, signature and message together
            Byte[] transmissionBytes = new byte[totalLength]; //signature is always 253 bytes
            Array.Copy(totalLengthBytes, transmissionBytes, 2); //first two bytes - copies the two bytes from total length into transmission bytes
            Array.Copy(signatureLengthBytes, 0 , transmissionBytes, 2, 2); //bytes 3&4 (2&3)
            Array.Copy(messageLengthBytes, 0 , transmissionBytes, 4, 2); //bytes 5&6 (4&5)
            Array.Copy(signatureBytes,0 , transmissionBytes, 6, signatureBytes.Length); //starting after lengths, insert signature bytes (always 253)
            Array.Copy(messageBytes, 0, transmissionBytes, 6+signatureBytes.Length, messageBytes.Length); //starting in position after signature bytes, add all message bytes

            //Console.WriteLine("signature: " + System.Convert.ToBase64String(signatureBytes));

            //sendSocket = new Socket(IPAddress.Parse(targetIP).AddressFamily, SocketType.Stream, ProtocolType.Tcp); //has to be reinstantiated for reasons
            try
            {
                //sendSocket.Connect(targetEndPoint); // tries to connect to another client
                sendSocket.Send(transmissionBytes); //sends message synchronously
                //sendSocket.Close();
            }
            catch (SocketException e)
            {
                Console.WriteLine("Could not connect to target");
                Console.WriteLine("Socket error code: " + e.ErrorCode);
                Console.WriteLine(e.Message);
            }
        }

        //takes int32 number, typecasts to int16, converts it to Byte[] with two elements and returns it
        private Byte[] lengthIntToBytes(int length)
        {
            short shortLength = (short) length; //typecast to short (anything greater than 65,536 will either throw exception or loose precision
            return BitConverter.GetBytes(shortLength);
        }

        //offers way to begin the main loop
        public void startListenToConsoleLoop()
        {
            Thread listenToConsoleLoopThread = new Thread(listenToConsoleLoop);
            listenToConsoleLoopThread.Start();
        }
    }
}
