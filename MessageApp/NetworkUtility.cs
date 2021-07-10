﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace MessageApp
{
    /// <summary>
    /// NetworkUtil contains methods for sending and receiving data utilising RSA or AES encryption.
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
        /// <returns>Bool if the socket has connected</returns>
        public static bool connect(Socket socket, string targetIP, int targetPort)
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
        public static bool connect(Socket socket, string targetIP, int targetPort, int timeout)
        {
            socket.SendTimeout= timeout;
            return connect(socket, targetIP, targetPort);
        }

        /// <summary>
        /// Takes a socket and data and transmits the data synchronously to the endpoint.
        /// </summary>
        /// <param name="socket">Socket to use to send transmission</param>
        /// <param name="data">Data to send</param>
        public static void PlainTextTransmit(Socket socket, byte[] data)
        {
            socket.Send(data);
        }

        /// <summary>
        /// Takes a socket and uses it to synchronously receive data.
        /// Returns received data as byte[].
        /// </summary>
        /// <param name="socket">Socket to use to receive transmission</param>
        /// <returns>bytes received</returns>
        public static byte[] PlainTextReceive(Socket socket)
        {
            byte[] receivedBytes = new byte[1024]; // receivedBytes holds the actual bytes received
            int byteCount = socket.Receive(receivedBytes); //byteCount are how many elements are taken up

            byte[] outArray = new byte[byteCount]; //creates new array of minimum required length
            Array.Copy(receivedBytes, outArray, byteCount);

            return outArray;
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
            Byte[] messageBytes = Encoding.UTF8.GetBytes(CryptoUtility.RSAEncryptData(data,receivedKey)); //creates byte array for encrypted message

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

        //public static byte[] RSAReceive()
        //{

        //}

        public static void AESTransmit()
        {

        }

        //public static byte[] AESReceive()
        //{

        //}

        /// <summary>
        /// takes int32 number, typecasts to int16, converts it to Byte[] with two elements and returns it
        /// </summary>
        /// <param name="length">int32 representation of number of bytes</param>
        /// <returns>Byte[] representing an int 16 with two elements</returns>
        private static Byte[] lengthIntToBytes(int length)
        {
            short shortLength = (short)length; //typecast to short (anything greater than 65,536 will either throw exception or lose precision)
            return BitConverter.GetBytes(shortLength);
        }
    }
}
