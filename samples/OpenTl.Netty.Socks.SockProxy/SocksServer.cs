/*
 * Copyright 2012 The Netty Project
 *
 * The Netty Project licenses this file to you under the Apache License,
 * version 2.0 (the "License"); you may not use this file except in compliance
 * with the License. You may obtain a copy of the License at:
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 */

using System;
using System.IO;
using System.Reflection;
using DotNetty.Common.Internal.Logging;
using DotNetty.Handlers.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using log4net;
using log4net.Config;
using OpenTl.Common.Testing.Logger;
using OpenTl.Netty.Socks.Codecs.Socksx;

namespace OpenTl.DotNetty.Socksx.SockProxy
{
    public sealed class SocksServer
    {
        private const int Port = 1080;
        private static readonly ILog Log = LogManager.GetLogger(typeof(SocksServer));

        public static void Main(string[] args)
        {
            ConfigureLogger();

            var bossGroup = new MultithreadEventLoopGroup(1);
            var workerGroup = new MultithreadEventLoopGroup();
            try
            {
                var b = new ServerBootstrap();
                b.Group(bossGroup, workerGroup)
                    .Channel<TcpServerSocketChannel>()
                    .ChildHandler(new ActionChannelInitializer<ISocketChannel>(channel =>
                    {
                        var pipeline = channel.Pipeline;

                        pipeline.AddLast(new LoggingHandler(LogLevel.DEBUG));
                        pipeline.AddLast(new SocksPortUnificationServerHandler());
                        pipeline.AddLast(SocksServerHandler.INSTANCE);
                    }));

                b.BindAsync(Port).Wait();

                Console.ReadKey();
            }
            catch (Exception e)
            {
            }
            finally
            {
                bossGroup.ShutdownGracefullyAsync();
                workerGroup.ShutdownGracefullyAsync();
            }
        }

        private static void ConfigureLogger()
        {
            var repo = LogManager.GetRepository(typeof(SocksPortUnificationServerHandler).GetTypeInfo().Assembly);
            XmlConfigurator.Configure(repo, new FileInfo("log4net.config"));

            InternalLoggerFactory.DefaultFactory.AddProvider(new Log4NetProvider(repo));

            Log.Info(
                $"\n\n#################################################  {DateTime.Now}  ################################################################################\n\n");
        }
    }
}