using System;
using System.Collections.Generic;
using System.Text;

namespace MessageApp
{
    /// <summary>
    /// This exception is thrown by NetworkUtility. Use the errorCode field to identify the type of error, as well as
    /// the innerException.
    /// </summary>
    public class NetworkUtilityException: Exception
    {
        public TransmissionErrorCode errorCode;

        /// <summary>
        /// Instantiate NetworkUtilityException with an error code and inner exception
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="innerException"></param>
        public NetworkUtilityException(TransmissionErrorCode errorCode,Exception innerException)
        {
            typeof(Exception)
                .GetField("_innerException", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(this, innerException); //I've got no fucking clue, this was on stackoverflow. innerException cannot be written to
            //new Exception("Exception", innerException);
            this.errorCode = errorCode;
            //this.InnerException = innerException;
        }
        public NetworkUtilityException(TransmissionErrorCode errorCode)
        {
            this.errorCode = errorCode;
            //this.InnerException = innerException;
        }
        public NetworkUtilityException()
        {
        }


    }
}
