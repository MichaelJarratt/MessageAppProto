using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MessageApp
{
    class ServerComp
    {
        private int listenPort; //port the socket will listen to

        private Socket connectionListener; //socker that listens for incoming connections
        private ManualResetEvent blockConnectionListenLoop; //blocks loop listening to connections until a connection is received
        
        Action<String> messageAppReturn; // Action can hold a reference to a method, this references the call back handler on MessageApp that prints the received message
        //Action<String> is a void return method that takes one string. Action<String,String> takes two
        //Func<String,String> is a String return method that takes one string. Func<String,String,String> takes two strings.

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

            blockConnectionListenLoop = new ManualResetEvent(false);
        }

        // THREAD //
        //loops indefinitely, listens for connections and creates new threads to handle them
        //if it needs to be stopped, I could store reference to the thread in a field so a method could be used to kill it
        private void connectionListenLoop()
        {
            while (true)
            {
                blockConnectionListenLoop.Reset(); //makes threads set to "wait one" be blocked
                
                connectionListener.BeginAccept(new AsyncCallback(acceptTCPRequest), connectionListener); //when receiving a request it creates a new thread (acceptTCPRequest
                blockConnectionListenLoop.WaitOne();
            }
        }
        //offers external method to start the loop
        public void startConnectionListenLoop()
        {
            Thread connectionListenLoopThread = new Thread(connectionListenLoop);
            connectionListenLoopThread.Start();
        }

        // THREAD //
        //started when connectionListener gets a request
        //handles creating a receiving socket and then terminates
        private void acceptTCPRequest(IAsyncResult ar)
        {
            blockConnectionListenLoop.Set(); //unblocks the connection listen loop (so new connections can be handled)

            Socket conListenerTemp = (Socket)ar.AsyncState; //gets the connectionListener socket from the listenLoop (although couldn't this be done via the field?)
            Socket receiveHandler = conListenerTemp.EndAccept(ar); //gets socket that will receive bytes

            String keyString = receiveKey(receiveHandler); //block
            sendKey(receiveHandler); //block .keys are exchanged syncrhonously (must be completed first), then continue receiving message

            BufferState bufferState = new BufferState(); //creates new bit buffer for receiving socket
            bufferState.socket = receiveHandler; //places socket this buffer is for inside so it can be passed in the IAsyncResult
            bufferState.keyString = keyString; //stores senders public key in bufferState
            receiveHandler.BeginReceive(bufferState.bytes, 0, BufferState.bufferSize, SocketFlags.None, new AsyncCallback(receiveBytes), bufferState);
        }
        
        //  THREAD  //
        //started when a request is accepted, and a socket is created to receive the bits.
        //handles receiving the bytes and then displaying the message when at <EOF>
        private void receiveBytes(IAsyncResult ar)
        {
            //Console.WriteLine("receive cycle");
            //extract socket and buffer from argument
            BufferState bufferState = (BufferState)ar.AsyncState; //gets the buffer from the argument
            Socket receiveHandler = bufferState.socket; //between beginReceive and now the receiver has been getting bits

            string messageString = String.Empty;
            int bytesReceived = receiveHandler.EndReceive(ar); //why does ar need to be passed?

            if(bytesReceived > 0) //if any bytes were received this cycle
            {
                bufferState.stringBuilder.Append(Encoding.UTF8.GetString(bufferState.bytes, 0, bytesReceived)); //convert received bytes stored in buffer to a mutable string, bytes buffer is overwritten each time
                messageString = bufferState.stringBuilder.ToString(); //converts stringbuilder to string

                if (messageString.IndexOf("<EOF>") != -1) //if the end of file tag has been received (transmission has completed)
                {
                    messageString = messageString.Substring(0, messageString.Length - 5); //removes <EOF> tag
                    //Console.WriteLine($"--{content}--"); //print received message to console
                    //Console.WriteLine(messageString);
                    //Console.WriteLine();
                    decryptMessage(messageString,bufferState); //decrypt message, senders public key is stored in bufferState
                }
                else //transmission is still going but has not finished yet
                {
                    //begin a new instance of this thread, presumably terminating this one
                    //note that buffer is passed again, meaning state is maintained between instances of this thread
                    receiveHandler.BeginReceive(bufferState.bytes, 0, BufferState.bufferSize, SocketFlags.None, new AsyncCallback(receiveBytes), bufferState);
                }
            }
            //arrive here when no bytes were received
            //or when <EOF> were received
        }

        //decrypts the message and returns it to MessageApp
        private void decryptMessage(string messageString, BufferState bufferState)
        {
            string privateKey = CryptoUtility.getPrivateKey(); //gets string represenation of receivers private key
            messageString = CryptoUtility.decryptData(messageString, privateKey); //decrypts message with private key

            messageString = CryptoUtility.decryptData(messageString, bufferState.keyString); //decrypts/unsigns message with senders public key   
            //Console.WriteLine($"Received encrypted message:\n{messageString}\nMessage end.");
            
            messageAppReturn(messageString);
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

        //callback called by receiveBytes, will use received key to decode message and the callback the message to MessageApp
        private void completeReceive()
        {

        }

        public void setMessageCallback(Action<String> NewObj)
        {
            messageAppReturn = NewObj;
        }
    }

    

    //this class is responsible for representing the state of a receiving buffer
    public class BufferState
    {
        public const int bufferSize = 1024; //size of buffer used to receive bytes
        public byte[] bytes = new byte[bufferSize]; //buffer used to receive bytes
        public StringBuilder stringBuilder = new StringBuilder(); //used to build strings from bytes in the buffer
        public Socket socket = null; //the socket this is acting as the buffer for
        public string keyString = String.Empty; //holds the key used to decrypt the data
    }
}
