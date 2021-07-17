using System;
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
            //socket.SendTimeout = 5;
            try
            {
                socket.Send(data);
            }
            catch(SocketException e)
            {
                Console.WriteLine(e.Message);
                throw new NetworkUtilityException(TransmissionErrorCode.CliTransmissionError);
            }
        }

        /// <summary>
        /// Takes a socket and uses it to synchronously receive data.
        /// Returns received data as byte[].
        /// </summary>
        /// <param name="socket">Socket to use to receive transmission</param>
        /// <returns>bytes received</returns>
        public static byte[] receiveSync(Socket socket)
        {
            //socket.ReceiveTimeout = 4;

            byte[] receivedBytes = new byte[4096]; // receivedBytes holds the actual bytes received
            int byteCount; //byteCount is how many elements are taken up
            try
            {
                byteCount = socket.Receive(receivedBytes); 
            }
            catch(SocketException e)
            {
                Console.WriteLine(e.Message);
                throw new NetworkUtilityException(TransmissionErrorCode.ServReceiveFail);
            }
            byte[] outArray = new byte[byteCount]; //creates new array of minimum required length
            Array.Copy(receivedBytes, outArray, byteCount); //copy received bytes over

            return outArray;
        }

        /// <summary>
        /// Takes a TransmissionState object (socket) and performs an asynchronous receive.
        /// Returns TransmissionState (socket, bytes, totalBytesReceived)
        /// </summary>
        /// <param name="transmissionState">TransmissionState object (socket)</param>
        /// <returns>TransmissionState object (socket, bytes, totalBytesReceived)</returns>
        public async static Task<TransmissionState> receiveAsync(TransmissionState transmissionState)
        {
            Socket socket = transmissionState.socket;
            byte[] buffer = transmissionState.bytes;
            int offset = 0; //how far into buffer to begin placing things

            try
            {
                IAsyncResult asyncResult = socket.BeginReceive(buffer, offset, buffer.Length, SocketFlags.None, null, null);
                int bytesReceived = await Task<int>.Factory.FromAsync(asyncResult, _ => socket.EndReceive(asyncResult));
                transmissionState.totalBytesReceived = bytesReceived;
            }
            catch(SocketException e)
            {
                throw new NetworkUtilityException(TransmissionErrorCode.ServUnspecifiedError);
            }
            return transmissionState;
        }

        /// <summary>
        /// Takes a TransmissionState object (socket, bytes) and performs an asynchronous transmission.
        /// Returns a TransmissionState object
        /// </summary>
        /// <param name="transmissionState"></param>
        /// <returns></returns>
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
        
    }
}
