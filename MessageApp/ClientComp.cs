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
        private ManualResetEvent pubkeyReceived; //blocks message encryption until key is received

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
                sendSocket.Connect(targetEndPoint);

                pubkeyReceived = new ManualResetEvent(false);

                sendKey(sendSocket);
                receiveKey();
                send(message); //all synchronous and block while being done, the key exchange must be done first

                sendSocket.Close();
            }
        }

        //get key from CryptoUtility and send it to paired application
        private void sendKey(Socket sendSocket)
        {
            string publicKey = CryptoUtility.getPublicKey(); //gets keystring from utility class
            Console.WriteLine("sent key: " + publicKey + "\n/end key\n\n");
            sendSocket.Send(Encoding.UTF8.GetBytes(publicKey)); //converts key to UTF-8 Byte array and sends it synchronously
        }
        //synchronously receives key from server
        private void receiveKey()
        {
            Byte[] keyBytes = new Byte[1024]; //raw bytes received
            sendSocket.Receive(keyBytes);

            String key = Encoding.UTF8.GetString(keyBytes);
            key = System.Text.RegularExpressions.Regex.Replace(key, @"[\0]", string.Empty); //for some reason it is padded up to 1024 characters with \0's, this removes them

            Console.WriteLine("received key: " + key +"\n/end key\n\n");
            receivedPublicKeyString = key;
            pubkeyReceived.Set();
        }

        //sends provided message to server
        private void send(string message)
        {
            //while(receivedPublicKeyString == null)
            //{ }
            pubkeyReceived.WaitOne();
            string encMessage = CryptoUtility.encryptData(message, receivedPublicKeyString);
            Console.WriteLine($"Encrypted message:\n{encMessage}\n/End encrypted message");

            //Byte[] messageByteArray = Encoding.UTF8.GetBytes(message.Trim() + "<EOF>"); //trims message, adds flag and then converts it to bytes
            Byte[] messageByteArray = Encoding.UTF8.GetBytes(encMessage+ "<EOF>"); //adds flag to encrypted message and then converts it to bytes

            //sendSocket = new Socket(IPAddress.Parse(targetIP).AddressFamily, SocketType.Stream, ProtocolType.Tcp); //has to be reinstantiated for reasons
            try
            {
                //sendSocket.Connect(targetEndPoint); // tries to connect to another client
                sendSocket.Send(messageByteArray); //sends message synchronously
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
