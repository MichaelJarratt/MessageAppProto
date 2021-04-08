using System;
using System.Collections.Generic;
using System.Text;

namespace Tests
{
    public class TestResult
    {
        public int totalLength; //length of signature + encrypted message
        public int encryptionTime;
        public int decryptionTime;
        public int totalTime;

        public enum Field
        {
            TotalLength, TotalTime, EncryptionTime, DecryptionTime
        }
    }
}
