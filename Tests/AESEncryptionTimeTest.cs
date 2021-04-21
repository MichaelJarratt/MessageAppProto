using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using MessageApp;

namespace Tests
{
    public class AESEncryptionTimeTest
    {
        private Stopwatch stopWatch = new Stopwatch();
        private byte[] AESKey;

        private ExcelWriter excelWriter; //class for exporting test data
        public AESEncryptionTimeTest()
        {
            excelWriter = new ExcelWriter("AESTest"); //create ExcelWriter for the RSA test results
            test(128, 10);  //128 bit key (16 bytes)
            test(128, 100);
            test(128, 501); //only testing up to 501 as that's the maximum allowed message length (because of chosen length of RSA key)

            test(192, 10);  //196 bit key
            test(192, 100);
            test(192, 501);

            test(256, 10);  //256 bit key
            test(256, 100);
            test(256, 501);

            excelWriter.saveAndRelease(); //save and close excel file
        }


        private void test(int keyLength, int messageLegth)
        {
            //setup
            string message = new string('a', messageLegth); //creates stirng of "a" of length <messageLength>
            int tests = 1000; //how many tests to run
            TestResult[] testResults = new TestResult[tests];

            CryptoUtility.AESkeyLength = keyLength; //set up CryptoUtility to use correct key length
            AESKey = CryptoUtility.GenerateAESRandomKey(); //gets random key for set of tests

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

            //write results to excel file (in memory)
            excelWriter.addRow(keyLength, messageLegth, testResults);
        }

        //calculates and displayes the results in a readable format
        //code for standard deviations was taken from the top answer to this stack overflow post: https://stackoverflow.com/questions/5336457/how-to-calculate-a-standard-deviation-array
        private void displayResults(TestResult[] results, int keyLength, int messageLength)
        {
            //calculations//
            //total time
            int[] totalTimes = extractField(results, TestResult.Field.TotalTime); //gets array with total times of every result
            double totalTimeAVG = totalTimes.Average();
            double totalTimeSD = Math.Sqrt(totalTimes.Select(val => (val - totalTimeAVG) * (val - totalTimeAVG)).Sum() / totalTimes.Length);
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
            if (field == TestResult.Field.TotalTime)
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
            //encryption
            stopWatch.Start();
            byte[] plainMessageBytes = Encoding.UTF8.GetBytes(message); //convert message to byte array
            Tuple<byte[], byte[]> res = CryptoUtility.AESEncrypt(plainMessageBytes, AESKey); //encrypts data
            //extract results
            byte[] encMessageBytes = res.Item1; //gets encrypted bytes from tuple
            byte[] IV = res.Item2; //gets initialisation vector from tuple
            stopWatch.Stop();
            testResult.encryptionTime = (int)stopWatch.ElapsedMilliseconds; //store time taken
            testResult.totalLength = encMessageBytes.Length; //store length of encrypted message


            //decryption//
            stopWatch.Reset();
            stopWatch.Start();
            //passes back encrypted message, key and initialisation vector to decrypt it
            string decryptedMessage = CryptoUtility.AESDecrypt(encMessageBytes, AESKey, IV);
            stopWatch.Stop();
            testResult.decryptionTime = (int)stopWatch.ElapsedMilliseconds;

            //check that nothing went wrong
            if (0 != String.Compare(decryptedMessage, message)) 
            {
                throw new Exception("something ain't right");
            }

            testResult.totalTime = testResult.encryptionTime + testResult.decryptionTime; //adds up total time
            return testResult;
        }

        static void Main(string[] args)
        {
            new AESEncryptionTimeTest();
        }



        //class TestResult
        //{
        //    public int totalLength; //length of signature + encrypted message
        //    public int encryptionTime;
        //    public int decryptionTime;
        //    public int totalTime;

        //    public enum Field
        //    {
        //        TotalLength, TotalTime, EncryptionTime, DecryptionTime
        //    }
        //}
    }
}
