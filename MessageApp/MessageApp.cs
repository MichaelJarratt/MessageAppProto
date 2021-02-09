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
        public static ManualResetEvent blockMain = new ManualResetEvent(false); //used to block the main thread from executing while waiting for connections
        public int localPortNo; //port to listen to
        public int targetPortNo; //port to send to
        public IPAddress targetIP = null; //IP to send to

        public IPEndPoint localEndPoint; //ip/port combo to listen to
        //will need a target EndPoint too



        public void MainLoop()
        {
            Socket listener = setUpListener(); //gets listener that listens for any incoming TCP requests from any network interface for port <localPortNo>
            Thread consoleInput = new Thread(listenToConsole); //listenToConsole becmes a thread and can act independently of main thread
            consoleInput.Start(); //starts thread to listen to console input

            listener.Bind(localEndPoint); //tells listener to listen to ip/port combo
            listener.Listen(10); //sockets starts listening and will queue up to 10 requests to be serviced

            Console.WriteLine($"Listening on port {localPortNo}");
            Console.WriteLine($"Listening on IP {localEndPoint.Address}");
            Console.WriteLine($"Sending to port {targetPortNo}");
            Console.WriteLine($"Sending to IP {targetIP}");


            while (true)
            {
                blockMain.Reset(); //reset flag (which is set by the listener handler to signify when main should create a new thread to listen for new connections)

                Console.WriteLine("\nWating for new message..");
                listener.BeginAccept(new AsyncCallback(acceptTCPRequest), listener); //listens and creates a thread (acceptTCPRequest) when it gets a request, listener is passed to the thread

                blockMain.WaitOne(); //block thread until it is given signal to resume (in this case resuming creates a new listening thread and then pauses again)
            }
        }

        //  THREAD  //
        //started when listener in main thread receives a TCP request, handles creating a socket to receive the data and then terminates
        private void acceptTCPRequest(IAsyncResult ar)
        {
            //Console.WriteLine("accepted request");
            blockMain.Set(); //tells the main thread to continue (resulting in creation of another thread to handle new requests)

            Socket lisener = (Socket)ar.AsyncState; //gets listener socket from main thread with info about the received request
            Socket handler = lisener.EndAccept(ar); //returns socket configured to receive bytes from connection

            BufferState buffer = new BufferState(); //creates buffer for the handler
            buffer.socket = handler; //sets socket configured to handle connection as the socket the buffer is for
            //creates thread that received bytes from connected socket and passes the BufferState instance to it
            handler.BeginReceive(buffer.bytes, 0, BufferState.bufferSize, SocketFlags.None, new AsyncCallback(receiveBytes), buffer);
        }

        //  THREAD  //
        //started then a request is accepted, handles receiving the bytes and then displaying the message when at <EOF>
        private void receiveBytes(IAsyncResult ar)
        {
            //Console.WriteLine("receiving bytes");
            String content = String.Empty; //will hold what has been received so far from the connected socket

            BufferState buffer = (BufferState)ar.AsyncState; //gets buffer object containing the socket to handle this transfer
            Socket handler = buffer.socket; //gets the socket from the buffer object

            int bytesReceived = handler.EndReceive(ar); //ends recieiving data that has been sent so far, but does not terminate connection (as data is sent as a stream, not all at once)
            
            if (bytesReceived > 0) //if any bytes were received via the connection
            {
                buffer.stringBuilder.Append(Encoding.UTF8.GetString(buffer.bytes, 0, bytesReceived)); //convert received bytes stored in buffer to a string, buffer is overwritten each time
                content = buffer.stringBuilder.ToString(); //converts stringbuilder to string
                //Console.WriteLine(content);
                if (content.IndexOf("<EOF>") != -1) //if the end of file tag has been received (transmission has completed)
                {
                    content = content.Substring(0, content.Length - 5); //removes <EOF> tag
                    //Console.WriteLine($"--{content}--"); //print received message to console
                    Console.WriteLine(content);
                    Console.WriteLine();
                }
                else //transmission is still going but has not finished yet
                {
                    //begin a new instance of this thread, presumably terminating this one
                    //note that buffer is passed again, meaning state is maintained between instances of this thread
                    handler.BeginReceive(buffer.bytes, 0, BufferState.bufferSize, SocketFlags.None, new AsyncCallback(receiveBytes), buffer);
                }
            }
            //arrive here either if <EOF> is received, or no bytes were received
            //handler.Close(); //if nothing was received then assume connection was lost, if <EOF>, then it is no longer needed.
        }

        //  THREAD  //
        //independent of main thread, listens to console input, when something is typed it will handle sending it to targetPortNo
        private void listenToConsole()
        {
            while (true)
            {
                String message = Console.ReadLine(); //blocks while waiting
                                                     //Console.WriteLine("you typed: " + message);

                //sending is done synchronously as it is done straight away and won't lock up the application (like listening)

                IPEndPoint targetEndPoint = new IPEndPoint(targetIP, targetPortNo); //what IP and port to send message to
              
                Socket sender = new Socket(targetIP.AddressFamily, SocketType.Stream, ProtocolType.Tcp); //creates TCP socket
                try //try to send message
                {
                    sender.Connect(targetEndPoint); //connect to socket listening to port at IP
                    sender.Send(Encoding.UTF8.GetBytes(message.Trim() + "<EOF>")); //converts types message to bytes and then sends it
                    sender.Close(); //close socket as it is no longer needed
                }
                catch (SocketException e) //if there is an exception it's going to be that no socket received it
                {
                    Console.WriteLine("Could not connect to target");
                    Console.WriteLine(e.ErrorCode);
                    Console.WriteLine(e.Message);
                    //throw e;
                }
               
            }
        }

        //sets up and returns a listener configured to listen to localPortNo
        private Socket setUpListener()
        {
            //IPAddress ip = IPAddress.Any; //IP to listen to is any on this machine
            IPAddress ip = IPAddress.Any;
            //IPAddress ip = IPAddress.Parse("fe80::24ec:3a1:2776:42aa%7");
            localEndPoint = new IPEndPoint(ip, localPortNo); //"listen to port <localPortNo> on all network interfaces (each IP)"
            Socket listener = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp); //defines a socket that uses Tcp to Stream data to an endpoint. this socket is part of the IPv6 address family (basically it's using ipv6)
                                                                                                 // listener.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false); //supposedly makes it listen to ipv6 and ipv4
            return listener;
        }



        //entry point
        static void Main(string[] args)
        {
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
            targetIP = IPAddress.Parse("86.29.29.12");
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

    //this class is responsible for representing the state of a receiving buffer
    public class BufferState
    {
        public const int bufferSize = 1024; //size of buffer used to receive bytes
        public byte[] bytes = new byte[bufferSize]; //buffer used to receive bytes
        public StringBuilder stringBuilder = new StringBuilder(); //used to build strings from bytes in the buffer
        public Socket socket = null; //the socket this is acting as the buffer for
    }
}
