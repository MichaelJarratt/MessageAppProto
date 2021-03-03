using MessageAppGUI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class ContactIPProtection
    {
        [TestMethod]
        public void protectThenUnprotect()
        {
            string ip = "125.23.53.124";
            Contact contact = new Contact(1,"Michael",ip);
            string returnedIp = contact.getIPString();
            
            Assert.AreEqual<string>(ip, returnedIp, "IP protected and unprotected succesfully");

            //bool pass = StringAssert.Equals(ip, returnedIp);

            //if(!pass)
            //{
            //    Assert.Fail("ip not protected and unprotected succesfully");
            //}
        }

        [TestMethod]
        public void protectThenUnprotectTwice()
        {
            string ip = "125.23.53.124";
            Contact contact = new Contact(1, "Michael", ip);
            string returnedIp = contact.getIPString(); //should unprotect, return, then reprotect
            returnedIp = contact.getIPString(); //should do the same as above

            bool pass = StringAssert.Equals(ip, returnedIp);

            if (!pass)
            {
                Assert.Fail("ip not protected and unprotected succesfully");
            }
        }
    }
}
