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
using DotNetty.Transport.Channels;

namespace OpenTl.DotNetty.Socksx.SockProxy
{
    public sealed class DirectClientHandler : ChannelHandlerAdapter
    {
        private readonly TaskCompletionSource<IChannelHandlerContext> _taskCompletionSource;

        public DirectClientHandler(TaskCompletionSource<IChannelHandlerContext> taskCompletionSource)
        {
            _taskCompletionSource = taskCompletionSource;
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            context.Channel.Pipeline.Remove(this);

            _taskCompletionSource.SetResult(context);
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            _taskCompletionSource.SetException(exception);

            base.ExceptionCaught(context, exception);
        }
    }
}