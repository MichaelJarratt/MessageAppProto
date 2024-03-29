﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using System.Net.Sockets;

namespace MessageApp
{
    public class ClientComp
    {
        private int targetPort;
        private string targetIP;
        private Socket sendSocket; //socket creating outbound connections
        IPEndPoint targetEndPoint; //ip/port combo to send messages to

        private string receivedPublicKeyString;

        private Action<TransmissionErrorCode> controllerSendErrorReport; //callback method to inform controller of a send error
        private Action<Message> controllerSendConfirmation; //calling this method confirms that the message was sent succesfully
        private Message currentMessage; //reference to currently used Message object

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
        }

        // takes message object, stores refeence and has the message sent by sendMessage(string)
        public void sendMessage(Message message)
        {
            currentMessage = message; //stores reference
            sendMessage(message.message); //extracts message text and sends it
        }

        //takes message as input, establishes connection to target and exchanged keys, then passes control over to transmit
        //to create and send the transmission
        public void sendMessage(string message) //still public so that console version does not break
        {
            sendSocket = new Socket(IPAddress.Parse(targetIP).AddressFamily, SocketType.Stream, ProtocolType.Tcp); //create socket to handle sending
            sendSocket.SendTimeout = 5000; //try for 5 seconds to get a connection
            try
            {
                sendSocket.Connect(targetEndPoint);
                sendKey(sendSocket);
                receiveKey();
                transmit(message); //all synchronous and block while being done, the key exchange must be done first
            }
            catch(SocketException e)
            {
                if (e.ErrorCode == 10060) //times out, AKA no target to connect to
                {
                    controllerSendErrorReport(TransmissionErrorCode.CliNoEndPointConnection);
                }
                else //some other kind of uncaught exception (not from transmit though, that catches its own errors)
                {
                    controllerSendErrorReport(TransmissionErrorCode.CliKeyExchangeFail);
                }
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
        private void transmit(string message)
        {
            Byte[] signatureBytes = CryptoUtility.signMessage(message); //creates signature for message
            Byte[] messageBytes = Encoding.UTF8.GetBytes(CryptoUtility.encryptData(message,receivedPublicKeyString)); //creates byte array for encrypted message

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
            Array.Copy(signatureLengthBytes, 0 , transmissionBytes, 2, 2); //bytes 3&4 (2&3)
            Array.Copy(messageLengthBytes, 0 , transmissionBytes, 4, 2); //bytes 5&6 (4&5)
            Array.Copy(signatureBytes,0 , transmissionBytes, 6, signatureBytes.Length); //starting after lengths, insert signature bytes (always 253)
            Array.Copy(messageBytes, 0, transmissionBytes, 6+signatureBytes.Length, messageBytes.Length); //starting in position after signature bytes, add all message bytes

            //Console.WriteLine("signature: " + System.Convert.ToBase64String(signatureBytes));

            try
            {
                sendSocket.Send(transmissionBytes); //sends message synchronously
            }
            catch (SocketException e)
            {
                //this is the only "expected" exception
                if (e.SocketErrorCode == SocketError.TimedOut)
                {
                    //Console.WriteLine("Could not connect to target");
                    controllerSendErrorReport(TransmissionErrorCode.CliConnectionLost); //could not connect to target
                }
                else
                {
                    controllerSendErrorReport(TransmissionErrorCode.CliTransmissionError); //unspecified transmission error
                }
            }
            //bytes have been sent, need confirmation they've been succesfully received and read
            try
            {
                sendSocket.ReceiveTimeout = 2000; //waits up to two seconds for confirmation
                int confirmationBytes = sendSocket.Receive(new byte[1024]); //don't care what is received, just that something is

                confirmSendSuccess(); //reports successful sending to controller
            }
            catch (Exception) //server did not return confirmation
            {

                controllerSendErrorReport(TransmissionErrorCode.CliNoReceiveConfirmaton);
            }
        }

        //returns currentMessage to controller and confirms that it was sent
        private void confirmSendSuccess()
        {
            if (controllerSendConfirmation != null)
            {
                controllerSendConfirmation(currentMessage); //controller uses meta data to store message
            }
        }

        //callback to be used to report errors
        public void setSendErrorCallBack(Action<TransmissionErrorCode> newObj)
        {
            controllerSendErrorReport = newObj;
        }
        //callback to be used to confirm message was sent succesfully
        public void setSendConfirmationCallback(Action<Message> newObj)
        {
            controllerSendConfirmation = newObj;
        }

        //takes int32 number, typecasts to int16, converts it to Byte[] with two elements and returns it
        private Byte[] lengthIntToBytes(int length)
        {
            short shortLength = (short) length; //typecast to short (anything greater than 65,536 will either throw exception or loose precision
            return BitConverter.GetBytes(shortLength);
        }
    }
}
