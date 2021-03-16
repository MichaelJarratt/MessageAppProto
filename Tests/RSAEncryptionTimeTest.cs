using MessageApp;
using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

namespace Tests
{
    //this class runs encryption and decryption functions at various message and key lengths and reports their times and standard deviations
    //doing it localy like this eliminates the uncontrollable variable of the network
    class RSAEncryptionTimeTest
    {
        private Stopwatch stopWatch = new Stopwatch();
        private RSACryptoServiceProvider localSet; //represents local key pair
        private RSACryptoServiceProvider remoteSet; //represents key pair held by another instance of the application
       
        public RSAEncryptionTimeTest()
        {
            test(2048, 10);
            test(2048, 100);
            test(2048, 245); //longest possible message with keyLength 2048 bits /8 = 256 bytes (characters) - 11 for headers = 245

            test(4096, 10);
            test(4096, 100);
            test(4096, 501); //longest possible message with keyLength 4096

            test(6144, 10);
            test(6144, 100);
            test(6144, 757); //lonest possible message with keyLength 6144
        }


        private void test(int keyLength, int messageLegth)
        {
            //setup
            string message = new string('a', messageLegth);
            localSet = new RSACryptoServiceProvider(keyLength);
            remoteSet = new RSACryptoServiceProvider(keyLength);
            int tests = 100; //how many tests to run
            TestResult[] testResults = new TestResult[tests];

            //first run done seporately because of set up time
            TestResult firstRun = process(message);
            //Console.WriteLine("First run:");
            //Console.WriteLine($"Total time: {firstRun.totalLength} ms");
            Console.WriteLine($"First Run - Total: {firstRun.totalTime} ms  Encryption: {firstRun.encryptionTime}ms  Decryption: {firstRun.decryptionTime}ms");
            Console.WriteLine($"Number of tests: {tests}");

            for (int i = 0; i < testResults.Length; i += 1)
            {
                testResults[i] = process(message);
            }
            displayResults(testResults, keyLength, messageLegth); //send results to be displayed
            Console.WriteLine();
        }

        //calculates and displayes the results in a readable format
        //code for standard deviations was taken from the top answer to this stack overflow post: https://stackoverflow.com/questions/5336457/how-to-calculate-a-standard-deviation-array
        private void displayResults(TestResult[] results, int keyLength, int messageLength)
        {
            //calculations//
            //total time
            int[] totalTimes = extractField(results, TestResult.Field.TotalTime); //gets array with total times of every result
            double totalTimeAVG = totalTimes.Average();
            double totalTimeSD = Math.Sqrt(totalTimes.Select(val => (val - totalTimeAVG) * (val - totalTimeAVG)).Sum()/totalTimes.Length);
            //encryptionTime
            int[] encryptionTimes = extractField(results, TestResult.Field.EncryptionTime); //gets array with encryption times of every result
            double encryptionTimeAVG = encryptionTimes.Average();
            double encryptionTimeSD = Math.Sqrt(encryptionTimes.Select(val => (val - encryptionTimeAVG) * (val - encryptionTimeAVG)).Sum() / encryptionTimes.Length);
            //decryptionTime
            int[] decryptionTimes = extractField(results, TestResult.Field.DecryptionTime); //gets array with decryption times of every result
            double decryptionTimeAVG = decryptionTimes.Average();
            double decryptionTimeSD = Math.Sqrt(decryptionTimes.Select(val => (val - decryptionTimeAVG) * (val - decryptionTimeAVG)).Sum() / decryptionTimes.Length);
            //round everything
            totalTimeAVG = Math.Round(totalTimeAVG, 2);
            totalTimeSD = Math.Round(totalTimeSD, 2);
            encryptionTimeAVG = Math.Round(encryptionTimeAVG, 2);
            encryptionTimeSD = Math.Round(encryptionTimeSD, 2);
            decryptionTimeAVG = Math.Round(decryptionTimeAVG, 2);
            decryptionTimeSD = Math.Round(decryptionTimeSD, 2);

            //display//
            Console.WriteLine($"//Results for: key length {keyLength} with message length {messageLength}//");
            //Console.WriteLine();
            Console.WriteLine($"Total Length of bytes: {results[0].totalLength}");
            Console.WriteLine($"Average Encryption and Decryption time: {totalTimeAVG}ms with {totalTimeSD}ms standard deviation");
            Console.WriteLine($"Average Encryption time: {encryptionTimeAVG}ms with {encryptionTimeSD}ms standard deviation");
            Console.WriteLine($"Average Decryption time: {decryptionTimeAVG}ms with {decryptionTimeSD}ms standard deviation");
        }

        //takes array of test result objects and returns array containing values from specified field
        private int[] extractField(TestResult[] results, TestResult.Field field)
        {
            int[] outArr = new int[results.Length];

            //extract total time
            if(field == TestResult.Field.TotalTime)
            {
                for (int i = 0; i < outArr.Length; i++)
                {
                    outArr[i] = results[i].totalTime;
                }
            }
            //extract encryption time
            if (field == TestResult.Field.EncryptionTime)
            {
                for (int i = 0; i < outArr.Length; i++)
                {
                    outArr[i] = results[i].encryptionTime;
                }
            }
            //extract decrytpion time
            if (field == TestResult.Field.DecryptionTime)
            {
                for (int i = 0; i < outArr.Length; i++)
                {
                    outArr[i] = results[i].decryptionTime;
                }
            }

            return outArr;
        }

        //does encryption and decryption and returns a filled testResult
        private TestResult process(String message)
        {
            TestResult testResult = new TestResult();

            //encryption//
            stopWatch.Start();
            CryptoUtility.setServiceProvider(localSet); //give local key set to utility
            Byte[] signatureBytes = CryptoUtility.signMessage(message); //creates signature for message with "local" key set
            CryptoUtility.setServiceProvider(remoteSet); //give remote key set to utility
            Byte[] messageBytes = Encoding.UTF8.GetBytes(CryptoUtility.encryptData(message, CryptoUtility.getPublicKey())); //encrypts with remote public key of "recipient"
            stopWatch.Stop();
            testResult.encryptionTime = (int)stopWatch.ElapsedMilliseconds;
            testResult.totalLength = signatureBytes.Length + messageBytes.Length;

            //decryption//
            //stopWatch.Restart();
            stopWatch.Reset();
            stopWatch.Start();
            //decrypts message with "recipients" remote private key. CryptoUtility already has the remote keyset as it's service provider
            String decMessage = CryptoUtility.decryptData(Encoding.UTF8.GetString(messageBytes), CryptoUtility.getPrivateKey());
            CryptoUtility.setServiceProvider(localSet); //give it the "senders" public key
            bool validSig = CryptoUtility.validateSignature(Encoding.UTF8.GetBytes(message), signatureBytes, CryptoUtility.getPublicKey());
            stopWatch.Stop();
            testResult.decryptionTime = (int)stopWatch.ElapsedMilliseconds;

            if(!validSig || 0 != String.Compare(decMessage,message)) //if signature failed to validate or the messages are not the same
            {
                throw new Exception("something ain't right");
            }

            testResult.totalTime = testResult.encryptionTime + testResult.decryptionTime; //adds up total time
            return testResult;
        }

        static void Main(string[] args)
        {
            new RSAEncryptionTimeTest();
        }



        class TestResult
        {
            public int totalLength; //length of signature + encrypted message
            public int encryptionTime;
            public int decryptionTime;
            public int totalTime;

            public enum Field
            {
                TotalLength,TotalTime,EncryptionTime,DecryptionTime
            }
        }
    }
}
