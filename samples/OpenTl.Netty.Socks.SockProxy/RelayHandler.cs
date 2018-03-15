using System;
using DotNetty.Buffers;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;

namespace OpenTl.DotNetty.Socksx.SockProxy
{
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

    public sealed class RelayHandler : ChannelHandlerAdapter
    {
        private readonly IChannel relayChannel;

        public RelayHandler(IChannel relayChannel)
        {
            this.relayChannel = relayChannel;
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            context.WriteAndFlushAsync(Unpooled.Empty);
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            if (relayChannel.Active)
                relayChannel.WriteAndFlushAsync(message);
            else
                ReferenceCountUtil.Release(message);
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            if (relayChannel.Active)
            {
                relayChannel.Flush();
                relayChannel.CloseAsync();
            }
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            context.CloseAsync();
        }
    }
}