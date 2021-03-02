using System;
using System.Collections.Generic;
using System.Text;

namespace MessageApp
{
    enum TransmissionErrorCode
    {
        //ClientComp errors
        CliNoEndPointConnection,    //could not connect to target during sendMessage
        CliKeyExchangeFail,         //caught exception during either key exchange method
        CliConnectionList,          //no connection when transmitting bytes
        CliTransmissionError,       //unspecified exeption caught during transmission
        CliNoReceiveConfirmaton     //did not get confirmation of successful receive from server

        //ServerComp errors
    } 
}
