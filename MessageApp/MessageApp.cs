using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Security.Cryptography;
using System.Timers;

namespace MessageApp
{
    /*
     * this is the messageapp console interface
     */
    class MessageApp
    {
        public int localPortNo; //port to listen to
        public int targetPortNo; //port to send to
        public IPAddress targetIP = null; //IP to send to

        public static ManualResetEvent blockMain = new ManualResetEvent(false); //used to block the main thread from executing while waiting for connections

        private ClientComp clientComp;

        public void MainLoop()
        {
            clientComp = new ClientComp(targetIP.ToString());
            ServerComp serverComp = new ServerComp();
            serverComp.setMessageCallback(getMessageCallbackHandler);
            serverComp.setReceiveErrorCallback(messageReceiveErrorCallbackHandler);
            clientComp.setSendErrorCallBack(messageSendErrorCallbackHandler);

            Console.WriteLine($"Listening on port {localPortNo}");
            Console.WriteLine($"Listening on IP <all interfaces>");
            Console.WriteLine($"Sending to port {targetPortNo}");
            Console.WriteLine($"Sending to IP {targetIP}");

            serverComp.startConnectionListenLoop();
            //clientComp.startListenToConsoleLoop();

            Thread listenToConsoleLoopThread = new Thread(listenToConsoleLoop); //done in thread to not block execution of main thread
            listenToConsoleLoopThread.Start();

        }
        //as it says on the tin, constantly listens to the console, when the user enters a message it will be sent
        //to the communication target
        private void listenToConsoleLoop()
        {
            while (true)
            {
                Console.WriteLine("Send message:");
                String message = Console.ReadLine(); //pauses execution of this thread while waiting for input
                clientComp.sendMessage(message);
            }
        }

        //this method is called back by ServerComp when a message is received, it takes the received message as a parameter
        public void getMessageCallbackHandler(Message message)
        {
            String messageString = message.message;
            Console.WriteLine($"Received message: {messageString}");
        }

        public void messageReceiveErrorCallbackHandler(TransmissionErrorCode errorCode)
        {
            Console.WriteLine($"Error receiving message - code {errorCode.ToString()}");
        }

        public void messageSendErrorCallbackHandler(TransmissionErrorCode errorCode)
        {
            if (errorCode == TransmissionErrorCode.CliNoEndPointConnection)
            {
                Console.WriteLine("Could not connect to target");
            }
            else 
            {
                Console.WriteLine("Error sending message");
            }
            
        }

        //entry point
        static void Main(string[] args)
        {

            short totalLength = 1000;
            byte[] lengthBytes = BitConverter.GetBytes(totalLength);
            int totalLengthInt = BitConverter.ToInt16(lengthBytes, 0); //calculate int16 (two bytes) then implicitly typecast to regular int32

            string test = "1234567890"; //byte array should be 10 bytes
            byte[] testBytes = Encoding.UTF8.GetBytes(test);

            //CryptoUtility.encrypt("yee yee ass haircut");
            Console.WriteLine(args);
            MessageApp messageApp;
            if (args.Length == 2) //if two arguments are provided (listen and send ports)
            {
                messageApp = new MessageApp(Int32.Parse(args[0]), Int32.Parse(args[1]));
            }
            else if (args.Length == 3) // three arguments, listen port, target port, target IP
            {
                messageApp = new MessageApp(Int32.Parse(args[0]), Int32.Parse(args[1]), args[2]);
            }
            else //default constructor
            {
                messageApp = new MessageApp(); //break out of static
            }
            messageApp.MainLoop();
        }

        //default constructors
        public MessageApp()
        {
            localPortNo = 65432; //5678
            targetPortNo = 65432; //8765
            //targetIP = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0]; //uses first IP of machine
            targetIP = IPAddress.Parse("192.168.1.192"); //192.168.1.192 = my laptop
        }
        //instantiate with local and target port numbers
        public MessageApp(int localPortNo, int targetPortNo)
        {
            this.localPortNo = localPortNo;
            this.targetPortNo = targetPortNo;
            targetIP = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0]; //uses first IP of machine
        }
        //instantiate with iven local and target port numbers
        public MessageApp(int localPortNo, int targetPortNo, String targetIP)
        {
            this.localPortNo = localPortNo;
            this.targetPortNo = targetPortNo;
            this.targetIP = IPAddress.Parse(targetIP); //uses supplied IP
        }
    }
}
