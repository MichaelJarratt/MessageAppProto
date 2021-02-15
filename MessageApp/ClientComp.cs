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
                
                send(publicKeyMessage());
                receiveKey();
                send(message); //all synchronous and block while being done, the key exchange must be done first
                
            }
        }

        //gets the public key and returns it in a sendable format
        private String publicKeyMessage()
        {
            //code
            return "public key";
        }

        //synchronously receives key from server
        private void receiveKey()
        {

        }

        //sends provided message to server
        private void send(string message)
        {
            Byte[] messageByteArray = Encoding.UTF8.GetBytes(message.Trim() + "<EOF>"); //trims message, adds flag and then converts it to bytes

            //sendSocket = new Socket(IPAddress.Parse(targetIP).AddressFamily, SocketType.Stream, ProtocolType.Tcp); //has to be reinstantiated for reasons
            try
            {
                sendSocket.Connect(targetEndPoint); // tries to connect to another client
                sendSocket.Send(messageByteArray); //sends message synchronously
                sendSocket.Close();
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
