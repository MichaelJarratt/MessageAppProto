using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Security.Cryptography;
using System.Timers;

namespace MessageApp
{
    class MessageApp
    {
        public int localPortNo; //port to listen to
        public int targetPortNo; //port to send to
        public IPAddress targetIP = null; //IP to send to

        public static ManualResetEvent blockMain = new ManualResetEvent(false); //used to block the main thread from executing while waiting for connections

        public void MainLoop()
        {
            ClientComp clientComp = new ClientComp(targetIP.ToString());
            ServerComp serverComp = new ServerComp();
            serverComp.setMessageCallback(getMessageCallbackHandler);

            Console.WriteLine($"Listening on port {localPortNo}");
            Console.WriteLine($"Listening on IP <all interfaces>");
            Console.WriteLine($"Sending to port {targetPortNo}");
            Console.WriteLine($"Sending to IP {targetIP}");

            serverComp.startConnectionListenLoop();
            clientComp.startListenToConsoleLoop();

        }

        //this method is called back by ServerComp when a message is received, it takes the received message as a parameter
        public void getMessageCallbackHandler(string message)
        {
            Console.WriteLine($"Received message: {message}");
        }

        //entry point
        static void Main(string[] args)
        {
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
            targetIP = IPAddress.Parse("192.168.1.192");
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
