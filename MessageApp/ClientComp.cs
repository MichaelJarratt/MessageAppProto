﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Threading.Tasks;

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

        public void sendMessage(string message) //still public so that console version does not break
        {
            //sendSocket = new Socket(IPAddress.Parse(targetIP).AddressFamily, SocketType.Stream, ProtocolType.Tcp); //create socket to handle sending
            //try
            //{
            //    NetworkUtility.connectAsync(sendSocket, targetIP, targetPort, finishConnect);
            //}
            //catch (NetworkUtilityException e)
            //{
            //    controllerSendErrorReport(e.errorCode);
            //}
        }

        /// <summary>
        /// This method should be called by the GUI version.
        /// Takes a message object as a parameter and stores reference so that it's sending can be confirmed,
        /// utilises NetworkUtility to exchange keys, then transmit the message.
        /// Errors are caught and reported to the controller,
        /// Successful sending of the message is reported to the controller by NetworkUtility directly.
        /// </summary>
        /// <param name="message">The message to send</param>
        public void sendMessage(Message message)
        {
            currentMessage = message; //stores reference so successful sending can be reported to the controller
            sendMessageAsync(message.message); //extracts message text and sends it
        }

        /// <summary>
        /// Takes a message as a parameter and utilises NetworkUtility to exchange keys, then asynchronously transmit the message.
        /// Errors are caught and reported to the controller,
        /// Successful sending of the message is reported to the controller by NetworkUtility directly.
        /// </summary>
        /// <param name="message">The message to send</param>
        private async void sendMessageAsync(string message)
        {
            sendSocket = new Socket(IPAddress.Parse(targetIP).AddressFamily, SocketType.Stream, ProtocolType.Tcp); //create socket to handle sending
            Task<Socket> connectTask = NetworkUtility.connectAsync(sendSocket, targetIP, targetPort);

            try
            {
                sendSocket = await connectTask;
                // send the client public RSA key to server as plaintext
                NetworkUtility.transmitSync(sendSocket, Encoding.UTF8.GetBytes(CryptoUtility.getPublicKey()));
                // receive the public RSA of the server as plaintext
                byte[] bytes = NetworkUtility.receiveSync(sendSocket);
                string serverPubKey = Encoding.UTF8.GetString(bytes);

                try
                {
                    //async version
                    NetworkUtility.RSATransmit2(sendSocket, Encoding.UTF8.GetBytes(message), serverPubKey, confirmSendSuccess);

                    //send it via NetworkUtility
                    //NetworkUtility.RSATransmit(sendSocket, Encoding.UTF8.GetBytes(message), serverPubKey, confirmSendSuccess);
                }
                catch (NetworkUtilityException e)
                {
                    controllerSendErrorReport(e.errorCode); //extract TransmissionErrorCode and send it to controller
                }
            }
            catch(NetworkUtilityException e) //failure to connect to endpoint
            {
                controllerSendErrorReport(e.errorCode);
            }
        }

        /// <summary>
        /// This method should only be externally called by the CLI version.
        /// Takes a message as a parameter and utilises NetworkUtility to exchange keys, then transmit the message.
        /// Errors are caught and reported to the controller,
        /// Successful sending of the message is reported to the controller by NetworkUtility directly.
        /// </summary>
        /// <param name="message">The message to send</param>
        public async void sendMessagesync(string message) //still public so that console version does not break
        {
            sendSocket = new Socket(IPAddress.Parse(targetIP).AddressFamily, SocketType.Stream, ProtocolType.Tcp); //create socket to handle sending
            if (!NetworkUtility.connectSync(sendSocket, targetIP, targetPort)) //try to connect, returns bool
            {
                controllerSendErrorReport(TransmissionErrorCode.CliNoEndPointConnection);
            }
            else //successfully binded to endpoint
            {
                // send the client public RSA key to server as plaintext
                NetworkUtility.transmitSync(sendSocket, Encoding.UTF8.GetBytes(CryptoUtility.getPublicKey()));
                // receive the public RSA of the server as plaintext
                byte[] bytes = NetworkUtility.receiveSync(sendSocket);
                string serverPubKey = Encoding.UTF8.GetString(bytes);

                try
                {
                    //async version
                    NetworkUtility.RSATransmit2(sendSocket, Encoding.UTF8.GetBytes(message), serverPubKey, confirmSendSuccess);

                    //send it via NetworkUtility
                    //NetworkUtility.RSATransmit(sendSocket, Encoding.UTF8.GetBytes(message), serverPubKey, confirmSendSuccess);
                }
                catch (NetworkUtilityException e)
                {
                    controllerSendErrorReport(e.errorCode); //extract TransmissionErrorCode and send it to controller
                }
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
    }
}
