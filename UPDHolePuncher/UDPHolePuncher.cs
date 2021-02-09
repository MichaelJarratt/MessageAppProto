using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using PacketDotNet;
using PacketDotNet.Tcp;
using SharpPcap;
using System.Timers;
using Timer = System.Timers.Timer;

namespace MessageApp
{
    class UDPHolePuncher
    {
        private ushort localPort;
        private ushort destPort;
        private IPAddress destAddress;
        private IPAddress srcAddress = IPAddress.Parse("192.168.1.199"); //ip of NIC this machine is using to get to the gateway
        private PhysicalAddress destMAC = PhysicalAddress.Parse("78-65-59-BE-3D-B7"); //MAC of the network gateway
        private EthernetPacket ethernetPacket; //this is the packer containing the payload TCP packet that is sent to the gateway

        private ICaptureDevice device;

        Timer sendTimer;

        static void Main(string[] args)
        {
            UDPHolePuncher hp = new UDPHolePuncher(65432, "2.73.184.193");
            hp.startHolePunching(1000);
            Console.ReadLine();
        }

        //constructor takes port (local and target) and the address to send packets to
        public UDPHolePuncher(int port, string garbageAddress)
        {
            localPort = Convert.ToUInt16(port);
            destPort = Convert.ToUInt16(port);
            this.destAddress = IPAddress.Parse(garbageAddress); //takes string parameter and parses it into IP object
            init();
        }
        //constructor takes local and target port seperately, incase that's ever needed for whatever reason
        public UDPHolePuncher(int localPort, int destPort, string garbageAddress)
        {
            this.localPort = Convert.ToUInt16(localPort);
            this.destPort = Convert.ToUInt16(destPort);
            this.destAddress = IPAddress.Parse(garbageAddress); //takes string parameter and parses it into IP object
            init();
        }

        //tasks used by each constructor path
        private void init()
        {
            //not really sure what the capture device is, I think it refers to the NIC
            device = CaptureDeviceList.New()[3]; //better than .instance[3] apparently, according to author
            device.Open(); //opens device, whatever that means

            //TCP packet is the payload of the IP packet
            //TcpPacket tcpPacket = new TcpPacket(localPort, destPort); //creates empty packet and sets the source and destination ports <port>
            //tcpPacket.Flags = 0x02; //syn flag
            //tcpPacket.WindowSize = 64240;
            UdpPacket udpPacket = new UdpPacket(localPort, destPort); 
            


            //IP packet delivers TCP packet as its payload. IP packet is the payload of the Ethernet packet going to the gateway
            IPv4Packet ipPacket = new IPv4Packet(srcAddress, destAddress);
            ipPacket.TotalLength = 52;
            ipPacket.Id = 0xa144;
            ipPacket.FragmentFlags = 0x40;
            ipPacket.TimeToLive = 128;

            //Ethernet packet delivers the IP packet to the gateway
            PhysicalAddress srcMAC = getLocalMAC();
            //Console.WriteLine(srcMAC); //just check that it is getting the right one, otherwise you'll have to enter it manually
            ethernetPacket = new EthernetPacket(srcMAC, destMAC, EthernetType.None);

            //matryoshka doll the packets
            ethernetPacket.PayloadPacket = ipPacket;
            ipPacket.PayloadPacket = udpPacket;

            //this may need to be moved elsewhere incase time factors into the checksum
            udpPacket.Checksum = udpPacket.CalculateUdpChecksum();
            ipPacket.Checksum = ipPacket.CalculateIPChecksum();
            

            sendTimer = new Timer();
            sendTimer.Elapsed += new ElapsedEventHandler(sendPacket); //method to be done every interval
            sendTimer.Enabled = true; //does not start timer, it means the method will be done every interval
        }

        //sends pre-made packet via the captureDevice
        private void sendPacket(Object state, EventArgs e)
        {
            device.SendPacket(ethernetPacket);
        }

        //starts hole punching with default 1 second interval
        public void startHolePunching()
        {
            sendTimer.Interval = 1000;
            sendTimer.Start();
        }
        //starts hole punching with specified interval
        public void startHolePunching(double interval)
        {
            sendTimer.Interval = interval;
            sendTimer.Start();
        }
        //just pauses the hole punching, may be needed for something
        public void stopHolePunching()
        {
            sendTimer.Stop();
        }


        //gets the MAC address of the first found NIC, should be fine if you only have one, more complicated if not but we can worry about that later
        private PhysicalAddress getLocalMAC()
        {
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces(); //gets network interfaces

            foreach (NetworkInterface nic in interfaces) //iterate over them
            {
                if (nic.OperationalStatus == OperationalStatus.Up) //if it is in use
                {
                    return nic.GetPhysicalAddress(); //return first MAC address
                }
            }
            return null;
        }
    }
}
