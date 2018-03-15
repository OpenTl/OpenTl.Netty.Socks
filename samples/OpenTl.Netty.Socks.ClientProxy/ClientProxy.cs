using System;
using System.IO;
using System.Net;
using System.Reflection;
using DotNetty.Common.Internal.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using log4net;
using log4net.Config;
using OpenTl.Common.Testing.Logger;
using OpenTl.Netty.Socks.Handlers;

namespace OpenTl.DotNetty.Socks.ClientProxy
{
    public static class ClientProxy
    {
        private static readonly Bootstrap Bootstrap = new Bootstrap();

        private static readonly ILog Log = LogManager.GetLogger(typeof(ClientProxy));

        public static void Main(string[] args)
        {
            ConfigureLogger();

            Bootstrap
                .Group(new MultithreadEventLoopGroup())
                .Channel<TcpSocketChannel>()
                .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {
                    var pipeline = channel.Pipeline;
                    pipeline.AddFirst(new Socks4ProxyHandler(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1080)));
                }));

            Bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse("173.194.44.87"), 443)).Wait();

            Console.ReadKey();
        }


        private static void ConfigureLogger()
        {
            var repo = LogManager.GetRepository(typeof(Socks4ProxyHandler).GetTypeInfo().Assembly);
            XmlConfigurator.Configure(repo, new FileInfo("log4net.config"));

            InternalLoggerFactory.DefaultFactory.AddProvider(new Log4NetProvider(repo));

            Log.Info(
                $"\n\n#################################################  {DateTime.Now}  ################################################################################\n\n");
        }
    }
}