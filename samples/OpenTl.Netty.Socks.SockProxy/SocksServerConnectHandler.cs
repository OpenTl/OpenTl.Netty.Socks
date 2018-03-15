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
using System.Threading.Tasks;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using OpenTl.Netty.Socks.Codecs.Socksx;
using OpenTl.Netty.Socks.Codecs.Socksx.v4;
using OpenTl.Netty.Socks.Codecs.Socksx.v5;

namespace OpenTl.DotNetty.Socksx.SockProxy
{
    public sealed class SocksServerConnectHandler : SimpleChannelInboundHandler<ISocksMessage>
    {
        private readonly Bootstrap b = new Bootstrap();

        public override bool IsSharable { get; } = true;

        protected override void ChannelRead0(IChannelHandlerContext ctx, ISocksMessage msg)
        {
            if (msg is ISocks4CommandRequest)
                HandleSocks4CommandRequest(ctx, msg);
            else if (msg is ISocks5CommandRequest)
                HandleSocks5CommandRequest(ctx, msg);
            else
                ctx.CloseAsync();
        }

        private async Task HandleSocks5CommandRequest(IChannelHandlerContext ctx, ISocksMessage msg)
        {
            var request = (ISocks5CommandRequest) msg;

            var promise = new TaskCompletionSource<IChannelHandlerContext>();

            var inboundChannel = ctx.Channel;
            b.Group(inboundChannel.EventLoop.Parent)
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.ConnectTimeout, TimeSpan.FromSeconds(10))
                .Option(ChannelOption.SoKeepalive, true)
                .Handler(new DirectClientHandler(promise));

            var outboundChannel = await b.ConnectAsync(request.DstAddr, request.DstPort);
            if (outboundChannel.Open)
            {
                // Connection established use handler provided results
            }
            else
            {
                // Close the connection if the connection attempt has failed.
                await ctx.Channel.WriteAndFlushAsync(
                    new DefaultSocks5CommandResponse(Socks5CommandStatus.Failure, request.DstAddrType));
                await ctx.CloseAsync();
            }

            if (outboundChannel.Active)
            {
                await ctx.Channel.WriteAndFlushAsync(new DefaultSocks5CommandResponse(
                    Socks5CommandStatus.Success,
                    request.DstAddrType,
                    request.DstAddr,
                    request.DstPort));

                ctx.Channel.Pipeline.Remove<SocksServerConnectHandler>();
                outboundChannel.Pipeline.AddLast(new RelayHandler(ctx.Channel));
                ctx.Channel.Pipeline.AddLast(new RelayHandler(outboundChannel));
            }
            else
            {
                await ctx.Channel.WriteAndFlushAsync(
                    new DefaultSocks5CommandResponse(Socks5CommandStatus.Failure, request.DstAddrType));
                await ctx.CloseAsync();
            }
        }


        private async Task HandleSocks4CommandRequest(IChannelHandlerContext ctx, ISocksMessage msg)
        {
            var request = (ISocks4CommandRequest) msg;

            var promise = new TaskCompletionSource<IChannelHandlerContext>();

            var inboundChannel = ctx.Channel;
            b.Group(inboundChannel.EventLoop.Parent)
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.ConnectTimeout, TimeSpan.FromSeconds(10))
                .Option(ChannelOption.SoKeepalive, true)
                .Handler(new DirectClientHandler(promise));

            var outboundChannel = await b.ConnectAsync(request.DstAddr, request.DstPort);
            if (outboundChannel.Open)
            {
                // Connection established use handler provided results
            }
            else
            {
                // Close the connection if the connection attempt has failed.
                await ctx.Channel.WriteAndFlushAsync(
                    new DefaultSocks4CommandResponse(Socks4CommandStatus.RejectedOrFailed));
                await ctx.CloseAsync();
            }

            if (outboundChannel.Active)
            {
                await ctx.Channel.WriteAndFlushAsync(new DefaultSocks4CommandResponse(Socks4CommandStatus.Success));

                ctx.Channel.Pipeline.Remove<SocksServerConnectHandler>();
                outboundChannel.Pipeline.AddLast(new RelayHandler(ctx.Channel));
                ctx.Channel.Pipeline.AddLast(new RelayHandler(outboundChannel));
            }
            else
            {
                await ctx.Channel.WriteAndFlushAsync(
                    new DefaultSocks4CommandResponse(Socks4CommandStatus.RejectedOrFailed));
                await ctx.CloseAsync();
            }
        }
    }
}