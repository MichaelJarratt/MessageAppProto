﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MessageApp
{
    /// <summary>
    /// NetworkUtility contains methods for sending and receiving data utilising RSA or AES encryption.
    /// This is done so that the server and client components share uniform code for sending and receiving, while -
    /// avoiding duplication
    /// It is also done so that the server and client can focus on maintaining state, rather than the details of transmission
    /// </summary>
    public static class NetworkUtility
    {
        /// <summary>
        /// Takes a socket, target IP and port (endpoint) and creates a connection to the endpoint.
        /// Returns a bool for the success of the operation.
        /// </summary>
        /// <param name="socket">The socket to connect to its endpoint</param>
        /// <param name="targetIP">The IP address to connect to</param>
        /// <param name="targetPort">The port to connect to</param>
        /// <returns>Bool if the socket has connected</returns>
        public static bool connectSync(Socket socket, string targetIP, int targetPort)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(targetIP), targetPort);
            try
            {
                socket.Connect(endPoint);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        /// <summary>
        /// Takes a socket, target IP and port (endpoint) and a timeout time, and creates a connection to the endpoint.
        /// Returns a bool for the success of the operation.
        /// </summary>
        /// <param name="socket">The socket to connect to its endpoint</param>
        /// <param name="timeout">How long to await a connection before timing out</param>
        /// <returns>Bool if the socket has connected</returns>
        public static bool connectSync(Socket socket, string targetIP, int targetPort, int timeout)
        {
            socket.SendTimeout = timeout;
            return connectSync(socket, targetIP, targetPort);
        }

        /// <summary>
        /// Takes a socket, targetIP and targetPort and asynchronously connects the socket. 
        /// Returns a Task<Socket> containing the socket
        /// </summary>
        /// <param name="socket">Socket object to connect</param>
        /// <param name="targetIP">IP of target machine</param>
        /// <param name="targetPort">Port to connect to</param>
        /// <returns>Task<Socket> containing the socket</returns>
        public static async Task<Socket> connectAsync(Socket socket, string targetIP, int targetPort)
        {
            try
            {
                await Task.Factory.FromAsync(socket.BeginConnect, socket.EndConnect, IPAddress.Parse(targetIP), targetPort, null);
            }
            catch(Exception)
            {
                throw new NetworkUtilityException(TransmissionErrorCode.CliNoEndPointConnection);
            }
            return socket;
        }

        /// <summary>
        /// Takes a socket and data and transmits the data synchronously to the endpoint.
        /// </summary>
        /// <param name="socket">Socket to use to send transmission</param>
        /// <param name="data">Data to send</param>
        public static void transmitSync(Socket socket, byte[] data)
        {
            socket.Send(data);
        }

        /// <summary>
        /// Takes a socket and uses it to synchronously receive data.
        /// Returns received data as byte[].
        /// </summary>
        /// <param name="socket">Socket to use to receive transmission</param>
        /// <returns>bytes received</returns>
        public static byte[] receiveSync(Socket socket)
        {
            byte[] receivedBytes = new byte[4096]; // receivedBytes holds the actual bytes received
            int byteCount = socket.Receive(receivedBytes); //byteCount are how many elements are taken up

            byte[] outArray = new byte[byteCount]; //creates new array of minimum required length
            Array.Copy(receivedBytes, outArray, byteCount); //copy received bytes over

            return outArray;
        }


        /// <summary>
        /// takes int32 number, typecasts to int16, converts it to Byte[] with two elements and returns it
        /// </summary>
        /// <param name="length">int32 representation of number of bytes</param>
        /// <returns>Byte[] representing an int 16 with two elements</returns>
        public static Byte[] lengthIntToBytes(int length)
        {
            short shortLength = (short)length; //typecast to short (anything greater than 65,536 will either throw exception or lose precision)
            return BitConverter.GetBytes(shortLength);
        }

        /// <summary>
        /// Takes an array of two bytes, representing a short, and converts it to an integer.
        /// </summary>
        /// <param name="bytes">Byte[2] representing the short</param>
        /// <returns>Integer conversion of the short</returns>
        public static int lengthBytesToInt(Byte[] bytes)
        {
            return (int)BitConverter.ToInt16(bytes); //convert two bytes to a short then typecast to int
        }

        //public static Task<TransmissionState> transmitAsync(TransmissionState transmissionState)
        //{
        //    Socket socket = transmissionState.socket;
        //}

        public async static Task<TransmissionState> receiveAsync(TransmissionState transmissionState)
        {
            Socket socket = transmissionState.socket;
            byte[] buffer = transmissionState.bytes;
            int offset = 0; //how far into buffer to begin placing things

            try
            {
                IAsyncResult asyncResult = socket.BeginReceive(buffer, offset, TransmissionState.bufferSize, SocketFlags.None, null, null);
                int bytesReceived = await Task<int>.Factory.FromAsync(asyncResult, _ => socket.EndReceive(asyncResult));
                transmissionState.totalBytesReceived = bytesReceived;
            }
            catch(SocketException e)
            {
                throw new NetworkUtilityException(TransmissionErrorCode.ServUnspecifiedError);
            }
            return transmissionState;
        }

        public async static Task<TransmissionState> transmitAsync(TransmissionState transmissionState)
        {
            Socket socket = transmissionState.socket;
            byte[] data = transmissionState.bytes;
            int offset = 0; //how far into buffer to begin sending things
            int size = data.Length; //how many bytes are to be sent from the buffer

            try
            {
                IAsyncResult asyncResult = socket.BeginSend(data, offset, size, SocketFlags.None, null, null);
                int bytesSent = await Task<int>.Factory.FromAsync(asyncResult, _ => socket.EndSend(asyncResult));
            }
            catch (SocketException e)
            {
                throw new NetworkUtilityException(TransmissionErrorCode.ServUnspecifiedError);
            }
            return transmissionState;
        }

        public async static void RSATransmit2(Socket socket, byte[] data, string receivedKey, Action reportSendSuccess)
        {
            Byte[] signatureBytes = CryptoUtility.signMessage(data); //creates signature for data
            Byte[] messageBytes = Encoding.UTF8.GetBytes(CryptoUtility.RSAEncryptData(data, receivedKey)); //creates byte array for encrypted message

            //calculate length headers
            int totalLength = signatureBytes.Length + messageBytes.Length + 6; //length of signature, message, itself and signature and message lengths //also I know it's a magic number but this is a prototype, no point setting up constants class for only this
            Byte[] totalLengthBytes = lengthIntToBytes(totalLength); //two bytes
            Byte[] signatureLengthBytes = lengthIntToBytes(signatureBytes.Length); //two bytes
            Byte[] messageLengthBytes = lengthIntToBytes(messageBytes.Length); //two bytes

            //temporary - testing what happens when corruption occurs
            //totalLengthBytes = lengthIntToBytes(1500);
            //signatureLengthBytes = lengthIntToBytes(260); 
            //messageLengthBytes = lengthIntToBytes(2000);
            //signatureBytes[0] = 0; // 1 in 256 chance that [0] would actually be 0
            //messageBytes[0] = 0;

            //stick lengths, signature and message together
            Byte[] transmissionBytes = new byte[totalLength]; //signature is always 253 bytes
            Array.Copy(totalLengthBytes, transmissionBytes, 2); //first two bytes - copies the two bytes from total length into transmission bytes
            Array.Copy(signatureLengthBytes, 0, transmissionBytes, 2, 2); //bytes 3&4 (2&3)
            Array.Copy(messageLengthBytes, 0, transmissionBytes, 4, 2); //bytes 5&6 (4&5)
            Array.Copy(signatureBytes, 0, transmissionBytes, 6, signatureBytes.Length); //starting after lengths, insert signature bytes (always 253)
            Array.Copy(messageBytes, 0, transmissionBytes, 6 + signatureBytes.Length, messageBytes.Length); //starting in position after signature bytes, add all message bytes

            TransmissionState transState = new TransmissionState();
            transState.socket = socket;
            transState.bytes = transmissionBytes;

            Task<TransmissionState> transmissionTask = transmitAsync(transState);

            transState = await transmissionTask;
            reportSendSuccess();

        }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="socket">Socket to send data with</param>
            /// <param name="data">Data to send to endpoint</param>
            /// <param name="receivedKey">The public key of the recipient</param>
            public static void RSATransmit(Socket socket, byte[] data, string receivedKey, Action reportSendSuccess)
        {
            Byte[] signatureBytes = CryptoUtility.signMessage(data); //creates signature for data
            Byte[] messageBytes = Encoding.UTF8.GetBytes(CryptoUtility.RSAEncryptData(data, receivedKey)); //creates byte array for encrypted message

            //calculate length headers
            int totalLength = signatureBytes.Length + messageBytes.Length + 6; //length of signature, message, itself and signature and message lengths //also I know it's a magic number but this is a prototype, no point setting up constants class for only this
            Byte[] totalLengthBytes = lengthIntToBytes(totalLength); //two bytes
            Byte[] signatureLengthBytes = lengthIntToBytes(signatureBytes.Length); //two bytes
            Byte[] messageLengthBytes = lengthIntToBytes(messageBytes.Length); //two bytes

            //temporary - testing what happens when corruption occurs
            //totalLengthBytes = lengthIntToBytes(1500);
            //signatureLengthBytes = lengthIntToBytes(260); 
            //messageLengthBytes = lengthIntToBytes(2000);
            //signatureBytes[0] = 0; // 1 in 256 chance that [0] would actually be 0
            //messageBytes[0] = 0;

            //stick lengths, signature and message together
            Byte[] transmissionBytes = new byte[totalLength]; //signature is always 253 bytes
            Array.Copy(totalLengthBytes, transmissionBytes, 2); //first two bytes - copies the two bytes from total length into transmission bytes
            Array.Copy(signatureLengthBytes, 0, transmissionBytes, 2, 2); //bytes 3&4 (2&3)
            Array.Copy(messageLengthBytes, 0, transmissionBytes, 4, 2); //bytes 5&6 (4&5)
            Array.Copy(signatureBytes, 0, transmissionBytes, 6, signatureBytes.Length); //starting after lengths, insert signature bytes (always 253)
            Array.Copy(messageBytes, 0, transmissionBytes, 6 + signatureBytes.Length, messageBytes.Length); //starting in position after signature bytes, add all message bytes

            try
            {
                socket.Send(transmissionBytes); //sends message synchronously
            }
            catch (SocketException e)
            {
                //this is the only "expected" exception
                if (e.SocketErrorCode == SocketError.TimedOut)
                {
                    //Console.WriteLine("Could not connect to target");
                    throw new NetworkUtilityException(TransmissionErrorCode.CliConnectionLost);
                }
                else
                {
                    throw new NetworkUtilityException(TransmissionErrorCode.CliTransmissionError); //unspecified transmission error
                }
            }
            //bytes have been sent, need confirmation they've been succesfully received and read
            try
            {
                socket.ReceiveTimeout = 2000; //waits up to two seconds for confirmation
                int confirmationBytes = socket.Receive(new byte[1024]); //don't care what is received, just that something is

                reportSendSuccess(); //reports successful sending to controller
            }
            catch (Exception) //server did not return confirmation
            {

                throw new NetworkUtilityException(TransmissionErrorCode.CliNoReceiveConfirmaton);
            }

        }

        /// <summary>
        /// Performs asynchronous receive and passes the BufferState object back to the provided callback.
        /// </summary>
        /// <param name="socket">The socket to receive the transmission</param>
        /// <param name="transState">BufferState object for the transmission</param>
        public static void AsyncReceive(Socket socket, BufferState transState)
        {
            // transState shorthand for TransmissionState. it's the object that maintains the state of the transmission between asynchronous method calls
            transState.socket = socket;

            socket.BeginReceive(transState.bytes, 0, BufferState.bufferSize, SocketFlags.None, new AsyncCallback(AsyncReceiveBytes), transState);
        }

        /// <summary>
        /// Asynchronously receives bytes and uses the callback in the BufferState to return the data to the invoker.
        /// </summary>
        /// <param name="ar">Asynchronous state object</param>
        private static void AsyncReceiveBytes(IAsyncResult ar)
        {
            BufferState transState = (BufferState)ar.AsyncState; //extracts transmission state from AsyncResult
            Socket socket = transState.socket;

            int bytesReceived = socket.EndReceive(ar); //bytesReceived is the number of bytes received this round
            transState.totalBytesReceived = transState.totalBytesReceived + bytesReceived; //self explanatory

            if (bytesReceived > 0) //if anything was received this round
            {
                //if the first two bits have been received and total length has not been extracted yet
                if (transState.totalLength == 0 && transState.totalBytesReceived >= 2)
                {   // extract totalLength
                    transState.totalLength = lengthBytesToInt(new ArraySegment<Byte>(transState.bytes, 0, 2).ToArray()); //creates array containg first two bits of transmission and converts them to int (total length) 
                }

                if (transState.totalBytesReceived < transState.totalLength) //if not every byte has been received
                {
                    //start another round
                    socket.BeginReceive(transState.bytes, 0, BufferState.bufferSize, SocketFlags.None, new AsyncCallback(AsyncReceiveBytes), transState);
                }
                else if (transState.totalLength == transState.totalBytesReceived) //every byte has been received
                {
                    //pass the transState containing the transmission information back via the callback
                    transState.callback(transState);
                }
                else if (transState.totalLength != transState.totalBytesReceived) //transmission error (total length too short)
                {
                    throw new NetworkUtilityException(TransmissionErrorCode.ServTotalLengthError); //total length error
                }
            }
            else //if no bytes were received, then a transmission error probably ocurred (total length longer than real length)
            {
                throw new NetworkUtilityException(TransmissionErrorCode.ServTotalLengthError); //total length error
            }
        }

        public static void asyncTransmit(Socket socket, BufferState transState, Action successCallback, Action<TransmissionErrorCode> exceptionCallback)
        {
            //socket.begin
        }


        
    }
}
