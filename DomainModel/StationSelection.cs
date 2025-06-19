using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
namespace DomainModel
{
    public class StationSelection
    {
        public int TSID { get; set; }
        public string Name { get; set; }
        public string IPAddress { get; set; }
        public string Port { get; set; }

        public int Priority { get; set; }
        
        public StationSelection(int TSIDVal, string NameVal, string IPVal, string PortVal, int PriorityVal)
        {
            TSID = TSIDVal;
            Name = NameVal;
            IPAddress = IPVal;
            Port = PortVal;
            Priority = PriorityVal;
        }
    }

    public class SelectedTStation
    {
        public int TSID { get; set; }
        public string Name { get; set; }
        public string IPAddress { get; set; }
        public string Port { get; set; }

        public int Priority { get; set; }

        public int Validity { get; set; }
        public SelectedTStation(int TSIDVal, string NameVal, string IPVal, string PortVal, int PriorityVal,int validbit)
        {
            TSID = TSIDVal;
            Name = NameVal;
            IPAddress = IPVal;
            Port = PortVal;
            Priority = PriorityVal;
            Validity = validbit;
        }
    }
    public class TelemetryStationUDP
    {
        public static int UDP1(string IPVal, string Port)
        {
            int validity = 0;
            UdpClient receivingUdpClient = new UdpClient(Convert.ToInt32(Port));
            //Creates an IPEndPoint to record the IP Address and port number of the sender.
            // The IPEndPoint will allow you to read datagrams sent from any source.
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Parse(IPVal), 0);
            try
            {
                // Blocks until a message returns on this socket from a remote host.
                Byte[] receiveBytes = receivingUdpClient.Receive(ref RemoteIpEndPoint);

                string returnData = Encoding.ASCII.GetString(receiveBytes);
                 string param=receiveBytes[5].ToString();
                validity = checkingvalidity(param);
                System.Diagnostics.Debug.WriteLine("This is the message you received " +
                                       returnData.ToString());
                System.Diagnostics.Debug.WriteLine("This message was sent from " +
                                            RemoteIpEndPoint.Address.ToString() +
                                            " on their port number " +
                                            RemoteIpEndPoint.Port.ToString());
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }

            return validity;

        }

        public static int checkingvalidity(string num)
        {
            int number = Convert.ToInt32(num);
            //Convert.ToInt32(num);
            int i = 1, j = 2;
            int temp1 = number & (1 << (i - 1));
            int temp2 = number & (1 << (j - 1));
            if ((temp1 > 0) && (temp2 > 0))
            {
                return 1;
            }
            else
            {
                return 0;
            }

        }
    }
   
}
