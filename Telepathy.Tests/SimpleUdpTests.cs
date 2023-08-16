using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Telepathy.Tests
{
    public class SimpleUdpTests
    {
        [Test]
        public async Task Udp_IPv4_Only()
        {
            var udpListener = new UdpClient(9587);
            var udpClient = new UdpClient();
            
            var ipEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9587);
            //udpClient.Connect(ipEndpoint);
            
            byte[] data = Encoding.UTF8.GetBytes("Hello World");

            udpClient.SendAsync(data, data.Length, ipEndpoint);
            var result = await udpListener.ReceiveAsync();
            var message = Encoding.UTF8.GetString(result.Buffer);
            Console.WriteLine($"Listener Message from {result.RemoteEndPoint} -> {message}");

            udpListener.SendAsync(data, data.Length, result.RemoteEndPoint);
            var result2 = await udpClient.ReceiveAsync();
            var message2 = Encoding.UTF8.GetString(result.Buffer);
            Console.WriteLine($"Client Message from {result.RemoteEndPoint} -> {message}");
            
            udpClient.Close();
            udpListener.Close();
        }
        
        [Test]
        public async Task Udp_IpV6()
        {
            var udpListener = new UdpClient(9587, AddressFamily.InterNetworkV6);
            var udpClient = new UdpClient(1234, AddressFamily.InterNetworkV6);
            
            var ipEndpoint = new IPEndPoint(IPAddress.Parse("::ffff:127.0.0.1"), 9587);
            //udpClient.Connect(ipEndpoint);
            
            byte[] data = Encoding.UTF8.GetBytes("Hello World");

            udpClient.SendAsync(data, data.Length, ipEndpoint);
            var result = await udpListener.ReceiveAsync();
            var message = Encoding.UTF8.GetString(result.Buffer);
            Console.WriteLine($"Listener Message from {result.RemoteEndPoint} -> {message}");

            udpListener.SendAsync(data, data.Length, result.RemoteEndPoint);
            var result2 = await udpClient.ReceiveAsync();
            var message2 = Encoding.UTF8.GetString(result.Buffer);
            Console.WriteLine($"Client Message from {result.RemoteEndPoint} -> {message}");
            
            udpClient.Close();
            udpListener.Close();
        }
        
        [Test]
        public async Task Udp_IpV4_And_IpV6_DualMode()
        {
            var udpListener = new UdpClient(9587, AddressFamily.InterNetworkV6);
            var udpClient = new UdpClient(1234, AddressFamily.InterNetwork);
            
            var ipEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9587);
            //udpClient.Connect(ipEndpoint);
            
            byte[] data = Encoding.UTF8.GetBytes("Hello World");

            udpClient.SendAsync(data, data.Length, ipEndpoint);
            var result = await udpListener.ReceiveAsync();
            var message = Encoding.UTF8.GetString(result.Buffer);
            Console.WriteLine($"Listener Message from {result.RemoteEndPoint} -> {message}");

            udpListener.SendAsync(data, data.Length, result.RemoteEndPoint);
            var result2 = await udpClient.ReceiveAsync();
            var message2 = Encoding.UTF8.GetString(result.Buffer);
            Console.WriteLine($"Client Message from {result.RemoteEndPoint} -> {message}");
            
            udpClient.Close();
            udpListener.Close();
        }
    }
}