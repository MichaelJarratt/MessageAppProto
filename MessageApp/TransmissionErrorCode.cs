using System;
using System.Collections.Generic;
using System.Text;

namespace MessageApp
{
    public enum TransmissionErrorCode
    {
        //ClientComp errors
        CliNoEndPointConnection,    //could not connect to target during sendMessage
        CliKeyExchangeFail,         //caught exception during either key exchange method
        CliConnectionLost,          //no connection when transmitting bytes
        CliTransmissionError,       //unspecified exeption caught during transmission
        CliNoReceiveConfirmaton,    //did not get confirmation of successful receive from server
        CliUnspecifiedError,

        //ServerComp errors
        ServTotalLengthError,       //the total length attribute of the transmission was incorrect
        ServDecOrValError,          //exception when trying to decrypt message or validate signature
        ServValidationFail,         //signature failed to validate
        ServUnspecifiedError,
    }
}
