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
            Byte[] signatureBytes = CryptoUtility.signMessage(message); //gets byte array representing signature of message signed with private key

            String encMessage = CryptoUtility.encryptData(message, receivedPublicKeyString); //encrypts message with public key of recipient
            Console.WriteLine($"Encrypted message:\n{encMessage}\n/End encrypted message");
            Byte[] messageByteArray = Encoding.UTF8.GetBytes(encMessage+ "<EOF>"); //adds flag to encrypted message and then converts it to bytes

            //stick signature and message together
            Byte[] transmissionBytes = new byte[253+messageByteArray.Length]; //signature is always 253 bytes
            Array.Copy(signatureBytes, transmissionBytes, 253); //fills first 253 bytes of array with signature
            Array.Copy(messageByteArray, 0, transmissionBytes, 254, messageByteArray.Length); //starting in position after signature bytes, add all message bytes

            Console.WriteLine("signature: " + System.Convert.ToBase64String(signatureBytes));

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

        //offers way to begin the main loop
        public void startListenToConsoleLoop()
        {
            Thread listenToConsoleLoopThread = new Thread(listenToConsoleLoop);
            listenToConsoleLoopThread.Start();
        }
    }
}
