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
using DotNetty.Transport.Channels;
using OpenTl.Netty.Socks.Codecs.Socksx;
using OpenTl.Netty.Socks.Codecs.Socksx.v4;
using OpenTl.Netty.Socks.Codecs.Socksx.v5;

namespace OpenTl.DotNetty.Socksx.SockProxy
{
    public sealed class SocksServerHandler : SimpleChannelInboundHandler<ISocksMessage>
    {
        public static readonly SocksServerHandler INSTANCE = new SocksServerHandler();

        private SocksServerHandler()
        {
        }

        public override bool IsSharable { get; } = true;

        protected override void ChannelRead0(IChannelHandlerContext ctx, ISocksMessage msg)
        {
            switch (msg.Version())
            {
                case SocksVersion.Socks4A:
                    var socksV4CmdRequest = (ISocks4CommandRequest) msg;
                    if (Equals(socksV4CmdRequest.Type, Socks4CommandType.Connect))
                    {
                        ctx.Channel.Pipeline.AddLast(new SocksServerConnectHandler());
                        ctx.Channel.Pipeline.Remove(this);
                        ctx.FireChannelRead(msg);
                    }
                    else
                    {
                        ctx.CloseAsync();
                    }

                    break;
                case SocksVersion.Socks5:
                    if (msg is ISocks5InitialRequest)
                    {
                        // auth support example
                        //ctx.pipeline().addFirst(new Socks5PasswordAuthRequestDecoder());
                        //ctx.write(new DefaultSocks5AuthMethodResponse(Socks5AuthMethod.PASSWORD));
                        ctx.Channel.Pipeline.AddFirst(new Socks5CommandRequestDecoder());
                        ctx.WriteAsync(new DefaultSocks5InitialResponse(Socks5AuthMethod.NoAuth));
                    }
                    else if (msg is ISocks5PasswordAuthRequest)
                    {
                        ctx.Channel.Pipeline.AddFirst(new Socks5CommandRequestDecoder());
                        ctx.WriteAsync(new DefaultSocks5PasswordAuthResponse(Socks5PasswordAuthStatus.Success));
                    }
                    else if (msg is ISocks5CommandRequest)
                    {
                        var socks5CmdRequest = (ISocks5CommandRequest) msg;
                        if (Equals(socks5CmdRequest.Type, Socks5CommandType.Connect))
                        {
                            ctx.Channel.Pipeline.AddLast(new SocksServerConnectHandler());
                            ctx.Channel.Pipeline.Remove(this);
                            ctx.FireChannelRead(msg);
                        }
                        else
                        {
                            ctx.CloseAsync();
                        }
                    }
                    else
                    {
                        ctx.CloseAsync();
                    }

                    break;
                case SocksVersion.Unknown:
                    ctx.CloseAsync();
                    break;
            }
        }

        public override void ChannelReadComplete(IChannelHandlerContext context)
        {
            context.Flush();
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            Flush(context);
            CloseAsync(context);
        }
    }
}