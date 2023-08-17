using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Telepathy.Tests
{
    public class SimpleUdpTests
    {
        const int port = 9587;
        const int MaxMessageSize = 16 * 1024;
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
        public async Task Udp_OverflowMessageThrows_And_SecondMessageGoingToServer()
        {
            var udpListener = new UdpClient(port);
            var udpClient = new UdpClient();
            
            var ipEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
            //udpClient.Connect(ipEndpoint);
            
            byte[] data = Encoding.UTF8.GetBytes("Hello World");

            var maxData = new byte[MaxMessageSize + 2];
            Assert.ThrowsAsync(typeof(SocketException), async () =>
            {
                await udpClient.SendAsync(maxData, maxData.Length, ipEndpoint);
            });
            Assert.DoesNotThrowAsync( async () =>
            {
                await udpClient.SendAsync(data, data.Length, ipEndpoint);
            });
            var result = await udpListener.ReceiveAsync();
            var message = Encoding.UTF8.GetString(result.Buffer);
            Console.WriteLine($"Listener Message from {result.RemoteEndPoint} -> {message}");
            Assert.AreEqual(data, result.Buffer);
            Assert.AreEqual(message, "Hello World");

            udpListener.SendAsync(data, data.Length, result.RemoteEndPoint);
            var result2 = await udpClient.ReceiveAsync();
            var message2 = Encoding.UTF8.GetString(result.Buffer);
            Console.WriteLine($"Client Message from {result.RemoteEndPoint} -> {message}");
            Assert.AreEqual(data, result.Buffer);
            Assert.AreEqual(message, "Hello World");
            
            udpClient.Close();
            udpListener.Close();
        }
        
        [Test]
        public async Task Udp_SequentialMessageTest()
        {
            var udpListener = new UdpClient(port);
            var udpClient1 = new UdpClient();
            
            var ipEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);

            var shortMessageData = new byte[6];
            var longMessageData = new byte[1024];

            var messageCount = 400;
            Assert.AreEqual(0, messageCount % 2);
            
            byte[][] datas = new byte[messageCount][];
            for (int i = 0; i < messageCount; i+=2)
            {
                Utils.IntToBytesBigEndianNonAlloc(i, shortMessageData, 0);
                Utils.IntToBytesBigEndianNonAlloc(i+1, longMessageData, 0);
                datas[i] = new byte[shortMessageData.Length];
                datas[i+1] = new byte[longMessageData.Length];
                Array.Copy(shortMessageData, datas[i], shortMessageData.Length);
                Array.Copy(longMessageData, datas[i+1], longMessageData.Length);
            }

            List<Task> tasks = new List<Task>();
            for (int i = 0; i < messageCount; i++)
            {
                tasks.Add(udpClient1.SendAsync(datas[i], datas[i].Length, ipEndpoint));
            }
            
            await Task.WhenAll(tasks);

            for (int i = 0; i < messageCount; i++)
            {
                var result = await udpListener.ReceiveAsync();
                ArraySegment<byte> arraySegment = new ArraySegment<byte>(result.Buffer, 0, 4);
                var messageOrder = Utils.BytesToIntBigEndian(arraySegment.Array);
                Console.WriteLine($"Listener Message from {result.RemoteEndPoint} -> Expected:{i}, Received:{messageOrder}");
                if (messageOrder != i)
                {
                    Console.WriteLine($" order from {result.RemoteEndPoint} -> Expected:{i}, Received:{messageOrder}");
                    udpClient1.Close();
                    udpListener.Close();
                    Assert.Fail($"Listener Message from {result.RemoteEndPoint} -> Expected:{i}, Received:{messageOrder}");
                }
            }
            udpClient1.Close();
            udpListener.Close();
        }
        
        [Test]
        public async Task Udp_IfFirstClientCloseSendMessageShouldThrow_SecondClientMessage_Should_Success()
        {
            var udpListener = new UdpClient(port);
            var udpClient1 = new UdpClient();
            var udpClient2 = new UdpClient();
            
            var ipEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
            //udpClient.Connect(ipEndpoint);
            
            byte[] data = Encoding.UTF8.GetBytes("Hello World");

            udpClient1.Close();
            Assert.ThrowsAsync(typeof(ObjectDisposedException), async () =>
            {
                await udpClient1.SendAsync(data, data.Length, ipEndpoint);
            });
            Assert.DoesNotThrowAsync( async () =>
            {
                await udpClient2.SendAsync(data, data.Length, ipEndpoint);
            });
            var result = await udpListener.ReceiveAsync();
            var message = Encoding.UTF8.GetString(result.Buffer);
            Console.WriteLine($"Listener Message from {result.RemoteEndPoint} -> {message}");
            Assert.AreEqual(data, result.Buffer);
            Assert.AreEqual(message, "Hello World");

            udpListener.SendAsync(data, data.Length, result.RemoteEndPoint);
            var result2 = await udpClient2.ReceiveAsync();
            var message2 = Encoding.UTF8.GetString(result.Buffer);
            Console.WriteLine($"Client Message from {result.RemoteEndPoint} -> {message}");
            Assert.AreEqual(data, result.Buffer);
            Assert.AreEqual(message, "Hello World");
            
            udpClient2.Close();
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