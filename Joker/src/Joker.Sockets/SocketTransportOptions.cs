using Joker.Connections;
using System.Buffers;
using System.Net;
using System.Net.Sockets;

namespace Joker.Sockets
{
    public class SocketTransportOptions
    {
        public int IOQueueCount { get; set; } = Math.Min(Environment.ProcessorCount, 16);

        public bool WaitForDataBeforeAllocatingBuffer { get; set; } = true;

        public bool NoDelay { get; set; } = true;

        public int Backlog { get; set; } = 512;

        public long? MaxReadBufferSize { get; set; } = 1024 * 1024;

        public long? MaxWriteBufferSize { get; set; } = 64 * 1024;

        public bool UnsafePreferInlineScheduling { get; set; }

        public Func<EndPoint, Socket> CreateBoundListenSocket { get; set; } = CreateDefaultBoundListenSocket;

        public Func<EndPoint, Socket> CreateBoundSocket { get; set; } = CreateDefaultBoundSocket;

        public static Socket CreateDefaultBoundListenSocket(EndPoint endpoint)
        {
            Socket listenSocket = CreateDefaultBoundSocket(endpoint);

            // we only call Bind on sockets that were _not_ created
            // using a file handle; the handle is already bound
            // to an underlying socket so doing it again causes the
            // underlying PAL call to throw
            if (endpoint is not FileHandleEndPoint)
            {
                listenSocket.Bind(endpoint);
            }

            return listenSocket;
        }

        public static Socket CreateDefaultBoundSocket(EndPoint endpoint)
        {
            Socket socket;
            switch (endpoint)
            {
                case FileHandleEndPoint fileHandle:
                    // We're passing "ownsHandle: false" to avoid side-effects on the
                    // handle when disposing the socket.
                    //
                    // When the non-owning SafeSocketHandle gets disposed (on .NET 7+),
                    // on-going async operations are aborted.
                    socket = new Socket(
                        new SafeSocketHandle((IntPtr)fileHandle.FileHandle, ownsHandle: false)
                    );
                    break;

                case UnixDomainSocketEndPoint unix:
                    socket = new Socket(unix.AddressFamily, SocketType.Stream, ProtocolType.Unspecified);
                    break;

                case UdpEndPoint udp:
                    socket = new Socket(udp.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                    break;

                case IPEndPoint ip:
                    socket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                    // Kestrel expects IPv6Any to bind to both IPv6 and IPv4
                    if (ip.Address.Equals(IPAddress.IPv6Any))
                    {
                        socket.DualMode = true;
                    }

                    break;

                default:
                    socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    break;
            } 

            return socket;
        }

        internal Func<MemoryPool<byte>> MemoryPoolFactory { get; set; } = Buffers.PinnedBlockMemoryPoolFactory.Create;
    }
}